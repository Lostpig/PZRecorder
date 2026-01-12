using Avalonia.Media;
using Avalonia.Styling;
using Microsoft.Extensions.DependencyInjection;
using PZRecorder.Core.Managers;
using PZRecorder.Desktop.Extensions;
using PZRecorder.Desktop.Modules.Shared;
using Ursa.Controls;

namespace PZRecorder.Desktop.Modules.Daily;

using TbDaily = PZRecorder.Core.Tables.Daily;

internal class StatisticsDialog : DialogContentBase<TbDaily>
{
    private Border BuildYearBar()
    {
        return new Border()
            .Theme(StaticResource<ControlTheme>("CardBorder"))
            .Child(
                HStackPanel(Aligns.HCenter, Aligns.VCenter)
                .Spacing(8)
                .Children(
                    IconButton(MIcon.ChevronLeft).OnClick(_ => ChangeYear(-1)),
                    PzText(() => $"{Year}"),
                    IconButton(MIcon.ChevronRight).OnClick(_ => ChangeYear(1))
                )
            );
    }
    protected override Control Build()
    {
        return PzGrid(rows: "auto, auto, auto, 70, auto, auto")
            .RowSpacing(8)
            .Children(
                VStackPanel(Aligns.Left).Row(0)
                    .Children(
                        PzText(() => Daily.Name)
                            .Theme(StaticResource<ControlTheme>("TitleTextBlock"))
                            .Classes("H4"),
                        PzText(() => Daily.Alias).Classes("Tertiary")
                    ),
                VStackPanel(Aligns.Left).Row(1)
                    .Children(
                        HStackPanel().Spacing(4).Children(
                            PzText("First Date: "),
                            PzText(() => FirstDate.ToString("yyyy-MM-dd")).Foreground(TextColor)
                        ),
                        HStackPanel().Spacing(4).Children(
                            PzText("Latest Date: "),
                            PzText(() => LatestDate.ToString("yyyy-MM-dd")).Foreground(TextColor)
                        )
                    ),
                HStackPanel(Aligns.Left).Row(2)
                    .Spacing(4)
                    .Children(
                        PzText("Complete percent: "),
                        PzText(() => $"{PercentText}%").Foreground(TextColor),
                        PzText(() => $"({CompleteDays} / {TotalDays})").Foreground(TextColor)
                    ),
                BuildYearBar().Row(3),
                HStackPanel(Aligns.Left).Row(4)
                    .Spacing(4)
                    .Children(
                        PzText(() => $"In year {Year} complete percent:"),
                        PzText(() => $"{PercentTextYear}%").Foreground(TextColor),
                        PzText(() => $"({CompleteOfYear} / {TotalOfYear})").Foreground(TextColor)
                    ),
                DataPanel.Row(5)
            );
    }

    private int Year { get; set; }
    private TbDaily Daily { get; set; }
    private DateOnly FirstDate { get; set; }
    private DateOnly LatestDate { get; set; }
    private int TotalDays { get; set; } = 0;
    private int CompleteDays { get; set; } = 0;
    private int CompleteOfYear { get; set; } = 0;
    private int TotalOfYear { get; set; } = 0;

    private string PercentText => TotalDays > 0 ? ((double)CompleteDays / TotalDays * 100).ToString("f1") : "0.0";
    private string PercentTextYear => TotalOfYear > 0 ? ((double)CompleteOfYear / TotalOfYear * 100).ToString("f1") : "0.0";
    private IBrush EmptyColor {  get; init; }
    private IBrush CompleteColor { get; init; }
    private IBrush GiveupColor { get; init; }
    private IBrush TextColor { get; init; }

    private StackPanel DataPanel { get; set; }

    private DailyManager _manager;
    private readonly int[][] _days;

    public StatisticsDialog(TbDaily daily) : base()
    {
        _manager = ServiceProvider.GetRequiredService<DailyManager>();
        Width = 560;
        Title = "Statistics";

        Year = DateTime.Now.Year;
        Daily = daily;

        EmptyColor = StaticColor("SemiColorTertiaryLight");
        CompleteColor = StaticColor("SemiColorSuccess");
        GiveupColor = StaticColor("SemiColorDanger");
        TextColor = StaticColor("SemiColorInformation");

        _days = InitDays();
        DataPanel = InitDataPanel();

        InitSumData();
    }
    protected override IEnumerable<IDisposable> WhenActivate()
    {
        UpdateYearData();
        return base.WhenActivate();
    }

    private static readonly string[] MonthText = ["Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"];
    private static int GetMonthDays(int month)
    {
        return month switch
        {
            1 or 3 or 5 or 7 or 8 or 10 or 12 => 31,
            2 => 29,
            _ => 30
        };
    }
    private static int[][] InitDays()
    {
        var days = new int[12][];
        for (int month = 0; month < 12; month++)
        {
            var dayCount = GetMonthDays(month + 1);
            var monthDays = new int[dayCount];
            Array.Fill(monthDays, 0);

            days[month] = monthDays;
        }

        return days;
    }
    private StackPanel InitDataPanel()
    {
        var p = VStackPanel().Spacing(4);
        for (int m = 0; m < _days.Length; m++)
        {
            var month = _days[m];
            var monthTitle = PzText(MonthText[m]).FontSize(12).Col(0);
            var monthPanel = HStackPanel().Col(1).Spacing(4);

            for (int d = 0; d < month.Length; d++)
            {
                monthPanel.Children.Add(
                    new Border() { Width = 12, Height = 12 }
                        .CornerRadius(2)
                        .Background(EmptyColor)
                        .ToolTip($"{m + 1}-{d + 1}")
                );
            }

            var monthGrid = PzGrid(cols: "50, *").Children(
                monthTitle,
                monthPanel
            );
            p.Children.Add(monthGrid);
        }

        return p;
    }
    private void InitSumData()
    {
        var datas = _manager.GetDailyDatas(Daily.Id);
        var weekdays = Enum.GetValues<DayOfWeek>();

        int totalDays = 0;
        int completeDays = 0;
        int firstDay = 0;
        int latestDay = 0;

        foreach (var dw in datas)
        {
            foreach (var day in weekdays)
            {
                int d = (int)day;
                d = d == 0 ? 7 : (d - 1);

                if (dw[day] != 0)
                {
                    totalDays++;
                    var dayNum = dw.MondayDay + d;
                    if (firstDay <= 0 || dayNum < firstDay) firstDay = dayNum;
                    if (latestDay <= 0 || dayNum > latestDay) latestDay = dayNum;
                }
                if (dw[day] == 1) completeDays++;
            }
        }

        TotalDays = totalDays;
        CompleteDays = completeDays;
        FirstDate = DateOnly.FromDayNumber(firstDay);
        LatestDate = DateOnly.FromDayNumber(latestDay);
    }

    private void UpdateYearData()
    {
        var datas = _manager.GetDailyDatas(Daily.Id, Year);

        for (int month = 0; month < 12; month++)
        {
            Array.Fill(_days[month], 0);
        }

        int totalDays = 0;
        int completeDays = 0;
        foreach (var dw in datas)
        {
            for (int i = 0; i < 7; i++)
            {
                var day = DateOnly.FromDayNumber(dw.MondayDay + i);
                if (day.Year != Year) continue;

                var value = dw[day.DayOfWeek];
                _days[day.Month - 1][day.Day - 1] = value;
                if (value != 0) totalDays++;
                if (value == 1) completeDays++;
            }
        }
        RenderYearData();

        TotalOfYear = totalDays;
        CompleteOfYear = completeDays;
        UpdateState();
    }
    private void RenderYearData()
    {
        var isLeapYear = DateTime.IsLeapYear(Year);

        for (int i = 0; i < DataPanel.Children.Count; i++)
        {
            if (DataPanel.Children[i] is not Grid monthGrid) throw new Exception("DataPanles not initialized!");
            var monthPanel = (monthGrid.Children[1] as StackPanel)!;

            for (int d = 0; d < monthPanel.Children.Count; d++)
            {
                if (monthPanel.Children[d] is Border b)
                {
                    var value = _days[i][d];
                    var color = value switch
                    {
                        1 => CompleteColor,
                        2 => GiveupColor,
                        _ => EmptyColor,
                    };

                    b.Background(color);

                    if (d == 28 && i == 1) // 2-29
                    {
                        b.IsVisible = isLeapYear;
                    }
                }
            }
        }
    }
    private void ChangeYear(int move)
    {
        Year += move;
        UpdateYearData();
    }

    public override Shared.DialogButton[] Buttons()
    {
        return
        [
            new Shared.DialogButton("Close", DialogResult.None)
        ];
    }
    public override bool Check(DialogResult buttonValue)
    {
        return true;
    }
    public override PzDialogResult<TbDaily> GetResult(DialogResult buttonValue)
    {
        return new(Daily, buttonValue);
    }
}
