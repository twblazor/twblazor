// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using Microsoft.AspNetCore.Components;
using TwBlazor.Charting;
using TwBlazor.Models;

namespace TwBlazor.Components;

/// <summary>
/// A visible series paired with its slot, which fixes its color even while other series are hidden.
/// </summary>
/// <param name="Slot">The zero-based index of the series in the chart's <c>Series</c> parameter.</param>
/// <param name="Series">The series.</param>
public readonly record struct ChartSeriesSlot(int Slot, ChartSeries Series)
{
    /// <summary>Gets the color of the series.</summary>
    public ChartColor Color => ChartColor.Series(Slot);

    /// <summary>Gets the value for a category, or <see langword="null"/> when it is missing.</summary>
    /// <param name="category">The zero-based category index.</param>
    public double? Value(int category) => category < Series.Values.Count ? Series.Values[category] : null;
}

/// <summary>
/// The base class for charts that plot one or more <see cref="ChartSeries"/> across a shared list of categories.
/// It supplies the legend, the series toggling and the table view, leaving only the marks to the chart type.
/// </summary>
public abstract class TwCategoryChartBase : TwChartBase
{
    /// <summary>
    /// Gets or sets the category labels, in order.
    /// </summary>
    [Parameter] public IReadOnlyList<string> Categories { get; set; } = [];

    /// <summary>
    /// Gets or sets the series to plot. Each holds one value per category.
    /// </summary>
    [Parameter] public IReadOnlyList<ChartSeries> Series { get; set; } = [];

    /// <summary>
    /// Gets or sets the heading of the category column in the table view.
    /// </summary>
    [Parameter] public string CategoryHeader { get; set; } = "Category";

    /// <summary>
    /// Gets whether each series gets a legend entry that can hide it. Charts that label their series directly
    /// on the plot turn this off.
    /// </summary>
    protected virtual bool hasSeriesLegend => true;

    /// <summary>
    /// Adds the marks, data points and axes for the visible series.
    /// </summary>
    /// <param name="builder">The builder that collects the scene.</param>
    /// <param name="series">The series the reader has not hidden. Never empty.</param>
    protected abstract void BuildMarks(ChartSceneBuilder builder, IReadOnlyList<ChartSeriesSlot> series);

    /// <inheritdoc />
    protected internal sealed override void BuildScene(ChartSceneBuilder builder)
    {
        if (Categories.Count == 0 || Series.Count == 0)
        {
            return;
        }

        List<ChartSeriesSlot> visible = [];
        for (var i = 0; i < Series.Count; i++)
        {
            var slot = new ChartSeriesSlot(i, Series[i]);
            var hidden = hasSeriesLegend && IsSeriesHidden(i);

            if (hasSeriesLegend && Series.Count > 1)
            {
                builder.Legend(Series[i].Name, slot.Color, i, hidden);
            }

            if (!hidden)
            {
                visible.Add(slot);
            }
        }

        builder.Table = new ChartTable(
            [CategoryHeader, .. Series.Select(series => series.Name)],
            [.. Categories.Select((category, index) => (IReadOnlyList<string>)[category, .. Series.Select((series, slot) => FormatCell(new ChartSeriesSlot(slot, series).Value(index)))])]);

        if (visible.Count > 0)
        {
            BuildMarks(builder, visible);
        }
    }

    /// <summary>
    /// Builds the tooltip rows for one category: every visible series that has a value there.
    /// </summary>
    /// <param name="series">The visible series.</param>
    /// <param name="category">The zero-based category index.</param>
    protected IReadOnlyList<ChartDatumRow> GetRows(IReadOnlyList<ChartSeriesSlot> series, int category) =>
        [.. series.Where(slot => slot.Value(category).HasValue).Select(slot => new ChartDatumRow(slot.Series.Name, FormatValue(slot.Value(category)!.Value), slot.Color))];

    /// <summary>
    /// Gets every value the visible series hold, skipping missing ones.
    /// </summary>
    /// <param name="series">The visible series.</param>
    protected IEnumerable<double> GetValues(IReadOnlyList<ChartSeriesSlot> series) =>
        series.SelectMany(slot => Enumerable.Range(0, Categories.Count).Select(slot.Value)).Where(value => value.HasValue).Select(value => value!.Value);

    private string FormatCell(double? value) => value.HasValue ? FormatValue(value.Value) : string.Empty;
}
