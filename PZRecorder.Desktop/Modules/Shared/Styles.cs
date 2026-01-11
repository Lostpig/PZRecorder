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
                    new Style<Grid>().Setter(Grid.BackgroundProperty, Brushes.Transparent),
                    new Style<Grid>(s => s.Class(":pointerover"))
                        .SetterEx(Grid.BackgroundProperty, DynamicColors.Get("SemiColorFill0")),
                },
                new Style<Grid>(s => s.Class("ListRow").Child()).Margin(16, 0),
                new Style<Grid>(s => s.Class("ListHeader").Child()).Margin(16, 0),
                new Style<TextBlock>().VerticalAlignment(VerticalAlignment.Center),
                new Style<Button>().Theme(ResourceHelpers.StaticResource("BorderlessButton", Application.Current!, ConvertControlTheme))
            ];
    }
}
