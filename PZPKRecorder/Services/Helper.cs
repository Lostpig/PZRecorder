using PZPKRecorder.Localization;
using System.Diagnostics;

namespace PZPKRecorder.Services;

internal static class Helper
{
    public const int DataVersion = 10007;

    public static bool IsCompatibleVersion(int version)
    {
        bool isCompatible = false;
        switch (version)
        {
            case 0:
            case 10001:
            case 10002:
            case 10003:
            case 10004:
            case 10005:
            case 10006:
            case DataVersion:
                isCompatible = true;
                break;
            default:
                isCompatible = false;
                break;
        }

        return isCompatible;
    }
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
