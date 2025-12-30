using PZRecorder.Core;
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

public class RecordCollection(Kind kind, List<Record> items)
{
    private IOrderedEnumerable<Record> QueriedRecords => Records
        .Where(r => {
            if (State != null)
            {
                if (r.State != State) { return false; }
            }
            if (Year > 0)
            {
                if (r.PublishYear != Year) return false;
            }
            if (Month > 0)
            {
                if (r.PublishMonth != Month) return false;
            }
            if (Rating >= 0)
            {
                if (r.Rating != Rating) return false;
            }

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                if (!(r.Name.Contains(SearchText) || r.Alias.Contains(SearchText))) return false;
            }

            return true;
        })
        .OrderBy(x => {
            return Sort switch
            {
                RecordSort.PublishTimeAsc => x.PublishYear * 100 + x.PublishMonth,
                RecordSort.PublishTimeDesc => -(x.PublishYear * 100 + x.PublishMonth),
                RecordSort.ModifyTimeAsc => x.ModifyDate.Ticks,
                RecordSort.ModifyTimeDesc => -x.ModifyDate.Ticks,
                RecordSort.RatingAsc => x.Rating,
                RecordSort.RatingDesc => -x.Rating,
                _ => -(x.PublishYear * 100 + x.PublishMonth),
            };
        });

    #region Query Parameters
    public string SearchText { get; set; } = "";
    public int Year { get; set; } = 0;
    public int Month { get; set; } = 0;
    public int Rating { get; set; } = -1;
    public RecordState? State { get; set; } = null;
    public RecordSort Sort { get; set; } = RecordSort.PublishTimeAsc;
    #endregion

    #region Page
    int page = 1;
    int pageSize = 10;
    public int Page
    {
        get => page;
        set
        {
            if (value > PageCount) page = PageCount;
            else if (value < 1) page = 1;
            else page = value;
        }
    }
    public int PageSize
    {
        get => pageSize;
        private set
        {
            int oldPage = Page;
            pageSize = value;
            Page = oldPage;
        }
    }
    public int PageCount => ComputePageCount();
    public int From => (page - 1) * pageSize + 1;
    public int To => From + CurrentPageCount - 1;
    public int Total => QueriedRecords.Count();
    #endregion

    public Kind Kind { get; init; } = kind;
    public List<Record> Records { get; init; } = items;

    public IEnumerable<int> Years => Records.Select(r => r.PublishYear).Distinct().Order();
    public IEnumerable<int> Months => Records.Where(r => r.PublishYear == Year).Select(r => r.PublishMonth).Distinct().Order();
    public IEnumerable<Record> Items => QueriedRecords.Skip((page - 1) * pageSize).Take(pageSize);

    public int CurrentPageCount => Items.Count();
    private int ComputePageCount()
    {
        int total = QueriedRecords.Count();

        int n = total % pageSize;
        return (total - n) / pageSize + (n > 0 ? 1 : 0);
    }

    public void NewRecord(Record record)
    {
        Records.Add(record);
    }
    public void UpdateRecord(Record record)
    {
        int index = Records.FindIndex(r => r.Id == record.Id);
        if (index >= 0)
        {
            Records[index] = record;
        }
    }
    public void DeleteRecord(int id)
    {
        int index = Records.FindIndex(r => r.Id == id);
        if (index >= 0)
        {
            Records.RemoveAt(index);
        }

        if (!Years.Contains(Year))
        {
            Year = 0;
            Month = 0;
        }
        if (page > PageCount)
        {
            page = PageCount;
        }
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

    public List<Record> GetRecords(int kindId, string searchText, int? year, int? month, RecordState? state, int limit, int offset)
    {
        string query = $"SELECT * FROM t_record WHERE kind = {kindId}"
            + $" {(state is not null ? "AND state = " + (int)state : "")}"
            + $" {(year is not null ? "AND publish_year = " + year : "")}"
            + $" {(month is not null ? "AND publish_month = " + month : "")}"
            + $" AND (name like '%{searchText}%' OR alias like '%{searchText}%')"
            + $" ORDER BY publish_year DESC, publish_month DESC LIMIT {limit} OFFSET {offset}";

        return DB.Conn.Query<Record>(query);
    }
    public List<Record> GetAllRecords()
    {
        return DB.Conn.Table<Record>().ToList();
    }
    public List<Record> GetKindRecords(int kindId)
    {
        return DB.Conn.Table<Record>().Where(r => r.Kind == kindId).ToList();
    }

    public Record GetRecord(int id)
    {
        return DB.Conn.Find<Record>(id);
    }

    public int GetCount(int kindId, string searchText, int? year, int? month, RecordState? state)
    {
        string query = $"SELECT count(*) AS count FROM t_record WHERE kind = {kindId}"
            + $" {(state is not null ? "AND state = " + (int)state : "")}"
            + $" {(year is not null ? "AND publish_year = " + year : "")}"
            + $" {(month is not null ? "AND publish_month = " + month : "")}"
            + $" AND (name like '%{searchText}%' OR alias like '%{searchText}%')";

        var result = DB.Conn.Query<ViewCounter>(query);
        if (result != null && result.Count > 0)
        {
            return result[0].Count;
        }
        else
        {
            return 0;
        }
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

    public RecordCollection GetCollection(Kind kind)
    {
        List<Record> records = GetKindRecords(kind.Id);
        return new RecordCollection(kind, records);
    }
}
