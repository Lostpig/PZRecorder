using SQLite;

namespace PZRecorder.Core.Tables;

public class ViewCounter
{
    [Column("count")]
    public int Count { get; set; }
}
