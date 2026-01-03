using Avalonia;
using Avalonia.Media;
using Semi.Avalonia;

namespace PZRecorder.Desktop.Common;

public class SemiHelper
{
    private readonly Icons _icons;
    private readonly Dictionary<string, Geometry> _iconCache = [];
    public SemiHelper()
    {
        _icons = new();
    }

    public static IBrush GetStaticColor(string key, IResourceNode? anchor = null)
    {
        anchor ??= Application.Current!;
        if (anchor.TryGetResource(key, Application.Current!.ActualThemeVariant, out var value))
        {
            if (value is IBrush b) return b;
            if (value is Color c) return new SolidColorBrush(c);
        }

        return Brushes.Black;
    }
    public static object GetStaticResource(string key, IResourceNode? anchor = null)
    {
        anchor ??= Application.Current!;
        if (anchor.TryGetResource(key, Application.Current!.ActualThemeVariant, out var value))
        {
            return value ?? AvaloniaProperty.UnsetValue;
        }

        return AvaloniaProperty.UnsetValue;
    }

    public Geometry GetIconGeometry(string key)
    {
        if (_iconCache.TryGetValue(key, out var icon)) return icon;

        foreach (var provider in _icons.MergedDictionaries)
        {
            if (provider is not ResourceDictionary dic) continue;
            if (dic.TryGetResource(key, Application.Current?.ActualThemeVariant, out var resource) 
                && resource is Geometry geometry)
            {
                _iconCache.Add(key, geometry);
                return geometry;
            }
        }

        throw new ArgumentException($"Not found icon key = [{key}]");
    }
}
