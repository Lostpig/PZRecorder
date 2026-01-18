using PZRecorder.Core.Managers;
using PZRecorder.Desktop.Common;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace PZRecorder.Desktop.Modules.Shared;

internal sealed class BackgroundService : IDisposable
{
    private System.Threading.Timer _timer;
    private DateTime _lastCheckTime;
    private readonly BroadcastManager _broadcast;
    private readonly ProcessMonitorService _monitorService;

    public BackgroundService(BroadcastManager broadcast, ProcessMonitorService monitorService)
    {
        _broadcast = broadcast;
        _monitorService = monitorService;

        _monitorService.UpdateWatches();
        _monitorService.OnProcessChanged += OnProcessChanged;
        _monitorService.OnProcessRecorded += OnProcessRecorded;

        InitTimer();
    }

    private void OnProcessRecorded(MonitorRecord _)
    {
        _broadcast.Publish(BroadcastEvent.DailyRecorded);
    }
    private void OnProcessChanged(ProcessChangedArgs e)
    {
        _broadcast.OnProcessChanged(e);
    }

    [MemberNotNull(nameof(_timer))]
    private void InitTimer()
    {
        _lastCheckTime = DateTime.Now;

        int interval = 1000 * 60; // 1 min
#if DEBUG
        interval = 1000 * 10; // 10 sec
#endif

        _timer = new ((e) =>
        {
            var now = DateTime.Now;
            if (now.DayOfYear != _lastCheckTime.DayOfYear)
            {
                _broadcast.Publish(BroadcastEvent.DateChanged);
            }
            _monitorService.ExcuteWatch();
            _broadcast.Publish(BroadcastEvent.TimerTick);

            _lastCheckTime = now;
            Debug.WriteLine("Timer ticked");
        }, null, 1000, interval); // 1 minutes
    }

    public void Dispose()
    {
        _timer.Dispose();
    }
}
