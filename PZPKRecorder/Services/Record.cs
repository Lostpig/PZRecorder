using PZPKRecorder.Components.Pages;
using PZPKRecorder.Data;
using System.Drawing.Printing;

namespace PZPKRecorder.Services;

class RecordCollection
{
    readonly Kind kind;
    readonly List<Record> records;
    private IOrderedEnumerable<Record> queriedRecords => records.Where(r =>
    {
        if (!string.IsNullOrWhiteSpace(searchText))
        {
            if (!(r.Name.Contains(searchText) || r.Alias.Contains(searchText))) return false;
        }
        if (state != null)
        {
            if (r.State != state) { return false; }
        }
        if (year > 0)
        {
            if (r.PublishYear != year) return false;
        }
        if (month > 0)
        {
            if (r.PublishMonth != month) return false;
        }

        return true;
    }).OrderByDescending(x => x.PublishYear * 100 + x.PublishMonth);

    #region Query Parameters
    string searchText = "";
    int year = 0;
    int month = 0;
    RecordState? state;

    public string SearchText
    {
        get => searchText;
        set
        {
            if (value == searchText) return;

            searchText = value;
        }
    }
    public int Year
    {
        get => year;
        set {
            if (value == year) return;

            year = value;
        }
    }
    public int Month
    {
        get => month;
        set
        {
            if (value == month) return;

            month = value;
        }
    }
    public RecordState? State
    {
        get => state;
        set
        {
            if (value == state) return;

            state = value;
        }
    }
    #endregion

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
    public int Total => queriedRecords.Count();

    public Kind Kind => kind;
    public IEnumerable<int> Years => records.Select(r => r.PublishYear).Distinct().Order();
    public IEnumerable<int> Months => records.Where(r => r.PublishYear == year).Select(r => r.PublishMonth).Distinct().Order();
    public IEnumerable<Record> Items => queriedRecords.Skip((page - 1) * pageSize).Take(pageSize);

    public int CurrentPageCount => Items.Count();

    public static RecordCollection GetCollection(Kind kind)
    {
        List<Record> records = RecordService.GetKindRecords(kind.Id);
        return new RecordCollection(kind, records);
    }
    private RecordCollection(Kind kind, List<Record> records)
    {
        this.kind = kind;
        this.records = records;
    }
    private int ComputePageCount()
    {
        int total = queriedRecords.Count();

        int n = total % pageSize;
        return (total - n) / pageSize + (n > 0 ? 1 : 0);
    }

    public void NewRecord(Record record)
    {
        records.Add(record);
    }
    public void UpdateRecord(Record record)
    {
        int index = records.FindIndex(r => r.Id == record.Id);
        if (index >= 0)
        {
            records[index] = record;
        }
    }
    public void DeleteRecord(int id)
    {
        int index = records.FindIndex(r => r.Id == id);
        if (index >= 0)
        {
            records.RemoveAt(index);
        }

        if (!Years.Contains(year))
        {
            year = 0;
            month = 0;
        }
        if (page > PageCount)
        {
            page = PageCount;
        }
    }
}

internal class RecordService
{
    public static IList<Record> GetRecords(int kindId, string searchText, int? year, int? month, RecordState? state, int limit, int offset)
    {
        string query = $"SELECT * FROM t_record WHERE kind = {kindId}"
            + $" {(state is not null ? "AND state = " + (int)state : "")}"
            + $" {(year is not null ? "AND publish_year = " + year : "")}"
            + $" {(month is not null ? "AND publish_month = " + month : "")}"
            + $" AND (name like '%{searchText}%' OR alias like '%{searchText}%')"
            + $" ORDER BY publish_year DESC, publish_month DESC LIMIT {limit} OFFSET {offset}";

        return SqlLiteHandler.Instance.DB.Query<Record>(query);
    }
    public static IList<Record> GetAllRecords()
    {
        return SqlLiteHandler.Instance.DB.Table<Record>().ToList();
    }
    public static List<Record> GetKindRecords(int kindId)
    {
        return SqlLiteHandler.Instance.DB.Table<Record>().Where(r => r.Kind == kindId).ToList();
    }

    public static Record GetRecord(int id)
    {
        return SqlLiteHandler.Instance.DB.Find<Record>(id);
    }

    public static int GetCount(int kindId, string searchText, int? year, int? month, RecordState? state)
    {
        string query = $"SELECT count(*) AS count FROM t_record WHERE kind = {kindId}"
            + $" {(state is not null ? "AND state = " + (int)state : "")}"
            + $" {(year is not null ? "AND publish_year = " + year : "")}"
            + $" {(month is not null ? "AND publish_month = " + month : "")}"
            + $" AND (name like '%{searchText}%' OR alias like '%{searchText}%')";

        var result = SqlLiteHandler.Instance.DB.Query<SQLCounter>(query);
        if (result != null && result.Count > 0)
        {
            return result[0].Count;
        }
        else
        {
            return 0;
        }
    }

    public static IList<int> GetYears(int kindId)
    {
        return SqlLiteHandler.Instance.DB.Table<Record>()
            .Where(r => r.Kind == kindId)
            .Select(r => r.PublishYear)
            .Distinct().OrderDescending().ToList();
    }

    public static int UpdateRecord(Record record)
    {
        return SqlLiteHandler.Instance.DB.Update(record);
    }
    public static int InsertRecord(Record record)
    {
        return SqlLiteHandler.Instance.DB.Insert(record);
    }
    public static int DeleteRecord(int id)
    {
        return SqlLiteHandler.Instance.DB.Delete<Record>(id);
    }

#if DEBUG
    public static void AddTestRecords()
    {
        SqlLiteHandler.Instance.DB.RunInTransaction(() =>
        {
            for (int i = 0; i < 500; i++)
            {
                var record = new Record()
                {
                    Name = $"AAAsNo{i}",
                    State = RecordState.Doing,
                    Kind = i % 3 + 1,
                    Episode = i % 12 + 1,
                    EpisodeCount = 13,
                    Remark = "Remark",
                    PublishYear = 2024,
                    PublishMonth = 7,
                    ModifyDate = DateTime.Now,
                };
                SqlLiteHandler.Instance.DB.Insert(record);
            }
        });
    }
#endif
}

