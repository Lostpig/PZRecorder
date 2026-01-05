namespace PZRecorder.Desktop.Common;

internal static class Utility
{
    public static string GetRootPath()
    {
        return System.AppDomain.CurrentDomain.BaseDirectory;
    }
    public static string GetDataBasePath()
    {
        var rootPath = GetRootPath();
        string dbPath = Path.Join(rootPath, "records.db");
        return dbPath;
    }
}
