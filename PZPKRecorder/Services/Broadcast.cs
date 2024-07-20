namespace PZPKRecorder.Services;

internal static class BroadcastEventName
{
    public const string ExceptionCatch = "ExceptionCatch";
    public const string LanguageChanged = "LanguageChanged";
    public const string ThemeChanged = "ThemeChanged";
}

internal class BroadcastService
{
    static Action<string, string>? ReceiverAction;

    public static void BindingReceiver(Action<string, string> action)
    {
        ReceiverAction = action;
    }
    public static void Broadcast(string eventName, string eventArg)
    {
        if (ReceiverAction != null)
        {
            ReceiverAction(eventName, eventArg);
        }
    }
}
