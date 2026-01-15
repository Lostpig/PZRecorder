using Avalonia.Styling;
using PZRecorder.Core.Managers;
using PZRecorder.Core.Tables;
using PZRecorder.Desktop.Extensions;
using PZRecorder.Desktop.Modules.Shared;
using Ursa.Controls;

namespace PZRecorder.Desktop.Modules.ClockIn;

internal class RecordsDialog : DialogContentBase<ClockInCollection>
{
    protected override StyleGroup? BuildStyles() => Shared.Styles.ListStyles();
    protected override Control Build()
    {
        return PzGrid(rows: "50, 40, *")
            .Children(
                HStackPanel().Row(0)
                    .Spacing(8)
                    .Margin(0, 8)
                    .Children(
                        PzText(Collection.ClockIn.Name, "H4")
                            .Align(Aligns.Bottom)
                            .Theme(StaticResource<ControlTheme>("TitleTextBlock")),
                        PzText(string.Format(LD.ClockInCounter, Collection.Records.Count))
                            .Align(Aligns.Bottom)
                    ),
                PzGrid(cols: "90, *, 120").Row(1)
                    .Classes("ListRowHeader")
                    .Children(
                        PzText(LD.Count).Col(0),
                        PzText(LD.ClockInTime).Col(1),
                        PzText(LD.ApartDays).Col(2)
                    ),
                new PagenationList<ClockInRecordItem, ClockInRecord>(Collection.Records)
                    .Row(2)
                    .ItemCreator(() => new ClockInRecordItem(Collection))
            );
    }

    private ClockInCollection Collection { get; init; }
    public RecordsDialog(ClockInCollection collection) : base()
    {
        Collection = collection;
        Width = 480;
        Height = 480;
        Title = LD.ClockInRecords;
    }

    public override Shared.DialogButton[] Buttons()
    {
        return [
                new Shared.DialogButton(LD.Close, DialogResult.None)
            ];
    }
    public override bool Check(DialogResult buttonValue) => true;
    public override PzDialogResult<ClockInCollection> GetResult(DialogResult buttonValue)
    {
        return new PzDialogResult<ClockInCollection>(Collection, buttonValue);
    }
}

internal class ClockInRecordItem(ClockInCollection Collection) : MvuComponent, IListItemComponent<ClockInRecord>
{
    private int Index { get; set; } = 0;
    private string Time { get; set; } = "";
    private int Distance { get; set; } = 0;

    protected override Control Build()
    {
        return PzGrid(cols: "90, *, 120")
            .Height(40)
            .Classes("ListRow")
            .Children(
                PzText(() => Index.ToString()).Col(0),
                PzText(() => Time).Col(1),
                PzText(() => Distance.ToString()).Col(2)
            );
    }
    public void UpdateItem(ClockInRecord item)
    {
        Index = item.Counter;
        Distance = Collection.GetDaysApart(item.Counter);
        Time = item.Time.ToString("yyyy-MM-dd HH:mm:ss");

        UpdateState();
    }
}