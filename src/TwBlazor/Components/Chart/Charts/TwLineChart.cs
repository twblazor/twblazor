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
    private const int maxAutoMarkers = 12;

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

    private bool hasMarkers => !isStacked && (ShowMarkers ?? Categories.Count <= maxAutoMarkers);

    /// <inheritdoc />
    protected override void BuildMarks(ChartSceneBuilder builder, IReadOnlyList<ChartSeriesSlot> series)
    {
        var band = new ChartBand(Categories.Count);
        var scale = isStacked
            ? ChartScale.Nice(0, Enumerable.Range(0, Categories.Count).Max(category => GetStackTotal(series, category)))
            : ChartScale.Nice(GetValues(series), includeZero: isArea);

        builder.ValueAxis(scale, FormatValue);
        builder.BandAxis(Categories, band);

        var lower = new double[Categories.Count];
        foreach (var slot in series)
        {
            foreach (var run in GetRuns(builder, scale, band, slot, lower))
            {
                BuildRun(builder, slot.Color, run);
            }
        }

        for (var i = 0; i < Categories.Count; i++)
        {
            builder.BandDatum(band.Center(i), band.Step, Categories[i], GetRows(series, i), ChartHover.Crosshair);
        }
    }

    private static double GetStackValue(ChartSeriesSlot slot, int category) => Math.Max(0, slot.Value(category) ?? 0);

    private static double GetStackTotal(IReadOnlyList<ChartSeriesSlot> series, int category)
    {
        double total = 0;
        foreach (var slot in series)
        {
            total += GetStackValue(slot, category);
        }

        return total;
    }

    /// <summary>
    /// Splits a series into unbroken runs of points. A missing value breaks the line, so each run is drawn as its own piece.
    /// </summary>
    /// <param name="builder">The builder, used to position points.</param>
    /// <param name="scale">The value scale.</param>
    /// <param name="band">The category slots.</param>
    /// <param name="slot">The series.</param>
    /// <param name="lower">The running total under the series for each category when stacked. Updated in place.</param>
    private List<LineRun> GetRuns(ChartSceneBuilder builder, ChartScale scale, ChartBand band, ChartSeriesSlot slot, double[] lower)
    {
        var floor = scale.Map(Math.Clamp(0, scale.Min, scale.Max));
        List<LineRun> runs = [new()];

        for (var i = 0; i < Categories.Count; i++)
        {
            var value = isStacked ? GetStackValue(slot, i) : slot.Value(i);
            if (value is not { } present)
            {
                runs.Add(new LineRun());
                continue;
            }

            var from = isStacked ? lower[i] : 0;
            runs[^1].Top.Add(builder.Point(band.Center(i), scale.Map(from + present)));
            runs[^1].Bottom.Add(builder.Point(band.Center(i), isStacked ? scale.Map(from) : floor));
            lower[i] = from + present;
        }

        return [.. runs.Where(run => run.Top.Count > 0)];
    }

    private void BuildRun(ChartSceneBuilder builder, ChartColor color, LineRun run)
    {
        if (isArea && run.Top.Count > 1)
        {
            var under = Enumerable.Reverse(run.Bottom).ToList();
            var path = new ChartPathBuilder()
                .MoveTo(run.Top[0].X, run.Top[0].Y).Through(run.Top, Curve)
                .LineTo(under[0].X, under[0].Y).Through(under, isStacked ? Curve : ChartCurve.Linear)
                .Close();
            builder.Path(path.ToString(), color, filled: true, opacity: isStacked ? 0.85 : 0.12, outline: isStacked);
        }

        if (!isStacked)
        {
            builder.Path(ChartGeometry.Line(run.Top, Curve), color);
        }

        if (hasMarkers)
        {
            run.Top.ForEach(point => builder.Dot(point.X, point.Y, color, 8));
        }
    }

    /// <summary>
    /// An unbroken run of points along a line, with the points directly beneath them that close an area.
    /// </summary>
    private sealed class LineRun
    {
        public List<(double X, double Y)> Top { get; } = [];

        public List<(double X, double Y)> Bottom { get; } = [];
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
