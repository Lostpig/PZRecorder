using Avalonia;
using Microsoft.Extensions.DependencyInjection;
using PZRecorder.Desktop.Common;

namespace PZRecorder.Desktop.Modules.Shared;

public abstract class MvuComponent(ViewInitializationStrategy s = ViewInitializationStrategy.Lazy) : ComponentBase(s)
{
    protected static ServiceProvider ServiceProvider => GlobalInstances.Services;
    protected static PzNotification Notification => GlobalInstances.Notification;
    protected static Application App => Application.Current ?? throw new ArgumentNullException("Application not boot!");
    protected static T? TypeConverter<T>(object? obj) => obj is T t ? t : default;
}

public abstract class MvuPage : MvuComponent
{
    protected MvuPage(ViewInitializationStrategy s = ViewInitializationStrategy.Lazy) : base(s)
    {
        var broadcaster = ServiceProvider.GetRequiredService<BroadcastManager>();
        broadcaster.Broadcast.Subscribe(e =>
        {
            if (e == BroadcastEvent.LanguageChanged || e == BroadcastEvent.ThemeChanged)
            {
                UpdateState();
            }
        });
    }

    public virtual void OnRouteEnter() { }
    public virtual void OnRouteExit() { }
}