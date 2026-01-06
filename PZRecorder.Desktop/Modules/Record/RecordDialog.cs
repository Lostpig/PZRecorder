using PZ.RxAvalonia.Extensions;
using PZ.RxAvalonia.DataValidations;
using PZRecorder.Core.Tables;
using PZRecorder.Desktop.Extensions;
using Ursa.Common;
using PZRecorder.Desktop.Modules.Shared;
using System.Reactive.Subjects;
using System.Diagnostics.CodeAnalysis;

namespace PZRecorder.Desktop.Modules.Record;
using TbRecord = PZRecorder.Core.Tables.Record;

internal sealed class RecordDialog : DialogContentBase<TbRecord>
{
    private readonly TbRecord Model = new();
    private readonly bool _isAdd = false;
    private BehaviorSubject<int> EpisodeCountSub;
    private BehaviorSubject<int> RatingSub;
    private DateTime PublishDate;

    public RecordDialog(int KindId) : base()
    {
        _isAdd = true;
        Title = "Add Record";
        Model = new TbRecord()
        {
            Kind = KindId,
            PublishYear = 2000,
            PublishMonth = 1,
        };

        InitMembers();
    }
    public RecordDialog(TbRecord record) : base()
    {
        _isAdd = false;
        Title = "Edit Record";
        Model = new TbRecord()
        {
            Id = record.Id,
            Kind = record.Kind,
            Name = record.Name,
            Alias = record.Alias,
            Episode = record.Episode,
            EpisodeCount = record.EpisodeCount,
            PublishYear = record.PublishYear,
            PublishMonth = record.PublishMonth,
            Rating = record.Rating,
            State = record.State,
            Remark = record.Remark,
        };

        InitMembers();
    }
    [MemberNotNull(nameof(RatingSub), nameof(PublishDate), nameof(EpisodeCountSub))]
    private void InitMembers()
    {
        RatingSub = new(Model.Rating);
        PublishDate = new(Model.PublishYear, Model.PublishMonth, 1);
        EpisodeCountSub = new(Model.EpisodeCount);
    }

    protected override void OnCreated()
    {
        base.OnCreated();
        Width = 480;
        RegisterDataValidation();
    }
    protected override IEnumerable<IDisposable> WhenActivate()
    {
        return 
        [
            RatingSub.Subscribe(n => Model.Rating = n)
        ];
    }


    protected override Control Build()
    {
        var epValid = new ThanValidtion<int>(ThanOperation.LessEqual, Model.Episode, EpisodeCountSub)
        {
            ErrorMessage = "Episode must less than or equal EpisodeCount"
        };

        return VStackPanel()
            .Align(Aligns.HStretch)
            .Spacing(10)
            .Children(
                new Uc.Form() { LabelPosition = Position.Left, LabelWidth = GridLength.Star }
                .Align(Aligns.HStretch)
                .Items(
                    new Uc.Divider().Content("Base"),
                    PzTextBox(() => Model.Name).FormLabel("Name")
                        .OnTextChanged(e => Model.Name = e.Text())
                        .FormRequired(true)
                        .Validation(DataValidations.Required())
                        .Validation(DataValidations.MaxLength(30)),
                    PzTextBox(() => Model.Alias).FormLabel("Alias")
                        .OnTextChanged(e => Model.Alias = e.Text())
                        .Validation(DataValidations.MaxLength(30)),
                    PzTextBox(() => Model.Remark).FormLabel("Remark")
                        .OnTextChanged(e => Model.Remark = e.Text())
                        .Classes("TextArea")
                        .Validation(DataValidations.MaxLength(140)),
                    PzNumericInt(() => Model.EpisodeCount)
                        .OnValueChanged(n => {
                            Model.EpisodeCount = n ?? 1;
                            EpisodeCountSub.OnNext(n ?? 1);
                        })
                        .FormLabel("EpisodeCount")
                        .DataValidation(DataValidations.MinValue(1)),
                    new DatePicker().FormLabel("Publish Date")
                        .DayVisible(false)
                        .SelectedDate(() => PublishDate)
                        .OnSelectedDateChanged(e =>
                        {
                            if (e.NewDate.HasValue) PublishDate = e.NewDate.Value.DateTime;
                        }),
                    new Uc.Divider().Content("States"),
                    new ComboBox()
                        .FormLabel("State")
                        .ItemsSource(Enum.GetValues<RecordState>())
                        .SelectedValue(() => Model.State)
                        .OnSelectionChanged(e => Model.State = e.ValueStruct<RecordState>() ?? RecordState.Wish),
                    PzNumericInt(() => Model.Episode)
                        .OnValueChanged(n => Model.Episode = n ?? 0)
                        .FormLabel("Episode")
                        .DataValidation(DataValidations.MinValue(0))
                        .ThanValidation(epValid),
                    new Uc.Divider().Content("Rating"),
                    new Uc.Rating() { AllowHalf = false, Count = 10 }
                        .ValueEx(RatingSub)
                )
            );
    }
    public override DialogButton[] Buttons()
    {
        return [
            new DialogButton(_isAdd ? "Add" : "Save", Uc.DialogResult.OK) { Validation = true },
            new DialogButton("Cancel", Uc.DialogResult.Cancel) { Styles = ["Tertiary"] }
        ];
    }

    public override PzDialogResult<TbRecord> GetResult(Uc.DialogResult btnValue)
    {
        return new(Model, btnValue);
    }
    public override bool Check(Uc.DialogResult btnValue)
    {
        return CheckDataValidation();
    }
}
