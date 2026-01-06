using Microsoft.Extensions.DependencyInjection;
using PZRecorder.Desktop.Common;
using System.Diagnostics.CodeAnalysis;

namespace PZRecorder.Desktop.Modules.Shared;

public abstract class MvuComponent : ComponentBase
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

    protected MvuComponent(ViewInitializationStrategy s = ViewInitializationStrategy.Lazy) : base(s)
    {
        InjectProperties();
    }
}

public abstract class MvuPage : MvuComponent
{
    public virtual void OnRouteEnter() { }
    public virtual void OnRouteExit() { }
}