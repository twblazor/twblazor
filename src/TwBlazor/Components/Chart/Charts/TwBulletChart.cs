// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using Microsoft.AspNetCore.Components;
using TwBlazor.Charting;
using TwBlazor.Models;

namespace TwBlazor.Components;

/// <summary>
/// A bullet chart: for each measure, a bar showing the actual value, a marker showing the target and optional
/// shaded bands for qualitative ranges such as poor, fair and good.
/// </summary>
/// <remarks>
/// Each measure has its own scale, so measures in different units can share one chart.
/// </remarks>
public class TwBulletChart : TwChartBase
{
    /// <summary>
    /// Gets or sets the measures to plot.
    /// </summary>
    [Parameter] public IReadOnlyList<ChartBullet> Items { get; set; } = [];

    /// <summary>
    /// Gets or sets the heading of the label column in the table view.
    /// </summary>
    [Parameter] public string LabelHeader { get; set; } = "Measure";

    /// <summary>
    /// Gets or sets the label for the actual value.
    /// </summary>
    [Parameter] public string ValueLabel { get; set; } = "Actual";

    /// <summary>
    /// Gets or sets the label for the target value.
    /// </summary>
    [Parameter] public string TargetLabel { get; set; } = "Target";

    /// <inheritdoc />
    protected internal override void BuildScene(ChartSceneBuilder builder)
    {
        if (Items.Count == 0)
        {
            return;
        }

        builder.Horizontal = true;
        var band = new ChartBand(Items.Count);
        builder.BandAxis([.. Items.Select(item => item.Label)], band);

        for (var i = 0; i < Items.Count; i++)
        {
            var item = Items[i];
            var center = band.Center(i);
            var thickness = Math.Min(band.Step * 0.6, 14);
            var limits = (item.Ranges ?? []).Where(limit => limit > 0).OrderByDescending(limit => limit).ToList();
            var scale = ChartScale.Nice(0, Math.Max(Math.Max(item.Value, item.Target), limits.FirstOrDefault()));

            if (limits.Count == 0)
            {
                limits.Add(scale.Max);
            }

            for (var k = 0; k < limits.Count; k++)
            {
                var opacity = limits.Count == 1 ? 1 : 0.45 + 0.55 * k / (limits.Count - 1);
                builder.Add(new ChartRect(0, center - thickness / 2, scale.Map(limits[k]), thickness, ChartColor.Track, Opacity: opacity, Gap: false));
            }

            builder.Bar(center, thickness * 0.36, 0, scale.Map(item.Value), ChartColor.Series(0));
            builder.Bar(center, thickness * 0.8, scale.Map(item.Target) - 0.3, scale.Map(item.Target) + 0.3, ChartColor.Ink, ChartCorner.None, gap: false);
            builder.BandDatum(center, band.Step, item.Label,
            [
                new ChartDatumRow(ValueLabel, FormatValue(item.Value), ChartColor.Series(0)),
                new ChartDatumRow(TargetLabel, FormatValue(item.Target), ChartColor.Ink)
            ]);
        }

        builder.Legend(ValueLabel, ChartColor.Series(0));
        builder.Legend(TargetLabel, ChartColor.Ink);
        builder.Table = new ChartTable([LabelHeader, ValueLabel, TargetLabel],
            [.. Items.Select(item => (IReadOnlyList<string>)[item.Label, FormatValue(item.Value), FormatValue(item.Target)])]);
    }
}
