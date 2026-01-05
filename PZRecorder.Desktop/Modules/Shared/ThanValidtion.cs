using PZ.RxAvalonia.DataValidations;
using System.Reactive.Subjects;

namespace PZRecorder.Desktop.Modules.Shared;

enum ThanOperation
{
    Greater, GreaterEqual,
    Less, LessEqual,
}

internal class ThanValidtion<T> : IDataValidation<T?> where T : struct, IComparable<T>
{
    public ThanOperation Operation { get; init; }
    public IObservable<T>? ThanObs { get; init; }
    public T Value { get; private set; }
    public ThanValidtion(ThanOperation opt, T value, IObservable<T>? thanObs = null)
    {
        Value = value;
        Operation = opt;
        ThanObs = thanObs;

        ThanObs?.Subscribe(v => Value = v);
    }
    public ThanValidtion(ThanOperation opt, BehaviorSubject<T> sub) : this(opt, sub.Value, sub) { }

    public string ErrorMessage { get; set; } = "Field value not valid!";

    public bool IsValid(T? v)
    {
        if (!v.HasValue) return false;

        switch (Operation)
        {
            case ThanOperation.Greater: return v.Value.CompareTo(Value) > 0;
            case ThanOperation.GreaterEqual: return v.Value.CompareTo(Value) >= 0;
            case ThanOperation.Less: return v.Value.CompareTo(Value) < 0;
            case ThanOperation.LessEqual: return v.Value.CompareTo(Value) <= 0;
            default: return false;
        }
    }
}
