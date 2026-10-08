// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using Microsoft.AspNetCore.Components;
using TwBlazor.Charting;
using TwBlazor.Enums;

namespace TwBlazor.Components;

/// <summary>
/// Small multiples: the same simple chart repeated in a grid, one panel per series, all on one shared scale so
/// the panels can be compared at a glance. Use it instead of crowding many series into one plot.
/// </summary>
public class TwSmallMultiplesChart : TwCategoryChartBase
{
    private const double titleSpace = 0.2;
    private const double labelSpace = 0.14;
    private const double gutter = 3;

    /// <summary>
    /// Gets or sets the mark drawn in each panel: columns, a line or an area.
    /// </summary>
    [Parameter] public SmallMultipleKind Kind { get; set; }

    /// <summary>
    /// Gets or sets how many panels sit in a row.
    /// </summary>
    /// <remarks>
    /// If not set, up to three panels share a row.
    /// </remarks>
    [Parameter] public int? Columns { get; set; }

    /// <inheritdoc />
    protected override bool hasSeriesLegend => false;

    /// <inheritdoc />
    protected override void BuildMarks(ChartSceneBuilder builder, IReadOnlyList<ChartSeriesSlot> series)
    {
        var perRow = Math.Clamp(Columns ?? 3, 1, series.Count);
        var rowCount = (int)Math.Ceiling(series.Count / (double)perRow);
        var cellWidth = 100.0 / perRow;
        var cellHeight = 100.0 / rowCount;
        var scale = ChartScale.Nice(GetValues(series), includeZero: Kind != SmallMultipleKind.Line);
        var floor = scale.Map(Math.Clamp(0, scale.Min, scale.Max));
        var color = ChartColor.Series(0);

        for (var panel = 0; panel < series.Count; panel++)
        {
            var slot = series[panel];
            var left = cellWidth * (panel % perRow) + gutter / 2;
            var top = cellHeight * (panel / perRow);
            var width = cellWidth - gutter;
            var plotTop = top + cellHeight * titleSpace;
            var plotHeight = cellHeight * (1 - titleSpace - labelSpace);
            var band = new ChartBand(Categories.Count, left, left + width);
            double Y(double value) => plotTop + plotHeight * (1 - scale.Map(value) / 100);

            builder.Text(left, top + 1, slot.Series.Name, ChartAnchor.Start, ChartBaseline.Top, ChartTextTone.Primary, strong: true);
            builder.Line(left, plotTop + plotHeight * (1 - floor / 100), left + width, plotTop + plotHeight * (1 - floor / 100), ChartColor.Axis);
            builder.Text(left, plotTop + plotHeight, Categories[0], ChartAnchor.Start, ChartBaseline.Top, offsetY: 4);

            if (Categories.Count > 1)
            {
                builder.Text(left + width, plotTop + plotHeight, Categories[^1], ChartAnchor.End, ChartBaseline.Top, offsetY: 4);
            }

            List<(double X, double Y)> points = [];
            for (var i = 0; i < Categories.Count; i++)
            {
                if (slot.Value(i) is not { } value)
                {
                    continue;
                }

                points.Add((band.Center(i), Y(value)));

                if (Kind == SmallMultipleKind.Column)
                {
                    var zero = plotTop + plotHeight * (1 - floor / 100);
                    builder.Rect(band.Center(i) - band.Step * 0.35, Math.Min(zero, Y(value)), band.Step * 0.7, Math.Abs(zero - Y(value)), color, value >= 0 ? ChartCorner.Top : ChartCorner.Bottom);
                }

                builder.Datum(band.Center(i), plotTop + plotHeight / 2, band.Step, plotHeight, $"{slot.Series.Name}, {Categories[i]}", [new ChartDatumRow(slot.Series.Name, FormatValue(value), color)]);
            }

            if (Kind != SmallMultipleKind.Column && points.Count > 0)
            {
                if (Kind == SmallMultipleKind.Area && points.Count > 1)
                {
                    var zero = plotTop + plotHeight * (1 - floor / 100);
                    var area = new ChartPathBuilder().MoveTo(points[0].X, points[0].Y).Through(points, ChartCurve.Linear).LineTo(points[^1].X, zero).LineTo(points[0].X, zero).Close();
                    builder.Path(area.ToString(), color, filled: true, opacity: 0.12);
                }

                builder.Path(ChartGeometry.Line(points), color);
            }
        }
    }
}
