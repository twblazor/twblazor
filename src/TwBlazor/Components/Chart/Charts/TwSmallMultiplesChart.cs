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
        var cellWidth = 100.0 / perRow;
        var cellHeight = 100.0 / Math.Ceiling(series.Count / (double)perRow);
        var scale = ChartScale.Nice(GetValues(series), includeZero: Kind != SmallMultipleKind.Line);

        for (var index = 0; index < series.Count; index++)
        {
            var top = cellHeight * (index / perRow);
            var panel = new Panel(scale, cellWidth * (index % perRow) + gutter / 2, top, cellWidth - gutter, top + cellHeight * titleSpace, cellHeight * (1 - titleSpace - labelSpace));
            BuildFrame(builder, panel, series[index].Series.Name);
            BuildPanel(builder, panel, series[index]);
        }
    }

    private void BuildFrame(ChartSceneBuilder builder, Panel panel, string title)
    {
        builder.Add(new ChartText(panel.Left, panel.Top + 1, title, ChartAnchor.Start, ChartBaseline.Top, ChartTextTone.Primary, Strong: true));
        builder.Line(panel.Left, panel.Zero, panel.Left + panel.Width, panel.Zero, ChartColor.Axis);
        builder.Add(new ChartText(panel.Left, panel.PlotTop + panel.PlotHeight, Categories[0], ChartAnchor.Start, ChartBaseline.Top, OffsetY: 4));

        if (Categories.Count > 1)
        {
            builder.Add(new ChartText(panel.Left + panel.Width, panel.PlotTop + panel.PlotHeight, Categories[^1], ChartAnchor.End, ChartBaseline.Top, OffsetY: 4));
        }
    }

    private void BuildPanel(ChartSceneBuilder builder, Panel panel, ChartSeriesSlot slot)
    {
        var color = ChartColor.Series(0);
        var band = new ChartBand(Categories.Count, panel.Left, panel.Left + panel.Width);
        List<(double X, double Y)> points = [];

        for (var i = 0; i < Categories.Count; i++)
        {
            if (slot.Value(i) is not { } value)
            {
                continue;
            }

            var y = panel.Y(value);
            points.Add((band.Center(i), y));

            if (Kind == SmallMultipleKind.Column)
            {
                builder.Rect(band.Center(i) - band.Step * 0.35, Math.Min(panel.Zero, y), band.Step * 0.7, Math.Abs(panel.Zero - y), color, value >= 0 ? ChartCorner.Top : ChartCorner.Bottom);
            }

            builder.Datum(band.Center(i), panel.PlotTop + panel.PlotHeight / 2, band.Step, panel.PlotHeight, $"{slot.Series.Name}, {Categories[i]}", [new ChartDatumRow(slot.Series.Name, FormatValue(value), color)]);
        }

        if (Kind == SmallMultipleKind.Column || points.Count == 0)
        {
            return;
        }

        if (Kind == SmallMultipleKind.Area && points.Count > 1)
        {
            var area = new ChartPathBuilder().MoveTo(points[0].X, points[0].Y).Through(points, ChartCurve.Linear).LineTo(points[^1].X, panel.Zero).LineTo(points[0].X, panel.Zero).Close();
            builder.Path(area.ToString(), color, filled: true, opacity: 0.12);
        }

        builder.Path(ChartGeometry.Line(points), color);
    }

    /// <summary>
    /// The position of one panel in the plot, and the scale its values are drawn on.
    /// </summary>
    private readonly record struct Panel(ChartScale Scale, double Left, double Top, double Width, double PlotTop, double PlotHeight)
    {
        /// <summary>Gets the vertical position of the baseline.</summary>
        public double Zero => Y(Math.Clamp(0, Scale.Min, Scale.Max));

        /// <summary>Gets the vertical position of a value.</summary>
        public double Y(double value) => PlotTop + PlotHeight * (1 - Scale.Map(value) / 100);
    }
}
