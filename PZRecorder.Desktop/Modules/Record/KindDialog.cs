using Avalonia.Layout;
using PZ.RxAvalonia.DataValidations;
using PZ.RxAvalonia.Extensions;
using PZRecorder.Core.Tables;
using PZRecorder.Desktop.Extensions;
using PZRecorder.Desktop.Modules.Shared;
using Ursa.Common;

namespace PZRecorder.Desktop.Modules.Record;

internal sealed class KindDialog : DialogContentBase<Kind>
{
    private Kind Model { get; set; }
    private readonly bool _isAdd = false;
    public KindDialog() : base()
    {
        _isAdd = true;
        Title = "Add Kind";
        Model = new();
    }
    public KindDialog(Kind kind) : base()
    {
        _isAdd = false;
        Title = "Edit Kind";
        Model = new Kind()
        {
            Id = kind.Id,
            Name = kind.Name,
            OrderNo = kind.OrderNo,
            StateCompleteName = kind.StateCompleteName,
            StateDoingName = kind.StateDoingName,
            StateGiveupName = kind.StateGiveupName,
            StateWishName = kind.StateWishName,
        };
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
                        .FormLabel("Name")
                        .FormRequired(true)
                        .Validation(DataValidations.Required())
                        .Validation(DataValidations.MaxLength(30)),
                    PzNumericInt(() => Model.OrderNo)
                        .OnValueChanged(n => Model.OrderNo = n ?? 0)
                        .FormLabel("Order No")
                        .DataValidation(DataValidations.MaxValue(99999)),
                    new Uc.Divider().Content("Custom State Name"),
                    PzTextBox(() => Model.StateWishName)
                        .OnTextChanged(e => Model.StateWishName = e.Text())
                        .FormLabel("Wish")
                        .Validation(DataValidations.MaxLength(8)),
                    PzTextBox(() => Model.StateDoingName)
                        .OnTextChanged(e => Model.StateDoingName = e.Text())
                        .FormLabel("Doing")
                        .Validation(DataValidations.MaxLength(8)),
                    PzTextBox(() => Model.StateCompleteName)
                        .OnTextChanged(e => Model.StateCompleteName = e.Text())
                        .FormLabel("Complete")
                        .Validation(DataValidations.MaxLength(8)),
                    PzTextBox(() => Model.StateGiveupName)
                        .OnTextChanged(e => Model.StateGiveupName = e.Text())
                        .FormLabel("Give up")
                        .Validation(DataValidations.MaxLength(8))
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

    public override PzDialogResult<Kind> GetResult(Uc.DialogResult btnValue)
    {
        return new(Model, btnValue);
    }
    public override bool Check(Uc.DialogResult btnValue)
    {
        return CheckDataValidation();
    }
}
