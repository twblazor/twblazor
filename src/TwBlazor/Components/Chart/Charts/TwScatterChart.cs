// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using Microsoft.AspNetCore.Components;
using TwBlazor.Charting;
using TwBlazor.Models;

namespace TwBlazor.Components;

/// <summary>
/// A scatter plot: points positioned by two numeric values to show how they relate. Give points a
/// <see cref="ChartPoint.Size"/> for a bubble chart, or set <see cref="Connect"/> for a connected scatter plot.
/// </summary>
/// <remarks>
/// Any two points can end up side by side, so keep to three series or fewer for colors that stay easy to tell apart.
/// </remarks>
public class TwScatterChart : TwChartBase
{
    /// <summary>
    /// Gets or sets the series of points to plot.
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
    /// Gets or sets the label for a point's size in tooltips and the table view.
    /// </summary>
    [Parameter] public string SizeTitle { get; set; } = "Size";

    /// <summary>
    /// Gets or sets whether the points of each series are joined in order (a connected scatter plot).
    /// </summary>
    [Parameter] public bool Connect { get; set; }

    /// <summary>
    /// Gets or sets whether each point that has a <see cref="ChartPoint.Label"/> shows it beside the dot.
    /// </summary>
    [Parameter] public bool ShowLabels { get; set; }

    /// <summary>
    /// Adds marks that sit behind the points, such as the dividers of a quadrant chart.
    /// </summary>
    /// <param name="builder">The builder that collects the scene.</param>
    /// <param name="xScale">The horizontal scale.</param>
    /// <param name="yScale">The vertical scale.</param>
    private protected virtual void BuildBackdrop(ChartSceneBuilder builder, ChartScale xScale, ChartScale yScale)
    {
    }

    /// <inheritdoc />
    protected internal override void BuildScene(ChartSceneBuilder builder)
    {
        if (!Series.Any(series => series.Points.Count > 0))
        {
            return;
        }

        var hasSize = Series.Any(series => series.Points.Any(point => point.Size.HasValue));
        var visible = Series.Select((series, index) => (Series: series, Slot: index)).Where(item => !IsSeriesHidden(item.Slot)).ToList();

        if (Series.Count > 1)
        {
            for (var i = 0; i < Series.Count; i++)
            {
                builder.Legend(Series[i].Name, ChartColor.Series(i), i, IsSeriesHidden(i));
            }
        }

        builder.Table = new ChartTable(
            ["Series", "Label", XTitle, YTitle, .. hasSize ? [SizeTitle] : Array.Empty<string>()],
            [.. Series.SelectMany(series => series.Points.Select(point => (IReadOnlyList<string>)
                [series.Name, point.Label ?? string.Empty, FormatValue(point.X), FormatValue(point.Y), .. hasSize ? [point.Size.HasValue ? FormatValue(point.Size.Value) : string.Empty] : Array.Empty<string>()]))]);

        var points = visible.SelectMany(item => item.Series.Points).ToList();
        if (points.Count == 0)
        {
            return;
        }

        var xScale = ChartScale.Nice(points.Select(point => point.X), includeZero: false);
        var yScale = ChartScale.Nice(points.Select(point => point.Y), includeZero: false);
        var maxSize = points.Max(point => point.Size ?? 0);
        builder.ValueAxis(yScale, FormatValue, YTitle);
        builder.XAxis = new ChartAxis([.. xScale.Ticks.Select(tick => new ChartTick(xScale.Map(tick), FormatValue(tick)))], XTitle);
        BuildBackdrop(builder, xScale, yScale);

        foreach (var (series, slot) in visible)
        {
            var color = ChartColor.Series(slot);
            var positions = series.Points.Select(point => (X: xScale.Map(point.X), Y: 100 - yScale.Map(point.Y))).ToList();

            if (Connect)
            {
                builder.Path(ChartGeometry.Line(positions), color);
            }

            for (var i = 0; i < series.Points.Count; i++)
            {
                var point = series.Points[i];
                var diameter = point.Size is { } size && maxSize > 0 ? 10 + 30 * Math.Sqrt(Math.Max(0, size) / maxSize) : 10;
                builder.Dot(positions[i].X, positions[i].Y, color, diameter, point.Size.HasValue ? 0.75 : 1);

                if (ShowLabels && point.Label != null)
                {
                    builder.Text(positions[i].X, positions[i].Y, point.Label, baseline: ChartBaseline.Bottom, tone: ChartTextTone.Primary, offsetY: -(diameter / 2 + 4));
                }

                List<ChartDatumRow> rows = [new(XTitle, FormatValue(point.X)), new(YTitle, FormatValue(point.Y))];
                if (point.Size.HasValue)
                {
                    rows.Add(new ChartDatumRow(SizeTitle, FormatValue(point.Size.Value)));
                }

                builder.Datum(positions[i].X, positions[i].Y, 0, 0, point.Label ?? series.Name, rows, ChartHover.None);
            }
        }
    }
}

/// <summary>
/// A quadrant chart: a scatter plot divided into four labelled regions by a horizontal and a vertical threshold,
/// for sorting items into groups such as "quick wins" and "major projects".
/// </summary>
public class TwQuadrantChart : TwScatterChart
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TwQuadrantChart"/> class, which labels its points by default.
    /// </summary>
    public TwQuadrantChart()
    {
        ShowLabels = true;
    }

    /// <summary>
    /// Gets or sets the X value of the vertical divider.
    /// </summary>
    /// <remarks>
    /// If not set, the divider sits in the middle of the horizontal axis.
    /// </remarks>
    [Parameter] public double? XThreshold { get; set; }

    /// <summary>
    /// Gets or sets the Y value of the horizontal divider.
    /// </summary>
    /// <remarks>
    /// If not set, the divider sits in the middle of the vertical axis.
    /// </remarks>
    [Parameter] public double? YThreshold { get; set; }

    /// <summary>
    /// Gets or sets the label of the top-left quadrant.
    /// </summary>
    [Parameter] public string? TopLeftLabel { get; set; }

    /// <summary>
    /// Gets or sets the label of the top-right quadrant.
    /// </summary>
    [Parameter] public string? TopRightLabel { get; set; }

    /// <summary>
    /// Gets or sets the label of the bottom-left quadrant.
    /// </summary>
    [Parameter] public string? BottomLeftLabel { get; set; }

    /// <summary>
    /// Gets or sets the label of the bottom-right quadrant.
    /// </summary>
    [Parameter] public string? BottomRightLabel { get; set; }

    private protected override void BuildBackdrop(ChartSceneBuilder builder, ChartScale xScale, ChartScale yScale)
    {
        var x = XThreshold.HasValue ? xScale.Map(XThreshold.Value) : 50;
        var y = YThreshold.HasValue ? 100 - yScale.Map(YThreshold.Value) : 50;
        builder.Line(x, 0, x, 100, ChartColor.Axis, ChartStroke.Normal);
        builder.Line(0, y, 100, y, ChartColor.Axis, ChartStroke.Normal);
        AddCorner(builder, TopLeftLabel, 0, 0, ChartAnchor.Start, ChartBaseline.Top);
        AddCorner(builder, TopRightLabel, 100, 0, ChartAnchor.End, ChartBaseline.Top);
        AddCorner(builder, BottomLeftLabel, 0, 100, ChartAnchor.Start, ChartBaseline.Bottom);
        AddCorner(builder, BottomRightLabel, 100, 100, ChartAnchor.End, ChartBaseline.Bottom);
    }

    private static void AddCorner(ChartSceneBuilder builder, string? label, double x, double y, ChartAnchor anchor, ChartBaseline baseline)
    {
        if (!string.IsNullOrWhiteSpace(label))
        {
            builder.Text(x, y, label, anchor, baseline, offsetX: anchor == ChartAnchor.Start ? 6 : -6, offsetY: baseline == ChartBaseline.Top ? 6 : -6, strong: true);
        }
    }
}
