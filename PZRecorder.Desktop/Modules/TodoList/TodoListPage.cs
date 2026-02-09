using Avalonia.Media;
using PZRecorder.Core.Managers;
using PZRecorder.Desktop.Extensions;
using PZRecorder.Desktop.Modules.Shared;

namespace PZRecorder.Desktop.Modules.TodoList;

using TbTodoList = PZRecorder.Core.Tables.TodoList;

public class TodoListPage(TodoListManager manager) : MvuPage()
{
    protected override StyleGroup? BuildStyles() => Shared.Styles.ListStyles();

    private StackPanel BuildOperatorBar()
    {
        return HStackPanel()
            .Align(Aligns.HStretch)
            .Spacing(10)
            .Children(
                IconButton(MIcon.Add, () => LD.Add)
                    .OnClick(_ => OnAdd())
            );
    }
    private DockPanel BuildItemsList()
    {
        return new DockPanel()
            .Children(
                PzGrid(cols: "200, 200, 1*, 200")
                .Dock(Dock.Top)
                .Classes("ListRowHeader")
                .Children(
                    PzText(() => LD.StartTime).Col(0).TextAlignment(TextAlignment.Left),
                    PzText(() => LD.Name).Col(1),
                    PzText(() => LD.Complete).Col(2),
                    PzText(() => LD.Action).Col(3).Align(Aligns.HCenter)
                ),
                new ScrollViewer()
                .Dock(Dock.Bottom)
                .Content(
                    new ItemsControl()
                    .ItemsPanel(VStackPanel(Aligns.HStretch).Spacing(5))
                    .ItemsSource(() => Items)
                    .ItemTemplate<TbTodoList, ItemsControl>(ListItemTemplate)
                )
            );
    }
    private Grid ListItemTemplate(TbTodoList item)
    {
        var stateClass = item.Completed ? "Success" : "Secondary";
        var stateText = item.Completed ? item.CompleteTime.ToString("yyyy-MM-dd HH:mm:ss") : "-";

        return PzGrid(cols: "200, 200, 1*, 200")
            .Classes("ListRow")
            .Children(
                PzText(item.AddTime.ToString("yyyy-MM-dd HH:mm:ss")).Col(0),
                PzText(item.Name).Col(1),
                HStackPanel().Children(
                    PzText(stateText).Classes(stateClass)
                ).Col(2),
                HStackPanel(Aligns.HCenter).Col(3).Spacing(10).Children(
                        IconButton(MIcon.Check, classes: "Success")
                            .IsVisible(!item.Completed)
                            .OnClick(_ => OnComplete(item)),
                        IconButton(MIcon.Edit)
                            .OnClick(_ => OnEdit(item)),
                        IconButton(MIcon.Delete, classes: "Danger")
                            .OnClick(_ => OnDelete(item))
                    )
            );
    }
    protected override Control Build() =>
        PzGrid(rows: "40, *")
            .Margin(8)
            .RowSpacing(8)
            .Children(
                BuildOperatorBar().Row(0),
                BuildItemsList()
                    .Row(1)
                    .Align(Aligns.VStretch)
            );

    private List<TbTodoList> Items { get; set; } = [];
    protected override IEnumerable<IDisposable> WhenActivate()
    {
        UpdateItems();
        return base.WhenActivate();
    }
    private void UpdateItems()
    {
        Items = manager.GetAllLists();
        UpdateState();
    }

    private async void OnAdd()
    {
        var res = await PzDialogManager.ShowDialog(new TodoListDialog());
        if (PzDialogManager.IsSureResult(res.Result))
        {
            manager.Insert(res.Value);
            UpdateItems();
        }
    }
    private async void OnEdit(TbTodoList item)
    {
        var res = await PzDialogManager.ShowDialog(new TodoListDialog(item));
        if (PzDialogManager.IsSureResult(res.Result))
        {
            manager.Update(res.Value);
            UpdateItems();
        }
    }
    private void OnComplete(TbTodoList item)
    {
        item.Completed = true;
        item.CompleteTime = DateTime.Now;
        manager.Update(item);
        UpdateItems();
    }
    private async void OnDelete(TbTodoList item)
    {
        var dialog = PzDialogManager.DeleteConfirmDialog();
        var delete = await PzDialogManager.ShowDialog(dialog);
        if (PzDialogManager.IsSureResult(delete.Result))
        {
            manager.Delete(item.Id);
            UpdateItems();
        }
    }
}
