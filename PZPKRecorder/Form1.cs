using Microsoft.AspNetCore.Components.WebView.WindowsForms;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor.Services;
using PZPKRecorder.Services;
using System.Reflection;

namespace PZPKRecorder;

public partial class Form1 : Form
{
    public Form1()
    {
        InitializeComponent();

        StartUp();
        ResumeWindow();

        var services = new ServiceCollection();
        services.AddWindowsFormsBlazorWebView();
        services.AddMudServices();
#if DEBUG
        services.AddBlazorWebViewDeveloperTools();
        // services.AddLogging();
#endif

        blazorWebView1.HostPage = "wwwroot\\index.html";
        blazorWebView1.Services = services.BuildServiceProvider();
        blazorWebView1.RootComponents.Add<Components.App>("#app");

        blazorWebView1.StartPath = "/records";
    }

    private void StartUp()
    {
        SqlLiteHandler.Instance.Initialize();
        Translate.Init();
    }
    private void ResumeWindow()
    {
        string? size = VariantService.GetVariant("window_size");
        if (size != null)
        {
            string[] sz = size.Split(',');
            Width = int.Parse(sz[0]);
            Height = int.Parse(sz[1]);
        }

        string? position = VariantService.GetVariant("window_position");
        if (position != null)
        {
            string[] ps = position.Split(',');
            int x = int.Parse(ps[0]);
            int y = int.Parse(ps[1]);

            Location = new Point(x, y);
        }

        Text = "PZ Recorder V" + Assembly.GetExecutingAssembly().GetName().Version?.ToString();
    }

    protected override void OnActivated(EventArgs e)
    {
        base.OnActivated(e);
        BroadcastService.Broadcast(BroadcastEventName.WindowActivated, string.Empty);
    }

    protected override void OnResizeEnd(EventArgs e)
    {
        base.OnResizeEnd(e);
        string size = $"{Width},{Height}";

        VariantService.SetVariant("window_size", size);
    }

    protected override void OnLocationChanged(EventArgs e)
    {
        base.OnLocationChanged(e);

        string position = $"{Location.X},{Location.Y}";
        VariantService.SetVariant("window_position", position);
    }

    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);

        Services.SqlLiteHandler.Instance.Dispose();
    }
}
