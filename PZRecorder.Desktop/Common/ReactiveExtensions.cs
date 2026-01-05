using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;

namespace PZRecorder.Desktop.Common;

internal static class ReactiveExtensions
{
    public static IObservable<Tuple<T?, T?>> PairWithPrevious<T>(this IObservable<T?> source)
    {
        return source.Scan(
            Tuple.Create(default(T), default(T)),
            (acc, current) => Tuple.Create(acc.Item2, current));
    }
}

public class PreviousSubject<T> : IObservable<(T?, T)>, IObserver<T>
{
    private BehaviorSubject<(T?, T)> _inner;
    public PreviousSubject(T initialValue)
    {
        (T?, T) pair = (default, initialValue);
        _inner = new(pair);
    }

    public void OnCompleted() => _inner.OnCompleted();
    public void OnError(Exception error) => _inner.OnError(error);
    public void OnNext(T value)
    {
        var prev = _inner.Value.Item2;
        _inner.OnNext((prev, value));
    }
    public IDisposable Subscribe(IObserver<(T?, T)> observer)
    {
        return _inner.Subscribe(observer);
    }
}
