// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using Microsoft.AspNetCore.Components;
using TwBlazor.Charting;
using TwBlazor.Enums;

namespace TwBlazor.Components;

/// <summary>
/// An icon chart: values shown with icons, either as a row of repeated icons per value (a pictogram) or as one
/// icon per value sized by it. Icons make a chart quick to relate to, at the cost of precision.
/// </summary>
/// <remarks>
/// The icons are decorative. The values are stated by the data points, tooltips and table view.
/// </remarks>
public class TwIconChart : TwValueChartBase
{
    private const int targetIcons = 10;

    /// <summary>
    /// Gets or sets the icon to draw.
    /// </summary>
    [Parameter] public Icon Icon { get; set; } = Icon.Person;

    /// <summary>
    /// Gets or sets whether values are shown by repeating the icon or by scaling it.
    /// </summary>
    [Parameter] public IconChartMode Mode { get; set; }

    /// <summary>
    /// Gets or sets how much one icon stands for in <see cref="IconChartMode.Count"/> mode.
    /// </summary>
    /// <remarks>
    /// If not set, a round amount is chosen so the largest value uses about ten icons.
    /// </remarks>
    [Parameter] public double? UnitValue { get; set; }

    /// <inheritdoc />
    protected override void BuildMarks(ChartSceneBuilder builder)
    {
        var max = Data.Max(item => item.Value);
        if (max <= 0)
        {
            return;
        }

        var color = ChartColor.Series(0);
        var band = new ChartBand(Data.Count);
        var unit = UnitValue > 0 ? UnitValue.Value : ChartScale.NiceStep(max / targetIcons);
        var columns = Math.Max(1, (int)Math.Ceiling(max / unit));
        builder.Horizontal = Mode == IconChartMode.Count;
        builder.BandAxis([.. Data.Select(item => item.Label)], band);

        for (var i = 0; i < Data.Count; i++)
        {
            var value = Math.Max(0, Data[i].Value);

            if (Mode == IconChartMode.Size)
            {
                builder.Glyph(band.Center(i), 50, Icon, color, 16 + 56 * Math.Sqrt(value / max));
            }
            else
            {
                for (var k = 0; k < (int)Math.Round(value / unit); k++)
                {
                    builder.Glyph((k + 0.5) * 100 / columns, band.Center(i), Icon, color);
                }
            }

            builder.BandDatum(band.Center(i), band.Step, Data[i].Label, [new ChartDatumRow(ValueHeader, FormatValue(Data[i].Value), color)]);
        }

        if (Mode == IconChartMode.Count)
        {
            builder.Legend($"1 icon = {FormatValue(unit)}", color);
        }
    }
}
