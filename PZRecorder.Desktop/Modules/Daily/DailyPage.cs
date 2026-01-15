using Avalonia;
using Avalonia.Input;
using Avalonia.Styling;
using Material.Icons.Avalonia;
using PZ.RxAvalonia.Reactive;
using PZRecorder.Core.Common;
using PZRecorder.Core.Managers;
using PZRecorder.Core.Tables;
using PZRecorder.Desktop.Extensions;
using PZRecorder.Desktop.Modules.Shared;

namespace PZRecorder.Desktop.Modules.Daily;
using TbDaily = PZRecorder.Core.Tables.Daily;

internal record DailyWeekModel(TbDaily Daily, DailyWeek WeekData)
{
    public static DailyWeekModel Empty = new(new(), new());
}
internal sealed class DailyPage : MvuPage
{
    protected override StyleGroup? BuildStyles() => Shared.Styles.ListStyles();
    private Border BuildWeekBar()
    {
        return new Border()
            .Theme(StaticResource<ControlTheme>("CardBorder"))
            .Padding(8)
            .Child(
                HStackPanel(Aligns.HCenter, Aligns.VCenter)
                .Spacing(8)
                .Children(
                    IconButton(MIcon.ChevronLeft).OnClick(_ => ChangeWeek(-1)),
                    PzText(() => WeekText).Align(Aligns.VCenter),
                    IconButton(MIcon.ChevronRight).OnClick(_ => ChangeWeek(1))
                )
            );
    }

    private StackPanel BuildHeaderCell(int i)
    {
        return VStackPanel(Aligns.HStretch, Aligns.VCenter)
                .Children(
                    PzText(WeekDayText(i))
                        .TextAlignment(Avalonia.Media.TextAlignment.Center),
                    PzText(() => MondayDate.AddDays(i).ToString("MM/dd"))
                        .TextAlignment(Avalonia.Media.TextAlignment.Center)
                        .FontSize(12)
                        .Classes("Tertiary")
                ).Col(i + 1);
    }
    private Grid BuildHeaderGrid()
    {
        StackPanel[] days = new StackPanel[7];
        for (int i = 0; i < 7; i++)
        {
            days[i] = BuildHeaderCell(i);
        }


        return PzGrid(cols: "*, 72, 72, 72, 72, 72, 72, 72")
            .Children(
                children: [
                    PzText(() => LD.Name).Col(0).Margin(16, 0),
                    .. days
                ]
            );
    }
    private ScrollViewer BuildDataGrid()
    {
        return new ScrollViewer()
            .Content(
                new CachedList<DailyWeekItem, DailyWeekModel>(Items)
                    .ItemsPanel(VStackPanel(Aligns.HStretch))
                    .ItemCreator(() => new DailyWeekItem(this))
            );
    }
    protected override Control Build()
    {
        return new Panel().Children(
                TodayMark
                    .Width(72)
                    .Background(DynamicColors.Get("SemiColorBorder")),
                PzGrid(rows: "60, 50, *")
                    .Children(
                        BuildWeekBar().Row(0),
                        BuildHeaderGrid().Row(1).Margin(0, 8, 0, 4),
                        BuildDataGrid().Row(2)
                    )
            );
    }

    private readonly DailyManager _manager;
    private DateOnly Today { get; set; }
    private DateOnly MondayDate { get; set; }
    private ReactiveList<DailyWeekModel> Items { get; set; } = [];
    private string WeekText => $"{MondayDate:yyyy/MM/dd} - {MondayDate.AddDays(6):yyyy/MM/dd}";
    private Border TodayMark { get; set; } = new() { HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left };

    public DailyPage(DailyManager manager) : base()
    {
        _manager = manager;

        Today = DateOnly.FromDateTime(DateTime.Today);
        MondayDate = _manager.GetMondayDate(Today);
    }
    protected override IEnumerable<IDisposable> WhenActivate()
    {
        UpdateWeeks();
        return base.WhenActivate();
    }
    protected override void OnBeforeReload()
    {
        base.OnBeforeReload();
        TodayMark = new() { HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left };
    }

    private static string WeekDayText(int i)
    {
        return i switch
        {
            0 => LD.Mon,
            1 => LD.Tue,
            2 => LD.Wed,
            3 => LD.Thu,
            4 => LD.Fri,
            5 => LD.Sat,
            6 => LD.Sun,
            _ => ""
        };
    }

    private void ChangeWeek(int change)
    {
        if (change > 0) MondayDate = MondayDate.AddDays(7);
        else MondayDate = MondayDate.AddDays(-7);

        UpdateWeeks();
    }
    private void UpdateWeeks()
    {
        var dailies = _manager.GetDailies(EnableState.Enabled);
        var weeks = _manager.GetDailyWeeks(MondayDate, dailies.Select(d => d.Id).ToList());

        var models = dailies.Select(d =>
        {
            var dw = weeks.FirstOrDefault(dw => dw.DailyId == d.Id);
            if (dw is null)
            {
                dw = new DailyWeek();
                dw.Init(d.Id, MondayDate);
            }
            return new DailyWeekModel(d, dw);
        });
        Items.ReplaceAll(models);
        UpdateState();
        ComputeTodayMarkPosition();
    }
    private void ComputeTodayMarkPosition()
    {
        if (Today < MondayDate || Today > MondayDate.AddDays(6))
        {
            TodayMark.Opacity = 0;
            return;
        }

        var n = Today.DayOfWeek == DayOfWeek.Sunday ? 7 : (int)Today.DayOfWeek;
        var left = this.Bounds.Width - 72 * (8 - n);
        TodayMark.Margin = new Thickness(left, 60, 0, 0);
        TodayMark.Opacity = 1;
    }

    public void ItemUpdated(DailyWeek dw)
    {
        _manager.WriteDailyWeek(dw);
    }
}

internal class DailyWeekItem : MvuComponent, IListItemComponent<DailyWeekModel>
{
    // start of monday
    public static readonly DayOfWeek[] Days = [DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday, DayOfWeek.Saturday, DayOfWeek.Sunday];

    protected override Control Build()
    {
        return PzGrid(cols: "*, 72, 72, 72, 72, 72, 72, 72")
            .Height(60)
            .Classes("ListRow")
            .Children(
                children: [
                    PzText(() => Model.Daily.Name).Col(0),
                    .. Buttons.Values
                ]
            );
    }

    private DailyWeekModel Model { get; set; }
    private readonly DailyPage _parent;
    private Dictionary<DayOfWeek, Button> Buttons { get; init; } = [];
    public DailyWeekItem(DailyPage parent) : base()
    {
        Model = DailyWeekModel.Empty;
        _parent = parent;

        for(int i = 0; i < Days.Length; i++)
        {
            var d = Days[i];
            var btn = IconButton(MIcon.BookmarkOutline)
                .OnPointerReleased(e => OnDayStateChanged(e, d))
                .Col(i + 1);
            Buttons.Add(d, btn);
        }

        Initialize();
    }

    private void UpdateButtonState(DayOfWeek d)
    {
        Buttons[d].Content = GetIcon(Model.WeekData[d]);
    }
    private void OnDayStateChanged(PointerReleasedEventArgs e, DayOfWeek d)
    {
        e.Handled = true;
        var orgState = Model.WeekData[d];
        if (e.Properties.PointerUpdateKind == PointerUpdateKind.LeftButtonReleased)
        {
            Model.WeekData[d] = orgState >= 2 ? 0 : orgState + 1;
        }
        else if (e.Properties.PointerUpdateKind == PointerUpdateKind.RightButtonReleased)
        {
            Model.WeekData[d] = orgState == 2 ? 0 : 2;
        }

        _parent.ItemUpdated(Model.WeekData);
        UpdateButtonState(d);
    }
    private MaterialIcon GetIcon(int dayState)
    {
        return dayState switch
        {
            1 => MaterialIcon(MIcon.BookmarkCheckOutline).Foreground(StaticColor("SemiColorSuccess")),
            2 => MaterialIcon(MIcon.BookmarkRemoveOutline).Foreground(StaticColor("SemiColorDanger")),
            0 or _ => MaterialIcon(MIcon.BookmarkOutline).Foreground(StaticColor("SemiColorTertiary")),
        };
    }

    public void UpdateItem(DailyWeekModel item)
    {
        Model = item;
        foreach (var d in Days) UpdateButtonState(d);
        UpdateState();
    }
}