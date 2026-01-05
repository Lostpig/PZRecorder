using Avalonia.Styling;
using PZ.RxAvalonia.Reactive;
using PZRecorder.Desktop.Common;
using PZRecorder.Desktop.Extensions;
using System.Reactive.Linq;
using Ursa.Controls;

namespace PZRecorder.Desktop;

internal class MainView: PzComponentBase
{
    private readonly PageRouter _router;
    public MainView(PageRouter router) : base(ViewInitializationStrategy.Lazy)
    {
        _router = router;
        Initialize();
    }

    protected override IEnumerable<IDisposable> WhenActivate()
    {
        return base.WhenActivate();
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
                PzText(p.PageName),
                new Label()
                    .IsVisible(p.Status.Select(s => !string.IsNullOrEmpty(s)))
                    .Content(p.Status)
                    .Theme(StaticResource("TagLabel", o => (ControlTheme)o!))
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

        var objSubject = new ObjectSubject<PageRecord>(_router.CurrentPage);
        menu._set(NavMenu.SelectedItemProperty!, objSubject);

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

    private void Menu_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (e.Source is NavMenu menu && menu.SelectedItem is PageRecord p)
        {
            _router.CurrentPage.OnNext(p);
        }
    }
}
