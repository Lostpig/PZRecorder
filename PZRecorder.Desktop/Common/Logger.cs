using System.Diagnostics;
using System.Text;

namespace PZRecorder.Desktop.Common;
internal class Logger
{
    public string LogFile { get; set; } = "";
    public void Initialize(string logfile)
    {
        LogFile = logfile;
    }
    public void Log(string message)
    {
#if DEBUG
        LogDebug(message);
#else
        LogRelease(message);
#endif
    }
    private void LogRelease(string message)
    {
        if (!string.IsNullOrWhiteSpace(LogFile))
        {
            try
            {
                File.AppendAllText(LogFile, message, Encoding.UTF8);
            }
            catch
            {
                // Log failed
            }
        }
    }
    private static void LogDebug(string message)
    {
        Debug.WriteLine(message);
    }

    public void Error(Exception ex)
    {
        string exceptionLog = $"\r\n[{DateTime.Now:yyyy-MM-dd HH:mm:ss.ff}] {ex.ToString()}\r\n";
        Log(exceptionLog);
    }
}

