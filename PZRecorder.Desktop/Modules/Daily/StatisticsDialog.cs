using Avalonia.Media;
using Avalonia.Styling;
using Microsoft.Extensions.DependencyInjection;
using PZRecorder.Core.Managers;
using PZRecorder.Desktop.Extensions;
using PZRecorder.Desktop.Modules.Shared;
using Ursa.Controls;

namespace PZRecorder.Desktop.Modules.Daily;

using TbDaily = PZRecorder.Core.Tables.Daily;

internal class StatisticsModel
{
    public int Year { get; set; }
    public DateOnly FirstDate { get; set; }
    public DateOnly LatestDate { get; set; }
    public int TotalDays { get; set; } = 0;
    public int CompleteDays { get; set; } = 0;
    public int CompleteOfYear { get; set; } = 0;
    public int TotalOfYear { get; set; } = 0;

    public string PercentText => TotalDays > 0 ? ((double)CompleteDays / TotalDays * 100).ToString("f1") : "0.0";
    public string PercentTextYear => TotalOfYear > 0 ? ((double)CompleteOfYear / TotalOfYear * 100).ToString("f1") : "0.0";

    public int[][] Days { get; init; } = InitDays();

    private static int[][] InitDays()
    {
        static int GetMonthDays(int month)
        {
            return month switch
            {
                1 or 3 or 5 or 7 or 8 or 10 or 12 => 31,
                2 => 29,
                _ => 30
            };
        }

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
}

internal class StatisticsDialog : DialogContentBase<TbDaily>
{
    static private StatisticsModel? _model;
    static private StatisticsModel Model => _model ??= new StatisticsModel();

    private Border BuildYearBar()
    {
        return new Border()
            .Theme(StaticResource<ControlTheme>("CardBorder"))
            .Child(
                HStackPanel(Aligns.HStretch, Aligns.VCenter)
                .Spacing(8)
                .Children(
                    IconButton(MIcon.ChevronLeft).OnClick(_ => ChangeYear(-1)),
                    PzText(() => $"{Model.Year}"),
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
                            PzText(() => Model.FirstDate.ToString("yyyy-MM-dd")).Foreground(TextColor)
                        ),
                        HStackPanel().Spacing(4).Children(
                            PzText("Latest Date: "),
                            PzText(() => Model.LatestDate.ToString("yyyy-MM-dd")).Foreground(TextColor)
                        )
                    ),
                HStackPanel(Aligns.Left).Row(2)
                    .Spacing(4)
                    .Children(
                        PzText("Complete percent: "),
                        PzText(() => $"{Model.PercentText}%").Foreground(TextColor),
                        PzText(() => $"({Model.CompleteDays} / {Model.TotalDays})").Foreground(TextColor)
                    ),
                BuildYearBar().Row(3),
                HStackPanel(Aligns.Left).Row(4)
                    .Spacing(4)
                    .Children(
                        PzText(() => $"In year {Model.Year} complete percent:"),
                        PzText(() => $"{Model.PercentTextYear}%").Foreground(TextColor),
                        PzText(() => $"({Model.CompleteOfYear} / {Model.TotalOfYear})").Foreground(TextColor)
                    ),
                DataPanel.Row(5)
            );
    }

    private TbDaily Daily { get; set; }
    private IBrush EmptyColor {  get; init; }
    private IBrush CompleteColor { get; init; }
    private IBrush GiveupColor { get; init; }
    private IBrush TextColor { get; init; }
    private StackPanel DataPanel { get; set; }

    private readonly DailyManager _manager;

    public StatisticsDialog(TbDaily daily) : base()
    {
        _manager = ServiceProvider.GetRequiredService<DailyManager>();
        Width = 560;
        Title = "Statistics";

        Model.Year = DateTime.Now.Year;
        Daily = daily;

        EmptyColor = StaticColor("SemiColorTertiaryLight");
        CompleteColor = StaticColor("SemiColorSuccess");
        GiveupColor = StaticColor("SemiColorDanger");
        TextColor = StaticColor("SemiColorInformation");

        DataPanel = InitDataPanel();
        InitSumData();
    }
    protected override IEnumerable<IDisposable> WhenActivate()
    {
        UpdateYearData();
        return base.WhenActivate();
    }
    private static readonly string[] MonthText = ["Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"];

    private StackPanel InitDataPanel()
    {
        var p = VStackPanel().Spacing(4);
        for (int m = 0; m < Model.Days.Length; m++)
        {
            var month = Model.Days[m];
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

        Model.TotalDays = totalDays;
        Model.CompleteDays = completeDays;
        Model.FirstDate = DateOnly.FromDayNumber(firstDay);
        Model.LatestDate = DateOnly.FromDayNumber(latestDay);
    }

    private void UpdateYearData()
    {
        var datas = _manager.GetDailyDatas(Daily.Id, Model.Year);

        for (int month = 0; month < 12; month++)
        {
            Array.Fill(Model.Days[month], 0);
        }

        int totalDays = 0;
        int completeDays = 0;
        foreach (var dw in datas)
        {
            for (int i = 0; i < 7; i++)
            {
                var day = DateOnly.FromDayNumber(dw.MondayDay + i);
                if (day.Year != Model.Year) continue;

                var value = dw[day.DayOfWeek];
                Model.Days[day.Month - 1][day.Day - 1] = value;
                if (value != 0) totalDays++;
                if (value == 1) completeDays++;
            }
        }
        RenderYearData();

        Model.TotalOfYear = totalDays;
        Model.CompleteOfYear = completeDays;
        UpdateState();
    }
    private void RenderYearData()
    {
        var isLeapYear = DateTime.IsLeapYear(Model.Year);

        for (int i = 0; i < DataPanel.Children.Count; i++)
        {
            if (DataPanel.Children[i] is not Grid monthGrid) throw new Exception("DataPanles not initialized!");
            var monthPanel = (monthGrid.Children[1] as StackPanel)!;

            for (int d = 0; d < monthPanel.Children.Count; d++)
            {
                if (monthPanel.Children[d] is Border b)
                {
                    var value = Model.Days[i][d];
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
        Model.Year += move;
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
