using Avalonia;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;
using PZ.RxAvalonia.Helpers;

namespace PZRecorder.Desktop.Modules.Shared;

internal static class Styles
{
    private static ControlTheme ConvertControlTheme(object? obj)
    {
        return obj is ControlTheme ct ? ct : new ControlTheme();
    }
    public static StyleGroup ListStyles()
    {
        return [
                new StyleGroup(s => s.Class("ListRow"))
                {
                    new Style<Grid>(s => s.Class("ListRow"))
                        .Background(Brushes.Transparent)
                        .Height(50),
                    new Style<Grid>(s => s.Class(":pointerover"))
                        .SetterEx(Grid.BackgroundProperty, DynamicColors.Get("SemiColorFill0")),
                },
                new Style(s => s.Class("ListRow").Child()).Setter(Control.MarginProperty, new Thickness(16, 0)),
                new StyleGroup(s => s.Class("ListRow").Descendant())
                {
                    new Style<TextBlock>().VerticalAlignment(VerticalAlignment.Center),
                    new Style<Button>()
                        .Theme(ResourceHelpers.StaticResource("BorderlessButton", Application.Current!, ConvertControlTheme)),
                },
                new Style<Grid>(s => s.Class("ListRowHeader")).Height(50).Margin(0, 0, 0, 8),
                new StyleGroup(s => s.Class("ListRowHeader").Descendant())
                {
                    new Style<TextBlock>()
                        .FontWeight(FontWeight.Bold)
                        .VerticalAlignment(VerticalAlignment.Center)
                        .Margin(16, 0),
                }
            ];
    }
}
