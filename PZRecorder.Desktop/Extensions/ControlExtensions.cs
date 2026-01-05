using Avalonia.Layout;

namespace PZRecorder.Desktop.Extensions;

public enum Aligns
{
    Left, Right, Top, Bottom,
    HCenter, VCenter,
    HStretch, VStretch
}
public static class ControlExtensions
{
    public static T Align<T>(this T control, Aligns ailgn) where T : Control
    {
        switch (ailgn)
        {
            case Aligns.Left: control.HorizontalAlignment(HorizontalAlignment.Left); break;
            case Aligns.Right: control.HorizontalAlignment(HorizontalAlignment.Right); break;
            case Aligns.HCenter: control.HorizontalAlignment(HorizontalAlignment.Center); break;
            case Aligns.HStretch: control.HorizontalAlignment(HorizontalAlignment.Stretch); break;
            case Aligns.VCenter: control.VerticalAlignment(VerticalAlignment.Center); break;
            case Aligns.VStretch: control.VerticalAlignment(VerticalAlignment.Stretch); break;
            case Aligns.Top: control.VerticalAlignment(VerticalAlignment.Top); break;
            case Aligns.Bottom: control.VerticalAlignment(VerticalAlignment.Bottom); break;
        }

        return control;
    }
    public static T Align<T>(this T control, params Aligns[] aligns) where T : Control
    {
        foreach (var align in aligns) { control.Align(align); }
        return control;
    }
} 
