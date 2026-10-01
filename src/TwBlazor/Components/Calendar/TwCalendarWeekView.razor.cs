// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using TwBlazor.Configuration.Components;
using TwBlazor.Utilities;

namespace TwBlazor.Components;

/// <summary>
/// Renders a Monday-Sunday week of <see cref="TwCalendarDayColumn{T}"/>s side by side, with a single
/// shared hourly time-label gutter and a row of day headers (e.g. "Mon 16").
/// </summary>
/// <typeparam name="T">The type of <see cref="Schedule{T}.Value"/> for the events shown.</typeparam>
public partial class TwCalendarWeekView<T> : TwBlazorComponentBase, IAsyncDisposable
{
    [Inject] public IJSRuntime JSRuntime { get; set; } = default!;

    private TwCalendarTheme theme => options.Theme.Components.Require<TwCalendarTheme>();

    private const int hoursPerDay = 24;
    private const int daysPerWeek = 7;

    private static readonly TimeSpan _nowTickInterval = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Any date within the week to display - the week actually shown is the Monday-Sunday range
    /// containing it (see <see cref="DateHelpers.GetStartOfWeek"/>).
    /// </summary>
    [Parameter, EditorRequired] public DateTime Date { get; set; }

    /// <summary>
    /// The full event list; filtered per day before being passed to each day's
    /// <see cref="TwCalendarDayColumn{T}"/>.
    /// </summary>
    [Parameter] public List<Schedule<T>> Schedules { get; set; } = [];

    /// <summary>
    /// When <see langword="true"/>, empty slots are clickable to start creating a new event.
    /// </summary>
    [Parameter] public bool Editable { get; set; }

    /// <summary>
    /// Optional custom rendering for an event's chip content.
    /// </summary>
    [Parameter] public RenderFragment<Schedule<T>>? EventContentTemplate { get; set; }

    /// <summary>
    /// Invoked with the clicked slot's start date/time when an empty slot is activated while
    /// <see cref="Editable"/>.
    /// </summary>
    [Parameter] public EventCallback<DateTime> OnSlotClick { get; set; }

    /// <summary>
    /// Invoked with the event when one of its chips is activated.
    /// </summary>
    [Parameter] public EventCallback<Schedule<T>> OnEventClick { get; set; }

    /// <summary>
    /// Invoked with a day's date when its header is clicked - typically wired up by
    /// <see cref="TwCalendar{T}"/> to switch to the Day view for that date.
    /// </summary>
    [Parameter] public EventCallback<DateTime> OnDayHeaderClick { get; set; }

    /// <summary>
    /// Invoked with an event when dragging it starts. See <see cref="TwCalendar{T}"/>'s
    /// <c>OnEventDragStart</c>/<c>OnEventDrop</c> remarks.
    /// </summary>
    [Parameter] public EventCallback<Schedule<T>> OnEventDragStart { get; set; }

    /// <summary>
    /// Invoked with a slot's start date/time when a dragged event is dropped there. Which day it
    /// lands on falls naturally out of which column the drop happened in - see
    /// <see cref="TwCalendarDayColumn{T}"/>, which each column instance already scopes to its own
    /// <see cref="TwCalendarDayColumn{T}.Date"/> - so this view doesn't need any extra cross-day logic.
    /// </summary>
    [Parameter] public EventCallback<DateTime> OnEventDrop { get; set; }

    /// <summary>
    /// Invoked with the start of the slot a dragged event is hovering. See <see cref="TwCalendarDayColumn{T}.OnEventDragOver"/>.
    /// </summary>
    [Parameter] public EventCallback<DateTime> OnEventDragOver { get; set; }

    /// <summary>
    /// Invoked when a drag ends, whether or not it was dropped on a valid target.
    /// </summary>
    [Parameter] public EventCallback OnEventDragEnd { get; set; }

    /// <summary>
    /// The event being dragged once the drag is live; see <see cref="TwCalendarDayColumn{T}.DraggedEvent"/>.
    /// </summary>
    [Parameter] public Schedule<T>? DraggedEvent { get; set; }

    /// <summary>
    /// Where <see cref="DraggedEvent"/> would land if released now; only the column for that day draws a placeholder.
    /// </summary>
    [Parameter] public DateTime? DropPreview { get; set; }

    /// <summary>
    /// The CSS <c>max-height</c> of the scrollable time grid - see <see cref="TwCalendar{T}.MaxHeight"/>.
    /// </summary>
    [Parameter] public string MaxHeight { get; set; } = "40rem";

    /// <summary>
    /// The time of day scrolled to when this view first mounts - see <see cref="TwCalendar{T}.ScrollToTime"/>.
    /// </summary>
    [Parameter] public TimeSpan ScrollToTime { get; set; } = TimeSpan.FromHours(8);

    private ElementReference scrollContainerRef;

    private DateTime weekStart => DateHelpers.GetStartOfWeek(Date);

    private IReadOnlyList<DateTime> weekDays => [.. Enumerable.Range(0, daysPerWeek).Select(i => weekStart.AddDays(i))];

    /// <summary>
    /// The timed events that touch <paramref name="day"/>, including ones that started the day before,
    /// which the day column clips to midnight. All-day and day-long events go in the strip above instead.
    /// </summary>
    private List<Schedule<T>> GetDayEvents(DateTime day) =>
        [.. Schedules.Where(e => !TwCalendarSpans.IsBanner(e) && TwCalendarSpans.OverlapsDay(e, day))];

    // items-start matters here for the same reason as TwCalendarDayView's identical property - without
    // it, flexbox's default align-items:stretch resizes the gutter/day columns to the (max-height-
    // clamped) scroll container's cross size instead of letting them keep their full content height.
    private string scrollContainerClasses => new ClassBuilder(options.Theme.Display.Flex)
        .AddClass(options.Theme.Flexbox.Align.Start)
        .AddClass(theme.ScrollContainer)
        .Build();

    private string scrollContainerStyle => $"max-height:{MaxHeight};";

    private string gutterClasses => new ClassBuilder(theme.TimeGutter).Build();

    private string GetGutterLabelClasses() => new ClassBuilder(theme.TimeGutterLabel).Build();

    private static string GetHourLabel(int hour) => DateTime.Today.AddHours(hour).ToString("h tt");

    /// <summary>
    /// The current time, refreshed every <see cref="_nowTickInterval"/> by <see cref="nowTimer"/> while
    /// this view is mounted - drives the gutter label's text and the indicator line/dot's position. The
    /// line always spans the full week (a "what time is it right now" reference bar, shown regardless of
    /// which week is displayed); the dot additionally marks whichever column is <see cref="todayColumnIndex"/>,
    /// when today happens to fall within the displayed week.
    /// </summary>
    private DateTime now = DateTime.Now;

    private System.Threading.Timer? nowTimer;

    private string nowLineContainerClasses => new ClassBuilder(options.Theme.Position.Relative)
        .AddClass(options.Theme.Display.Flex)
        .AddClass(options.Theme.Flexbox.Flex1)
        .Build();

    private string nowOffsetStyle => $"top:{TwCalendarTimeGrid.GetOffsetRem(now.TimeOfDay.TotalMinutes).ToString(System.Globalization.CultureInfo.InvariantCulture)}rem;";

    /// <summary>
    /// The 0-based (Monday = 0) index of today's column within <see cref="weekDays"/>, or
    /// <see langword="null"/> when today isn't in the displayed week.
    /// </summary>
    private int? todayColumnIndex
    {
        get
        {
            var index = weekDays.ToList().FindIndex(d => d.Date == DateTime.Today);
            return index >= 0 ? index : null;
        }
    }

    private static string GetDotOffsetStyle(int columnIndex) =>
        $"left:{(100m / daysPerWeek * columnIndex).ToString(System.Globalization.CultureInfo.InvariantCulture)}%;";

    private void OnNowTick(object? state)
    {
        now = DateTime.Now;
        _ = InvokeAsync(StateHasChanged);
    }

    private string dayHeaderClasses => new ClassBuilder(theme.DayColumnHeader)
        .AddClass(options.Theme.Sizing.FullWidth)
        .AddClass(options.Theme.Interaction.PointerCursor, OnDayHeaderClick.HasDelegate)
        .Build();

    private string dayColumnClasses => new ClassBuilder(options.Theme.Flexbox.Flex1)
        .AddClass(theme.DayColumnDivider)
        .Build();

    private Task OnDayHeaderClickedAsync(DateTime day) => OnDayHeaderClick.InvokeAsync(day);

    /// <summary>
    /// Component references to each of the week's seven <see cref="TwCalendarDayColumn{T}"/> instances,
    /// indexed 0 (Monday) to 6 (Sunday), so <see cref="OnWrapperKeyDownAsync"/> can move focus into a
    /// neighboring column.
    /// </summary>
    private readonly TwCalendarDayColumn<T>?[] _columnRefs = new TwCalendarDayColumn<T>?[daysPerWeek];

    private int focusedColumn;
    private int focusedRow;

    /// <summary>
    /// Tracks which column/row currently holds keyboard focus, reported by each day column's own
    /// <see cref="TwCalendarDayColumn{T}.OnSlotFocused"/>.
    /// </summary>
    private void OnColumnSlotFocused(int columnIndex, int slotIndex)
    {
        focusedColumn = columnIndex;
        focusedRow = slotIndex;
    }

    /// <summary>
    /// Continues ArrowLeft/ArrowRight movement (which each <see cref="TwCalendarDayColumn{T}"/>
    /// deliberately leaves unhandled - see its own remarks) across the week's day columns, moving
    /// focus to the neighboring column at the same time row.
    /// </summary>
    private async Task OnWrapperKeyDownAsync(KeyboardEventArgs e)
    {
        int? target = e.Key switch
        {
            "ArrowRight" => focusedColumn + 1,
            "ArrowLeft" => focusedColumn - 1,
            _ => null
        };

        if (target is null)
        {
            return;
        }

        var clamped = Math.Clamp(target.Value, 0, daysPerWeek - 1);
        if (clamped == focusedColumn)
        {
            return;
        }

        focusedColumn = clamped;
        var column = _columnRefs[focusedColumn];
        if (column is not null)
        {
            await column.FocusSlotAsync(focusedRow);
        }
    }

    /// <summary>
    /// Scrolls to <see cref="ScrollToTime"/> once, the first time this view mounts - see the identical
    /// reasoning on <see cref="TwCalendarDayView{T}.OnAfterRenderAsync"/>.
    /// </summary>
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);

        if (firstRender)
        {
            nowTimer = new System.Threading.Timer(OnNowTick, null, _nowTickInterval, _nowTickInterval);

            var fraction = Math.Clamp(ScrollToTime.TotalMinutes / (24 * 60), 0, 1);
            try
            {
                await JSRuntime.InvokeVoidAsync("twCalendar.scrollToFraction", scrollContainerRef, fraction);
            }
            catch (JSDisconnectedException)
            {
                // The circuit disconnected before the script could run; nothing to scroll.
            }
        }
    }

    public ValueTask DisposeAsync()
    {
        nowTimer?.Dispose();
        GC.SuppressFinalize(this);
        return ValueTask.CompletedTask;
    }
}
