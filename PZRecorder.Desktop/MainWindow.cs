using Avalonia.Platform;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using PZRecorder.Core.Managers;
using PZRecorder.Desktop.Common;
using PZRecorder.Desktop.Modules.Shared;
using System.Diagnostics;
using System.Reactive.Linq;
using System.Reflection;
using Ursa.Controls;

namespace PZRecorder.Desktop;

internal class MainWindow : UrsaWindow
{
    private readonly VariantsManager variantsManager;
    private readonly BroadcastManager broadcaster;
    private readonly BackgroundService backgroundService;

    public MainWindow(IServiceProvider serviceProvider) : base()
    {
        var icon = AssetLoader.Open(new Uri($"avares://PZRecorder.Desktop/pz-recorder-icon.ico"));
        Icon = new WindowIcon(icon);

        MinHeight = 720;
        MinWidth = 1280;

        variantsManager = serviceProvider.GetRequiredService<VariantsManager>();
        broadcaster = serviceProvider.GetRequiredService<BroadcastManager>();

        Activated += OnActivated;
        Observable
            .FromEventPattern<EventHandler<WindowResizedEventArgs>, WindowResizedEventArgs>(h => Resized += h, h => Resized -= h)
            .Throttle(TimeSpan.FromSeconds(0.3))
            .Subscribe(_ => Dispatcher.UIThread.Post(WindowResized));
        Observable
            .FromEventPattern<EventHandler<PixelPointEventArgs>, PixelPointEventArgs>(h => PositionChanged += h, h => PositionChanged -= h)
            .Throttle(TimeSpan.FromSeconds(0.3))
            .Subscribe(e => OnPositionChanged(e.EventArgs));

        var monitorService = serviceProvider.GetRequiredService<ProcessMonitorService>();
        backgroundService = new(broadcaster, monitorService);
    }

    protected override void OnOpened(EventArgs e)
    {
        base.OnOpened(e);
        ApplyVariants();
    }
    private void ApplyVariants()
    {
        var size = variantsManager.GetVariant("window_size");
        if (!string.IsNullOrWhiteSpace(size))
        {
            string[] sz = size.Split(',');
            if (sz.Length == 2)
            {
                Width = double.TryParse(sz[0], out var w) ? w : 1280;
                Height = double.TryParse(sz[1], out var h) ? h : 720;
            }
        }

        var position = variantsManager.GetVariant("window_position");
        if (string.IsNullOrWhiteSpace(position))
        {
            string[] ps = position.Split(',');
            if (ps.Length == 2 && int.TryParse(ps[0], out var x) && int.TryParse(ps[1], out var y))
            {
                Position = new(x, y);
            }
        }

        string? maximize = variantsManager.GetVariant("window_maximize");
        if (maximize == "1")
        {
            WindowState = WindowState.Maximized;
        }
    }
    private void WindowResized()
    {
        string size = $"{Width},{Height}";
        variantsManager.SetVariant("window_size", size);

        var maximized = WindowState == WindowState.Maximized ? "1" : "0";
        variantsManager.SetVariant("window_maximize", maximized);

        Debug.WriteLine($"window resized: size = {size}, maximize = {maximized}");
    }
    private void OnPositionChanged(PixelPointEventArgs e)
    {
        string position = $"{e.Point.X},{e.Point.Y}";
        variantsManager.SetVariant("window_position", position);

        Debug.WriteLine($"window positoon changed: {position}");
    }
    private void OnActivated(object? sender, EventArgs e)
    {
        broadcaster.Publish(BroadcastEvent.WindowActivated);
    }

    public void BuildContent()
    {
        Content = new MainView();
    }
    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);

        backgroundService.Dispose();
        Environment.Exit(0);
    }
}
