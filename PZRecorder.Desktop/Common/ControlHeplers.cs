using Avalonia;
using Avalonia.Layout;
using Material.Icons;
using Material.Icons.Avalonia;
using PZRecorder.Desktop.Extensions;
using System.Diagnostics.CodeAnalysis;
using System.Reactive.Subjects;

namespace PZRecorder.Desktop.Common;

internal record TextSegment
{
    [MemberNotNullWhen(true, nameof(Getter))]
    public bool IsFuncSegment { get; init; }
    public Func<string>? Getter { get; init; }
    public string Text { get; init; }
    public string[] Classes { get; init; }

    public TextSegment(Func<string> getter, params string[] classes)
    {
        Getter = getter;
        Classes = classes;
        Text = "";
        IsFuncSegment = true;
    }
    public TextSegment(string text, params string[] classes)
    {
        Getter = null;
        Classes = classes;
        Text = text;
        IsFuncSegment = false;
    }
}

internal static class ControlHeplers
{
    public static StackPanel VStackPanel(params Aligns[] aligns)
    {
        return new StackPanel()
            .Orientation(Orientation.Vertical)
            .Align(aligns);
    }
    public static StackPanel HStackPanel(params Aligns[] aligns)
    {
        return new StackPanel()
            .Orientation(Orientation.Horizontal)
            .Align(aligns);
    }

    public static Grid PzGrid(string? cols = null, string? rows = null)
    {
        var grid = new Grid();

        if (!string.IsNullOrEmpty(cols)) grid.ColumnDefinitions = new(cols);
        if (!string.IsNullOrEmpty(rows)) grid.RowDefinitions = new(rows);

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

    public static Button PzButton(string text, params string[] classes)
    {
        var btn = new Button()
        {
            Content = text,
        };

        foreach (var c in classes) btn.Classes.Add(c);

        return btn;
    }
    public static Button PzButton(Func<string> textGetter, params string[] classes)
    {
        var btn = new Button().Content(textGetter);
        foreach (var c in classes) btn.Classes.Add(c);

        return btn;
    }
    public static Button IconButton(MIcon icon, int size = 24, params string[] classes)
    {
        var btn = new Button()
        {
            Padding = new Thickness(0),
            CornerRadius = new CornerRadius(100),
            Width = 40,
            Height = 40,
            Content = MaterialIcon(icon, size),
        };
        foreach (var c in classes) btn.Classes.Add(c);

        return btn;
    }
    public static Button IconButton(MIcon icon, string text, int size = 24, params string[] classes)
    {
        var tx = PzText(text).Align(Aligns.VCenter);
        var ic = MaterialIcon(icon, size);

        var btn = new Button()
        {
            Content = HStackPanel().Spacing(5).Children(ic, tx),
        };
        foreach (var c in classes) btn.Classes.Add(c);

        return btn;
    }
    public static Button IconButton(MIcon icon, Func<string> textGetter, int size = 24, params string[] classes)
    {
        var tx = PzText(textGetter).Align(Aligns.VCenter);
        var ic = MaterialIcon(icon, size);

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

    public static TextBox PzTextBox(ISubject<string?> subject)
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

    public static Uc.NumericIntUpDown PzNumericInt(ISubject<int> subject)
    {
        return new Uc.NumericIntUpDown().ValueEx(subject);
    }
    public static Uc.NumericIntUpDown PzNumericInt(Func<int?> getter)
    {
        return new Uc.NumericIntUpDown()._set(avap: Uc.NumericIntUpDown.ValueProperty, getter: getter);
    }

    public static List<TextBlock> FormatTextBlock(string format, params TextSegment[] args)
    {
        var list = new List<TextBlock>();
        int pos = 0;

        while (true)
        {
            if ((uint)pos >= (uint)format.Length)
            {
                return list;
            }

            ReadOnlySpan<char> remainder = format.AsSpan(pos);
            int leftPos = remainder.IndexOf('{');
            if (leftPos < 0)
            {
                list.Add(PzText(remainder.ToString()));
                return list;
            }

            int rightPos = remainder.IndexOf('}');
            if (rightPos < 0 || rightPos < leftPos)
            {
                throw new Exception($"format string error: {format}");
            }

            var argCountStr = remainder.Slice(leftPos + 1, rightPos - leftPos - 1);
            if (!int.TryParse(argCountStr, out int argCount))
            {
                throw new Exception($"format string error: {format}");
            }
            if (argCount >= args.Length || argCount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(args));
            }

            list.Add(PzText(remainder[..leftPos].ToString()));
            var segment = args[argCount];
            if (segment.IsFuncSegment) list.Add(PzText(segment.Getter, segment.Classes));
            else list.Add(PzText(segment.Text, segment.Classes));

            pos += rightPos + 1;
        }
    }
}
