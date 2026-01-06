using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Avalonia.Styling;
using Microsoft.Extensions.DependencyInjection;
using PZ.RxAvalonia.Reactive;
using PZRecorder.Core.Managers;
using PZRecorder.Core.Tables;
using PZRecorder.Desktop.Extensions;
using PZRecorder.Desktop.Modules.Shared;
using System.Diagnostics.CodeAnalysis;
using System.Reactive.Linq;
using System.Reflection;


namespace PZRecorder.Desktop.Modules.Record;

using TbRecord = Core.Tables.Record;

internal class RecordPageModel
{
    public List<Kind> Kinds { get; set; } = [];
    public RecordsQuery Query { get; set; } = new();
    public MvuPagenationState Pagenation { get; set; } = new();
    public ReactiveList<TbRecord> Items { get; init; } = [];
    public RecordSort Order { get; set; } = RecordSort.ModifyTimeDesc;
    public int SelectedKind { get; set; } = 0;
}

internal class RecordPage : MvuPage
{
    #region templete
    private TabStrip BuildTabs()
    {
        return new TabStrip()
            .Theme(StaticResource<ControlTheme>("ScrollLineTabStrip"))
            .ItemsSource(() => Model.Kinds)
            .ItemTemplate<Kind, TabStrip>(k => PzText(k.Name))
            .SelectedIndex(() => Model.SelectedKind)
            .OnSelectionChanged(OnSelectKind);
    }
    private PagenationControls<RecordItem, TbRecord> BuildList()
    {
        var pagedList = new PagenationControls<RecordItem, TbRecord>(Model.Items)
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
                PzTextBox(() => Model.Query.SearchText)
                    .OnTextChanged(OnSearchChanged)
                    .Margin(16, 8)
                    .Watermark("Search..."),
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
                        .SelectedValue(() => Model.Query.State)
                        .OnSelectionChanged(OnStateChanged)
                ),
                new Border().Theme(StaticResource<ControlTheme>("CardBorder"))
                .Child(
                    VStackPanel()
                        .Styles(new Style<ComboBox>().Width(240))
                        .Spacing(8)
                        .Children(
                            PzText("Year"),
                            new ComboBox()
                                .ItemsSource(() => Years)
                                .ItemTemplate<int, ComboBox>(n => PzText(n > 0 ? n.ToString() : "-"))
                                .SelectedValue(() => Model.Query.Year)
                                .OnSelectionChanged(OnYearChanged),
                            PzText("Month"),
                            new ComboBox()
                                .ItemsSource(Months)
                                .ItemTemplate<int, ComboBox>(n => PzText(n > 0 ? n.ToString() : "-"))
                                .SelectedValue(() => Model.Query.Month)
                                .OnSelectionChanged(OnMonthChanged),
                            PzText("Rating"),
                            new ComboBox()
                                .ItemsSource(Ratings)
                                .ItemTemplate<int, ComboBox>(n => PzText(n > 0 ? n.ToString() : "-"))
                                .SelectedValue(() => Model.Query.Rating)
                                .OnSelectionChanged(OnRatingChanged),
                            PzText("Order"),
                            new ComboBox()
                                .ItemsSource(Enum.GetValues<RecordSort>())
                                .ItemTemplate<RecordSort, ComboBox>(n => PzText(n.ToString()))
                                .SelectedValue(() => Model.Order)
                                .OnSelectionChanged(OnOrderChanged)
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

    #region event handlers
    private void OnSelectKind(SelectionChangedEventArgs e)
    {
        if (e.Source is SelectingItemsControl c && c.SelectedValue is Kind k)
        {
            Model.Query = Model.Query with { KindId = k.Id };
            UpdateItems();
        }
    }
    private void OnSearchChanged(TextChangedEventArgs e)
    {
        var text = ((TextBox)e.Source!).Text ?? "";
        Model.Query = Model.Query with { SearchText = text };
        UpdateItems();
    }
    private void OnStateChanged(SelectionChangedEventArgs e)
    {
        if (e.Source is ListBox l && l.SelectedValue is RecordState s)
        {
            Model.Query = Model.Query with { State = s };
        }
        else
        {
            Model.Query = Model.Query with { State = null };
        }
        UpdateItems();
    }
    private void OnYearChanged(SelectionChangedEventArgs e)
    {
        if (e.Source is ComboBox c && c.SelectedValue is int x)
        {
            Model.Query = Model.Query with { Year = x };
            UpdateItems();
        }
    }
    private void OnMonthChanged(SelectionChangedEventArgs e)
    {
        if (e.Source is ComboBox c && c.SelectedValue is int x)
        {
            Model.Query = Model.Query with { Month = x };
            UpdateItems();
        }
    }
    private void OnRatingChanged(SelectionChangedEventArgs e)
    {
        if (e.Source is ComboBox c && c.SelectedValue is int x)
        {
            Model.Query = Model.Query with { Rating = x };
            UpdateItems();
        }
    }
    private void OnOrderChanged(SelectionChangedEventArgs e)
    {
        if (e.Source is ComboBox c && c.SelectedValue is RecordSort s)
        {
            Model.Order = s;
            UpdateState();
        }
    }
    #endregion

    private readonly RecordManager _manager;
    protected RecordPageModel Model { get; set; }
    private int[] Years = [-1];
    private readonly int[] Months = [-1, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12];
    private readonly int[] Ratings = [-1, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10];

    public RecordPage() : base()
    {
        Model = new();
        _manager = ServiceProvider.GetRequiredService<RecordManager>();
        Initialize();
    }
    protected override IEnumerable<IDisposable> WhenActivate()
    {
        // TODO: 使用消息事件，在KindPage进行更新后自动更新，不再在每次进入页面时更新
        var kindId = Model.Query.KindId;
        Model.Kinds = _manager.GetKinds();
        UpdateState();

        Kind? kind = null;
        if (kindId > 0)
        {
            kind = Model.Kinds.FirstOrDefault(k => k.Id == kindId);
        }
        
        if (kind == null)
        {
            kind = Model.Kinds.FirstOrDefault();
        }

        Model.Query = Model.Query with { KindId = kind?.Id ?? -1 };
        UpdateItems();
        return base.WhenActivate();
    }


    private RecordsQuery? _lastQuery;
    [MemberNotNullWhen(true, nameof(_lastQuery))]
    private bool EqualQuery(RecordsQuery query)
    {
        if (_lastQuery == null) return false;

        return _lastQuery.KindId == query.KindId
            && _lastQuery.SearchText == query.SearchText
            && _lastQuery.Year == query.Year
            && _lastQuery.Month == query.Month
            && _lastQuery.State == query.State
            && _lastQuery.Rating == query.Rating;
    }
    private void UpdateItems(bool force = false)
    {
        if (Model.Query.KindId < 0) return;
        if (EqualQuery(Model.Query) && !force) return;

        if (Model.Query.KindId != _lastQuery?.KindId || force)
        {
            Years = _manager.GetYears(Model.Query.KindId).ToArray();
        }
        if (!Years.Contains(Model.Query.Year))
        {
            Model.Query = Model.Query with { Year = -1 };
        }

        var items = _manager.QueryRecords(Model.Query);
        Model.Items.Clear();
        Model.Items.AddRange(items);
        _lastQuery = Model.Query;
        Model.SelectedKind = Model.Kinds.FindIndex(k => k.Id == Model.Query.KindId);

        UpdateState();
    }

    internal async void AddRecord()
    {
        if (Model.Query.KindId < 0)
        {
            await PzDialogManager.Alert("Kind not selected!", "Error");
            return;
        }

        var res = await PzDialogManager.ShowDialog(new RecordDialog(Model.Query.KindId));
        if (PzDialogManager.IsSureResult(res.Result))
        {
            _manager.InsertRecord(res.Value);
            UpdateItems(true);
        }
    }
    internal async void EditRecord(TbRecord item)
    {
        var res = await PzDialogManager.ShowDialog(new RecordDialog(item));
        if (PzDialogManager.IsSureResult(res.Result))
        {
            _manager.UpdateRecord(res.Value);
            UpdateItems(true);
        }
    }
    internal async void DeleteRecord(TbRecord item)
    {
        var dialog = PzDialogManager.ConfirmDialog("Delete", "Sure to delete?");
        dialog.Mode = Uc.DialogMode.Question;
        dialog.BoxButtons[0].Text = "Delete";
        dialog.BoxButtons[0].Styles = ["Danger"];

        var delete = await PzDialogManager.ShowDialog(dialog);
        if (PzDialogManager.IsSureResult(delete.Result))
        {
            _manager.DeleteRecord(item.Id);
            UpdateItems(true);
        }
    }
}

internal class RecordItem(RecordPage Page) : MvuComponent, IPageItemComponent<TbRecord>
{
    protected TbRecord State { get; set; } = new();
    public void UpdateItem(TbRecord item)
    {
        State = item;
        UpdateState();
    }
    protected override Control Build()
    {
        var content = VStackPanel()
            .Margin(0, 0, 8, 0)
            .Spacing(8)
            .TextBlock_TextWrapping(TextWrapping.Wrap)
            .Children(
                PzText(() => State.Name)
                    .Align(Aligns.VCenter)
                    .FontSize(20)
                    .Foreground(StaticColor("SemiColorLink")),
                PzText(() => State.Alias)
                    .Margin(0, -8, 0, 0)
                    .Align(Aligns.VCenter)
                    .Foreground(StaticColor("SemiColorText2")),
                new DockPanel()
                    .LastChildFill(false)
                    .HorizontalSpacing(8)
                    .Children(
                        PzText(() => $"{State.Episode} / {State.EpisodeCount}").Dock(Dock.Left),
                        PzText(() => $"{State.PublishYear}-{State.PublishMonth}").Dock(Dock.Left),
                        PzText(() => $"{State.ModifyDate:yyyy-MM-dd HH:mm}").Dock(Dock.Right)
                    ),
                PzText(() => State.Remark)
                    .FontSize(14)
                    .Foreground(StaticColor("SemiColorText3")),
                new Uc.Rating() { Count = 10 }
                    .DefaultValue(() => State.Rating)
                    .Classes("Small")
                    .IsEnabled(false)
            );

        var stateColor = State.State switch
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
                            .Content(() => State.State.ToString())
                            .Align(Aligns.HCenter),
                        IconButton(MIcon.Edit, classes: "Primary")
                            .Theme(StaticResource<ControlTheme>("BorderlessButton"))
                            .OnClick(_ => Page.EditRecord(State)),
                        IconButton(MIcon.Delete, classes: "Danger")
                            .Theme(StaticResource<ControlTheme>("BorderlessButton"))
                            .OnClick(_ => Page.DeleteRecord(State))
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
