using PZRecorder.Core.Common;
using PZRecorder.Core.Tables;
using SQLite;

namespace PZRecorder.Core;

public sealed class SqlHandler : IDisposable
{
    public static SqlHandler Open(string dbPath)
    {
        var conn = new SQLiteConnection(dbPath);

        var vt = conn.Table<VariantTable>().Where(r => r.Key == Constant.DBVersionKey).FirstOrDefault();
        int version = vt != null ? int.Parse(vt.Value) : 0;

        if (version != Constant.DBVersion)
        {
            conn.Close();
            conn.Dispose();
            throw new Exception("Not supported db version:" + version);
        }

        return new SqlHandler(conn);
    }
    public static SqlHandler Create(string dbPath)
    {
        if (File.Exists(dbPath))
        {
            throw new Exception($"{dbPath} is already exists!");
        }

        var conn = new SQLiteConnection(dbPath);

        conn.CreateTable<VariantTable>();
        conn.CreateTable<Kind>();
        conn.CreateTable<Record>();
        conn.CreateTable<Daily>();
        conn.CreateTable<DailyWeek>();
        conn.CreateTable<ClockIn>();
        conn.CreateTable<ClockInRecord>();
        conn.CreateTable<ProcessWatch>();
        conn.CreateTable<ProcessRecord>();

        conn.Insert(new VariantTable() { Key = Constant.DBVersionKey, Value = Constant.DBVersion.ToString() });

        return new SqlHandler(conn);
    }

    public SQLiteConnection Conn { get; init; }
    private SqlHandler(SQLiteConnection conn)
    {
        Conn = conn;
    }

    public void BackupTo(string backupPath)
    {
        string backupDirPath = Path.GetDirectoryName(backupPath)!;

        if (!Directory.Exists(backupDirPath))
        {
            Directory.CreateDirectory(backupDirPath);
        }

        Conn.Backup(backupPath);
    }

    public void ResetDB()
    {
        Conn.DeleteAll<Kind>();
        Conn.DeleteAll<Record>();
        Conn.DeleteAll<Daily>();
        Conn.DeleteAll<DailyWeek>();
        Conn.DeleteAll<ClockIn>();
        Conn.DeleteAll<ClockInRecord>();
        Conn.DeleteAll<ProcessWatch>();
        Conn.DeleteAll<ProcessRecord>();

        Conn.Execute("UPDATE SQLITE_SEQUENCE SET SEQ=0 WHERE 1 = 1");
    }
    public void Dispose()
    {
        Conn.Close();
        Conn.Dispose();
    }
}
