using Avalonia.Media;
using PZ.RxAvalonia.Helpers;
using Avalonia;

namespace PZRecorder.Desktop.Modules.Shared;

internal static class DynamicColors
{
    static Dictionary<string, IObservable<IBrush>> _colors = [];

    public static IObservable<IBrush> Get(string key)
    {
        if (!_colors.TryGetValue(key, out var color))
        {
            color = ResourceHelpers.DynamicColor(key, Application.Current!);
            _colors.Add(key, color);
        }
        return color;
    }
}
