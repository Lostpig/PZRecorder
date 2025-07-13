using PZPKRecorder.Data;

namespace PZPKRecorder.Services;

internal class DailyService
{
    public static DateOnly GetMondayDate(DateOnly date)
    {
        int diffDays = 0;
        if (date.DayOfWeek == DayOfWeek.Sunday) diffDays = 6;
        else diffDays = (int)date.DayOfWeek - 1;

        var mondayDate = DateOnly.FromDayNumber(date.DayNumber - diffDays);

        return mondayDate;
    }

    public static List<Daily> GetDailies(DailyState? state)
    {
        var q = SqlLiteHandler.Instance.DB.Table<Daily>();
        if (state is not null) q = q.Where(d => d.State == state);

        return q.OrderByDescending(d => d.State).ThenBy(d => d.OrderNo).ToList();
    }

    public static Daily? GetDaily(int id)
    {
        return SqlLiteHandler.Instance.DB.Find<Daily>(id);
    }

    public static int UpdateDaily(Daily daily)
    {
        return SqlLiteHandler.Instance.DB.Update(daily);
    }
    public static int InsertDaily(Daily daily)
    {
        return SqlLiteHandler.Instance.DB.Insert(daily);
    }
    public static int DeleteDaily(int id)
    {
        int result = 0;
        SqlLiteHandler.Instance.DB.RunInTransaction(() =>
        {
            SqlLiteHandler.Instance.DB.Table<DailyWeek>().Delete(dw => dw.DailyId == id);
            result = SqlLiteHandler.Instance.DB.Delete<Daily>(id);
        });

        return result;
    }

    public static int WriteDailyWeek(DailyWeek dw)
    {
        return SqlLiteHandler.Instance.DB.InsertOrReplace(dw);
    }
    public static List<DailyWeek> GetDailyDatas(int? dailyId)
    {
        var data = SqlLiteHandler.Instance.DB.Table<DailyWeek>();
        if (dailyId != null)
        {
            return data.Where(dw => dw.DailyId == dailyId).ToList();
        }
        else
        {
            return data.ToList();
        }
    }
    public static List<DailyWeek> GetDailyWeeks(DateOnly mondayDate, IList<int> dailyIds)
    {
        int mondayDay = mondayDate.DayNumber;
        // var ids = dailyIds.Select(d => DailyWeek.CreateId(d, mondayDay)).ToList();
        // return SqlLiteHandler.Instance.DB.Table<DailyWeek>().Where(dw => ids.Contains(dw.Id)).ToList();
        return SqlLiteHandler.Instance.DB.Table<DailyWeek>().Where(dw => dw.MondayDay == mondayDay && dailyIds.Contains(dw.DailyId)).ToList();
    }
    public static DailyWeek GetDailyWeek(DateOnly mondayDate, int dailyId)
    {
        var id = DailyWeek.CreateId(dailyId, mondayDate.DayNumber);

        return SqlLiteHandler.Instance.DB.Find<DailyWeek>(id);
    }

    public static void UpdateDailyWeekByWatcher(int dailyId, DateOnly day)
    {
        var daily = GetDaily(dailyId);
        if (daily == null || daily.State != DailyState.Enabled) return;

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
        BroadcastService.Broadcast(BroadcastEvent.WatcherChangedDaily, string.Empty);
    }
}
