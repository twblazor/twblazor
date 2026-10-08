// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using Microsoft.AspNetCore.Components;
using TwBlazor.Charting;

namespace TwBlazor.Components;

/// <summary>
/// A histogram: raw values grouped into equal-width bins, with a column per bin showing how many values fall in
/// it, revealing the shape of a distribution.
/// </summary>
public class TwHistogramChart : TwChartBase
{
    /// <summary>
    /// Gets or sets the raw values to group.
    /// </summary>
    [Parameter] public IReadOnlyList<double> Values { get; set; } = [];

    /// <summary>
    /// Gets or sets how many bins to group the values into.
    /// </summary>
    /// <remarks>
    /// If not set, the count follows Sturges' rule, which grows slowly with the number of values.
    /// </remarks>
    [Parameter] public int? BinCount { get; set; }

    /// <summary>
    /// Gets or sets the label for a bin's range in tooltips and the table view.
    /// </summary>
    [Parameter] public string RangeLabel { get; set; } = "Range";

    /// <summary>
    /// Gets or sets the label for a bin's count in tooltips and the table view.
    /// </summary>
    [Parameter] public string CountLabel { get; set; } = "Count";

    /// <inheritdoc />
    protected internal override void BuildScene(ChartSceneBuilder builder)
    {
        var values = Values.Where(double.IsFinite).ToList();
        if (values.Count == 0)
        {
            return;
        }

        var bins = Math.Max(1, BinCount ?? (int)Math.Ceiling(Math.Log2(values.Count)) + 1);
        var (min, max) = (values.Min(), values.Max());
        var size = max > min ? (max - min) / bins : 1;
        var counts = new int[bins];

        foreach (var value in values)
        {
            counts[Math.Min(bins - 1, (int)((value - min) / size))]++;
        }

        var scale = ChartScale.Nice(0, counts.Max());
        var band = new ChartBand(bins);
        var every = (int)Math.Ceiling((bins + 1) / 8.0);
        builder.ValueAxis(scale, value => value.ToString("#,0"), CountLabel);
        builder.XAxis = new ChartAxis([.. Enumerable.Range(0, bins + 1).Where(edge => edge % every == 0).Select(edge => new ChartTick(band.Step * edge, FormatValue(min + size * edge)))]);

        List<IReadOnlyList<string>> rows = [];
        for (var i = 0; i < bins; i++)
        {
            var range = $"{FormatValue(min + size * i)} to {FormatValue(min + size * (i + 1))}";
            builder.Bar(band.Center(i), band.Step, 0, scale.Map(counts[i]), ChartColor.Series(0));
            builder.BandDatum(band.Center(i), band.Step, range, [new ChartDatumRow(CountLabel, counts[i].ToString("#,0"), ChartColor.Series(0))]);
            rows.Add([range, counts[i].ToString("#,0")]);
        }

        builder.Baseline(0);
        builder.Table = new ChartTable([RangeLabel, CountLabel], rows);
    }
}
