using Microsoft.Extensions.DependencyInjection;

namespace PZRecorder.Desktop.Common;

internal static class GlobalInstances
{
    private static MainWindow? _mainWindow;
    public static MainWindow GetMainWindow()
    {
        if (_mainWindow == null)
            throw new InvalidOperationException("Not set MainWindow!");

        return _mainWindow;
    }
    public static void SetMainWindow(MainWindow mainWindow)
    {
        _mainWindow = mainWindow;
    }

    private static ServiceProvider? _serviceProvider;
    public static ServiceProvider GetServiceProvider()
    {
        if (_serviceProvider == null)
            throw new InvalidOperationException("Not set ServiceProvider!");

        return _serviceProvider;
    }
    public static void SetServiceProvider(ServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    private static SukiHelpers? _sukiHelper;
    public static SukiHelpers GetSukiHelpers()
    {
        if (_sukiHelper == null)
            throw new InvalidOperationException("Not set SukiHelpers!");

        return _sukiHelper;
    }
    public static void SetSukiHelpers(SukiHelpers sukiHelper)
    {
        _sukiHelper = sukiHelper;
    }

}
