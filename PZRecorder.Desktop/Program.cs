using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Material.Icons.Avalonia;
using Microsoft.Extensions.DependencyInjection;
using PZRecorder.Core;
using PZRecorder.Core.Data;
using PZRecorder.Core.Managers;
using PZRecorder.Desktop.Common;
using PZRecorder.Desktop.Localization;
using PZRecorder.Desktop.Modules.Shared;
using Semi.Avalonia;

namespace PZRecorder.Desktop;

internal sealed class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args) => BuildAvaloniaApp(args);

    private static ServiceProvider CreatePZServices()
    {
        ServiceCollection services = new();

        SqlHandler sqlHandler = OpenDatabase();
        DailyManager dailyManager = new(sqlHandler);
        RecordManager recordManager = new(sqlHandler);
        ClockInManager clockInManager = new(sqlHandler);
        ProcessMonitorManager processMonitorManager = new(sqlHandler);
        ProcessMonitorService processMonitorService = new(processMonitorManager, dailyManager);
        TodoListManager todoListManager = new(sqlHandler);
        services.AddSingleton(sqlHandler);
        services.AddSingleton(dailyManager);
        services.AddSingleton(recordManager);
        services.AddSingleton(clockInManager);
        services.AddSingleton(processMonitorManager);
        services.AddSingleton(processMonitorService);
        services.AddSingleton(todoListManager);

        VariantsManager variantsManager = new(sqlHandler);
        ImportManager importManager = new(sqlHandler);
        ExportManager exportManager = new(sqlHandler);
        services.AddSingleton(variantsManager);
        services.AddSingleton(importManager);
        services.AddSingleton(exportManager);

        Logger logger = new();
        BroadcastManager broadcaster = new();
        ErrorProxy errorProxy = new(broadcaster, logger);
        Translate translate = new(errorProxy, broadcaster);
        PageRouter router = new();
        services.AddSingleton(logger);
        services.AddSingleton(broadcaster);
        services.AddSingleton(errorProxy);
        services.AddSingleton(translate);
        services.AddSingleton(router);

        return services.BuildServiceProvider();
    }
    private static SqlHandler OpenDatabase()
    {
        var dbPath = Utility.GetDataBasePath();
        if (File.Exists(dbPath))
        {
            return SqlHandler.Open(dbPath);
        }
        else
        {
            return SqlHandler.Create(dbPath);
        }
    }

    public static void BuildAvaloniaApp(string[] args)
    {
        var lifetime = new ClassicDesktopStyleApplicationLifetime { Args = args, ShutdownMode = ShutdownMode.OnLastWindowClose };
        var app = AppBuilder.Configure<Application>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace()
            .AfterSetup(b =>
            {
                b.Instance?.Styles.Add(new SemiTheme());
                b.Instance?.Styles.Add(new Ursa.Themes.Semi.SemiTheme());
                b.Instance?.Styles.Add(new MaterialIconStyles(null));
            })
#if DEBUG
            .UseHotReload()
#endif
            .SetupWithLifetime(lifetime);

        // if (OperatingSystem.IsWindows() || OperatingSystem.IsMacOS() || OperatingSystem.IsLinux())
        // {
        //     //app.UseManagedSystemDialogs();
        // }

        var serviceProvider = CreatePZServices();
        PageLocator pageLocator = new(serviceProvider);
        MainWindow mainWindow = new(serviceProvider);

        GlobalInstances.SetMainWindow(mainWindow);
        GlobalInstances.SetServiceProvider(serviceProvider);
        GlobalInstances.SetPageLocator(pageLocator);
        ValidtionMessags.SetValidtionMessags();

        lifetime.MainWindow = mainWindow;
#if DEBUG
        lifetime.MainWindow?.AttachDevTools();
#endif
        mainWindow.BuildContent();
        lifetime.Start(args);
    }
}
