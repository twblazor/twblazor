// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using Microsoft.AspNetCore.Components;
using TwBlazor.Charting;
using TwBlazor.Localization;
using TwBlazor.Models;

namespace TwBlazor.Components;

/// <summary>
/// A density plot: a smooth curve for each group of raw observations showing where its values concentrate, for
/// comparing the shape of distributions on one axis.
/// </summary>
/// <remarks>
/// Each curve is a kernel density estimate, and all share one vertical scale so a taller curve always means a
/// tighter cluster. The curves have no value axis because density has no unit a reader needs.
/// </remarks>
public class TwDensityPlotChart : TwChartBase
{
    // How far past its outermost values a curve is drawn, in bandwidths: far enough for it to fall back to zero.
    private const double tail = 3;

    // The tallest curve stops short of the top of the plot.
    private const double peakHeight = 92;

    /// <summary>
    /// Gets or sets the groups of raw observations to plot.
    /// </summary>
    [Parameter] public IReadOnlyList<ChartSample> Samples { get; set; } = [];

    /// <summary>
    /// Gets or sets the heading of the group column in the table view.
    /// Defaults to the localized <see cref="TwStrings.Chart_Group"/> text ("Group").
    /// </summary>
    [Parameter] public string? LabelHeader { get; set; }

    private string effectiveLabelHeader => LabelHeader ?? localizer[TwStrings.Chart_Group];

    /// <inheritdoc />
    protected internal override void BuildScene(ChartSceneBuilder builder)
    {
        List<DensityGroup> groups = [.. Samples.Select(CreateGroup).Where(group => group.Sorted.Count > 0)];
        if (groups.Count == 0)
        {
            return;
        }

        string[] labels = [localizer[TwStrings.Chart_Count], localizer[TwStrings.Chart_Minimum], localizer[TwStrings.Chart_Median], localizer[TwStrings.Chart_Maximum]];
        builder.Table = new ChartTable([effectiveLabelHeader, .. labels], [.. groups.Select(group => (IReadOnlyList<string>)[group.Label, .. group.Summary])]);

        if (groups.Count > 1)
        {
            groups.ForEach(group => builder.Legend(group.Label, ChartColor.Series(group.Slot), group.Slot, IsSeriesHidden(group.Slot)));
        }

        List<DensityGroup> visible = [.. groups.Where(group => groups.Count == 1 || !IsSeriesHidden(group.Slot))];
        if (visible.Count == 0)
        {
            return;
        }

        var scale = ChartScale.Nice(visible.Select(group => group.From).Min(), visible.Select(group => group.To).Max(), includeZero: false);
        var curves = visible.Select(group => ChartDensity.Curve(group.Sorted, group.Bandwidth, group.From, group.To)).ToList();
        var peak = curves.SelectMany(curve => curve).Select(point => point.Density).Max();

        builder.XAxis = new ChartAxis([.. scale.Ticks.Select(tick => new ChartTick(scale.Map(tick), FormatValue(tick)))]);
        builder.Line(0, 100, 100, 100, ChartColor.Axis);

        for (var i = 0; i < visible.Count; i++)
        {
            var color = ChartColor.Series(visible[i].Slot);
            List<(double X, double Y)> line = [.. curves[i].Select(point => (scale.Map(point.Value), 100 - point.Density / peak * peakHeight))];
            List<(double X, double Y)> area = [(line[0].X, 100), .. line, (line[^1].X, 100)];

            builder.Path(ChartGeometry.Line(area, close: true), color, filled: true, opacity: 0.2);
            builder.Path(ChartGeometry.Line(line), color);
            builder.ShapeDatum(area, visible[i].Label, [.. labels.Zip(visible[i].Summary, (label, value) => new ChartDatumRow(label, value))], line.MinBy(point => point.Y));
        }
    }

    private DensityGroup CreateGroup(ChartSample sample, int slot)
    {
        IReadOnlyList<double> sorted = [.. sample.Values.Where(double.IsFinite).Order()];
        if (sorted.Count == 0)
        {
            return new DensityGroup(slot, sample.Label, sorted, [], 0);
        }

        string[] summary = [sorted.Count.ToString("#,0"), FormatValue(sorted[0]), FormatValue(ChartGeometry.Percentile(sorted, 0.5)), FormatValue(sorted[^1])];
        return new DensityGroup(slot, sample.Label, sorted, summary, ChartDensity.Bandwidth(sorted));
    }

    /// <summary>
    /// One group of observations with what the plot needs to draw and describe it.
    /// </summary>
    /// <param name="Slot">The index of the group in <see cref="Samples"/>, which fixes its color.</param>
    /// <param name="Label">The name of the group.</param>
    /// <param name="Sorted">The observations in ascending order.</param>
    /// <param name="Summary">The count, minimum, median and maximum, formatted for the tooltip and the table view.</param>
    /// <param name="Bandwidth">The kernel width of the group's curve.</param>
    private sealed record DensityGroup(int Slot, string Label, IReadOnlyList<double> Sorted, string[] Summary, double Bandwidth)
    {
        /// <summary>Gets the value the curve starts at.</summary>
        public double From => Sorted[0] - Bandwidth * tail;

        /// <summary>Gets the value the curve ends at.</summary>
        public double To => Sorted[^1] + Bandwidth * tail;
    }
}

/// <summary>
/// A ridgeline plot: one density curve per group, each on its own row, for comparing the shape of many
/// distributions without the curves hiding one another.
/// </summary>
/// <remarks>
/// Each ridge is scaled to the height of its own row, so compare where the ridges sit and how wide they are, not
/// how tall. Set <see cref="TwSampleChartBase.Horizontal"/> to <see langword="false"/> to stand the ridges side by side.
/// </remarks>
public class TwRidgelinePlotChart : TwSampleChartBase
{
    private const double tail = 3;

    /// <summary>
    /// Initializes a new instance of the <see cref="TwRidgelinePlotChart"/> class, which lists its groups top to bottom by default.
    /// </summary>
    public TwRidgelinePlotChart()
    {
        Horizontal = true;
    }

    private protected override void BuildGroup(ChartSceneBuilder builder, ChartScale scale, double center, double slot, IReadOnlyList<double> sorted)
    {
        var color = ChartColor.Series(0);
        var bandwidth = ChartDensity.Bandwidth(sorted);

        // The axis covers the observations, so a tail that would run past it is cut off at the edge of the plot.
        var from = Math.Max(scale.Min, sorted[0] - bandwidth * tail);
        var to = Math.Min(scale.Max, sorted[^1] + bandwidth * tail);
        var curve = ChartDensity.Curve(sorted, bandwidth, from, to);
        var peak = curve.Max(point => point.Density);
        var floor = center + slot / 2;

        List<(double X, double Y)> ridge = [.. curve.Select(point => builder.Point(floor - point.Density / peak * slot * 0.9, scale.Map(point.Value)))];
        List<(double X, double Y)> area = [builder.Point(floor, scale.Map(from)), .. ridge, builder.Point(floor, scale.Map(to))];

        builder.Path(ChartGeometry.Line(area, close: true), color, filled: true, opacity: 0.3);
        builder.Path(ChartGeometry.Line(ridge), color);
    }
}

/// <summary>
/// A horizon chart: one compact row per series in which the values are cut into bands and layered, so a strong
/// value shows as a darker color instead of a taller mark. It fits many series into little height.
/// </summary>
/// <remarks>
/// Every row shares one scale, so the same darkness means the same magnitude in each. Negative values use the
/// color for an unfavourable change and are named as negative in tooltips and the table view.
/// </remarks>
public class TwHorizonChart : TwCategoryChartBase
{
    // Layered bands of this opacity build up to a solid color at the top band.
    private const double bandOpacity = 0.45;

    /// <summary>
    /// Gets or sets how many bands the values are cut into, from 1 to 6.
    /// </summary>
    /// <remarks>
    /// Default is 3. More bands show finer differences but are harder to tell apart.
    /// </remarks>
    [Parameter] public int BandCount { get; set; } = 3;

    /// <inheritdoc />
    protected override bool hasSeriesLegend => false;

    /// <inheritdoc />
    protected override void BuildMarks(ChartSceneBuilder builder, IReadOnlyList<ChartSeriesSlot> series)
    {
        var peak = GetValues(series).Where(double.IsFinite).Select(Math.Abs).DefaultIfEmpty(0).Max();
        var bands = Math.Clamp(BandCount, 1, 6);
        var columns = new ChartBand(Categories.Count);
        var rows = new ChartBand(series.Count);
        var gap = Math.Min(2, rows.Step * 0.1);

        builder.YAxis = new ChartAxis([.. series.Select((slot, index) => new ChartTick(rows.Center(index), slot.Series.Name))]);
        builder.BandAxis(Categories, columns);

        for (var row = 0; row < series.Count; row++)
        {
            var floor = rows.Step * (row + 1) - gap;
            var height = rows.Step - gap * 2;
            builder.Line(0, floor, 100, floor, ChartColor.Axis);

            for (var column = 0; column < Categories.Count && peak > 0; column++)
            {
                if (series[row].Value(column) is { } value && double.IsFinite(value))
                {
                    BuildBands(builder, value, Math.Abs(value) / peak * bands, columns.Step * column, floor, columns.Step, height);
                }
            }
        }

        for (var column = 0; column < Categories.Count; column++)
        {
            builder.BandDatum(columns.Center(column), columns.Step, Categories[column], GetRows(series, column), ChartHover.Crosshair);
        }
    }

    /// <summary>
    /// Adds the layered bands for one value: a full band for each whole band the value reaches, and a part of the next.
    /// </summary>
    /// <param name="builder">The builder that collects the scene.</param>
    /// <param name="value">The value, whose sign picks the color.</param>
    /// <param name="magnitude">The size of the value, measured in bands.</param>
    /// <param name="x">The left edge of the value's column.</param>
    /// <param name="floor">The bottom of the value's row.</param>
    /// <param name="width">The width of the column.</param>
    /// <param name="height">The height of one band.</param>
    private static void BuildBands(ChartSceneBuilder builder, double value, double magnitude, double x, double floor, double width, double height)
    {
        for (var band = 0; magnitude > band; band++)
        {
            var fill = Math.Min(1, magnitude - band) * height;
            builder.Add(new ChartRect(x, floor - fill, width, fill, value < 0 ? ChartColor.Negative : ChartColor.Sequential, Opacity: bandOpacity, Gap: false));
        }
    }
}

/// <summary>
/// A radial histogram: the values are grouped into equal ranges that wrap around a circle, each bar reaching
/// further out the more values fall in its range. It suits cyclical data such as hours of the day or compass bearings.
/// </summary>
/// <remarks>
/// Bars start from an inner ring so that small counts stay visible. For values that do not wrap around, prefer
/// <see cref="TwHistogramChart"/>, whose bars are easier to compare.
/// </remarks>
public class TwRadialHistogramChart : TwChartBase
{
    private const double innerRadius = 12;

    private const double outerRadius = 48;

    /// <summary>
    /// Gets or sets the raw observations to group.
    /// </summary>
    [Parameter] public IReadOnlyList<double> Values { get; set; } = [];

    /// <summary>
    /// Gets or sets how many bins to group the values into, from 4 to 36.
    /// </summary>
    /// <remarks>
    /// Default is 12.
    /// </remarks>
    [Parameter] public int BinCount { get; set; } = 12;

    /// <summary>
    /// Gets or sets the label for a bin's range in the table view.
    /// Defaults to the localized <see cref="TwStrings.Chart_Range"/> text ("Range").
    /// </summary>
    [Parameter] public string? RangeLabel { get; set; }

    private string effectiveRangeLabel => RangeLabel ?? localizer[TwStrings.Chart_Range];

    /// <summary>
    /// Gets or sets the label for a bin's count in tooltips and the table view.
    /// Defaults to the localized <see cref="TwStrings.Chart_Count"/> text ("Count").
    /// </summary>
    [Parameter] public string? CountLabel { get; set; }

    private string effectiveCountLabel => CountLabel ?? localizer[TwStrings.Chart_Count];

    /// <inheritdoc />
    protected internal override void BuildScene(ChartSceneBuilder builder)
    {
        var values = Values.Where(double.IsFinite).ToList();
        if (values.Count == 0)
        {
            return;
        }

        builder.Shape = ChartShape.Square;
        var bins = Math.Clamp(BinCount, 4, 36);
        var (min, max) = (values.Min(), values.Max());
        var size = max > min ? (max - min) / bins : 1;
        var counts = new int[bins];

        foreach (var value in values)
        {
            counts[Math.Min(bins - 1, (int)((value - min) / size))]++;
        }

        var peak = counts.Max();
        var span = 360.0 / bins;
        var color = ChartColor.Series(0);
        builder.Path(ChartGeometry.Sector(50, 50, innerRadius, outerRadius, 0, 360), ChartColor.Track, filled: true);

        List<IReadOnlyList<string>> rows = [];
        for (var i = 0; i < bins; i++)
        {
            var range = localizer[TwStrings.Chart_RangeValue, FormatValue(min + size * i), FormatValue(min + size * (i + 1))];
            var count = counts[i].ToString("#,0");

            if (counts[i] > 0)
            {
                var reach = innerRadius + (outerRadius - innerRadius) * counts[i] / peak;
                builder.Path(ChartGeometry.Sector(50, 50, innerRadius, reach, span * i, span * (i + 1)), color, filled: true, outline: true, title: $"{range}: {count}");
            }

            builder.SectorDatum(new ChartSector(50, 50, innerRadius, outerRadius, span * i, span * (i + 1)), range, [new ChartDatumRow(effectiveCountLabel, count, color)]);
            rows.Add([range, count]);
        }

        builder.Table = new ChartTable([effectiveRangeLabel, effectiveCountLabel], rows);
    }
}

/// <summary>
/// A beeswarm chart: every observation drawn as a dot along the value axis, nudged sideways just far enough that
/// no two dots overlap, so both the spread and the number of values at each level are visible.
/// </summary>
/// <remarks>
/// A swarm widens where values cluster. Once a cluster is too large for its group's slot the outermost dots
/// overlap; for that much data prefer <see cref="TwViolinPlotChart"/> or <see cref="TwBoxPlotChart"/>.
/// </remarks>
public class TwBeeswarmChart : TwSampleChartBase
{
    private const double diameter = 8;

    // The dot plus its surface-colored ring.
    private const double spacing = diameter + 2;

    private protected override void BuildGroup(ChartSceneBuilder builder, ChartScale scale, double center, double slot, IReadOnlyList<double> sorted)
    {
        var height = ChartMetrics.PlotHeight(effectiveHeight);
        var valuePixels = (Horizontal ? height * ChartMetrics.WideAspect : height) / 100;
        var bandPixels = (Horizontal ? height : height * ChartMetrics.WideAspect) / 100;
        var limit = slot * 0.45 * bandPixels;
        List<(double Offset, double Position)> placed = [];

        foreach (var value in sorted)
        {
            var position = scale.Map(value) * valuePixels;

            // Each dot already placed close enough along the value axis rules out a stretch of sideways offsets.
            // The free offset nearest the middle is either the middle itself or the edge of one of those stretches.
            var blocked = placed.Where(dot => Math.Abs(dot.Position - position) < spacing)
                .Select(dot => (dot.Offset, Reach: Math.Sqrt(spacing * spacing - (dot.Position - position) * (dot.Position - position)))).ToList();
            var offset = blocked.SelectMany(dot => (double[])[dot.Offset - dot.Reach, dot.Offset + dot.Reach]).Prepend(0)
                .Where(candidate => blocked.All(dot => Math.Abs(candidate - dot.Offset) >= dot.Reach - 1e-9))
                .MinBy(Math.Abs);

            offset = Math.Clamp(offset, -limit, limit);
            placed.Add((offset, position));
            builder.Marker(center + offset / bandPixels, scale.Map(value), ChartColor.Series(0), diameter, 0.8);
        }
    }
}

/// <summary>
/// A violin plot: for each group, a mirrored density curve showing where its values concentrate, with a bar
/// across the middle half of the values and a dot at the median.
/// </summary>
/// <remarks>
/// Each violin is drawn to the full width of its slot and ends at the group's lowest and highest values, so
/// compare the shapes, not the widths.
/// </remarks>
public class TwViolinPlotChart : TwSampleChartBase
{
    private protected override void BuildGroup(ChartSceneBuilder builder, ChartScale scale, double center, double slot, IReadOnlyList<double> sorted)
    {
        var color = ChartColor.Series(0);
        var halfWidth = Math.Min(slot * 0.4, 18);
        var curve = ChartDensity.Curve(sorted, ChartDensity.Bandwidth(sorted), sorted[0], sorted[^1]);
        var peak = curve.Max(point => point.Density);

        if (sorted[^1] > sorted[0])
        {
            List<(double X, double Y)> outline =
            [
                .. curve.Select(point => builder.Point(center - point.Density / peak * halfWidth, scale.Map(point.Value))),
                .. Enumerable.Reverse(curve).Select(point => builder.Point(center + point.Density / peak * halfWidth, scale.Map(point.Value)))
            ];
            builder.Path(ChartGeometry.Line(outline, close: true), color, filled: true, opacity: 0.3);
        }

        builder.Segment(center, scale.Map(ChartGeometry.Percentile(sorted, 0.25)), center, scale.Map(ChartGeometry.Percentile(sorted, 0.75)), color, ChartStroke.Thick);
        builder.Marker(center, scale.Map(ChartGeometry.Percentile(sorted, 0.5)), ChartColor.Ink, 6);
    }
}

/// <summary>
/// Kernel density estimation shared by the density, ridgeline and violin charts.
/// </summary>
internal static class ChartDensity
{
    private const int steps = 64;

    /// <summary>
    /// Gets the kernel width for a set of observations using Silverman's rule of thumb, which narrows as the
    /// observations cluster and as there are more of them.
    /// </summary>
    /// <param name="sorted">The observations in ascending order. Never empty.</param>
    public static double Bandwidth(IReadOnlyList<double> sorted)
    {
        var mean = sorted.Average();
        var deviation = Math.Sqrt(sorted.Sum(value => (value - mean) * (value - mean)) / Math.Max(1, sorted.Count - 1));
        var quartiles = (ChartGeometry.Percentile(sorted, 0.75) - ChartGeometry.Percentile(sorted, 0.25)) / 1.34;
        var spread = quartiles > 0 ? Math.Min(deviation, quartiles) : deviation;

        // Identical observations have no spread to estimate from, so give them a narrow spike.
        return spread > 0 ? 0.9 * spread * Math.Pow(sorted.Count, -0.2) : Math.Max(Math.Abs(mean), 1) * 0.01;
    }

    /// <summary>
    /// Estimates the density of a set of observations at evenly spaced values between two limits.
    /// </summary>
    /// <param name="sorted">The observations. Never empty.</param>
    /// <param name="bandwidth">The kernel width, from <see cref="Bandwidth"/>.</param>
    /// <param name="from">The first value to estimate at.</param>
    /// <param name="to">The last value to estimate at.</param>
    public static (double Value, double Density)[] Curve(IReadOnlyList<double> sorted, double bandwidth, double from, double to)
    {
        var normalizer = sorted.Count * bandwidth * Math.Sqrt(2 * Math.PI);
        return [.. Enumerable.Range(0, steps + 1).Select(step =>
        {
            var value = from + (to - from) * step / steps;
            return (value, sorted.Sum(sample => Math.Exp(-0.5 * (value - sample) * (value - sample) / (bandwidth * bandwidth))) / normalizer);
        })];
    }
}