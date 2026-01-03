using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Avalonia.Styling;
using Microsoft.Extensions.DependencyInjection;
using PZRecorder.Core.Managers;
using PZRecorder.Core.Tables;
using PZRecorder.Desktop.Common;
using SQLitePCL;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace PZRecorder.Desktop.Record;

using TbRecord = Core.Tables.Record;


internal class RecordPage : PZPageBase
{
    private Border RecordItemTemplete(TbRecord record)
    {
        var content = VStackPanel()
            .Children(
                HStackPanel()
                    .Spacing(8)
                    .Children(
                        PzText(record.Name)
                            .FontSize(24)
                            .Foreground(StaticColor("SemiColorLink")),
                        PzText(record.Alias)
                            .Foreground(StaticColor("SemiColorText2"))
                    ),
                new DockPanel()
                    .LastChildFill(false)
                    .Children(
                        PzText($"{record.Episode} / {record.EpisodeCount}").Dock(Dock.Left),
                        PzText($"{record.PublishYear}-{record.PublishMonth}").Dock(Dock.Left),
                        PzText($"{record.ModifyDate:yyyy-MM-dd HH:mm}").Dock(Dock.Right)
                    ),
                PzText(record.Remark)
                    .FontSize(14)
                    .Foreground(StaticColor("SemiColorText3"))
                    .TextWrapping(TextWrapping.Wrap)
            );
        var rightBar = new Border()
            .Col(1)
            .BorderThickness(0, 0, 1, 0)
            .BorderBrush(StaticColor("SemiColorBorder"))
            .Child(
                VStackPanel()
                    .Spacing(8)
                    .Children(
                        PzText(record.State.ToString()),
                        PzButton("Edit"),
                        PzButton("Remove")
                    )
            );

        return new Border()
            .Theme(StaticResource<ControlTheme>("CardBorder"))
            .Child(
                PzGrid(cols: "*, 80")
                .Children(
                    content.Col(0),
                    rightBar.Col(1)
                )
            );
    }
    private TabStrip BuildTabs()
    {
        return new TabStrip()
            .Theme(StaticResource<ControlTheme>("ScrollLineTabStrip"))
            .ItemsSource(Kinds)
            .ItemTemplate<Kind, TabStrip>(k => PzText(k.Name))
            .SelectedItemEx(SelectedKind);
    }
    private DockPanel BuildList()
    {
        return new DockPanel()
            .Children(
                new ScrollViewer().Dock(Dock.Right)
                    // .AllowAutoHide(true)
                    .Content(
                        new ItemsControl()
                            .ItemsPanel(VStackPanel(Aligns.HStretch).Spacing(12))
                            .ItemsSource(Records)
                            .ItemTemplate<TbRecord, ItemsControl>(RecordItemTemplete)
                    )
            );
    }
    private StackPanel BuildSearchPanel()
    {
        return VStackPanel(Aligns.Left, Aligns.VStretch)
            .Spacing(16)
            .Children(
                new TextBox().Width(200).Watermark("Search..."),
                new Border().Theme(StaticResource<ControlTheme>("CardBorder"))
                .Child(
                    VStackPanel()
                        .Spacing(8)
                        .Children(
                            PzText("Year"),
                            new ComboBox()
                                .ItemsSource(Years),
                            PzText("Month"),
                            new ComboBox()
                                .ItemsSource(Years)
                        )
                )
            );
    }
    private Grid BuildContent()
    {
        return PzGrid(cols: "*, auto")
            .ColumnSpacing(8)
            .Children(
                BuildList().Col(0),
                BuildSearchPanel().Col(1)
            );
    }
    protected override Control Build() => 
        PzGrid(rows: "auto, *")
            .Margin(8)
            .RowSpacing(8)
            .Children(
                BuildTabs().Row(0),
                BuildContent().Row(1)
            );

    private readonly RecordManager _manager;
    private Subject<List<Kind>> Kinds { get; init; } = new();
    private BehaviorSubject<Kind?> SelectedKind { get; init; } = new(null);
    private Subject<List<TbRecord>> Records { get; init; } = new();

    internal static readonly int[] Years = [2025, 2024];

    public RecordPage() : base(ViewInitializationStrategy.Lazy)
    {
        _manager = ServiceProvider.GetRequiredService<RecordManager>();
        Initialize();
    }

    protected override IEnumerable<IDisposable> WhenActivate()
    {
        var kinds = _manager.GetKinds();
        Kinds.OnNext(kinds);
        if (SelectedKind.Value == null || !kinds.Any(k => k.Id == SelectedKind.Value.Id))
        {
            SelectedKind.OnNext(kinds.FirstOrDefault());
        }

        return [
            SelectedKind.Subscribe(UpdateList),
            ..base.WhenActivate()
        ];
    }
    private void UpdateList(Kind? kind)
    {
        if (kind == null)
        {
            Records.OnNext([]);
        }
        else
        {
            var records = _manager.GetKindRecords(kind.Id);
            Records.OnNext(records);
        }
    }
}
