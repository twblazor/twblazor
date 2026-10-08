// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using Microsoft.AspNetCore.Components;
using TwBlazor.Charting;
using TwBlazor.Models;

namespace TwBlazor.Components;

/// <summary>
/// The shared definition of the charts that summarise groups of raw observations:
/// <see cref="TwBoxPlotChart"/> and <see cref="TwStripPlotChart"/>.
/// </summary>
public abstract class TwSampleChartBase : TwChartBase
{
    /// <summary>
    /// Gets or sets the groups of raw observations to plot.
    /// </summary>
    [Parameter] public IReadOnlyList<ChartSample> Samples { get; set; } = [];

    /// <summary>
    /// Gets or sets whether groups are listed top to bottom with values running left to right.
    /// </summary>
    [Parameter] public bool Horizontal { get; set; }

    /// <summary>
    /// Gets or sets the heading of the group column in the table view.
    /// </summary>
    [Parameter] public string LabelHeader { get; set; } = "Group";

    /// <summary>
    /// Adds the marks for one group.
    /// </summary>
    /// <param name="builder">The builder that collects the scene.</param>
    /// <param name="scale">The value scale shared by every group.</param>
    /// <param name="center">The center of the group's slot on the category axis.</param>
    /// <param name="slot">The size of the group's slot on the category axis.</param>
    /// <param name="sorted">The group's observations in ascending order. Never empty.</param>
    private protected abstract void BuildGroup(ChartSceneBuilder builder, ChartScale scale, double center, double slot, IReadOnlyList<double> sorted);

    /// <inheritdoc />
    protected internal override void BuildScene(ChartSceneBuilder builder)
    {
        var groups = Samples.Select(sample => (sample.Label, Sorted: (IReadOnlyList<double>)[.. sample.Values.Where(double.IsFinite).Order()])).Where(group => group.Sorted.Count > 0).ToList();
        if (groups.Count == 0)
        {
            return;
        }

        builder.Horizontal = Horizontal;
        var scale = ChartScale.Nice(groups.SelectMany(group => group.Sorted), includeZero: false);
        var band = new ChartBand(groups.Count);
        builder.ValueAxis(scale, FormatValue);
        builder.BandAxis([.. groups.Select(group => group.Label)], band);

        List<IReadOnlyList<string>> rows = [];
        for (var i = 0; i < groups.Count; i++)
        {
            var sorted = groups[i].Sorted;
            string[] summary =
            [
                sorted.Count.ToString("#,0"),
                FormatValue(sorted[0]),
                FormatValue(ChartGeometry.Percentile(sorted, 0.25)),
                FormatValue(ChartGeometry.Percentile(sorted, 0.5)),
                FormatValue(ChartGeometry.Percentile(sorted, 0.75)),
                FormatValue(sorted[^1])
            ];

            BuildGroup(builder, scale, band.Center(i), band.Step, sorted);
            builder.BandDatum(band.Center(i), band.Step, groups[i].Label, [.. _summaryLabels.Zip(summary, (label, value) => new ChartDatumRow(label, value))]);
            rows.Add([groups[i].Label, .. summary]);
        }

        builder.Table = new ChartTable([LabelHeader, .. _summaryLabels], rows);
    }

    private static readonly string[] _summaryLabels = ["Count", "Minimum", "Lower quartile", "Median", "Upper quartile", "Maximum"];
}

/// <summary>
/// A box plot: for each group, a box spanning the middle half of the values with a line at the median, whiskers
/// reaching to the furthest values within 1.5 times the box's length, and dots for outliers beyond them.
/// </summary>
public class TwBoxPlotChart : TwSampleChartBase
{
    private protected override void BuildGroup(ChartSceneBuilder builder, ChartScale scale, double center, double slot, IReadOnlyList<double> sorted)
    {
        var color = ChartColor.Series(0);
        var thickness = Math.Min(slot * 0.5, 12);
        var lowerQuartile = ChartGeometry.Percentile(sorted, 0.25);
        var upperQuartile = ChartGeometry.Percentile(sorted, 0.75);
        var median = scale.Map(ChartGeometry.Percentile(sorted, 0.5));
        var reach = (upperQuartile - lowerQuartile) * 1.5;
        var low = sorted.First(value => value >= lowerQuartile - reach);
        var high = sorted.Last(value => value <= upperQuartile + reach);

        builder.Segment(center, scale.Map(low), center, scale.Map(high), color);
        builder.Segment(center - thickness / 4, scale.Map(low), center + thickness / 4, scale.Map(low), color);
        builder.Segment(center - thickness / 4, scale.Map(high), center + thickness / 4, scale.Map(high), color);
        builder.Bar(center, thickness, scale.Map(lowerQuartile), scale.Map(upperQuartile), ChartColor.Track, ChartCorner.All);
        builder.Bar(center, thickness, median - 0.4, median + 0.4, color, ChartCorner.None, gap: false);

        foreach (var outlier in sorted.Where(value => value < low || value > high))
        {
            builder.Marker(center, scale.Map(outlier), color, 6, 0.7);
        }
    }
}

/// <summary>
/// A strip plot: every observation drawn as a dot along the value axis, one strip per group, so the raw spread
/// and any clusters are visible. Set <see cref="Jitter"/> to spread overlapping dots sideways (a jitter plot).
/// </summary>
public class TwStripPlotChart : TwSampleChartBase
{
    /// <summary>
    /// Gets or sets whether dots are nudged sideways by a small, repeatable amount so overlapping values stay visible.
    /// </summary>
    [Parameter] public bool Jitter { get; set; }

    private protected override void BuildGroup(ChartSceneBuilder builder, ChartScale scale, double center, double slot, IReadOnlyList<double> sorted)
    {
        for (var i = 0; i < sorted.Count; i++)
        {
            // A fixed multiplicative hash, not a random number, so the same data always draws the same picture.
            var nudge = Jitter ? ((uint)(i + 1) * 2654435761u % 1000 / 1000.0 - 0.5) * slot * 0.5 : 0;
            builder.Marker(center + nudge, scale.Map(sorted[i]), ChartColor.Series(0), 8, 0.6);
        }
    }
}
