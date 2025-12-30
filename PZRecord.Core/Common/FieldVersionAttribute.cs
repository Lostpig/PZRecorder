namespace PZRecorder.Core.Common;

[AttributeUsage(AttributeTargets.Property)]
public class FieldVersionAttribute : Attribute
{
    public int MaxVersion { get; private set; }
    public int MinVersion { get; private set; }

    public object DefaultValue { get; private set; }
    public FieldVersionAttribute(int minVersion, int maxVersion, object defaultValue)
    {
        MinVersion = minVersion;
        MaxVersion = maxVersion;
        DefaultValue = defaultValue;
    }
}

