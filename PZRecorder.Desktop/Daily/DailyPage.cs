using PZRecorder.Desktop.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace PZRecorder.Desktop.Daily;

internal class DailyPage : PZPageBase
{
    protected override Control Build()
    {
        return PzText("DailyPage");
    }
}
