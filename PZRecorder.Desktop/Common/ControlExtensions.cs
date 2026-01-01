using Avalonia.Layout;
using PZ.RxAvalonia.Reactive;
using System.Reactive.Subjects;

namespace PZRecorder.Desktop.Common;

public enum UnionAlign
{
    Left, Right, Top, Bottom,
    HCenter, VCenter,
    HStretch, VStretch
}
public static class ControlExtensions
{
    public static T Align<T>(this T control, UnionAlign ailgn) where T : Control
    {
        switch (ailgn)
        {
            case UnionAlign.Left: control.HorizontalAlignment(HorizontalAlignment.Left); break;
            case UnionAlign.Right: control.HorizontalAlignment(HorizontalAlignment.Right); break;
            case UnionAlign.HCenter: control.HorizontalAlignment(HorizontalAlignment.Center); break;
            case UnionAlign.HStretch: control.HorizontalAlignment(HorizontalAlignment.Stretch); break;
            case UnionAlign.VCenter: control.VerticalAlignment(VerticalAlignment.Center); break;
            case UnionAlign.VStretch: control.VerticalAlignment(VerticalAlignment.Stretch); break;
            case UnionAlign.Top: control.VerticalAlignment(VerticalAlignment.Top); break;
            case UnionAlign.Bottom: control.VerticalAlignment(VerticalAlignment.Bottom); break;
        }

        return control;
    }
    public static T Align<T>(this T control, params UnionAlign[] aligns) where T : Control
    {
        foreach (var align in aligns) { control.Align(align); }
        return control;
    }

    public static Uc.NumericIntUpDown ValueEx(this Uc.NumericIntUpDown control, ISubject<int> subject)
    {
        var nbSubject = new NullableSubject<int>(subject);
        return control._set(Uc.NumericIntUpDown.ValueProperty, nbSubject);
    }
}
