using System.Diagnostics;
using PZPKRecorder.Localization;

namespace PZPKRecorder.Services;

internal static class Helper
{
    public const int DataVersion = 10002;
    public static void OpenFolder(string path)
    {
        if (File.Exists(path))
        {
            string? folderPath = Path.GetDirectoryName(path);
            if (folderPath != null) path = folderPath;
        }
        
        if (Directory.Exists(path))
        {
            var psi = new ProcessStartInfo()
            {
                FileName = path,
                UseShellExecute = true
            };
            Process.Start(psi);
        }
    }

    public static string WeekdayName(DayOfWeek d, bool isShort)
    {
        return d switch
        {
            DayOfWeek.Sunday => isShort ? LocalizeDict.Sun : LocalizeDict.Sunday,
            DayOfWeek.Monday => isShort ? LocalizeDict.Mon : LocalizeDict.Monday,
            DayOfWeek.Tuesday => isShort ? LocalizeDict.Tue : LocalizeDict.Tuesday,
            DayOfWeek.Wednesday => isShort ? LocalizeDict.Wed : LocalizeDict.Wednesday,
            DayOfWeek.Thursday => isShort ? LocalizeDict.Thu : LocalizeDict.Thursday,
            DayOfWeek.Friday => isShort ? LocalizeDict.Fri : LocalizeDict.Friday,
            DayOfWeek.Saturday => isShort ? LocalizeDict.Sat : LocalizeDict.Saturday,
            _ => ""
        };
    }
}
