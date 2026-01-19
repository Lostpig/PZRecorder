using Avalonia.Styling;
using Material.Icons.Avalonia;
using Microsoft.Extensions.DependencyInjection;
using PZRecorder.Core.Managers;
using PZRecorder.Desktop.Common;
using PZRecorder.Desktop.Extensions;
using PZRecorder.Desktop.Localization;
using PZRecorder.Desktop.Modules.Shared;
using System.Reactive.Linq;
using System.Reflection;
using Ursa.Controls;

namespace PZRecorder.Desktop;

internal class MainView: MvuComponent
{
    private readonly PageRouter _router;
    private readonly Translate _translate;
    private readonly ErrorProxy _errorProxy;
    private readonly ClockInManager _clockIn;
    public MainView() : base(ViewInitializationStrategy.Lazy)
    {
        _router = ServiceProvider.GetRequiredService<PageRouter>();
        _translate = ServiceProvider.GetRequiredService<Translate>();
        _errorProxy = ServiceProvider.GetRequiredService<ErrorProxy>();
        _clockIn = ServiceProvider.GetRequiredService<ClockInManager>();

        var broadcaster = ServiceProvider.GetRequiredService<BroadcastManager>();
        broadcaster.ExceptionCatched.Subscribe(ErrorProxy_OnCatched);
        broadcaster.Broadcast.Subscribe(OnBroadCast);

        var varManager = ServiceProvider.GetRequiredService<VariantsManager>();
        var theme = varManager.GetVariant(VariantFields.Theme);

        App.RequestedThemeVariant = theme switch
        {
            "dark" => ThemeVariant.Dark,
            "light" => ThemeVariant.Light,
            _ => ThemeVariant.Default,
        };

        var language = varManager.GetVariant(VariantFields.Language);
        if (string.IsNullOrWhiteSpace(language)) language = _translate.Default;

        try
        {
            _translate.ChangeLanguage(language);
        }
        catch (Exception ex)
        {
            _errorProxy.CatchException(ex);
        }

        CheckRemindState();
        Initialize();
    }

    private void OnBroadCast(BroadcastEvent e)
    {
        if (e == BroadcastEvent.LanguageChanged) UpdateState();
        else if (e == BroadcastEvent.RemindStateChanged || e == BroadcastEvent.DateChanged)
        {
            CheckRemindState();
        }
    }
    private void ErrorProxy_OnCatched(string msg)
    {
        Notification.Error(msg);
    }
    private void CheckRemindState()
    {
        var remindCount = _clockIn.CheckReminds();
        if (remindCount > 0)
        {
            PageRouter.GetNavItem("ClockIn")?.Status.OnNext(remindCount.ToString());
        }
    }

    private NavMenuItem NavItemTemplate(NavItem p)
    {
        if (p.IsSeparator)
        {
            return new NavMenuItem() { IsSeparator = true };
        }

        var micon = MaterialIcon(p.Icon, 16).BindClass(_router.CurrentPage.Select(c => c == p), "Active");
        var header = HStackPanel().Spacing(8)
                .Children(
                PzText(() => p.PageName),
                new Label()
                    .IsVisible(p.Status.Select(s => !string.IsNullOrEmpty(s)))
                    .Content(p.Status)
                    .Classes("Ghost")
                    .Classes("Orange")
                    .Theme(StaticResource<ControlTheme>("TagLabel"))
            );

        return new NavMenuItem()
        {
            Header = header,
            Icon = micon,
            DataContext = p
        };
    }
    protected override Control Build()
    {
        var version = Assembly.GetExecutingAssembly().GetName().Version?.ToString(3);
#if DEBUG
        version += " (Debug)";
#endif

        var menu = new NavMenu
        {
            ExpandWidth = 240,
            IsHorizontalCollapsed = false,
            Header = VStackPanel(Aligns.HCenter)
                .Margin(8, 32, 8, 24)
                .Children(
                    PzText("PZRecorder", "H4")
                        .Theme(StaticResource<ControlTheme>("TitleTextBlock"))
                        .Align(Aligns.HCenter),
                    PzText(version, "Tertiary")
                        .Align(Aligns.HCenter)
                )
        };
        menu.Styles(
            new Style<MaterialIcon>().Foreground(StaticColor("SemiGrey5")),
            new Style<MaterialIcon>(s => s.Class("Active")).Foreground(StaticColor("SemiBlue5"))
        );
        foreach (var p in Routes.Pages)
        {
            menu.Items.Add(NavItemTemplate(p));
        }

        BindingRouter(menu);

        return new Panel()
            .Children(
                PzGrid(cols: "auto, *")
                .Children(
                     new Border().Col(0)
                        .Padding(8, 4)
                        .Align(Aligns.VStretch)
                        .Theme(DynamicResource("CardBorder", TypeConverter<ControlTheme>))
                        .Child(menu),
                     new ContentControl().Col(1).Margin(12, 36, 12, 12)
                        .ContentTemplate(GlobalInstances.PageLocator)
                        .Content(_router.CurrentPage)
                )
            );
    }

    private void BindingRouter(NavMenu menu)
    {
        menu.SelectionChanged += Menu_SelectionChanged;
        _router.CurrentPage.Subscribe(p =>
        {
            if (menu.SelectedItem is NavItem si)
            {
                if (si != p) menu.SelectedItem = p;
            }
        });
    }
    private void Menu_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (e.Source is NavMenu menu && menu.SelectedItem is NavItem p)
        {
            _router.CurrentPage.OnNext(p);
        }
    }
}
