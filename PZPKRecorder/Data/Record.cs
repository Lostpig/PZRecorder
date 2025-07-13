using SQLite;
using LD = PZPKRecorder.Localization.LocalizeDict;

namespace PZPKRecorder.Data;

enum RecordState
{
    Wish = 0,
    Doing = 1,
    Complete = 2,
    Giveup = 3,
}

[Table("t_record")]
internal class Record
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

    [Ignore]
    public string StateText => State switch
    {
        RecordState.Wish => LD.Wish,
        RecordState.Doing => LD.Doing,
        RecordState.Complete => LD.Complete,
        RecordState.Giveup => LD.Giveup,
        _ => "",
    };

    [Column("remark"), MaxLength(1000)]
    public string Remark { get; set; } = string.Empty;

    [Column("publish_year")]
    public int PublishYear { get; set; }

    [Column("publish_month")]
    public int PublishMonth { get; set; }

    [Column("modify_date")]
    public DateTime ModifyDate { get; set; }
}
