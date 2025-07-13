using PZPKRecorder.Data;

namespace PZPKRecorder.Services;

internal class ClockInModel
{
    public ClockIn ClockIn { get; private set; }
    public IList<ClockInRecord> Records { get; private set; }

    public ClockInModel(ClockIn clockIn, IList<ClockInRecord> records)
    {
        ClockIn = clockIn;
        Records = records;

        for (int i = 0; i < Records.Count; i++)
        {
            Records[i].Counter = Records.Count - i;
        }
    }

    public ClockInRecord? LastRecord => Records.FirstOrDefault();
    public int GetLastDaySince(DateTime time)
    {
        if (LastRecord == null)
        {
            return -1;
        }
        else
        {
            int d = DateOnly.FromDateTime(time).DayNumber - DateOnly.FromDateTime(LastRecord.Time).DayNumber;
            return d >= 0 ? d : 0;
        }
    }

    public bool CheckRemind(DateTime time)
    {
        if (ClockIn.RemindDays > 0)
        {
            var days = GetLastDaySince(time);
            if (days > ClockIn.RemindDays)
            {
                return true;
            }
        }
        return false;
    }

    public int GetDaysApart(int counter)
    {
        var index = Records.Count - counter;

        if (index < 0 || index >= Records.Count - 1)
        {
            return 0;
        }
        var lastRecord = Records[index + 1];
        var record = Records[index];
        return DateOnly.FromDateTime(record.Time).DayNumber - DateOnly.FromDateTime(lastRecord.Time).DayNumber;
    }
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

    public static ClockInModel GetClockInModel(int id)
    {
        var item = SqlLiteHandler.Instance.DB.Find<ClockIn>(id);
        var records = SqlLiteHandler.Instance.DB.Table<ClockInRecord>().Where(r => r.Pid == id).OrderByDescending(r => r.Time).ToList();
        return new ClockInModel(item, records);
    }
    public static IList<ClockInModel> GetClockInModels()
    {
        var items = SqlLiteHandler.Instance.DB.Table<ClockIn>().ToList();
        var records = SqlLiteHandler.Instance.DB.Table<ClockInRecord>();

        return items.Select(i =>
        {
            var r = records.Where(r => r.Pid == i.Id).OrderByDescending(r => r.Time).ToList();
            return new ClockInModel(i, r);
        }).OrderBy(i => i.ClockIn.OrderNo).ToList();
    }

    public static bool CheckReminds()
    {
        var list = GetClockInModels();
        var today = DateTime.Now;

        bool remind = list.Any(m => m.CheckRemind(today));
        return remind;
    }
}
