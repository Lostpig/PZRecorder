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
