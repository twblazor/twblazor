// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using System.Globalization;
using TwBlazor.Configuration.Components;
using TwBlazor.Utilities;

namespace TwBlazor.Components;

/// <summary>
/// Renders a full month grid (5 or 6 weeks, however many the month actually spans - see
/// <see cref="GetWeekRows"/>) with each day's events shown as small chips, following the same
/// <c>&lt;table role="grid"&gt;</c>/roving-tabindex/arrow-key pattern as
/// <see cref="TwBlazor.Components.DatePicker.TwDatePickerDayView"/>.
/// </summary>
/// <typeparam name="T">The type of <see cref="Schedule{T}.Value"/> for the events shown.</typeparam>
public partial class TwCalendarMonthView<T> : TwBlazorComponentBase, IAsyncDisposable
{
    [Inject] public IJSRuntime JSRuntime { get; set; } = default!;

    private TwCalendarTheme theme => options.Theme.Components.Require<TwCalendarTheme>();

    private const int maxVisiblePerCell = 3;

    /// <summary>
    /// Any date within the month to display.
    /// </summary>
    [Parameter, EditorRequired] public DateTime Date { get; set; }

    /// <summary>
    /// The full event list; filtered per day before being rendered in each cell.
    /// </summary>
    [Parameter] public List<Schedule<T>> Schedules { get; set; } = [];

    /// <summary>
    /// When <see langword="true"/>, non-read-only events can be dragged between days (their time-of-day
    /// and duration stay unchanged - only the date moves).
    /// </summary>
    [Parameter] public bool Editable { get; set; }

    /// <summary>
    /// Optional custom rendering for an event's chip content; defaults to just <see cref="Schedule{T}.Name"/>.
    /// </summary>
    [Parameter] public RenderFragment<Schedule<T>>? EventContentTemplate { get; set; }

    /// <summary>
    /// Invoked with a day's date when its day number or "+N more" overflow label is clicked -
    /// typically wired up by <see cref="TwCalendar{T}"/> to switch to the Day view for that date.
    /// </summary>
    [Parameter] public EventCallback<DateTime> OnDayClick { get; set; }

    /// <summary>
    /// Invoked with the event when one of its chips is clicked.
    /// </summary>
    [Parameter] public EventCallback<Schedule<T>> OnEventClick { get; set; }

    /// <summary>
    /// Invoked with an event when dragging it starts. See <see cref="TwCalendar{T}"/>'s
    /// <c>OnEventDragStart</c>/<c>OnEventDrop</c> remarks.
    /// </summary>
    [Parameter] public EventCallback<Schedule<T>> OnEventDragStart { get; set; }

    /// <summary>
    /// Invoked with a day's date (time-of-day irrelevant - <see cref="TwCalendar{T}"/> keeps the
    /// dragged event's original time-of-day) when it's dropped on that day's cell.
    /// </summary>
    [Parameter] public EventCallback<DateTime> OnEventDrop { get; set; }

    /// <summary>
    /// A day whose cell pulses, used to draw the eye to today after "Today" is pressed. Leave
    /// <see langword="null"/> for no highlight.
    /// </summary>
    [Parameter] public DateTime? HighlightDate { get; set; }

    /// <summary>
    /// Invoked with the hovered day when a dragged event enters one of its cells.
    /// </summary>
    [Parameter] public EventCallback<DateTime> OnEventDragOver { get; set; }

    /// <summary>
    /// Invoked when a drag ends, whether or not it was dropped on a valid target.
    /// </summary>
    [Parameter] public EventCallback OnEventDragEnd { get; set; }

    /// <summary>
    /// The event being dragged once the drag is live; a placeholder for it is drawn in the hovered day.
    /// </summary>
    [Parameter] public Schedule<T>? DraggedEvent { get; set; }

    /// <summary>
    /// The day <see cref="DraggedEvent"/> would land on if released now.
    /// </summary>
    [Parameter] public DateTime? DropPreview { get; set; }

    private static IReadOnlyList<string> weekdayHeaders
    {
        get
        {
            var format = CultureInfo.CurrentCulture.DateTimeFormat;
            // Monday-first, matching DateHelpers.GetStartOfWeek's convention throughout TwCalendar.
            return [.. Enumerable.Range(1, 7).Select(i => format.ShortestDayNames[i % 7])];
        }
    }

    /// <summary>
    /// Splits the visible grid (the Monday-Sunday weeks spanning the displayed month - 5 or 6 rows
    /// depending on the month, never a hardcoded count) into rows of 7 real dates, including the
    /// lead-in/trailing days from the previous/next month.
    /// </summary>
    private List<List<DateTime>> GetWeekRows()
    {
        var firstOfMonth = new DateTime(Date.Year, Date.Month, 1);
        var daysInMonth = DateTime.DaysInMonth(Date.Year, Date.Month);
        var lastOfMonth = new DateTime(Date.Year, Date.Month, daysInMonth);

        var gridStart = DateHelpers.GetStartOfWeek(firstOfMonth);
        var gridEnd = DateHelpers.GetStartOfWeek(lastOfMonth).AddDays(6);

        var days = new List<DateTime>();
        for (var day = gridStart; day <= gridEnd; day = day.AddDays(1))
        {
            days.Add(day);
        }

        var rows = new List<List<DateTime>>();
        for (var i = 0; i < days.Count; i += 7)
        {
            rows.Add(days.GetRange(i, 7));
        }

        return rows;
    }

    /// <summary>
    /// The events that start and end on <paramref name="day"/>, drawn as rows inside its cell. Events that
    /// cover more than one day are drawn as bars across the days instead - see <see cref="GetRowSegments"/>.
    /// </summary>
    private List<Schedule<T>> GetDayEvents(DateTime day) =>
        [.. Schedules.Where(e => e.DateTimeStart.DateTime.Date == day.Date && !TwCalendarSpans.IsMultiDay(e)).OrderBy(e => e.DateTimeStart)];

    /// <summary>
    /// The bars for events covering more than one day within one week row, packed into lanes.
    /// </summary>
    private IReadOnlyList<TwCalendarSpans.Segment<T>> GetRowSegments(IReadOnlyList<DateTime> week) =>
        TwCalendarSpans.GetSegments(Schedules.Where(TwCalendarSpans.IsMultiDay), week[0], week.Count);

    /// <summary>
    /// How many lanes of bars pass through the cell at <paramref name="column"/>: every lane up to the
    /// highest one used there gets a slot, a bar where one starts and an invisible spacer elsewhere, so
    /// the cell's own events sit below all of them.
    /// </summary>
    private static int GetLaneSlotCount(IReadOnlyList<TwCalendarSpans.Segment<T>> segments, int column)
    {
        var lanes = segments.Where(s => s.StartColumn <= column && column < s.StartColumn + s.Span).Select(s => s.Lane + 1);
        return lanes.DefaultIfEmpty(0).Max();
    }

    private static TwCalendarSpans.Segment<T>? GetSegmentStartingAt(IReadOnlyList<TwCalendarSpans.Segment<T>> segments, int column, int lane)
    {
        foreach (var segment in segments)
        {
            if (segment.Lane == lane && segment.StartColumn == column)
            {
                return segment;
            }
        }

        return null;
    }

    private string spacerClasses => new ClassBuilder(theme.SegmentSpacer).Build();

    private string GetSegmentClasses(TwCalendarSpans.Segment<T> segment) => new ClassBuilder(theme.MonthEventRow)
        .AddClass(theme.SegmentSpan)
        .AddClass(theme.EventChipDraggable, IsDraggable(segment.Event))
        .AddClass(theme.SegmentContinuesBefore, segment.ContinuesBefore)
        .AddClass(theme.SegmentContinuesAfter, segment.ContinuesAfter)
        .Build();

    /// <summary>
    /// The inline style stretching a bar across the cells it covers: each extra day adds a full cell
    /// width plus the gap between cells (the cell's horizontal padding, <c>p-1</c> in the default
    /// theme, plus its 1px border).
    /// </summary>
    private static string GetSegmentStyle(TwCalendarSpans.Segment<T> segment) =>
        $"width:calc({segment.Span * 100}% + {segment.Span - 1} * (0.5rem + 1px));{TwCalendarColors.GetEventCardStyle(segment.Event.Color)}";

    private static string GetSegmentLabel(Schedule<T> evt) =>
        $"{evt.Name}, {evt.DateTimeStart:d MMM} to {TwCalendarSpans.GetLastDay(evt):d MMM}";

    private bool IsCurrentMonth(DateTime day) => day.Month == Date.Month && day.Year == Date.Year;

    private string GetCellClasses(DateTime day) => new ClassBuilder(theme.MonthCell)
        .AddClass(theme.TodayHighlight, HighlightDate?.Date == day.Date)
        .AddClass(theme.MonthCellPrevNext, !IsCurrentMonth(day))
        .Build();

    private string GetDayNumberClasses(DateTime day) => new ClassBuilder(theme.MonthDayNumber)
        .AddClass(options.Theme.Interaction.PointerCursor)
        .Build();

    private string eventListClasses => new ClassBuilder(theme.MonthEventList).Build();

    private string GetEventRowClasses(Schedule<T> evt) => new ClassBuilder(theme.MonthEventRow)
        .AddClass(theme.EventChipDraggable, IsDraggable(evt))
        .Build();

    private static string GetEventRowStyle(Schedule<T> evt) => TwCalendarColors.GetEventCardStyle(evt.Color);

    private string nameClasses => new ClassBuilder(theme.MonthEventName).Build();

    private string timeClasses => new ClassBuilder(theme.MonthEventTime).Build();

    /// <summary>
    /// The event's start time (e.g. "9:00 AM"), shown after its name on the row's single truncated line.
    /// </summary>
    private static string GetStartTimeLabel(Schedule<T> evt) =>
        TwCalendarSpans.IsAllDay(evt) ? "All day" : evt.DateTimeStart.DateTime.ToString("h:mm tt");

    private string overflowLabelClasses => new ClassBuilder(theme.MonthOverflowLabel).Build();

    private bool IsDraggable(Schedule<T> evt) => Editable && !evt.ReadOnly;

    private string placeholderClasses => new ClassBuilder(theme.DropPlaceholder)
        .AddClass(options.Theme.Display.Flex)
        .AddClass(options.Theme.Flexbox.Align.Center)
        .AddClass(options.Theme.Sizing.FullWidth)
        .AddClass(options.Theme.Interaction.PointerEventsNone)
        .AddClass(options.Theme.Spacing.Padding.Sm)
        .Build();

    private bool IsDropTarget(DateTime day) =>
        Editable && DraggedEvent is not null && DropPreview is { } preview && preview.Date == day.Date;

    private Task OnChipDragEndAsync() => OnEventDragEnd.InvokeAsync();

    private Task OnDayDragEnterAsync(DateTime day) =>
        Editable ? OnEventDragOver.InvokeAsync(day) : Task.CompletedTask;

    private Task OnDayClickedAsync(DateTime day) => OnDayClick.InvokeAsync(day);

    private Task OnEventClickedAsync(Schedule<T> evt) => OnEventClick.InvokeAsync(evt);

    private Task OnChipDragStartAsync(Schedule<T> evt) =>
        IsDraggable(evt) ? OnEventDragStart.InvokeAsync(evt) : Task.CompletedTask;

    private Task OnDayDropAsync(DateTime day) =>
        Editable ? OnEventDrop.InvokeAsync(day) : Task.CompletedTask;

    // --- Roving-tabindex/arrow-key grid navigation, mirroring TwDatePickerDayView.OnGridKeyDown ---

    private ElementReference gridRef;
    private bool keydownGuardRegistered;

    private sealed class DayCellRef
    {
        public ElementReference Element;
    }

    private readonly Dictionary<int, DayCellRef> _dayCellRefs = [];
    private int focusedIndex;
    private DateTime trackedMonth = DateTime.MinValue;

    private DayCellRef GetDayCellRef(int index)
    {
        if (!_dayCellRefs.TryGetValue(index, out var cellRef))
        {
            cellRef = new DayCellRef();
            _dayCellRefs[index] = cellRef;
        }

        return cellRef;
    }

    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        // Only reset the roving tabindex when the displayed month actually changes (including on
        // first render) - not on every render, which would otherwise fight OnGridKeyDownAsync's own
        // updates to focusedIndex as the user arrows around within the same month.
        if (Date.Year != trackedMonth.Year || Date.Month != trackedMonth.Month)
        {
            trackedMonth = new DateTime(Date.Year, Date.Month, 1);
            var rows = GetWeekRows();
            var flattened = rows.SelectMany(r => r).ToList();
            focusedIndex = Math.Max(0, flattened.FindIndex(d => d.Date == Date.Date));
            _dayCellRefs.Clear();
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);

        if (firstRender)
        {
            try
            {
                await JSRuntime.InvokeVoidAsync("twTabs.registerKeydownGuard", gridRef);
                keydownGuardRegistered = true;
            }
            catch (JSDisconnectedException)
            {
                // The circuit disconnected before the script could run; nothing to register.
            }
        }
    }

    private async Task OnGridKeyDownAsync(KeyboardEventArgs e)
    {
        var rowStart = focusedIndex - (focusedIndex % 7);

        int? target = e.Key switch
        {
            "ArrowRight" => focusedIndex + 1,
            "ArrowLeft" => focusedIndex - 1,
            "ArrowDown" => focusedIndex + 7,
            "ArrowUp" => focusedIndex - 7,
            "Home" => rowStart,
            "End" => rowStart + 6,
            _ => null
        };

        if (target is null)
        {
            return;
        }

        var totalCells = GetWeekRows().Sum(r => r.Count);
        var clamped = Math.Clamp(target.Value, 0, totalCells - 1);
        if (clamped == focusedIndex)
        {
            return;
        }

        focusedIndex = clamped;
        StateHasChanged();

        if (_dayCellRefs.TryGetValue(focusedIndex, out var cellRef))
        {
            await cellRef.Element.FocusAsync();
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (!keydownGuardRegistered)
        {
            GC.SuppressFinalize(this);
            return;
        }

        try
        {
            await JSRuntime.InvokeVoidAsync("twTabs.unregisterKeydownGuard", gridRef);
        }
        catch (JSDisconnectedException)
        {
            // The circuit is already gone; nothing left to clean up.
        }
        catch (InvalidOperationException)
        {
            // JS interop unavailable during teardown (e.g. prerendering); safe to ignore.
        }
        finally
        {
            GC.SuppressFinalize(this);
        }
    }
}
