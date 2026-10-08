// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using TwBlazor.Charting;
using TwBlazor.Enums;

namespace TwBlazor.Components;

/// <summary>
/// A radar chart: one spoke per category around a center, with each series drawn as a shape joining its values,
/// for comparing a few items across several attributes.
/// </summary>
/// <remarks>
/// Needs at least three categories. All spokes share one scale that starts at zero, so use values in the same unit.
/// </remarks>
public class TwRadarChart : TwCategoryChartBase
{
    private const double center = 50;
    private const double radius = 34;

    /// <inheritdoc />
    protected override void BuildMarks(ChartSceneBuilder builder, IReadOnlyList<ChartSeriesSlot> series)
    {
        if (Categories.Count < 3)
        {
            return;
        }

        builder.Shape = ChartShape.Square;
        var scale = ChartScale.Nice(0, GetValues(series).DefaultIfEmpty(0).Max());
        BuildGrid(builder, scale);

        foreach (var slot in series)
        {
            BuildSeries(builder, scale, slot);
        }
    }

    private (double X, double Y) GetSpoke(int index, double distance) => ChartGeometry.Polar(center, center, distance, 360.0 / Categories.Count * index);

    private void BuildGrid(ChartSceneBuilder builder, ChartScale scale)
    {
        foreach (var tick in scale.Ticks.Where(tick => tick > scale.Min))
        {
            var ring = Enumerable.Range(0, Categories.Count).Select(i => GetSpoke(i, radius * scale.Map(tick) / 100)).ToList();
            builder.Path(ChartGeometry.Line(ring, close: true), ChartColor.Grid, stroke: ChartStroke.Hairline);
        }

        for (var i = 0; i < Categories.Count; i++)
        {
            var end = GetSpoke(i, radius);
            var label = GetSpoke(i, radius + 4);
            builder.Line(center, center, end.X, end.Y, ChartColor.Grid);
            builder.Text(label.X, label.Y, Categories[i], GetAnchor(label.X), GetBaseline(label.Y));
        }
    }

    private void BuildSeries(ChartSceneBuilder builder, ChartScale scale, ChartSeriesSlot slot)
    {
        var points = Enumerable.Range(0, Categories.Count).Select(i => GetSpoke(i, radius * scale.Map(Math.Max(0, slot.Value(i) ?? 0)) / 100)).ToList();
        var outline = ChartGeometry.Line(points, ChartCurve.Linear, close: true);
        builder.Path(outline, slot.Color, filled: true, opacity: 0.1);
        builder.Path(outline, slot.Color);

        for (var i = 0; i < points.Count; i++)
        {
            builder.Dot(points[i].X, points[i].Y, slot.Color, 8);
            builder.Datum(points[i].X, points[i].Y, 0, 0, Categories[i], [new ChartDatumRow(slot.Series.Name, FormatValue(slot.Value(i) ?? 0), slot.Color)], ChartHover.None);
        }
    }

    /// <summary>
    /// Anchors a label away from the plot: labels left of center end at their position, labels right of it start there.
    /// </summary>
    private static ChartAnchor GetAnchor(double x)
    {
        if (x < center - 1)
        {
            return ChartAnchor.End;
        }

        return x > center + 1 ? ChartAnchor.Start : ChartAnchor.Middle;
    }

    /// <summary>
    /// Sits a label above the top spokes and below the bottom ones, so it never overlaps the plot.
    /// </summary>
    private static ChartBaseline GetBaseline(double y)
    {
        if (y < center - radius / 2)
        {
            return ChartBaseline.Bottom;
        }

        return y > center + radius / 2 ? ChartBaseline.Top : ChartBaseline.Middle;
    }
}
