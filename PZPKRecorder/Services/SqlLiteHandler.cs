﻿using PZPKRecorder.Data;
using SQLite;
using System.Reflection;

namespace PZPKRecorder.Services;

internal class SqlLiteHandler : IDisposable
{
    static private SqlLiteHandler? _instance;
    static public SqlLiteHandler Instance
    {
        get
        {
            _instance ??= new SqlLiteHandler();
            return _instance;
        }
    }

    private SQLiteConnection? _db;
    public SQLiteConnection DB
    {
        get
        {
            if (_db == null)
            {
                var ex = new Exception("Sqlite not connected!");
                ExceptionProxy.CatchException(ex);
                throw ex;
            }
            return _db;
        }
    }

    public static string DBPath
    {
        get
        {
            string rootPath = System.AppDomain.CurrentDomain.BaseDirectory;
            string filePath = Path.Join(rootPath, "records.db");
            return filePath;
        }
    }
    public const string DBVersionKey = "dbversion";

    public void Initialize()
    {
        if (File.Exists(DBPath))
        {
            OpenAvailableDB(DBPath);
        }
        else
        {
            CreateNewDB(DBPath);
        }
    }
    private void OpenAvailableDB(string path)
    {
        try
        {
            _db = new SQLiteConnection(path);

            var vt = _db.Table<VariantTable>().Where(r => r.Key == DBVersionKey).FirstOrDefault();
            int version = vt != null ? int.Parse(vt.Value) : 0;

            if (version != Helper.DataVersion)
            {
                // DO compatibles process
                _db.Close();
                _db.Dispose();

                string backupDBPath = BackupDB(path);
                File.Delete(path);

                CreateNewDB(path);
                UpdateDB(version, backupDBPath);
            }
        }
        catch (Exception ex)
        {
            ExceptionProxy.CatchException(ex);
            throw;
        }
    }

    private void CreateNewDB(string path)
    {
        try
        {
            _db = new SQLiteConnection(path);

            _db.CreateTable<VariantTable>();
            _db.CreateTable<Kind>();
            _db.CreateTable<Record>();
            _db.CreateTable<Daily>();
            _db.CreateTable<DailyWeek>();
            _db.CreateTable<ClockIn>();
            _db.CreateTable<ClockInRecord>();
            _db.CreateTable<ProcessWatch>();
            _db.CreateTable<ProcessRecord>();

            _db.InsertOrReplace(new VariantTable() { Key = DBVersionKey, Value = Helper.DataVersion.ToString() });
        }
        catch (Exception ex)
        {
            ExceptionProxy.CatchException(ex);
            throw;
        }
    }
    public string BackupDB(string path)
    {
        string rootPath = System.AppDomain.CurrentDomain.BaseDirectory;
        string backupPath = Path.Join(rootPath, "backup", $"records.{DateTime.Now.ToString("yyyy-MM-dd-HHmmss")}.db");
        string backupDirPath = Path.GetDirectoryName(backupPath)!;

        if (!Directory.Exists(backupDirPath))
        {
            Directory.CreateDirectory(backupDirPath);
        }

        File.Copy(path, backupPath);
        return backupPath;
    }

    /** 
     * Delete all data
     */
    public void ResetDB()
    {
        DB.DeleteAll<Kind>();
        DB.DeleteAll<Record>();
        DB.DeleteAll<Daily>();
        DB.DeleteAll<DailyWeek>();
        DB.DeleteAll<ClockIn>();
        DB.DeleteAll<ClockInRecord>();
        DB.DeleteAll<ProcessWatch>();
        DB.DeleteAll<ProcessRecord>();

        DB.Execute("UPDATE SQLITE_SEQUENCE SET SEQ=0 WHERE 1 = 1");
    }
    public void Dispose()
    {
        _db?.Close();
        _db?.Dispose();
    }

    private void UpdateDB(int version, string oldDbPath)
    {
        if (Helper.IsCompatibleVersion(version))
        {
            UpdateFromOldVersion(oldDbPath, version);
        }
        else
        {
            throw new NotSupportedException($"Not supported DB version: {version}");
        }
    }

    private void UpdateFromOldVersion(string oldDbPath, int oldDbVersion)
    {
        SQLiteConnection oldDB = new SQLiteConnection(oldDbPath, SQLiteOpenFlags.ReadOnly);

        // datas
        var kinds = UpdateCollection<Kind>(oldDB, oldDbVersion);
        var records = UpdateCollection<Record>(oldDB, oldDbVersion);
        var dailies = UpdateCollection<Daily>(oldDB, oldDbVersion);
        var dailyweeks = UpdateCollection<DailyWeek>(oldDB, oldDbVersion);
        var clockIns = UpdateCollection<ClockIn>(oldDB, oldDbVersion);
        var clockInRecords = UpdateCollection<ClockInRecord>(oldDB, oldDbVersion);
        var processWatches = UpdateCollection<ProcessWatch>(oldDB, oldDbVersion);
        var processRecords = UpdateCollection<ProcessRecord>(oldDB, oldDbVersion);

        // variant
        IList<VariantTable> vars = oldDB.Table<VariantTable>().Where(v => v.Key != DBVersionKey).ToList();

        ExcuteBatchInsert(kinds, records, dailies, dailyweeks, clockIns, clockInRecords, processWatches, processRecords, vars);
    }
    private List<T> UpdateCollection<T>(SQLiteConnection oldDB, int oldDbVersion) where T : new()
    {
        List<T> list = new();
        Type t = typeof(T);

        TableVersionAttribute? tverAttr = t.GetCustomAttribute<TableVersionAttribute>();
        if (tverAttr is not null)
        {
            if (tverAttr.MaxVersion < oldDbVersion || tverAttr.MinVersion > oldDbVersion)
            {
                return list;
            }
        }

        IList<T> oldList = oldDB.Table<T>().ToList();
        foreach (var item in oldList)
        {
            T instance = new T();
            foreach (PropertyInfo pi in t.GetProperties())
            {
                ColumnAttribute? colAttr = pi.GetCustomAttribute<ColumnAttribute>();
                if (colAttr is null) continue;

                FieldVersionAttribute? versionAttr = pi.GetCustomAttribute<FieldVersionAttribute>();
                if (versionAttr is not null)
                {
                    if (versionAttr.MaxVersion < oldDbVersion || versionAttr.MinVersion > oldDbVersion)
                    {
                        pi.SetValue(instance, versionAttr.DefaultValue);
                        continue;
                    }
                }

                pi.SetValue(instance, pi.GetValue(item));
            }

            list.Add(instance);
        }

        return list;
    }

    /*
        批量插入时,AutoIncrement会覆盖原有ID
        所以关联的表需要在之后插入,并基于原ID关联上新ID
     */
    public void ExcuteBatchInsert(
        IList<Kind> kinds, IList<Record> records,
        IList<Daily> dailies, IList<DailyWeek> dailyweeks,
        IList<ClockIn> clockIns, IList<ClockInRecord> clockInRecords,
        IList<ProcessWatch> processWatches, IList<ProcessRecord> processRecords,
        IList<VariantTable> vars)
    {
        DB.RunInTransaction(() =>
        {
            foreach (var kind in kinds)
            {
                var recordsInKind = records.Where(r => r.Kind == kind.Id).ToList();

                kind.Id = 0;
                DB.Insert(kind);
                int newId = kind.Id;

                foreach (var record in recordsInKind)
                {
                    DB.Insert(new Record()
                    {
                        Kind = newId,
                        Name = record.Name,
                        Alias = record.Alias,
                        Remark = record.Remark,
                        PublishMonth = record.PublishMonth,
                        PublishYear = record.PublishYear,
                        State = record.State,
                        Episode = record.Episode,
                        EpisodeCount = record.EpisodeCount,
                        ModifyDate = record.ModifyDate,
                    });
                }
            }

            foreach (var daily in dailies)
            {
                var weeksForDaily = dailyweeks.Where(dw => dw.DailyId == daily.Id).ToList(); ;

                daily.Id = 0;
                DB.Insert(daily);
                int newId = daily.Id;

                foreach (var dw in weeksForDaily)
                {
                    var ndw = new DailyWeek();
                    ndw.Init(newId, dw);

                    DB.Insert(ndw);
                }
            }

            foreach (var clockin in clockIns)
            {
                var currentItems = clockInRecords.Where(r => r.Pid == clockin.Id).ToList();

                clockin.Id = 0;
                DB.Insert(clockin);
                int newId = clockin.Id;

                foreach (var item in currentItems)
                {
                    DB.Insert(new ClockInRecord()
                    {
                        Pid = newId,
                        Time = item.Time
                    });
                }
            }

            foreach (var watch in processWatches)
            {
                var currentItems = processRecords.Where(r => r.Pid == watch.Id).ToList();

                watch.Id = 0;
                DB.Insert(watch);
                int newId = watch.Id;

                foreach (var item in currentItems)
                {
                    DB.Insert(new ProcessRecord()
                    {
                        Pid = newId,
                        Date = item.Date,
                        StartTime = item.StartTime,
                        EndTime = item.EndTime,
                    });
                }
            }

            DB.InsertAll(vars);
        });
    }
}
