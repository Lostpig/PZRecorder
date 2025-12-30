using Avalonia.Controls.Notifications;
using SukiUI.Dialogs;

namespace PZRecorder.Desktop.Common;

public interface IResultDialog<T>
{
    T GetResult();
    bool Check();
}
public record DialogResult<T>(bool ButtonValue, T Result);

public class DialogButton<T>(string text, T value, bool dismiss = false)
{
    public string Text { get; set; } = text;
    public string[]? Styles { get; set; }
    public T Value { get; set; } = value;
    public bool Dismiss { get; set; } = dismiss;
}
public class DialogOptions<T>
{
    public string Title { get; set; } = "";
    public object Content { get; set; } = "";
    public NotificationType? Type { get; set; } = null;
    public DialogButton<T>[] Buttons { get; set; } = [];
}
public class ResultDialogOptions<T>(IResultDialog<T> content)
{
    public string Title { get; set; } = "";
    public IResultDialog<T> Content { get; set; } = content;
    public NotificationType? Type { get; set; } = null;
    public DialogButton<bool>[] Buttons { get; set; } = [];
}

public class PzDialog(ISukiDialogManager manager)
{
    public ISukiDialogManager Manager { get; init; } = manager;

    public Task<T> ShowDialog<T>(DialogOptions<T> options)
    {
        var builder = Manager.CreateDialog()
            .WithTitle(options.Title)
            .WithContent(options.Content);
        if (options.Type != null) builder.OfType(options.Type.Value);

        var completion = new TaskCompletionSource<T>();
        for (int i = 0; i < options.Buttons.Length; i++)
        {
            var btn = options.Buttons[i];
            builder.AddActionButton(btn.Text, d =>
            {
                completion.SetResult(btn.Value);
            }, btn.Dismiss, btn.Styles ?? []);
        }
        builder.TryShow();

        return completion.Task;
    }
    public Task<DialogResult<T>> ShowResultDialog<T>(ResultDialogOptions<T> options)
    {
        var builder = Manager.CreateDialog()
            .WithTitle(options.Title)
            .WithContent(options.Content);
        if (options.Type != null) builder.OfType(options.Type.Value);

        var completion = new TaskCompletionSource<DialogResult<T>>();
        for (int i = 0; i < options.Buttons.Length; i++)
        {
            var btn = options.Buttons[i];
            builder.AddActionButton(btn.Text, d =>
            {
                var res = options.Content.GetResult();
                if (btn.Value && options.Content.Check())
                {
                    completion.SetResult(new(btn.Value, res));
                    d.Dismiss();
                }
                else
                {
                    completion.SetResult(new(btn.Value, res));
                    d.Dismiss();
                }
            }, false, btn.Styles ?? []);
        }
        builder.TryShow();

        return completion.Task;
    }

    public static DialogOptions<bool> ConfirmOptions(string title, object content)
    {
        return new DialogOptions<bool>
        {
            Title = title,
            Content = content,
            Type = NotificationType.Information,
            Buttons = [
                new("OK", true, true),
                new("Cancel", false, true) { Styles = ["Accent"] },
            ]
        };
    }
    public static DialogOptions<bool> AlertOptions(string title, object content)
    {
        return new DialogOptions<bool>
        {
            Title = title,
            Content = content,
            Type = NotificationType.Warning,
            Buttons = [
                new("OK", true, true)
            ]
        };
    }
    public static ResultDialogOptions<T> ResultOptions<T>(string title, IResultDialog<T> content)
    {
        return new ResultDialogOptions<T>(content)
        {
            Title = title,
            Buttons = [
                new("OK", true, true),
                new("Cancel", false, true) { Styles = ["Accent"] },
            ],
        };
    }
}

