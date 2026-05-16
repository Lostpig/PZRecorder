using Avalonia.Platform.Storage;
using Avalonia.Styling;
using PZRecorder.Core.Data;
using PZRecorder.Desktop.Common;
using PZRecorder.Desktop.Extensions;
using PZRecorder.Desktop.Localization;
using PZRecorder.Desktop.Modules.Shared;

namespace PZRecorder.Desktop.Modules.Settings;

internal sealed class SettingsPage(VariantsManager _manager, Translate _translate, ImportManager _import, ExportManager _export, ErrorProxy _errProxy, BroadcastManager _broadcast) : MvuPage()
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
                            IconButton(MIcon.FileImport,() => $"{LD.Import} Json").OnClick(_ => ImportJson()),
                            IconButton(MIcon.FileExport,() => $"{LD.Export} Json").OnClick(_ => ExportJson()),
                            IconButton(MIcon.DatabaseImport,() => $"{LD.Import} DB").OnClick(_ => ImportDB()),
                            IconButton(MIcon.DatabaseExport,() => $"{LD.Export} DB").OnClick(_ => ExportDB())
                        )
                )
            );
    }
    private Border BuildVariantsPanel()
    {
        return new Border()
            .Padding(16)
            .Child(
                HStackPanel().Children(
                    PzButton(() => LD.Variants, "H4").OnClick(_ => ShowVariants())
                )
            );
    }
    protected override Control Build()
    {
        return PzGrid(rows: "auto, auto, auto")
            .RowSpacing(8)
            .Children(
                BuildSettingItems().Row(0),
                BuildDataOperatorPanel().Row(1),
                BuildVariantsPanel().Row(2)
            );
    }

    private static readonly string[] ThemeNames = ["default", "dark", "light"];
    private LanguageItem? CurrentLanguge { get; set; }
    private string CurrentTheme { get; set; } = "default";

    protected override IEnumerable<IDisposable> WhenActivate()
    {
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

    private async void ImportJson() 
    {
        var sure = await PzDialogManager.Confirm(LD.ImportConfirmMessage, LD.Warning);
        if (!PzDialogManager.IsSureResult(sure.Result)) return;

        var result = await GlobalInstances.MainWindow.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions()
        {
            Title = LD.Import,
            FileTypeFilter = [new("json file") { Patterns = ["*.json"] }],
            AllowMultiple = true,
        });

        if (result.Count >= 1)
        {
            try
            {
                var backup = Utility.GetBackupDBPath();
                _import.ImportFromJson(result[0].Path.LocalPath, backup);
                Notification.Success(LD.ImportSuccess);
                _broadcast.Publish(BroadcastEvent.DataImported);
            }
            catch (Exception ex)
            {
                _errProxy.CatchException(ex);
            }
        }
    }
    private async void ImportDB() 
    {
        var sure = await PzDialogManager.Confirm(LD.ImportConfirmMessage, LD.Warning);
        if (!PzDialogManager.IsSureResult(sure.Result)) return;

        var result = await GlobalInstances.MainWindow.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions()
        {
            Title = LD.Import,
            FileTypeFilter = [new("db file") { Patterns = ["*.db"] }],
            AllowMultiple = true,
        });

        if (result.Count >= 1)
        {
            try
            {
                var backup = Utility.GetBackupDBPath();
                _import.ImportFromDB(result[0].Path.LocalPath, backup);
                Notification.Success(LD.ImportSuccess);
                _broadcast.Publish(BroadcastEvent.DataImported);
            }
            catch (Exception ex) 
            {
                _errProxy.CatchException(ex);
            }
        }
    }
    private async void ExportJson() 
    {
        var file = await GlobalInstances.MainWindow.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = LD.Export,
            SuggestedFileName = $"pzrecorder-{DateTime.Now:yyyyMMddHHmmss}.json",
            DefaultExtension = "json",
        });

        if (file is not null)
        {
            try
            {
                var localPath = file.Path.LocalPath;
                _export.ExportJson(localPath, false);
                Notification.Success(LD.ExportSuccess);
            }
            catch (Exception ex)
            {
                _errProxy.CatchException(ex);
            }
        }
    }
    private async void ExportDB() 
    {
        var file = await GlobalInstances.MainWindow.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = LD.Export,
            SuggestedFileName = $"pzrecorder-{DateTime.Now:yyyyMMddHHmmss}.db",
            DefaultExtension = "db",
        });

        if (file is not null)
        {
            try
            {
                var localPath = file.Path.LocalPath;
                _export.ExportDB(localPath);
                Notification.Success(LD.ExportSuccess);
            }
            catch (Exception ex)
            {
                _errProxy.CatchException(ex);
            }
        }
    }
    private static async void ShowVariants()
    {
        await PzDialogManager.ShowDialog(new VariantsDialog());
    }
}
