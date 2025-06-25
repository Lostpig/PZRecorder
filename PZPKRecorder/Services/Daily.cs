using PZPKRecorder.Data;

namespace PZPKRecorder.Services;

internal class DailyService
{

    public static IList<Daily> GetDailies (DailyState? state)
    {
        var q = SqlLiteHandler.Instance.DB.Table<Daily>();
        if (state is not null) q = q.Where(d =>  d.State == state);

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
        var ids = dailyIds.Select(d => DailyWeek.CreateId(d, mondayDay)).ToList();

        /// return SqlLiteHandler.Instance.DB.Table<DailyWeek>().Where(dw => ids.Contains(dw.Id)).ToList();
        return SqlLiteHandler.Instance.DB.Table<DailyWeek>().Where(dw => dw.MondayDay == mondayDay && dailyIds.Contains(dw.DailyId)).ToList();
    }
}
