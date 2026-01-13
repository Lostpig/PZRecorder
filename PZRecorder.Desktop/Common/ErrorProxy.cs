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

    public static event Action<string>? OnCatched;
    public static void CatchException(Exception ex)
    {
        try
        {
            Logger.Instance.Error(ex);

            var msg = FormatException(ex);
            OnCatched?.Invoke(msg);
        }
        catch
        {
            // DO NOTHING
        }
    }
}
