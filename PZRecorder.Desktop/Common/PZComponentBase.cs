using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;

namespace PZRecorder.Desktop.Common;

public abstract class PzComponentBase : ComponentBase
{
    protected ServiceProvider ServiceProvider { get; private set; }
    protected SemiHelper Semi { get; private set; }
    protected PzNotification Notification { get; private set; }

    [MemberNotNull(nameof(ServiceProvider), nameof(Semi), nameof(Notification))]
    private void InjectProperties()
    {
        ServiceProvider = GlobalInstances.Services;
        Semi = GlobalInstances.Semi;
        Notification = ServiceProvider.GetRequiredService<PzNotification>();
    }
    protected static T? TypeConverter<T>(object? obj) => obj is T t ? t : default;

    protected PzComponentBase() : base()
    {
        InjectProperties();
    }
    protected PzComponentBase(ViewInitializationStrategy s) : base(s)
    {
        InjectProperties();
    }
}

public abstract class PzPageBase : PzComponentBase
{
    protected PzPageBase() : base() { }
    protected PzPageBase(ViewInitializationStrategy s) : base(s) { }

    public virtual void OnRouteEnter() { }
    public virtual void OnRouteExit() { }
}
