using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;

namespace PZRecorder.Desktop.Common;

public abstract class PZComponentBase : ComponentBase
{
    protected ServiceProvider ServiceProvider { get; private set; }
    protected SukiHelpers Suki { get; private set; }
    protected PzDialog Dialog { get; private set; }
    protected PzToast Toast { get; private set; }

    [MemberNotNull(nameof(ServiceProvider), nameof(Suki), nameof(Dialog), nameof(Toast))]
    private void InjectProperties()
    {
        ServiceProvider = GlobalInstances.GetServiceProvider();
        Suki = GlobalInstances.GetSukiHelpers();
        Dialog = ServiceProvider.GetRequiredService<PzDialog>();
        Toast = ServiceProvider.GetRequiredService<PzToast>();
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
