using Avalonia.Styling;
using PZRecorder.Core.Tables;
using PZRecorder.Desktop.Common;
using PZRecorder.Desktop.Extensions;
using PZRecorder.Desktop.Localization;
using PZRecorder.Desktop.Modules.Shared;

namespace PZRecorder.Desktop.Modules.Settings;

internal sealed class SettingsPage(VariantsManager manager, Translate translate) : MvuPage()
{
    protected override StyleGroup? BuildStyles() => Shared.Styles.ListStyles();

    private Border BuildSettingItems()
    {
        var content = PzGrid(cols: "150, *", rows: "auto, auto")
            .RowSpacing(8)
            .Children(
                PzText(() => LD.Language).Cell(0, 0),
                new ComboBox().Width(200).Cell(1, 0)
                    .ItemsSource(() => _translate.Languages)
                    .ItemTemplate<LanguageItem, ComboBox>(l => PzText(l?.Name ?? ""))
                    .SelectedValue(() => CurrentLanguge)
                    .OnSelectionChanged(SelectLanguage),
                PzText(() => LD.Theme).Cell(0, 1),
                new ComboBox().Width(200).Cell(1, 1)
                    .ItemsSource(() => ThemeNames)
                    .SelectedValue(() => CurrentTheme)
                    .OnSelectionChanged(SelectTheme)
            );

        return new Border()
            .BorderThickness(0, 0, 0, 1)
            .BorderBrush(StaticColor("SemiColorBorder"))
            .Padding(16)
            .Child(
                VStackPanel().Children(
                    PzText(() => LD.Setting, "H4")
                        .Theme(StaticResource<ControlTheme>("TitleTextBlock"))
                        .Margin(0, 16),
                    content
                )
            );
    }
    private Border BuildDataOperatorPanel()
    {
        return new Border()
            .BorderThickness(0, 0, 0, 1)
            .BorderBrush(StaticColor("SemiColorBorder"))
            .Padding(16)
            .Child(
                VStackPanel().Children(
                    PzText(() => $"{LD.Import} / {LD.Export}", "H4")
                        .Theme(StaticResource<ControlTheme>("TitleTextBlock"))
                        .Margin(0, 16),
                    HStackPanel()
                        .Spacing(8)
                        .Children(
                            PzButton(() => $"{LD.Import} Json"),
                            PzButton(() => $"{LD.Export} Json"),
                            PzButton(() => $"{LD.Import} DB"),
                            PzButton(() => $"{LD.Export} DB")
                        )
                )
            );
    }
    private Border BuildVariantsTable()
    {
        return new Border()
            .Padding(16)
            .Child(
                VStackPanel().Children(
                    PzText(() => LD.Variants, "H4")
                        .Theme(StaticResource<ControlTheme>("TitleTextBlock"))
                        .Margin(0, 16),
                    new ScrollViewer()
                        .Content(
                            new ItemsControl()
                                .ItemsPanel(VStackPanel().Spacing(4))
                                .ItemsSource(() => Variants)
                                .ItemTemplate<VariantTable, ItemsControl>(VariantItemTemplate)
                        )
                )
            );
    }
    private Grid VariantItemTemplate(VariantTable item)
    {
        return PzGrid(cols: "150, *")
            .Classes("ListRow")
            .Height(32)
            .Children(
                PzText(item.Key).Col(0),
                PzText(item.Value).Col(1)
            );
    }
    protected override Control Build()
    {
        return PzGrid(rows: "auto, auto, *")
            .RowSpacing(8)
            .Children(
                BuildSettingItems().Row(0),
                BuildDataOperatorPanel().Row(1),
                BuildVariantsTable().Row(2)
            );
    }

    private static readonly string[] ThemeNames = ["default", "dark", "light"];
    private VariantTable[] Variants { get; set; } = [];
    private LanguageItem? CurrentLanguge { get; set; }
    private string CurrentTheme { get; set; } = "default";

    private readonly VariantsManager _manager = manager;
    private readonly Translate _translate = translate;

    protected override IEnumerable<IDisposable> WhenActivate()
    {
        Variants = _manager.GetAll();
        CurrentLanguge = _translate.Current;
        CurrentTheme = GetThemeName();
        UpdateState();

        return base.WhenActivate();
    }
    private static string GetThemeName()
    {
        if (App.RequestedThemeVariant == ThemeVariant.Dark) return "dark";
        if (App.RequestedThemeVariant == ThemeVariant.Light) return "light";
        return "default";
    }

    private void SelectLanguage(SelectionChangedEventArgs e)
    {
        e.Handled = true;
        var value = e.ValueObj<LanguageItem>();
        if (value != null && value != CurrentLanguge)
        {
            CurrentLanguge = value;
            _manager.SetVariant(VariantFields.Language, value.Value);
            Task.Run(() =>
            {
                Thread.Sleep(150);
                _translate.ChangeLanguage(value.Value);
            });
        }
    }
    private void SelectTheme(SelectionChangedEventArgs e)
    {
        e.Handled = true;
        var value = e.ValueObj<string>();

        if (value != null)
        {

            App.RequestedThemeVariant = value switch
            {
                "dark" => ThemeVariant.Dark,
                "light" => ThemeVariant.Light,
                _ => ThemeVariant.Default,
            };
            _manager.SetVariant(VariantFields.Theme, value);
        }
    }
}
