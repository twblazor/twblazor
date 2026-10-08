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

    /// <inheritdoc />
    protected override void BuildMarks(ChartSceneBuilder builder, IReadOnlyList<ChartSeriesSlot> series)
    {
        builder.Horizontal = isHorizontal;
        var stacked = Stacking != ChartStacking.None;
        var rows = new double?[Categories.Count][];
        double min = 0, max = 0;

        for (var i = 0; i < Categories.Count; i++)
        {
            var row = series.Select(slot => slot.Value(i)).ToArray();
            if (Stacking == ChartStacking.Percent)
            {
                var total = row.Sum(value => Math.Abs(value ?? 0));
                row = [.. row.Select(value => total == 0 ? value : value / total * 100)];
            }

            rows[i] = row;
            var present = row.Where(value => value.HasValue).Select(value => value!.Value).ToList();
            max = Math.Max(max, stacked ? present.Where(value => value > 0).Sum() : present.DefaultIfEmpty(0).Max());
            min = Math.Min(min, stacked ? present.Where(value => value < 0).Sum() : present.DefaultIfEmpty(0).Min());
        }

        var scale = ChartScale.Nice(min, max);
        var band = new ChartBand(Categories.Count);
        var zero = scale.Map(0);
        builder.ValueAxis(scale, Stacking == ChartStacking.Percent ? value => $"{value:0}%" : FormatValue);
        builder.BandAxis(Categories, band);

        for (var i = 0; i < Categories.Count; i++)
        {
            var center = band.Center(i);
            var row = rows[i];

            if (stacked)
            {
                var thickness = Math.Min(band.Step * 0.6, 12);
                var lastRising = Array.FindLastIndex(row, value => value > 0);
                var lastFalling = Array.FindLastIndex(row, value => value < 0);
                double rising = 0, falling = 0;

                for (var k = 0; k < row.Length; k++)
                {
                    if (row[k] is not { } value || value == 0)
                    {
                        continue;
                    }

                    var from = value > 0 ? rising : falling;
                    var isDataEnd = k == (value > 0 ? lastRising : lastFalling);
                    builder.Bar(center, thickness, scale.Map(from), scale.Map(from + value), series[k].Color, isDataEnd ? null : ChartCorner.None);

                    if (value > 0)
                    {
                        rising += value;
                    }
                    else
                    {
                        falling += value;
                    }
                }
            }
            else
            {
                var group = Math.Min(band.Step * 0.7, 12.0 * series.Count);
                var slot = group / series.Count;

                for (var k = 0; k < row.Length; k++)
                {
                    if (row[k] is not { } value)
                    {
                        continue;
                    }

                    var position = center - group / 2 + slot * (k + 0.5);
                    builder.Bar(position, slot * (series.Count > 1 ? 0.9 : 1), zero, scale.Map(value), series[k].Color);

                    if (ShowValues)
                    {
                        builder.TipLabel(position, scale.Map(value), FormatValue(value), value >= 0);
                    }
                }
            }

            builder.BandDatum(center, band.Step, Categories[i], GetRows(series, i));
        }

        builder.Baseline(zero);
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
