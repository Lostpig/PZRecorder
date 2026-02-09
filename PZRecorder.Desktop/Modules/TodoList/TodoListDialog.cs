using PZ.RxAvalonia.DataValidations;
using PZ.RxAvalonia.Extensions;
using PZRecorder.Desktop.Extensions;
using PZRecorder.Desktop.Modules.Shared;
using Ursa.Common;

namespace PZRecorder.Desktop.Modules.TodoList;

using TbTodoList = PZRecorder.Core.Tables.TodoList;

public class TodoListDialog : DialogContentBase<TbTodoList>
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
                        .Validation(DataValidations.MaxLength(20)),
                    PzTextBox(() => Model.Remark)
                        .OnTextChanged(e => Model.Remark = e.Text())
                        .FormLabel(() => LD.Remark)
                        .Classes("TextArea")
                        .Validation(DataValidations.MaxLength(500))
                )
            );
    }

    private TbTodoList Model { get; set; }
    private readonly bool _isAdd = false;
    public TodoListDialog() : base()
    {
        _isAdd = true;
        Title = LD.Add;
        Model = new();
    }
    public TodoListDialog(TbTodoList item) : base()
    {
        _isAdd = false;
        Title = LD.Edit;
        Model = new()
        {
            Id = item.Id,
            Name = item.Name,
            AddTime = item.AddTime,
            Remark = item.Remark,
            Completed = item.Completed,
            CompleteTime = item.CompleteTime,
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
    public override PzDialogResult<TbTodoList> GetResult(Uc.DialogResult buttonValue)
    {
        return new(Model, buttonValue);
    }
}
