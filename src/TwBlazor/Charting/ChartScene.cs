// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using TwBlazor.Enums;

namespace TwBlazor.Charting;

/// <summary>
/// Everything a chart draws, described as data: marks, interactive data points, axes, a legend and a table.
/// </summary>
/// <remarks>
/// A chart type only produces a scene. <see cref="TwBlazor.Components.TwChartBase"/> turns any scene into markup,
/// tooltips, keyboard navigation and the table view, so those concerns live in one place.
/// </remarks>
public sealed class ChartScene
{
    /// <summary>Gets a scene with nothing in it.</summary>
    public static ChartScene Empty { get; } = new();

    /// <summary>Gets the proportions of the plot area.</summary>
    public ChartShape Shape { get; init; }

    /// <summary>Gets the marks, in drawing order.</summary>
    public IReadOnlyList<IChartMark> Marks { get; init; } = [];

    /// <summary>Gets the interactive data points, in keyboard navigation order.</summary>
    public IReadOnlyList<ChartDatum> Datums { get; init; } = [];

    /// <summary>Gets the legend entries.</summary>
    public IReadOnlyList<ChartLegendItem> Legend { get; init; } = [];

    /// <summary>Gets the labels along the bottom of the plot area.</summary>
    public ChartAxis? XAxis { get; init; }

    /// <summary>Gets the labels along the left of the plot area.</summary>
    public ChartAxis? YAxis { get; init; }

    /// <summary>Gets the chart's data as a table.</summary>
    public ChartTable? Table { get; init; }

    /// <summary>Gets whether there is nothing to show, so the chart renders its empty state.</summary>
    public bool IsEmpty => Marks.Count == 0 && Legend.Count == 0;
}

/// <summary>
/// Collects the marks, data points, axes, legend and table of a <see cref="ChartScene"/>.
/// </summary>
/// <remarks>
/// The first group of methods adds raw marks at plot percentages. The second group works in "band and value"
/// terms and respects <see cref="Horizontal"/>, so one chart definition can draw both as columns and as bars.
/// </remarks>
public sealed class ChartSceneBuilder
{
    private const int maxBandLabels = 12;

    private readonly List<IChartMark> _marks = [];
    private readonly List<ChartDatum> _datums = [];
    private readonly List<ChartLegendItem> _legend = [];

    /// <summary>Gets or sets the proportions of the plot area.</summary>
    public ChartShape Shape { get; set; }

    /// <summary>
    /// Gets or sets whether values run left to right with categories stacked top to bottom (bars), instead of
    /// values running bottom to top with categories left to right (columns).
    /// </summary>
    public bool Horizontal { get; set; }

    /// <summary>Gets or sets the labels along the bottom of the plot area.</summary>
    public ChartAxis? XAxis { get; set; }

    /// <summary>Gets or sets the labels along the left of the plot area.</summary>
    public ChartAxis? YAxis { get; set; }

    /// <summary>Gets or sets the chart's data as a table.</summary>
    public ChartTable? Table { get; set; }

    /// <summary>Adds any mark.</summary>
    public void Add(IChartMark mark) => _marks.Add(mark);

    /// <summary>Adds a filled rectangle. For the less common options, pass a <see cref="ChartRect"/> to <see cref="Add"/>.</summary>
    public void Rect(double x, double y, double width, double height, ChartColor color, ChartCorner corner = ChartCorner.None, double opacity = 1) =>
        _marks.Add(new ChartRect(x, y, width, height, color, corner, opacity));

    /// <summary>Adds a dot. <paramref name="diameter"/> is in pixels.</summary>
    public void Dot(double x, double y, ChartColor color, double diameter = 10, double opacity = 1, string? title = null) =>
        _marks.Add(new ChartDot(x, y, color, diameter, opacity, title));

    /// <summary>Adds a straight line.</summary>
    public void Line(double x1, double y1, double x2, double y2, ChartColor color, ChartStroke stroke = ChartStroke.Hairline) =>
        _marks.Add(new ChartLine(x1, y1, x2, y2, color, stroke));

    /// <summary>Adds an SVG path.</summary>
    public void Path(string data, ChartColor color, bool filled = false, double opacity = 1, ChartStroke stroke = ChartStroke.Normal, bool outline = false, string? title = null)
    {
        if (!string.IsNullOrEmpty(data))
        {
            _marks.Add(new ChartPath(data, color, filled, opacity, stroke, outline, title));
        }
    }

    /// <summary>Adds a text label. For pixel offsets and emphasis, pass a <see cref="ChartText"/> to <see cref="Add"/>.</summary>
    public void Text(double x, double y, string text, ChartAnchor anchor = ChartAnchor.Middle, ChartBaseline baseline = ChartBaseline.Middle, ChartTextTone tone = ChartTextTone.Muted) =>
        _marks.Add(new ChartText(x, y, text, anchor, baseline, tone));

    /// <summary>Adds an icon. <paramref name="size"/> is in pixels.</summary>
    public void Glyph(double x, double y, Icon icon, ChartColor color, double size = 20) =>
        _marks.Add(new ChartGlyph(x, y, icon, color, size));

    /// <summary>Adds an interactive data point, positioned by its center.</summary>
    public void Datum(double x, double y, double width, double height, string title, IReadOnlyList<ChartDatumRow> rows, ChartHover hover = ChartHover.Highlight) =>
        _datums.Add(new ChartDatum(x, y, width, height, title, rows, hover));

    /// <summary>Adds a legend entry.</summary>
    public void Legend(string label, ChartColor color, int? seriesIndex = null, bool hidden = false) =>
        _legend.Add(new ChartLegendItem(label, color, seriesIndex, hidden));

    /// <summary>
    /// Converts a position along the category axis and a position along the value axis into a plot point.
    /// </summary>
    public (double X, double Y) Point(double band, double value) => Horizontal ? (value, band) : (band, 100 - value);

    /// <summary>
    /// Adds a bar across the category axis at <paramref name="band"/>, spanning <paramref name="from"/> to
    /// <paramref name="to"/> on the value axis. Unless <paramref name="corner"/> is given, only the data end is rounded.
    /// Set <paramref name="gap"/> to <see langword="false"/> for a bar layered over another, which needs no separating gap.
    /// </summary>
    public void Bar(double band, double thickness, double from, double to, ChartColor color, ChartCorner? corner = null, bool gap = true)
    {
        var low = Math.Min(from, to);
        var size = Math.Abs(to - from);
        var rising = to >= from;

        if (Horizontal)
        {
            Add(new ChartRect(low, band - thickness / 2, size, thickness, color, corner ?? (rising ? ChartCorner.Right : ChartCorner.Left), Gap: gap));
        }
        else
        {
            Add(new ChartRect(band - thickness / 2, 100 - low - size, thickness, size, color, corner ?? (rising ? ChartCorner.Top : ChartCorner.Bottom), Gap: gap));
        }
    }

    /// <summary>Adds a line between two band and value positions.</summary>
    public void Segment(double band1, double value1, double band2, double value2, ChartColor color, ChartStroke stroke = ChartStroke.Normal)
    {
        var start = Point(band1, value1);
        var end = Point(band2, value2);
        Line(start.X, start.Y, end.X, end.Y, color, stroke);
    }

    /// <summary>Adds a dot at a band and value position.</summary>
    public void Marker(double band, double value, ChartColor color, double diameter = 10, double opacity = 1, string? title = null)
    {
        var point = Point(band, value);
        Dot(point.X, point.Y, color, diameter, opacity, title);
    }

    /// <summary>Adds an interactive data point covering one whole category slot.</summary>
    public void BandDatum(double band, double size, string title, IReadOnlyList<ChartDatumRow> rows, ChartHover hover = ChartHover.Highlight)
    {
        if (Horizontal)
        {
            Datum(50, band, 100, size, title, rows, hover);
        }
        else
        {
            Datum(band, 50, size, 100, title, rows, hover);
        }
    }

    /// <summary>Adds value axis labels and gridlines from a scale.</summary>
    public void ValueAxis(ChartScale scale, Func<double, string> format, string? title = null) =>
        ValueAxis([.. scale.Ticks.Select(tick => new ChartTick(scale.Map(tick), format(tick)))], title);

    /// <summary>Adds value axis labels and gridlines at the given positions along the value axis.</summary>
    public void ValueAxis(IReadOnlyList<ChartTick> ticks, string? title = null)
    {
        foreach (var tick in ticks)
        {
            Segment(0, tick.Position, 100, tick.Position, ChartColor.Grid, ChartStroke.Hairline);
        }

        if (Horizontal)
        {
            XAxis = new ChartAxis(ticks, title);
        }
        else
        {
            YAxis = new ChartAxis([.. ticks.Select(tick => tick with { Position = 100 - tick.Position })], title);
        }
    }

    /// <summary>
    /// Adds category axis labels, one per slot. Along the bottom, labels are thinned out once there are too many to read.
    /// </summary>
    public void BandAxis(IReadOnlyList<string> labels, ChartBand band, string? title = null)
    {
        var every = Horizontal ? 1 : Math.Max(1, (int)Math.Ceiling(labels.Count / (double)maxBandLabels));
        ChartTick[] ticks = [.. Enumerable.Range(0, labels.Count).Where(index => index % every == 0).Select(index => new ChartTick(band.Center(index), labels[index]))];

        if (Horizontal)
        {
            YAxis = new ChartAxis(ticks, title);
        }
        else
        {
            XAxis = new ChartAxis(ticks, title);
        }
    }

    /// <summary>Adds the baseline that bars grow from.</summary>
    public void Baseline(double value) => Segment(0, value, 100, value, ChartColor.Axis, ChartStroke.Hairline);

    /// <summary>Adds a value label just past the data end of a bar.</summary>
    public void TipLabel(double band, double value, string text, bool rising = true)
    {
        var point = Point(band, value);
        if (Horizontal)
        {
            Add(new ChartText(point.X, point.Y, text, rising ? ChartAnchor.Start : ChartAnchor.End, ChartBaseline.Middle, ChartTextTone.Primary, rising ? 6 : -6));
        }
        else
        {
            Add(new ChartText(point.X, point.Y, text, ChartAnchor.Middle, rising ? ChartBaseline.Bottom : ChartBaseline.Top, ChartTextTone.Primary, 0, rising ? -4 : 4));
        }
    }

    /// <summary>Creates the scene.</summary>
    public ChartScene Build() => new()
    {
        Shape = Shape,
        Marks = [.. _marks],
        Datums = [.. _datums],
        Legend = [.. _legend],
        XAxis = XAxis,
        YAxis = YAxis,
        Table = Table
    };
}
