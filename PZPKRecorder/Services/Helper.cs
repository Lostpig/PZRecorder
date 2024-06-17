using System.Diagnostics;

namespace PZPKRecorder.Services;

internal class Helper
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
}
