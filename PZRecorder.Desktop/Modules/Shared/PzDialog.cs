using Avalonia.Media;
using Avalonia.Styling;
using Irihi.Avalonia.Shared.Contracts;
using PZ.RxAvalonia.DataValidations;
using PZRecorder.Desktop.Extensions;
using Ursa.Controls;

namespace PZRecorder.Desktop.Modules.Shared;

public abstract class DialogContentBase<T> : MvuComponent
{
    public string Title { get; set; } = "";
    public DialogMode Mode { get; set; } = DialogMode.None;
    public abstract PzDialogResult<T> GetResult(DialogResult buttonValue);
    public abstract bool Check(DialogResult buttonValue);
    public abstract DialogButton[] Buttons();
    protected DialogContentBase(ViewInitializationStrategy strategy = ViewInitializationStrategy.Lazy) : base(strategy) { }
}
public record PzDialogResult<T>(T Value, DialogResult Result);

public class PzDialog<T> : ComponentBase, IDialogContext
{
    private readonly DialogContentBase<T> _content;
    public PzDialog(DialogContentBase<T> content) : base(ViewInitializationStrategy.Lazy)
    {
        _content = content;
        Initialize();
    }
    protected override Control Build()
    {
        return PzGrid(rows: "auto, *, auto")
            .Margin(24)
            .RowSpacing(16)
            .Children(
                BuildHeader().Row(0),
                _content.Row(1),
                BuildButtons().Row(2)
            );
    }

    protected StackPanel BuildHeader()
    {
        var modeIcon = _content.Mode switch
        {
            DialogMode.Info => MaterialIcon(MIcon.Info).Foreground(StaticColor("SemiColorInformation")),
            DialogMode.Warning => MaterialIcon(MIcon.AlertCircle).Foreground(StaticColor("SemiColorWarning")),
            DialogMode.Error => MaterialIcon(MIcon.Alert).Foreground(StaticColor("SemiColorDanger")),
            DialogMode.Success => MaterialIcon(MIcon.CheckCircle).Foreground(StaticColor("SemiColorSuccess")),
            DialogMode.Question => MaterialIcon(MIcon.HelpCircle).Foreground(StaticColor("SemiColorPrimary")),
            _ => MaterialIcon(MIcon.Dialogue).Foreground(StaticColor("SemiColorPrimary"))
        };

        return HStackPanel(Aligns.Left)
            .Spacing(8)
            .Children(
                modeIcon,
                PzText(_content.Title, "H5")
                    .Theme(StaticResource<ControlTheme>("TitleTextBlock"))
            );
    }
    protected StackPanel BuildButtons()
    {
        ComponentValidation? Validation = null;
        if (_content.IsRegisterDataValidation)
        {
            Validation = ComponentValidation.Get(_content);
        }

        var buttonsPanel = HStackPanel(Aligns.Right).Spacing(8);

        foreach (var btn in _content.Buttons())
        {
            var button = PzButton(btn.Text, btn.Styles ?? [])
                .OnClick(_ => TryClose(btn));
            if (Validation != null && btn.Validation)
            {
                button.IsEnabled(Validation.IsValidObservable);
            }
            buttonsPanel.Children(button);
        }

        return buttonsPanel;
    }

    private void TryClose(DialogButton btn)
    {
        if (btn.Validation && _content.Check(btn.Value))
        {
            RequestClose?.Invoke(this, _content.GetResult(btn.Value));
        }
        else
        {
            RequestClose?.Invoke(this, _content.GetResult(btn.Value));
        }
    }
    public event EventHandler<object?>? RequestClose;
    public void Close()
    {
        
    }
}

public class DialogButton(string text, DialogResult value)
{
    public string Text { get; set; } = text;
    public bool Validation { get; set; } = false;
    public string[] Styles { get; set; } = [];
    public DialogResult Value { get; set; } = value;
}
public sealed class PzMessageBoxContent : DialogContentBase<int>
{
    private readonly string _message;
    public DialogButton[] BoxButtons;
    public PzMessageBoxContent(string message) : base()
    {
        _message = message;
        MinWidth = 300;
        MaxWidth = 600;

        BoxButtons = [
            new ("OK", DialogResult.OK),
        ];

        Initialize();
    }

    protected override Control Build()
    {
        return PzText(_message)
            .TextWrapping(TextWrapping.Wrap)
            .Margin(8);
    }
    public override DialogButton[] Buttons() => BoxButtons;

    public override PzDialogResult<int> GetResult(DialogResult buttonValue)
    {
        return new(0, buttonValue);
    }
    override public bool Check(DialogResult buttonValue)
    {
        return true;
    }
}

public static class PzDialogManager
{
    public static Task<PzDialogResult<int>> Confirm(string message, string title)
    {
        var opt = ConfirmDialog(message, title);
        return ShowDialog(opt);
    }
    public static Task<PzDialogResult<int>> Alert(string message, string title)
    {
        var opt = AlertDialog(message, title);
        return ShowDialog(opt);
    }
    public static async Task<PzDialogResult<T>> ShowDialog<T>(DialogContentBase<T> content)
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

        var dialog = new PzDialog<T>(content);
        var result = await OverlayDialog.ShowCustomModal<PzDialogResult<T>>(dialog, dialog, options: semiOpts);

        return result!;
    }

    public static PzMessageBoxContent ConfirmDialog(string title, string message)
    {
        return new PzMessageBoxContent(message)
        {
            Title = title,
            Mode = DialogMode.Info,
            BoxButtons = [
                new("OK", DialogResult.OK),
                new("Cancel", DialogResult.Cancel) { Styles = ["Tertiary"] },
            ]
        };
    }
    public static PzMessageBoxContent AlertDialog(string title, string message)
    {
        return new PzMessageBoxContent(message)
        {
            Title = title,
            Mode = DialogMode.Info,
        };
    }

    public static bool IsSureResult(DialogResult res)
    {
        return res == DialogResult.OK || res == DialogResult.Yes;
    }
}

