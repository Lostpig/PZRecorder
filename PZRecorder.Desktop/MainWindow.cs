using Avalonia.Platform;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using PZRecorder.Desktop.Common;
using System.Diagnostics;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reflection;
using Ursa.Controls;

namespace PZRecorder.Desktop;

internal class MainWindow : UrsaWindow
{
    private readonly VariantsManager variantsManager;
    private readonly BroadcastManager broadcaster;

    public MainWindow(IServiceProvider serviceProvider) : base()
    {
        var icon = AssetLoader.Open(new Uri($"avares://PZRecorder.Desktop/pz-recorder-icon.ico"));
        Icon = new WindowIcon(icon);

        Title = "PZRecorder Desktop V" + Assembly.GetExecutingAssembly().GetName().Version?.ToString();
#if DEBUG
        Title += " (Debug)";
#endif

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
    }

    protected override void OnOpened(EventArgs e)
    {
        base.OnOpened(e);
        ApplyVariants();
    }
    private void ApplyVariants()
    {
        var size = variantsManager.GetVariant("window_size");
        if (size != null)
        {
            string[] sz = size.Split(',');
            Width = double.Parse(sz[0]);
            Height = double.Parse(sz[1]);
        }

        var position = variantsManager.GetVariant("window_position");
        if (position != null)
        {
            string[] ps = position.Split(',');
            int x = int.Parse(ps[0]);
            int y = int.Parse(ps[1]);

            Position = new(x, y);
        }

        string? maximize = variantsManager.GetVariant("window_maximize");
        if (maximize is not null && maximize == "1")
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
        Environment.Exit(0);
    }
}
