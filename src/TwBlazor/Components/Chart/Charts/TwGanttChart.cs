// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using Microsoft.AspNetCore.Components;
using System.Globalization;
using TwBlazor.Charting;
using TwBlazor.Configuration.Components;
using TwBlazor.Enums;
using TwBlazor.Localization;
using TwBlazor.Models;

namespace TwBlazor.Components;

/// <summary>
/// A Gantt chart: a horizontal bar per task from its start date to its end date, showing a schedule and where
/// tasks overlap. Tasks can sit under summary tasks, show their progress, depend on each other, and be moved,
/// resized and linked by dragging.
/// </summary>
/// <remarks>
/// <para>
/// The chart never changes the tasks it is given. Every gesture raises an event (<see cref="OnTaskChange"/>,
/// <see cref="OnDependencyCreate"/>) that can be cancelled; store the change and pass the new tasks back in.
/// </para>
/// <para>
/// Accessibility: each row is a data point whose label gives the task's dates and progress, and the table view
/// lists every task, including those under collapsed summaries. The expand buttons, zoom controls and toolbar
/// buttons are keyboard operable. Dragging is pointer only, so offer another way to change dates, such as a form,
/// when editing matters.
/// </para>
/// </remarks>
public partial class TwGanttChart : TwChartBase
{
    private const string defaultDateFormat = "d MMM";

    private GanttPlan plan = GanttPlan.Empty;
    private GanttTimeline? timeline;
    private HashSet<string> collapsed = new(StringComparer.Ordinal);
    private IReadOnlySet<string>? lastCollapsedIds;
    private bool collapseInitialized;
    private GanttZoom zoom;
    private GanttZoom? lastZoom;
    private DateTime? todayShown;

    // The today line has no series of its own, so its legend entry toggles a number no series can have.
    private const int todaySeries = -1;
    private List<(ChartTaskDependency Dependency, string Path)> arrows = [];

    /// <summary>
    /// Gets or sets the tasks to plot, in the order they should be listed. Tasks with a
    /// <see cref="ChartTask.ParentId"/> are listed under their summary task.
    /// </summary>
    [Parameter] public IReadOnlyList<ChartTask> Tasks { get; set; } = [];

    /// <summary>
    /// Gets or sets the .NET date format string used on the axis at <see cref="GanttZoom.Auto"/>, and in tooltips
    /// and the table view.
    /// </summary>
    /// <remarks>Default is <c>"d MMM"</c>, which also shows the time at <see cref="GanttZoom.Hour"/>.</remarks>
    [Parameter] public string DateFormat { get; set; } = defaultDateFormat;

    /// <summary>
    /// Gets or sets the heading of the task column in the table view.
    /// Defaults to the localized <see cref="TwStrings.Chart_Task"/> text ("Task").
    /// </summary>
    [Parameter] public string? LabelHeader { get; set; }

    private string effectiveLabelHeader => LabelHeader ?? localizer[TwStrings.Chart_Task];

    /// <summary>
    /// Gets or sets the label for a task's start date.
    /// Defaults to the localized <see cref="TwStrings.Chart_Start"/> text ("Start").
    /// </summary>
    [Parameter] public string? StartLabel { get; set; }

    private string effectiveStartLabel => StartLabel ?? localizer[TwStrings.Chart_Start];

    /// <summary>
    /// Gets or sets the label for a task's end date.
    /// Defaults to the localized <see cref="TwStrings.Chart_End"/> text ("End").
    /// </summary>
    [Parameter] public string? EndLabel { get; set; }

    private string effectiveEndLabel => EndLabel ?? localizer[TwStrings.Chart_End];

    /// <summary>
    /// Gets or sets the dependencies between tasks, each drawn as an arrow from one task to the other.
    /// </summary>
    [Parameter] public IReadOnlyList<ChartTaskDependency> Dependencies { get; set; } = [];

    /// <summary>
    /// Gets or sets whether the dependency arrows are drawn. Default is <see langword="true"/>.
    /// </summary>
    [Parameter] public bool ShowDependencies { get; set; } = true;

    /// <summary>
    /// Gets or sets how much time the chart shows. Bind it with <c>@bind-Zoom</c> to follow the zoom controls.
    /// </summary>
    /// <remarks>
    /// Default is <see cref="GanttZoom.Auto"/>: the whole schedule fits the chart's width. Any other level gives
    /// each day, week, month, quarter or year a column of <see cref="SlotWidth"/> pixels and scrolls the plot
    /// when it is wider than the chart, keeping the task names in view.
    /// </remarks>
    [Parameter] public GanttZoom Zoom { get; set; }

    /// <summary>
    /// Raised when the zoom level changes from the zoom controls or <see cref="SetZoomAsync"/>.
    /// </summary>
    [Parameter] public EventCallback<GanttZoom> ZoomChanged { get; set; }

    /// <summary>
    /// Gets or sets the width of one column in pixels at any zoom level other than <see cref="GanttZoom.Auto"/>.
    /// If not set, each level has its own width, from 40 pixels a day to 88 a year.
    /// </summary>
    [Parameter] public int? SlotWidth { get; set; }

    /// <summary>
    /// Gets or sets the first date the chart shows. If not set, the chart starts at the earliest task.
    /// </summary>
    [Parameter] public DateTime? RangeStart { get; set; }

    /// <summary>
    /// Gets or sets the last date the chart shows. If not set, the chart ends at the latest task.
    /// </summary>
    [Parameter] public DateTime? RangeEnd { get; set; }

    /// <summary>
    /// Gets or sets the days of the week that are shaded as non-working at <see cref="GanttZoom.Day"/> and
    /// <see cref="GanttZoom.Hour"/>. If not set, Saturday and Sunday.
    /// </summary>
    [Parameter] public IReadOnlyCollection<DayOfWeek>? NonWorkingDays { get; set; }

    /// <summary>
    /// Gets or sets whether non-working days are shaded. Default is <see langword="true"/>.
    /// </summary>
    [Parameter] public bool ShowNonWorkingDays { get; set; } = true;

    /// <summary>
    /// Gets or sets whether a line marks today, when today is in the chart's range. Default is <see langword="true"/>.
    /// </summary>
    [Parameter] public bool ShowToday { get; set; } = true;

    /// <summary>
    /// Gets or sets the moment the today line marks. If not set, the current time.
    /// </summary>
    [Parameter] public DateTime? Today { get; set; }

    /// <summary>
    /// Gets or sets whether a scrolling plot first scrolls to today, when today is in the chart's range.
    /// Default is <see langword="true"/>.
    /// </summary>
    [Parameter] public bool ScrollToToday { get; set; } = true;

    /// <summary>
    /// Gets or sets the keys of the summary tasks whose tasks are hidden. Bind it with
    /// <c>@bind-CollapsedIds</c> to keep the state.
    /// </summary>
    /// <remarks>The set names the closed summaries, so a task added later starts open.</remarks>
    [Parameter] public IReadOnlySet<string>? CollapsedIds { get; set; }

    /// <summary>
    /// Raised with the new set of collapsed summaries when one is expanded or collapsed.
    /// </summary>
    [Parameter] public EventCallback<IReadOnlySet<string>> CollapsedIdsChanged { get; set; }

    /// <summary>
    /// Gets or sets whether summaries start expanded when <see cref="CollapsedIds"/> is not set.
    /// Default is <see langword="true"/>.
    /// </summary>
    [Parameter] public bool DefaultExpandAll { get; set; } = true;

    /// <summary>
    /// Gets or sets whether a summary task's dates and progress are worked out from the tasks under it.
    /// </summary>
    /// <remarks>
    /// Default is <see langword="true"/>: a summary spans its tasks, and its progress is theirs weighted by length.
    /// Turn it off when a summary keeps dates of its own, such as an agreed baseline.
    /// </remarks>
    [Parameter] public bool RollUpSummaries { get; set; } = true;

    /// <summary>
    /// Gets or sets whether bars can be dragged to new dates. Default is <see langword="false"/>.
    /// </summary>
    [Parameter] public bool AllowDrag { get; set; }

    /// <summary>
    /// Gets or sets whether either end of a bar can be dragged to change its start or end.
    /// Default is <see langword="false"/>.
    /// </summary>
    [Parameter] public bool AllowResize { get; set; }

    /// <summary>
    /// Gets or sets whether the grip under a bar can be dragged to set the task's progress.
    /// Default is <see langword="false"/>.
    /// </summary>
    [Parameter] public bool AllowProgressDrag { get; set; }

    /// <summary>
    /// Gets or sets whether dragged dates snap to the nearest column (or to midnight at <see cref="GanttZoom.Auto"/>).
    /// Default is <see langword="true"/>.
    /// </summary>
    [Parameter] public bool SnapToSlot { get; set; } = true;

    /// <summary>
    /// Raised when a bar is moved or resized, or its progress is dragged. Store the new values, or set
    /// <see cref="ChartTaskChange.Cancel"/> to leave the bar where it was.
    /// </summary>
    [Parameter] public EventCallback<ChartTaskChange> OnTaskChange { get; set; }

    /// <summary>
    /// Gets or sets whether dependencies can be drawn by dragging from the dot at either end of a bar to another
    /// bar. The ends picked decide the <see cref="ChartDependencyType"/>. Default is <see langword="false"/>.
    /// </summary>
    [Parameter] public bool AllowLinking { get; set; }

    /// <summary>
    /// Raised when a dependency is drawn. Add it to <see cref="Dependencies"/> to keep it, or set
    /// <see cref="ChartDependencyRequest.Cancel"/>.
    /// </summary>
    [Parameter] public EventCallback<ChartDependencyRequest> OnDependencyCreate { get; set; }

    /// <summary>
    /// Raised when a dependency arrow is clicked while <see cref="AllowLinking"/> is on, for example to remove it.
    /// </summary>
    [Parameter] public EventCallback<ChartTaskDependency> OnDependencyClick { get; set; }

    /// <summary>
    /// Raised when a task's row is clicked, or Enter or Space is pressed on it.
    /// </summary>
    [Parameter] public EventCallback<ChartTask> OnTaskClick { get; set; }

    /// <summary>
    /// Gets or sets whether the toolbar with the zoom controls, expand and collapse buttons and today button is
    /// shown. Default is <see langword="true"/>.
    /// </summary>
    [Parameter] public bool ShowToolbar { get; set; } = true;

    /// <summary>
    /// Gets or sets whether the toolbar has zoom controls. Default is <see langword="true"/>.
    /// </summary>
    [Parameter] public bool ShowZoomControls { get; set; } = true;

    /// <summary>
    /// Gets or sets whether each bar is labelled with its task's name. Default is <see langword="false"/>.
    /// </summary>
    [Parameter] public bool ShowTaskLabels { get; set; }

    /// <summary>
    /// Gets or sets the height of each row in pixels, so the chart grows with its tasks. If not set, the rows
    /// share the chart's <see cref="TwChartBase.Height"/>.
    /// </summary>
    [Parameter] public int? RowHeight { get; set; }

    private TwGanttTheme ganttTheme => options.Theme.Components.Require<TwGanttTheme>();

    private CultureInfo culture => localizer.Formats.Culture;

    private string effectiveDateFormat => zoom == GanttZoom.Hour && DateFormat == defaultDateFormat ? "d MMM HH:mm" : DateFormat;

    private bool isScrolling => timeline is { SlotCount: > 0 };

    private bool isInteractive => AllowDrag || AllowResize || AllowProgressDrag || AllowLinking;

    /// <inheritdoc />
    protected override void OnParametersSet()
    {
        if (CollapsedIds != null && !ReferenceEquals(CollapsedIds, lastCollapsedIds))
        {
            collapsed = new HashSet<string>(CollapsedIds, StringComparer.Ordinal);
            lastCollapsedIds = CollapsedIds;
            collapseInitialized = true;
        }
        else if (!collapseInitialized && Tasks.Count > 0)
        {
            if (!DefaultExpandAll)
            {
                collapsed = new HashSet<string>(Tasks.Select(task => task.ParentId).OfType<string>(), StringComparer.Ordinal);
            }

            collapseInitialized = true;
        }

        if (Zoom != lastZoom)
        {
            zoom = Zoom;
            lastZoom = Zoom;
        }

        base.OnParametersSet();
    }

    /// <inheritdoc />
    protected internal override void BuildScene(ChartSceneBuilder builder)
    {
        plan = GanttPlan.Build(Tasks, collapsed, RollUpSummaries);
        timeline = null;
        todayShown = null;
        arrows = [];

        if (plan.Visible.Count == 0)
        {
            return;
        }

        builder.Horizontal = true;
        var rows = plan.Visible;
        var axis = timeline = GanttTimeline.Create(zoom,
            RangeStart ?? plan.All.Min(node => node.Start),
            RangeEnd ?? plan.All.Max(node => node.End),
            culture);
        var band = new ChartBand(rows.Count);
        var thickness = Math.Min(band.Step * 0.5, 8);

        var shadedNonWorking = AddNonWorkingDays(builder, axis);
        AddTimeAxis(builder, axis);
        builder.BandAxis([.. rows.Select(node => node.Task.Label)], band);
        var drewToday = AddToday(builder, axis);

        if (ShowDependencies)
        {
            AddDependencies(builder, axis, band);
        }

        for (var i = 0; i < rows.Count; i++)
        {
            AddTask(builder, axis, rows[i], band.Center(i), thickness);
            // The tooltip points at the bar rather than the middle of the row, which may be scrolled out of view.
            var anchor = rows[i].IsMilestone ? axis.Position(rows[i].Start) : (axis.Position(rows[i].Start) + axis.Position(rows[i].End)) / 2;
            builder.Datum(new ChartDatum(50, band.Center(i), 100, band.Step, rows[i].Task.Label, DescribeTask(rows[i]), AnchorX: anchor, AnchorY: band.Center(i)));
        }

        AddLegend(builder, rows, drewToday, shadedNonWorking);
        AddTable(builder);
    }

    private bool AddNonWorkingDays(ChartSceneBuilder builder, GanttTimeline axis)
    {
        if (!ShowNonWorkingDays || axis.Zoom is not (GanttZoom.Day or GanttZoom.Hour))
        {
            return false;
        }

        var days = NonWorkingDays ?? [DayOfWeek.Saturday, DayOfWeek.Sunday];
        var shading = new ChartPathBuilder();
        var shaded = false;
        int? runStart = null;
        var slot = 0;
        foreach (var (start, _) in axis.Slots())
        {
            if (days.Contains(start.DayOfWeek))
            {
                runStart ??= slot;
            }
            else if (runStart is { } from)
            {
                AddShade(shading, axis, from, slot);
                shaded = true;
                runStart = null;
            }

            slot++;
        }

        if (runStart is { } last)
        {
            AddShade(shading, axis, last, slot);
            shaded = true;
        }

        // Drawn in the plot's SVG, under the grid and today line; a rectangle would sit above both and hide today
        // when it falls on a non-working day.
        if (shaded)
        {
            builder.Add(new ChartPath(shading.ToString(), ChartColor.Track, Filled: true));
        }

        return shaded;
    }

    private static void AddShade(ChartPathBuilder path, GanttTimeline axis, int fromSlot, int toSlot) =>
        path.MoveTo(fromSlot * axis.SlotSize, 0).LineTo(toSlot * axis.SlotSize, 0).LineTo(toSlot * axis.SlotSize, 100).LineTo(fromSlot * axis.SlotSize, 100).Close();

    private void AddTimeAxis(ChartSceneBuilder builder, GanttTimeline axis)
    {
        var ticks = axis.Ticks(FormatDate, FormatSlot);
        if (axis.SlotCount == 0)
        {
            builder.ValueAxis(ticks);
            return;
        }

        for (var boundary = 0; boundary <= axis.SlotCount; boundary++)
        {
            var x = boundary * axis.SlotSize;
            builder.Line(x, 0, x, 100, ChartColor.Grid);
        }

        builder.XAxis = new ChartAxis(ticks);
    }

    private bool AddToday(ChartSceneBuilder builder, GanttTimeline axis)
    {
        var today = Today ?? DateTime.Now;
        if (!ShowToday || today < axis.Start || today > axis.End)
        {
            return false;
        }

        todayShown = today;
        if (!IsSeriesHidden(todaySeries))
        {
            var x = axis.Position(today);
            builder.Line(x, 0, x, 100, ChartColor.Negative, ChartStroke.Normal);
        }

        return true;
    }

    private void AddDependencies(ChartSceneBuilder builder, GanttTimeline axis, ChartBand band)
    {
        var gap = axis.SlotCount > 0 ? axis.SlotSize * 0.35 : 1.2;
        foreach (var dependency in Dependencies)
        {
            AddDependency(builder, axis, band, dependency, gap);
        }
    }

    private void AddDependency(ChartSceneBuilder builder, GanttTimeline axis, ChartBand band, ChartTaskDependency dependency, double gap)
    {
        var fromRow = plan.VisibleRowOf(dependency.FromId);
        var toRow = plan.VisibleRowOf(dependency.ToId);

        // A dependency inside one collapsed summary has nowhere to go.
        if (fromRow < 0 || toRow < 0 || fromRow == toRow)
        {
            return;
        }

        var fromEnd = dependency.Type is ChartDependencyType.FinishToStart or ChartDependencyType.FinishToFinish;
        var toStart = dependency.Type is ChartDependencyType.FinishToStart or ChartDependencyType.StartToStart;
        var from = plan.Visible[fromRow];
        var to = plan.Visible[toRow];
        var exit = new GanttArrowEnd(axis.Position(fromEnd ? from.End : from.Start), band.Center(fromRow), fromEnd ? 1 : -1);
        var entry = new GanttArrowEnd(axis.Position(toStart ? to.Start : to.End), band.Center(toRow), toStart ? 1 : -1);
        var path = GanttArrow.Path(exit, entry, gap, band.Step);

        builder.Path(path, ChartColor.Axis, stroke: ChartStroke.Hairline, title: DescribeDependency(dependency));
        builder.Glyph(entry.X, entry.Y, toStart ? Icon.Caret_Right_Fill : Icon.Caret_Left_Fill, ChartColor.Axis, 10);
        arrows.Add((dependency, path));
    }

    private void AddTask(ChartSceneBuilder builder, GanttTimeline axis, GanttNode node, double center, double thickness)
    {
        var color = ChartColor.Series(node.Task.ColorIndex ?? 0);
        var x1 = axis.Position(node.Start);
        var x2 = axis.Position(node.End);

        if (node.IsMilestone)
        {
            builder.Glyph(x1, center, Icon.Diamond_Fill, color, 14);
        }
        else if (node.IsSummary)
        {
            AddSummary(builder, axis, node, color, center, thickness);
        }
        else if (node.Progress is { } progress)
        {
            var top = center - thickness / 2;
            builder.Add(new ChartRect(x1, top, x2 - x1, thickness, color, ChartCorner.All, 0.35));
            if (progress > 0)
            {
                builder.Add(new ChartRect(x1, top, (x2 - x1) * progress, thickness, color, progress >= 1 ? ChartCorner.All : ChartCorner.Left, Gap: false));
            }
        }
        else
        {
            builder.Bar(center, thickness, x1, x2, color, ChartCorner.All);
        }

        if (ShowTaskLabels)
        {
            AddLabel(builder, node, x1, x2, center);
        }
    }

    /// <summary>
    /// Draws a summary as a slim bar spanning its children, filled as far as the summary's progress.
    /// </summary>
    private static void AddSummary(ChartSceneBuilder builder, GanttTimeline axis, GanttNode node, ChartColor color, double center, double thickness)
    {
        var x1 = axis.Position(node.Start);
        var x2 = axis.Position(node.End);
        var height = thickness * 0.6;
        var top = center - height / 2;

        builder.Add(new ChartRect(x1, top, x2 - x1, height, color, ChartCorner.All, node.Progress is null ? 1 : 0.35));
        if (node.Progress is > 0 and var progress)
        {
            builder.Add(new ChartRect(x1, top, (x2 - x1) * progress, height, color, progress >= 1 ? ChartCorner.All : ChartCorner.Left, Gap: false));
        }
    }

    /// <summary>
    /// Writes a task's name after its bar, or before it when there is more room there, so a task near the end of
    /// the plot keeps its name inside the chart. A name too long for either side is cut short.
    /// </summary>
    private static void AddLabel(ChartSceneBuilder builder, GanttNode node, double x1, double x2, double center)
    {
        var offset = node.IsMilestone ? 10 : 6;
        var after = 100 - x2 >= x1;
        builder.Add(after
            ? new ChartText(x2, center, node.Task.Label, ChartAnchor.Start, ChartBaseline.Middle, ChartTextTone.Primary, offset, MaxWidth: 100 - x2)
            : new ChartText(x1, center, node.Task.Label, ChartAnchor.End, ChartBaseline.Middle, ChartTextTone.Primary, -offset, MaxWidth: x1));
    }

    private List<ChartDatumRow> DescribeTask(GanttNode node)
    {
        List<ChartDatumRow> rows = node.IsMilestone
            ? [new(localizer[TwStrings.Chart_Date], FormatDate(node.Start))]
            : [new(effectiveStartLabel, FormatDate(node.Start)), new(effectiveEndLabel, FormatDate(node.End))];

        if (node.Progress is { } progress)
        {
            rows.Add(new(localizer[TwStrings.Chart_Done], FormatPercent(progress)));
        }

        return rows;
    }

    private string DescribeDependency(ChartTaskDependency dependency)
    {
        var from = plan.Find(dependency.FromId)?.Task.Label ?? dependency.FromId;
        var to = plan.Find(dependency.ToId)?.Task.Label ?? dependency.ToId;
        var key = dependency.Type switch
        {
            ChartDependencyType.StartToStart => TwStrings.Chart_StartToStart,
            ChartDependencyType.FinishToFinish => TwStrings.Chart_FinishToFinish,
            ChartDependencyType.StartToFinish => TwStrings.Chart_StartToFinish,
            _ => TwStrings.Chart_FinishToStart
        };
        return localizer[key, from, to];
    }

    private void AddLegend(ChartSceneBuilder builder, IReadOnlyList<GanttNode> rows, bool drewToday, bool shadedNonWorking)
    {
        if (rows.Any(node => node.IsSummary))
        {
            builder.Legend(localizer[TwStrings.Chart_Summary], ChartColor.Series(0));
        }

        if (rows.Any(node => node.IsMilestone))
        {
            builder.Legend(localizer[TwStrings.Chart_Milestone], ChartColor.Series(0));
        }

        if (drewToday)
        {
            builder.Legend(localizer[TwStrings.Chart_Today], ChartColor.Negative, todaySeries, IsSeriesHidden(todaySeries));
        }

        if (shadedNonWorking)
        {
            builder.Legend(localizer[TwStrings.Chart_NonWorking], ChartColor.Track);
        }
    }

    private void AddTable(ChartSceneBuilder builder)
    {
        var hasProgress = plan.All.Any(node => node.Progress.HasValue);
        List<string> headers = [effectiveLabelHeader, effectiveStartLabel, effectiveEndLabel];
        if (hasProgress)
        {
            headers.Add(localizer[TwStrings.Chart_Done]);
        }

        // Every task, including those under collapsed summaries: the table is the complete alternative to the plot.
        builder.Table = new ChartTable(headers, [.. plan.All.Select(node =>
        {
            List<string> row = [node.Task.Label, FormatDate(node.Start), FormatDate(node.End)];
            if (hasProgress)
            {
                row.Add(node.Progress is { } progress ? FormatPercent(progress) : string.Empty);
            }

            return (IReadOnlyList<string>)row;
        })]);
    }

    private string FormatDate(DateTime date) => date.ToString(effectiveDateFormat, culture);

    private string FormatPercent(double fraction) => fraction.ToString("P0", culture);

    private string FormatSlot(DateTime start, int slot) => zoom switch
    {
        GanttZoom.Hour => start.ToString("HH:mm", culture),
        GanttZoom.Day => start.ToString(start.Day == 1 || slot == 0 ? "d MMM" : "%d", culture),
        GanttZoom.Week => localizer[TwStrings.Chart_WeekNumber, ISOWeek.GetWeekOfYear(start.AddDays(3))],
        GanttZoom.Month => start.ToString("MMM yyyy", culture),
        GanttZoom.Quarter => $"{localizer[TwStrings.Chart_QuarterNumber, (start.Month - 1) / 3 + 1]} {start.Year.ToString(culture)}",
        _ => start.Year.ToString(culture)
    };
}
