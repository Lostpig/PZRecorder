using Avalonia;
using Avalonia.Media;
using Avalonia.Styling;
using SukiUI;
using SukiUI.Models;

namespace PZRecorder.Desktop.Common;

public class SukiHelpers
{
    private static Color GetDynamicColor(string key)
    {
        if (Application.Current is null)
        {
            throw new Exception("Application have not luanched");
        }

        Color res;

        if (GetStylesColor(Application.Current, key, out res))
        {
            return res;
        }

        if (GetResourceColor(Application.Current, key, out res))
        {
            return res;
        }

        throw new ArgumentException($"DynamicResource key \"{key}\" is not available or not a color");
    }
    private static bool GetStylesColor(Application app, string key, out Color color)
    {
        var r = app.Styles.TryGetResource(key, app.ActualThemeVariant, out var v);
        if (r == true && v is Color c)
        {
            color = c;
            return r;
        }

        color = Colors.Black;
        return false;
    }
    private static bool GetResourceColor(Application app, string key, out Color color)
    {
        var res = app.Resources.TryGetValue(key, out var v);
        if (res == true && v is Color c)
        {
            color = c;
            return res;
        }

        color = Colors.Black;
        return false;
    }

    private readonly Dictionary<string, IBrush> _colors = [];
    private readonly SukiTheme _currentTheme;

    private string _themeId;
    private string _cacheThemeId = string.Empty;

    public SukiHelpers()
    {
        _currentTheme = SukiTheme.GetInstance();
        _currentTheme.OnBaseThemeChanged += BaseThemeChanged;
        _currentTheme.OnColorThemeChanged += ColorThemeChanged;

        _themeId = GetThemeId();
        _cacheThemeId = _themeId;
    }
    public IBrush GetSukiColor(string key)
    {
        if (_cacheThemeId != _themeId)
        {
            _colors.Clear();
            _cacheThemeId = _themeId;
        }

        if (_colors.TryGetValue(key, out IBrush? value)) return value;

        var color = GetDynamicColor(key);
        var brush = new SolidColorBrush(color);
        _colors.Add(key, brush);
        return brush;
    }

    private string GetThemeId()
    {
        return _currentTheme.ActiveColorTheme?.DisplayName + "_" + _currentTheme.ActiveBaseTheme.Key.ToString();
    }
    private void BaseThemeChanged(ThemeVariant _)
    {
        _themeId = GetThemeId();
    }
    private void ColorThemeChanged(SukiColorTheme _)
    {
        _themeId = GetThemeId();
    }
}
