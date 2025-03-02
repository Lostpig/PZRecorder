namespace PZPKRecorder.Services;

internal static class BroadcastEventName
{
    public const string ExceptionCatch = "ExceptionCatch";
    public const string LanguageChanged = "LanguageChanged";
    public const string ThemeChanged = "ThemeChanged";
    public const string WindowActivated = "WindowActivated";
}

internal class BroadcastService
{
    static HashSet<Action<string, string>> ReceiverActions = new();

    public static void RegisterReceiver(Action<string, string> action)
    {
        ReceiverActions.Add(action);
    }
    public static void RemoveReceiver(Action<string, string> action)
    {
        ReceiverActions.Remove(action);
    }

    public static void Broadcast(string eventName, string eventArg)
    {
        foreach (var action in ReceiverActions)
        {
            action(eventName, eventArg);
        }
    }
}
