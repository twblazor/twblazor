// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using Microsoft.AspNetCore.Components;
using TwBlazor.Charting;

namespace TwBlazor.Components;

/// <summary>
/// A pie chart: slices of a circle sized by each value's share of the whole. Set <see cref="InnerRadius"/> for a
/// donut chart and <see cref="Semicircle"/> for a half donut.
/// </summary>
/// <remarks>
/// Best for a handful of parts. Beyond eight values the smallest are folded into one "Other" slice, and negative
/// or zero values are left out because they have no share to draw.
/// </remarks>
public class TwPieChart : TwValueChartBase
{
    /// <summary>
    /// Gets or sets the size of the hole in the middle, as a fraction of the radius from 0 (a pie) to 0.9.
    /// </summary>
    [Parameter] public double InnerRadius { get; set; }

    /// <summary>
    /// Gets or sets whether the chart is drawn as a half circle.
    /// </summary>
    [Parameter] public bool Semicircle { get; set; }

    /// <summary>
    /// Gets or sets whether slices large enough to hold a label show their share.
    /// </summary>
    [Parameter] public bool ShowPercentages { get; set; } = true;

    /// <summary>
    /// Gets or sets text shown in the hole of a donut, for example the total.
    /// </summary>
    [Parameter] public string? CenterText { get; set; }

    /// <inheritdoc />
    protected override void BuildMarks(ChartSceneBuilder builder)
    {
        var data = GetFoldedData();
        var total = data.Sum(item => item.Value);
        if (total <= 0)
        {
            return;
        }

        builder.Shape = ChartShape.Square;
        const double radius = 48;
        var hole = Math.Clamp(InnerRadius, 0, 0.9);
        var centerY = Semicircle ? 74 : 50;
        var sweep = Semicircle ? 180 : 360;
        double angle = Semicircle ? -90 : 0;

        for (var i = 0; i < data.Count; i++)
        {
            var item = data[i];
            var span = item.Value / total * sweep;
            var color = ChartColor.Series(i);
            var share = FormatShare(item.Value, total);
            var middle = ChartGeometry.Polar(50, centerY, radius * (hole > 0 ? (1 + hole) / 2 : 0.65), angle + span / 2);

            builder.Path(ChartGeometry.Sector(50, centerY, radius * hole, radius, angle, angle + span), color, filled: true, outline: true, title: $"{item.Label}: {FormatValue(item.Value)} ({share})");
            builder.Legend(item.Label, color);

            if (ShowPercentages && item.Value / total >= 0.07)
            {
                builder.Add(new ChartText(middle.X, middle.Y, share, Tone: ChartTextTone.Inverse, Strong: true));
            }

            builder.SectorDatum(50, centerY, radius * hole, radius, angle, angle + span, item.Label, [new ChartDatumRow(ValueHeader, FormatValue(item.Value), color), new ChartDatumRow("Share", share)]);
            angle += span;
        }

        if (hole > 0 && !string.IsNullOrWhiteSpace(CenterText))
        {
            builder.Add(new ChartText(50, centerY, CenterText, Baseline: Semicircle ? ChartBaseline.Bottom : ChartBaseline.Middle, Tone: ChartTextTone.Primary, Strong: true, Large: true));
        }
    }
}

/// <summary>
/// A Nightingale (rose) chart: equal-angle wedges whose radius shows each value, for comparing values that
/// follow a cycle such as months.
/// </summary>
/// <remarks>
/// The area of each wedge, not its radius, is proportional to the value, so large values are not exaggerated.
/// </remarks>
public class TwNightingaleChart : TwValueChartBase
{
    /// <inheritdoc />
    protected override void BuildMarks(ChartSceneBuilder builder)
    {
        var data = GetFoldedData();
        if (data.Count == 0)
        {
            return;
        }

        builder.Shape = ChartShape.Square;
        var max = data.Max(item => item.Value);
        var span = 360.0 / data.Count;

        for (var i = 0; i < data.Count; i++)
        {
            var item = data[i];
            var color = ChartColor.Series(i);
            var radius = 46 * Math.Sqrt(item.Value / max);
            builder.Path(ChartGeometry.Sector(50, 50, 0, radius, span * i, span * (i + 1)), color, filled: true, outline: true, title: $"{item.Label}: {FormatValue(item.Value)}");
            builder.Legend(item.Label, color);
            builder.SectorDatum(50, 50, 0, radius, span * i, span * (i + 1), item.Label, [new ChartDatumRow(ValueHeader, FormatValue(item.Value), color)]);
        }
    }
}

/// <summary>
/// A radial bar chart: bars bent around a circle, one ring per value, each sweeping further the larger its value.
/// </summary>
/// <remarks>
/// Outer rings are longer than inner rings for the same value, so read the angle swept, not the length. For
/// precise comparison prefer <see cref="TwBarChart"/>.
/// </remarks>
public class TwRadialBarChart : TwValueChartBase
{
    private const double maxSweep = 270;

    /// <inheritdoc />
    protected override void BuildMarks(ChartSceneBuilder builder)
    {
        builder.Shape = ChartShape.Square;
        var scale = ChartScale.Nice(0, Data.Max(item => item.Value));
        var color = ChartColor.Series(0);
        var step = 36.0 / Data.Count;
        var thickness = step * 0.7;

        for (var i = 0; i < Data.Count; i++)
        {
            var item = Data[i];
            var outer = 48 - step * i;
            var inner = outer - thickness;
            var sweep = maxSweep * Math.Clamp(scale.Map(item.Value) / 100, 0, 1);

            builder.Path(ChartGeometry.Sector(50, 50, inner, outer, 0, maxSweep), ChartColor.Track, filled: true);

            if (sweep > 0)
            {
                builder.Path(ChartGeometry.Sector(50, 50, inner, outer, 0, sweep), color, filled: true, title: $"{item.Label}: {FormatValue(item.Value)}");
            }

            builder.Add(new ChartText(50, 50 - (inner + outer) / 2, item.Label, ChartAnchor.End, OffsetX: -8));
            builder.SectorDatum(50, 50, inner, outer, 0, maxSweep, item.Label, [new ChartDatumRow(ValueHeader, FormatValue(item.Value), color)], anchorAngle: sweep);
        }
    }
}
