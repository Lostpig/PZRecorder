using Microsoft.Extensions.DependencyInjection;
using PZRecorder.Desktop.Common;
using PZRecorder.Desktop.Extensions;
using PZRecorder.Desktop.Modules.Shared;
using System.Diagnostics;

namespace PZRecorder.Desktop.Modules.Dev;

internal class DevGridPage : MvuPage
{
    private Grid BuildGridRow(string text)
    {
        var grid = PzGrid("auto, 10, 1*");

        // grid.ColumnDefinitions = new();
        // grid.ColumnDefinitions.AddRange(
        //         GridLength.ParseLengths("auto, 10, 1*").Select(x => new ColumnDefinition(x))
        //     );
        // 
        // grid.ColumnDefinitions = [.. GridLength.ParseLengths("auto, 10, 1*").Select(x => new ColumnDefinition(x))];

        grid.ColSharedGroup(0, "A");

        grid.Children(
            PzText(text).Col(0),
            new TextBox().Col(2)
        );

        return grid;
    }
    protected override Control Build()
    {
        return VStackPanel(Aligns.Left)
            .Width(300)
            .Grid_IsSharedSizeScope(true)
            .Children(
                BuildGridRow("AAA"),
                BuildGridRow("XXX_BBB_CCC"),
                BuildGridRow("FFFFF"),
                PzButton("Text").OnClick(_ => FireTest()),
                HStackPanel().Spacing(8).Children(
                        PzButton("Goto record").OnClick(_ => GotoPage("Record")),
                        PzButton("Goto daily").OnClick(_ => GotoPage("Daily")),
                        PzButton("Goto setting").OnClick(_ => GotoPage("Setting"))
                    )
            );
    }

    private readonly PageRouter _router;
    public DevGridPage() : base()
    {
        _router = ServiceProvider.GetRequiredService<PageRouter>();
    }

    private void FireTest()
    {
        if (Child is StackPanel sp)
        {
            foreach(var c in sp.Children)
            {
                if (c is Grid cgg)
                {
                    foreach (var col in cgg.ColumnDefinitions)
                    {
                        Debug.WriteLine(col.SharedSizeGroup ?? "None");
                    }
                }
            }
        }
    }

    private void GotoPage(string key)
    {
        var p = Routes.Pages.FirstOrDefault(p => p.Key == key);
        if (p != null)
        {
            _router.RouteTo(p);
        }
    }
}
