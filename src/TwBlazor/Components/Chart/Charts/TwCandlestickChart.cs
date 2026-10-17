// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using Microsoft.AspNetCore.Components;
using TwBlazor.Charting;
using TwBlazor.Enums;
using TwBlazor.Localization;
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
    /// Defaults to the localized <see cref="TwStrings.Chart_Rising"/> text ("Rising").
    /// </summary>
    [Parameter] public string? RisingLabel { get; set; }

    private string effectiveRisingLabel => RisingLabel ?? localizer[TwStrings.Chart_Rising];

    /// <summary>
    /// Gets or sets the legend label for a period that closed below its open.
    /// Defaults to the localized <see cref="TwStrings.Chart_Falling"/> text ("Falling").
    /// </summary>
    [Parameter] public string? FallingLabel { get; set; }

    private string effectiveFallingLabel => FallingLabel ?? localizer[TwStrings.Chart_Falling];

    /// <summary>
    /// Gets or sets the heading of the period column in the table view.
    /// Defaults to the localized <see cref="TwStrings.Chart_Period"/> text ("Period").
    /// </summary>
    [Parameter] public string? LabelHeader { get; set; }

    private string effectiveLabelHeader => LabelHeader ?? localizer[TwStrings.Chart_Period];

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
                var flat = Math.Abs(item.Open - item.Close) < double.Epsilon ? 0.4 : 0;
                builder.Bar(center, thickness, scale.Map(item.Open) - flat, scale.Map(item.Close) + flat, color, ChartCorner.None);
            }

            builder.BandDatum(center, band.Step, $"{item.Label} ({(rising ? effectiveRisingLabel : effectiveFallingLabel)})",
            [
                new ChartDatumRow(localizer[TwStrings.Chart_Open], FormatValue(item.Open)),
                new ChartDatumRow(localizer[TwStrings.Chart_High], FormatValue(item.High)),
                new ChartDatumRow(localizer[TwStrings.Chart_Low], FormatValue(item.Low)),
                new ChartDatumRow(localizer[TwStrings.Chart_Close], FormatValue(item.Close), color)
            ]);
        }

        builder.Legend(effectiveRisingLabel, ChartColor.Positive);
        builder.Legend(effectiveFallingLabel, ChartColor.Negative);
        builder.Table = new ChartTable([effectiveLabelHeader, localizer[TwStrings.Chart_Open], localizer[TwStrings.Chart_High], localizer[TwStrings.Chart_Low], localizer[TwStrings.Chart_Close]],
            [.. Items.Select(item => (IReadOnlyList<string>)[item.Label, FormatValue(item.Open), FormatValue(item.High), FormatValue(item.Low), FormatValue(item.Close)])]);
    }
}
