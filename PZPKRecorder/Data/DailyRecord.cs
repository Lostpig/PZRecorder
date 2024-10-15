using SQLite;

namespace PZPKRecorder.Data;

enum DailyState
{
    Disabled = 0,
    Enabled = 1,
}

[Table("t_daily")]
internal class Daily
{
    [PrimaryKey,AutoIncrement]
    [Column("id")]
    public int Id { get; set; }

    [Column("name"), Indexed]
    public string Name { get; set; } = string.Empty;

    [Column("alias")]
    public string Alias { get; set; } = string.Empty;

    [Column("remark")]
    public string Remark { get; set; } = string.Empty;

    [Column("state")]
    public DailyState State { get; set; }

    [Column("order_no")]
    [DataField(10001, 99999, 0)]
    public int OrderNo { get; set; }

    [Ignore]
    public string StateText => State switch
    {
        DailyState.Enabled => Localization.LocalizeDict.Enabled,
        DailyState.Disabled => Localization.LocalizeDict.Disabled,
        _ => ""
    };

    [Column("modify_date")]
    public DateTime ModifyDate { get; set; }
}


[Table("t_dailyweek")]
internal class DailyWeek
{
    [PrimaryKey]
    [Column("id")]
    public int Id { get; set; }

    [Column("daily_id")]
    public int DailyId { get; set; }

    [Column("monday_day")]
    public int MondayDay { get; set; } // DateOnly.DayNumber

    [Ignore]
    public DateOnly MondayDate => DateOnly.FromDayNumber(MondayDay);

    public static int GenerateId(int dailyId, DateOnly date)
    {
        var startDate = new DateOnly(2000, 1, 1);
        int daysFromStart = date.DayNumber - startDate.DayNumber;

        // Max to year 2273
        int id = dailyId * 100000 + daysFromStart;
        return id;
    }
    public int Init(int dailyId, DateOnly date)
    {
        Id = GenerateId(dailyId, date);
        DailyId = dailyId;
        MondayDay = date.DayNumber;

        Day1 = Day2 = Day3 = Day4 = Day5 = Day6 = Day7 = 0;

        return Id;
    }

    // day state 0 = unknown; 1 = complete; 2 = giveup;
    [Column("d1")]
    public int Day1 { get; set; }
    [Column("d2")]
    public int Day2 { get; set; }
    [Column("d3")]
    public int Day3 { get; set; }
    [Column("d4")]
    public int Day4 { get; set; }
    [Column("d5")]
    public int Day5 { get; set; }
    [Column("d6")]
    public int Day6 { get; set; }
    [Column("d7")]
    public int Day7 { get; set; }

    [Ignore]
    public int this[DayOfWeek index]
    {
        get
        {
            return index switch
            {
                DayOfWeek.Monday => Day1,
                DayOfWeek.Tuesday => Day2,
                DayOfWeek.Wednesday => Day3,
                DayOfWeek.Thursday => Day4,
                DayOfWeek.Friday => Day5,
                DayOfWeek.Saturday => Day6,
                DayOfWeek.Sunday => Day7,
                _ => 0
            };
        }
        set
        {
            switch (index) {
                case DayOfWeek.Monday: Day1 = value; break;
                case DayOfWeek.Tuesday: Day2 = value; break;
                case DayOfWeek.Wednesday: Day3 = value; break;
                case DayOfWeek.Thursday: Day4 = value; break;
                case DayOfWeek.Friday: Day5 = value; break;
                case DayOfWeek.Saturday: Day6 = value; break;
                case DayOfWeek.Sunday: Day7 = value; break;
                default: break;
            }
        }
    }
}

// Old version
[Table("t_daily")]
internal class DailyVersion0
{
    [PrimaryKey, AutoIncrement]
    [Column("id")]
    public int Id { get; set; }

    [Column("name"), Indexed]
    public string Name { get; set; } = string.Empty;

    [Column("alias")]
    public string Alias { get; set; } = string.Empty;

    [Column("remark")]
    public string Remark { get; set; } = string.Empty;

    [Column("state")]
    public DailyState State { get; set; }

    [Column("modify_date")]
    public DateTime ModifyDate { get; set; }
}
