// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using Microsoft.AspNetCore.Components;
using TwBlazor.Charting;

namespace TwBlazor.Components;

/// <summary>
/// A funnel chart: stacked stages whose width shows how much remains at each step of a process, such as a sales
/// pipeline. Set <see cref="Inverted"/> to draw it as a pyramid.
/// </summary>
/// <remarks>
/// The stages are an ordered sequence, so they share one hue that fades from the first stage to the last.
/// </remarks>
public class TwFunnelChart : TwValueChartBase
{
    private const double maxWidth = 70;

    /// <summary>
    /// Gets or sets whether the stages are drawn bottom to top, turning the funnel into a pyramid.
    /// </summary>
    [Parameter] public bool Inverted { get; set; }

    /// <summary>
    /// Gets or sets the label for a stage's share of the first stage in tooltips.
    /// </summary>
    [Parameter] public string ShareLabel { get; set; } = "Of first stage";

    /// <inheritdoc />
    protected override void BuildMarks(ChartSceneBuilder builder)
    {
        var max = Data.Max(item => item.Value);
        if (max <= 0)
        {
            return;
        }

        var band = new ChartBand(Data.Count);
        var color = ChartColor.Series(0);
        double Width(int index) => maxWidth * Math.Max(0, Data[index].Value) / max;

        for (var i = 0; i < Data.Count; i++)
        {
            var row = Inverted ? Data.Count - 1 - i : i;
            var top = band.Step * row + 0.6;
            var bottom = band.Step * (row + 1) - 0.6;
            var width = Width(i);
            var next = i < Data.Count - 1 ? Width(i + 1) : width;
            var (nearWidth, farWidth) = Inverted ? (next, width) : (width, next);

            var path = new ChartPathBuilder()
                .MoveTo(50 - nearWidth / 2, top).LineTo(50 + nearWidth / 2, top)
                .LineTo(50 + farWidth / 2, bottom).LineTo(50 - farWidth / 2, bottom).Close();

            builder.Path(path.ToString(), color, filled: true, opacity: Data.Count == 1 ? 1 : 1 - 0.55 * i / (Data.Count - 1));
            builder.Add(new ChartText(50 + Math.Max(nearWidth, farWidth) / 2, band.Center(row), FormatValue(Data[i].Value), ChartAnchor.Start, Tone: ChartTextTone.Primary, OffsetX: 8));
            builder.Datum(50, band.Center(row), 100, band.Step, Data[i].Label,
            [
                new ChartDatumRow(ValueHeader, FormatValue(Data[i].Value), color),
                new ChartDatumRow(ShareLabel, FormatShare(Data[i].Value, Data[0].Value))
            ]);
        }

        builder.YAxis = new ChartAxis([.. Data.Select((item, index) => new ChartTick(band.Center(Inverted ? Data.Count - 1 - index : index), item.Label))]);
    }
}
