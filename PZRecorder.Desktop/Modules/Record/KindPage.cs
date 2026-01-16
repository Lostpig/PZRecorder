using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using PZRecorder.Core.Managers;
using PZRecorder.Core.Tables;
using PZRecorder.Desktop.Extensions;
using PZRecorder.Desktop.Modules.Shared;

namespace PZRecorder.Desktop.Modules.Record;

internal sealed class KindPage(RecordManager manager) : MvuPage()
{
    protected override StyleGroup? BuildStyles() => Shared.Styles.ListStyles();

    private StackPanel BuildOperatorBar()
    {
        return HStackPanel()
            .Align(Aligns.HStretch)
            .Spacing(10)
            .Children(
                IconButton(MIcon.Add, () => LD.Add)
                    .OnClick(_ => OnAdd())
            );
    }
    private DockPanel BuildKindList()
    {
        return new DockPanel()
            .Children(
                PzGrid(cols: "100, *, 150")
                .Dock(Dock.Top)
                .Styles(new Style<TextBlock>().FontWeight(FontWeight.Bold).Margin(16, 0))
                .Children(
                    PzText(() => LD.OrderBy).Col(0).TextAlignment(TextAlignment.Left),
                    PzText(() => LD.Name).Col(1),
                    PzText(() => LD.Action).Col(2).Align(Aligns.HCenter)
                ),
                new ScrollViewer()
                .Dock(Dock.Bottom)
                .Margin(0, 8, 0 ,0)
                .Content(
                    new ItemsControl()
                    .ItemsPanel(VStackPanel(Aligns.HStretch).Spacing(5))
                    .ItemsSource(() => Kinds)
                    .ItemTemplate<Kind, ItemsControl>(KindItemTemplate)
                )
            );
    }
    private Grid KindItemTemplate(Kind kind)
    {
        return PzGrid(cols: "100, *, 150")
            .Classes("ListRow")
            .Children(
                PzText(kind.OrderNo.ToString()).Col(0),
                PzText(kind.Name).Col(1),
                HStackPanel(Aligns.HCenter).Col(2).Spacing(10).Children(
                        IconButton(MIcon.Edit)
                            .OnClick(_ => OnEdit(kind)),
                        IconButton(MIcon.Delete, classes: "Danger")
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

    private readonly RecordManager _manager = manager;
    private List<Kind> Kinds { get; set; } = new();

    protected override IEnumerable<IDisposable> WhenActivate()
    {
        UpdateKinds();
        return base.WhenActivate();
    }

    private void UpdateKinds()
    {
        Kinds = _manager.GetKinds();
        UpdateState();
    }
    private async void OnAdd()
    {
        var res = await PzDialogManager.ShowDialog(new KindDialog());
        if (PzDialogManager.IsSureResult(res.Result))
        {
            _manager.InsertKind(res.Value);
            UpdateKinds();
        }
    }
    private async void OnEdit(Kind kind)
    {
        var res = await PzDialogManager.ShowDialog(new KindDialog(kind));
        if (PzDialogManager.IsSureResult(res.Result))
        {
            _manager.UpdateKind(res.Value);
            UpdateKinds();
        }
    }
    private async void OnDelete(Kind kind)
    {
        var dialog = PzDialogManager.DeleteConfirmDialog();
        var delete = await PzDialogManager.ShowDialog(dialog);
        if (PzDialogManager.IsSureResult(delete.Result))
        {
            _manager.DeleteKind(kind.Id);
            UpdateKinds();
        }
    }
}
