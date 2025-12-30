using Avalonia.Platform;
using SukiUI.Controls;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace PZRecorder.Desktop;

using static PZRecorder.Desktop.Common.ControlHeplers;

public class MainWindow : SukiWindow
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

        Initialize();
    }

    private SukiSideMenu Sidemenu;
    [MemberNotNull(nameof(Sidemenu))]
    private void Initialize()
    {
        Sidemenu = new SukiSideMenu()
        {
            IsSearchEnabled = false,
        };

        foreach (var p in Routes.Pages)
        {
            var item = new SukiSideMenuItem()
            {
                Icon = MaterialIcon(p.Icon),
                Header = p.PageName,
                PageContent = p,
            };

            Sidemenu.Items.Add(item);
        }

        Content = Sidemenu;
    }

    protected void UpdateState()
    {
        foreach (var item in Sidemenu.Items)
        {
            if (item is SukiSideMenuItem ssmi)
            {
                ssmi.Header = ((PageRecord)ssmi.PageContent).PageName;
            }
        }
    }

    public void DebugReRender()
    {
        Sidemenu.Items.Clear();
        Sidemenu.Content = null;
        PageLocator.Instance.Reset();

        Sidemenu = new SukiSideMenu()
        {
            IsSearchEnabled = false
        };

        foreach (var p in Routes.Pages)
        {
            var item = new SukiSideMenuItem()
            {
                Icon = MaterialIcon(p.Icon),
                Header = p.PageName,
                PageContent = p
            };

            Sidemenu.Items.Add(item);
        }

        Content = Sidemenu;
    }

    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
        Environment.Exit(0);
    }
}
