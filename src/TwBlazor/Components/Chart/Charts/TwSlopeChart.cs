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
        builder.XAxis = new ChartAxis([.. Categories.Select((category, index) => new ChartTick(GetColumn(index), category))]);

        for (var i = 0; i < Categories.Count; i++)
        {
            builder.Line(GetColumn(i), 0, GetColumn(i), 100, ChartColor.Grid);
        }

        foreach (var slot in series)
        {
            BuildLine(builder, scale, slot);
        }
    }

    /// <summary>
    /// Gets the horizontal position of a category, inset from both edges to leave room for the end labels.
    /// </summary>
    private double GetColumn(int index) => Categories.Count == 1 ? 50 : 25 + 50.0 / (Categories.Count - 1) * index;

    private void BuildLine(ChartSceneBuilder builder, ChartScale scale, ChartSeriesSlot slot)
    {
        List<(double X, double Y)> points = [];
        for (var i = 0; i < Categories.Count; i++)
        {
            if (slot.Value(i) is not { } value)
            {
                continue;
            }

            var point = (X: GetColumn(i), Y: 100 - scale.Map(value));
            points.Add(point);
            builder.Datum(point.X, point.Y, 0, 0, slot.Series.Name, [new ChartDatumRow(Categories[i], FormatValue(value), slot.Color)], ChartHover.None);
            BuildEndLabel(builder, point, i, slot.Series.Name, FormatValue(value));
        }

        builder.Path(ChartGeometry.Line(points), slot.Color);
        points.ForEach(point => builder.Dot(point.X, point.Y, slot.Color));
    }

    private void BuildEndLabel(ChartSceneBuilder builder, (double X, double Y) point, int category, string name, string value)
    {
        if (category == 0)
        {
            builder.Add(new ChartText(point.X, point.Y, $"{name} {value}", ChartAnchor.End, Tone: ChartTextTone.Primary, OffsetX: -10));
        }
        else if (category == Categories.Count - 1)
        {
            builder.Add(new ChartText(point.X, point.Y, value, ChartAnchor.Start, Tone: ChartTextTone.Primary, OffsetX: 10));
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
                builder.Add(new ChartText(line[^1].X, line[^1].Y, slot.Series.Name, ChartAnchor.Start, Tone: ChartTextTone.Primary, OffsetX: 12));
            }
        }
    }
}
