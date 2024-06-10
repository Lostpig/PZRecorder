using SQLite;
namespace PZPKRecorder.Data;

internal class SQLCounter
{
    [Column("count")]
    public int Count { get; set; }
}
