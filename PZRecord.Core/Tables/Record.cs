using PZRecorder.Core.Common;
using SQLite;

namespace PZRecorder.Core.Tables;

public enum RecordState
{
    Wish = 0,
    Doing = 1,
    Complete = 2,
    Giveup = 3,
}

[Table("t_kind")]
public class Kind
{
    [PrimaryKey, AutoIncrement]
    [Column("id")]
    public int Id { get; set; }

    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [Column("order_no")]
    [FieldVersion(10001, 99999, 0)]
    public int OrderNo { get; set; }

    [Column("state_wish")]
    [FieldVersion(10003, 99999, "")]
    public string StateWishName { get; set; } = string.Empty;

    [Column("state_doing")]
    [FieldVersion(10003, 99999, "")]
    public string StateDoingName { get; set; } = string.Empty;

    [Column("state_complete")]
    [FieldVersion(10003, 99999, "")]
    public string StateCompleteName { get; set; } = string.Empty;

    [Column("state_giveup")]
    [FieldVersion(10003, 99999, "")]
    public string StateGiveupName { get; set; } = string.Empty;
}


[Table("t_record")]
public class Record
{
    [PrimaryKey, AutoIncrement]
    [Column("id")]
    public int Id { get; set; }

    [Column("name"), Indexed]
    public string Name { get; set; } = string.Empty;

    [Column("alias")]
    public string Alias { get; set; } = string.Empty;

    [Column("kind")]
    public int Kind { get; set; }   // table t_kind

    [Column("episode")]
    public int Episode { get; set; }

    [Column("episode_count")]
    public int EpisodeCount { get; set; }

    [Column("state")]
    public RecordState State { get; set; }

    [Column("remark"), MaxLength(1000)]
    public string Remark { get; set; } = string.Empty;

    [Column("publish_year")]
    public int PublishYear { get; set; }

    [Column("publish_month")]
    public int PublishMonth { get; set; }

    [Column("modify_date")]
    public DateTime ModifyDate { get; set; }

    [Column("rating")]
    [FieldVersion(10010, 99999, 0)]
    public int Rating { get; set; }
}


