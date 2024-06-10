using SQLite;

namespace PZPKRecorder.Data;

[Table("t_kind")]
internal class Kind
{
    [PrimaryKey, AutoIncrement]
    [Column("id")]
    public int Id { get; set; }

    [Column("name")]
    public string Name { get; set; } = string.Empty;
}
