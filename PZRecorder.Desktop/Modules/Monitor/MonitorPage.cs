using Avalonia.Media;
using PZ.RxAvalonia.Reactive;
using PZRecorder.Core.Managers;
using PZRecorder.Core.Tables;
using PZRecorder.Desktop.Common;
using PZRecorder.Desktop.Extensions;
using PZRecorder.Desktop.Modules.Shared;
using System.Reactive.Linq;

namespace PZRecorder.Desktop.Modules.Monitor;

internal class MonitorPage(ProcessMonitorManager _manager, ProcessMonitorService _service, BroadcastManager _broadcast) : MvuPage()
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
                PzGrid(cols: "100, 1*, 240, 80, 240")
                    .Dock(Dock.Top)
                    .Classes("ListRowHeader")
                    .Children(
                        PzText(() => LD.OrderBy).Col(0).TextAlignment(TextAlignment.Left),
                        PzText(() => LD.Name).Col(1),
                        PzText(() => LD.State).Col(2),
                        PzText(() => LD.Enabled).Col(3).Align(Aligns.HCenter),
                        PzText(() => LD.Action).Col(4).Align(Aligns.HCenter)
                    ),
                new CachedList<MonitorItem, ProcessWatchWithState>(Items)
                    .Dock(Dock.Bottom)
                    .ItemCreator(() => new MonitorItem(this))
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
    protected override IEnumerable<IDisposable> WhenActivate()
    {
        UpdateItems();
        return [
            _broadcast.ProcessChanged.Subscribe(OnMonitorProcessChanged),
            _broadcast.Broadcast.Where(e => e == BroadcastEvent.TimerTick).Subscribe(_ => OnTimerTick()),
        ];
    }

    private ReactiveList<ProcessWatchWithState> Items { get; set; } = [];

    private void OnTimerTick()
    {
        if (Items.Any(x => x.IsRunning))
        {
            Items.ForceNext(ChangedType.ReplaceAll, 0, Items.Count);
        }
    }
    private void OnMonitorProcessChanged(ProcessChangedArgs e)
    {
        var p = (Items as IList<ProcessWatchWithState>).FirstOrDefault(p => p.Watch.Id == e.Watch.Id);
        if (p != null)
        {
            p.IsRunning = e.IsRunning;
            p.StartTime = e.IsRunning ? e.StartTime : null;
        }
    }
    private void UpdateItems()
    {
        var items = _manager.GetAllWatches();
        var monitors = _service.GetRunningMonitor();

        List<ProcessWatchWithState> list = new(items.Count);
        foreach (var item in items)
        {
            var n = new ProcessWatchWithState(item);
            var m = monitors.Find(x => x.Watch.Id == n.Watch.Id);
            if (m != null)
            {
                n.IsRunning = m.Runing;
                n.StartTime = m.Runing ? m.StartTime : null;
            }
            list.Add(n);
        }

        Items.ReplaceAll(list);
    }

    private async void OnAdd()
    {
        var res = await PzDialogManager.ShowDialog(new MonitorDialog());
        if (PzDialogManager.IsSureResult(res.Result))
        {
            _manager.InsertWatch(res.Value);
            _service.UpdateWatches();
            UpdateItems();
        }
    }
    public async void OnEdit(ProcessWatch item)
    {
        var res = await PzDialogManager.ShowDialog(new MonitorDialog(item));
        if (PzDialogManager.IsSureResult(res.Result))
        {
            _manager.UpdateWatch(res.Value);
            _service.UpdateWatches();
            UpdateItems();
        }
    }
    public async void OnDelete(ProcessWatch item)
    {
        var dialog = PzDialogManager.DeleteConfirmDialog();
        var delete = await PzDialogManager.ShowDialog(dialog);
        if (PzDialogManager.IsSureResult(delete.Result))
        {
            _manager.DeleteWatch(item.Id);
            _service.UpdateWatches();
            UpdateItems();
        }
    }
    public async void ShowRecords(ProcessWatch item)
    {
        _ = await PzDialogManager.ShowDialog(new RecordsDialog(item, _manager));
    }
}


public class ProcessWatchWithState(ProcessWatch watch)
{
    public ProcessWatch Watch { get; init; } = watch;
    public bool IsRunning { get; set; } = false;
    public DateTime? StartTime { get; set; } = null;

    public string RuningText => StartTime == null ? "-" : Utility.FormatDuration(DateTime.Now - StartTime.Value);
}
internal class MonitorItem(MonitorPage _parentPage) : MvuComponent, IListItemComponent<ProcessWatchWithState>
{
    private ProcessWatchWithState Model { get; set; } = new(new());

    public void UpdateItem(ProcessWatchWithState item)
    {
        Model = item;
        UpdateState();
    }

    protected override Control Build()
    {
        return PzGrid(cols: "100, 1*, 240, 80, 240")
            .Classes("ListRow")
            .Children(
                PzText(() => $"{Model.Watch.OrderNo}").Col(0),
                PzText(() => Model.Watch.Name).Col(1),
                PzText(() => Model.RuningText).BindClass(() => Model.IsRunning, "Success").Col(2),
                MaterialIcon(() => Model.Watch.Enabled ? MIcon.CheckCircle : MIcon.MinusCircle).Col(3)
                    .Align(Aligns.HCenter)
                    .Foreground(() => Model.Watch.Enabled ? StaticColor("SemiColorSuccessActive") : StaticColor("SemiColorDangerActive")),
                HStackPanel(Aligns.HCenter).Col(4).Spacing(10).Children(
                        IconButton(MIcon.ViewList, classes: "Warning")
                            .OnClick(_ => _parentPage.ShowRecords(Model.Watch)),
                        IconButton(MIcon.Edit)
                            .OnClick(_ => _parentPage.OnEdit(Model.Watch)),
                        IconButton(MIcon.Delete, classes: "Danger")
                            .OnClick(_ => _parentPage.OnDelete(Model.Watch))
                    )
            );
    }
}
