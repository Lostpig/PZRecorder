using Avalonia.Styling;
using Microsoft.Extensions.DependencyInjection;
using PZRecorder.Core.Tables;
using PZRecorder.Desktop.Extensions;
using PZRecorder.Desktop.Modules.Shared;
using Ursa.Controls;

namespace PZRecorder.Desktop.Modules.Settings;

internal class VariantsDialog : DialogContentBase<int>
{
    protected override StyleGroup? BuildStyles() => Shared.Styles.ListStyles();
    protected override Control Build()
    {
        return PzGrid(rows: "50, *")
            .Children(
                HStackPanel().Row(0)
                    .Spacing(8)
                    .Margin(0, 8)
                    .Children(
                        PzText(LD.Variants, "H4")
                            .Align(Aligns.Bottom)
                            .Theme(StaticResource<ControlTheme>("TitleTextBlock"))
                    ),
                new ScrollViewer().Row(1)
                    .Content(
                        new ItemsControl()
                            .ItemsPanel(VStackPanel().Spacing(4))
                            .ItemsSource(() => Variants)
                            .ItemTemplate<VariantTable, ItemsControl>(VariantItemTemplate)
                    )
            );
    }
    private static Grid VariantItemTemplate(VariantTable item)
    {
        return PzGrid(cols: "150, *")
            .Classes("ListRow")
            .Children(
                PzText(item.Key).Col(0),
                PzText(item.Value).Col(1)
            );
    }

    private readonly VariantTable[] Variants;
    public VariantsDialog(): base()
    {
        Width = 480;
        Height = 480;
        Title = LD.Variants;
        Variants = ServiceProvider.GetService<VariantsManager>()?.GetAll() ?? [];
    }

    public override PzDialogResult<int> GetResult(DialogResult buttonValue)
    {
        return new PzDialogResult<int>(0, buttonValue);
    }
    public override bool Check(DialogResult buttonValue) => true;
    public override Shared.DialogButton[] Buttons()
    {
        return [
            new Shared.DialogButton(LD.Close, DialogResult.None)
        ];
    }
}
