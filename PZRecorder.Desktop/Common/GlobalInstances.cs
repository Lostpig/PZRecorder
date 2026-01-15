using Avalonia.Controls.Notifications;
using Microsoft.Extensions.DependencyInjection;

namespace PZRecorder.Desktop.Common;

internal static class GlobalInstances
{
    private static MainWindow? _mainWindow;
    public static MainWindow MainWindow => _mainWindow ?? throw new InvalidOperationException("Not set MainWindow!");

    private static PzNotification? _notification;
    public static PzNotification Notification => _notification ?? throw new InvalidOperationException("Not set MainWindow!");
    public static void SetMainWindow(MainWindow mainWindow)
    {
        _mainWindow = mainWindow;

        var notificationManager = new WindowNotificationManager(mainWindow)
        {
            MaxItems = 3
        };
        _notification = new PzNotification(notificationManager);
    }

    private static ServiceProvider? _serviceProvider;
    public static ServiceProvider Services => _serviceProvider ?? throw new InvalidOperationException("Not set ServiceProvider!");
    public static void SetServiceProvider(ServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    private static SemiHelper? _semiHelper;
    public static SemiHelper Semi => _semiHelper ??= new SemiHelper();

    private static PageLocator? _pageLocator;
    public static PageLocator PageLocator => _pageLocator ?? throw new InvalidOperationException("Not set PageLocator!");
    public static void SetPageLocator(PageLocator pageLocator)
    {
        _pageLocator = pageLocator;
    }
}
