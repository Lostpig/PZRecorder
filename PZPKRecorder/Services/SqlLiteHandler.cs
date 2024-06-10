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

    private readonly SQLiteConnection _db;
    public SQLiteConnection DB => _db;

    private SqlLiteHandler()
    {
        string rootPath = System.AppDomain.CurrentDomain.BaseDirectory;
        string filePath = Path.Join(rootPath, "records.db");

        _db = new SQLiteConnection(filePath);
    }
    public void Initialize()
    {
        _db.CreateTable<VariantTable>();
        _db.CreateTable<Kind>();
        _db.CreateTable<Record>();
        _db.CreateTable<Daily>();
        _db.CreateTable<DailyWeek>();
    }

    public void AddRecord(Record record)
    {
        _db.Insert(record);
    }

    public void Dispose()
    {
        _db.Close();
    }
}
