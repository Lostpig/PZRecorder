using PZRecorder.Core.Tables;

namespace PZRecorder.Core.Managers;

public enum RecordSort
{
    PublishTimeAsc,
    PublishTimeDesc,
    ModifyTimeAsc,
    ModifyTimeDesc,
    RatingAsc, 
    RatingDesc,
}

public record RecordsQuery
{
    public int KindId { get; init; }
    public string SearchText { get; init; } = "";
    public int? Year { get; init; } = null;
    public int? Month { get; init; } = null;
    public RecordState? State { get; init; } = null;
    public int? Rating { get; init; } = null;
}

public class RecordManager(SqlHandler db)
{
    private SqlHandler DB { get; init;  } = db;

    public List<Kind> GetKinds()
    {
        return DB.Conn.Table<Kind>().OrderBy(k => k.OrderNo).ToList();
    }
    public Kind GetKind(int id)
    {
        return DB.Conn.Get<Kind>(id);
    }
    public int InsertKind(Kind kind)
    {
        return DB.Conn.Insert(kind);
    }
    public int DeleteKind(int id)
    {
        int usingCount = DB.Conn.Table<Record>().Where(t => t.Kind == id).Count();
        if (usingCount > 0)
        {
            throw new Exception("Kind is usage, cannot delete!");
        }

        return DB.Conn.Delete<Kind>(id);
    }
    public int UpdateKind(Kind kind)
    {
        return DB.Conn.Update(kind);
    }

    public List<Record> GetAllRecords()
    {
        return DB.Conn.Table<Record>().ToList();
    }
    public List<Record> GetKindRecords(int kindId)
    {
        return DB.Conn.Table<Record>().Where(r => r.Kind == kindId).ToList();
    }
    public List<Record> QueryRecords(RecordsQuery query)
    {
        string sql = $"SELECT * FROM t_record WHERE kind = {query.KindId}";

        if (query.State is not null) sql += $" AND state = {(int)query.State}";
        if (query.Year is not null) sql += $" AND publish_year = {query.Year}";
        if (query.Month is not null) sql += $" AND publish_month = {query.Month}";
        if (query.Rating is not null) sql += $" AND rating = {query.Rating}";
        if (string.IsNullOrWhiteSpace(query.SearchText)) 
            sql += $" AND (name like '%{query.SearchText}%' OR alias like '%{query.SearchText}%')";

        sql += $" ORDER BY publish_year DESC, publish_month DESC";

        return DB.Conn.Query<Record>(sql);
    }
    public Record GetRecord(int id)
    {
        return DB.Conn.Find<Record>(id);
    }

    public List<int> GetYears(int kindId)
    {
        return DB.Conn.Table<Record>()
            .Where(r => r.Kind == kindId)
            .Select(r => r.PublishYear)
            .Distinct().OrderDescending().ToList();
    }

    public int UpdateRecord(Record record)
    {
        return DB.Conn.Update(record);
    }
    public int InsertRecord(Record record)
    {
        return DB.Conn.Insert(record);
    }
    public int DeleteRecord(int id)
    {
        return DB.Conn.Delete<Record>(id);
    }

}
