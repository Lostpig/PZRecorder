using PZRecorder.Desktop.Common;
using PZRecorder.Desktop.Extensions;
using System.Diagnostics;

namespace PZRecorder.Desktop.Modules.Dev;

internal class DevGridPage : PzPageBase
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
                PzButton("Text").OnClick(_ => FireTest())
            );
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
}
