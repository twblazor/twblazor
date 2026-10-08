// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using Microsoft.AspNetCore.Components;
using TwBlazor.Charting;
using TwBlazor.Enums;

namespace TwBlazor.Components;

/// <summary>
/// A slope chart: a straight line per series between two (or a few) points in time, labelled at both ends, so the
/// direction and size of each change is read from the slope.
/// </summary>
public class TwSlopeChart : TwCategoryChartBase
{
    /// <inheritdoc />
    protected override void BuildMarks(ChartSceneBuilder builder, IReadOnlyList<ChartSeriesSlot> series)
    {
        var scale = ChartScale.Nice(GetValues(series), includeZero: false);
        var band = Categories.Count == 1 ? new ChartBand(1) : new ChartBand(Categories.Count - 1, 25, 75);
        double Column(int index) => Categories.Count == 1 ? 50 : band.Start + band.Step * index;

        builder.XAxis = new ChartAxis([.. Categories.Select((category, index) => new ChartTick(Column(index), category))]);

        for (var i = 0; i < Categories.Count; i++)
        {
            builder.Line(Column(i), 0, Column(i), 100, ChartColor.Grid);
        }

        foreach (var slot in series)
        {
            List<(double X, double Y)> points = [];
            for (var i = 0; i < Categories.Count; i++)
            {
                if (slot.Value(i) is not { } value)
                {
                    continue;
                }

                var point = (X: Column(i), Y: 100 - scale.Map(value));
                points.Add(point);
                builder.Datum(point.X, point.Y, 0, 0, slot.Series.Name, [new ChartDatumRow(Categories[i], FormatValue(value), slot.Color)], ChartHover.None);

                if (i == 0)
                {
                    builder.Text(point.X, point.Y, $"{slot.Series.Name} {FormatValue(value)}", ChartAnchor.End, tone: ChartTextTone.Primary, offsetX: -10);
                }
                else if (i == Categories.Count - 1)
                {
                    builder.Text(point.X, point.Y, FormatValue(value), ChartAnchor.Start, tone: ChartTextTone.Primary, offsetX: 10);
                }
            }

            builder.Path(ChartGeometry.Line(points), slot.Color);
            foreach (var point in points)
            {
                builder.Dot(point.X, point.Y, slot.Color);
            }
        }
    }
}

/// <summary>
/// A bump chart: lines that track how each series ranks against the others across the categories. The highest
/// value in a category ranks first.
/// </summary>
public class TwBumpChart : TwCategoryChartBase
{
    /// <summary>
    /// Gets or sets the label for the rank in tooltips.
    /// </summary>
    [Parameter] public string RankLabel { get; set; } = "Rank";

    /// <inheritdoc />
    protected override void BuildMarks(ChartSceneBuilder builder, IReadOnlyList<ChartSeriesSlot> series)
    {
        var columns = new ChartBand(Categories.Count, 0, 82);
        var ranks = new ChartBand(series.Count);
        builder.XAxis = new ChartAxis([.. Categories.Select((category, index) => new ChartTick(columns.Center(index), category))]);
        builder.YAxis = new ChartAxis([.. Enumerable.Range(0, series.Count).Select(index => new ChartTick(ranks.Center(index), (index + 1).ToString()))]);

        var points = series.ToDictionary(slot => slot.Slot, _ => new List<(double X, double Y)>());
        for (var i = 0; i < Categories.Count; i++)
        {
            var ordered = series.Where(slot => slot.Value(i).HasValue).OrderByDescending(slot => slot.Value(i)).ToList();
            for (var rank = 0; rank < ordered.Count; rank++)
            {
                var slot = ordered[rank];
                var point = (X: columns.Center(i), Y: ranks.Center(rank));
                points[slot.Slot].Add(point);
                builder.Datum(point.X, point.Y, 0, 0, $"{slot.Series.Name}, {Categories[i]}",
                [
                    new ChartDatumRow(RankLabel, (rank + 1).ToString(), slot.Color),
                    new ChartDatumRow(slot.Series.Name, FormatValue(slot.Value(i)!.Value))
                ], ChartHover.None);
            }
        }

        foreach (var slot in series)
        {
            var line = points[slot.Slot];
            builder.Path(ChartGeometry.Line(line, ChartCurve.Smooth), slot.Color);

            foreach (var point in line)
            {
                builder.Dot(point.X, point.Y, slot.Color, 12);
            }

            if (line.Count > 0)
            {
                builder.Text(line[^1].X, line[^1].Y, slot.Series.Name, ChartAnchor.Start, tone: ChartTextTone.Primary, offsetX: 12);
            }
        }
    }
}
