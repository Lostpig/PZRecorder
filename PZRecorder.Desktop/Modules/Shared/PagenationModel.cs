using PZRecorder.Desktop.Extensions;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace PZRecorder.Desktop.Modules.Shared;

internal class OrderedPagenationModel<T, OrderT> : PagenationModel
{
    public Subject<IEnumerable<T>> Items { get; init; } = new();
    public IObservable<IEnumerable<T>> PageItems { get; init; }
    public BehaviorSubject<OrderT> Order { get; init; }

    public OrderedPagenationModel(Func<IEnumerable<T>, OrderT, IOrderedEnumerable<T>> orderFunc, OrderT initOrder) : base()
    {
        Order = new(initOrder);
        Items.Subscribe(x => TotalCount.OnNext(x.Count()));
        PageItems = Items
            .CombineLatest(Order, orderFunc)
            .CombineLatest(CurrentRange, (items, range) => items.Take(range));
    }

    public void Update(IEnumerable<T> items)
    {
        Page.OnNext(1);
        Items.OnNext(items);
    }
}
internal class PagenationModel<T> : PagenationModel
{
    public Subject<IEnumerable<T>> Items { get; init; } = new();
    public IObservable<IEnumerable<T>> PageItems { get; init; }

    public PagenationModel() : base()
    {
        PageItems = CurrentRange.CombineLatest(Items, (range, items) =>
        {
            return items.Take(range);
        });
        Items.Subscribe(x => TotalCount.OnNext(x.Count()));
    }
    public void Update(IEnumerable<T> items)
    {
        Page.OnNext(1);
        Items.OnNext(items); 
    }
}

internal class PagenationModel
{
    public BehaviorSubject<int> Page { get; init; } = new(1);
    public BehaviorSubject<int> PageSize { get; init; } = new(15);
    public BehaviorSubject<int> TotalCount { get; init; } = new(0);
    public IObservable<Range> CurrentRange { get; init; }

    public PagenationModel()
    {
        PageSize.PairWithPrevious().Subscribe(PageSizeChanged);
        CurrentRange = Page.CombineLatest(PageSize, PageRange);
    }

    private Range PageRange(int page, int pageSize)
    {
        if (page <= 0) return new Range(0, 0);

        var size = PageSize.Value;
        return new Range((page - 1) * size, page * size);
    }
    private void PageSizeChanged(Tuple<int, int> sizes)
    {
        var (prev, curr) = sizes;

        var start = Page.Value * prev;
        var newPage = start / curr;
        if (newPage <= 0) newPage = 1;

        Page.OnNext(newPage);
    }
}
