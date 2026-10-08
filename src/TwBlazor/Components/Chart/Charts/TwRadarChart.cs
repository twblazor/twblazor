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
        var step = 360.0 / Categories.Count;
        (double X, double Y) Spoke(int index, double distance) => ChartGeometry.Polar(center, center, distance, step * index);

        foreach (var tick in scale.Ticks.Where(tick => tick > scale.Min))
        {
            var ring = Enumerable.Range(0, Categories.Count).Select(i => Spoke(i, radius * scale.Map(tick) / 100)).ToList();
            builder.Path(ChartGeometry.Line(ring, close: true), ChartColor.Grid, stroke: ChartStroke.Hairline);
        }

        for (var i = 0; i < Categories.Count; i++)
        {
            var end = Spoke(i, radius);
            var label = Spoke(i, radius + 4);
            builder.Line(center, center, end.X, end.Y, ChartColor.Grid);
            builder.Text(label.X, label.Y, Categories[i],
                label.X < center - 1 ? ChartAnchor.End : label.X > center + 1 ? ChartAnchor.Start : ChartAnchor.Middle,
                label.Y < center - radius / 2 ? ChartBaseline.Bottom : label.Y > center + radius / 2 ? ChartBaseline.Top : ChartBaseline.Middle);
        }

        foreach (var slot in series)
        {
            var points = Enumerable.Range(0, Categories.Count).Select(i => Spoke(i, radius * scale.Map(Math.Max(0, slot.Value(i) ?? 0)) / 100)).ToList();
            var outline = ChartGeometry.Line(points, ChartCurve.Linear, close: true);
            builder.Path(outline, slot.Color, filled: true, opacity: 0.1);
            builder.Path(outline, slot.Color);

            for (var i = 0; i < points.Count; i++)
            {
                builder.Dot(points[i].X, points[i].Y, slot.Color, 8);
                builder.Datum(points[i].X, points[i].Y, 0, 0, Categories[i], [new ChartDatumRow(slot.Series.Name, FormatValue(slot.Value(i) ?? 0), slot.Color)], ChartHover.None);
            }
        }
    }
}
