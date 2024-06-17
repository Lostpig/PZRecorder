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

    [Column("order_no")]
    public int OrderNo { get; set; }
}


[Table("t_kind")]
internal class KindVersion0
{
    [PrimaryKey, AutoIncrement]
    [Column("id")]
    public int Id { get; set; }

    [Column("name")]
    public string Name { get; set; } = string.Empty;
}