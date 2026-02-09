using Avalonia.Controls.Templates;
using Microsoft.Extensions.DependencyInjection;
using PZRecorder.Desktop.Modules.ClockIn;
using PZRecorder.Desktop.Modules.Daily;
using PZRecorder.Desktop.Modules.Dev;
using PZRecorder.Desktop.Modules.Monitor;
using PZRecorder.Desktop.Modules.Record;
using PZRecorder.Desktop.Modules.Settings;
using PZRecorder.Desktop.Modules.Shared;
using PZRecorder.Desktop.Modules.TodoList;
using System.Reactive.Subjects;

namespace PZRecorder.Desktop;

internal class Routes
{
    static public readonly NavItem[] Pages = [
        new("Record", () => LD.Record, MIcon.StarBoxMultiple, typeof(RecordPage)),
        new("KindManager", () => LD.KindManager, MIcon.Class, typeof(KindPage)),
        NavItem.Separator,
        new("Daily", () => LD.Daily, MIcon.CalendarMultiselect, typeof(DailyPage)),
        new("DailyManager", () => LD.DailyManager, MIcon.CalendarEdit, typeof(DailyManagerPage)),
        new("Monitor", () => LD.ProcessWatcher, MIcon.Monitor, typeof(MonitorPage)),
        NavItem.Separator,
        new("ClockIn", () => LD.ClockIn, MIcon.TextBoxCheckOutline, typeof(ClockInPage)),
        new("TodoList", () => LD.TodoList, MIcon.ToDo, typeof(TodoListPage)),
        NavItem.Separator,
        new("Setting", () => LD.Setting, MIcon.Settings, typeof(SettingsPage)),
#if DEBUG
        new("Dev", () => "Dev", MIcon.DeveloperBoard, typeof(DevGridPage)),
#endif
    ];
}

internal class NavItem(string key, Func<string> pageNameGetter, MIcon icon, Type pageType)
{
    public bool IsSeparator { get; init; } = key == "Separator";
    public string Key { get; init; } = key;
    private readonly Func<string> _getter = pageNameGetter;
    public MIcon Icon { get; init; } = icon;
    public Type PageType { get; init; } = pageType;
    public string PageName => _getter();
    public BehaviorSubject<string> Status { get; init; } = new("");

    public static NavItem Separator = new("Separator", () => "", MIcon.Block, null);
}

internal class PageRouter
{
    public readonly BehaviorSubject<NavItem> CurrentPage;

    public PageRouter()
    {
        CurrentPage = new(Routes.Pages[0]);
    }

    public static NavItem? GetNavItem(string pageName)
    {
        return Routes.Pages.FirstOrDefault(p => p.Key == pageName);
    }
    public void RouteTo(NavItem record)
    {
        CurrentPage.OnNext(record);
    }
    public void RouteTo(string pageName)
    {
        var p = GetNavItem(pageName);
        if (p != null)
        {
            RouteTo(p);
        }
    }
}

internal class PageLocator : IDataTemplate
{
    private readonly Dictionary<Type, MvuPage> _views = [];
    private MvuPage? _current;
    private IServiceProvider _serviceProvider;

    public PageLocator(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public void Reset()
    {
        _views.Clear();
    }
    public MvuPage? GetPage(NavItem pr)
    {
        if (_views.TryGetValue(pr.PageType, out var page))
        {
            return page;
        }

        if (pr.PageType.IsAssignableTo(typeof(MvuPage)))
        {
            page = ActivatorUtilities.CreateInstance(_serviceProvider, pr.PageType) as MvuPage;

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
        _current?.OnRouteExit();
        if (param is NavItem pr)
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

    public bool Match(object? data) => data is NavItem;

}
