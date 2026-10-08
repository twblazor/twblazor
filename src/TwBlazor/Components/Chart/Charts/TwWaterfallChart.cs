// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using Microsoft.AspNetCore.Components;
using TwBlazor.Charting;

namespace TwBlazor.Components;

/// <summary>
/// A waterfall chart: floating columns that show how a series of increases and decreases build up to a total.
/// </summary>
/// <remarks>
/// Each value in <see cref="TwValueChartBase.Data"/> is a change, positive or negative. Direction is shown by
/// color and by the legend, so it never relies on color alone.
/// </remarks>
public class TwWaterfallChart : TwValueChartBase
{
    /// <summary>
    /// Gets or sets whether a final column showing the running total is added.
    /// </summary>
    [Parameter] public bool ShowTotal { get; set; } = true;

    /// <summary>
    /// Gets or sets the label of the total column.
    /// </summary>
    [Parameter] public string TotalLabel { get; set; } = "Total";

    /// <summary>
    /// Gets or sets the legend label for positive changes.
    /// </summary>
    [Parameter] public string IncreaseLabel { get; set; } = "Increase";

    /// <summary>
    /// Gets or sets the legend label for negative changes.
    /// </summary>
    [Parameter] public string DecreaseLabel { get; set; } = "Decrease";

    /// <summary>
    /// Gets or sets whether each column is labelled with its change.
    /// </summary>
    [Parameter] public bool ShowValues { get; set; }

    /// <inheritdoc />
    protected override void BuildMarks(ChartSceneBuilder builder)
    {
        double running = 0, min = 0, max = 0;
        foreach (var item in Data)
        {
            running += item.Value;
            min = Math.Min(min, running);
            max = Math.Max(max, running);
        }

        var count = Data.Count + (ShowTotal ? 1 : 0);
        var scale = ChartScale.Nice(min, max);
        var band = new ChartBand(count);
        var thickness = Math.Min(band.Step * 0.6, 12);
        builder.ValueAxis(scale, FormatValue);
        builder.BandAxis([.. Data.Select(item => item.Label), .. ShowTotal ? [TotalLabel] : Array.Empty<string>()], band);

        running = 0;
        for (var i = 0; i < Data.Count; i++)
        {
            var item = Data[i];
            var from = running;
            running += item.Value;
            var color = item.Value >= 0 ? ChartColor.Positive : ChartColor.Negative;
            builder.Bar(band.Center(i), thickness, scale.Map(from), scale.Map(running), color);

            if (i < count - 1)
            {
                builder.Segment(band.Center(i) + thickness / 2, scale.Map(running), band.Center(i + 1) - thickness / 2, scale.Map(running), ChartColor.Axis, ChartStroke.Hairline);
            }

            if (ShowValues)
            {
                builder.TipLabel(band.Center(i), scale.Map(Math.Max(from, running)), FormatChange(item.Value));
            }

            builder.BandDatum(band.Center(i), band.Step, item.Label,
            [
                new ChartDatumRow(item.Value >= 0 ? IncreaseLabel : DecreaseLabel, FormatChange(item.Value), color),
                new ChartDatumRow(TotalLabel, FormatValue(running))
            ]);
        }

        if (ShowTotal)
        {
            builder.Bar(band.Center(count - 1), thickness, scale.Map(0), scale.Map(running), ChartColor.Total);
            builder.BandDatum(band.Center(count - 1), band.Step, TotalLabel, [new ChartDatumRow(TotalLabel, FormatValue(running), ChartColor.Total)]);
        }

        builder.Baseline(scale.Map(0));
        builder.Legend(IncreaseLabel, ChartColor.Positive);
        builder.Legend(DecreaseLabel, ChartColor.Negative);

        if (ShowTotal)
        {
            builder.Legend(TotalLabel, ChartColor.Total);
        }
    }

    private string FormatChange(double value) => (value > 0 ? "+" : string.Empty) + FormatValue(value);
}
