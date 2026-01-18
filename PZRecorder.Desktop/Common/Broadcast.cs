using PZRecorder.Core.Managers;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace PZRecorder.Desktop.Common;

internal enum BroadcastEvent
{
    LanguageChanged,
    ThemeChanged,
    WindowActivated,
    DateChanged,
    RemindStateChanged,
    DailyRecorded,
    DataImported,
    TimerTick
}

internal sealed class BroadcastManager
{
    private readonly Subject<BroadcastEvent> _broadcast = new();
    public IObservable<BroadcastEvent> Broadcast => _broadcast;
    public void Publish(BroadcastEvent ev) => _broadcast.OnNext(ev);

    private readonly Subject<string> _exceptionCatched = new();
    public IObservable<string> ExceptionCatched => _exceptionCatched;
    public void OnExceptionCatched(string message) => _exceptionCatched.OnNext(message);


    private readonly Subject<ProcessChangedArgs> _processChanged = new();
    public IObservable<ProcessChangedArgs> ProcessChanged => _processChanged;
    public void OnProcessChanged(ProcessChangedArgs e) => _processChanged.OnNext(e);
}
