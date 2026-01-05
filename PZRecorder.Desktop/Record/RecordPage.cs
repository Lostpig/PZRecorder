using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Avalonia.Styling;
using Microsoft.Extensions.DependencyInjection;
using PZ.RxAvalonia.Reactive;
using PZRecorder.Core.Managers;
using PZRecorder.Core.Tables;
using PZRecorder.Desktop.Common;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace PZRecorder.Desktop.Record;

using TbRecord = Core.Tables.Record;

internal class QueryModel
{
    public BehaviorSubject<Kind?> Kind { get; init; } = new(null);
    public Subject<string> Search { get; init; } = new();
    public Subject<int?> Year { get; init; } = new();
    public Subject<int?> Month { get; init; } = new();
    public Subject<int?> Rating { get; init; } = new();
    public Subject<RecordState?> State { get; init; } = new();

    public Subject<RecordsQuery> _query { get; init; } = new();
    public IObservable<RecordsQuery> Query { get; init; }
    public QueryModel()
    {
        Query = Observable.CombineLatest(
            Kind, 
            Search.StartWith(""), 
            Year.StartWith((int?)null), 
            Month.StartWith((int?)null), 
            Rating.StartWith((int?)null), 
            State.StartWith((RecordState?)null), 
            (kind, search, year, month, rating, state) =>
            {
                if (kind is null) return null;
                return new RecordsQuery
                {
                    KindId = kind.Id,
                    SearchText = search,
                    Year = year,
                    Month = month,
                    Rating = rating,
                    State = state
                };
            }).WhereNotNull();
    }
}

internal class RecordPage : PZPageBase
{
    private Border RecordItemTemplete(TbRecord record)
    {
        var content = VStackPanel()
            .Margin(0, 0, 8, 0)
            .Spacing(8)
            .TextBlock_TextWrapping(TextWrapping.Wrap)
            .Children(
                PzText(record.Name)
                    .Align(Aligns.VCenter)
                    .FontSize(20)
                    .Foreground(StaticColor("SemiColorLink")),
                PzText(record.Alias)
                    .Margin(0, -8, 0 ,0)
                    .Align(Aligns.VCenter)
                    .Foreground(StaticColor("SemiColorText2")),
                new DockPanel()
                    .LastChildFill(false)
                    .HorizontalSpacing(8)
                    .Children(
                        PzText($"{record.Episode} / {record.EpisodeCount}").Dock(Dock.Left),
                        PzText($"{record.PublishYear}-{record.PublishMonth}").Dock(Dock.Left),
                        PzText($"{record.ModifyDate:yyyy-MM-dd HH:mm}").Dock(Dock.Right)
                    ),
                PzText(record.Remark)
                    .FontSize(14)
                    .Foreground(StaticColor("SemiColorText3")),
                new Uc.Rating() { DefaultValue = record.Rating, Count = 10 }
                    .Classes("Small")
                    .IsEnabled(false)
            );
        var rightBar = new Border()
            .Col(1)
            .BorderThickness(1, 0, 0, 0)
            .BorderBrush(StaticColor("SemiColorBorder"))
            .Child(
                VStackPanel(Aligns.HCenter, Aligns.VCenter)
                    .Margin(8)
                    .Spacing(8)
                    .Children(
                        PzText(record.State.ToString()),
                        PzButton("Edit"),
                        PzButton("Remove")
                    )
            );

        return new Border()
            .Theme(StaticResource<ControlTheme>("CardBorder"))
            .Child(
                PzGrid(cols: "*, 100")
                .Children(
                    content.Col(0),
                    rightBar.Col(1)
                )
            );
    }
    private TabStrip BuildTabs()
    {
        return new TabStrip()
            .Theme(StaticResource<ControlTheme>("ScrollLineTabStrip"))
            .ItemsSource(Kinds)
            .ItemTemplate<Kind, TabStrip>(k => PzText(k.Name))
            .SelectedItemEx(Query.Kind);
    }
    private DockPanel BuildList()
    {
        return new DockPanel()
            .Children(
                new Uc.Pagination() { ShowPageSizeSelector = true, ShowQuickJump = true, PageSizeOptions = [10, 15, 20, 30] }
                    .Dock(Dock.Bottom)
                    .PageSize(PageSize)
                    .TotalCount(Records.Select(r => r.Count))
                    .CurrentPage(Page),
                new ScrollViewer().Dock(Dock.Top)
                    .Content(
                        new ItemsControl()
                            .ItemsPanel(
                                VStackPanel(Aligns.HStretch)
                                    .Spacing(12)
                                    .Margin(16, 0)
                            )
                            .ItemsSource(PagedRecords)
                            .ItemTemplate<TbRecord, ItemsControl>(RecordItemTemplete)
                    )
            );
    }
    private StackPanel BuildSearchPanel()
    {
        return VStackPanel(Aligns.Left, Aligns.VStretch)
            .Spacing(16)
            .Children(
                PzTextBox(Query.Search).Width(200).Watermark("Search..."),
                new Uc.SelectionList() {  }
                    .ItemsPanel(HStackPanel(Aligns.HCenter))
                    .ItemsSource(States)
                    .ItemTemplate<RecordState?, Uc.SelectionList>(s => PzText(s.HasValue ? s.Value.ToString() : "All"))
                    .SelectedItemEx(Query.State),
                new Border().Theme(StaticResource<ControlTheme>("CardBorder"))
                .Child(
                    VStackPanel()
                        .Spacing(8)
                        .Children(
                            PzText("Year"),
                            new ComboBox()
                                .ItemsSource(Years)
                                .ItemTemplate<int?, ComboBox>(n => PzText(n.HasValue ? n.Value.ToString() : "-"))
                                .SelectedItemEx(Query.Year),
                            PzText("Month"),
                            new ComboBox()
                                .ItemsSource(Monthes)
                                .ItemTemplate<int?, ComboBox>(n => PzText(n.HasValue ? n.Value.ToString() : "-"))
                                .SelectedItemEx(Query.Month),
                            PzText("Rating"),
                            new ComboBox()
                                .ItemsSource(Ratings)
                                .ItemTemplate<int?, ComboBox>(n => PzText(n.HasValue ? n.Value.ToString() : "-"))
                                .SelectedItemEx(Query.Rating),
                            PzText("Order"),
                            new Uc.EnumSelector()
                                .EnumType(typeof(RecordSort))
                                .Value(Sort)
                        )
                )
            );
    }
    private Grid BuildContent()
    {
        return PzGrid(cols: "*, auto")
            .ColumnSpacing(8)
            .Children(
                BuildList().Col(0),
                BuildSearchPanel().Col(1)
            );
    }
    protected override Control Build() => 
        PzGrid(rows: "auto, *")
            .Margin(8)
            .RowSpacing(8)
            .Children(
                BuildTabs().Row(0),
                BuildContent().Row(1)
            );

    private readonly RecordManager _manager;

    #region datas
    private readonly QueryModel Query = new();
    private Subject<List<Kind>> Kinds { get; init; } = new();
    private Subject<List<TbRecord>> Records { get; init; } = new();
    private IObservable<List<TbRecord>> PagedRecords { get; init; }

    private BehaviorSubject<int> Page { get; init; } = new(0);
    private BehaviorSubject<int> PageSize { get; init; } = new(15);
    private BehaviorSubject<RecordSort> Sort { get; init; } = new(RecordSort.ModifyTimeDesc);

    private Subject<List<int?>> Years { get; init; } = new();
    private static readonly int?[] Monthes = [null, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12];
    private static readonly int?[] Ratings = [null, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10];
    private readonly RecordState?[] States = [null, ..Enum.GetValues<RecordState>()];
    #endregion

    public RecordPage() : base(ViewInitializationStrategy.Lazy)
    {
        _manager = ServiceProvider.GetRequiredService<RecordManager>();
        PagedRecords = Records.CombineLatest(Sort)
            .Select(ReocordOrder)
            .CombineLatest(Page)
            .Select(x => x.First.Take(GetPageRange(x.Second)).ToList());
        Initialize();
    }

    private static IOrderedEnumerable<TbRecord> ReocordOrder((List<TbRecord>, RecordSort) tuple)
    {
        var (list, s) = tuple;

        return list.OrderBy(x => s switch
            {
                RecordSort.PublishTimeAsc => x.PublishYear * 100 + x.PublishMonth,
                RecordSort.PublishTimeDesc => -(x.PublishYear * 100 + x.PublishMonth),
                RecordSort.ModifyTimeAsc => x.ModifyDate.Ticks,
                RecordSort.ModifyTimeDesc => -x.ModifyDate.Ticks,
                RecordSort.RatingAsc => x.Rating,
                RecordSort.RatingDesc => -x.Rating,
                _ => -(x.PublishYear * 100 + x.PublishMonth),
            }
        );
    }
    private Range GetPageRange(int page)
    {
        var size = PageSize.Value;
        return new Range(page * size, (page + 1) * size);
    }
    private void PageSizeChanged(Tuple<int, int> sizes)
    {
        var p = Page.Value;
        var start = p * sizes.Item1;
        var newPage = start / sizes.Item2;

        Page.OnNext(newPage);
    }
    private void UpdateYears(Kind? kind)
    {
        if (kind is null)
        {
            Years.OnNext([null]);
            return;
        }
        var years = _manager.GetYears(kind.Id);
        Years.OnNext([null, ..years]);
    }

    protected override IEnumerable<IDisposable> WhenActivate()
    {
        var kinds = _manager.GetKinds();
        Kinds.OnNext(kinds);

        if (Query.Kind.Value == null)
        {
            Query.Kind.OnNext(kinds.FirstOrDefault());
        }
        else
        {
            var k = kinds.FirstOrDefault(k => k.Id == Query.Kind.Value.Id);
            Query.Kind.OnNext(k);
        }

        return [
            Query.Kind.Subscribe(k => UpdateYears(k)),
            Query.Query.Subscribe(UpdateList),
            PageSize.PairWithPrevious().Subscribe(PageSizeChanged),
            ..base.WhenActivate()
        ];
    }

    private RecordsQuery? _cacheQuery;
    private static bool EqualsQuery(RecordsQuery query, RecordsQuery? _cahce)
    {
        if (_cahce is null) return false;
        return query.KindId == _cahce.KindId
            && query.SearchText == _cahce.SearchText
            && query.Year == _cahce.Year
            && query.Month == _cahce.Month
            && query.Rating == _cahce.Rating
            && query.State == _cahce.State;
    }
    private void UpdateList(RecordsQuery query)
    {
        if (EqualsQuery(query, _cacheQuery)) return;

        var records = _manager.QueryRecords(query);
        Page.OnNext(0);
        Records.OnNext(records);
        _cacheQuery = query;
    }
}
