using Avalonia.Controls.Notifications;
using SukiUI.Toasts;

namespace PZRecorder.Desktop.Common;

public class PzToast(ISukiToastManager manager)
{
    public ISukiToastManager Manager { get; init; } = manager;

    public void ShowToast(string title, string message, NotificationType toastType, int time = 3)
    {
        Manager.CreateToast()
            .WithTitle(title)
            .WithContent(message)
            .OfType(toastType)
            .Dismiss().After(TimeSpan.FromSeconds(time))
            .Queue();
    }

    public void Error(string message, string? title = null) =>
        ShowToast(title ?? "Error", message, NotificationType.Error);
    public void Warning(string message, string? title = null) =>
        ShowToast(title ?? "Warning", message, NotificationType.Warning);
    public void Info(string message, string? title = null) =>
        ShowToast(title ?? "Info", message, NotificationType.Information);
    public void Success(string message, string? title = null) =>
        ShowToast(title ?? "Success", message, NotificationType.Success);
}
