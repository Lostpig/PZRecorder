﻿using SQLite;

namespace PZPKRecorder.Data;

[Table("t_process_watch")]
[TableVersion(10007, 99999)]
internal class ProcessWatch
{
    [PrimaryKey, AutoIncrement]
    [Column("id")]
    public int Id { get; set; }

    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [Column("enabled")]
    public bool Enabled { get; set; } = true;

    [Column("remark"), MaxLength(1000)]
    public string Remark { get; set; } = string.Empty;

    [Column("process_name")]
    public string ProcessName { get; set; } = string.Empty;

    [Column("binding_daily")]
    public bool BindingDaily { get; set; } = false;

    [Column("daily_id")]
    public int DailyId { get; set; }

    [Column("daily_duration")]
    public int DailyDuration { get; set; }

    [Column("order_no")]
    public int OrderNo { get; set; }

    [Ignore]
    public string StateText => Enabled switch
    {
        true => Localization.LocalizeDict.Enabled,
        false => Localization.LocalizeDict.Disabled,
    };
}


[Table("t_process_record")]
[TableVersion(10007, 99999)]
internal class ProcessRecord
{
    [PrimaryKey, AutoIncrement]
    [Column("id")]
    public int Id { get; set; }

    [Column("pid")]
    public int Pid { get; set; }

    [Column("date")]
    public int Date { get; set; } // DateOnly.DayNumber

    [Column("start_time")]
    public int StartTime { get; set; }

    [Column("end_time")]
    public int EndTime { get; set; }

    [Ignore]
    public string DateText
    {
        get
        {
            var day = DateOnly.FromDayNumber(Date);
            return $"{day.Year}-{day.Month}-{day.Day}";
        }
    }
    [Ignore]
    public string StartTimeText
    {
        get
        {
            var time = TimeSpan.FromSeconds(StartTime);
            return time.ToString("g");
        }
    }
    [Ignore]
    public string EndTimeText
    {
        get
        {
            var time = TimeSpan.FromSeconds(EndTime);
            return time.ToString("g");
        }
    }
    [Ignore]
    public string DurationText
    {
        get
        {
            var duration = TimeSpan.FromSeconds(EndTime - StartTime);
            return duration.ToString("g");
        }
    }
}