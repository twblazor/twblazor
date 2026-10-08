// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using Microsoft.AspNetCore.Components;
using TwBlazor.Charting;

namespace TwBlazor.Components;

/// <summary>
/// A dot plot: one dot per series on a shared line for each category, so values are compared by position.
/// Unlike bars the scale does not have to start at zero, which suits values that sit in a narrow range.
/// </summary>
public class TwDotPlotChart : TwCategoryChartBase
{
    /// <summary>
    /// Gets or sets whether categories run left to right with values bottom to top, instead of the default
    /// rows with values left to right.
    /// </summary>
    [Parameter] public bool Vertical { get; set; }

    private protected virtual bool hasConnector => false;

    /// <inheritdoc />
    protected override void BuildMarks(ChartSceneBuilder builder, IReadOnlyList<ChartSeriesSlot> series)
    {
        builder.Horizontal = !Vertical;
        var scale = ChartScale.Nice(GetValues(series), includeZero: false);
        var band = new ChartBand(Categories.Count);
        builder.ValueAxis(scale, FormatValue);
        builder.BandAxis(Categories, band);

        for (var i = 0; i < Categories.Count; i++)
        {
            var center = band.Center(i);
            var values = series.Select(slot => slot.Value(i)).Where(value => value.HasValue).Select(value => scale.Map(value!.Value)).ToList();

            if (hasConnector && values.Count > 1)
            {
                builder.Segment(center, values.Min(), center, values.Max(), ChartColor.Track, ChartStroke.Thick);
            }

            foreach (var slot in series)
            {
                if (slot.Value(i) is { } value)
                {
                    builder.Marker(center, scale.Map(value), slot.Color, 12);
                }
            }

            builder.BandDatum(center, band.Step, Categories[i], GetRows(series, i));
        }
    }
}

/// <summary>
/// A dumbbell chart: a dot plot whose dots are joined by a bar, drawing the eye to the gap between two (or more)
/// values for each category, such as before and after.
/// </summary>
public class TwDumbbellChart : TwDotPlotChart
{
    private protected override bool hasConnector => true;
}
