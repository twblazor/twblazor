// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using Microsoft.AspNetCore.Components;
using TwBlazor.Charting;
using TwBlazor.Enums;

namespace TwBlazor.Components;

/// <summary>
/// The shared definition of <see cref="TwLineChart"/> and <see cref="TwAreaChart"/>.
/// </summary>
public abstract class TwLineChartBase : TwCategoryChartBase
{
    /// <summary>
    /// Gets or sets how points are joined: straight (line), smooth (spline) or stepped (step line).
    /// </summary>
    [Parameter] public ChartCurve Curve { get; set; }

    /// <summary>
    /// Gets or sets whether a dot is drawn at every point.
    /// </summary>
    /// <remarks>
    /// If not set, dots are drawn when there are twelve categories or fewer.
    /// </remarks>
    [Parameter] public bool? ShowMarkers { get; set; }

    private protected virtual bool isArea => false;

    private protected virtual bool isStacked => false;

    /// <inheritdoc />
    protected override void BuildMarks(ChartSceneBuilder builder, IReadOnlyList<ChartSeriesSlot> series)
    {
        var count = Categories.Count;
        var band = new ChartBand(count);
        var totals = new double[count];

        if (isStacked)
        {
            foreach (var slot in series)
            {
                for (var i = 0; i < count; i++)
                {
                    totals[i] += Math.Max(0, slot.Value(i) ?? 0);
                }
            }
        }

        var scale = isStacked ? ChartScale.Nice(0, totals.Max()) : ChartScale.Nice(GetValues(series), includeZero: isArea);
        var floor = scale.Map(Math.Clamp(0, scale.Min, scale.Max));
        builder.ValueAxis(scale, FormatValue);
        builder.BandAxis(Categories, band);

        var lower = new double[count];
        var showMarkers = !isStacked && (ShowMarkers ?? count <= 12);

        foreach (var slot in series)
        {
            // A missing value breaks the line, so each unbroken run of points is drawn as its own piece.
            List<List<(double X, double Y)>> runs = [[]];
            List<(double X, double Y)> bottom = [];

            for (var i = 0; i < count; i++)
            {
                var value = isStacked ? Math.Max(0, slot.Value(i) ?? 0) : slot.Value(i);
                if (value is not { } present)
                {
                    runs.Add([]);
                    continue;
                }

                runs[^1].Add(builder.Point(band.Center(i), scale.Map(lower[i] + present)));
                bottom.Add(builder.Point(band.Center(i), isStacked ? scale.Map(lower[i]) : floor));

                if (isStacked)
                {
                    lower[i] += present;
                }
            }

            var offset = 0;
            foreach (var run in runs.Where(run => run.Count > 0))
            {
                if (isArea && run.Count > 1)
                {
                    var under = bottom.Skip(offset).Take(run.Count).Reverse().ToList();
                    var path = new ChartPathBuilder().MoveTo(run[0].X, run[0].Y).Through(run, Curve).LineTo(under[0].X, under[0].Y).Through(under, isStacked ? Curve : ChartCurve.Linear).Close();
                    builder.Path(path.ToString(), slot.Color, filled: true, opacity: isStacked ? 0.85 : 0.12, outline: isStacked);
                }

                if (!isStacked)
                {
                    builder.Path(ChartGeometry.Line(run, Curve), slot.Color);
                }

                offset += run.Count;
            }

            if (showMarkers)
            {
                foreach (var point in runs.SelectMany(run => run))
                {
                    builder.Dot(point.X, point.Y, slot.Color, 8);
                }
            }
        }

        for (var i = 0; i < count; i++)
        {
            builder.BandDatum(band.Center(i), band.Step, Categories[i], GetRows(series, i), ChartHover.Crosshair);
        }
    }
}

/// <summary>
/// A line chart: values joined in category order to show a trend. Set <see cref="TwLineChartBase.Curve"/> for a
/// spline or step line chart.
/// </summary>
public class TwLineChart : TwLineChartBase;

/// <summary>
/// An area chart: a line chart with the space under each line filled, emphasising volume. Set
/// <see cref="Stacked"/> to show how the series add up to a total.
/// </summary>
public class TwAreaChart : TwLineChartBase
{
    /// <summary>
    /// Gets or sets whether the series are stacked on top of each other (a stacked area chart).
    /// </summary>
    /// <remarks>
    /// When stacked, missing and negative values count as zero.
    /// </remarks>
    [Parameter] public bool Stacked { get; set; }

    private protected override bool isArea => true;

    private protected override bool isStacked => Stacked;
}
