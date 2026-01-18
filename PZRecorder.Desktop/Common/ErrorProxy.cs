namespace PZRecorder.Desktop.Common;

internal class ErrorProxy(BroadcastManager broadcaster, Logger logger)
{
    private static string FormatException(Exception ex)
    {
        return ex switch
        {
            _ => string.Format("Error: {0}", ex.Message),
        };
    }

    private readonly BroadcastManager _broadcaster = broadcaster;
    private readonly Logger _logger = logger;

    public void CatchException(Exception ex)
    {
        try
        {
            _logger.Error(ex);

            var msg = FormatException(ex);
            _broadcaster.OnExceptionCatched(msg);
        }
        catch
        {
            // DO NOTHING
        }
    }
}
