using PZRecorder.Core.Tables;

namespace PZRecorder.Core.Managers;

public abstract class CycleTaskBase
{
    internal CycleTask InnerTask { get; init; }
    internal List<CycleTaskItem> Items { get; init; }

    public CycleTaskBase(CycleTask task, List<CycleTaskItem> items)
    {
        InnerTask = task;
        Items = items;
    }

    public string Name => InnerTask.Name;
    public int OrderNo => InnerTask.OrderNo;
    public bool IsComplete()
    {
        return Items.Count > 0 && Items.All(n => n.Completed >= n.Total);
    }
}

public sealed class OnceCycleTask : CycleTaskBase
{
    public OnceCycleTask(CycleTask task, List<CycleTaskItem> items) : base(task, items)
    {
        if (task.Mode != CycleMode.Once) throw new ArgumentException("CycleTask mode is not Once.");
    }

    public DateOnly StartDate => DateOnly.FromDayNumber(InnerTask.StartDay);
    public DateOnly EndDate => DateOnly.FromDayNumber(InnerTask.StartDay + InnerTask.CycleLength);
    public bool IsNearDeadline(DateOnly date)
    {
        return EndDate.DayNumber - date.DayNumber <= 2;
    }
}
public sealed class DaysCycleTask : CycleTaskBase
{
    public DaysCycleTask(CycleTask task, List<CycleTaskItem> items) : base(task, items)
    {
        if (task.Mode != CycleMode.Day) throw new ArgumentException("CycleTask mode is not Day.");
    }

    public DateOnly StartDate => DateOnly.FromDayNumber(InnerTask.StartDay);
    public DateOnly EndDate => DateOnly.FromDayNumber(InnerTask.StartDay + InnerTask.CycleLength);
    public bool IsNearDeadline(DateOnly date)
    {
        return EndDate.DayNumber - date.DayNumber <= 2;
    }
    public bool NeedToReset(DateOnly date)
    {
        return date >= EndDate;
    }
}
public sealed class WeeksCycleTask : CycleTaskBase
{
    public DayOfWeek ResetDay => (DayOfWeek)InnerTask.ResetPoint;
    public WeeksCycleTask(CycleTask task, List<CycleTaskItem> items) : base(task, items)
    {
        if (task.Mode != CycleMode.Week) throw new ArgumentException("CycleTask mode is not Week.");
    }

    public DateOnly StartDate => DateOnly.FromDayNumber(InnerTask.StartDay);
    public DateOnly EndDate => DateOnly.FromDayNumber(InnerTask.StartDay + 7 * InnerTask.CycleLength);

    public bool IsNearDeadline(DateOnly date)
    {
        return EndDate.DayNumber - date.DayNumber <= 2;
    }
    public bool NeedToReset(DateOnly date)
    {
        return date >= EndDate;
    }
}
public sealed class MonthsCycleTask : CycleTaskBase
{
    public MonthsCycleTask(CycleTask task, List<CycleTaskItem> items) : base(task, items)
    {
        if (task.Mode != CycleMode.Month) throw new ArgumentException("CycleTask mode is not Month.");
    }
    public DateOnly StartDate => DateOnly.FromDayNumber(InnerTask.StartDay);
    public DateOnly EndDate => ComputeEndDate();
    private DateOnly ComputeEndDate()
    {
        var start = StartDate;
        var end = start.AddMonths(InnerTask.CycleLength);

        var daysInMonth = DateTime.DaysInMonth(end.Year, end.Month);
        if (InnerTask.ResetPoint > daysInMonth)
        {
            return new DateOnly(end.Year, end.Month, daysInMonth);
        }
        else
        {
            return new DateOnly(end.Year, end.Month, InnerTask.ResetPoint);
        }
    }

    public bool IsNearDeadline(DateOnly date)
    {
        return EndDate.DayNumber - date.DayNumber <= 2;
    }
    public bool NeedToReset(DateOnly date)
    {
        return date >= EndDate;
    }

    public static DateOnly ComputeCurrentStartDate(DateOnly today, CycleTask task)
    {
        if (today.Day > task.ResetPoint)
        {
            return new DateOnly(today.Year, today.Month, task.ResetPoint);
        }
        else
        {
            var prevMonth = today.AddMonths(-1);
            var daysInPrevMonth = DateTime.DaysInMonth(prevMonth.Year, prevMonth.Month);
            if (task.ResetPoint > daysInPrevMonth)
            {
                return new DateOnly(prevMonth.Year, prevMonth.Month, daysInPrevMonth);
            }
            else
            {
                return new DateOnly(prevMonth.Year, prevMonth.Month, task.ResetPoint);
            }
        }
    }
}

public class CycleTaskManager(SqlHandler db)
{
    private SqlHandler DB { get; init; } = db;

    public List<CycleTaskKind> GetKinds()
    {
        var list = DB.Conn.Table<CycleTaskKind>().ToList();
        list.Sort((x, y) => x.OrderNo - y.OrderNo);

        return list;
    }
    public List<CycleTask> GetTasks(int kindId)
    {
        var list = DB.Conn.Table<CycleTask>().Where(n => n.KindId == kindId).ToList();
        list.Sort((x, y) => x.OrderNo - y.OrderNo);

        return list;
    }
}
