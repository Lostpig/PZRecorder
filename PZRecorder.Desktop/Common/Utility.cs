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
    public static string GetBackupDBPath()
    {
        var rootPath = GetRootPath();
        var backupPath = Path.Join(rootPath, "backup", $"records.{DateTime.Now.ToString("yyyy-MM-dd-HHmmss")}.db");
        return backupPath;
    }

    public static string FormatTimeSpan(TimeSpan time)
    {
        int hours = time.Hours + time.Days * 24;
        return $"{hours}:{time.Minutes:d2}:{time.Seconds:d2}";
    }
    public static string FormatDuration(TimeSpan duration)
    {
        int hours = duration.Hours + duration.Days * 24;
        string hoursStr = hours > 0 ? hours + "h" : "";
        return $"{hoursStr} {duration.Minutes:d2}m";
    }
}
