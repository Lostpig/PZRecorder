using Avalonia.Threading;
using PZ.RxAvalonia.Reactive;
using PZRecorder.Desktop.Extensions;
using System.Diagnostics.CodeAnalysis;

namespace PZRecorder.Desktop.Modules.Shared;

internal interface IListItemComponent<T>
{
    public void UpdateItem(T item);
}

internal class CachedList<TControl, TState> : ContentControl
    where TControl : Control, IListItemComponent<TState>
{
    private readonly ReactiveList<TState> _items;
    private Panel _itemsPanel;
    private readonly ScrollViewer _container;
    private Func<TControl>? _itemCreator;

    public CachedList(ReactiveList<TState> items) : base()
    {
        _items = items;
        _container = new ScrollViewer();
        ItemsPanel(VStackPanel(Aligns.Top));

        Content = _container;
        _items.Subscribe(_ => UpdateItems());
    }

    [MemberNotNull(nameof(_itemsPanel))]
    public CachedList<TControl, TState> ItemsPanel(Panel itemsPanel)
    {
        _itemsPanel = itemsPanel;
        _container.Content = _itemsPanel;
        return this;
    }
    public CachedList<TControl, TState> ItemCreator(Func<TControl> creator)
    {
        _itemCreator = creator;
        return this;
    }

    private TControl GetControl(int index)
    {
        if (_itemCreator is null) throw new Exception("ItemTemplete not set!");

        if (_itemsPanel.Children.Count > index) return (TControl)_itemsPanel.Children[index]!;
        var control = _itemCreator();
        _itemsPanel.Children.Add(control);

        return control;
    }

    private void UpdateItems()
    {
        if (Dispatcher.UIThread.CheckAccess())
        {
            RenderItems();
        }
        else
        {
            Dispatcher.UIThread.Post(RenderItems, DispatcherPriority.Normal);
        }
    }
    private void RenderItems()
    {
        var index = 0;
        foreach (var item in _items)
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
