using Avalonia.Platform;
using System.Reflection;
using Ursa.Controls;

namespace PZRecorder.Desktop;

public class MainWindow : UrsaWindow
{
    PageRouter _router;
    public MainWindow() : base()
    {
        _router = new();
        var icon = AssetLoader.Open(new Uri($"avares://PZRecorder.Desktop/pz-recorder-icon.ico"));
        Icon = new WindowIcon(icon);

        Title = "PZRecorder Desktop V" + Assembly.GetExecutingAssembly().GetName().Version?.ToString();
#if DEBUG
        Title += " (Debug)";
#endif

        Width = 1280;
        Height = 720;
    }

    public void BuildContent()
    {
        Content = new MainView(_router);
    }

    protected void UpdateState()
    {
        // foreach (var item in _container.Items)
        // {
        //     if (item is TabItem tab)
        //     {
        //         tab.Header = ((PageRecord)tab.Content!).PageName;
        //     }
        // }
    }

    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
        Environment.Exit(0);
    }
}
