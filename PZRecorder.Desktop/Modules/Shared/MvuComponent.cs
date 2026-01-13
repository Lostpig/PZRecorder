using Avalonia;
using Microsoft.Extensions.DependencyInjection;
using PZRecorder.Desktop.Common;
using PZRecorder.Desktop.Localization;
using System.Diagnostics.CodeAnalysis;

namespace PZRecorder.Desktop.Modules.Shared;

public abstract class MvuComponent : ComponentBase
{
    protected static ServiceProvider ServiceProvider => GlobalInstances.Services;
    protected static SemiHelper Semi => GlobalInstances.Semi;
    protected PzNotification Notification { get; private set; }
    protected static Application App => Application.Current ?? throw new ArgumentNullException("Application not boot!");

    [MemberNotNull(nameof(Notification))]
    private void InjectProperties()
    {
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
    protected MvuPage(ViewInitializationStrategy s = ViewInitializationStrategy.Lazy) : base(s)
    {
        Translate.LanguageChanged += UpdateState;
    }

    public virtual void OnRouteEnter() { }
    public virtual void OnRouteExit() { }
}