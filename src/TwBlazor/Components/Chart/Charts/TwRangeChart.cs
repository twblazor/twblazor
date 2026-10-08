// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using Microsoft.AspNetCore.Components;
using TwBlazor.Charting;
using TwBlazor.Models;

namespace TwBlazor.Components;

/// <summary>
/// A range chart: a floating bar from a low value to a high value for each category, such as a temperature
/// range or a salary band.
/// </summary>
public class TwRangeChart : TwChartBase
{
    /// <summary>
    /// Gets or sets the ranges to plot.
    /// </summary>
    [Parameter] public IReadOnlyList<ChartRange> Ranges { get; set; } = [];

    /// <summary>
    /// Gets or sets whether categories run left to right with values bottom to top, instead of the default
    /// rows with values left to right.
    /// </summary>
    [Parameter] public bool Vertical { get; set; }

    /// <summary>
    /// Gets or sets the heading of the label column in the table view.
    /// </summary>
    [Parameter] public string LabelHeader { get; set; } = "Category";

    /// <summary>
    /// Gets or sets the label for the low end of a range.
    /// </summary>
    [Parameter] public string LowLabel { get; set; } = "Low";

    /// <summary>
    /// Gets or sets the label for the high end of a range.
    /// </summary>
    [Parameter] public string HighLabel { get; set; } = "High";

    /// <inheritdoc />
    protected internal override void BuildScene(ChartSceneBuilder builder)
    {
        if (Ranges.Count == 0)
        {
            return;
        }

        builder.Horizontal = !Vertical;
        var scale = ChartScale.Nice(Ranges.Min(range => Math.Min(range.Low, range.High)), Ranges.Max(range => Math.Max(range.Low, range.High)), includeZero: false);
        var band = new ChartBand(Ranges.Count);
        builder.ValueAxis(scale, FormatValue);
        builder.BandAxis([.. Ranges.Select(range => range.Label)], band);

        for (var i = 0; i < Ranges.Count; i++)
        {
            var range = Ranges[i];
            builder.Bar(band.Center(i), Math.Min(band.Step * 0.45, 8), scale.Map(range.Low), scale.Map(range.High), ChartColor.Series(0), ChartCorner.All);
            builder.BandDatum(band.Center(i), band.Step, range.Label,
            [
                new ChartDatumRow(LowLabel, FormatValue(range.Low)),
                new ChartDatumRow(HighLabel, FormatValue(range.High))
            ]);
        }

        builder.Table = new ChartTable([LabelHeader, LowLabel, HighLabel],
            [.. Ranges.Select(range => (IReadOnlyList<string>)[range.Label, FormatValue(range.Low), FormatValue(range.High)])]);
    }
}
