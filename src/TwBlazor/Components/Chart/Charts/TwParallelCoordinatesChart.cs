// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using TwBlazor.Charting;

namespace TwBlazor.Components;

/// <summary>
/// A parallel coordinates chart: one vertical axis per variable, with a line per item crossing each axis at its
/// value, for comparing items across several variables at once.
/// </summary>
/// <remarks>
/// <see cref="TwCategoryChartBase.Categories"/> are the variables and each series is one item. Every axis has
/// its own scale, labelled with its lowest and highest value, so variables in different units can sit side by side.
/// </remarks>
public class TwParallelCoordinatesChart : TwCategoryChartBase
{
    /// <inheritdoc />
    protected override void BuildMarks(ChartSceneBuilder builder, IReadOnlyList<ChartSeriesSlot> series)
    {
        var band = new ChartBand(Categories.Count);
        var scales = Enumerable.Range(0, Categories.Count)
            .Select(i => ChartScale.Nice(series.Select(slot => slot.Value(i)).Where(value => value.HasValue).Select(value => value!.Value), includeZero: false))
            .ToList();

        builder.XAxis = new ChartAxis([.. Categories.Select((category, index) => new ChartTick(band.Center(index), category))]);

        for (var i = 0; i < Categories.Count; i++)
        {
            var x = band.Center(i);
            builder.Line(x, 0, x, 100, ChartColor.Axis);
            builder.Text(x, 0, FormatValue(scales[i].Max), ChartAnchor.Start, ChartBaseline.Top, offsetX: 4);
            builder.Text(x, 100, FormatValue(scales[i].Min), ChartAnchor.Start, ChartBaseline.Bottom, offsetX: 4);
        }

        foreach (var slot in series)
        {
            List<(double X, double Y)> points = [];
            for (var i = 0; i < Categories.Count; i++)
            {
                if (slot.Value(i) is { } value)
                {
                    points.Add((band.Center(i), 100 - scales[i].Map(value)));
                }
            }

            builder.Path(ChartGeometry.Line(points), slot.Color);
        }

        for (var i = 0; i < Categories.Count; i++)
        {
            builder.BandDatum(band.Center(i), band.Step, Categories[i], GetRows(series, i), ChartHover.Crosshair);
        }
    }
}
