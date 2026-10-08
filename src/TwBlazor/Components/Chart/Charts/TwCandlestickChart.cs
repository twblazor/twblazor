// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using Microsoft.AspNetCore.Components;
using TwBlazor.Charting;
using TwBlazor.Enums;
using TwBlazor.Models;

namespace TwBlazor.Components;

/// <summary>
/// A candlestick chart: for each period, a body between the opening and closing value and a wick reaching the
/// high and the low. Set <see cref="Kind"/> to <see cref="CandlestickStyle.Ohlc"/> for an OHLC chart.
/// </summary>
/// <remarks>
/// Rising and falling periods differ by color, and the legend and tooltips name the direction, so it never relies
/// on color alone.
/// </remarks>
public class TwCandlestickChart : TwChartBase
{
    /// <summary>
    /// Gets or sets the periods to plot, in order.
    /// </summary>
    [Parameter] public IReadOnlyList<ChartOhlc> Items { get; set; } = [];

    /// <summary>
    /// Gets or sets whether each period is drawn as a candle or as open and close ticks.
    /// </summary>
    [Parameter] public CandlestickStyle Kind { get; set; }

    /// <summary>
    /// Gets or sets the legend label for a period that closed at or above its open.
    /// </summary>
    [Parameter] public string RisingLabel { get; set; } = "Rising";

    /// <summary>
    /// Gets or sets the legend label for a period that closed below its open.
    /// </summary>
    [Parameter] public string FallingLabel { get; set; } = "Falling";

    /// <summary>
    /// Gets or sets the heading of the period column in the table view.
    /// </summary>
    [Parameter] public string LabelHeader { get; set; } = "Period";

    /// <inheritdoc />
    protected internal override void BuildScene(ChartSceneBuilder builder)
    {
        if (Items.Count == 0)
        {
            return;
        }

        var scale = ChartScale.Nice(Items.Min(item => item.Low), Items.Max(item => item.High), includeZero: false);
        var band = new ChartBand(Items.Count);
        var thickness = Math.Min(band.Step * 0.6, 8);
        builder.ValueAxis(scale, FormatValue);
        builder.BandAxis([.. Items.Select(item => item.Label)], band);

        for (var i = 0; i < Items.Count; i++)
        {
            var item = Items[i];
            var center = band.Center(i);
            var rising = item.Close >= item.Open;
            var color = rising ? ChartColor.Positive : ChartColor.Negative;
            builder.Segment(center, scale.Map(item.Low), center, scale.Map(item.High), color);

            if (Kind == CandlestickStyle.Ohlc)
            {
                builder.Segment(center - thickness / 2, scale.Map(item.Open), center, scale.Map(item.Open), color);
                builder.Segment(center, scale.Map(item.Close), center + thickness / 2, scale.Map(item.Close), color);
            }
            else
            {
                // A period that opens and closes level still needs a visible body.
                var flat = item.Open == item.Close ? 0.4 : 0;
                builder.Bar(center, thickness, scale.Map(item.Open) - flat, scale.Map(item.Close) + flat, color, ChartCorner.None);
            }

            builder.BandDatum(center, band.Step, $"{item.Label} ({(rising ? RisingLabel : FallingLabel)})",
            [
                new ChartDatumRow("Open", FormatValue(item.Open)),
                new ChartDatumRow("High", FormatValue(item.High)),
                new ChartDatumRow("Low", FormatValue(item.Low)),
                new ChartDatumRow("Close", FormatValue(item.Close), color)
            ]);
        }

        builder.Legend(RisingLabel, ChartColor.Positive);
        builder.Legend(FallingLabel, ChartColor.Negative);
        builder.Table = new ChartTable([LabelHeader, "Open", "High", "Low", "Close"],
            [.. Items.Select(item => (IReadOnlyList<string>)[item.Label, FormatValue(item.Open), FormatValue(item.High), FormatValue(item.Low), FormatValue(item.Close)])]);
    }
}
