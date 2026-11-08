// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

namespace TwBlazor.Enums;

/// <summary>
/// Defines how tall a chart's plot area is.
/// </summary>
public enum ChartHeight
{
    /// <summary>
    /// A compact plot, for dashboards and cards.
    /// </summary>
    Small,

    /// <summary>
    /// The standard plot height - Default.
    /// </summary>
    Medium,

    /// <summary>
    /// A tall plot, for charts with many rows or fine detail.
    /// </summary>
    Large
}

/// <summary>
/// Defines how multiple series share a category on a bar or column chart.
/// </summary>
public enum ChartStacking
{
    /// <summary>
    /// Series sit side by side (a grouped chart) - Default.
    /// </summary>
    None,

    /// <summary>
    /// Series are stacked on top of each other so each bar shows the category total.
    /// </summary>
    Stacked,

    /// <summary>
    /// Series are stacked and scaled so every bar fills 100%, showing the share of each series.
    /// </summary>
    Percent
}

/// <summary>
/// Defines how the points of a line or area chart are joined.
/// </summary>
public enum ChartCurve
{
    /// <summary>
    /// Straight segments between points (a line chart) - Default.
    /// </summary>
    Linear,

    /// <summary>
    /// A smooth curve through the points (a spline chart).
    /// </summary>
    Smooth,

    /// <summary>
    /// Horizontal then vertical segments (a step line chart), for values that change at discrete moments.
    /// </summary>
    Step
}

/// <summary>
/// Defines how <see cref="TwBlazor.Components.TwIconChart"/> encodes a value.
/// </summary>
public enum IconChartMode
{
    /// <summary>
    /// Each value is drawn as a row of repeated icons (a pictogram) - Default.
    /// </summary>
    Count,

    /// <summary>
    /// Each value is drawn as one icon whose area follows the value.
    /// </summary>
    Size
}

/// <summary>
/// Defines the mark drawn in each panel of <see cref="TwBlazor.Components.TwSmallMultiplesChart"/>.
/// </summary>
public enum SmallMultipleKind
{
    /// <summary>
    /// A column per category - Default.
    /// </summary>
    Column,

    /// <summary>
    /// A line through the categories.
    /// </summary>
    Line,

    /// <summary>
    /// A filled area under a line through the categories.
    /// </summary>
    Area
}

/// <summary>
/// Defines how <see cref="TwBlazor.Components.TwCandlestickChart"/> draws each period.
/// </summary>
public enum CandlestickStyle
{
    /// <summary>
    /// A filled body between open and close with a wick to the high and low - Default.
    /// </summary>
    Candle,

    /// <summary>
    /// A high-low line with an open tick on the left and a close tick on the right (an OHLC chart).
    /// </summary>
    Ohlc
}

/// <summary>
/// Defines how much time a <see cref="TwBlazor.Components.TwGanttChart"/> shows, and how finely its axis is divided.
/// </summary>
public enum GanttZoom
{
    /// <summary>
    /// The whole schedule fits the width of the chart, with a few evenly spaced dates on the axis - Default.
    /// </summary>
    Auto,

    /// <summary>
    /// One column per hour. The plot scrolls when it is wider than the chart.
    /// </summary>
    Hour,

    /// <summary>
    /// One column per day. The plot scrolls when it is wider than the chart.
    /// </summary>
    Day,

    /// <summary>
    /// One column per week. The plot scrolls when it is wider than the chart.
    /// </summary>
    Week,

    /// <summary>
    /// One column per month. The plot scrolls when it is wider than the chart.
    /// </summary>
    Month,

    /// <summary>
    /// One column per quarter. The plot scrolls when it is wider than the chart.
    /// </summary>
    Quarter,

    /// <summary>
    /// One column per year. The plot scrolls when it is wider than the chart.
    /// </summary>
    Year
}

/// <summary>
/// Defines how two tasks in a <see cref="TwBlazor.Components.TwGanttChart"/> depend on each other.
/// </summary>
public enum ChartDependencyType
{
    /// <summary>
    /// The second task starts once the first finishes - Default.
    /// </summary>
    FinishToStart,

    /// <summary>
    /// The second task starts once the first starts.
    /// </summary>
    StartToStart,

    /// <summary>
    /// The second task finishes once the first finishes.
    /// </summary>
    FinishToFinish,

    /// <summary>
    /// The second task finishes once the first starts.
    /// </summary>
    StartToFinish
}

/// <summary>
/// Defines which gesture changed a task on a <see cref="TwBlazor.Components.TwGanttChart"/>.
/// </summary>
public enum ChartTaskChangeKind
{
    /// <summary>
    /// The bar was dragged to new dates, keeping its length.
    /// </summary>
    Move,

    /// <summary>
    /// The start of the bar was dragged.
    /// </summary>
    ResizeStart,

    /// <summary>
    /// The end of the bar was dragged.
    /// </summary>
    ResizeEnd,

    /// <summary>
    /// The grip under the bar was dragged to set how far along the task is.
    /// </summary>
    Progress
}
