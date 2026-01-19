using Avalonia.Styling;
using PZ.RxAvalonia.Reactive;
using PZRecorder.Core.Managers;
using PZRecorder.Core.Tables;
using PZRecorder.Desktop.Common;
using PZRecorder.Desktop.Extensions;
using PZRecorder.Desktop.Modules.Shared;

namespace PZRecorder.Desktop.Modules.Monitor;

internal class RecordsDialog : DialogContentBase<ProcessWatch>
{
    private ProcessWatch Model { get; init; }
    private ReactiveList<ProcessRecord> Items { get; init; }
    private readonly ProcessMonitorManager _manager;
    private DateTime SelectedDate;

    private TimeSpan TimePerDay => ActiveDays > 0 ? TotalTime / ActiveDays : TimeSpan.Zero;
    private TimeSpan TotalTime = TimeSpan.Zero;
    private int ActiveDays = 0;

    public RecordsDialog(ProcessWatch model, ProcessMonitorManager manager) : base(ViewInitializationStrategy.Lazy)
    {
        Model = model;
        _manager = manager;
        Items = [];

        Title = LD.ProcessRecords;
        Height = 500;
        Width = 560;

        UpdateDate(new(DateTime.Today.Year, DateTime.Today.Month, 1));
        Initialize();
    }

    protected override StyleGroup? BuildStyles() => 
    [
        .. Shared.Styles.ListStyles(),
        new Style<TextBlock>(s => s.Class("Infomation")).Foreground(StaticColor("SemiColorInformation"))
    ];
    private Border BuildMonthBar()
    {
        return new Border()
            .Theme(StaticResource<ControlTheme>("CardBorder"))
            .Child(
                PzGrid(cols: "auto, *, auto").Children(
                    IconButton(MIcon.ChevronLeft).Col(0).OnClick(_ => ChangeMonth(-1)),
                    new DatePicker().Col(1)
                        .Align(Aligns.HCenter)
                        .DayVisible(false)
                        .SelectedDate(() => SelectedDate)
                        .OnSelectedDateChanged(e =>
                        {
                            if (e.NewDate.HasValue) UpdateDate(e.NewDate.Value.DateTime);
                        }),
                    IconButton(MIcon.ChevronRight).Col(2).OnClick(_ => ChangeMonth(-1))
                )
            );
    }
    private StackPanel BuildInfoPanel()
    {
        return VStackPanel().Spacing(8).Children(
                PzText(Model.Name, "H4")
                    .Theme(StaticResource<ControlTheme>("TitleTextBlock")),
                HStackPanel().Children(
                    [..FormatTextBlock(LD.RecordCount,
                        new(() => $"{Items.Count}", ["Infomation"]),
                        new(() => $"{Utility.FormatDuration(TotalTime)}", ["Infomation"]),
                        new(() => $"{ActiveDays}", ["Infomation"]),
                        new(() => $"{Utility.FormatDuration(TimePerDay)}", ["Infomation"])
                    )]
                )
            );
    }
    protected override Control Build()
    {
        return PzGrid(rows: "60, 80, auto, *").Children(
                BuildMonthBar().Row(0),
                BuildInfoPanel().Row(1),
                PzGrid(cols: "1*, 100, 100, 90")
                    .Row(2)
                    .Classes("ListRowHeader")
                    .Children(
                        PzText(LD.Date).Col(0),
                        PzText(LD.StartTime).Col(1),
                        PzText(LD.EndTime).Col(2),
                        PzText(LD.Duration).Align(Aligns.Right).Col(3)
                    ),
                new CachedList<RecordRow, ProcessRecord>(Items)
                    .Row(3)
                    .ItemCreator(() => new RecordRow())
            );
    }

    private void ChangeMonth(int move)
    {
        var newDate = SelectedDate.AddMonths(move);
        UpdateDate(newDate);
    }
    private void UpdateDate(DateTime newDate)
    {
        SelectedDate = newDate;
        var firstDay = new DateOnly(SelectedDate.Year, SelectedDate.Month, 1);
        var latestDay = firstDay.AddMonths(1).AddDays(-1);

        var items = _manager.GetRecords(Model.Id, firstDay.DayNumber, latestDay.DayNumber);
        var totalTime = TimeSpan.Zero;
        HashSet<int> days = new();
        foreach (var p in items)
        {
            totalTime += p.Duration;
            days.Add(p.Date);
        }

        TotalTime = totalTime;
        ActiveDays = days.Count;
        Items.ReplaceAll(items);

        UpdateState();
    }

    public override DialogButton[] Buttons()
    {
        return [
                new DialogButton(LD.Close, Uc.DialogResult.None)
            ];
    }
    public override bool Check(Uc.DialogResult buttonValue) => true;
    public override PzDialogResult<ProcessWatch> GetResult(Uc.DialogResult buttonValue)
    {
        return new PzDialogResult<ProcessWatch>(Model, buttonValue);
    }
}

internal class RecordRow : ComponentBase, IListItemComponent<ProcessRecord>
{
    private string Date { get; set; } = "";
    private string StartTime { get; set; } = "";
    private string EndTime { get; set; } = "";
    private string Duration { get; set; } = "";

    protected override Control Build()
    {
        return PzGrid(cols: "1*, 100, 100, 90")
            .Classes("ListRow")
            .Children(
                PzText(() => Date).Col(0),
                PzText(() => StartTime).Col(1),
                PzText(() => EndTime).Col(2),
                PzText(() => Duration).Align(Aligns.Right).Col(3)
            );
    }

    public void UpdateItem(ProcessRecord item)
    {
        Date = DateOnly.FromDayNumber(item.Date).ToString("yyyy-MM-dd");
        StartTime = Utility.FormatTimeSpan(TimeSpan.FromSeconds(item.StartTime));
        EndTime = Utility.FormatTimeSpan(TimeSpan.FromSeconds(item.EndTime));
        Duration = Utility.FormatDuration(item.Duration);

        UpdateState();
    }
}