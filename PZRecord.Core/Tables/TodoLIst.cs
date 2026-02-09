using PZRecorder.Core.Common;
using SQLite;

namespace PZRecorder.Core.Tables;

[Table("t_todolist")]
[TableVersion(10011, 99999)]
public class TodoList
{
    [PrimaryKey, AutoIncrement]
    [Column("id")]
    public int Id { get; set; }

    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [Column("remark"), MaxLength(1000)]
    public string Remark { get; set; } = string.Empty;

    [Column("completed")]
    public bool Completed { get; set; } = false;

    [Column("add_time")]
    public DateTime AddTime { get; set; }

    [Column("complete_time")]
    public DateTime CompleteTime { get; set; }
}
