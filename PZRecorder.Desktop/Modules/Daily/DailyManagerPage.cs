using Avalonia.Media;
using PZRecorder.Core.Managers;
using PZRecorder.Desktop.Extensions;
using PZRecorder.Desktop.Modules.Shared;

namespace PZRecorder.Desktop.Modules.Daily;

using TbDaily = PZRecorder.Core.Tables.Daily;

internal sealed class DailyManagerPage(DailyManager manager) : MvuPage()
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
    private DockPanel BuildItemsList()
    {
        return new DockPanel()
            .Children(
                PzGrid(cols: "100, *, 100, 200")
                .Dock(Dock.Top)
                .Classes("ListRowHeader")
                .Children(
                    PzText(() => LD.OrderBy).Col(0).TextAlignment(TextAlignment.Left),
                    PzText(() => LD.Name).Col(1),
                    PzText(() => LD.State).Col(2).Align(Aligns.HCenter),
                    PzText(() => LD.Action).Col(3).Align(Aligns.HCenter)
                ),
                new ScrollViewer()
                .Dock(Dock.Bottom)
                .Content(
                    new ItemsControl()
                    .ItemsPanel(VStackPanel(Aligns.HStretch).Spacing(5))
                    .ItemsSource(() => Items)
                    .ItemTemplate<TbDaily, ItemsControl>(ListItemTemplate)
                )
            );
    }
    private Grid ListItemTemplate(TbDaily item)
    {
        var statusIcon = item.State == Core.Common.EnableState.Enabled ? MIcon.CheckCircle : MIcon.MinusCircle;
        var statusColor = item.State == Core.Common.EnableState.Enabled ? "SemiColorSuccessActive" : "SemiColorDangerActive";

        return PzGrid(cols: "100, *, 100, 200")
            .Classes("ListRow")
            .Children(
                PzText(item.OrderNo.ToString()).Col(0),
                PzText(item.Name).Col(1),
                MaterialIcon(statusIcon).Col(2)
                    .Align(Aligns.HCenter)
                    .Foreground(StaticColor(statusColor)),
                HStackPanel(Aligns.HCenter).Col(3).Spacing(10).Children(
                        IconButton(MIcon.ChartBar, classes: "Warning")
                            .OnClick(_ => ShowStatistics(item)),
                        IconButton(MIcon.Edit)
                            .OnClick(_ => OnEdit(item)),
                        IconButton(MIcon.Delete, classes: "Danger")
                            .OnClick(_ => OnDelete(item))
                    )
            );
    }
    protected override Control Build() =>
        PzGrid(rows: "40, *")
            .Margin(8)
            .RowSpacing(8)
            .Children(
                BuildOperatorBar().Row(0),
                BuildItemsList()
                    .Row(1)
                    .Align(Aligns.VStretch)
            );

    private readonly DailyManager _manager = manager;
    private List<TbDaily> Items { get; set; } = [];

    protected override IEnumerable<IDisposable> WhenActivate()
    {
        UpdateItems();
        return base.WhenActivate();
    }

    private void UpdateItems()
    {
        Items = _manager.GetDailies();
        UpdateState();
    }
    private async void OnAdd()
    {
        var res = await PzDialogManager.ShowDialog(new DailyDialog());
        if (PzDialogManager.IsSureResult(res.Result))
        {
            _manager.InsertDaily(res.Value);
            UpdateItems();
        }
    }
    private async void OnEdit(TbDaily item)
    {
        var res = await PzDialogManager.ShowDialog(new DailyDialog(item));
        if (PzDialogManager.IsSureResult(res.Result))
        {
            _manager.UpdateDaily(res.Value);
            UpdateItems();
        }
    }
    private async void OnDelete(TbDaily item)
    {
        var dialog = PzDialogManager.DeleteConfirmDialog();
        var delete = await PzDialogManager.ShowDialog(dialog);
        if (PzDialogManager.IsSureResult(delete.Result))
        {
            _manager.DeleteDaily(item.Id);
            UpdateItems();
        }
    }
    private async void ShowStatistics(TbDaily item)
    {
        _ = await PzDialogManager.ShowDialog(new StatisticsDialog(item));
    }
}
