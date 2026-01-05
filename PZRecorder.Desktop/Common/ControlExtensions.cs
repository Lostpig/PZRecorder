using Avalonia.Layout;
using PZ.RxAvalonia.DataValidations;
using PZ.RxAvalonia.Extensions;
using PZ.RxAvalonia.Reactive;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace PZRecorder.Desktop.Common;

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

    public static Uc.NumericIntUpDown ValueEx(this Uc.NumericIntUpDown control, ISubject<int> subject)
    {
        var nbSubject = new NullableSubject<int>(subject);
        return control._set(Uc.NumericIntUpDown.ValueProperty, nbSubject);
    }

    public static T FormLabel<T>(this T control, object label) where T : Control
    {
        Uc.FormItem.SetLabel(control, label);
        return control;
    }
    public static T FormRequired<T>(this T control, bool required) where T : Control
    {
        Uc.FormItem.SetIsRequired(control, required);
        return control;
    }

    public static Uc.NumericUpDownBase<T> DataValidation<T>(this Uc.NumericUpDownBase<T> control, IDataValidation<T?> validation)
        where T : struct, IComparable<T>
    {
        return control.SetValidation(Uc.NumericUpDownBase<T>.ValueProperty, validation);
    }

    public static Uc.EnumSelector EnumType(this Uc.EnumSelector control, Type? type)
    {
        control.EnumType = type;
        return control;
    }
    public static Uc.EnumSelector Value<T>(this Uc.EnumSelector control, ISubject<T?> subject) where T : Enum
    {
        var obv = Observer.Create<object?>(x =>
        {
            if (x is T enumValue)
            {
                subject.OnNext(enumValue);
            }
            else
            {
                subject.OnNext(default);
            }
        });
        control._setEx(Uc.EnumSelector.ValueProperty, subject.Select(x => (object?)x), obv);
        return control;
    }
    
    public static Uc.Pagination CurrentPage(this Uc.Pagination control, ISubject<int> subject)
    {
        var nbSubject = new NullableSubject<int>(subject)
        {
            DefaultValue = 1
        };

        control._set(Uc.Pagination.CurrentPageProperty, nbSubject);
        return control;
    }
    public static Uc.Pagination PageSize(this Uc.Pagination control, ISubject<int> subject)
    {
        control._set(Uc.Pagination.PageSizeProperty, subject);
        return control;
    }
    public static Uc.Pagination PageCount(this Uc.Pagination control, ISubject<int> subject)
    {
        control._set(Uc.Pagination.PageCountProperty, subject);
        return control;
    }
    public static Uc.Pagination TotalCount(this Uc.Pagination control, IObservable<int> obs)
    {
        control._set(Uc.Pagination.TotalCountProperty, obs);
        return control;
    }
} 
