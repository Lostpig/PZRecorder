using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;
using Microsoft.Extensions.DependencyInjection;
using PZRecorder.Core.Managers;
using PZRecorder.Core.Tables;
using PZRecorder.Desktop.Common;
using System.Reactive.Subjects;

namespace PZRecorder.Desktop.Record;

internal class KindPage : PZPageBase
{
    protected override StyleGroup? BuildStyles()
    {
        return [
                new StyleGroup(s => s.Class("KindRow")) 
                {
                    new Style<Grid>().Setter(Grid.BackgroundProperty, Brushes.Transparent),
                    new Style<Grid>(s => s.Class(":pointerover"))
                        .SetterEx(Grid.BackgroundProperty, DynamicColor("SemiColorFill0")),
                },
                new Style<Grid>(s => s.Class("KindRow").Child()).Margin(16, 0),
                new Style<TextBlock>().VerticalAlignment(VerticalAlignment.Center),
                new Style<Button>().Theme(StaticResource<ControlTheme>("BorderlessButton"))
            ];
    }

    private StackPanel BuildOperatorBar()
    {
        return HStackPanel()
            .Align(Aligns.HStretch)
            .Spacing(10)
            .Children(
                IconButton(MIcon.Add, "Add")
                    .OnClick(_ => OnAdd())
            );
    }
    private DockPanel BuildKindList()
    {
        return new DockPanel()
            .Grid_IsSharedSizeScope(true)
            .Children(
                PzGrid(cols: "60, 1*, auto")
                .Dock(Dock.Top)
                .ColSharedGroup(2, "OperatorsCol")
                .Styles(new Style<TextBlock>().FontWeight(FontWeight.Bold).Margin(16, 0))
                .Children(
                    PzText("Order").Col(0).TextAlignment(TextAlignment.Left),
                    PzText("Name").Col(1),
                    PzText("Operators").Align(Aligns.HCenter).Col(2)
                ),
                new ScrollViewer()
                .Dock(Dock.Bottom)
                .Margin(0, 8, 0 ,0)
                .Content(
                    new ItemsControl()
                    .ItemsPanel(VStackPanel(Aligns.HStretch).Spacing(5))
                    .ItemsSource(Kinds)
                    .ItemTemplate<Kind, ItemsControl>(KindItemTemplate)
                )
            );
    }
    private Grid KindItemTemplate(Kind kind)
    {
        return PzGrid(cols: "60, 1*, auto")
            .Classes("KindRow")
            .ColSharedGroup(2, "OperatorsCol")
            .Children(
                PzText(kind.OrderNo.ToString()).Col(0),
                PzText(kind.Name).Col(1),
                HStackPanel(Aligns.Right).Col(2).Spacing(10).Children(
                        IconButton(MIcon.Edit, "Edit")
                            .OnClick(_ => OnEdit(kind)),
                        IconButton(MIcon.Delete, "Delete", "Danger")
                            .OnClick(_ => OnDelete(kind))
                    )
            );
    }
    protected override Control Build() =>
        PzGrid(rows: "40, *")
            .Margin(8)
            .RowSpacing(8)
            .Children(
                BuildOperatorBar().Row(0),
                BuildKindList()
                    .Row(1)
                    .Align(Aligns.VStretch)
            );

    private readonly RecordManager _manager;
    private Subject<List<Kind>> Kinds { get; init; } = new();
    public KindPage() : base(ViewInitializationStrategy.Lazy)
    {
        _manager = ServiceProvider.GetRequiredService<RecordManager>();
        Initialize();
    }

    protected override IEnumerable<IDisposable> WhenActivate()
    {
        Kinds.OnNext(_manager.GetKinds());
        return base.WhenActivate();
    }

    private void UpdateKinds()
    {
        Kinds.OnNext(_manager.GetKinds());
    }
    private async void OnAdd()
    {
        var res = await PzDialogManager.ShowDialog(new KindDialog());
        if (res != null)
        {
            _manager.InsertKind(res);
            UpdateKinds();
        }
    }
    private async void OnEdit(Kind kind)
    {
        var res = await PzDialogManager.ShowDialog(new KindDialog(kind));
        if (res != null)
        {
            _manager.UpdateKind(res);
            UpdateKinds();
        }
    }
    private async void OnDelete(Kind kind)
    {
        var dialog = PzDialogManager.ConfirmDialog("Delete", "Sure to delete?");
        dialog.Mode = Uc.DialogMode.Question;
        dialog.BoxButtons[0].Text = "Delete";
        dialog.BoxButtons[0].Styles = ["Danger"];

        var delete = await PzDialogManager.ShowDialog(dialog);
        if (PzDialogManager.IsSureResult(delete))
        {
            _manager.DeleteKind(kind.Id);
            UpdateKinds();
        }
    }
}
