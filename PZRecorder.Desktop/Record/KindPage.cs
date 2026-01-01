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
                        .SetterEx(Grid.BackgroundProperty, () => SemiHelper.GetColor("SemiColorTertiaryLightActive"))
                },
                new Style<TextBlock>().VerticalAlignment(VerticalAlignment.Center)
            ];
    }

    private StackPanel BuildOperatorBar()
    {
        return HStackPanel()
            .Align(UnionAlign.HStretch)
            .Spacing(10)
            .Children(
                IconButton(MIcon.Add, "Add", "Basic")
                    .OnClick(_ => OnAdd())
            );
    }
    private DockPanel BuildKindList()
    {
        return new DockPanel()
            .Grid_IsSharedSizeScope(true)
            .Children(
                PzGrid(cols: "60, 1*, auto")
                    .ColSharedGroup(2, "OperatorsCol")
                    .Dock(Dock.Top)
                    .Children(
                        PzText("Order").Col(0).TextAlignment(TextAlignment.Left),
                        PzText("Name").Col(1),
                        PzText("Operators").Align(UnionAlign.HCenter).Col(2)
                    ),
                new ScrollViewer()
                    .Dock(Dock.Bottom)
                    .Margin(0, 8, 0 ,0)
                    .Content(
                        new ItemsControl()
                            .ItemsPanel(
                                VStackPanel(HorizontalAlignment.Stretch)
                                    .Spacing(5)
                            )
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
                HStackPanel().Col(2).Spacing(10).Children(
                        IconButton(MIcon.Edit, "Edit", "Basic")
                            .OnClick(_ => OnEdit(kind)),
                        IconButton(MIcon.Delete, "Delete", "Basic", "Accent")
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
                    .Align(UnionAlign.VStretch)
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
        var opt = PzDialogManager.ResultOptions("Add", new KindDialog(new Kind()));
        opt.Buttons[0].Text = "Add";

        var res = await PzDialogManager.ShowDialog(opt);
        if (res != null)
        {
            _manager.InsertKind(res);
            UpdateKinds();
        }
    }
    private async void OnEdit(Kind kind)
    {
        var opt = PzDialogManager.ResultOptions("Edit", new KindDialog(kind));
        opt.Buttons[0].Text = "Save";

        var res = await PzDialogManager.ShowDialog(opt);
        if (res != null)
        {
            _manager.UpdateKind(res);
            UpdateKinds();
        }
    }
    private async void OnDelete(Kind kind)
    {
        var opt = PzDialogManager.ConfirmOptions("Delete", "Sure to delete?");
        opt.Mode = Uc.DialogMode.Question;
        opt.Buttons[0].Text = "Delete";
        opt.Buttons[0].Styles = ["Danger"];

        var delete = await PzDialogManager.ShowDialog(opt);

        if (PzDialogManager.IsSureResult(delete))
        {
            _manager.DeleteKind(kind.Id);
            UpdateKinds();
        }
    }
}
