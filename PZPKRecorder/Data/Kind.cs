using SQLite;

namespace PZPKRecorder.Data;
using LD = Localization.LocalizeDict;

[Table("t_kind")]
internal class Kind
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

    public string GetStateName(RecordState state)
    {
        return state switch
        {
            RecordState.Complete => string.IsNullOrWhiteSpace(StateCompleteName) ? LD.Complete : StateCompleteName,
            RecordState.Giveup => string.IsNullOrWhiteSpace(StateGiveupName) ? LD.Giveup : StateGiveupName,
            RecordState.Doing => string.IsNullOrWhiteSpace(StateDoingName) ? LD.Doing : StateDoingName,
            RecordState.Wish => string.IsNullOrWhiteSpace(StateWishName) ? LD.Wish : StateWishName,
            _ => throw new NotImplementedException()
        };
    }
}
