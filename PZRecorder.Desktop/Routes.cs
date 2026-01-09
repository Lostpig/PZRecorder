using Avalonia.Controls.Templates;
using PZRecorder.Desktop.Modules.Daily;
using PZRecorder.Desktop.Modules.Dev;
using PZRecorder.Desktop.Modules.Record;
using PZRecorder.Desktop.Modules.Shared;
using System.Reactive.Subjects;

namespace PZRecorder.Desktop;

internal class Routes
{
    static public readonly PageRecord[] Pages = [
        new PageRecord(() => "Records", "SemiIconExcel", typeof(RecordPage)),
        new PageRecord(() => "Kind", "SemiIconExport", typeof(KindPage)),
        new PageRecord(() => "Daily", "SemiIconExport", typeof(DailyPage)),
        new PageRecord(() => "DailyManager", "SemiIconExport", typeof(DailyManagerPage)),
        new PageRecord(() => "Dev", "SemiIconExport", typeof(DevGridPage)),
    ];
}

internal class PageRecord(Func<string> PageNameGetter, string icon, Type pageType)
{
    private readonly Func<string> _getter = PageNameGetter;
    public string Icon { get; init; } = icon;
    public Type PageType { get; init; } = pageType;
    public string PageName => _getter();
    public BehaviorSubject<string> Status { get; set; } = new("");
}
internal class PageRouter
{
    public readonly BehaviorSubject<PageRecord> CurrentPage;

    public PageRouter()
    {
        CurrentPage = new(Routes.Pages[0]);
    }
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

    private readonly Dictionary<Type, MvuPage> _views = [];
    private MvuPage? _current;

    public void Reset()
    {
        _views.Clear();
    }

    public MvuPage? GetPage(PageRecord pr)
    {
        if (_views.TryGetValue(pr.PageType, out var page))
        {
            return page;
        }

        if (pr.PageType.IsAssignableTo(typeof(MvuPage)))
        {
            page = Activator.CreateInstance(pr.PageType) as MvuPage;

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
