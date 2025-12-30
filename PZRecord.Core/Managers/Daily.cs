using PZRecorder.Core;
using PZRecorder.Core.Common;
using PZRecorder.Core.Tables;

namespace PZRecorder.Core.Managers;

public class DailyManager(SqlHandler db)
{
    private SqlHandler DB { get; init; } = db;

    public DateOnly GetMondayDate(DateOnly date)
    {
        int diffDays = 0;
        if (date.DayOfWeek == DayOfWeek.Sunday) diffDays = 6;
        else diffDays = (int)date.DayOfWeek - 1;

        var mondayDate = DateOnly.FromDayNumber(date.DayNumber - diffDays);

        return mondayDate;
    }

    public List<Daily> GetDailies(EnableState? state = null)
    {
        var q = DB.Conn.Table<Daily>();
        if (state is not null) q = q.Where(d => d.State == state);

        return q.OrderByDescending(d => d.State).ThenBy(d => d.OrderNo).ToList();
    }

    public Daily? GetDaily(int id)
    {
        return DB.Conn.Find<Daily>(id);
    }

    public int UpdateDaily(Daily daily)
    {
        return DB.Conn.Update(daily);
    }
    public int InsertDaily(Daily daily)
    {
        return DB.Conn.Insert(daily);
    }
    public int DeleteDaily(int id)
    {
        int result = 0;
        DB.Conn.RunInTransaction(() =>
        {
            DB.Conn.Table<DailyWeek>().Delete(dw => dw.DailyId == id);
            result = DB.Conn.Delete<Daily>(id);
        });

        return result;
    }

    public int WriteDailyWeek(DailyWeek dw)
    {
        return DB.Conn.InsertOrReplace(dw);
    }
    public List<DailyWeek> GetDailyDatas(int? dailyId)
    {
        var data = DB.Conn.Table<DailyWeek>();
        if (dailyId != null)
        {
            return data.Where(dw => dw.DailyId == dailyId).ToList();
        }
        else
        {
            return data.ToList();
        }
    }
    public List<DailyWeek> GetDailyWeeks(DateOnly mondayDate, IList<int> dailyIds)
    {
        int mondayDay = mondayDate.DayNumber;
        // var ids = dailyIds.Select(d => DailyWeek.CreateId(d, mondayDay)).ToList();
        // return DB.Conn.Table<DailyWeek>().Where(dw => ids.Contains(dw.Id)).ToList();
        return DB.Conn.Table<DailyWeek>().Where(dw => dw.MondayDay == mondayDay && dailyIds.Contains(dw.DailyId)).ToList();
    }
    public DailyWeek GetDailyWeek(DateOnly mondayDate, int dailyId)
    {
        var id = DailyWeek.CreateId(dailyId, mondayDate.DayNumber);

        return DB.Conn.Find<DailyWeek>(id);
    }

    public void UpdateDailyWeekByWatcher(int dailyId, DateOnly day)
    {
        var daily = GetDaily(dailyId);
        if (daily == null || daily.State != EnableState.Enabled) return;

        var mondayDate = GetMondayDate(day);
        var dailyweek = GetDailyWeek(mondayDate, dailyId);
        if (dailyweek == null)
        {
            dailyweek = new();
            dailyweek.Init(daily.Id, mondayDate);
        }

        if (dailyweek[day.DayOfWeek] == 1) return;

        dailyweek[day.DayOfWeek] = 1;
        WriteDailyWeek(dailyweek);
    }
}
