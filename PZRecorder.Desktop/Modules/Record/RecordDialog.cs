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
    private Dictionary<int, Kind> KindsMap { get; init; }
    private Kind ParentKind => KindsMap[Model.Kind];
    private readonly TbRecord Model;
    private readonly bool _isAdd = false;
    private BehaviorSubject<int> EpisodeCountSub;
    private BehaviorSubject<int> RatingSub;
    private DateTime PublishDate;

    public RecordDialog(int parentKindId, List<Kind> kinds) : base()
    {
        _isAdd = true;
        Title = LD.AddRecord;
        KindsMap = kinds.ToDictionary(k => k.Id);
        Model = new TbRecord()
        {
            Kind = parentKindId,
            PublishYear = DateTime.Now.Year,
            PublishMonth = DateTime.Now.Month,
        };

        InitMembers();
    }
    public RecordDialog(TbRecord record, List<Kind> kinds) : base()
    {
        _isAdd = false;
        Title = LD.EditRecord;
        KindsMap = kinds.ToDictionary(k => k.Id);
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
        Width = 560;
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
        var epValid = new ThanValidtion<int>(ThanOperation.LessEqual, EpisodeCountSub);
        epValid.CustomMessage<ThanValidtion<int>>(v => LD.EposideValidtionMsg);

        return VStackPanel()
            .Align(Aligns.HStretch)
            .Spacing(10)
            .Children(
                new Uc.Form() { LabelPosition = Position.Left, LabelWidth = GridLength.Star }
                .Align(Aligns.HStretch)
                .Items(
                    new Uc.Divider().Content(() => LD.BaseInfo),
                    PzTextBox(() => Model.Name).FormLabel(() => LD.Name)
                        .OnTextChanged(e => Model.Name = e.Text())
                        .FormRequired(true)
                        .Validation(DataValidations.Required())
                        .Validation(DataValidations.MaxLength(50)),
                    PzTextBox(() => Model.Alias).FormLabel(() => LD.Alias)
                        .OnTextChanged(e => Model.Alias = e.Text())
                        .Validation(DataValidations.MaxLength(50)),
                    PzTextBox(() => Model.Remark).FormLabel(() => LD.Remark)
                        .OnTextChanged(e => Model.Remark = e.Text())
                        .MaxHeight(160)
                        .Classes("TextArea")
                        .Validation(DataValidations.MaxLength(140)),
                    PzNumericInt(() => Model.EpisodeCount)
                        .OnValueChanged(n => {
                            Model.EpisodeCount = n ?? 1;
                            EpisodeCountSub.OnNext(n ?? 1);
                        })
                        .FormLabel(() => LD.EpisodeCount)
                        .DataValidation(DataValidations.MinValue(1)),
                    new DatePicker().FormLabel(() => LD.PublishDate)
                        .DayVisible(false)
                        .SelectedDate(() => PublishDate)
                        .OnSelectedDateChanged(e =>
                        {
                            if (e.NewDate.HasValue) PublishDate = e.NewDate.Value.DateTime;
                        }),
                    new ComboBox()
                        .FormLabel(() => LD.Kind)
                        .ItemsSource(KindsMap.Values)
                        .ItemTemplate<Kind, ComboBox>(k => PzText(k.Name))
                        .SelectedValue(() => ParentKind)
                        .OnSelectionChanged(ChangeKind),
                    new Uc.Divider().Content(() => LD.State),
                    new ComboBox()
                        .FormLabel(() => LD.State)
                        .ItemsSource(() => Enum.GetValues<RecordState>())
                        .ItemTemplate<RecordState, ComboBox>(s => PzText(GetStateText(s)))
                        .SelectedValue(() => Model.State)
                        .OnSelectionChanged(e => Model.State = e.ValueStruct<RecordState>() ?? RecordState.Wish),
                    PzNumericInt(() => Model.Episode)
                        .OnValueChanged(n => Model.Episode = n ?? 0)
                        .FormLabel(() => LD.Episode)
                        .DataValidation(DataValidations.MinValue(0))
                        .ThanValidation(epValid),
                    new Uc.Divider().Content(() => LD.Rating),
                    new Uc.Rating() { AllowHalf = false, Count = 10 }
                        .DefaultValue(() => Model.Rating)
                        .ValueEx(RatingSub)
                )
            );
    }
    public override DialogButton[] Buttons()
    {
        return [
            new DialogButton(_isAdd ? LD.Add : LD.Save, Uc.DialogResult.OK) { Validation = true },
            new DialogButton(LD.Cancel, Uc.DialogResult.Cancel) { Styles = ["Tertiary"] }
        ];
    }
    private string GetStateText(RecordState state)
    {
        var kind = ParentKind;

        return state switch
        {
            RecordState.Wish => string.IsNullOrEmpty(kind?.StateWishName) ? LD.Wish : kind.StateWishName,
            RecordState.Doing => string.IsNullOrEmpty(kind?.StateDoingName) ? LD.Doing : kind.StateDoingName,
            RecordState.Complete => string.IsNullOrEmpty(kind?.StateCompleteName) ? LD.Complete : kind.StateCompleteName,
            RecordState.Giveup => string.IsNullOrEmpty(kind?.StateGiveupName) ? LD.Giveup : kind.StateGiveupName,
            _ => "-"
        };
    }
    private void ChangeKind(SelectionChangedEventArgs e)
    {
        var kind = e.ValueObj<Kind>()?.Id ?? KindsMap.First().Key;
        Model.Kind = kind;
        UpdateState();
    }

    public override PzDialogResult<TbRecord> GetResult(Uc.DialogResult btnValue)
    {
        Model.PublishYear = PublishDate.Year;
        Model.PublishMonth = PublishDate.Month;
        return new(Model, btnValue);
    }
    public override bool Check(Uc.DialogResult btnValue)
    {
        return CheckDataValidation();
    }
}
