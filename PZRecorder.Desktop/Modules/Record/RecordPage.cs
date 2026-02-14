using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Styling;
using PZ.RxAvalonia.Reactive;
using PZRecorder.Core.Managers;
using PZRecorder.Core.Tables;
using PZRecorder.Desktop.Common;
using PZRecorder.Desktop.Extensions;
using PZRecorder.Desktop.Modules.Shared;
using System.Reactive.Linq;

namespace PZRecorder.Desktop.Modules.Record;

using TbRecord = Core.Tables.Record;

internal sealed class RecordPageModel
{
    public List<Kind> Kinds { get; set; } = [];
    public RecordsQuery Query { get; set; } = new();
    public MvuPagenationState Pagenation { get; set; } = new();
    public ReactiveList<TbRecord> Items { get; init; } = [];
    public RecordSort Order { get; set; } = RecordSort.ModifyTimeDesc;
    public Kind? SelectedKind { get; set; } = null;
}

internal sealed class RecordPage(RecordManager _manager, BroadcastManager _broadcast) : MvuPage()
{
    #region templete
    private TabStrip BuildTabs()
    {
        return new TabStrip()
            .Theme(StaticResource<ControlTheme>("ScrollLineTabStrip"))
            .ItemsSource(() => Model.Kinds)
            .ItemTemplate<Kind, TabStrip>(k => PzText(k.Name))
            .SelectedValue(() => Model.SelectedKind)
            .OnSelectionChanged(OnSelectKind);
    }
    private PagenationList<RecordItem, TbRecord> BuildList()
    {
        var pagedList = new PagenationList<RecordItem, TbRecord>(Model.Items)
            .ItemsPanel(
                VStackPanel(Aligns.HStretch)
                    .Spacing(12)
                    .Margin(16, 0)
            )
            .ItemCreator(() => new RecordItem(this));

        return pagedList;
    }
    private StackPanel BuildSearchPanel()
    {
        return VStackPanel(Aligns.HStretch, Aligns.VStretch)
            .Spacing(16)
            .Children(
                PzTextBox(() => SearchText)
                    .OnTextChanged(OnSearchChanged)
                    .OnKeyDown(OnSearchKeyDown)
                    .Margin(16, 8)
                    .Watermark(() => LD.Search),
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
                        .ItemTemplate(new FuncDataTemplete<RecordState, TextBlock>((s) => PzText(() => GetStateText(s)), this))
                        .SelectionMode(SelectionMode.Single | SelectionMode.Toggle)
                        .SelectedValue(() => Model.Query.State)
                        .OnSelectionChanged(OnStateChanged)
                ),
                new Border().Theme(StaticResource<ControlTheme>("CardBorder"))
                .Child(
                    VStackPanel()
                        .Styles(new Style<ComboBox>().Width(240))
                        .Spacing(8)
                        .Children(
                            PzText(() => LD.Year),
                            new ComboBox()
                                .ItemsSource(() => Years)
                                .ItemTemplate<int, ComboBox>(n => PzText(n > 0 ? n.ToString() : "-"))
                                .SelectedValue(() => Model.Query.Year)
                                .OnSelectionChanged(OnYearChanged),
                            PzText(() => LD.Month),
                            new ComboBox()
                                .ItemsSource(Months)
                                .ItemTemplate<int, ComboBox>(n => PzText(n > 0 ? n.ToString() : "-"))
                                .SelectedValue(() => Model.Query.Month)
                                .OnSelectionChanged(OnMonthChanged),
                            PzText(() => LD.Rating),
                            new ComboBox()
                                .ItemsSource(Ratings)
                                .ItemTemplate<int, ComboBox>(n => PzText(n >= 0 ? n.ToString() : "-"))
                                .SelectedValue(() => Model.Query.Rating)
                                .OnSelectionChanged(OnRatingChanged),
                            PzText(() => LD.OrderBy),
                            new ComboBox()
                                .ItemsSource(Enum.GetValues<RecordSort>())
                                .ItemTemplate(new FuncDataTemplete<RecordSort, TextBlock>((s) => PzText(() => GetSortText(s)), this))
                                .SelectedValue(() => Model.Order)
                                .OnSelectionChanged(OnOrderChanged)
                        )
                ),
                IconButton(MIcon.Add, () => LD.Add).OnClick(_ => AddRecord())
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

    #region event handlers
    private void OnSelectKind(SelectionChangedEventArgs e)
    {
        if (e.Source is SelectingItemsControl c && c.SelectedValue is Kind k)
        {
            Model.Query.KindId = k.Id;
            ExcuteQuery();
        }
    }
    private void OnSearchChanged(TextChangedEventArgs e)
    {
        var text = ((TextBox)e.Source!).Text ?? "";
        SearchText = text;
    }
    private void OnSearchKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            Model.Query.SearchText = SearchText;
            ExcuteQuery();
        }
    }
    private void OnStateChanged(SelectionChangedEventArgs e)
    {
        if (e.Source is ListBox l && l.SelectedValue is RecordState s)
        {
            Model.Query.State = s;
        }
        else
        {
            Model.Query.State = null;
        }
        ExcuteQuery();
    }
    private void OnYearChanged(SelectionChangedEventArgs e)
    {
        if (e.Source is ComboBox c && c.SelectedValue is int x)
        {
            Model.Query.Year = x;
            ExcuteQuery();
        }
    }
    private void OnMonthChanged(SelectionChangedEventArgs e)
    {
        if (e.Source is ComboBox c && c.SelectedValue is int x)
        {
            Model.Query.Month = x;
            ExcuteQuery();
        }
    }
    private void OnRatingChanged(SelectionChangedEventArgs e)
    {
        if (e.Source is ComboBox c && c.SelectedValue is int x)
        {
            Model.Query.Rating = x;
            ExcuteQuery();
        }
    }
    private void OnOrderChanged(SelectionChangedEventArgs e)
    {
        if (e.Source is ComboBox c && c.SelectedValue is RecordSort s)
        {
            Model.Order = s;
            Model.Items.Sort(RecordComparsion);
        }
    }
    #endregion

    private RecordPageModel Model { get; set; } = new();
    private int[] Years = [-1];
    private readonly int[] Months = [-1, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12];
    private readonly int[] Ratings = [-1, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10];
    private RecordsQuery? _cachedQuery;
    private string SearchText { get; set; } = "";

    protected override void OnCreated()
    {
        base.OnCreated();
        _broadcast.Broadcast.Where(e => e == BroadcastEvent.DataImported)
            .Subscribe(_ =>
            {
                _cachedQuery = null;
            });
    }
    protected override IEnumerable<IDisposable> WhenActivate()
    {
        var kindId = Model.Query.KindId;
        UpdateKinds();

        var kind = Model.Kinds.Find(k => k.Id == kindId);
        kind ??= Model.Kinds.FirstOrDefault();
        if (kind?.Id != kindId)
        {
            Model.Query.KindId = kind?.Id ?? -1;
            ExcuteQuery();
        }

        return base.WhenActivate();
    }

    private bool _updating = false;
    private void UpdatePage()
    {
        _updating = true;
        UpdateState();
        _updating = false;
    }
    private void UpdateKinds()
    {
        static void updateKind(Kind newOne, Kind current)
        {
            current.Name = newOne.Name;
            current.OrderNo = newOne.OrderNo;
            current.StateWishName = newOne.StateWishName;
            current.StateDoingName = newOne.StateDoingName;
            current.StateCompleteName = newOne.StateCompleteName;
            current.StateGiveupName = newOne.StateGiveupName;
        }

        var currentKinds = Model.Kinds;
        var newKinds = _manager.GetKinds();

        List<Kind> kl = new(newKinds.Count);
        foreach (var nk in newKinds)
        {
            var ck = currentKinds.Find(k => k.Id == nk.Id);
            if (ck == null)
            {
                kl.Add(nk);
            }
            else
            {
                updateKind(nk, ck);
                kl.Add(ck);
            }
        }
        Model.Kinds = kl;

        UpdatePage();
    }
    private void ExcuteQuery(bool force = false)
    {
        if (Model.Query.KindId < 0 || _updating) return;
        if (Model.Query.Equals(_cachedQuery) && !force) return;

        if (Model.Query.KindId != _cachedQuery?.KindId || force)
        {
            Years = _manager.GetYears(Model.Query.KindId).ToArray();
        }
        if (!Years.Contains(Model.Query.Year))
        {
            Model.Query.Year = -1;
        }

        var items = _manager.QueryRecords(Model.Query);
        Model.SelectedKind = Model.Kinds.Find(k => k.Id == Model.Query.KindId);

        items.Sort(RecordComparsion);
        Model.Items.ReplaceAll(items);

        Model.Query.CopyValueTo(_cachedQuery ??= new());

        UpdatePage();
    }

    private int RecordComparsion(TbRecord x, TbRecord y)
    {
        var res = Model.Order switch
        {
            RecordSort.PublishTimeAsc => (x.PublishYear * 100 + x.PublishMonth) - (y.PublishYear * 100 + y.PublishMonth),
            RecordSort.PublishTimeDesc => (y.PublishYear * 100 + y.PublishMonth) - (x.PublishYear * 100 + x.PublishMonth),
            RecordSort.RatingAsc => x.Rating - y.Rating,
            RecordSort.RatingDesc => y.Rating - x.Rating,
            RecordSort.ModifyTimeAsc => (x.ModifyDate - y.ModifyDate).Seconds,
            RecordSort.ModifyTimeDesc or _ => (y.ModifyDate - x.ModifyDate).Seconds,
        };
        return res;
    }
    internal string GetStateText(RecordState state)
    {
        var kind = Model.SelectedKind;

        return state switch
        {
            RecordState.Wish => string.IsNullOrEmpty(kind?.StateWishName) ? LD.Wish : kind.StateWishName,
            RecordState.Doing => string.IsNullOrEmpty(kind?.StateDoingName) ? LD.Doing : kind.StateDoingName,
            RecordState.Complete => string.IsNullOrEmpty(kind?.StateCompleteName) ? LD.Complete : kind.StateCompleteName,
            RecordState.Giveup => string.IsNullOrEmpty(kind?.StateGiveupName) ? LD.Giveup : kind.StateGiveupName,
            _ => "-"
        };
    }
    private static string GetSortText(RecordSort sort)
    {
        return sort switch
        {
            RecordSort.ModifyTimeAsc => $"{LD.EditTime}({LD.Ascending})",
            RecordSort.ModifyTimeDesc => $"{LD.EditTime}({LD.Descending})",
            RecordSort.PublishTimeAsc => $"{LD.PublishDate}({LD.Ascending})",
            RecordSort.PublishTimeDesc => $"{LD.PublishDate}({LD.Descending})",
            RecordSort.RatingAsc => $"{LD.Rating}({LD.Ascending})",
            RecordSort.RatingDesc => $"{LD.Rating}({LD.Descending})",
            _ => "-"
        };
    }

    internal async void AddRecord()
    {
        if (Model.Query.KindId < 0)
        {
            Notification.Error("Kind not selected!", "Error");
            return;
        }

        var kind = Model.Kinds.Find(k => k.Id == Model.Query.KindId);
        if (kind == null)
        {
            Notification.Error("Kind not found!", "Error");
            return;
        }

        var res = await PzDialogManager.ShowDialog(new RecordDialog(kind.Id, Model.Kinds));
        if (PzDialogManager.IsSureResult(res.Result))
        {
            _manager.InsertRecord(res.Value);
            ExcuteQuery(true);
        }
    }
    internal async void EditRecord(TbRecord item)
    {
        var res = await PzDialogManager.ShowDialog(new RecordDialog(item, Model.Kinds));
        if (PzDialogManager.IsSureResult(res.Result))
        {
            _manager.UpdateRecord(res.Value);
            ExcuteQuery(true);
        }
    }
    internal async void DeleteRecord(TbRecord item)
    {
        var dialog = PzDialogManager.DeleteConfirmDialog();
        var delete = await PzDialogManager.ShowDialog(dialog);
        if (PzDialogManager.IsSureResult(delete.Result))
        {
            _manager.DeleteRecord(item.Id);
            ExcuteQuery(true);
        }
    }
}

internal sealed class RecordItem(RecordPage Page) : MvuComponent, IListItemComponent<TbRecord>
{
    private TbRecord Model { get; set; } = new();
    private string StatusColor => Model.State switch
    {
        RecordState.Wish => "Orange",
        RecordState.Doing => "Blue",
        RecordState.Complete => "Green",
        RecordState.Giveup or _ => "Grey",
    };

    private Uc.DualBadge StatusBagde()
    {
        var badge = new Uc.DualBadge()
            .Classes("ForTheBadge")
            .Content(() => Page.GetStateText(Model.State))
            .Align(Aligns.HCenter);

        badge._set<Uc.DualBadge, string>(
                setter: (c, sc) => {
                    c.Classes.Clear();
                    c.Classes.AddRange(["ForTheBadge", sc]);
                },
                getter: () => StatusColor
            );

        return badge;
    }

    public void UpdateItem(TbRecord item)
    {
        Model = item;
        UpdateState();
    }
    protected override Control Build()
    {
        var content = VStackPanel()
            .Margin(0, 0, 8, 0)
            .Spacing(8)
            .TextBlock_TextWrapping(TextWrapping.Wrap)
            .Children(
                PzText(() => Model.Name)
                    .Align(Aligns.VCenter)
                    .FontSize(20)
                    .Foreground(DynamicColors.Get("SemiColorLink")),
                PzText(() => Model.Alias)
                    .Margin(0, -8, 0, 0)
                    .Align(Aligns.VCenter)
                    .Foreground(DynamicColors.Get("SemiColorText2")),
                new DockPanel()
                    .LastChildFill(false)
                    .HorizontalSpacing(8)
                    .Children(
                        PzText(() => $"{Model.Episode} / {Model.EpisodeCount}").Dock(Dock.Left),
                        PzText(() => $"{Model.PublishYear}-{Model.PublishMonth}").Dock(Dock.Left),
                        PzText(() => $"{Model.ModifyDate:yyyy-MM-dd HH:mm}").Dock(Dock.Right)
                    ),
                PzText(() => Model.Remark)
                    .TextWrapping(TextWrapping.WrapWithOverflow)
                    .MaxHeight(80)
                    .FontSize(14)
                    .Foreground(DynamicColors.Get("SemiColorText3")),
                new Uc.Rating() { Count = 10 }
                    .DefaultValue(() => Model.Rating)
                    .Value(() => Model.Rating)
                    .Classes("Small")
                    .IsEnabled(false)
            );

        var rightBar = new Border()
            .Col(1)
            .BorderThickness(1, 0, 0, 0)
            .BorderBrush(DynamicColors.Get("SemiColorBorder"))
            .Child(
                VStackPanel(Aligns.HCenter, Aligns.VCenter)
                    .Margin(8)
                    .Spacing(8)
                    .Children(
                        StatusBagde(),
                        IconButton(MIcon.Edit, classes: "Primary")
                            .Theme(StaticResource<ControlTheme>("BorderlessButton"))
                            .OnClick(_ => Page.EditRecord(Model)),
                        IconButton(MIcon.Delete, classes: "Danger")
                            .Theme(StaticResource<ControlTheme>("BorderlessButton"))
                            .OnClick(_ => Page.DeleteRecord(Model))
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
}
