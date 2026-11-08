// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using TwBlazor.Enums;

namespace TwBlazor.Models;

/// <summary>
/// A named run of values, one per category, plotted by the category-based charts (bar, column, line, radar and so on).
/// </summary>
/// <param name="Name">The series name shown in the legend, tooltips and table view.</param>
/// <param name="Values">The values in category order. Use <see langword="null"/> for a missing value.</param>
public sealed record ChartSeries(string Name, IReadOnlyList<double?> Values);

/// <summary>
/// A single labelled value, plotted by the single-series charts (pie, funnel, waterfall, treemap and so on).
/// </summary>
/// <param name="Label">The label shown on the axis, legend, tooltips and table view.</param>
/// <param name="Value">The value.</param>
public sealed record ChartValue(string Label, double Value);

/// <summary>
/// A point positioned by two numeric values, plotted by <see cref="TwBlazor.Components.TwScatterChart"/> and
/// <see cref="TwBlazor.Components.TwQuadrantChart"/>.
/// </summary>
/// <param name="X">The value along the horizontal axis.</param>
/// <param name="Y">The value along the vertical axis.</param>
/// <param name="Label">An optional name for the point, shown in its tooltip and the table view.</param>
/// <param name="Size">An optional third value. When set the point is drawn as a bubble whose area follows it.</param>
public sealed record ChartPoint(double X, double Y, string? Label = null, double? Size = null);

/// <summary>
/// A named group of <see cref="ChartPoint"/> values that share a color.
/// </summary>
/// <param name="Name">The series name shown in the legend, tooltips and table view.</param>
/// <param name="Points">The points in the series.</param>
public sealed record ChartPointSeries(string Name, IReadOnlyList<ChartPoint> Points);

/// <summary>
/// A labelled span between two values, plotted by <see cref="TwBlazor.Components.TwRangeChart"/>.
/// </summary>
/// <param name="Label">The label shown on the axis, tooltips and table view.</param>
/// <param name="Low">The low end of the range.</param>
/// <param name="High">The high end of the range.</param>
public sealed record ChartRange(string Label, double Low, double High);

/// <summary>
/// One measure on a <see cref="TwBlazor.Components.TwBulletChart"/>: an actual value benchmarked against a target.
/// </summary>
/// <param name="Label">The label shown on the axis, tooltips and table view.</param>
/// <param name="Value">The actual value, drawn as the bar.</param>
/// <param name="Target">The target value, drawn as a marker across the bar.</param>
/// <param name="Ranges">Optional qualitative range limits (for example poor, fair, good) drawn behind the bar.</param>
public sealed record ChartBullet(string Label, double Value, double Target, IReadOnlyList<double>? Ranges = null);

/// <summary>
/// The open, high, low and close values for one period, plotted by <see cref="TwBlazor.Components.TwCandlestickChart"/>.
/// </summary>
/// <param name="Label">The period label shown on the axis, tooltips and table view.</param>
/// <param name="Open">The opening value.</param>
/// <param name="High">The highest value.</param>
/// <param name="Low">The lowest value.</param>
/// <param name="Close">The closing value.</param>
public sealed record ChartOhlc(string Label, double Open, double High, double Low, double Close);

/// <summary>
/// A scheduled task, plotted by <see cref="TwBlazor.Components.TwGanttChart"/>.
/// </summary>
/// <remarks>
/// A task that ends when it starts (or before) is a milestone, drawn as a diamond. A task that other tasks name as
/// their <see cref="ParentId"/> is a summary: its dates and progress are rolled up from the tasks under it.
/// </remarks>
/// <param name="Label">The task name shown on the axis, tooltips and table view.</param>
/// <param name="Start">When the task starts.</param>
/// <param name="End">When the task ends.</param>
public sealed record ChartTask(string Label, DateTime Start, DateTime End)
{
    /// <summary>
    /// Gets the identifier that dependencies and child tasks use to refer to this task. Defaults to
    /// <see cref="Label"/>, so it only needs setting when two tasks share a name.
    /// </summary>
    public string? Id { get; init; }

    /// <summary>
    /// Gets the identifier of the summary task this task sits under, or <see langword="null"/> for a top-level task.
    /// </summary>
    public string? ParentId { get; init; }

    /// <summary>
    /// Gets how far along the task is, from 0 (not started) to 1 (done), or <see langword="null"/> to show no progress.
    /// </summary>
    public double? Progress { get; init; }

    /// <summary>
    /// Gets the zero-based slot of the theme's series colors the bar uses. Defaults to the first.
    /// </summary>
    public int? ColorIndex { get; init; }

    /// <summary>
    /// Gets the identifier the chart uses for the task: <see cref="Id"/>, or <see cref="Label"/> when that is not set.
    /// </summary>
    public string Key => Id ?? Label;
}

/// <summary>
/// A dependency between two tasks on a <see cref="TwBlazor.Components.TwGanttChart"/>, drawn as an arrow.
/// </summary>
/// <param name="FromId">The <see cref="ChartTask.Key"/> of the task the other depends on.</param>
/// <param name="ToId">The <see cref="ChartTask.Key"/> of the task that depends on it.</param>
/// <param name="Type">How the two tasks depend on each other.</param>
public sealed record ChartTaskDependency(string FromId, string ToId, ChartDependencyType Type = ChartDependencyType.FinishToStart);

/// <summary>
/// A change a person made to a task on a <see cref="TwBlazor.Components.TwGanttChart"/> by dragging it. The chart
/// never changes the task itself: store the new values, or set <see cref="Cancel"/> to put the bar back.
/// </summary>
public sealed class ChartTaskChange
{
    /// <summary>Gets which gesture made the change.</summary>
    public ChartTaskChangeKind Kind { get; init; }

    /// <summary>Gets the task as it was before the change.</summary>
    public required ChartTask Task { get; init; }

    /// <summary>Gets the new start. Unchanged by a <see cref="ChartTaskChangeKind.ResizeEnd"/> or progress drag.</summary>
    public DateTime Start { get; init; }

    /// <summary>Gets the new end. Unchanged by a <see cref="ChartTaskChangeKind.ResizeStart"/> or progress drag.</summary>
    public DateTime End { get; init; }

    /// <summary>Gets the new progress, from 0 to 1. Only a progress drag changes it.</summary>
    public double? Progress { get; init; }

    /// <summary>Gets or sets whether to refuse the change, so the bar stays where it was.</summary>
    public bool Cancel { get; set; }
}

/// <summary>
/// A dependency a person drew between two tasks on a <see cref="TwBlazor.Components.TwGanttChart"/>. The arrow
/// appears once the dependency is in the chart's <c>Dependencies</c>; set <see cref="Cancel"/> to refuse it.
/// </summary>
public sealed class ChartDependencyRequest
{
    /// <summary>Gets the dependency that was drawn.</summary>
    public required ChartTaskDependency Dependency { get; init; }

    /// <summary>Gets the task the arrow starts from.</summary>
    public required ChartTask From { get; init; }

    /// <summary>Gets the task the arrow points to.</summary>
    public required ChartTask To { get; init; }

    /// <summary>Gets or sets whether to refuse the dependency.</summary>
    public bool Cancel { get; set; }
}

/// <summary>
/// A labelled set of raw observations, summarised by the distribution charts
/// (<see cref="TwBlazor.Components.TwBoxPlotChart"/> and <see cref="TwBlazor.Components.TwStripPlotChart"/>).
/// </summary>
/// <param name="Label">The group label shown on the axis, tooltips and table view.</param>
/// <param name="Values">The raw observations in the group.</param>
public sealed record ChartSample(string Label, IReadOnlyList<double> Values);
