using Avalonia.Media;
using Irihi.Avalonia.Shared.Contracts;
using Ursa.Controls;

namespace PZRecorder.Desktop.Common;

public abstract class DialogContentBase<T> : PZComponentBase
{
    public abstract T GetResult(DialogResult buttonValue);
    public abstract bool Check(DialogResult buttonValue);
    protected DialogContentBase(ViewInitializationStrategy strategy = ViewInitializationStrategy.Lazy) : base(strategy) { }
}
public class PzDialog<T> : ContentControl, IDialogContext
{
    private DialogContentBase<T> _content;
    public PzDialog(ResultDialogOptions<T> options)
    {
        _content = options.Content;
        Content = PzGrid(rows: "auto, *, auto")
            .Margin(24)
            .Children(
                PzText(options.Title)
                    .Row(0)
                    .FontSize(16)
                    .FontWeight(Avalonia.Media.FontWeight.Bold)
                    .Margin(8),
                options.Content.Row(1),
                BuildButtons(options.Buttons).Row(2)
            );
    }
    private StackPanel BuildButtons(DialogButton[] buttons)
    {
        var btns = buttons.Select(btn =>
            PzButton(btn.Text, btn.Styles ?? [])
            .OnClick(_ => TryClose(btn))
        ).ToArray();

        return HStackPanel()
                .Align(UnionAlign.Right)
                .Spacing(8)
                .Children(children: btns);
    }

    private void TryClose(DialogButton btn)
    {
        if (btn.HasResult && _content.Check(btn.Value))
        {
            RequestClose?.Invoke(this, _content.GetResult(btn.Value));
        }
        else if (!btn.HasResult)
        {
            RequestClose?.Invoke(this, null);
        }
    }
    public event EventHandler<object?>? RequestClose;
    public void Close()
    {
        
    }
}

public sealed class PzMessageBoxContent : DialogContentBase<DialogResult>
{
    private string _message;
    public PzMessageBoxContent(string message) : base()
    {
        _message = message;
        Initialize();
    }

    protected override Control Build()
    {
        return PzText(_message)
            .TextWrapping(TextWrapping.Wrap)
            .Margin(8);
    }
    public override DialogResult GetResult(DialogResult buttonValue)
    {
        return buttonValue;
    }
    override public bool Check(DialogResult buttonValue)
    {
        return true;
    }
}

public record DialogResult<T>(bool ButtonValue, T Result);
public class DialogButton(string text, DialogResult value, bool hasResult = false)
{
    public string Text { get; set; } = text;
    public string[]? Styles { get; set; }
    public DialogResult Value { get; set; } = value;
    public bool HasResult { get; set; } = hasResult;
}
public class ResultDialogOptions<T>(DialogContentBase<T> content)
{
    public string Title { get; set; } = "";
    public DialogContentBase<T> Content { get; set; } = content;
    public DialogMode Mode { get; set; } = DialogMode.None;
    public DialogButton[] Buttons { get; set; } = [];
}

public class PzDialogManager
{
    public static Task<DialogResult> Confirm(string message, string title)
    {
        var opt = ConfirmOptions(message, title);
        return ShowDialog(opt);
    }
    public static Task<DialogResult> Alert(string message, string title)
    {
        var opt = AlertOptions(message, title);
        return ShowDialog(opt);
    }
    public static async Task<T?> ShowDialog<T>(ResultDialogOptions<T> options)
    {
        var semiOpts = new OverlayDialogOptions()
        {
            FullScreen = false,
            HorizontalAnchor = HorizontalPosition.Center,
            VerticalAnchor = VerticalPosition.Center,
            Mode = DialogMode.None,
            CanLightDismiss = false,
            CanDragMove = true,
            IsCloseButtonVisible = false,
            CanResize = false
        };

        var dialog = new PzDialog<T>(options);
        return await OverlayDialog.ShowCustomModal<T>(dialog, dialog, options: semiOpts);
    }

    public static ResultDialogOptions<DialogResult> ConfirmOptions(string title, string message)
    {
        return new ResultDialogOptions<DialogResult>(new PzMessageBoxContent(message))
        {
            Title = title,
            Mode = DialogMode.Info,
            Buttons = [
                new("OK", DialogResult.OK, true),
                new("Cancel", DialogResult.Cancel, true) { Styles = ["Tertiary"] },
            ]
        };
    }
    public static ResultDialogOptions<DialogResult> AlertOptions(string title, string message)
    {
        return new ResultDialogOptions<DialogResult>(new PzMessageBoxContent(message))
        {
            Title = title,
            Mode = DialogMode.Info,
            Buttons = [
                new("OK", DialogResult.OK, false)
            ]
        };
    }
    public static ResultDialogOptions<T> ResultOptions<T>(string title, DialogContentBase<T> content)
    {
        return new ResultDialogOptions<T>(content)
        {
            Title = title,
            Buttons = [
                new("OK", DialogResult.OK, true),
                new("Cancel", DialogResult.Cancel, false) { Styles = ["Tertiary"] },
            ],
        };
    }

    public static bool IsSureResult(DialogResult res)
    {
        return res == DialogResult.OK || res == DialogResult.Yes;
    }
}

