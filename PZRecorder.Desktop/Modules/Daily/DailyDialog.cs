using Avalonia.Interactivity;
using Avalonia.Styling;
using PZ.RxAvalonia.DataValidations;
using PZ.RxAvalonia.Extensions;
using PZRecorder.Desktop.Extensions;
using PZRecorder.Desktop.Modules.Shared;
using Ursa.Common;

namespace PZRecorder.Desktop.Modules.Daily;

using TbDaily = PZRecorder.Core.Tables.Daily;

internal sealed class DailyDialog : DialogContentBase<TbDaily>
{
    private TbDaily Model { get; set; }
    private readonly bool _isAdd = false;
    private bool Enabled
    {
        get => Model.State == Core.Common.EnableState.Enabled; 
        set
        {
            Model.State = value ? Core.Common.EnableState.Enabled : Core.Common.EnableState.Disabled;
        }
    }
    private DateTimeOffset StartDate { get; set; }
    private DateTimeOffset EndDate { get; set; }

    public DailyDialog() : base()
    {
        _isAdd = true;
        Title = LD.AddDaily;
        Model = new();

        StartDate = DateOnly.FromDayNumber(Model.StartDay).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        EndDate = DateOnly.FromDayNumber(Model.EndDay).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
    }
    public DailyDialog(TbDaily item) : base()
    {
        _isAdd = false;
        Title = LD.EditDaily;
        Model = new()
        {
            Id = item.Id,
            Name = item.Name,
            Alias = item.Alias,
            Remark = item.Remark,
            State = item.State,
            StartDay = item.StartDay,
            EndDay = item.EndDay,
            OrderNo = item.OrderNo,
        };

        StartDate = DateOnly.FromDayNumber(Model.StartDay).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        EndDate = DateOnly.FromDayNumber(Model.EndDay).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
    }
    protected override void OnCreated()
    {
        base.OnCreated();
        Width = 480;
        RegisterDataValidation();
    }

    protected override Control Build()
    {
        return VStackPanel()
            .Align(Aligns.HStretch)
            .Spacing(10)
            .Children(
                new Uc.Form() { LabelPosition = Position.Left, LabelWidth = GridLength.Star }
                .Align(Aligns.HStretch)
                .Items(
                    PzTextBox(() => Model.Name)
                        .OnTextChanged(e => Model.Name = e.Text())
                        .FormLabel(() => LD.Name)
                        .FormRequired(true)
                        .Validation(DataValidations.Required())
                        .Validation(DataValidations.MaxLength(10)),
                    PzTextBox(() => Model.Alias)
                        .OnTextChanged(e => Model.Alias = e.Text())
                        .FormLabel(() => LD.Alias)
                        .Validation(DataValidations.MaxLength(30)),
                    PzTextBox(() => Model.Remark)
                        .OnTextChanged(e => Model.Remark = e.Text())
                        .FormLabel(() => LD.Remark)
                        .Classes("TextArea")
                        .Validation(DataValidations.MaxLength(500)),
                    PzNumericInt(() => Model.OrderNo)
                        .OnValueChanged(n => Model.OrderNo = n ?? 0)
                        .FormLabel(() => LD.OrderBy)
                        .DataValidation(DataValidations.MaxValue(99999)),
                    new Uc.Divider().Content(() => LD.State),
                    new ToggleSwitch()
                        .Theme(StaticResource<ControlTheme>("SimpleToggleSwitch"))
                        .IsChecked(() => Enabled)
                        .OnIsCheckedChanged(e => Enabled = GetChecked(e))
                        .FormLabel(() => LD.Enabled),
                    new DatePicker()
                        .FormLabel(() => LD.StartTime)
                        .SelectedDate(() => StartDate)
                        .OnSelectedDateChanged(e =>
                        {
                            if (e.NewDate.HasValue) StartDate = e.NewDate.Value.DateTime;
                        }),
                    new DatePicker()
                        .FormLabel(() => LD.EndTime)
                        .SelectedDate(() => EndDate)
                        .OnSelectedDateChanged(e =>
                        {
                            if (e.NewDate.HasValue) EndDate = e.NewDate.Value.DateTime;
                        })
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
    private static bool GetChecked(RoutedEventArgs e)
    {
        if (e.Source is ToggleSwitch ts)
        {
            return ts.IsChecked ?? false;
        }
        return false;
    }

    public override PzDialogResult<TbDaily> GetResult(Uc.DialogResult buttonValue)
    {
        Model.StartDay = DateOnly.FromDateTime(StartDate.DateTime).DayNumber;
        Model.EndDay = DateOnly.FromDateTime(EndDate.DateTime).DayNumber;

        return new(Model, buttonValue);
    }
    public override bool Check(Uc.DialogResult buttonValue)
    {
        return CheckDataValidation();
    }
}
