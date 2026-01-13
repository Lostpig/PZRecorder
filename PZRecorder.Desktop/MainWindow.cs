using Avalonia.Platform;
using System.Reflection;
using Ursa.Controls;

namespace PZRecorder.Desktop;

internal class MainWindow : UrsaWindow
{
    public MainWindow() : base()
    {
        var icon = AssetLoader.Open(new Uri($"avares://PZRecorder.Desktop/pz-recorder-icon.ico"));
        Icon = new WindowIcon(icon);

        Title = "PZRecorder Desktop V" + Assembly.GetExecutingAssembly().GetName().Version?.ToString();
#if DEBUG
        Title += " (Debug)";
#endif

        Width = 1280;
        Height = 720;
        MinHeight = 720;
        MinWidth = 1280;
    }

    public void BuildContent(PageRouter router)
    {
        Content = new MainView(router);
    }

    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
        Environment.Exit(0);
    }
}
