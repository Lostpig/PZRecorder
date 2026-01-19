using Avalonia.Media;
using PZRecorder.Core.Managers;
using PZRecorder.Desktop.Common;
using PZRecorder.Desktop.Extensions;
using PZRecorder.Desktop.Modules.Shared;

namespace PZRecorder.Desktop.Modules.ClockIn;
using TbClockIn = PZRecorder.Core.Tables.ClockIn;

internal class ClockInPage(ClockInManager _manager, BroadcastManager _broadcast) : MvuPage()
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
                PzGrid(cols: "100, 150, 1*, 240")
                .Dock(Dock.Top)
                .Classes("ListRowHeader")
                .Children(
                    PzText(() => LD.OrderBy).Col(0).TextAlignment(TextAlignment.Left),
                    PzText(() => LD.Name).Col(1),
                    PzText(() => LD.State).Col(2),
                    PzText(() => LD.Action).Col(3).Align(Aligns.HCenter)
                ),
                new ScrollViewer()
                .Dock(Dock.Bottom)
                .Content(
                    new ItemsControl()
                    .ItemsPanel(VStackPanel(Aligns.HStretch).Spacing(5))
                    .ItemsSource(() => Items)
                    .ItemTemplate<ClockInCollection, ItemsControl>(ListItemTemplate)
                )
            );
    }
    private static Control[] GetStatusText(ClockInCollection model)
    {
        if (model.LastRecord != null)
        {
            var days = model.GetLastDaySince(DateTime.Now);
            if (days == 0)
            {
                return [PzText(LD.ClockInTodayText).Classes("Success")];
            }
            else
            {
                bool remind = model.CheckRemind(DateTime.Now);
                if (remind)
                {
                    var p1 = FormatTextBlock(LD.ClockInDiffText, new Common.TextSegment($"{days}", ["Warning"]));
                    var p2 = FormatTextBlock(LD.ClockInRemindText, new Common.TextSegment($"{days - model.ClockIn.RemindDays}", ["Warning"]));


                    return [
                            ..p1,
                            PzText("("),
                            ..p2,
                            PzText(")")
                        ];
                }
                else
                {
                    return [.. FormatTextBlock(LD.ClockInDiffText, new Common.TextSegment($"{days}", ["Success"]))];
                }
            }
        }
        else
        {
            return [PzText(LD.ClockInNoRecord).Classes("Secondary")];
        }
    }
    private Grid ListItemTemplate(ClockInCollection item)
    {
        var ci = item.ClockIn;

        return PzGrid(cols: "100, 150, 1*, 240")
            .Classes("ListRow")
            .Children(
                PzText(ci.OrderNo.ToString()).Col(0),
                PzText(ci.Name).Col(1),
                HStackPanel().Children(GetStatusText(item)).Col(2),
                HStackPanel(Aligns.HCenter).Col(3).Spacing(10).Children(
                        IconButton(MIcon.CalendarCheck, classes: "Success")
                            .OnClick(_ => OnCheckIn(item)),
                        IconButton(MIcon.ViewList, classes: "Warning")
                            .OnClick(_ => ShowRecords(item)),
                        IconButton(MIcon.Edit)
                            .OnClick(_ => OnEdit(item.ClockIn)),
                        IconButton(MIcon.Delete, classes: "Danger")
                            .OnClick(_ => OnDelete(item.ClockIn))
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

    private List<ClockInCollection> Items { get; set; } = [];

    protected override IEnumerable<IDisposable> WhenActivate()
    {
        UpdateItems();
        return base.WhenActivate();
    }

    private static int DaysDiff(DateTime d1, DateTime d2)
    {
        return DateOnly.FromDateTime(d1).DayNumber - DateOnly.FromDateTime(d2).DayNumber;
    }

    private void UpdateItems()
    {
        Items = _manager.GetCollections();
        UpdateState();
    }

    private void OnCheckIn(ClockInCollection collection)
    {
        if (collection.LastRecord != null)
        {
            var days = DaysDiff(DateTime.Now, collection.LastRecord.Time);
            if (days == 0)
            {
                Notification.Warning(LD.ClockInTodayText);
                return;
            }
        }
        _manager.AddRecord(collection.ClockIn.Id);
        UpdateItems();
    }
    private static async void ShowRecords(ClockInCollection collection)
    {
        _ = await PzDialogManager.ShowDialog(new RecordsDialog(collection));
    }
    private async void OnAdd()
    {
        var res = await PzDialogManager.ShowDialog(new ClockInDialog());
        if (PzDialogManager.IsSureResult(res.Result))
        {
            _manager.AddClockIn(res.Value);
            UpdateItems();
            _broadcast.Publish(BroadcastEvent.RemindStateChanged);
        }
    }
    private async void OnEdit(TbClockIn item)
    {
        var res = await PzDialogManager.ShowDialog(new ClockInDialog(item));
        if (PzDialogManager.IsSureResult(res.Result))
        {
            _manager.UpdateClockIn(res.Value);
            UpdateItems();
            _broadcast.Publish(BroadcastEvent.RemindStateChanged);
        }
    }
    private async void OnDelete(TbClockIn item)
    {
        var dialog = PzDialogManager.DeleteConfirmDialog();
        var delete = await PzDialogManager.ShowDialog(dialog);
        if (PzDialogManager.IsSureResult(delete.Result))
        {
            _manager.DeleteClockIn(item.Id);
            UpdateItems();
            _broadcast.Publish(BroadcastEvent.RemindStateChanged);
        }
    }
}
