using PZ.RxAvalonia.Extensions;
using PZ.RxAvalonia.DataValidations;
using PZRecorder.Core.Tables;
using PZRecorder.Desktop.Common;
using PZRecorder.Desktop.Extensions;
using System.Reactive.Subjects;
using Ursa.Common;
using PZRecorder.Desktop.Modules.Shared;

namespace PZRecorder.Desktop.Modules.Record;
using TbRecord = PZRecorder.Core.Tables.Record;

internal class RecordDialogModel
{
    public BehaviorSubject<TbRecord> Data { get; init; } = new(new());
    public TbRecord Value => Data.Value;

    public Subject<string> Name { get; init; } = new();
    public Subject<string> Alias { get; init; } = new();
    public Subject<int> Episode { get; init; } = new();
    public Subject<int> EpisodeCount { get; init; } = new();
    public Subject<RecordState> State { get; init; } = new();
    public Subject<string> Remark { get; init; } = new();
    public Subject<DateTimeOffset> PublishDate { get; init; } = new();
    public Subject<int> Rating { get; init; } = new();

    public IEnumerable<IDisposable> Activate()
    {
        return
        [
            Data.Subscribe(d => {
                Name.OnNext(d.Name);
                Alias.OnNext(d.Alias);
                Episode.OnNext(d.Episode);
                EpisodeCount.OnNext(d.EpisodeCount);
                State.OnNext(d.State);
                Remark.OnNext(d.Remark);
                Rating.OnNext(d.Rating);
                PublishDate.OnNext(new DateTimeOffset(d.PublishYear, d.PublishMonth, 1, 0, 0 ,0, TimeSpan.Zero));
            }),
            Name.Subscribe(n => Value.Name = n),
            Alias.Subscribe(n => Value.Alias = n),
            Episode.Subscribe(n => Value.Episode = n),
            EpisodeCount.Subscribe(n => Value.EpisodeCount = n),
            State.Subscribe(n => Value.State = n),
            Remark.Subscribe(n => Value.Remark = n),
            Rating.Subscribe(n => Value.Rating = n),
            PublishDate.Subscribe(ChangePublishDate)
        ];
    }
    public void ChangePublishDate(DateTimeOffset d)
    {
        Value.PublishYear = d.Year;
        Value.PublishMonth = d.Month;
    }
}

internal class RecordDialog : DialogContentBase<TbRecord?>
{
    private static readonly RecordDialogModel Model = new();
    private readonly bool _isAdd = false;

    public RecordDialog(int KindId)
    {
        _isAdd = true;
        Title = "Add Record";
        Model.Data.OnNext(new TbRecord()
        {
            Kind = KindId,
        });
    }
    public RecordDialog(TbRecord record)
    {
        _isAdd = false;
        Title = "Edit Record";
        Model.Data.OnNext(record);
    }
    protected override void OnCreated()
    {
        base.OnCreated();
        Width = 480;
        RegisterDataValidation();
    }
    protected override IEnumerable<IDisposable> WhenActivate()
    {
        return Model.Activate();
    }

    protected override Control Build()
    {
        var epValid = new ThanValidtion<int>(ThanOperation.LessEqual, Model.Value.Episode, Model.EpisodeCount);
        epValid.ErrorMessage = "Episode must less than or equal EpisodeCount";

        return VStackPanel()
            .Align(Aligns.HStretch)
            .Spacing(10)
            .Children(
                new Uc.Form() { LabelPosition = Position.Left, LabelWidth = GridLength.Star }
                .Align(Aligns.HStretch)
                .Items(
                    new Uc.Divider().Content("Base"),
                    PzTextBox(Model.Name).FormLabel("Name")
                        .FormRequired(true)
                        .Validation(DataValidations.Required())
                        .Validation(DataValidations.MaxLength(30)),
                    PzTextBox(Model.Alias).FormLabel("Alias")
                        .Validation(DataValidations.MaxLength(30)),
                    PzTextBox(Model.Remark).FormLabel("Remark")
                        .Classes("TextArea")
                        .Validation(DataValidations.MaxLength(140)),
                    new Uc.NumericIntUpDown().FormLabel("EpisodeCount")
                        .ValueEx(Model.EpisodeCount),
                    new DatePicker().FormLabel("Publish Date")
                        .DayVisible(false)
                        .SelectedDateEx(subject: Model.PublishDate),
                    new Uc.Divider().Content("States"),
                    new ComboBox().FormLabel("State")
                        .ItemsSource(Enum.GetValues<RecordState>())
                        .SelectedValueEx(Model.State),
                    new Uc.NumericIntUpDown().FormLabel("Episode")
                        .ValueEx(Model.Episode)
                        .ThanValidation(epValid),
                    new Uc.Divider().Content("Rating"),
                    new Uc.Rating() { AllowHalf = false, Count = 10 }
                        .ValueEx(Model.Rating)
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

    public override TbRecord? GetResult(Uc.DialogResult btnValue)
    {
        if (PzDialogManager.IsSureResult(btnValue))
        {
            return Model.Value;
        }
        return null;
    }
    public override bool Check(Uc.DialogResult btnValue)
    {
        return CheckDataValidation();
    }
}
