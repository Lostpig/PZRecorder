using PZPKRecorder.Data;
using SQLite;

namespace PZPKRecorder.Services;

internal class DailyService
{

    public static IList<Daily> GetDailies (DailyState? state)
    {
        string query = $"SELECT * FROM t_daily WHERE 1 = 1 "
                + $" {(state is not null ? "AND state = " + (int)state : "")}"
                + $" ORDER BY state DESC, order_no ASC";
        return SqlLiteHandler.Instance.DB.Query<Daily>(query);
    }

    public static Daily GetDaily(int id)
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
        string deleteQuery = $"DELETE FROM t_dailyweek WHERE daily_id = {id}";
        int result = 0;
        SqlLiteHandler.Instance.DB.RunInTransaction(() =>
        {
            SqlLiteHandler.Instance.DB.Query<DailyWeek>(deleteQuery);
            result = SqlLiteHandler.Instance.DB.Delete<Daily>(id);
        });

        return result;
    }

    public static int WriteDailyWeek(DailyWeek dw)
    {
        return SqlLiteHandler.Instance.DB.InsertOrReplace(dw);
    }
    public static IList<DailyWeek> GetDailyDatas(int? dailyId)
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
    public static IList<DailyWeek> GetDailyWeeks(DateOnly mondayDate, IList<int> dailyIds)
    {
        int mondayDay = mondayDate.DayNumber;

        return SqlLiteHandler.Instance.DB.Table<DailyWeek>().Where(dw => dw.MondayDay == mondayDay && dailyIds.Contains(dw.DailyId)).ToList();
    }
}
