using PZPKRecorder.Data;
using System.Diagnostics;
using TrdTimer = System.Threading.Timer;

namespace PZPKRecorder.Services;

internal class ProcessWatchService
{
    public static ProcessWatcher? Watcher { get; private set; }
    public static void InitializeWatcher()
    {
        if (Watcher is not null) return;
        var watches = GetAllWatches();
        Watcher = new ProcessWatcher(watches);
    }
    public static void UpdateWatcher()
    {
        if (Watcher is not null)
        {
            var watches = GetAllWatches();
            Watcher.UpdateWatches(watches);
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
    public static List<ProcessRecord> GetRecords(int pid)
    {
        return SqlLiteHandler.Instance.DB.Table<ProcessRecord>()
            .Where(pr => pr.Pid == pid)
            .OrderBy(pr => pr.StartTime)
            .ToList();
    }
    public static List<ProcessRecord> GetAllRecords()
    {
        return SqlLiteHandler.Instance.DB.Table<ProcessRecord>().ToList();
    }
}

class WatcherRecord
{
    public ProcessWatch Watch { get; init; }
    private DateTime StartTime { get; set; }
    private bool Runing { get; set; } = false;

    public WatcherRecord(ProcessWatch watch)
    {
        Watch = watch;
    }
    public void CheckProcess()
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
        }
        else if (process is not null && !Runing)
        {
            Runing = true;
            StartTime = DateTime.Now;
#if DEBUG
            Debug.WriteLine($"Find Process [{Watch.ProcessName}] running");
#endif
        }
    }

    private void AddProcessRecord(ProcessWatch watch, DateTime startTime, DateTime exitTime)
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
            if (duration.Minutes >= watch.DailyDuration)
            {
                DailyService.UpdateDailyWeekByWatcher(watch.DailyId, date);
            }
        }
    }
}
internal class ProcessWatcher : IDisposable
{
    TrdTimer _timer;
    List<WatcherRecord> _items = new();

    public ProcessWatcher(List<ProcessWatch> watches)
    {
        UpdateWatches(watches);

        int timerInterval = 60 * 1000; // 1 minute
#if DEBUG
        timerInterval = 10 * 1000; // 10 seconds for debugging
#endif
        _timer = new TrdTimer(ExcuteWatch, null, timerInterval, timerInterval); // per minute
    }
    private void ExcuteWatch(object? _)
    {
        lock (_items)
        {
            try
            {
                foreach (var item in _items)
                {
                    item.CheckProcess();
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
                    _items.Add(new WatcherRecord(watch));
                }
            }
        }
    }

    public void Dispose()
    {
        _items.Clear();
        _timer.Dispose();
    }
}
