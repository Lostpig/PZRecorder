using PZRecorder.Desktop.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace PZRecorder.Desktop.Modules.Daily;

internal class DailyPage : PzPageBase
{
    protected override Control Build()
    {
        return PzText("DailyPage");
    }
}
