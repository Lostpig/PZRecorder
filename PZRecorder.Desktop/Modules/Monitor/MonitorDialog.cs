using Avalonia.Interactivity;
using Avalonia.Styling;
using Microsoft.Extensions.DependencyInjection;
using PZ.RxAvalonia.DataValidations;
using PZ.RxAvalonia.Extensions;
using PZRecorder.Core.Managers;
using PZRecorder.Core.Tables;
using PZRecorder.Desktop.Common;
using PZRecorder.Desktop.Extensions;
using PZRecorder.Desktop.Modules.Shared;
using Ursa.Common;

namespace PZRecorder.Desktop.Modules.Monitor;
using TbDaily = Core.Tables.Daily;

internal class MonitorDialog : DialogContentBase<ProcessWatch>
{
    private ProcessWatch Model { get; set; }
    private readonly bool _isAdd = false;
    private readonly List<TbDaily> dailys;

    public MonitorDialog() : base()
    {
        _isAdd = true;
        Title = LD.Add;
        Model = new();

        dailys = GlobalInstances.Services.GetRequiredService<DailyManager>().GetDailies();
    }
    public MonitorDialog(ProcessWatch item) : base()
    {
        _isAdd = false;
        Title = LD.EditDaily;
        Model = new()
        {
            Id = item.Id,
            Name = item.Name,
            ProcessName = item.ProcessName,
            Remark = item.Remark,
            OrderNo = item.OrderNo,
            Enabled = item.Enabled,
            BindingDaily = item.BindingDaily,
            DailyDuration = item.DailyDuration,
            DailyId = item.DailyId,
        };

        dailys = GlobalInstances.Services.GetRequiredService<DailyManager>().GetDailies();
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
                    PzTextBox(() => Model.ProcessName)
                        .OnTextChanged(e => Model.ProcessName = e.Text())
                        .FormLabel(() => LD.ProcessName)
                        .FormRequired(true)
                        .Validation(DataValidations.Required())
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
                        .IsChecked(() => Model.Enabled)
                        .OnIsCheckedChanged(e => Model.Enabled = GetChecked(e))
                        .FormLabel(() => LD.Enabled),
                    new ToggleSwitch()
                        .Theme(StaticResource<ControlTheme>("SimpleToggleSwitch"))
                        .IsChecked(() => Model.BindingDaily)
                        .OnIsCheckedChanged(e => Model.BindingDaily = GetChecked(e))
                        .FormLabel(() => LD.BindingDaily),
                    new ComboBox()
                        .Align(Aligns.HStretch)
                        .FormLabel(() => LD.Daily)
                        .ItemsSource(dailys)
                        .ItemTemplate<TbDaily, ComboBox>(d => PzText(d?.Name ?? ""))
                        .SelectedValue(() => dailys.FirstOrDefault(d => d.Id == Model.DailyId))
                        .OnSelectionChanged(e => Model.DailyId = e.ValueObj<TbDaily>()?.Id ?? 0),
                    PzNumericInt(() => Model.DailyDuration)
                        .OnValueChanged(n => Model.DailyDuration = n ?? 0)
                        .FormLabel(() => LD.Duration)
                        .DataValidation(DataValidations.MinValue(0))
                )
            );
    }
    private static bool GetChecked(RoutedEventArgs e)
    {
        if (e.Source is ToggleSwitch ts)
        {
            return ts.IsChecked ?? false;
        }
        return false;
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
    public override PzDialogResult<ProcessWatch> GetResult(Uc.DialogResult buttonValue)
    {
        return new(Model, buttonValue);
    }
}
