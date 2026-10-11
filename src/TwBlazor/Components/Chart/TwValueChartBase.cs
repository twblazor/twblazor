// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using Microsoft.AspNetCore.Components;
using TwBlazor.Charting;
using TwBlazor.Models;
using TwBlazor.Localization;

namespace TwBlazor.Components;

/// <summary>
/// The base class for charts that plot a single list of labelled values, such as a pie or a funnel.
/// It supplies the table view, leaving the marks to the chart type.
/// </summary>
public abstract class TwValueChartBase : TwChartBase
{
    /// <summary>
    /// Gets or sets the values to plot.
    /// </summary>
    [Parameter] public IReadOnlyList<ChartValue> Data { get; set; } = [];

    /// <summary>
    /// Gets or sets the heading of the label column in the table view.
    /// Defaults to the localized <see cref="TwStrings.Chart_Category"/> text ("Category").
    /// </summary>
    [Parameter] public string? LabelHeader { get; set; }

    private string effectiveLabelHeader => LabelHeader ?? localizer[TwStrings.Chart_Category];

    /// <summary>
    /// Gets or sets the heading of the value column in the table view. It also labels the value in tooltips.
    /// Defaults to the localized <see cref="TwStrings.Chart_Value"/> text ("Value").
    /// </summary>
    [Parameter] public string? ValueHeader { get; set; }

    private protected string effectiveValueHeader => ValueHeader ?? localizer[TwStrings.Chart_Value];

    /// <summary>
    /// Gets or sets the label of the slice that collects the smallest values once there are more values than colors.
    /// Defaults to the localized <see cref="TwStrings.Chart_Other"/> text ("Other").
    /// </summary>
    [Parameter] public string? OtherLabel { get; set; }

    private string effectiveOtherLabel => OtherLabel ?? localizer[TwStrings.Chart_Other];

    /// <summary>
    /// Adds the marks, data points and axes for the values.
    /// </summary>
    /// <param name="builder">The builder that collects the scene.</param>
    protected abstract void BuildMarks(ChartSceneBuilder builder);

    /// <inheritdoc />
    protected internal sealed override void BuildScene(ChartSceneBuilder builder)
    {
        if (Data.Count == 0)
        {
            return;
        }

        builder.Table = new ChartTable([effectiveLabelHeader, effectiveValueHeader], [.. Data.Select(item => (IReadOnlyList<string>)[item.Label, FormatValue(item.Value)])]);
        BuildMarks(builder);
    }

    /// <summary>
    /// Gets the positive values, with the smallest folded into one <see cref="OtherLabel"/> entry when there are
    /// more than <paramref name="limit"/>. Charts that color each value use this so a color is never reused.
    /// </summary>
    /// <param name="limit">The most entries to return.</param>
    protected IReadOnlyList<ChartValue> GetFoldedData(int limit = 8)
    {
        var positive = Data.Where(item => item.Value > 0).ToList();
        if (positive.Count <= limit)
        {
            return positive;
        }

        var kept = positive.OrderByDescending(item => item.Value).Take(limit - 1).ToHashSet();
        return [.. positive.Where(kept.Contains), new ChartValue(effectiveOtherLabel, positive.Where(item => !kept.Contains(item)).Sum(item => item.Value))];
    }

    /// <summary>
    /// Formats a share of a whole as a percentage.
    /// </summary>
    /// <param name="value">The part.</param>
    /// <param name="total">The whole.</param>
    protected static string FormatShare(double value, double total) => total == 0 ? "0%" : (value / total).ToString("0.#%");
}
