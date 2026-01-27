using PZRecorder.Core.Tables;
using System.Diagnostics.CodeAnalysis;

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

public class RecordsQuery : IEquatable<RecordsQuery>
{
    public int KindId { get; set; } = -1;
    public string SearchText { get; set; } = "";
    public int Year { get; set; } = -1;
    public int Month { get; set; } = -1;
    public RecordState? State { get; set; } = null;
    public int Rating { get; set; } = -1;

    public bool Equals([NotNullWhen(true)] RecordsQuery? other)
    {
        if (other == null) return false;

        return this.KindId == other.KindId
            && this.SearchText == other.SearchText
            && this.Year == other.Year
            && this.Month == other.Month
            && this.State == other.State
            && this.Rating == other.Rating;
    }
    public override bool Equals(object? obj)
    {
        return Equals(obj as RecordsQuery);
    }
    public void CopyValueTo(RecordsQuery other)
    {
        other.KindId = this.KindId;
        other.SearchText = this.SearchText;
        other.Year = this.Year;
        other.Month = this.Month;
        other.State = this.State;
        other.Rating = this.Rating;
    }
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
        if (query.Year >= 0) sql += $" AND publish_year = {query.Year}";
        if (query.Month >= 0) sql += $" AND publish_month = {query.Month}";
        if (query.Rating >= 0) sql += $" AND rating = {query.Rating}";
        if (!string.IsNullOrWhiteSpace(query.SearchText)) 
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
        record.ModifyDate = DateTime.Now;
        return DB.Conn.Update(record);
    }
    public int InsertRecord(Record record)
    {
        record.ModifyDate = DateTime.Now;
        return DB.Conn.Insert(record);
    }
    public int DeleteRecord(int id)
    {
        return DB.Conn.Delete<Record>(id);
    }
}
