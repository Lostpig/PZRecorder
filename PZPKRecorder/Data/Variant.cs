using SQLite;

namespace PZPKRecorder.Data;

[Table("t_variant")]
internal class VariantTable
{
    [PrimaryKey]
    [Column("key")]
    public string Key { get; set; } = string.Empty;

    [Column("value"), Indexed]
    public string Value { get; set; } = string.Empty;
}
