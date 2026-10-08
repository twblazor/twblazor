// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using Microsoft.AspNetCore.Components;
using TwBlazor.Charting;
using TwBlazor.Enums;

namespace TwBlazor.Components;

/// <summary>
/// The shared definition of <see cref="TwBarChart"/> and <see cref="TwColumnChart"/>, which differ only in direction.
/// </summary>
public abstract class TwBarChartBase : TwCategoryChartBase
{
    /// <summary>
    /// Gets or sets how multiple series share a category: side by side, stacked, or stacked to 100%.
    /// </summary>
    /// <remarks>
    /// Default is <see cref="ChartStacking.None"/>, which draws a grouped chart. Negative values stack downwards
    /// from the baseline, which makes a diverging chart.
    /// </remarks>
    [Parameter] public ChartStacking Stacking { get; set; }

    /// <summary>
    /// Gets or sets whether each bar is labelled with its value. Ignored when <see cref="Stacking"/> is on.
    /// </summary>
    /// <remarks>
    /// Default is <see langword="false"/>. Best kept for charts with few bars; the axis and tooltips carry the
    /// values otherwise.
    /// </remarks>
    [Parameter] public bool ShowValues { get; set; }

    private protected abstract bool isHorizontal { get; }

    private bool isStacked => Stacking != ChartStacking.None;

    /// <inheritdoc />
    protected override void BuildMarks(ChartSceneBuilder builder, IReadOnlyList<ChartSeriesSlot> series)
    {
        builder.Horizontal = isHorizontal;
        var rows = Enumerable.Range(0, Categories.Count).Select(category => GetRow(series, category)).ToArray();
        var scale = ChartScale.Nice(rows.Min(row => GetExtent(row, rising: false)), rows.Max(row => GetExtent(row, rising: true)));
        var band = new ChartBand(Categories.Count);
        builder.ValueAxis(scale, Stacking == ChartStacking.Percent ? value => $"{value:0}%" : FormatValue);
        builder.BandAxis(Categories, band);

        for (var i = 0; i < Categories.Count; i++)
        {
            if (isStacked)
            {
                BuildStack(builder, scale, band.Center(i), Math.Min(band.Step * 0.6, 12), series, rows[i]);
            }
            else
            {
                BuildGroup(builder, scale, band.Center(i), band.Step, series, rows[i]);
            }

            builder.BandDatum(band.Center(i), band.Step, Categories[i], GetRows(series, i));
        }

        builder.Baseline(scale.Map(0));
    }

    /// <summary>
    /// Gets the value each visible series plots for a category: its own value, or its share when stacking to 100%.
    /// </summary>
    private double?[] GetRow(IReadOnlyList<ChartSeriesSlot> series, int category)
    {
        var row = series.Select(slot => slot.Value(category)).ToArray();
        var total = row.Sum(value => Math.Abs(value ?? 0));
        return Stacking == ChartStacking.Percent && total > 0 ? [.. row.Select(value => value / total * 100)] : row;
    }

    /// <summary>
    /// Gets how far a category reaches above (or below) the baseline: the sum of its values when stacked, otherwise
    /// its largest (or smallest) value.
    /// </summary>
    private double GetExtent(double?[] row, bool rising)
    {
        var values = row.Select(value => value ?? 0).Where(value => rising ? value > 0 : value < 0).ToList();
        if (values.Count == 0)
        {
            return 0;
        }

        if (isStacked)
        {
            return values.Sum();
        }

        return rising ? values.Max() : values.Min();
    }

    private static void BuildStack(ChartSceneBuilder builder, ChartScale scale, double center, double thickness, IReadOnlyList<ChartSeriesSlot> series, double?[] row)
    {
        var lastRising = Array.FindLastIndex(row, value => value > 0);
        var lastFalling = Array.FindLastIndex(row, value => value < 0);
        double rising = 0, falling = 0;

        for (var k = 0; k < row.Length; k++)
        {
            var value = row[k] ?? 0;
            if (value is not (> 0 or < 0))
            {
                continue;
            }

            var isRising = value > 0;
            var from = isRising ? rising : falling;
            var isDataEnd = k == (isRising ? lastRising : lastFalling);
            builder.Bar(center, thickness, scale.Map(from), scale.Map(from + value), series[k].Color, isDataEnd ? null : ChartCorner.None);
            rising += isRising ? value : 0;
            falling += isRising ? 0 : value;
        }
    }

    private void BuildGroup(ChartSceneBuilder builder, ChartScale scale, double center, double step, IReadOnlyList<ChartSeriesSlot> series, double?[] row)
    {
        var group = Math.Min(step * 0.7, 12.0 * series.Count);
        var slot = group / series.Count;

        for (var k = 0; k < row.Length; k++)
        {
            if (row[k] is not { } value)
            {
                continue;
            }

            var position = center - group / 2 + slot * (k + 0.5);
            builder.Bar(position, slot * (series.Count > 1 ? 0.9 : 1), scale.Map(0), scale.Map(value), series[k].Color);

            if (ShowValues)
            {
                builder.TipLabel(position, scale.Map(value), FormatValue(value), value >= 0);
            }
        }
    }
}

/// <summary>
/// A bar chart: horizontal bars whose length shows the value of each category. With several series it becomes a
/// grouped, stacked or 100% stacked bar chart, see <see cref="TwBarChartBase.Stacking"/>.
/// </summary>
/// <remarks>
/// Prefer bars over columns when category labels are long or there are many categories.
/// </remarks>
public class TwBarChart : TwBarChartBase
{
    private protected override bool isHorizontal => true;
}

/// <summary>
/// A column chart: vertical bars whose height shows the value of each category. With several series it becomes a
/// grouped, stacked or 100% stacked column chart, see <see cref="TwBarChartBase.Stacking"/>.
/// </summary>
public class TwColumnChart : TwBarChartBase
{
    private protected override bool isHorizontal => false;
}
