using PZRecorder.Core.Common;
using SQLite;

namespace PZRecorder.Core.Tables;

public enum CycleMode
{
    Once,
    Day,
    Week,
    Month
}

[Table("t_cycle_task_kind")]
[TableVersion(10011, 99999)]
public class CycleTaskKind
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

[Table("t_cycle_task")]
[TableVersion(10011, 99999)]
public class CycleTask
{
    [PrimaryKey, AutoIncrement]
    [Column("id")]
    public int Id { get; set; }

    [Column("kind_id")]
    public int KindId { get; set; }

    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [Column("order_no")]
    public int OrderNo { get; set; }

    [Column("cycle_mode")]
    public CycleMode Mode { get; set; } = CycleMode.Day;

    // when mode = Once means days count
    // others means how much day/week/month to reset
    [Column("cycle_length")]
    public int CycleLength { get; set; } = 1;

    // cycle task reset point
    // when mode = Day or Once not used
    // when mode = Week means DayOfWeek
    // when mode = Month means DayOfMonth
    [Column("reset_point")]
    public int ResetPoint { get; set; } = 1;

    [Column("start_day")]
    public int StartDay { get; set; } = 0;
}

[Table("t_cycle_task_item")]
[TableVersion(10011, 99999)]
public class CycleTaskItem
{
    [PrimaryKey, AutoIncrement]
    [Column("id")]
    public int Id { get; set; }

    [Column("pid")]
    public int Pid { get; set; }

    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [Column("order_no")]
    public int OrderNo { get; set; }

    [Column("completed")]
    public int Completed { get; set; }

    [Column("total")]
    public int Total { get; set; }
}