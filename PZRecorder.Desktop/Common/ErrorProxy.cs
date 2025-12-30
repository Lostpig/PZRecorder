using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace PZRecorder.Desktop.Common;

internal class ErrorProxy
{
    private static string FormatException(Exception ex)
    {
        return ex switch
        {
            _ => string.Format("Error: {0}", ex.Message),
        };
    }

    public static string CatchException(Exception ex)
    {
        try
        {
            Logger.Instance.Error(ex);
            return FormatException(ex);
        }
        catch
        {
            // DO NOTHING
            return "";
        }
    }
}
