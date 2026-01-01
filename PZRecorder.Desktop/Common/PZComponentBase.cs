using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;

namespace PZRecorder.Desktop.Common;

public abstract class PZComponentBase : ComponentBase
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

    protected PZComponentBase() : base()
    {
        InjectProperties();
    }
    protected PZComponentBase(ViewInitializationStrategy s) : base(s)
    {
        InjectProperties();
    }
}

public abstract class PZPageBase : PZComponentBase
{
    protected PZPageBase() : base() { }
    protected PZPageBase(ViewInitializationStrategy s) : base(s) { }

    public virtual void OnRouteEnter() { }
    public virtual void OnRouteExit() { }
}
