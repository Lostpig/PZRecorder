namespace PZPKRecorder.Services;

public enum BroadcastEvent
{
    ExceptionCatch,
    LanguageChanged,
    ThemeChanged,
    WindowActivated,
    DateChanged,
    RemindStateChanged,
    WatcherChangedDaily,
    RunningProcessChanged,
}
public record BroadcastEventArgs(BroadcastEvent Event, string EventArg, bool UseInvokeAsync);

internal class BroadcastService
{
    static HashSet<Action<BroadcastEventArgs>> ReceiverActions = new();

    public static void RegisterReceiver(Action<BroadcastEventArgs> action)
    {
        ReceiverActions.Add(action);
    }
    public static void RemoveReceiver(Action<BroadcastEventArgs> action)
    {
        ReceiverActions.Remove(action);
    }

    public static void Broadcast(BroadcastEvent ev, string eventArg = "", bool useInvokeAsync = false)
    {
        foreach (var action in ReceiverActions)
        {
            action(new(ev, eventArg, useInvokeAsync));
        }
    }
}
