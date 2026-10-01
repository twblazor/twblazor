// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using TwBlazor.Configuration.Components;
using TwBlazor.Utilities;

namespace TwBlazor.Components;

/// <summary>
/// Renders a single day's time-slot grid (via <see cref="TwCalendarDayColumn{T}"/>) with an hourly
/// time-label gutter on the left, under an all-day strip (<see cref="TwCalendarAllDayRow{T}"/>).
/// </summary>
/// <typeparam name="T">The type of <see cref="Schedule{T}.Value"/> for the events shown.</typeparam>
public partial class TwCalendarDayView<T> : TwBlazorComponentBase, IAsyncDisposable
{
    [Inject] public IJSRuntime JSRuntime { get; set; } = default!;

    private TwCalendarTheme theme => options.Theme.Components.Require<TwCalendarTheme>();

    private const int hoursPerDay = 24;

    private static readonly TimeSpan _nowTickInterval = TimeSpan.FromSeconds(30);

    /// <summary>
    /// The day being displayed.
    /// </summary>
    [Parameter, EditorRequired] public DateTime Date { get; set; }

    /// <summary>
    /// The full event list; filtered down to <see cref="Date"/> before being passed to the
    /// underlying <see cref="TwCalendarDayColumn{T}"/>.
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
    /// Invoked with an event when dragging it starts. See <see cref="TwCalendar{T}"/>'s
    /// <c>OnEventDragStart</c>/<c>OnEventDrop</c> remarks.
    /// </summary>
    [Parameter] public EventCallback<Schedule<T>> OnEventDragStart { get; set; }

    /// <summary>
    /// Invoked with a slot's start date/time when a dragged event is dropped there.
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
    /// Where <see cref="DraggedEvent"/> would land if released now.
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

    /// <summary>
    /// The timed events that touch <see cref="Date"/>, including ones that started the day before, which
    /// the day column clips to midnight. All-day and day-long events go in the strip above instead.
    /// </summary>
    private List<Schedule<T>> dayEvents => [.. Schedules.Where(e => !TwCalendarSpans.IsBanner(e) && TwCalendarSpans.OverlapsDay(e, Date))];

    private IReadOnlyList<DateTime> allDayDays => [Date.Date];

    private string containerClasses => new ClassBuilder(options.Theme.Display.Flex)
        .AddClass(options.Theme.Flexbox.Col)
        .AddClass(Class)
        .Build();

    // items-start (rather than flexbox's default align-items:stretch) matters here: without it, the
    // gutter/day-column flex children get stretched to the scroll container's own (max-height-clamped)
    // cross size instead of keeping their full 144rem content height, which corrupts the scrollable
    // area's real height and the day column's own containing-block box for its absolutely-positioned
    // event chips - the same class of bug as https://github.com/philipwalton/flexbugs (a flex
    // container's cross-axis stretch fighting a max-height + overflow-y-auto on itself).
    private string scrollContainerClasses => new ClassBuilder(options.Theme.Flexbox.Flex1)
        .AddClass(options.Theme.Display.Flex)
        .AddClass(options.Theme.Flexbox.Align.Start)
        .AddClass(theme.ScrollContainer)
        .Build();

    private string scrollContainerStyle => $"max-height:{MaxHeight};";

    private string gutterClasses => new ClassBuilder(theme.TimeGutter).Build();

    private string GetGutterLabelClasses() => new ClassBuilder(theme.TimeGutterLabel).Build();

    private static string GetHourLabel(int hour) => DateTime.Today.AddHours(hour).ToString("h tt");

    /// <summary>
    /// The current time, refreshed every <see cref="_nowTickInterval"/> by <see cref="nowTimer"/> while
    /// this view is mounted - drives both the gutter label's text and the indicator line/dot's position.
    /// Shown regardless of which day <see cref="Date"/> is - it's a "what time is it right now" reference
    /// bar rather than something scoped to today specifically.
    /// </summary>
    private DateTime now = DateTime.Now;

    private System.Threading.Timer? nowTimer;

    private string nowLineContainerClasses => new ClassBuilder(options.Theme.Position.Relative)
        .AddClass(options.Theme.Flexbox.Flex1)
        .Build();

    private string nowOffsetStyle => $"top:{TwCalendarTimeGrid.GetOffsetRem(now.TimeOfDay.TotalMinutes).ToString(System.Globalization.CultureInfo.InvariantCulture)}rem;";

    private void OnNowTick(object? state)
    {
        now = DateTime.Now;
        _ = InvokeAsync(StateHasChanged);
    }

    /// <summary>
    /// Scrolls to <see cref="ScrollToTime"/> once, the first time this view mounts - not on every
    /// render, so a user's own subsequent scrolling (or navigating between dates without leaving the
    /// Day view) is never fought.
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
