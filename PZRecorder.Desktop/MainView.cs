using Avalonia.Styling;
using Microsoft.Extensions.DependencyInjection;
using PZRecorder.Desktop.Common;
using PZRecorder.Desktop.Extensions;
using PZRecorder.Desktop.Localization;
using PZRecorder.Desktop.Modules.Shared;
using System.Reactive.Linq;
using Ursa.Controls;

namespace PZRecorder.Desktop;

internal class MainView: MvuComponent
{
    private readonly PageRouter _router;
    public MainView(PageRouter router) : base(ViewInitializationStrategy.Lazy)
    {
        _router = router;
        ErrorProxy.OnCatched += ErrorProxy_OnCatched;
        Translate.LanguageChanged += UpdateState;

        Initialize();
    }

    private void ErrorProxy_OnCatched(string msg)
    {
        Notification.Error(msg);
    }
    protected override void OnCreated()
    {
        base.OnCreated();

        var varManager = ServiceProvider.GetRequiredService<VariantsManager>();
        var theme = varManager.GetVariant(VariantFields.Theme);

        App.RequestedThemeVariant = theme switch
        {
            "dark" => ThemeVariant.Dark,
            "light" => ThemeVariant.Light,
            _ => ThemeVariant.Default,
        };

        var language = varManager.GetVariant(VariantFields.Language);
        if (string.IsNullOrWhiteSpace(language)) language = Translate.Default;

        try
        {
            Translate.ChangeLanguage(language);
        }
        catch (Exception ex)
        {
            ErrorProxy.CatchException(ex);
        }
    }
    private NavMenuItem NavItemTemplate(PageRecord p)
    {
        var icon = new TwoTonePathIcon()
        {
            Width = 16,
            Height = 16,
            Data = GlobalInstances.Semi.GetIconGeometry(p.Icon),
            Foreground = StaticColor("SemiGrey5"),
            StrokeBrush = StaticColor("SemiGrey5"),
            ActiveForeground = StaticColor("SemiBlue5"),
            ActiveStrokeBrush = StaticColor("SemiBlue5"),
        };
        icon._set(TwoTonePathIcon.IsActiveProperty, _router.CurrentPage.Select(c => c == p));
        var header = HStackPanel().Spacing(8)
                .Children(
                PzText(() => p.PageName),
                new Label()
                    .IsVisible(p.Status.Select(s => !string.IsNullOrEmpty(s)))
                    .Content(p.Status)
                    .Theme(StaticResource<ControlTheme>("TagLabel"))
            );

        return new NavMenuItem()
        {
            Header = header,
            Icon = icon,
            DataContext = p
        };
    }
    protected override Control Build()
    {
        var menu = new NavMenu
        {
            ExpandWidth = 240,
            IsHorizontalCollapsed = false,
            Header = PzText("PZRecorder", "H4")
                .Align(Aligns.HCenter, Aligns.VCenter)
                .Margin(8, 32, 8, 8)
                .Theme(StaticResource<ControlTheme>("TitleTextBlock"))
        };
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
                        .ContentTemplate(PageLocator.Instance)
                        .Content(_router.CurrentPage)
                )
            );
    }

    private void BindingRouter(NavMenu menu)
    {
        menu.SelectionChanged += Menu_SelectionChanged;
        _router.CurrentPage.Subscribe(p =>
        {
            if (menu.SelectedItem is PageRecord op)
            {
                if (op != p) menu.SelectedItem = p;
            }
        });
    }
    private void Menu_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (e.Source is NavMenu menu && menu.SelectedItem is PageRecord p)
        {
            _router.CurrentPage.OnNext(p);
        }
    }
}
