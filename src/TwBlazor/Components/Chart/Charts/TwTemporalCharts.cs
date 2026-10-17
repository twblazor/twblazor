// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using Microsoft.AspNetCore.Components;
using TwBlazor.Charting;
using TwBlazor.Enums;
using TwBlazor.Localization;

namespace TwBlazor.Components;

/// <summary>
/// A stream graph: a stacked area chart whose layers flow around a central line instead of rising from a
/// baseline, emphasising how the make-up of a total shifts over time.
/// </summary>
/// <remarks>
/// The thickness of a layer is its value; its position carries no meaning. Missing and negative values count as zero.
/// </remarks>
public class TwStreamGraphChart : TwCategoryChartBase
{
    // The thickest part of the stream stops short of the top and bottom of the plot.
    private const double maxThickness = 92;

    /// <inheritdoc />
    protected override void BuildMarks(ChartSceneBuilder builder, IReadOnlyList<ChartSeriesSlot> series)
    {
        var band = new ChartBand(Categories.Count);
        var totals = Enumerable.Range(0, Categories.Count).Select(category => series.Sum(slot => GetLayerValue(slot, category))).ToList();
        var peak = totals.Max();
        if (peak <= 0)
        {
            return;
        }

        builder.BandAxis(Categories, band);

        // Each category's stack is centered on the middle of the plot, then the layers are laid on from the top down.
        var edges = totals.Select(total => 50 - total / peak * maxThickness / 2).ToList();
        foreach (var slot in series)
        {
            List<(double X, double Y)> upper = [];
            List<(double X, double Y)> lower = [];
            for (var category = 0; category < Categories.Count; category++)
            {
                upper.Add((band.Center(category), edges[category]));
                edges[category] += GetLayerValue(slot, category) / peak * maxThickness;
                lower.Add((band.Center(category), edges[category]));
            }

            ChartRibbon.Add(builder, slot.Color, upper, lower, band.Step);
        }

        for (var category = 0; category < Categories.Count; category++)
        {
            builder.BandDatum(band.Center(category), band.Step, Categories[category], GetRows(series, category), ChartHover.Crosshair);
        }
    }

    private static double GetLayerValue(ChartSeriesSlot slot, int category) => slot.Value(category) is { } value && double.IsFinite(value) ? Math.Max(0, value) : 0;
}

/// <summary>
/// A bump area chart: a band for each series that is as thick as its value and moves up or down as its rank
/// among the series changes, showing both who leads and by how much.
/// </summary>
/// <remarks>
/// At every category the bands are stacked from the largest value at the top to the smallest at the bottom. A
/// band breaks where its series has no value. Negative values count as zero.
/// </remarks>
public class TwBumpAreaChart : TwCategoryChartBase
{
    // The share of the plot's height left as gaps between the bands.
    private const double spacing = 12;

    /// <inheritdoc />
    protected override void BuildMarks(ChartSceneBuilder builder, IReadOnlyList<ChartSeriesSlot> series)
    {
        var band = new ChartBand(Categories.Count);
        var ranked = Enumerable.Range(0, Categories.Count)
            .Select(category => series.Where(slot => slot.Value(category) is { } value && double.IsFinite(value)).OrderByDescending(slot => slot.Value(category)).ToList()).ToList();
        var peak = ranked.Select((slots, category) => slots.Sum(slot => Math.Max(0, slot.Value(category)!.Value))).DefaultIfEmpty(0).Max();
        if (peak <= 0)
        {
            return;
        }

        builder.BandAxis(Categories, band);
        var gap = series.Count > 1 ? spacing / (series.Count - 1) : 0;
        var unit = (100 - spacing) / peak;

        var edges = GetEdges(series, ranked, gap, unit);
        foreach (var slot in series)
        {
            BuildBand(builder, slot, edges[slot.Slot], band);
        }

        // Category by category, top band first, so the arrow keys read down each column in rank order.
        for (var category = 0; category < Categories.Count; category++)
        {
            foreach (var slot in ranked[category])
            {
                var (top, bottom, rank) = edges[slot.Slot][category]!.Value;
                builder.Datum(band.Center(category), (top + bottom) / 2, band.Step, bottom - top, $"{slot.Series.Name}, {Categories[category]}",
                    [new ChartDatumRow(localizer[TwStrings.Chart_Rank], rank.ToString("#,0")), new ChartDatumRow(localizer[TwStrings.Chart_Value], FormatValue(slot.Value(category)!.Value), slot.Color)], ChartHover.None);
            }
        }
    }

    /// <summary>
    /// Works out the top and bottom of every series' band at every category, or nothing where the series has no value.
    /// </summary>
    /// <param name="series">The visible series.</param>
    /// <param name="ranked">For each category, the series that have a value there, largest first.</param>
    /// <param name="gap">The gap between neighboring bands.</param>
    /// <param name="unit">The thickness of one unit of value.</param>
    /// <returns>The edges and rank of each series at each category, keyed by the slot of the series.</returns>
    private Dictionary<int, (double Top, double Bottom, int Rank)?[]> GetEdges(IReadOnlyList<ChartSeriesSlot> series, List<List<ChartSeriesSlot>> ranked, double gap, double unit)
    {
        var edges = series.ToDictionary(slot => slot.Slot, _ => new (double Top, double Bottom, int Rank)?[Categories.Count]);
        for (var category = 0; category < Categories.Count; category++)
        {
            var slots = ranked[category];
            var top = (100 - slots.Sum(slot => Math.Max(0, slot.Value(category)!.Value)) * unit - gap * (slots.Count - 1)) / 2;
            for (var rank = 0; rank < slots.Count; rank++)
            {
                var thickness = Math.Max(0, slots[rank].Value(category)!.Value) * unit;
                edges[slots[rank].Slot][category] = (top, top + thickness, rank + 1);
                top += thickness + gap;
            }
        }

        return edges;
    }

    /// <summary>
    /// Adds the band of one series, as a separate piece for each unbroken run of categories it has a value at.
    /// </summary>
    private static void BuildBand(ChartSceneBuilder builder, ChartSeriesSlot slot, (double Top, double Bottom, int Rank)?[] edges, ChartBand band)
    {
        List<(double X, double Y)> upper = [];
        List<(double X, double Y)> lower = [];
        for (var category = 0; category <= edges.Length; category++)
        {
            if (category < edges.Length && edges[category] is { } edge)
            {
                upper.Add((band.Center(category), edge.Top));
                lower.Add((band.Center(category), edge.Bottom));
                continue;
            }

            ChartRibbon.Add(builder, slot.Color, upper, lower, band.Step);
            upper.Clear();
            lower.Clear();
        }
    }
}

/// <summary>
/// Draws the smooth filled band shared by <see cref="TwStreamGraphChart"/> and <see cref="TwBumpAreaChart"/>.
/// </summary>
internal static class ChartRibbon
{
    /// <summary>
    /// Adds a band between a run of points along its top edge and the matching points along its bottom edge.
    /// </summary>
    /// <param name="builder">The builder that collects the scene.</param>
    /// <param name="color">The fill color.</param>
    /// <param name="upper">The top edge, left to right.</param>
    /// <param name="lower">The bottom edge, left to right.</param>
    /// <param name="step">The width of one category.</param>
    public static void Add(ChartSceneBuilder builder, ChartColor color, List<(double X, double Y)> upper, List<(double X, double Y)> lower, double step)
    {
        if (upper.Count == 0)
        {
            return;
        }

        // A band with a single point has no length, so give it half the width of its category.
        if (upper.Count == 1)
        {
            upper = [(upper[0].X - step / 4, upper[0].Y), (upper[0].X + step / 4, upper[0].Y)];
            lower = [(lower[0].X - step / 4, lower[0].Y), (lower[0].X + step / 4, lower[0].Y)];
        }

        var under = Enumerable.Reverse(lower).ToList();
        var path = new ChartPathBuilder()
            .MoveTo(upper[0].X, upper[0].Y).Through(upper, ChartCurve.Smooth)
            .LineTo(under[0].X, under[0].Y).Through(under, ChartCurve.Smooth)
            .Close();
        builder.Path(path.ToString(), color, filled: true, outline: true);
    }
}

/// <summary>
/// A barcode chart: a thin line for every observation along a single value axis, so clusters show as dense
/// stripes and gaps as white space.
/// </summary>
/// <remarks>
/// It shows the raw values without grouping them into ranges, which suits a few dozen to a few hundred
/// observations. Repeated values share one line, and its tooltip gives the count.
/// </remarks>
public class TwBarcodeChart : TwChartBase
{
    /// <summary>
    /// Gets or sets the raw observations to plot.
    /// </summary>
    [Parameter] public IReadOnlyList<double> Values { get; set; } = [];

    /// <summary>
    /// Gets or sets the label for an observation's value in the table view.
    /// Defaults to the localized <see cref="TwStrings.Chart_Value"/> text ("Value").
    /// </summary>
    [Parameter] public string? ValueLabel { get; set; }

    private string effectiveValueLabel => ValueLabel ?? localizer[TwStrings.Chart_Value];

    /// <summary>
    /// Gets or sets the label for how many observations share a value in tooltips and the table view.
    /// Defaults to the localized <see cref="TwStrings.Chart_Count"/> text ("Count").
    /// </summary>
    [Parameter] public string? CountLabel { get; set; }

    private string effectiveCountLabel => CountLabel ?? localizer[TwStrings.Chart_Count];

    /// <inheritdoc />
    protected internal override void BuildScene(ChartSceneBuilder builder)
    {
        var values = Values.Where(double.IsFinite).GroupBy(value => value).OrderBy(group => group.Key).ToList();
        if (values.Count == 0)
        {
            return;
        }

        builder.Horizontal = true;
        var scale = ChartScale.Nice(values.Select(group => group.Key), includeZero: false);
        var color = ChartColor.Series(0);
        builder.ValueAxis(scale, FormatValue);

        List<IReadOnlyList<string>> rows = [];
        foreach (var group in values)
        {
            string[] cells = [FormatValue(group.Key), group.Count().ToString("#,0")];
            builder.Segment(15, scale.Map(group.Key), 85, scale.Map(group.Key), color);
            builder.Datum(scale.Map(group.Key), 50, 0, 70, cells[0], [new ChartDatumRow(effectiveCountLabel, cells[1], color)], ChartHover.Crosshair);
            rows.Add(cells);
        }

        builder.Table = new ChartTable([effectiveValueLabel, effectiveCountLabel], rows);
    }
}