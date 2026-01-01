using Avalonia.Controls.Notifications;

namespace PZRecorder.Desktop.Common;

public class PzNotification(WindowNotificationManager manager)
{
    private WindowNotificationManager Manager { get; init; } = manager;

    public void ShowNotification(string title, string message, NotificationType type, int time = 3)
    {
        Manager.Show(new Notification(title, message, type, TimeSpan.FromSeconds(time)));
    }

    public void Error(string message, string? title = null) =>
        ShowNotification(title ?? "Error", message, NotificationType.Error);
    public void Warning(string message, string? title = null) =>
        ShowNotification(title ?? "Warning", message, NotificationType.Warning);
    public void Info(string message, string? title = null) =>
        ShowNotification(title ?? "Info", message, NotificationType.Information);
    public void Success(string message, string? title = null) =>
        ShowNotification(title ?? "Success", message, NotificationType.Success);
}
