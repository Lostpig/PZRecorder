using PZPKRecorder.Data;

namespace PZPKRecorder.Services;

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

    public static void AddTestRecords()
    {
        SqlLiteHandler.Instance.DB.RunInTransaction(() => {
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
}
