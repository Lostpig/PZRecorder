using System.Text;

namespace PZPKRecorder.Services;

internal class ExceptionProxy
{
    public static void CatchException(Exception ex)
    {
        BroadcastService.Broadcast(BroadcastEvent.ExceptionCatch, string.IsNullOrWhiteSpace(ex.Message) ? "Unknown exception" : ex.Message);

        try
        {
            string exceptionLog = $"\r\n[{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ff")}] {ex.ToString()}\r\n";
            WriteLog(exceptionLog);
        }
        catch
        {
            // DO NOTHING
        }
    }

    private static void WriteLog(string log)
    {
        string rootPath = System.AppDomain.CurrentDomain.BaseDirectory;
        string logDirPath = Path.Join(rootPath, "logs");
        string filePath = Path.Join(rootPath, "logs", $"{DateTime.Today.ToString("yyyy-MM-dd")}.log");

        if (!Directory.Exists(logDirPath))
        {
            Directory.CreateDirectory(logDirPath);
        }

        File.AppendAllText(filePath, log, Encoding.UTF8);
    }
}
