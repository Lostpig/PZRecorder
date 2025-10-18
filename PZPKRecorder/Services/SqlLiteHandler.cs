using PZPKRecorder.Data;
using SQLite;
using System.Reflection;

namespace PZPKRecorder.Services;

internal class SqlLiteHandler : IDisposable
{
    public const int DBVersion = 10010;
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

    public bool Initialize()
    {
        if (File.Exists(DBPath))
        {
            return OpenAvailableDB(DBPath);
        }
        else
        {
            CreateNewDB(DBPath);
            return true;
        }
    }
    private bool OpenAvailableDB(string path)
    {
        try
        {
            _db = new SQLiteConnection(path);

            var vt = _db.Table<VariantTable>().Where(r => r.Key == DBVersionKey).FirstOrDefault();
            int version = vt != null ? int.Parse(vt.Value) : 0;

            if (version != DBVersion)
            {
                _db.Close();
                _db.Dispose();
                return false;
            }

            return true;
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

            _db.InsertOrReplace(new VariantTable() { Key = DBVersionKey, Value = DBVersion.ToString() });
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
}
