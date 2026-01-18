using Avalonia.Controls.Templates;
using Microsoft.Extensions.DependencyInjection;
using PZRecorder.Desktop.Modules.ClockIn;
using PZRecorder.Desktop.Modules.Daily;
using PZRecorder.Desktop.Modules.Dev;
using PZRecorder.Desktop.Modules.Monitor;
using PZRecorder.Desktop.Modules.Record;
using PZRecorder.Desktop.Modules.Settings;
using PZRecorder.Desktop.Modules.Shared;
using System.Reactive.Subjects;

namespace PZRecorder.Desktop;

internal class Routes
{
    static public readonly PageRecord[] Pages = [
        new PageRecord("Record", () => LD.Record, MIcon.Record, typeof(RecordPage)),
        new PageRecord("KindManager", () => LD.KindManager, MIcon.Class, typeof(KindPage)),
        new PageRecord("Daily", () => LD.Daily, MIcon.Calendar, typeof(DailyPage)),
        new PageRecord("DailyManager", () => LD.DailyManager, MIcon.CalendarAccount, typeof(DailyManagerPage)),
        new PageRecord("Monitor", () => LD.ProcessWatcher, MIcon.Monitor, typeof(MonitorPage)),
        new PageRecord("ClockIn", () => LD.ClockIn, MIcon.ClockIn, typeof(ClockInPage)),
        new PageRecord("Setting", () => LD.Setting, MIcon.Settings, typeof(SettingsPage)),
        new PageRecord("Dev", () => "Dev", MIcon.DeveloperBoard, typeof(DevGridPage)),
    ];
}

internal class PageRecord(string key, Func<string> pageNameGetter, MIcon icon, Type pageType)
{
    public string Key { get; init; } = key;
    private readonly Func<string> _getter = pageNameGetter;
    public MIcon Icon { get; init; } = icon;
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

    public static PageRecord? GetPageRecord(string pageName)
    {
        return Routes.Pages.FirstOrDefault(p => p.Key == pageName);
    }
    public void RouteTo(PageRecord record)
    {
        CurrentPage.OnNext(record);
    }
    public void RouteTo(string pageName)
    {
        var p = GetPageRecord(pageName);
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
    public MvuPage? GetPage(PageRecord pr)
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
