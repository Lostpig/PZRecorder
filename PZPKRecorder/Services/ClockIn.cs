using PZPKRecorder.Data;

namespace PZPKRecorder.Services;

internal class ClockInModel(ClockIn clockIn, IList<ClockInRecord> records)
{
    public ClockIn ClockIn { get; private set; } = clockIn;
    public IList<ClockInRecord> Records { get; private set; } = records;

    public ClockInRecord? LastRecord => Records.LastOrDefault();
}

internal class ClockInService
{

    public static ClockIn? GetClockIn(int id)
    {
        return SqlLiteHandler.Instance.DB.Find<ClockIn>(id);
    }
    public static IList<ClockIn> GetClockIns()
    {
        return SqlLiteHandler.Instance.DB.Table<ClockIn>().ToList();
    }
    public static int AddClockIn(ClockIn clockIn)
    {
        return SqlLiteHandler.Instance.DB.Insert(clockIn);
    }
    public static void UpdateClockIn(ClockIn clockIn)
    {
        SqlLiteHandler.Instance.DB.Update(clockIn);
    }
    public static void DeleteClockIn(int id)
    {
        SqlLiteHandler.Instance.DB.RunInTransaction(() =>
        {
            SqlLiteHandler.Instance.DB.Delete<ClockIn>(id);
            SqlLiteHandler.Instance.DB.Table<ClockInRecord>().Delete(r => r.Pid == id);
        });
    }


    public static IList<ClockInRecord> GetRecords(int? pid)
    {
        var q = SqlLiteHandler.Instance.DB.Table<ClockInRecord>();

        if (pid is not null)
        {
            return q.Where(r => r.Pid == pid).ToList();
        }
        else
        {
            return q.ToList();
        }
    }
    public static int AddRecord(int pid)
    {
        return SqlLiteHandler.Instance.DB.Insert(new ClockInRecord
        {
            Pid = pid,
            Time = DateTime.Now
        });
    }

#if DEBUG
    public static int AddRecordTest(int pid)
    {
        int hours = Random.Shared.Next(16, 2048);
        int minutes = Random.Shared.Next(0, 60);
        int seconds = Random.Shared.Next(0, 60);
        TimeSpan ts = new TimeSpan(hours, minutes, seconds);

        return SqlLiteHandler.Instance.DB.Insert(new ClockInRecord
        {
            Pid = pid,
            Time = DateTime.Now.Add(-ts)
        });
    }
#endif

    public static ClockInModel GetClockInModel(int id)
    {
        var item = SqlLiteHandler.Instance.DB.Find<ClockIn>(id);
        var records = SqlLiteHandler.Instance.DB.Table<ClockInRecord>().Where(r => r.Pid == id).OrderBy(r => r.Time).ToList();
        return new ClockInModel(item, records);
    }
    public static IList<ClockInModel> GetClockInModels()
    {
        var items = SqlLiteHandler.Instance.DB.Table<ClockIn>().ToList();
        var records = SqlLiteHandler.Instance.DB.Table<ClockInRecord>();

        return items.Select(i =>
        {
            var r = records.Where(r => r.Pid == i.Id).OrderBy(r => r.Time).ToList();
            return new ClockInModel(i, r);
        }).ToList();
    }
}
