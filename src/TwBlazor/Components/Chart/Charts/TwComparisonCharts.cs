// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using Microsoft.AspNetCore.Components;
using TwBlazor.Charting;
using TwBlazor.Localization;
using TwBlazor.Models;

namespace TwBlazor.Components;

/// <summary>
/// A categorical scatter plot: a dot where a horizontal category meets a vertical one, sized by its value, for
/// showing how a measure varies across two sets of categories when most combinations are empty.
/// </summary>
/// <remarks>
/// The area of each dot, not its diameter, follows the value. Categories appear in the order they are first used.
/// </remarks>
public class TwCategoricalScatterChart : TwChartBase
{
    /// <summary>
    /// Gets or sets the points to plot.
    /// </summary>
    [Parameter] public IReadOnlyList<ChartCategoricalPoint> Points { get; set; } = [];

    /// <summary>
    /// Gets or sets the title of the horizontal axis. It also labels the horizontal category in tooltips and the table view.
    /// </summary>
    [Parameter] public string XTitle { get; set; } = "X";

    /// <summary>
    /// Gets or sets the title of the vertical axis. It also labels the vertical category in tooltips and the table view.
    /// </summary>
    [Parameter] public string YTitle { get; set; } = "Y";

    /// <summary>
    /// Gets or sets the label for a point's value in tooltips and the table view.
    /// Defaults to the localized <see cref="TwStrings.Chart_Value"/> text ("Value").
    /// </summary>
    [Parameter] public string? ValueTitle { get; set; }

    private string effectiveValueTitle => ValueTitle ?? localizer[TwStrings.Chart_Value];

    /// <inheritdoc />
    protected internal override void BuildScene(ChartSceneBuilder builder)
    {
        var points = Points.Where(point => double.IsFinite(point.Value)).ToList();
        if (points.Count == 0)
        {
            return;
        }

        var xCategories = points.Select(point => point.X).Distinct().ToList();
        var yCategories = points.Select(point => point.Y).Distinct().ToList();
        var columns = new ChartBand(xCategories.Count);
        var rows = new ChartBand(yCategories.Count);
        var peak = points.Max(point => point.Value);
        var color = ChartColor.Series(0);
        var hasLabels = points.Any(point => point.Label != null);

        builder.XAxis = new ChartAxis([.. xCategories.Select((label, index) => new ChartTick(columns.Center(index), label))], XTitle);
        builder.YAxis = new ChartAxis([.. yCategories.Select((label, index) => new ChartTick(rows.Center(index), label))], YTitle);

        for (var column = 0; column < xCategories.Count; column++)
        {
            builder.Line(columns.Center(column), 0, columns.Center(column), 100, ChartColor.Grid);
        }

        for (var row = 0; row < yCategories.Count; row++)
        {
            builder.Line(0, rows.Center(row), 100, rows.Center(row), ChartColor.Grid);
        }

        List<IReadOnlyList<string>> table = [];
        foreach (var point in points)
        {
            var x = columns.Center(xCategories.IndexOf(point.X));
            var y = rows.Center(yCategories.IndexOf(point.Y));
            var value = FormatValue(point.Value);

            builder.Dot(x, y, color, peak > 0 ? 8 + 24 * Math.Sqrt(Math.Max(0, point.Value) / peak) : 8, 0.85);
            builder.Datum(x, y, 0, 0, point.Label ?? $"{point.X}, {point.Y}",
                [new ChartDatumRow(XTitle, point.X), new ChartDatumRow(YTitle, point.Y), new ChartDatumRow(effectiveValueTitle, value, color)], ChartHover.None);
            table.Add(hasLabels ? [point.Label ?? string.Empty, point.X, point.Y, value] : [point.X, point.Y, value]);
        }

        string[] headers = [XTitle, YTitle, effectiveValueTitle];
        builder.Table = new ChartTable(hasLabels ? [localizer[TwStrings.Chart_PointLabel], .. headers] : headers, table);
    }
}

/// <summary>
/// The shared definition of the charts that summarise a cloud of numeric points:
/// <see cref="TwHexbinChart"/> and <see cref="TwContourChart"/>.
/// </summary>
public abstract class TwPointSurfaceChartBase : TwChartBase
{
    /// <summary>
    /// Gets or sets the series of points to summarise. Every series is combined into one cloud.
    /// </summary>
    [Parameter] public IReadOnlyList<ChartPointSeries> Series { get; set; } = [];

    /// <summary>
    /// Gets or sets the title of the horizontal axis. It also labels the X value in tooltips and the table view.
    /// </summary>
    [Parameter] public string XTitle { get; set; } = "X";

    /// <summary>
    /// Gets or sets the title of the vertical axis. It also labels the Y value in tooltips and the table view.
    /// </summary>
    [Parameter] public string YTitle { get; set; } = "Y";

    /// <summary>
    /// Gets whether a point can be plotted.
    /// </summary>
    /// <param name="point">The point.</param>
    private protected virtual bool IsPlotted(ChartPoint point) => double.IsFinite(point.X) && double.IsFinite(point.Y);

    /// <summary>
    /// Adds the marks, data points and table for the points.
    /// </summary>
    /// <param name="builder">The builder that collects the scene.</param>
    /// <param name="points">The points that can be plotted, with the series each belongs to. Never empty.</param>
    /// <param name="xScale">The horizontal scale.</param>
    /// <param name="yScale">The vertical scale.</param>
    private protected abstract void BuildPoints(ChartSceneBuilder builder, IReadOnlyList<(ChartPoint Point, string Series)> points, ChartScale xScale, ChartScale yScale);

    /// <inheritdoc />
    protected internal sealed override void BuildScene(ChartSceneBuilder builder)
    {
        List<(ChartPoint Point, string Series)> points = [.. Series.SelectMany(series => series.Points.Where(IsPlotted).Select(point => (point, series.Name)))];
        if (points.Count == 0)
        {
            return;
        }

        var xScale = ChartScale.Nice(points.Select(item => item.Point.X), includeZero: false);
        var yScale = ChartScale.Nice(points.Select(item => item.Point.Y), includeZero: false);
        builder.XAxis = new ChartAxis([.. xScale.Ticks.Select(tick => new ChartTick(xScale.Map(tick), FormatValue(tick)))], XTitle);
        builder.YAxis = new ChartAxis([.. yScale.Ticks.Select(tick => new ChartTick(100 - yScale.Map(tick), FormatValue(tick)))], YTitle);
        BuildPoints(builder, points, xScale, yScale);
    }
}

/// <summary>
/// A hexbin chart: the plot is tiled with hexagons and each is shaded by how many points fall inside it, for
/// showing where a scatter plot too dense to read is most crowded.
/// </summary>
/// <remarks>
/// Shading uses a single hue, so darker always means more points. Hexagons with no points are left out.
/// </remarks>
public class TwHexbinChart : TwPointSurfaceChartBase
{
    /// <summary>
    /// Gets or sets the width of a hexagon as a percentage of the plot's width, from 4 to 25.
    /// </summary>
    /// <remarks>
    /// Default is 10. Smaller hexagons show finer detail but need more points to fill.
    /// </remarks>
    [Parameter] public double BinWidth { get; set; } = 10;

    /// <summary>
    /// Gets or sets the label for a hexagon's count in tooltips and the table view.
    /// Defaults to the localized <see cref="TwStrings.Chart_Count"/> text ("Count").
    /// </summary>
    [Parameter] public string? CountLabel { get; set; }

    private string effectiveCountLabel => CountLabel ?? localizer[TwStrings.Chart_Count];

    private protected override void BuildPoints(ChartSceneBuilder builder, IReadOnlyList<(ChartPoint Point, string Series)> points, ChartScale xScale, ChartScale yScale)
    {
        // Pointed-top hexagons, sized so they look regular on a plot ChartMetrics.WideAspect times wider than tall.
        // The grid is centered and holds only whole hexagons, so none reaches past the edge of the plot.
        var width = Math.Clamp(BinWidth, 4, 25);
        var radius = width * ChartMetrics.WideAspect / Math.Sqrt(3);
        var rowStep = radius * 1.5;
        var rowCount = (int)((100 - radius * 2) / rowStep) + 1;
        var columnCount = (int)(100 / width);
        var top = (100 - (rowCount - 1) * rowStep) / 2;
        var left = (100 - columnCount * width) / 2 + width / 2;

        (double X, double Y) Center(int column, int row) => (left + column * width + (row % 2 == 0 ? 0 : width / 2), top + row * rowStep);

        (int Column, int Row) Nearest(double x, double y)
        {
            var guess = (int)Math.Round((y - top) / rowStep);
            return Enumerable.Range(guess - 1, 3).Where(row => row >= 0 && row < rowCount)
                .Select(row => (Column: Math.Clamp((int)Math.Round((x - Center(0, row).X) / width), 0, columnCount - 1 - (row % 2)), Row: row))
                .MinBy(cell => Math.Pow((Center(cell.Column, cell.Row).X - x) * ChartMetrics.WideAspect, 2) + Math.Pow(Center(cell.Column, cell.Row).Y - y, 2));
        }

        var bins = points.GroupBy(item => Nearest(xScale.Map(item.Point.X), 100 - yScale.Map(item.Point.Y)))
            .OrderBy(bin => bin.Key.Row).ThenBy(bin => bin.Key.Column).ToList();
        var peak = bins.Max(bin => bin.Count());

        List<IReadOnlyList<string>> table = [];
        foreach (var bin in bins)
        {
            var (x, y) = Center(bin.Key.Column, bin.Key.Row);
            (double X, double Y)[] corners =
            [
                (x, y - radius), (x + width / 2, y - radius / 2), (x + width / 2, y + radius / 2),
                (x, y + radius), (x - width / 2, y + radius / 2), (x - width / 2, y - radius / 2)
            ];
            string[] cells = [FormatValue(bin.Average(item => item.Point.X)), FormatValue(bin.Average(item => item.Point.Y)), bin.Count().ToString("#,0")];

            builder.Path(ChartGeometry.Line(corners, close: true), ChartColor.Sequential, filled: true, opacity: 0.15 + 0.85 * bin.Count() / peak, outline: true);
            builder.ShapeDatum(corners, $"{cells[0]}, {cells[1]}", [new ChartDatumRow(effectiveCountLabel, cells[2], ChartColor.Sequential)]);
            table.Add(cells);
        }

        builder.Table = new ChartTable([XTitle, YTitle, effectiveCountLabel], table);
    }
}

/// <summary>
/// A contour plot: lines joining the places where a measured surface has the same value, like the height lines
/// on a map, drawn from samples taken at scattered positions.
/// </summary>
/// <remarks>
/// Give each point its measurement as <see cref="ChartPoint.Size"/>; points without one are left out. The surface
/// between the samples is estimated from the nearest of them, and a stronger line marks a higher value. At least
/// three samples are needed.
/// </remarks>
public class TwContourChart : TwPointSurfaceChartBase
{
    private const int divisions = 24;

    /// <summary>
    /// Gets or sets how many contour lines are drawn, from 2 to 10.
    /// </summary>
    /// <remarks>
    /// Default is 5. The lines are spaced evenly between the lowest and highest measurement.
    /// </remarks>
    [Parameter] public int Levels { get; set; } = 5;

    /// <summary>
    /// Gets or sets the label for a sample's measurement in tooltips and the table view.
    /// Defaults to the localized <see cref="TwStrings.Chart_Value"/> text ("Value").
    /// </summary>
    [Parameter] public string? ValueTitle { get; set; }

    private string effectiveValueTitle => ValueTitle ?? localizer[TwStrings.Chart_Value];

    private protected override bool IsPlotted(ChartPoint point) => base.IsPlotted(point) && point.Size is { } value && double.IsFinite(value);

    private protected override void BuildPoints(ChartSceneBuilder builder, IReadOnlyList<(ChartPoint Point, string Series)> points, ChartScale xScale, ChartScale yScale)
    {
        if (points.Count < 3)
        {
            return;
        }

        List<(double X, double Y, double Value)> samples = [.. points.Select(item => (xScale.Map(item.Point.X), 100 - yScale.Map(item.Point.Y), item.Point.Size!.Value))];
        var (min, max) = (samples.Min(sample => sample.Value), samples.Max(sample => sample.Value));
        const double step = 100.0 / divisions;

        // The surface at each grid corner is the average of the samples, weighted by the inverse square of their distance.
        var surface = new double[divisions + 1, divisions + 1];
        for (var row = 0; row <= divisions; row++)
        {
            for (var column = 0; column <= divisions; column++)
            {
                double weighted = 0, weights = 0;
                foreach (var (x, y, value) in samples)
                {
                    var weight = 1 / Math.Max(0.01, (x - column * step) * (x - column * step) + (y - row * step) * (y - row * step));
                    weighted += value * weight;
                    weights += weight;
                }

                surface[row, column] = weighted / weights;
            }
        }

        var levels = Math.Clamp(Levels, 2, 10);
        for (var level = 1; level <= levels && max > min; level++)
        {
            var threshold = min + (max - min) * level / (levels + 1);
            var path = new ChartPathBuilder();

            for (var row = 0; row < divisions; row++)
            {
                for (var column = 0; column < divisions; column++)
                {
                    // Corners clockwise from the top left. The line crosses every edge whose ends lie either side of it.
                    (double X, double Y, double Value)[] corners =
                    [
                        (column * step, row * step, surface[row, column]),
                        ((column + 1) * step, row * step, surface[row, column + 1]),
                        ((column + 1) * step, (row + 1) * step, surface[row + 1, column + 1]),
                        (column * step, (row + 1) * step, surface[row + 1, column])
                    ];

                    List<(double X, double Y)> crossings = [];
                    for (var edge = 0; edge < 4; edge++)
                    {
                        var (from, to) = (corners[edge], corners[(edge + 1) % 4]);
                        if ((from.Value < threshold) != (to.Value < threshold))
                        {
                            var fraction = (threshold - from.Value) / (to.Value - from.Value);
                            crossings.Add((from.X + (to.X - from.X) * fraction, from.Y + (to.Y - from.Y) * fraction));
                        }
                    }

                    for (var i = 1; i < crossings.Count; i += 2)
                    {
                        path.MoveTo(crossings[i - 1].X, crossings[i - 1].Y).LineTo(crossings[i].X, crossings[i].Y);
                    }
                }
            }

            builder.Path(path.ToString(), ChartColor.Sequential, opacity: 0.3 + 0.7 * level / levels, title: FormatValue(threshold));
        }

        List<IReadOnlyList<string>> table = [];
        for (var i = 0; i < points.Count; i++)
        {
            var (point, series) = points[i];
            string[] cells = [FormatValue(point.X), FormatValue(point.Y), FormatValue(point.Size!.Value)];

            builder.Dot(samples[i].X, samples[i].Y, ChartColor.Ink, 6);
            builder.Datum(samples[i].X, samples[i].Y, 0, 0, point.Label ?? series,
                [new ChartDatumRow(XTitle, cells[0]), new ChartDatumRow(YTitle, cells[1]), new ChartDatumRow(effectiveValueTitle, cells[2])], ChartHover.None);
            table.Add(cells);
        }

        builder.Table = new ChartTable([XTitle, YTitle, effectiveValueTitle], table);
    }
}