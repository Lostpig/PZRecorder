using PZ.RxAvalonia.DataValidations;
using PZ.RxAvalonia.Reactive;
using PZ.RxAvalonia.Extensions;
using System.Reactive;
using System.Reactive.Subjects;
using PZRecorder.Desktop.Modules.Shared;
using System.Reactive.Linq;

namespace PZRecorder.Desktop.Extensions;

internal static class UrsaExtensions
{
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


    public static Uc.NumericIntUpDown ValueEx(this Uc.NumericIntUpDown control, ISubject<int> subject)
    {
        var nbSubject = new NullableSubject<int>(subject);
        return control._set(Uc.NumericIntUpDown.ValueProperty, nbSubject);
    }
    public static Uc.NumericUpDownBase<T> DataValidation<T>(this Uc.NumericUpDownBase<T> control, IDataValidation<T?> validation)
        where T : struct, IComparable<T>
    {
        return control.SetValidation(Uc.NumericUpDownBase<T>.ValueProperty, validation);
    }
    public static Uc.NumericUpDownBase<T> ThanValidation<T>(this Uc.NumericUpDownBase<T> control, ThanValidtion<T> validation)
        where T : struct, IComparable<T>
    {
        var validator = control._getValidator(Uc.NumericUpDownBase<T>.ValueProperty);
        validation.ThanObs?.Subscribe(_ => validator?.ExcuteCheck());

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
    public static Uc.Pagination WithModel(this Uc.Pagination control, PagenationModel model)
    {
        return control
            .CurrentPage(model.Page)
            .PageSize(model.PageSize)
            .TotalCount(model.TotalCount);
    }

    public static Uc.Rating ValueEx(this Uc.Rating control, ISubject<int> subject)
    {
        var obv = Observer.Create<double>(x => subject.OnNext((int)x));

        control._setEx(
            avap: Uc.Rating.ValueProperty, 
            obs: subject.Select(n => (double)n),
            changed: obv);
        return control;
    }
}
