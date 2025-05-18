using SQLite;
namespace PZPKRecorder.Data;

internal class SQLCounter
{
    [Column("count")]
    public int Count { get; set; }
}

[AttributeUsage(AttributeTargets.Property)]
internal class FieldVersionAttribute : Attribute
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

[AttributeUsage(AttributeTargets.Class)]
internal class TableVersionAttribute : Attribute
{
    public int MaxVersion { get; private set; }
    public int MinVersion { get; private set; }

    public TableVersionAttribute(int minVersion, int maxVersion)
    {
        MinVersion = minVersion;
        MaxVersion = maxVersion;
    }
}
