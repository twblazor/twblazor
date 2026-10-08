// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using Microsoft.AspNetCore.Components;
using TwBlazor.Charting;

namespace TwBlazor.Components;

/// <summary>
/// A lollipop chart: a thin stem ending in a dot for each value. It reads like a bar chart with less ink, which
/// helps when there are many categories with similar values.
/// </summary>
public class TwLollipopChart : TwCategoryChartBase
{
    /// <summary>
    /// Gets or sets whether the stems run left to right with categories listed top to bottom.
    /// </summary>
    [Parameter] public bool Horizontal { get; set; }

    /// <summary>
    /// Gets or sets whether each dot is labelled with its value.
    /// </summary>
    [Parameter] public bool ShowValues { get; set; }

    /// <inheritdoc />
    protected override void BuildMarks(ChartSceneBuilder builder, IReadOnlyList<ChartSeriesSlot> series)
    {
        builder.Horizontal = Horizontal;
        var scale = ChartScale.Nice(GetValues(series));
        var band = new ChartBand(Categories.Count);
        var zero = scale.Map(0);
        builder.ValueAxis(scale, FormatValue);
        builder.BandAxis(Categories, band);
        builder.Baseline(zero);

        for (var i = 0; i < Categories.Count; i++)
        {
            var group = Math.Min(band.Step * 0.7, 8.0 * series.Count);
            var slot = group / series.Count;

            for (var k = 0; k < series.Count; k++)
            {
                if (series[k].Value(i) is not { } value)
                {
                    continue;
                }

                var position = band.Center(i) - group / 2 + slot * (k + 0.5);
                builder.Segment(position, zero, position, scale.Map(value), series[k].Color);
                builder.Marker(position, scale.Map(value), series[k].Color, 12);

                if (ShowValues)
                {
                    builder.TipLabel(position, scale.Map(value), FormatValue(value), value >= 0);
                }
            }

            builder.BandDatum(band.Center(i), band.Step, Categories[i], GetRows(series, i));
        }
    }
}
