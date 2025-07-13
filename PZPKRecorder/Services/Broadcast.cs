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
}

internal class BroadcastService
{
    static HashSet<Action<BroadcastEvent, string>> ReceiverActions = new();

    public static void RegisterReceiver(Action<BroadcastEvent, string> action)
    {
        ReceiverActions.Add(action);
    }
    public static void RemoveReceiver(Action<BroadcastEvent, string> action)
    {
        ReceiverActions.Remove(action);
    }

    public static void Broadcast(BroadcastEvent ev, string eventArg)
    {
        foreach (var action in ReceiverActions)
        {
            action(ev, eventArg);
        }
    }
}
