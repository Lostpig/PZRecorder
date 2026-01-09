using PZRecorder.Desktop.Modules.Shared;

namespace PZRecorder.Desktop.Modules.Daily;

internal class DailyPageModel
{

}
internal sealed class DailyPage : MvuPage
{
    protected override Control Build()
    {
        return PzText("DailyPage");
    }
}
