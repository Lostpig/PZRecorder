using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Tmds.DBus.Protocol;

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

    public static TPanel ChildrenEx<TPanel>(this TPanel container,  Func<Control[]> getter)
        where TPanel : Panel
    {
        container._set(static (c, v) =>
        {
            c.Children.Clear();
            foreach (var child in v) c.Children.Add(child);
        }, getter);

        return container;
    }

    public static string Text(this TextChangedEventArgs e)
    {
        return ((TextBox)e.Source!).Text ?? "";
    } 

    public static T? ValueObj<T>(this SelectionChangedEventArgs e) where T : class
    {
        if (e.Source is SelectingItemsControl c && c.SelectedValue is T t)
        {
            return t;
        }

        return null;
    }
    public static T? ValueStruct<T>(this SelectionChangedEventArgs e) where T : struct
    {
        if (e.Source is SelectingItemsControl c && c.SelectedValue is T t)
        {
            return t;
        }

        return null;
    }
} 
