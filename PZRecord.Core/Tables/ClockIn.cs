using PZRecorder.Core.Common;
using SQLite;

namespace PZRecorder.Core.Tables;

[Table("t_clockin")]
[TableVersion(10005, 99999)]
public class ClockIn
{
    [PrimaryKey, AutoIncrement]
    [Column("id")]
    public int Id { get; set; }

    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [Column("remind_days")]
    [FieldVersion(10006, 99999, 0)]
    public int RemindDays { get; set; } = 0;

    [Column("order_no")]
    public int OrderNo { get; set; }

    [Column("remark"), MaxLength(1000)]
    public string Remark { get; set; } = string.Empty;
}

[Table("t_clockin_record")]
[TableVersion(10005, 99999)]
public class ClockInRecord
{
    [PrimaryKey, AutoIncrement]
    [Column("id")]
    public int Id { get; set; }

    [Column("pid")]
    public int Pid { get; set; }

    [Column("time")]
    public DateTime Time { get; set; }

    [Ignore]
    public int Counter { get; set; } = 0;
}