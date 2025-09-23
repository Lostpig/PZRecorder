using PZPKRecorder.Data;
using System.Diagnostics;
using TrdTimer = System.Threading.Timer;

namespace PZPKRecorder.Services;

internal class ProcessWatchService
{
    public static WatcherManager? WatchManager { get; private set; }
    public static void InitializeWatcher()
    {
        if (WatchManager is not null) return;
        var watches = GetAllWatches();
        WatchManager = new WatcherManager(watches);
    }
    public static void UpdateWatcher()
    {
        if (WatchManager is not null)
        {
            var watches = GetAllWatches();
            WatchManager.UpdateWatches(watches);
        }
    }

    public static List<ProcessWatch> GetAllWatches()
    {
        return SqlLiteHandler.Instance.DB.Table<ProcessWatch>().OrderBy(pw => pw.OrderNo).ToList();
    }
    public static ProcessWatch GetWatch(int id)
    {
        return SqlLiteHandler.Instance.DB.Find<ProcessWatch>(id);
    }
    public static int UpdateWatch(ProcessWatch item)
    {
        var res = SqlLiteHandler.Instance.DB.Update(item);
        UpdateWatcher();
        return res;
    }
    public static int InsertWatch(ProcessWatch item)
    {
        var res = SqlLiteHandler.Instance.DB.Insert(item);
        UpdateWatcher();
        return res;
    }
    public static int DeleteWatch(int id)
    {
        int result = 0;
        SqlLiteHandler.Instance.DB.RunInTransaction(() =>
        {
            SqlLiteHandler.Instance.DB.Table<ProcessRecord>().Delete(pr => pr.Pid == id);
            result = SqlLiteHandler.Instance.DB.Delete<ProcessWatch>(id);
        });

        UpdateWatcher();
        return result;
    }
    public static bool CheckDuplicate(string processName)
    {
        var exists = SqlLiteHandler.Instance.DB.Table<ProcessWatch>().Where(pw => pw.ProcessName == processName).FirstOrDefault();
        return exists == null;
    }

    public static int InsertRecord(ProcessRecord record)
    {
        return SqlLiteHandler.Instance.DB.Insert(record);
    }
    public static List<ProcessRecord> GetRecords(int pid, int startDate, int endDate)
    {
        return SqlLiteHandler.Instance.DB.Table<ProcessRecord>()
            .Where(pr => pr.Pid == pid && pr.Date >= startDate && pr.Date <= endDate)
            .OrderByDescending(pr => pr.Date)
            .ToList();
    }
    public static List<ProcessRecord> GetAllRecords()
    {
        return SqlLiteHandler.Instance.DB.Table<ProcessRecord>().ToList();
    }
}

internal record WatchingRecord(int Id, string Name, string ProcessName, DateTime StartTime);
class ProcessWatcher
{
    public ProcessWatch Watch { get; init; }
    public DateTime StartTime { get; private set; }
    public bool Runing { get; private set; } = false;

    public ProcessWatcher(ProcessWatch watch)
    {
        Watch = watch;
    }
    public bool CheckProcess()
    {
        var process = Process.GetProcessesByName(Watch.ProcessName).FirstOrDefault();
        if (process == null && Runing)
        {
            // Process has exited
            Runing = false;
            AddProcessRecord(Watch, StartTime, DateTime.Now);
            
#if DEBUG
            Debug.WriteLine($"Find Process [{Watch.ProcessName}] exited");
#endif
            return true;
        }
        else if (process is not null && !Runing)
        {
            Runing = true;
            StartTime = process.StartTime;
            
#if DEBUG
            Debug.WriteLine($"Find Process [{Watch.ProcessName}] running");
#endif
            return true;
        }

        return false;
    }

    private static void AddProcessRecord(ProcessWatch watch, DateTime startTime, DateTime exitTime)
    {
        var date = DateOnly.FromDateTime(startTime);
        int startTimeSeconds = (int)startTime.TimeOfDay.TotalSeconds;
        int exitTimeSeconds = (int)exitTime.TimeOfDay.TotalSeconds;
        if (DateOnly.FromDateTime(exitTime) != date)
        {
            // If the exit time is not on the same day
            exitTimeSeconds += 24 * 3600;
        }

        var record = new ProcessRecord()
        {
            Pid = watch.Id,
            Date = date.DayNumber,
            StartTime = startTimeSeconds,
            EndTime = exitTimeSeconds
        };
        ProcessWatchService.InsertRecord(record);

        if (watch.BindingDaily)
        {
            TimeSpan duration = DateTime.Now - startTime;
            if (duration.TotalMinutes >= watch.DailyDuration)
            {
                DailyService.UpdateDailyWeekByWatcher(watch.DailyId, date);
                BroadcastService.Broadcast(BroadcastEvent.WatcherChangedDaily, string.Empty, true);
            }
        }
    }
}
internal class WatcherManager : IDisposable
{
    readonly TrdTimer _timer;
    readonly List<ProcessWatcher> _items = new();

    public WatcherManager(List<ProcessWatch> watches)
    {
        UpdateWatches(watches);

        int timerInterval = 60 * 1000; // per minute
#if DEBUG
        timerInterval = 10 * 1000; // 10 seconds for debugging
#endif
        _timer = new TrdTimer(ExcuteWatch, null, timerInterval, timerInterval); 
    }
    private void ExcuteWatch(object? _)
    {
        lock (_items)
        {
            try
            {
                foreach (var item in _items)
                {
                    var changed = item.CheckProcess();
                    if (changed)
                    {
                        BroadcastService.Broadcast(BroadcastEvent.RunningProcessChanged, string.Empty, true);
                    }
                }
            }
            catch (Exception ex)
            {
                ExceptionProxy.CatchException(ex);
            }
        }
    }
    public void UpdateWatches(List<ProcessWatch> watches)
    {
        lock (_items)
        {
            _items.Clear();
            foreach (var watch in watches)
            {
                if (watch.Enabled && !string.IsNullOrWhiteSpace(watch.ProcessName))
                {
                    _items.Add(new ProcessWatcher(watch));
                }
            }
        }
    }

    public List<WatchingRecord> GetRunningRecords()
    {
        lock (_items)
        {
            return _items
                .Where(i => i.Runing)
                .Select(i => new WatchingRecord(i.Watch.Id, i.Watch.Name, i.Watch.ProcessName, i.StartTime))
                .ToList();
        }
    }
    public void Dispose()
    {
        _items.Clear();
        _timer.Dispose();
    }
}
