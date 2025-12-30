namespace PZRecorder.Core.Common;

[AttributeUsage(AttributeTargets.Class)]
public class TableVersionAttribute : Attribute
{
    public int MaxVersion { get; private set; }
    public int MinVersion { get; private set; }

    public TableVersionAttribute(int minVersion, int maxVersion)
    {
        MinVersion = minVersion;
        MaxVersion = maxVersion;
    }
}


