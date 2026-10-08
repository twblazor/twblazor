// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using Microsoft.AspNetCore.Components;
using TwBlazor.Charting;

namespace TwBlazor.Components;

/// <summary>
/// The shared definition of <see cref="TwHeatmapChart"/> and <see cref="TwMatrixChart"/>: a grid with one row per
/// series and one column per category.
/// </summary>
public abstract class TwGridChartBase : TwCategoryChartBase
{
    /// <inheritdoc />
    protected override bool hasSeriesLegend => false;

    /// <summary>
    /// Adds the mark for one cell.
    /// </summary>
    /// <param name="builder">The builder that collects the scene.</param>
    /// <param name="x">The left edge of the cell.</param>
    /// <param name="y">The top edge of the cell.</param>
    /// <param name="width">The width of the cell.</param>
    /// <param name="height">The height of the cell.</param>
    /// <param name="value">The value of the cell.</param>
    /// <param name="intensity">Where the value sits between the lowest (0) and highest (1) value in the grid.</param>
    private protected abstract void BuildCell(ChartSceneBuilder builder, double x, double y, double width, double height, double value, double intensity);

    /// <inheritdoc />
    protected override void BuildMarks(ChartSceneBuilder builder, IReadOnlyList<ChartSeriesSlot> series)
    {
        var columns = new ChartBand(Categories.Count);
        var rows = new ChartBand(series.Count);
        var values = GetValues(series).DefaultIfEmpty(0).ToList();
        var (min, max) = (values.Min(), values.Max());

        builder.XAxis = new ChartAxis([.. Categories.Select((category, index) => new ChartTick(columns.Center(index), category))]);
        builder.YAxis = new ChartAxis([.. series.Select((slot, index) => new ChartTick(rows.Center(index), slot.Series.Name))]);

        for (var row = 0; row < series.Count; row++)
        {
            for (var column = 0; column < Categories.Count; column++)
            {
                if (series[row].Value(column) is not { } value)
                {
                    continue;
                }

                var intensity = max > min ? (value - min) / (max - min) : 1;
                BuildCell(builder, columns.Step * column, rows.Step * row, columns.Step, rows.Step, value, intensity);
                builder.Datum(columns.Center(column), rows.Center(row), columns.Step, rows.Step, $"{series[row].Series.Name}, {Categories[column]}",
                    [new ChartDatumRow(ValueLabel, FormatValue(value))]);
            }
        }
    }

    /// <summary>
    /// Gets or sets the label for a cell's value in tooltips.
    /// </summary>
    [Parameter] public string ValueLabel { get; set; } = "Value";
}

/// <summary>
/// A heatmap: a grid of cells whose color intensity shows the value where a series (row) meets a category (column).
/// </summary>
/// <remarks>
/// Intensity uses a single hue, so darker always means more. Exact values are in the tooltips and the table view,
/// or on the cells with <see cref="ShowValues"/>.
/// </remarks>
public class TwHeatmapChart : TwGridChartBase
{
    /// <summary>
    /// Gets or sets whether each cell is labelled with its value.
    /// </summary>
    [Parameter] public bool ShowValues { get; set; }

    private protected override void BuildCell(ChartSceneBuilder builder, double x, double y, double width, double height, double value, double intensity)
    {
        builder.Rect(x, y, width, height, ChartColor.Sequential, ChartCorner.All, 0.1 + 0.9 * intensity);

        if (ShowValues)
        {
            builder.Text(x + width / 2, y + height / 2, FormatValue(value), tone: intensity > 0.5 ? ChartTextTone.Inverse : ChartTextTone.Primary);
        }
    }
}

/// <summary>
/// A matrix chart: a grid with a dot where a series (row) meets a category (column), sized by the value, for
/// spotting patterns across two categorical dimensions.
/// </summary>
public class TwMatrixChart : TwGridChartBase
{
    private protected override void BuildCell(ChartSceneBuilder builder, double x, double y, double width, double height, double value, double intensity)
    {
        builder.Line(x + width / 2, y, x + width / 2, y + height, ChartColor.Grid);
        builder.Line(x, y + height / 2, x + width, y + height / 2, ChartColor.Grid);
        builder.Dot(x + width / 2, y + height / 2, ChartColor.Series(0), 8 + 24 * Math.Sqrt(intensity));
    }
}
