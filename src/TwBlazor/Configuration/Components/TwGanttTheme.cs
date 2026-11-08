// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;

namespace TwBlazor.Configuration.Components;

/// <summary>
/// Theme configuration for the parts of the Gantt chart (<see cref="TwBlazor.Components.TwGanttChart"/>) that other
/// charts do not have: the task tree, the zoom controls and the drag handles. Its bars, axes, legend and tooltips
/// use <see cref="TwChartTheme"/> like every other chart.
/// </summary>
[ExcludeFromCodeCoverage]
public class TwGanttTheme
{
    /// <summary>
    /// Gets or sets the classes for the element that scrolls the plot when a zoom level makes it wider than the chart.
    /// </summary>
    public required string Scroller { get; set; }

    /// <summary>
    /// Gets or sets the classes for the task names while the plot scrolls, so they keep the plot's height instead of stretching.
    /// </summary>
    public required string YAxisScrolling { get; set; }

    /// <summary>
    /// Gets or sets the classes for the task names on the left of the plot, such as how wide they can grow.
    /// </summary>
    public required string YAxis { get; set; }

    /// <summary>
    /// Gets or sets the classes for a task row while <see cref="TwBlazor.Components.TwGanttChart.OnTaskClick"/> is set.
    /// </summary>
    public required string Clickable { get; set; }

    /// <summary>
    /// Gets or sets the classes for the zoom, expand, collapse and today controls.
    /// </summary>
    public required string Toolbar { get; set; }

    /// <summary>
    /// Gets or sets the classes for a task name on the left: its indent, expand button and text.
    /// </summary>
    public required string Tick { get; set; }

    /// <summary>
    /// Gets or sets the classes for the text of a task name.
    /// </summary>
    public required string TickLabel { get; set; }

    /// <summary>
    /// Gets or sets the classes for the button that shows or hides the tasks under a summary task.
    /// </summary>
    public required string Toggle { get; set; }

    /// <summary>
    /// Gets or sets the classes for the chevron in an expand button.
    /// </summary>
    public required string ToggleIcon { get; set; }

    /// <summary>
    /// Gets or sets the classes for the chevron in an expand button while its tasks are shown.
    /// </summary>
    public required string ToggleIconOpen { get; set; }

    /// <summary>
    /// Gets or sets the classes for the space in place of an expand button on a task with nothing under it, so names line up.
    /// </summary>
    public required string ToggleSpacer { get; set; }

    /// <summary>
    /// Gets or sets the classes for the layer over the plot that holds the drag handles.
    /// </summary>
    public required string Overlay { get; set; }

    /// <summary>
    /// Gets or sets the classes for the outline over a bar that can be dragged; it holds the handles and shows them while the pointer is over the bar.
    /// </summary>
    public required string Bar { get; set; }

    /// <summary>
    /// Gets or sets the classes for a handle at either end of a bar for changing its dates.
    /// </summary>
    public required string Handle { get; set; }

    /// <summary>
    /// Gets or sets the classes for the handle at the start of a bar.
    /// </summary>
    public required string HandleStart { get; set; }

    /// <summary>
    /// Gets or sets the classes for the handle at the end of a bar.
    /// </summary>
    public required string HandleEnd { get; set; }

    /// <summary>
    /// Gets or sets the classes for the grip under a bar for setting how far along the task is.
    /// </summary>
    public required string ProgressGrip { get; set; }

    /// <summary>
    /// Gets or sets the classes for a dot beside either end of a bar for drawing a dependency.
    /// </summary>
    public required string LinkDot { get; set; }

    /// <summary>
    /// Gets or sets the classes for the dot before the start of a bar.
    /// </summary>
    public required string LinkDotStart { get; set; }

    /// <summary>
    /// Gets or sets the classes for the dot after the end of a bar.
    /// </summary>
    public required string LinkDotEnd { get; set; }

    /// <summary>
    /// Gets or sets the classes for the outline that previews where a dragged bar will go.
    /// </summary>
    public required string Ghost { get; set; }

    /// <summary>
    /// Gets or sets the classes for the line that follows the pointer while a dependency is drawn.
    /// </summary>
    public required string LinkLine { get; set; }

    /// <summary>
    /// Gets or sets the classes for the layer of click targets over the dependency arrows.
    /// </summary>
    public required string Links { get; set; }

    /// <summary>
    /// Gets or sets the classes for the click target over a dependency arrow.
    /// </summary>
    public required string LinkHit { get; set; }
}
