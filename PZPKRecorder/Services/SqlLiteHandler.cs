using SQLite;
using PZPKRecorder.Data;

namespace PZPKRecorder.Services;

internal class SqlLiteHandler
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
    public SQLiteConnection DB {
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

    public static string dbPath
    {
        get
        {
            string rootPath = System.AppDomain.CurrentDomain.BaseDirectory;
            string filePath = Path.Join(rootPath, "records.db");
            return filePath;
        }
    }
    public const string dbVersionKey = "dbversion";

    public void Initialize()
    {
        if (File.Exists(dbPath))
        {
            OpenAvailableDB(dbPath);
        }
        else
        {
            CreateNewDB(dbPath);
        }
    }
    private void OpenAvailableDB(string path)
    {
        try
        {
            _db = new SQLiteConnection(path);

            var vt = _db.Table<VariantTable>().Where(r => r.Key == dbVersionKey).FirstOrDefault();
            int version = vt != null ? int.Parse(vt.Value) : 0;

            if (version != Helper.DataVersion)
            {
                // DO compatibles process
                _db.Close();
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

            _db.InsertOrReplace(new VariantTable() { Key = dbVersionKey, Value = Helper.DataVersion.ToString() });
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

        DB.Execute("UPDATE SQLITE_SEQUENCE SET SEQ=0 WHERE NAME IN ('t_kind', 't_record', 't_daily')");
    }
    public void Dispose()
    {
        _db.Close();
    }

    private void UpdateDB(int version, string oldDbPath)
    {
        switch (version)
        {
            case 0:
                UpdateFromVersion0(oldDbPath); break;
            case 10001:
                UpdateFromVersion10001(oldDbPath); break;
            default:
                throw new NotSupportedException($"Not supported DB version: {version}");
        }
    }

    private void UpdateFromVersion0(string oldDbPath)
    {
        SQLiteConnection oldDB = new SQLiteConnection(oldDbPath, SQLiteOpenFlags.ReadOnly);

        // kind and record
        IList<KindVersion0> kinds = oldDB.Table<KindVersion0>().ToList();
        IList<Record> records = oldDB.Table<Record>().ToList();
        List<Kind> newKinds = kinds.Select(d => new Kind()
        {
            Id = d.Id,
            Name = d.Name,
            OrderNo = 0
        }).ToList();

        // daily and dailyweek
        IList<DailyVersion0> dailies = oldDB.Table<DailyVersion0>().ToList();
        IList<DailyWeek> dailyweeks = oldDB.Table<DailyWeek>().ToList();
        List<Daily> newDailies = dailies.Select(d => new Daily()
        {
            Id = d.Id,
            Name = d.Name,
            State = d.State,
            Remark = d.Remark,
            ModifyDate = d.ModifyDate,
            Alias = d.Alias,
            OrderNo = 0
        }).ToList();

        // variant
        IList<VariantTable> vars = oldDB.Table<VariantTable>().Where(v => v.Key != dbVersionKey).ToList();

        ExcuteBatchInsert(newKinds, records, newDailies, dailyweeks, vars);
    }

    private void UpdateFromVersion10001(string oldDbPath)
    {
        SQLiteConnection oldDB = new SQLiteConnection(oldDbPath, SQLiteOpenFlags.ReadOnly);

        IList<Kind> kinds = oldDB.Table<Kind>().ToList();
        IList<Record> records = oldDB.Table<Record>().ToList();
        IList<Daily> dailies = oldDB.Table<Daily>().ToList();
        IList<DailyWeek> dailyweeks = oldDB.Table<DailyWeek>().ToList();
        IList<VariantTable> vars = oldDB.Table<VariantTable>().Where(v => v.Key != dbVersionKey).ToList();

        ExcuteBatchInsert(kinds, records, dailies, dailyweeks, vars);
    }

    public void ExcuteBatchInsert(IList<Kind> kinds, IList<Record> records, IList<Daily> dailies, IList<DailyWeek> dailyweeks, IList<VariantTable> vars)
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
                    var ndw = new DailyWeek()
                    {
                        DailyId = newId,
                        MondayDay = dw.MondayDay,
                        Day1 = dw.Day1,
                        Day2 = dw.Day2,
                        Day3 = dw.Day3,
                        Day4 = dw.Day4,
                        Day5 = dw.Day5,
                        Day6 = dw.Day6,
                        Day7 = dw.Day7,
                    };
                    ndw.Id = DailyWeek.GenerateId(newId, dw.MondayDate);

                    DB.Insert(ndw);
                }
            }

            DB.InsertAll(vars);
        });
    }
}
