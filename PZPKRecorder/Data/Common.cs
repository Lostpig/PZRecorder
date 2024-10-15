using SQLite;
namespace PZPKRecorder.Data;

internal class SQLCounter
{
    [Column("count")]
    public int Count { get; set; }
}

[AttributeUsage(AttributeTargets.Property)]
internal class DataFieldAttribute : Attribute
{
    public int MaxVersion { get; private set; }
    public int MinVersion { get; private set; }

    public object DefaultValue { get; private set; }
    public DataFieldAttribute(int minVersion, int maxVersion, object defaultValue)
    {
        MinVersion = minVersion;
        MaxVersion = maxVersion;
        DefaultValue = defaultValue;
    }
}
