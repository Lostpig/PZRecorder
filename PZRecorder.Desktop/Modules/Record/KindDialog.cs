using Avalonia.Layout;
using PZ.RxAvalonia.DataValidations;
using PZ.RxAvalonia.Extensions;
using PZRecorder.Core.Tables;
using PZRecorder.Desktop.Common;
using PZRecorder.Desktop.Extensions;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Ursa.Common;

namespace PZRecorder.Desktop.Modules.Record;

internal class KindDialogModel
{
    public BehaviorSubject<Kind> Kind { get; init; } = new(new());
    public Subject<string> KindName { get; init; } = new();
    public Subject<int> Order { get; init; } = new();
    public Subject<string> StateWishName { get; init; } = new();
    public Subject<string> StateDoingName { get; init; } = new();
    public Subject<string> StateCompleteName { get; init; } = new();
    public Subject<string> StateGiveupName { get; init; } = new();

    public IEnumerable<IDisposable> Activate()
    {
        return 
        [
            Kind.Subscribe(k => {
                KindName.OnNext(k.Name);
                Order.OnNext(k.OrderNo);
                StateWishName.OnNext(k.StateWishName);
                StateDoingName.OnNext(k.StateDoingName);
                StateCompleteName.OnNext(k.StateCompleteName);
                StateGiveupName.OnNext(k.StateGiveupName);
            }),
            KindName.Subscribe(n => Kind.Value.Name = n),
            Order.Subscribe(n => Kind.Value.OrderNo = n),
            StateWishName.Subscribe(n => Kind.Value.StateWishName = n),
            StateDoingName.Subscribe(n => Kind.Value.StateDoingName = n),
            StateCompleteName.Subscribe(n => Kind.Value.StateCompleteName = n),
            StateGiveupName.Subscribe(n => Kind.Value.StateGiveupName = n),
        ];
    }
}

internal class KindDialog : DialogContentBase<Kind?>
{
    private static readonly KindDialogModel Model = new();
    private readonly bool _isAdd = false;
    public KindDialog()
    {
        _isAdd = true;
        Title = "Add Kind";
        Model.Kind.OnNext(new Kind());
    }
    public KindDialog(Kind kind)
    {
        _isAdd = false;
        Title = "Edit Kind";
        Model.Kind.OnNext(kind);
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
        return VStackPanel()
            .Align(Aligns.HStretch)
            .Spacing(10)
            .Children(
                new Uc.Form() { LabelPosition = Position.Left, LabelWidth = GridLength.Star }
                .Align(Aligns.HStretch)
                .Items(
                    PzTextBox(Model.KindName)
                        .FormLabel("Name")
                        .FormRequired(true)
                        .Validation(DataValidations.Required())
                        .Validation(DataValidations.MaxLength(30)),
                    PzNumericInt(Model.Order).FormLabel("Order No"),
                    new Uc.Divider().Content("Custom State Name"),
                    PzTextBox(Model.StateWishName).FormLabel("Wish"),
                    PzTextBox(Model.StateDoingName).FormLabel("Doing"),
                    PzTextBox(Model.StateCompleteName).FormLabel("Complete"),
                    PzTextBox(Model.StateGiveupName).FormLabel("Give up")
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

    public override Kind? GetResult(Uc.DialogResult btnValue)
    {
        if (PzDialogManager.IsSureResult(btnValue))
        {
            return Model.Kind.Value;
        }
        return null;
    }
    public override bool Check(Uc.DialogResult btnValue)
    {
        return CheckDataValidation();
    }
}
