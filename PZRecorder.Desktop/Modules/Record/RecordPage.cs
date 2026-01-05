using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Avalonia.Styling;
using Microsoft.Extensions.DependencyInjection;
using PZ.RxAvalonia.Reactive;
using PZRecorder.Core.Managers;
using PZRecorder.Core.Tables;
using PZRecorder.Desktop.Common;
using PZRecorder.Desktop.Extensions;
using PZRecorder.Desktop.Modules.Shared;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace PZRecorder.Desktop.Modules.Record;

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

internal class RecordPage : PzPageBase
{
    #region templete
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

        var stateColor = record.State switch
        {
            RecordState.Wish => "Blue",
            RecordState.Doing => "Orange",
            RecordState.Complete => "Green",
            RecordState.Giveup => "Pink",
            _ => "Gray"
        };
        var rightBar = new Border()
            .Col(1)
            .BorderThickness(1, 0, 0, 0)
            .BorderBrush(StaticColor("SemiColorBorder"))
            .Child(
                VStackPanel(Aligns.HCenter, Aligns.VCenter)
                    .Margin(8)
                    .Spacing(8)
                    .Children(
                        new Uc.DualBadge()
                            .Classes("ForTheBadge").Classes(stateColor)
                            .Content(record.State.ToString())
                            .Align(Aligns.HCenter),
                        IconButton(MIcon.Edit, classes: "Primary")
                            .Theme(StaticResource<ControlTheme>("BorderlessButton"))
                            .OnClick(_ => EditRecord(record)),
                        IconButton(MIcon.Delete, classes: "Danger")
                            .Theme(StaticResource<ControlTheme>("BorderlessButton"))
                            .OnClick(_ => DeleteRecord(record))
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
            .SelectedValueEx(Query.Kind);
    }
    private DockPanel BuildList()
    {
        return new DockPanel()
            .Children(
                new Uc.Pagination() { ShowPageSizeSelector = true, ShowQuickJump = true, PageSizeOptions = [10, 15, 20, 30] }
                    .Dock(Dock.Bottom)
                    .WithModel(Pagenation),
                new ScrollViewer().Dock(Dock.Top)
                    .Content(
                        new ItemsControl()
                            .ItemsPanel(
                                VStackPanel(Aligns.HStretch)
                                    .Spacing(12)
                                    .Margin(16, 0)
                            )
                            .ItemsSource(Pagenation.PageItems)
                            .ItemTemplate<TbRecord, ItemsControl>(RecordItemTemplete)
                    )
            );
    }
    private StackPanel BuildSearchPanel()
    {
        return VStackPanel(Aligns.HStretch, Aligns.VStretch)
            .Spacing(16)
            .Children(
                PzTextBox(Query.Search).Margin(16, 8).Watermark("Search..."),
                new Border().Theme(StaticResource<ControlTheme>("RadioButtonGroupBorder"))
                .MaxWidth(284)
                .Child(
                    new ListBox()
                        .Theme(StaticResource<ControlTheme>("ButtonRadioGroupListBox"))
                        .ItemsPanel(
                            new Uc.ElasticWrapPanel()
                                .Orientation(Avalonia.Layout.Orientation.Horizontal)
                                .ItemsAlignment(WrapPanelItemsAlignment.Start)
                        )
                        .ItemsSource(Enum.GetValues<RecordState>())
                        .SelectionMode(SelectionMode.Single | SelectionMode.Toggle)
                        .SelectedValueEx(Query.State)
                ),
                new Border().Theme(StaticResource<ControlTheme>("CardBorder"))
                .Child(
                    VStackPanel()
                        .Styles(new Style<ComboBox>().Width(240))
                        .Spacing(8)
                        .Children(
                            PzText("Year"),
                            new ComboBox()
                                .ItemsSource(Years)
                                .ItemTemplate<int?, ComboBox>(n => PzText(n.HasValue ? n.Value.ToString() : "-"))
                                .SelectedValueEx(Query.Year),
                            PzText("Month"),
                            new ComboBox()
                                .ItemsSource(Monthes)
                                .ItemTemplate<int?, ComboBox>(n => PzText(n.HasValue ? n.Value.ToString() : "-"))
                                .SelectedValueEx(Query.Month),
                            PzText("Rating"),
                            new ComboBox()
                                .ItemsSource(Ratings)
                                .ItemTemplate<int?, ComboBox>(n => PzText(n.HasValue ? n.Value.ToString() : "-"))
                                .SelectedValueEx(Query.Rating),
                            PzText("Order"),
                            new Uc.EnumSelector()
                                .Width(240)
                                .EnumType(typeof(RecordSort))
                                .Value(Pagenation.Order)
                        )
                ),
                PzButton("Add").OnClick(_ => AddRecord())
            );
    }
    private Grid BuildContent()
    {
        var grid = PzGrid(cols: "*, auto");
        grid.ColumnDefinitions[1].MaxWidth = 300;

        return grid
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
    #endregion

    private readonly RecordManager _manager;

    #region datas
    private readonly QueryModel Query = new();
    private Subject<List<Kind>> Kinds { get; init; } = new();
    private OrderedPagenationModel<TbRecord, RecordSort> Pagenation { get; init; } = new(ReocordOrder, RecordSort.ModifyTimeDesc);

    private Subject<List<int?>> Years { get; init; } = new();
    private static readonly int?[] Monthes = [null, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12];
    private static readonly int?[] Ratings = [null, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10];
    #endregion

    public RecordPage() : base(ViewInitializationStrategy.Lazy)
    {
        _manager = ServiceProvider.GetRequiredService<RecordManager>();
        Initialize();
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
            Query.Query.Subscribe(q => UpdateList(q)),
            ..base.WhenActivate()
        ];
    }

    private static IOrderedEnumerable<TbRecord> ReocordOrder(IEnumerable<TbRecord> items, RecordSort sort)
    {
        return items.OrderBy(x => sort switch
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
    private void UpdateList(RecordsQuery query, bool force = false)
    {
        if (EqualsQuery(query, _cacheQuery) && !force) return;

        var records = _manager.QueryRecords(query);
        Pagenation.Update(records);
        _cacheQuery = query;
    }
    private void UpdateYears(Kind? kind)
    {
        if (kind is null)
        {
            Years.OnNext([null]);
            return;
        }
        var years = _manager.GetYears(kind.Id);
        Years.OnNext([null, .. years]);
    }

    private async void AddRecord()
    {
        if (Query.Kind.Value == null)
        {
            await PzDialogManager.Alert("Kind not selected!", "Error");
            return;
        }

        var res = await PzDialogManager.ShowDialog(new RecordDialog(Query.Kind.Value.Id));
        if (res != null)
        {
            _manager.InsertRecord(res);
            if (_cacheQuery != null) UpdateList(_cacheQuery, true);
        }
    }
    private async void EditRecord(TbRecord item)
    {
        var res = await PzDialogManager.ShowDialog(new RecordDialog(item));
        if (res != null)
        {
            _manager.UpdateRecord(res);
            if (_cacheQuery != null) UpdateList(_cacheQuery, true);
        }
    }
    private async void DeleteRecord(TbRecord item)
    {
        var dialog = PzDialogManager.ConfirmDialog("Delete", "Sure to delete?");
        dialog.Mode = Uc.DialogMode.Question;
        dialog.BoxButtons[0].Text = "Delete";
        dialog.BoxButtons[0].Styles = ["Danger"];

        var delete = await PzDialogManager.ShowDialog(dialog);
        if (PzDialogManager.IsSureResult(delete))
        {
            _manager.DeleteRecord(item.Id);
            if (_cacheQuery != null) UpdateList(_cacheQuery, true);
        }
    }
}
