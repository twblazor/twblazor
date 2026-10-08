// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using System.Globalization;
using Microsoft.AspNetCore.Components;
using TwBlazor.Charting;
using TwBlazor.Models;

namespace TwBlazor.Components;

/// <summary>
/// A Gantt chart: a horizontal bar per task from its start date to its end date, showing a schedule and where
/// tasks overlap.
/// </summary>
public class TwGanttChart : TwChartBase
{
    private const int tickCount = 4;

    /// <summary>
    /// Gets or sets the tasks to plot, in the order they should be listed.
    /// </summary>
    [Parameter] public IReadOnlyList<ChartTask> Tasks { get; set; } = [];

    /// <summary>
    /// Gets or sets the .NET date format string used on the axis, tooltips and table view.
    /// </summary>
    [Parameter] public string DateFormat { get; set; } = "d MMM";

    /// <summary>
    /// Gets or sets the heading of the task column in the table view.
    /// </summary>
    [Parameter] public string LabelHeader { get; set; } = "Task";

    /// <summary>
    /// Gets or sets the label for a task's start date.
    /// </summary>
    [Parameter] public string StartLabel { get; set; } = "Start";

    /// <summary>
    /// Gets or sets the label for a task's end date.
    /// </summary>
    [Parameter] public string EndLabel { get; set; } = "End";

    /// <inheritdoc />
    protected internal override void BuildScene(ChartSceneBuilder builder)
    {
        if (Tasks.Count == 0)
        {
            return;
        }

        builder.Horizontal = true;
        var first = Tasks.Min(task => task.Start);
        var span = Math.Max(1, (Tasks.Max(task => task.End) - first).Ticks);
        var band = new ChartBand(Tasks.Count);
        double Position(DateTime date) => (date - first).Ticks / (double)span * 100;
        string Format(DateTime date) => date.ToString(DateFormat, CultureInfo.CurrentCulture);

        builder.ValueAxis([.. Enumerable.Range(0, tickCount + 1).Select(tick => new ChartTick(100.0 * tick / tickCount, Format(first.AddTicks(span * tick / tickCount))))]);
        builder.BandAxis([.. Tasks.Select(task => task.Label)], band);

        for (var i = 0; i < Tasks.Count; i++)
        {
            var task = Tasks[i];
            builder.Bar(band.Center(i), Math.Min(band.Step * 0.5, 8), Position(task.Start), Position(task.End), ChartColor.Series(0), ChartCorner.All);
            builder.BandDatum(band.Center(i), band.Step, task.Label, [new ChartDatumRow(StartLabel, Format(task.Start)), new ChartDatumRow(EndLabel, Format(task.End))]);
        }

        builder.Table = new ChartTable([LabelHeader, StartLabel, EndLabel],
            [.. Tasks.Select(task => (IReadOnlyList<string>)[task.Label, Format(task.Start), Format(task.End)])]);
    }
}
