using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Styling;
using PZ.RxAvalonia.Reactive;
using PZRecorder.Desktop.Common;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Ursa.Controls;

namespace PZRecorder.Desktop;

internal class MainView: PZComponentBase
{
    private readonly PageRouter _router;
    private PageRecord PageRecord => _router.CurrentPage.Value;
    public MainView(PageRouter router) : base(ViewInitializationStrategy.Lazy)
    {
        Margin = new Avalonia.Thickness(0, 32, 0, 0);
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
            Foreground = SemiHelper.GetColor("SemiGrey5"),
            StrokeBrush = SemiHelper.GetColor("SemiGrey5"),
            ActiveForeground = SemiHelper.GetColor("SemiBlue5"),
            ActiveStrokeBrush = SemiHelper.GetColor("SemiBlue5"),
        };
        icon._set(TwoTonePathIcon.IsActiveProperty, _router.CurrentPage.Select(c => c == p));
        var header = HStackPanel().Spacing(8)
                .Children(
                PzText(p.PageName),
                new Label()
                    .IsVisible(p.Status.Select(s => !string.IsNullOrEmpty(s)))
                    .Content(p.Status)
                    .Theme((ControlTheme)SemiHelper.GetStaticResource("TagLabel"))
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
        var menu = new NavMenu()
        {
            ExpandWidth = 300,
            IsHorizontalCollapsed = false,
        };
        foreach (var p in Routes.Pages)
        {
            menu.Items.Add(NavItemTemplate(p));
        }
        // menu.SelectionChanged += Menu_SelectionChanged;
        var objSubject = new ObjectSubject<PageRecord>(_router.CurrentPage);
        menu._set(NavMenu.SelectedItemProperty!, objSubject);

        // menu.ItemTemplate<PageRecord, NavMenu>(p => NavItemTemplate(p));
        // menu.ItemsSource(Routes.Pages);

        return new Panel()
            .Children(
                PzGrid(cols: "auto, *")
                .Children(
                     new Border().Col(0).Padding(8, 4).Child(menu),
                     new ContentControl().Col(1)
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
