// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using TwBlazor.Enums;

namespace TwBlazor.Charting;

/// <summary>
/// The job a color does in a chart. The theme maps each role to its classes, so a chart never names a color itself.
/// </summary>
public enum ChartColorRole
{
    /// <summary>Identity: one of the categorical series colors, picked by slot.</summary>
    Series,

    /// <summary>A favourable change, such as an increase on a waterfall chart.</summary>
    Positive,

    /// <summary>An unfavourable change, such as a decrease on a waterfall chart.</summary>
    Negative,

    /// <summary>A neutral summary value, such as the total on a waterfall chart.</summary>
    Total,

    /// <summary>Magnitude: a single hue whose opacity follows the value.</summary>
    Sequential,

    /// <summary>The unfilled part of a meter, gauge or bullet range.</summary>
    Track,

    /// <summary>Recessive gridlines.</summary>
    Grid,

    /// <summary>Baselines and axis lines.</summary>
    Axis,

    /// <summary>High-contrast ink for target markers.</summary>
    Ink
}

/// <summary>
/// A themed color reference: a <see cref="ChartColorRole"/> and, for <see cref="ChartColorRole.Series"/>, the slot.
/// </summary>
/// <param name="Role">The job the color does.</param>
/// <param name="Slot">The zero-based series slot. Ignored by every role except <see cref="ChartColorRole.Series"/>.</param>
public readonly record struct ChartColor(ChartColorRole Role, int Slot = 0)
{
    /// <summary>Gets the categorical color for a series. A series keeps its slot when others are hidden.</summary>
    /// <param name="slot">The zero-based series slot.</param>
    public static ChartColor Series(int slot) => new(ChartColorRole.Series, Math.Max(0, slot));

    /// <summary>Gets the color for a favourable change.</summary>
    public static ChartColor Positive => new(ChartColorRole.Positive);

    /// <summary>Gets the color for an unfavourable change.</summary>
    public static ChartColor Negative => new(ChartColorRole.Negative);

    /// <summary>Gets the color for a neutral summary value.</summary>
    public static ChartColor Total => new(ChartColorRole.Total);

    /// <summary>Gets the single hue used to show magnitude through opacity.</summary>
    public static ChartColor Sequential => new(ChartColorRole.Sequential);

    /// <summary>Gets the color for an unfilled track.</summary>
    public static ChartColor Track => new(ChartColorRole.Track);

    /// <summary>Gets the color for gridlines.</summary>
    public static ChartColor Grid => new(ChartColorRole.Grid);

    /// <summary>Gets the color for baselines and axis lines.</summary>
    public static ChartColor Axis => new(ChartColorRole.Axis);

    /// <summary>Gets the high-contrast ink color.</summary>
    public static ChartColor Ink => new(ChartColorRole.Ink);
}

/// <summary>Which corners of a rectangle are rounded.</summary>
public enum ChartCorner { None, Top, Bottom, Left, Right, All }

/// <summary>Where a text mark sits horizontally relative to its position.</summary>
public enum ChartAnchor { Start, Middle, End }

/// <summary>Where a text mark sits vertically relative to its position.</summary>
public enum ChartBaseline { Top, Middle, Bottom }

/// <summary>The ink a text mark uses. Text never wears a series color.</summary>
public enum ChartTextTone { Muted, Primary, Inverse }

/// <summary>The weight of a stroked line or path.</summary>
public enum ChartStroke { Hairline, Normal, Thick }

/// <summary>How a datum responds to hover and keyboard focus.</summary>
public enum ChartHover { Highlight, Crosshair, None }

/// <summary>The proportions of the plot area.</summary>
public enum ChartShape
{
    /// <summary>Fills the available width at the chart's height. Used by axis-based charts.</summary>
    Wide,

    /// <summary>A centered square. Used by radial charts so circles stay round.</summary>
    Square
}

/// <summary>
/// A visual primitive in a <see cref="ChartScene"/>. All positions and sizes are percentages (0 to 100) of the plot
/// area, measured from its top-left corner, so a scene scales to any width without script.
/// </summary>
public interface IChartMark;

/// <summary>A filled rectangle.</summary>
public sealed record ChartRect(double X, double Y, double Width, double Height, ChartColor Color, ChartCorner Corner = ChartCorner.None, double Opacity = 1, bool Gap = true, string? Title = null) : IChartMark;

/// <summary>A filled circle centered on a point. <paramref name="Diameter"/> is in pixels so dots stay round and legible at any width.</summary>
public sealed record ChartDot(double X, double Y, ChartColor Color, double Diameter = 10, double Opacity = 1, string? Title = null) : IChartMark;

/// <summary>A straight stroked line.</summary>
public sealed record ChartLine(double X1, double Y1, double X2, double Y2, ChartColor Color, ChartStroke Stroke = ChartStroke.Hairline) : IChartMark;

/// <summary>An SVG path, either stroked or filled. <paramref name="Outline"/> adds a surface-colored gap around a filled shape.</summary>
public sealed record ChartPath(string Data, ChartColor Color, bool Filled = false, double Opacity = 1, ChartStroke Stroke = ChartStroke.Normal, bool Outline = false, string? Title = null) : IChartMark;

/// <summary>A text label. Offsets are in pixels.</summary>
public sealed record ChartText(double X, double Y, string Text, ChartAnchor Anchor = ChartAnchor.Middle, ChartBaseline Baseline = ChartBaseline.Middle, ChartTextTone Tone = ChartTextTone.Muted, double OffsetX = 0, double OffsetY = 0, bool Strong = false, bool Large = false) : IChartMark;

/// <summary>An icon centered on a point. <paramref name="Size"/> is in pixels.</summary>
public sealed record ChartGlyph(double X, double Y, Icon Icon, ChartColor Color, double Size = 20) : IChartMark;

/// <summary>One line of a tooltip: a label, its formatted value and an optional color key.</summary>
public sealed record ChartDatumRow(string Label, string Value, ChartColor? Color = null);

/// <summary>
/// An interactive data point: the hover and keyboard-focus target that carries a tooltip. Positioned by its center.
/// </summary>
public sealed record ChartDatum(double X, double Y, double Width, double Height, string Title, IReadOnlyList<ChartDatumRow> Rows, ChartHover Hover = ChartHover.Highlight)
{
    /// <summary>Gets the datum described as one sentence for assistive technology.</summary>
    public string AriaLabel => Rows.Count == 0
        ? Title
        : $"{Title}: {string.Join(", ", Rows.Select(row => $"{row.Label} {row.Value}"))}";
}

/// <summary>A label at a position along an axis.</summary>
public sealed record ChartTick(double Position, string Label);

/// <summary>The labels along one edge of the plot area.</summary>
public sealed record ChartAxis(IReadOnlyList<ChartTick> Ticks, string? Title = null);

/// <summary>A legend entry. Entries with a <paramref name="SeriesIndex"/> can be toggled to hide that series.</summary>
public sealed record ChartLegendItem(string Label, ChartColor Color, int? SeriesIndex = null, bool Hidden = false);

/// <summary>The chart's data as a table: the accessible alternative to the plot.</summary>
public sealed record ChartTable(IReadOnlyList<string> Headers, IReadOnlyList<IReadOnlyList<string>> Rows);
