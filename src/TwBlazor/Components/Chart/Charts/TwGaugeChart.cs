// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using Microsoft.AspNetCore.Components;
using TwBlazor.Charting;

namespace TwBlazor.Components;

/// <summary>
/// A circular gauge: an arc filled in proportion to a single value between a minimum and a maximum, with the
/// value shown in the middle.
/// </summary>
public class TwGaugeChart : TwChartBase
{
    private const double startAngle = -135;
    private const double sweep = 270;

    /// <summary>
    /// Gets or sets the value the gauge shows.
    /// </summary>
    [Parameter] public double Value { get; set; }

    /// <summary>
    /// Gets or sets the value at the start of the arc.
    /// </summary>
    [Parameter] public double Min { get; set; }

    /// <summary>
    /// Gets or sets the value at the end of the arc.
    /// </summary>
    [Parameter] public double Max { get; set; } = 100;

    /// <summary>
    /// Gets or sets what the value measures, shown under it.
    /// </summary>
    [Parameter] public string Label { get; set; } = "Value";

    /// <inheritdoc />
    protected internal override void BuildScene(ChartSceneBuilder builder)
    {
        if (Max <= Min)
        {
            return;
        }

        builder.Shape = ChartShape.Square;
        var fraction = Math.Clamp((Value - Min) / (Max - Min), 0, 1);
        var color = ChartColor.Series(0);
        var start = ChartGeometry.Polar(50, 50, 42, startAngle);
        var end = ChartGeometry.Polar(50, 50, 42, startAngle + sweep);

        builder.Path(ChartGeometry.Sector(50, 50, 36, 48, startAngle, startAngle + sweep), ChartColor.Track, filled: true);

        if (fraction > 0)
        {
            builder.Path(ChartGeometry.Sector(50, 50, 36, 48, startAngle, startAngle + sweep * fraction), color, filled: true);
        }

        builder.Add(new ChartText(50, 50, FormatValue(Value), Tone: ChartTextTone.Primary, Strong: true, Large: true));
        builder.Add(new ChartText(50, 50, Label, Baseline: ChartBaseline.Top, OffsetY: 22));
        builder.Add(new ChartText(start.X, start.Y, FormatValue(Min), Baseline: ChartBaseline.Top, OffsetY: 12));
        builder.Add(new ChartText(end.X, end.Y, FormatValue(Max), Baseline: ChartBaseline.Top, OffsetY: 12));
        builder.Datum(50, 50, 40, 30, Label, [new ChartDatumRow(Label, FormatValue(Value), color), new ChartDatumRow("Range", $"{FormatValue(Min)} to {FormatValue(Max)}")], ChartHover.None);
        builder.Table = new ChartTable(["Measure", "Value", "Minimum", "Maximum"], [[Label, FormatValue(Value), FormatValue(Min), FormatValue(Max)]]);
    }
}
