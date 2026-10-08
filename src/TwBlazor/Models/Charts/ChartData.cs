// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

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
/// <param name="Label">The task name shown on the axis, tooltips and table view.</param>
/// <param name="Start">When the task starts.</param>
/// <param name="End">When the task ends.</param>
public sealed record ChartTask(string Label, DateTime Start, DateTime End);

/// <summary>
/// A labelled set of raw observations, summarised by the distribution charts
/// (<see cref="TwBlazor.Components.TwBoxPlotChart"/> and <see cref="TwBlazor.Components.TwStripPlotChart"/>).
/// </summary>
/// <param name="Label">The group label shown on the axis, tooltips and table view.</param>
/// <param name="Values">The raw observations in the group.</param>
public sealed record ChartSample(string Label, IReadOnlyList<double> Values);
