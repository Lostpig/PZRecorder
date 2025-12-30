using Avalonia.Controls.Templates;
using Material.Icons;
using PZRecorder.Desktop.Common;
using PZRecorder.Desktop.Daily;
using PZRecorder.Desktop.Dev;
using PZRecorder.Desktop.Record;

namespace PZRecorder.Desktop;

internal class Routes
{
    static public readonly PageRecord[] Pages = [
        new PageRecord(() => "Records", MaterialIconKind.ViewGallery, typeof(RecordPage)),
        new PageRecord(() => "Kind", MaterialIconKind.ViewGallery, typeof(KindPage)),
        new PageRecord(() => "Daily", MaterialIconKind.ViewGallery, typeof(DailyPage)),
        new PageRecord(() => "Dev", MaterialIconKind.DeveloperBoard, typeof(DevGridPage)),
    ];
}

internal class PageRecord(Func<string> PageNameGetter, MaterialIconKind icon, Type pageType)
{
    private readonly Func<string> _getter = PageNameGetter;
    public MaterialIconKind Icon { get; init; } = icon;
    public Type PageType { get; init; } = pageType;
    public string PageName => _getter();

}
internal class PageLocator : IDataTemplate
{
    private static PageLocator? _instance;
    public static PageLocator Instance
    {
        get
        {
            _instance ??= new PageLocator();
            return _instance;
        }
    }
    private PageLocator() { }

    private readonly Dictionary<Type, PZPageBase> _views = [];
    private PZPageBase? _current;

    public void Reset()
    {
        _views.Clear();
    }

    private PZPageBase? GetPage(PageRecord pr)
    {
        if (_views.TryGetValue(pr.PageType, out var page))
        {
            return page;
        }

        if (pr.PageType.IsAssignableTo(typeof(PZPageBase)))
        {
            page = Activator.CreateInstance(pr.PageType) as PZPageBase;

            if (page is not null)
            {
                _views.Add(pr.PageType, page);
                return page;
            }
        }

        return null;
    }
    public Control Build(object? param)
    {
        if (_current != null) _current.OnRouteExit();
        if (param is PageRecord pr)
        {
            var page = GetPage(pr);
            if (page is not null)
            {
                page.OnRouteEnter();
                _current = page;
                return page;
            }
        }

        _current = null;
        return new TextBlock() { Text = "create page param error." };
    }

    public bool Match(object? data) => data is PageRecord;

}
