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
                ExceptionProxy.PublishException(ex);
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
            ExceptionProxy.PublishException(ex);
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
            ExceptionProxy.PublishException(ex);
            throw;
        }
    }
    private string BackupDB(string path) 
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
        DB.InsertAll(kinds);
        DB.InsertAll(records);

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
        DB.InsertAll(newDailies);
        DB.InsertAll(dailyweeks);

        // variant
        IList<VariantTable> vars = oldDB.Table<VariantTable>().Where(v => v.Key != dbVersionKey).ToList();
        DB.InsertAll(vars);
    }
}
