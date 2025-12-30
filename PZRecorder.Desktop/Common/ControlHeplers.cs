using Avalonia.Layout;
using Material.Icons;
using Material.Icons.Avalonia;
using System.Reactive.Subjects;
using System.Linq;

namespace PZRecorder.Desktop.Common;

internal static class ControlHeplers
{
    public static StackPanel VStackPanel(HorizontalAlignment alignment = HorizontalAlignment.Left)
    {
        return new StackPanel
        {
            Orientation = Orientation.Vertical,
            HorizontalAlignment = alignment,
        };
    }
    public static StackPanel HStackPanel(VerticalAlignment alignment = VerticalAlignment.Center)
    {
        return new StackPanel
        {
            Orientation = Orientation.Horizontal,
            VerticalAlignment = alignment
        };
    }

    public static Grid PzGrid(string? cols = null, string? rows = null)
    {
        var grid = new Grid();
        if (!string.IsNullOrEmpty(cols))
        {
            // have a bug, maybe about lazy evaluation
            // grid.ColumnDefinitions = [.. GridLength.ParseLengths(cols).Select(x => new ColumnDefinition(x))];
            grid.ColumnDefinitions = new();
            grid.ColumnDefinitions.AddRange(
                    GridLength.ParseLengths(cols).Select(x => new ColumnDefinition(x))
                );
        }
            
        if (!string.IsNullOrEmpty(rows))
        {
            // grid.RowDefinitions = [.. GridLength.ParseLengths(rows).Select(x => new RowDefinition(x))];
            grid.RowDefinitions = new();
            grid.RowDefinitions.AddRange(
                    GridLength.ParseLengths(rows).Select(x => new RowDefinition(x))
                );
        }

        return grid;
    }
    public static Grid ColSharedGroup(this Grid grid, int colIdx, string groupName)
    {
        if (grid.ColumnDefinitions.Count <= colIdx) return grid;

        grid.ColumnDefinitions[colIdx].SharedSizeGroup = groupName;
        return grid;
    }
    public static Grid RowSharedGroup(this Grid grid, int rowIdx, string groupName)
    {
        if (grid.RowDefinitions.Count <= rowIdx) return grid;

        grid.RowDefinitions[rowIdx].SharedSizeGroup = groupName;
        return grid;
    }

    public static MaterialIcon MaterialIcon(MaterialIconKind kind, int size = 24)
    {
        return new MaterialIcon
        {
            Kind = kind,
            Width = size,
            Height = size,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
        };
    }
    public static MaterialIcon MaterialIcon(Func<MaterialIconKind> func, int size = 24)
    {
        var icon = new MaterialIcon
        {
            Width = size,
            Height = size,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
        };
        static void setter(MaterialIcon i, MaterialIconKind v) => i.Kind = v;

        icon._set(setter, func);

        return icon;
    }

    public static Button SukiButton(string text, params string[] classes)
    {
        var btn = new Button()
        {
            Content = text,
        };

        foreach (var c in classes) btn.Classes.Add(c);

        return btn;
    }
    public static Button SukiButton(Func<string> textGetter, params string[] classes)
    {
        var btn = new Button().Content(textGetter);
        foreach (var c in classes) btn.Classes.Add(c);

        return btn;
    }
    public static Button IconButton(MaterialIconKind icon, params string[] classes)
    {
        var btn = new Button()
        {
            Padding = new Avalonia.Thickness(0),
            CornerRadius = new Avalonia.CornerRadius(100),
            Width = 40,
            Height = 40,
            Content = new MaterialIcon()
            {
                Kind = icon,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            },
        };
        foreach (var c in classes) btn.Classes.Add(c);

        return btn;
    }
    public static Button IconButton(MaterialIconKind icon, string text, params string[] classes)
    {
        var tx = new TextBlock().Text(text);
        var ic = new MaterialIcon()
        {
            Kind = icon,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
        };

        var btn = new Button()
        {
            Content = HStackPanel().Spacing(5).Children(ic, tx),
        };
        foreach (var c in classes) btn.Classes.Add(c);

        return btn;
    }
    public static Button IconButton(MaterialIconKind icon, Func<string> textGetter, params string[] classes)
    {
        var tx = new TextBlock().Text(textGetter);
        var ic = new MaterialIcon()
        {
            Kind = icon,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
        };

        var btn = new Button()
        {
            Content = HStackPanel().Spacing(5).Children(ic, tx),
        };
        foreach (var c in classes) btn.Classes.Add(c);

        return btn;
    }

    public static TextBlock PzText(Func<string> func, params string[] classes)
    {
        var ctrl = new TextBlock().Text(func);
        foreach (var c in classes) ctrl.Classes.Add(c);
        return ctrl;
    }
    public static TextBlock PzText(string text, params string[] classes)
    {
        var ctrl = new TextBlock() { Text = text };
        foreach (var c in classes) ctrl.Classes.Add(c);
        return ctrl;
    }
    public static TextBlock PzText(IObservable<string> obs, params string[] classes)
    {
        var ctrl = new TextBlock().Text(obs);
        foreach (var c in classes) ctrl.Classes.Add(c);
        return ctrl;
    }

    public static TextBox PzTextBox(ISubject<string> subject)
    {
        return new TextBox().Text(subject);
    }
    public static TextBox PzTextBox(Func<string> getter)
    {
        return new TextBox().Text(getter);
    }
    public static TextBox PzTextBox(IObservable<string> obs)
    {
        return new TextBox().Text(obs);
    }

}
