using SQLite;

namespace PZPKRecorder.Data;

[Table("t_clockin")]
internal class ClockIn
{
    [PrimaryKey, AutoIncrement]
    [Column("id")]
    public int Id { get; set; }

    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [Column("order_no")]
    public int OrderNo { get; set; }

    [Column("remark"), MaxLength(1000)]
    public string Remark { get; set; } = string.Empty;
}

[Table("t_clockin_record")]
internal class ClockInRecord
{
    [PrimaryKey, AutoIncrement]
    [Column("id")]
    public int Id { get; set; }

    [Column("pid")]
    public int Pid { get; set; }

    [Column("time")]
    public DateTime Time { get; set; }
}