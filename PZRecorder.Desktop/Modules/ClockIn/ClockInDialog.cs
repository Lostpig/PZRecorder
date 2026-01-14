using PZ.RxAvalonia.DataValidations;
using PZRecorder.Desktop.Extensions;
using PZRecorder.Desktop.Modules.Shared;
using PZ.RxAvalonia.Extensions;
using Ursa.Common;

namespace PZRecorder.Desktop.Modules.ClockIn;
using TbClockIn = PZRecorder.Core.Tables.ClockIn;

internal class ClockInDialog : DialogContentBase<TbClockIn>
{
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
                    PzNumericInt(() => Model.RemindDays)
                        .OnValueChanged(v => Model.RemindDays = v ?? 0)
                        .FormLabel(() => LD.RemindDays)
                        .DataValidation(DataValidations.MinValue(0)),
                    PzTextBox(() => Model.Remark)
                        .OnTextChanged(e => Model.Remark = e.Text())
                        .FormLabel(() => LD.Remark)
                        .Classes("TextArea")
                        .Validation(DataValidations.MaxLength(500)),
                    PzNumericInt(() => Model.OrderNo)
                        .OnValueChanged(n => Model.OrderNo = n ?? 0)
                        .FormLabel(() => LD.OrderBy)
                        .DataValidation(DataValidations.MaxValue(99999))
                )
            );
    }

    private TbClockIn Model { get; set; }
    private readonly bool _isAdd = false;
    public ClockInDialog() : base()
    {
        _isAdd = true;
        Title = LD.AddClockIn;
        Model = new();
    }
    public ClockInDialog(TbClockIn item) : base()
    {
        _isAdd = false;
        Title = LD.EditClockIn;
        Model = new()
        {
            Id = item.Id,
            Name = item.Name,
            RemindDays = item.RemindDays,
            Remark = item.Remark,
            OrderNo = item.OrderNo,
        };
    }
    protected override void OnCreated()
    {
        base.OnCreated();
        Width = 480;
        RegisterDataValidation();
    }

    public override DialogButton[] Buttons()
    {
        return [
            new DialogButton(_isAdd ? LD.Add : LD.Save, Uc.DialogResult.OK) { Validation = true },
            new DialogButton(LD.Cancel, Uc.DialogResult.Cancel) { Styles = ["Tertiary"] }
        ];
    }
    public override bool Check(Uc.DialogResult buttonValue)
    {
        return CheckDataValidation();
    }
    public override PzDialogResult<TbClockIn> GetResult(Uc.DialogResult buttonValue)
    {
        return new(Model, buttonValue);
    }
}
