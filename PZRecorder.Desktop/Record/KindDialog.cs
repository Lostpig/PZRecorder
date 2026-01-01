using Avalonia.Layout;
using PZRecorder.Core.Tables;
using PZRecorder.Desktop.Common;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace PZRecorder.Desktop.Record;

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
        return [
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

internal class KindDialog : DialogContentBase<Kind>
{
    private static readonly KindDialogModel Model = new();
    public KindDialog(Kind kind)
    {
        Width = 480;
        Model.Kind.OnNext(kind);
    }
    protected override IEnumerable<IDisposable> WhenActivate()
    {
        return Model.Activate();
    }

    private Grid TextRow(string label, Subject<string> subject)
    {
        return PzGrid("auto, 4, 1*")
            .ColSharedGroup(0, "FieldColumn")
            .Children(
                PzText(label).Col(0).VerticalAlignment(VerticalAlignment.Center),
                PzTextBox(subject).Col(2)
            );
    }
    private Grid NumericRow(string label, Subject<int> subject)
    {
        return PzGrid("auto, 4, 1*")
            .ColSharedGroup(0, "FieldColumn")
            .Children(
                PzText(label).Col(0).VerticalAlignment(VerticalAlignment.Center),
                new Uc.NumericIntUpDown().Col(2)
                    .ValueEx(subject)
            );
    }
    protected override Control Build()
    {
        return VStackPanel(HorizontalAlignment.Stretch)
            .Grid_IsSharedSizeScope(true)
            .Spacing(10)
            .Children(
                TextRow("Name", Model.KindName),
                NumericRow("OrderNo", Model.Order),
                new Separator(),
                PzText("Custom State Text").Classes("h5"),
                TextRow("Wish", Model.StateWishName),
                TextRow("Doing", Model.StateDoingName),
                TextRow("Completed", Model.StateCompleteName),
                TextRow("Give up", Model.StateGiveupName)
            );
    }

    public override Kind GetResult(Uc.DialogResult btnValue)
    {
        return Model.Kind.Value;
    }
    public override bool Check(Uc.DialogResult btnValue)
    {
        if (btnValue == Uc.DialogResult.OK || btnValue == Uc.DialogResult.Yes)
            return !string.IsNullOrWhiteSpace(Model.Kind.Value.Name);
        else return true;
    }
}
