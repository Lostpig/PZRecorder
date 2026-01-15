using PZ.RxAvalonia.Reactive;
using PZRecorder.Desktop.Extensions;
using System.Diagnostics.CodeAnalysis;
using System.Reactive.Linq;

namespace PZRecorder.Desktop.Modules.Shared;

internal class PagenationList<TControl, TState> : MvuComponent
    where TControl : Control, IListItemComponent<TState>
{
    protected MvuPagenationState Pagenation { get; set; } = new();
    private readonly IList<TState> _items;
    private Panel _itemsPanel;
    private readonly ScrollViewer _container;
    private Func<TControl>? _itemCreator;

    public PagenationList(IList<TState> items) : base()
    {
        _items = items;
        _container = new ScrollViewer();
        ItemsPanel(VStackPanel(Aligns.Top));

        Initialize();
    }

    protected override Control Build()
    {
        return new DockPanel()
            .Children(
                new Uc.Pagination() { ShowPageSizeSelector = false, ShowQuickJump = true }
                    .Dock(Dock.Bottom)
                    .Margin(0, 4, 0, 0)
                    .WithState(() => Pagenation)
                    .OnPageChanged(OnPageChanged),
                _container.Dock(Dock.Top)
            );
    }
    protected override IEnumerable<IDisposable> WhenActivate()
    {
        if (_items is ReactiveList<TState> rxList)
        {
            return [rxList.Subscribe(_ => UpdateItems())];
        }

        UpdateItems();
        return base.WhenActivate();
    }

    [MemberNotNull(nameof(_itemsPanel))]
    public PagenationList<TControl, TState> ItemsPanel(Panel itemsPanel)
    {
        _itemsPanel = itemsPanel;
        _container.Content = _itemsPanel;
        return this;
    }
    public PagenationList<TControl, TState> ItemCreator(Func<TControl> creator)
    {
        _itemCreator = creator;
        return this;
    }

    public void UpdateItems()
    {
        Pagenation = Pagenation with { TotalCount = _items.Count, Page = 1 };
        RenderItems();
        UpdateState();
    }
    private void OnPageChanged(Uc.ValueChangedEventArgs<int> e)
    {
        Pagenation = Pagenation with { Page = e.NewValue ?? 1 };
        RenderItems();
    }

    private TControl GetControl(int index)
    {
        if (_itemCreator is null) throw new Exception("ItemTemplete not set!");

        if (_itemsPanel.Children.Count > index) return (TControl)_itemsPanel.Children[index]!;
        var control = _itemCreator();
        _itemsPanel.Children.Add(control);

        return control;
    }
    private void RenderItems()
    {
        var pageItems = _items.Take(Pagenation.PageRange);
        var index = 0;
        foreach(var item in pageItems)
        {
            var control = GetControl(index);
            control.IsVisible = true;
            control.UpdateItem(item);
            index++;
        }

        for (var i = index; i < _itemsPanel.Children.Count; i++)
        {
            _itemsPanel.Children[i].IsVisible = false;
        }
    }
}
