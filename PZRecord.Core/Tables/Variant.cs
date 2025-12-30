using SQLite;

namespace PZRecorder.Core.Tables;

[Table("t_variant")]
public class VariantTable
{
    [PrimaryKey]
    [Column("key")]
    public string Key { get; set; } = string.Empty;

    [Column("value"), Indexed]
    public string Value { get; set; } = string.Empty;
}
