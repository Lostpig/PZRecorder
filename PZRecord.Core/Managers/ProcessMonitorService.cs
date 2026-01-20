using PZRecorder.Core.Tables;
using System.Diagnostics;

namespace PZRecorder.Core.Managers;

public enum ProcessStatus { Started, Exited, None }
public record MonitorRecord(ProcessWatch Watch, DateTime StartTime, DateTime EndTime);
public record ProcessChangedArgs(ProcessWatch Watch, bool IsRunning, DateTime StartTime);
public class ProcessMonitor
{
    public ProcessWatch Watch { get; init; }
    public DateTime StartTime { get; private set; }
    public bool Runing { get; private set; } = false;

    internal ProcessMonitor(ProcessWatch watch)
    {
        Watch = watch;
    }
    internal ProcessStatus CheckProcess()
    {
        var process = Process.GetProcessesByName(Watch.ProcessName).FirstOrDefault();
        if (process == null && Runing)
        {
            // Process has exited
            Runing = false;
            Debug.WriteLine($"Find Process [{Watch.ProcessName}] exited");
            return ProcessStatus.Exited;
        }
        else if (process is not null && !Runing)
        {
            Runing = true;
            StartTime = process.StartTime;
            Debug.WriteLine($"Find Process [{Watch.ProcessName}] running");
            return ProcessStatus.Started;
        }

        return ProcessStatus.None;
    }
}
public sealed class ProcessMonitorService(ProcessMonitorManager pmManager, DailyManager dManager)
{
    readonly List<ProcessMonitor> _items = [];
    readonly ProcessMonitorManager _pmManager = pmManager;
    readonly DailyManager _dManager = dManager;
    public event Action<ProcessChangedArgs>? OnProcessChanged;
    public event Action<MonitorRecord>? OnProcessRecorded;

    public void ExcuteWatch()
    {
        lock (_items)
        {
            try
            {
                foreach (var item in _items)
                {
                    var status = item.CheckProcess();
                    if (status == ProcessStatus.Started || status == ProcessStatus.Exited)
                    {
                        OnProcessChanged?.Invoke(new(item.Watch, item.Runing, item.StartTime));
                    }
                    if (status == ProcessStatus.Exited)
                    {
                        var mr = new MonitorRecord(item.Watch, item.StartTime, DateTime.Now);
                        AddProcessRecord(mr);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }
    }
    private void AddProcessRecord(MonitorRecord mreocrd)
    {
        var date = DateOnly.FromDateTime(mreocrd.StartTime);
        int startTimeSeconds = (int)mreocrd.StartTime.TimeOfDay.TotalSeconds;
        int exitTimeSeconds = (int)mreocrd.EndTime.TimeOfDay.TotalSeconds;
        if (DateOnly.FromDateTime(mreocrd.EndTime) != date)
        {
            // If the exit time is not on the same day
            exitTimeSeconds += 24 * 3600;
        }

        var record = new ProcessRecord()
        {
            Pid = mreocrd.Watch.Id,
            Date = date.DayNumber,
            StartTime = startTimeSeconds,
            EndTime = exitTimeSeconds
        };
        _pmManager.InsertRecord(record);

        if (mreocrd.Watch.BindingDaily)
        {
            TimeSpan duration = mreocrd.EndTime - mreocrd.StartTime;
            if (duration.TotalMinutes >= mreocrd.Watch.DailyDuration)
            {
                var writed = _dManager.UpdateDailyWeekByWatcher(mreocrd.Watch.DailyId, date);
                if (writed) OnProcessRecorded?.Invoke(mreocrd);
            }
        }
    }

    public void UpdateWatches()
    {
        var watches = _pmManager.GetAllWatches();
        UpdateWatches(watches);
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
                    _items.Add(new ProcessMonitor(watch));
                }
            }
        }
    }
    public List<ProcessMonitor> GetRunningMonitor()
    {
        lock (_items)
        {
            return _items.Where(i => i.Runing).ToList();
        }
    }
}

