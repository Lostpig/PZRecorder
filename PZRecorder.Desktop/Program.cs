using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Notifications;
using Material.Icons.Avalonia;
using Microsoft.Extensions.DependencyInjection;
using PZRecorder.Core;
using PZRecorder.Core.Managers;
using PZRecorder.Desktop.Common;
using Semi.Avalonia;

namespace PZRecorder.Desktop;

internal sealed class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args) => BuildAvaloniaApp(args);

    private static void AddPZServices(ServiceCollection services)
    {
        SqlHandler sqlHandler = OpenDatabase();
        DailyManager dailyManager = new(sqlHandler);
        RecordManager recordManager = new(sqlHandler);
        ClockInManager clockInManager = new(sqlHandler);
        ProcessMonitorManager processMonitorManager = new(sqlHandler);
        ProcessMonitorService processMonitorService = new(processMonitorManager, dailyManager);

        services.AddSingleton(sqlHandler);
        services.AddSingleton(dailyManager);
        services.AddSingleton(recordManager);
        services.AddSingleton(clockInManager);
        services.AddSingleton(processMonitorManager);
        services.AddSingleton(processMonitorService);
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
        ServiceCollection services = new();
        // pzrecorder services
        AddPZServices(services);

        var app = AppBuilder.Configure<Application>()
            .UsePlatformDetect()
            .WithInterFont()
            // .UseServiceProvider(serviceProvider)
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

        var mainWindow = new MainWindow();
        var notificationManager = new WindowNotificationManager(mainWindow)
        {
            MaxItems = 3
        };
        var pzNotification = new PzNotification(notificationManager);
        services.AddSingleton(pzNotification);

        lifetime.MainWindow = mainWindow;

        var serviceProvider = services.BuildServiceProvider();
        GlobalInstances.SetMainWindow(mainWindow);
        GlobalInstances.SetServiceProvider(serviceProvider);
#if DEBUG
        lifetime.MainWindow?.AttachDevTools();
#endif
        mainWindow.BuildContent();
        lifetime.Start(args);
    }
}
