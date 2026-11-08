// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using System.Globalization;
using TwBlazor.Charting;
using TwBlazor.Enums;
using TwBlazor.Localization;
using TwBlazor.Models;
using TwBlazor.Utilities;

namespace TwBlazor.Components;

public partial class TwGanttChart
{
    private static readonly GanttZoom[] _zoomLevels = [GanttZoom.Auto, GanttZoom.Hour, GanttZoom.Day, GanttZoom.Week, GanttZoom.Month, GanttZoom.Quarter, GanttZoom.Year];

    private DotNetObjectReference<TwGanttChart>? selfReference;
    private string? attachedPlotId;
    private bool scrollPending = true;

    [Inject] private IJSRuntime ganttJs { get; set; } = null!;

    /// <inheritdoc />
    protected override double? plotMinWidth => isScrolling ? timeline!.SlotCount * (double)(SlotWidth ?? GanttTimeline.DefaultSlotWidth(zoom)) : null;

    /// <inheritdoc />
    protected override double? plotHeight => RowHeight is { } height && plan.Visible.Count > 0 ? height * plan.Visible.Count : null;

    /// <inheritdoc />
    protected override string? yAxisClass => new ClassBuilder(ganttTheme.YAxis).AddClass(ganttTheme.YAxisScrolling, isScrolling).Build();

    /// <inheritdoc />
    protected override string? plotScrollerClass => ganttTheme.Scroller;

    /// <inheritdoc />
    protected override bool datumsActivatable => OnTaskClick.HasDelegate;

    /// <inheritdoc />
    protected override string? datumClass => datumsActivatable ? ganttTheme.Clickable : null;

    /// <inheritdoc />
    protected override Task OnDatumActivatedAsync(int index) =>
        index >= 0 && index < plan.Visible.Count ? OnTaskClick.InvokeAsync(plan.Visible[index].Task) : Task.CompletedTask;

    /// <summary>Shows the tasks under every summary.</summary>
    /// <returns>A task that completes once the change is reported.</returns>
    public Task ExpandAllAsync() => SetCollapsedAsync([]);

    /// <summary>Hides the tasks under every summary.</summary>
    /// <returns>A task that completes once the change is reported.</returns>
    public Task CollapseAllAsync() => SetCollapsedAsync(plan.SummaryKeys);

    /// <summary>Changes the zoom level and raises <see cref="ZoomChanged"/>.</summary>
    /// <param name="value">The new zoom level.</param>
    /// <returns>A task that completes once the change is reported.</returns>
    public async Task SetZoomAsync(GanttZoom value)
    {
        if (value == zoom)
        {
            return;
        }

        zoom = value;
        scrollPending = true;
        Refresh();
        await ZoomChanged.InvokeAsync(value);
    }

    /// <summary>Scrolls a zoomed-in plot to today.</summary>
    /// <returns>A task that completes once the plot has scrolled.</returns>
    public Task GoToTodayAsync() => ScrollToAsync(Today ?? DateTime.Now);

    /// <summary>
    /// Scrolls a zoomed-in plot so a date is near its start. Does nothing at <see cref="GanttZoom.Auto"/>, where
    /// the whole schedule is in view.
    /// </summary>
    /// <param name="value">The date to scroll to.</param>
    /// <returns>A task that completes once the plot has scrolled.</returns>
    public async Task ScrollToAsync(DateTime value)
    {
        if (!isScrolling || !isPlotVisible)
        {
            return;
        }

        await InvokeGanttScriptAsync("twGanttChart.scrollTo", scrollElement, timeline!.Position(value) / 100);
    }

    /// <inheritdoc />
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);

        if (!isPlotVisible)
        {
            attachedPlotId = null;
            return;
        }

        // The plot is drawn again after the table view, so the gestures attach to whichever plot is on screen.
        if (isInteractive && attachedPlotId != plotElement.Id)
        {
            selfReference ??= DotNetObjectReference.Create(this);
            attachedPlotId = plotElement.Id;
            await InvokeGanttScriptAsync("twGanttChart.attach", plotElement, selfReference);
        }
        else if (!isInteractive && attachedPlotId != null)
        {
            attachedPlotId = null;
            await InvokeGanttScriptAsync("twGanttChart.detach", plotElement);
        }

        if (scrollPending && isScrolling)
        {
            scrollPending = false;
            if (ScrollToToday && todayShown is { } today)
            {
                await ScrollToAsync(today);
            }
        }
    }

    /// <inheritdoc />
    protected override async ValueTask DisposeCoreAsync()
    {
        if (attachedPlotId != null)
        {
            attachedPlotId = null;
            await InvokeGanttScriptAsync("twGanttChart.detach", plotElement);
        }

        selfReference?.Dispose();
    }

    /// <summary>
    /// Takes the dates a bar was dragged to. Called by the gesture script; not for app code.
    /// </summary>
    /// <param name="key">The task's key.</param>
    /// <param name="kind">"move", "resize-start" or "resize-end".</param>
    /// <param name="startPosition">Where the bar now starts, from 0 to 100 across the plot.</param>
    /// <param name="endPosition">Where the bar now ends, from 0 to 100 across the plot.</param>
    /// <returns><see langword="true"/> if the change was accepted.</returns>
    [JSInvokable]
    public async Task<bool> CommitBarDrag(string key, string kind, double startPosition, double endPosition)
    {
        if (plan.Find(key) is not { } node || timeline is not { } axis)
        {
            return false;
        }

        DateTime Date(double position) => SnapToSlot ? axis.Snap(axis.At(position)) : axis.At(position);

        var (changeKind, start, end) = kind switch
        {
            "resize-start" => (ChartTaskChangeKind.ResizeStart, Min(Date(startPosition), node.Task.End), node.Task.End),
            "resize-end" => (ChartTaskChangeKind.ResizeEnd, node.Task.Start, Max(Date(endPosition), node.Task.Start)),
            _ => (ChartTaskChangeKind.Move, Date(startPosition), Date(startPosition) + (node.Task.End - node.Task.Start))
        };

        if (start == node.Task.Start && end == node.Task.End)
        {
            return false;
        }

        return await RaiseTaskChangeAsync(new ChartTaskChange { Kind = changeKind, Task = node.Task, Start = start, End = end, Progress = node.Task.Progress });
    }

    /// <summary>
    /// Takes the progress a bar's grip was dragged to. Called by the gesture script; not for app code.
    /// </summary>
    /// <param name="key">The task's key.</param>
    /// <param name="progress">The new progress, from 0 to 1.</param>
    /// <returns><see langword="true"/> if the change was accepted.</returns>
    [JSInvokable]
    public async Task<bool> CommitProgressDrag(string key, double progress)
    {
        if (plan.Find(key) is not { } node)
        {
            return false;
        }

        var value = Math.Round(Math.Clamp(progress, 0, 1), 2);
        return await RaiseTaskChangeAsync(new ChartTaskChange { Kind = ChartTaskChangeKind.Progress, Task = node.Task, Start = node.Task.Start, End = node.Task.End, Progress = value });
    }

    /// <summary>
    /// Takes a dependency drawn between two bars. Called by the gesture script; not for app code.
    /// </summary>
    /// <param name="fromKey">The key of the task the line was drawn from.</param>
    /// <param name="fromAnchor">"start" or "end": which end of that bar it was drawn from.</param>
    /// <param name="toKey">The key of the task the line was drawn to.</param>
    /// <param name="toAnchor">"start" or "end": which end of that bar it was drawn to.</param>
    /// <returns><see langword="true"/> if the dependency was accepted.</returns>
    [JSInvokable]
    public async Task<bool> CommitLink(string fromKey, string fromAnchor, string toKey, string toAnchor)
    {
        if (fromKey == toKey || plan.Find(fromKey) is not { } from || plan.Find(toKey) is not { } to)
        {
            return false;
        }

        var type = (fromAnchor == "start", toAnchor == "start") switch
        {
            (false, true) => ChartDependencyType.FinishToStart,
            (true, true) => ChartDependencyType.StartToStart,
            (false, false) => ChartDependencyType.FinishToFinish,
            _ => ChartDependencyType.StartToFinish
        };

        var request = new ChartDependencyRequest { Dependency = new ChartTaskDependency(fromKey, toKey, type), From = from.Task, To = to.Task };
        await OnDependencyCreate.InvokeAsync(request);
        return !request.Cancel;
    }

    private async Task<bool> RaiseTaskChangeAsync(ChartTaskChange change)
    {
        await OnTaskChange.InvokeAsync(change);
        return !change.Cancel;
    }

    private async Task ToggleAsync(string key)
    {
        var next = new HashSet<string>(collapsed, StringComparer.Ordinal);
        if (!next.Remove(key))
        {
            next.Add(key);
        }

        await SetCollapsedAsync(next);
    }

    private async Task SetCollapsedAsync(IEnumerable<string> keys)
    {
        var next = new HashSet<string>(keys, StringComparer.Ordinal);
        collapsed = next;
        lastCollapsedIds = next;
        Refresh();
        await CollapsedIdsChanged.InvokeAsync(next);
    }

    private async Task InvokeGanttScriptAsync(string identifier, params object?[] args)
    {
        try
        {
            await ganttJs.InvokeVoidAsync(identifier, args);
        }
        catch (JSDisconnectedException)
        {
            // The circuit disconnected before the script could run.
        }
        catch (InvalidOperationException)
        {
            // JS interop is not available (prerendering).
        }
    }

    private static DateTime Min(DateTime a, DateTime b) => a < b ? a : b;

    private static DateTime Max(DateTime a, DateTime b) => a > b ? a : b;

    private static string Number(double value) => Math.Round(value, 3).ToString(CultureInfo.InvariantCulture);

    #region Toolbar

    private bool hasTodayButton => isScrolling && todayShown.HasValue;

    /// <inheritdoc />
    protected override RenderFragment? toolbarContent =>
        ShowToolbar && plan.Visible.Count > 0 && (ShowZoomControls || plan.HasTree || hasTodayButton) ? RenderToolbar : null;

    private void RenderToolbar(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "div");
        builder.AddAttribute(1, "class", ganttTheme.Toolbar);

        if (ShowZoomControls)
        {
            builder.OpenComponent<TwToggleGroup<GanttZoom>>(2);
            builder.AddComponentParameter(3, nameof(TwToggleGroup<GanttZoom>.Value), zoom);
            builder.AddComponentParameter(4, nameof(TwToggleGroup<GanttZoom>.ValueChanged), EventCallback.Factory.Create<GanttZoom>(this, SetZoomAsync));
            builder.AddComponentParameter(5, nameof(TwToggleGroup<GanttZoom>.Size), (Size?)Size.Sm);
            builder.AddComponentParameter(6, nameof(AriaLabel), localizer[TwStrings.Chart_Zoom]);
            builder.AddComponentParameter(7, nameof(TwToggleGroup<GanttZoom>.ChildContent), (RenderFragment)(items =>
            {
                foreach (var level in _zoomLevels)
                {
                    items.OpenComponent<TwToggleGroupItem<GanttZoom>>(0);
                    items.SetKey(level);
                    items.AddComponentParameter(1, nameof(TwToggleGroupItem<GanttZoom>.Value), level);
                    items.AddComponentParameter(2, nameof(TwToggleGroupItem<GanttZoom>.Label), ZoomLabel(level));
                    items.CloseComponent();
                }
            }));
            builder.CloseComponent();
        }

        if (plan.HasTree)
        {
            RenderToolbarButton(builder, 10, localizer[TwStrings.Chart_ExpandAll], Icon.Arrows_Expand, ExpandAllAsync);
            RenderToolbarButton(builder, 20, localizer[TwStrings.Chart_CollapseAll], Icon.Arrows_Collapse, CollapseAllAsync);
        }

        if (hasTodayButton)
        {
            RenderToolbarButton(builder, 30, localizer[TwStrings.Chart_Today], Icon.Calendar_Event, GoToTodayAsync);
        }

        builder.CloseElement();
    }

    private void RenderToolbarButton(RenderTreeBuilder builder, int sequence, string label, Icon icon, Func<Task> onClick)
    {
        builder.OpenComponent<TwButton>(sequence);
        builder.AddComponentParameter(sequence + 1, nameof(TwButton.Label), label);
        builder.AddComponentParameter(sequence + 2, nameof(TwButton.StartIcon), (Icon?)icon);
        builder.AddComponentParameter(sequence + 3, nameof(TwButton.Variant), (ButtonVariant?)ButtonVariant.Text);
        builder.AddComponentParameter(sequence + 4, nameof(TwButton.Dense), true);
        builder.AddComponentParameter(sequence + 5, nameof(TwButton.OnClick), EventCallback.Factory.Create(this, onClick));
        builder.CloseComponent();
    }

    private string ZoomLabel(GanttZoom level) => localizer[level switch
    {
        GanttZoom.Hour => TwStrings.Chart_ZoomHour,
        GanttZoom.Day => TwStrings.Chart_ZoomDay,
        GanttZoom.Week => TwStrings.Chart_ZoomWeek,
        GanttZoom.Month => TwStrings.Chart_ZoomMonth,
        GanttZoom.Quarter => TwStrings.Chart_ZoomQuarter,
        GanttZoom.Year => TwStrings.Chart_ZoomYear,
        _ => TwStrings.Chart_ZoomAuto
    }];

    #endregion

    #region Task names

    /// <inheritdoc />
    protected override RenderFragment? RenderYTick(ChartTick tick, int index)
    {
        if (!plan.HasTree || index < 0 || index >= plan.Visible.Count)
        {
            return null;
        }

        var node = plan.Visible[index];
        return builder =>
        {
            builder.OpenElement(0, "span");
            builder.AddAttribute(1, "class", ganttTheme.Tick);
            builder.AddAttribute(2, "style", $"padding-inline-start:{node.Depth * 12}px");

            if (node.IsSummary)
            {
                builder.OpenElement(3, "button");
                builder.AddAttribute(4, "type", "button");
                builder.AddAttribute(5, "class", ganttTheme.Toggle);
                builder.AddAttribute(6, "aria-expanded", node.IsExpanded ? "true" : "false");
                builder.AddAttribute(7, "aria-label", localizer[node.IsExpanded ? TwStrings.Chart_CollapseTask : TwStrings.Chart_ExpandTask, node.Task.Label]);
                builder.AddAttribute(8, "onclick", EventCallback.Factory.Create<MouseEventArgs>(this, () => ToggleAsync(node.Key)));
                builder.OpenComponent<TwIcon>(9);
                builder.AddComponentParameter(10, nameof(TwIcon.Icon), Icon.Chevron_Right);
                builder.AddComponentParameter(11, nameof(Class), new ClassBuilder(ganttTheme.ToggleIcon).AddClass(ganttTheme.ToggleIconOpen, node.IsExpanded).Build());
                builder.CloseComponent();
                builder.CloseElement();
            }
            else
            {
                builder.OpenElement(12, "span");
                builder.AddAttribute(13, "class", ganttTheme.ToggleSpacer);
                builder.AddAttribute(14, "aria-hidden", "true");
                builder.CloseElement();
            }

            builder.OpenElement(15, "span");
            builder.AddAttribute(16, "class", ganttTheme.TickLabel);
            builder.AddContent(17, tick.Label);
            builder.CloseElement();
            builder.CloseElement();
        };
    }

    #endregion

    #region Drag overlay

    private bool hasLinkTargets => AllowLinking && OnDependencyClick.HasDelegate && arrows.Count > 0;

    /// <inheritdoc />
    protected override RenderFragment? plotOverlay => (isInteractive || hasLinkTargets) && timeline != null ? RenderOverlay : null;

    private void RenderOverlay(RenderTreeBuilder builder)
    {
        var axis = timeline!;
        var rows = plan.Visible;
        var band = new ChartBand(rows.Count);
        var thickness = Math.Min(band.Step * 0.5, 8);

        builder.OpenElement(0, "div");
        builder.AddAttribute(1, "class", ganttTheme.Overlay);
        builder.AddAttribute(2, "data-gantt-overlay", true);
        builder.AddAttribute(3, "data-ghost-class", ganttTheme.Ghost);
        builder.AddAttribute(4, "data-line-class", ganttTheme.LinkLine);

        for (var i = 0; i < rows.Count; i++)
        {
            var node = rows[i];
            var fixedDates = node.IsSummary && RollUpSummaries;
            var canMove = AllowDrag && !fixedDates;
            var canResize = AllowResize && !fixedDates && !node.IsMilestone;
            var canProgress = AllowProgressDrag && !fixedDates && !node.IsMilestone;
            var flags = string.Join(' ', new[] { canMove ? "move" : null, canResize ? "resize" : null, canProgress ? "progress" : null, AllowLinking ? "link" : null }.OfType<string>());
            if (flags.Length == 0)
            {
                continue;
            }

            var left = axis.Position(node.Start);
            var width = axis.Position(node.End) - left;
            var top = band.Center(i) - thickness / 2;

            // A milestone has no length, so its handle area is the diamond's own size.
            var box = node.IsMilestone
                ? $"left:calc({Number(left)}% - 7px);width:14px;top:{Number(band.Center(i))}%;height:14px;transform:translateY(-50%)"
                : $"left:{Number(left)}%;width:{Number(width)}%;top:{Number(top)}%;height:{Number(thickness)}%";

            builder.OpenElement(10, "div");
            builder.SetKey(node.Key);
            builder.AddAttribute(11, "class", ganttTheme.Bar);
            builder.AddAttribute(12, "style", box);
            builder.AddAttribute(13, "data-gantt-bar", true);
            builder.AddAttribute(14, "data-key", node.Key);
            builder.AddAttribute(15, "data-flags", flags);
            builder.AddAttribute(16, "data-left", Number(left));
            builder.AddAttribute(17, "data-width", Number(node.IsMilestone ? 0 : width));
            builder.AddAttribute(18, "data-progress", Number(node.Progress ?? 0));

            if (canResize)
            {
                RenderPart(builder, 20, $"{ganttTheme.Handle} {ganttTheme.HandleStart}");
                RenderPart(builder, 23, $"{ganttTheme.Handle} {ganttTheme.HandleEnd}");
            }

            if (canProgress)
            {
                RenderPart(builder, 26, ganttTheme.ProgressGrip, $"left:{Number((node.Progress ?? 0) * 100)}%");
            }

            if (AllowLinking)
            {
                RenderPart(builder, 29, $"{ganttTheme.LinkDot} {ganttTheme.LinkDotStart}");
                RenderPart(builder, 32, $"{ganttTheme.LinkDot} {ganttTheme.LinkDotEnd}");
            }

            builder.CloseElement();
        }

        if (hasLinkTargets)
        {
            builder.OpenElement(40, "svg");
            builder.AddAttribute(41, "class", ganttTheme.Links);
            builder.AddAttribute(42, "viewBox", "0 0 100 100");
            builder.AddAttribute(43, "preserveAspectRatio", "none");
            builder.AddAttribute(44, "aria-hidden", "true");
            builder.AddAttribute(45, "focusable", "false");
            foreach (var (dependency, path) in arrows)
            {
                builder.OpenElement(46, "path");
                builder.AddAttribute(47, "d", path);
                builder.AddAttribute(48, "class", ganttTheme.LinkHit);
                builder.AddAttribute(49, "stroke-width", "10");
                builder.AddAttribute(50, "vector-effect", "non-scaling-stroke");
                builder.AddAttribute(51, "data-gantt-link", true);
                builder.AddAttribute(52, "onclick", EventCallback.Factory.Create<MouseEventArgs>(this, () => OnDependencyClick.InvokeAsync(dependency)));
                builder.OpenElement(53, "title");
                builder.AddContent(54, DescribeDependency(dependency));
                builder.CloseElement();
                builder.CloseElement();
            }

            builder.CloseElement();
        }

        builder.CloseElement();
    }

    private static void RenderPart(RenderTreeBuilder builder, int sequence, string classes, string? style = null)
    {
        builder.OpenElement(sequence, "span");
        builder.AddAttribute(sequence + 1, "class", classes);
        builder.AddAttribute(sequence + 2, "style", style);
        builder.CloseElement();
    }

    #endregion
}
