// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using Microsoft.AspNetCore.Components;
using TwBlazor.Configuration.Components;
using TwBlazor.Utilities;

namespace TwBlazor.Components;

/// <summary>
/// The strip above the Day and Week time grids that holds all-day events and events lasting a day or
/// longer (see <see cref="TwCalendarSpans.IsBanner{T}"/>), the way Microsoft Teams does. Each event is
/// one bar that spans every day it covers, so a three-day event is a single bar across three columns.
/// Renders nothing when no event needs it.
/// </summary>
/// <typeparam name="T">The type of <see cref="Schedule{T}.Value"/> for the events shown.</typeparam>
public partial class TwCalendarAllDayRow<T> : TwBlazorComponentBase
{
    private TwCalendarTheme theme => options.Theme.Components.Require<TwCalendarTheme>();

    /// <summary>
    /// The days the strip's columns stand for: one for the Day view, seven for the Week view.
    /// </summary>
    [Parameter, EditorRequired] public IReadOnlyList<DateTime> Days { get; set; } = [];

    /// <summary>
    /// The full event list; only events that need the strip are drawn.
    /// </summary>
    [Parameter] public List<Schedule<T>> Schedules { get; set; } = [];

    /// <summary>
    /// When <see langword="true"/>, non-read-only events can be dragged to another day.
    /// </summary>
    [Parameter] public bool Editable { get; set; }

    /// <summary>
    /// Invoked with the event when its bar is activated.
    /// </summary>
    [Parameter] public EventCallback<Schedule<T>> OnEventClick { get; set; }

    /// <summary>
    /// Invoked with an event when dragging it starts.
    /// </summary>
    [Parameter] public EventCallback<Schedule<T>> OnEventDragStart { get; set; }

    /// <summary>
    /// Invoked with the day a dragged event is hovering.
    /// </summary>
    [Parameter] public EventCallback<DateTime> OnEventDragOver { get; set; }

    /// <summary>
    /// Invoked when a drag ends, whether or not it was dropped on a valid target.
    /// </summary>
    [Parameter] public EventCallback OnEventDragEnd { get; set; }

    /// <summary>
    /// Invoked with a day when a dragged event is dropped on it.
    /// </summary>
    [Parameter] public EventCallback<DateTime> OnEventDrop { get; set; }

    /// <summary>
    /// The event being dragged once the drag is live; see <see cref="TwCalendarDayColumn{T}.DraggedEvent"/>.
    /// Only all-day and multi-day events can be dropped here.
    /// </summary>
    [Parameter] public Schedule<T>? DraggedEvent { get; set; }

    /// <summary>
    /// The day <see cref="DraggedEvent"/> would land on if released now.
    /// </summary>
    [Parameter] public DateTime? DropPreview { get; set; }

    private IReadOnlyList<TwCalendarSpans.Segment<T>> segments =>
        Days.Count == 0 ? [] : TwCalendarSpans.GetSegments(Schedules.Where(TwCalendarSpans.IsBanner), Days[0], Days.Count);

    private int laneCount => Math.Max(1, segments.Count == 0 ? 1 : segments.Max(s => s.Lane) + 1);

    private bool canDrop => Editable && DraggedEvent is not null && TwCalendarSpans.IsBanner(DraggedEvent);

    private bool IsDraggable(Schedule<T> evt) => Editable && !evt.ReadOnly;

    private string rowClasses => new ClassBuilder(options.Theme.Display.Flex).AddClass(theme.AllDayRow).AddClass(Class).Build();

    private string labelClasses => new ClassBuilder(theme.AllDayLabel).Build();

    private string gridClasses => new ClassBuilder(options.Theme.Display.Grid)
        .AddClass(options.Theme.Position.Relative)
        .AddClass(options.Theme.Flexbox.Flex1)
        .AddClass(theme.AllDayGrid)
        .Build();

    private string gridStyle => $"grid-template-columns:repeat({Days.Count},minmax(0,1fr));";

    private string cellClasses => new ClassBuilder(theme.DayColumnDivider).Build();

    private string nameClasses => new ClassBuilder(theme.MonthEventName).Build();

    private string GetChipClasses(TwCalendarSpans.Segment<T> segment) => new ClassBuilder(theme.MonthEventRow)
        .AddClass(theme.SegmentSpan)
        .AddClass(theme.AllDayChipInset)
        .AddClass(theme.EventChipDraggable, IsDraggable(segment.Event))
        .AddClass(theme.SegmentContinuesBefore, segment.ContinuesBefore)
        .AddClass(theme.SegmentContinuesAfter, segment.ContinuesAfter)
        .AddClass(options.Theme.Interaction.PointerEventsNone, DraggedEvent is not null)
        .Build();

    private static string GetChipStyle(TwCalendarSpans.Segment<T> segment) =>
        $"grid-column:{segment.StartColumn + 1} / span {segment.Span};grid-row:{segment.Lane + 1};{TwCalendarColors.GetEventCardStyle(segment.Event.Color)}";

    private static string GetChipLabel(Schedule<T> evt) =>
        TwCalendarSpans.IsAllDay(evt)
            ? $"{evt.Name}, {TwCalendarEventDialog<T>.FormatRange(evt.DateTimeStart, evt.DateTimeEnd)}"
            : $"{evt.Name}, {evt.DateTimeStart:d MMM} to {evt.DateTimeEnd:d MMM}";

    private string placeholderClasses => new ClassBuilder(theme.MonthEventRow)
        .AddClass(theme.DropPlaceholder)
        .AddClass(theme.AllDayChipInset)
        .AddClass(options.Theme.Interaction.PointerEventsNone)
        .Build();

    /// <summary>
    /// The inline style positioning the landing placeholder across the days the dragged event would
    /// cover (clipped to the strip's last day), or <see langword="null"/> when there is nothing to show.
    /// </summary>
    private string? placeholderStyle
    {
        get
        {
            if (!canDrop || DropPreview is not { } preview)
            {
                return null;
            }

            var index = Days.ToList().FindIndex(d => d.Date == preview.Date);
            if (index < 0)
            {
                return null;
            }

            var span = Math.Min(TwCalendarSpans.GetDayCount(DraggedEvent!), Days.Count - index);
            return $"grid-column:{index + 1} / span {span};grid-row:1;";
        }
    }

    private Task OnChipDragStartAsync(Schedule<T> evt) =>
        IsDraggable(evt) ? OnEventDragStart.InvokeAsync(evt) : Task.CompletedTask;

    private Task OnChipDragEndAsync() => OnEventDragEnd.InvokeAsync();

    private Task OnCellDragEnterAsync(DateTime day) => canDrop ? OnEventDragOver.InvokeAsync(day) : Task.CompletedTask;

    private Task OnCellDropAsync(DateTime day) => canDrop ? OnEventDrop.InvokeAsync(day) : Task.CompletedTask;
}
