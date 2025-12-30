using PZRecorder.Core;
using PZRecorder.Core.Tables;

namespace PZRecorder.Core.Managers;

public class ClockInCollection
{
    public ClockIn ClockIn { get; private set; }
    public IList<ClockInRecord> Records { get; private set; }

    public ClockInCollection(ClockIn clockIn, IList<ClockInRecord> records)
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

public class ClockInManager(SqlHandler db)
{
    private SqlHandler DB { get; init; } = db;

    public List<ClockIn> GetClockIns()
    {
        return DB.Conn.Table<ClockIn>().ToList();
    }

    public int AddClockIn(ClockIn clockIn)
    {
        return DB.Conn.Insert(clockIn);
    }
    public bool UpdateClockIn(ClockIn clockIn)
    {
        var count = DB.Conn.Update(clockIn);
        return count > 0;
    }

    public void DeleteClockIn(int id)
    {
        DB.Conn.RunInTransaction(() =>
        {
            DB.Conn.Delete<ClockIn>(id);
            DB.Conn.Table<ClockInRecord>().Delete(r => r.Pid == id);
        });
    }

    public List<ClockInRecord> GetRecords(int? pid)
    {
        var q = DB.Conn.Table<ClockInRecord>();

        if (pid is not null)
        {
            return q.Where(r => r.Pid == pid).ToList();
        }
        else
        {
            return q.ToList();
        }
    }
    public int AddRecord(int pid)
    {
        return DB.Conn.Insert(new ClockInRecord
        {
            Pid = pid,
            Time = DateTime.Now
        });
    }

    public ClockInCollection GetCollection(int id)
    {
        var item = DB.Conn.Find<ClockIn>(id);
        var records = DB.Conn.Table<ClockInRecord>().Where(r => r.Pid == id).OrderByDescending(r => r.Time).ToList();
        return new ClockInCollection(item, records);
    }
    public List<ClockInCollection> GetCollections()
    {
        var items = DB.Conn.Table<ClockIn>().ToList();
        var records = DB.Conn.Table<ClockInRecord>();

        return items.Select(i =>
        {
            var r = records.Where(r => r.Pid == i.Id).OrderByDescending(r => r.Time).ToList();
            return new ClockInCollection(i, r);
        }).OrderBy(i => i.ClockIn.OrderNo).ToList();
    }

    public bool CheckReminds()
    {
        var list = GetCollections();
        var today = DateTime.Now;

        bool remind = list.Any(m => m.CheckRemind(today));
        return remind;
    }
}
