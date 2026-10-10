// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using System.Globalization;
using TwBlazor.Configuration.Components;
using TwBlazor.Utilities;

namespace TwBlazor.Components.DatePicker;

public partial class TwDatePickerDayView : TwBlazorComponentBase, IAsyncDisposable
{
    [Inject] public IJSRuntime JSRuntime { get; set; } = default!;

    private TwDatePickerTheme theme => options.Theme.Components.Require<TwDatePickerTheme>();

    /// <summary>
    /// The month this grid displays (only <c>Year</c>/<c>Month</c> matter for that; <c>Day</c> is
    /// otherwise unused). Purely a display/navigation anchor - independent of which day, if any,
    /// is actually selected (see <see cref="SelectedDate"/>), since a caller can browse to a
    /// different month than the one containing its real selection without changing that selection.
    /// </summary>
    [Parameter] public DateTime Value { get; set; }
    [Parameter] public EventCallback<DateTime> ValueChanged { get; set; }

    /// <summary>
    /// The actually-selected date in single-select mode (<see cref="Range"/> not set), used only to
    /// decide which day cell (if any within the currently displayed month) renders as selected.
    /// <see langword="null"/> (the default) means nothing is selected, and is also what
    /// <see cref="TwBlazor.Components.TwDateRangePicker"/> leaves it as, since range mode drives its
    /// own highlighting entirely from <see cref="Range"/> instead.
    /// </summary>
    [Parameter] public DateTime? SelectedDate { get; set; }

    /// <summary>
    /// When set, switches the grid from single-day selection to range highlighting: days matching
    /// <c>Key</c> (start) / <c>Value</c> (end) render as range endpoints and days strictly between
    /// them render with <see cref="TwDatePickerTheme.RangeClass"/>. Left <see langword="null"/>
    /// (the default) for <see cref="TwBlazor.Components.TwDatePicker"/>'s single-date rendering;
    /// set by <see cref="TwBlazor.Components.TwDateRangePicker"/> - the outer <see cref="Nullable{T}"/>
    /// distinguishes "not in range mode at all" (<see langword="null"/>) from "range mode, nothing
    /// picked yet" (a value whose <c>Key</c>/<c>Value</c> are both themselves <see langword="null"/>).
    /// </summary>
    [Parameter] public KeyValuePair<DateTime?, DateTime?>? Range { get; set; }

    /// <summary>
    /// When <see langword="true"/>, renders a "Month Year" caption above the grid using
    /// <see cref="TwDatePickerTheme.RangeMonthCaptionClass"/>. Used by
    /// <see cref="TwBlazor.Components.TwDateRangePicker"/>, which shows two of these grids side by
    /// side and needs each to label its own month since a shared outer header only shows one title.
    /// </summary>
    [Parameter] public bool ShowMonthCaption { get; set; }

    /// <summary>
    /// The earliest selectable date (inclusive, date-only comparison). Days before this render
    /// disabled. <see langword="null"/> (the default) leaves the lower bound unrestricted.
    /// </summary>
    [Parameter] public DateTime? MinDate { get; set; }

    /// <summary>
    /// The latest selectable date (inclusive, date-only comparison). Days after this render
    /// disabled. <see langword="null"/> (the default) leaves the upper bound unrestricted.
    /// </summary>
    [Parameter] public DateTime? MaxDate { get; set; }

    private string gridClasses => new ClassBuilder("datepicker-grid")
        .AddClass(theme.DayGrid).Build();

    private string dayHeaderClasses => new ClassBuilder(theme.WeekdaysHeader).Build();

    private string monthCaptionClasses => new ClassBuilder(theme.RangeMonthCaptionClass).Build();

    private bool IsDayDisabled(DateTime date) =>
        (MinDate.HasValue && date.Date < MinDate.Value.Date) || (MaxDate.HasValue && date.Date > MaxDate.Value.Date);

    /// <summary>
    /// Determines whether <paramref name="date"/> falls strictly between <paramref name="range"/>'s
    /// <c>Key</c> (start) and <c>Value</c> (end) - exclusive of the endpoints themselves, which are
    /// rendered as range endpoints instead (see <see cref="GetDayAriaLabel"/>).
    /// </summary>
    private static bool IsStrictlyWithinRange(KeyValuePair<DateTime?, DateTime?> range, DateTime date) =>
        range.Key.HasValue && range.Value.HasValue &&
        date.Date > range.Key.Value.Date && date.Date < range.Value.Value.Date;

    /// <summary>
    /// Localized weekday column headers (abbreviation shown, full name in the <c>&lt;abbr&gt;</c>'s
    /// <c>title</c>), Sunday-first to match <see cref="GetWeekRows"/>'s column order. Sourced from
    /// <see cref="CultureInfo.CurrentCulture"/> instead of hardcoded English strings so the calendar
    /// reads correctly for other locales.
    /// </summary>
    private static IReadOnlyList<(string Abbreviation, string FullName)> weekdayHeaders
    {
        get
        {
            var format = CultureInfo.CurrentCulture.DateTimeFormat;
            var headers = new (string, string)[7];

            for (var day = 0; day < 7; day++)
            {
                headers[day] = (format.ShortestDayNames[day], format.DayNames[day]);
            }

            return headers;
        }
    }

    /// <summary>
    /// Reference to the &lt;table role="grid"&gt; element, used to register a native keydown guard
    /// that suppresses the browser's default scroll behavior for the grid navigation keys (arrow
    /// keys, Home, End) - the same generic guard used by <see cref="TwTabContainer"/>'s tablist.
    /// </summary>
    private ElementReference gridRef = default;

    private bool keydownGuardRegistered;

    /// <summary>
    /// Holds a settable <see cref="ElementReference"/> for one day-of-month button, referenced by
    /// day number via <see cref="_dayCellRefs"/> so <see cref="OnGridKeyDown"/> can move focus there
    /// after the roving tabindex changes. A plain class (rather than a raw <see cref="ElementReference"/>
    /// field per entry) so <c>@ref</c> has a stable settable target to bind to from within the
    /// day-cell loop, mirroring the pattern <see cref="TwTabContainer"/> uses for its tab buttons.
    /// </summary>
    private sealed class DayCellRef
    {
        public ElementReference Element = default;
    }

    private readonly Dictionary<int, DayCellRef> _dayCellRefs = [];

    /// <summary>
    /// The day-of-month (1-based) currently holding the grid's roving tabindex - i.e. the one
    /// focusable cell reachable by Tab. Arrow keys move this within the currently displayed month;
    /// it does not itself change the selected date (that still only happens via click, Enter, or
    /// Space on the focused cell).
    /// </summary>
    private int focusedDay;

    /// <summary>
    /// Raised when a keyboard move would leave the displayed month: an arrow key past the first or last
    /// day, or Page Up / Page Down (Shift for a year). Carries the date the user was heading for.
    /// </summary>
    [Parameter] public EventCallback<DateTime> NavigationRequested { get; set; }

    private bool pendingDayFocus;

    /// <summary>
    /// Moves the grid's roving tabindex to <paramref name="day"/> and focuses that day's button once it
    /// has rendered. Called by the owner after it has turned the calendar page.
    /// </summary>
    public void FocusDay(int day)
    {
        focusedDay = Math.Clamp(day, 1, DateTime.DaysInMonth(Value.Year, Value.Month));
        pendingDayFocus = true;
        StateHasChanged();
    }

    private int trackedYear = int.MinValue;
    private int trackedMonth = int.MinValue;

    private DayCellRef GetDayCellRef(int day)
    {
        if (!_dayCellRefs.TryGetValue(day, out var cellRef))
        {
            cellRef = new DayCellRef();
            _dayCellRefs[day] = cellRef;
        }

        return cellRef;
    }

    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        // Only reset the roving tabindex when the displayed month actually changes (including on
        // first render) - not on every render, which would otherwise fight OnGridKeyDown's own
        // updates to focusedDay as the user arrows around within the same month.
        if (Value.Year != trackedYear || Value.Month != trackedMonth)
        {
            trackedYear = Value.Year;
            trackedMonth = Value.Month;
            focusedDay = Value.Day;
            _dayCellRefs.Clear();
        }
    }

    /// <summary>
    /// Registers the same generic keydown guard <see cref="TwTabContainer"/> uses for its tablist,
    /// so ArrowUp/ArrowDown/ArrowLeft/ArrowRight/Home/End don't also scroll the page while
    /// navigating the day grid, without ever touching Tab's default behavior.
    /// </summary>
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);

        if (pendingDayFocus && _dayCellRefs.TryGetValue(focusedDay, out var pendingCell))
        {
            pendingDayFocus = false;
            await pendingCell.Element.FocusAsync();
        }

        if (firstRender)
        {
            try
            {
                await JSRuntime.InvokeVoidAsync("twTabs.registerKeydownGuard", gridRef, true);
                keydownGuardRegistered = true;
            }
            catch (JSDisconnectedException)
            {
                // The circuit disconnected before the script could run; nothing to register.
            }
        }
    }

    /// <summary>
    /// Implements the WAI-ARIA APG grid keyboard pattern for the day-of-month grid: ArrowRight/
    /// ArrowLeft move the roving tabindex by one day, ArrowDown/ArrowUp by one week, and Home/End
    /// jump to the first/last day of the current week row. Movement is clamped to the days in the
    /// currently displayed month - crossing into an adjacent month requires switching the calendar
    /// page first (via the header's Previous/Next controls), same as native OS date pickers.
    /// </summary>
    private async Task OnGridKeyDown(KeyboardEventArgs e)
    {
        var daysInMonth = DateTime.DaysInMonth(Value.Year, Value.Month);
        var focusedDate = new DateTime(Value.Year, Value.Month, focusedDay, 0, 0, 0, Value.Kind);
        var dayOfWeek = (int)focusedDate.DayOfWeek;

        // A move that leaves the displayed month is handed to whoever owns the calendar page, which turns
        // the page and then puts focus on the day the user was heading for. Home and End stay within the
        // week row, so they never leave the month.
        DateTime? leavingTo = e.Key switch
        {
            "ArrowRight" => focusedDate.AddDays(1),
            "ArrowLeft" => focusedDate.AddDays(-1),
            "ArrowDown" => focusedDate.AddDays(7),
            "ArrowUp" => focusedDate.AddDays(-7),
            "PageDown" => e.ShiftKey ? focusedDate.AddYears(1) : focusedDate.AddMonths(1),
            "PageUp" => e.ShiftKey ? focusedDate.AddYears(-1) : focusedDate.AddMonths(-1),
            _ => null
        };

        if (leavingTo is { } destination
            && (destination.Year != Value.Year || destination.Month != Value.Month)
            && NavigationRequested.HasDelegate)
        {
            await NavigationRequested.InvokeAsync(destination);
            return;
        }

        int? target = e.Key switch
        {
            "ArrowRight" => focusedDay + 1,
            "ArrowLeft" => focusedDay - 1,
            "ArrowDown" => focusedDay + 7,
            "ArrowUp" => focusedDay - 7,
            "Home" => focusedDay - dayOfWeek,
            "End" => focusedDay + (6 - dayOfWeek),
            _ => null
        };

        if (target is null)
        {
            return;
        }

        var clamped = Math.Clamp(target.Value, 1, daysInMonth);
        if (clamped == focusedDay)
        {
            return;
        }

        focusedDay = clamped;
        StateHasChanged();

        if (_dayCellRefs.TryGetValue(focusedDay, out var cellRef))
        {
            await cellRef.Element.FocusAsync();
        }
    }

    /// <summary>
    /// Unregisters the JS-side keydown guard registered for this grid.
    /// </summary>
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

    /// <summary>
    /// Gets the CSS classes for a previous-month lead-in day (the greyed-out, non-interactive
    /// cells before day 1). When it falls within the selected range - even though it's shown here
    /// only as a lead-in - it gets the same <see cref="TwDatePickerTheme.RangeClass"/> tint and
    /// merged-bar rounding (via <see cref="GetRangeAwareRoundedClass"/>) as an in-range day in the
    /// current month, with the default (unmuted) text color instead of <see cref="TwDatePickerTheme.PrevMonthClass"/>
    /// so it stays legible against the tint, so a range spanning the month boundary reads as one
    /// continuous bar instead of stopping dead at day 1 with a mismatched, still-individually-rounded pill.
    /// </summary>
    /// <remarks>
    /// Sizing/layout reuses <see cref="TwDatePickerTheme.ButtonClass"/> - the same classes a real
    /// day button gets - rather than its own hardcoded copy, so this cell can never drift out of
    /// sync with its real-day neighbors again the way its old hardcoded <c>h-9</c> (vs. buttons'
    /// <c>h-8</c>) did: invisible before merged-range styling existed, but immediately obvious once
    /// cells sit flush against each other with no gap to hide a size mismatch behind.
    /// </remarks>
    private string GetPrevDayClasses(bool isRangeStart, bool isRangeEnd, bool isInRange)
    {
        var isRangePart = isRangeStart || isRangeEnd || isInRange;
        return new ClassBuilder("day prev")
            .AddClass(options.Theme.Interaction.ReadonlyCursor)
            .AddClass(theme.ButtonClass)
            .AddClass(GetRangeAwareRoundedClass(isRangeStart, isRangeEnd, isInRange), isRangePart)
            .AddClass(options.Theme.Rounded.Lg, !isRangePart)
            .AddClass(theme.RangeClass, isRangePart)
            .AddClass(theme.PrevMonthClass, !isRangePart)
            .Build();
    }

    /// <summary>
    /// Gets the rounding class for a day button so a contiguous range reads as one merged bar
    /// instead of a row of individually-rounded pills: the start day rounds only its leading edge,
    /// the end day only its trailing edge, in-between days are square (no rounding, so they touch
    /// flush on both sides), a single-day range (start and end the same day) keeps full rounding
    /// since it isn't merging with anything, and outside range mode (or <see cref="Range"/> is
    /// <see langword="null"/>) every day keeps its normal full rounding - unchanged from before
    /// range support existed.
    /// </summary>
    private string GetRangeAwareRoundedClass(bool isRangeStart, bool isRangeEnd, bool isInRange) => (isRangeStart, isRangeEnd) switch
    {
        (true, true) => roundedBuilder.GetRounded(),
        (true, false) => roundedBuilder.GetRoundedStart(),
        (false, true) => roundedBuilder.GetRoundedEnd(),
        _ => isInRange ? string.Empty : roundedBuilder.GetRounded()
    };

    /// <summary>
    /// Gets the CSS classes for the buttons present in the dialog.
    /// </summary>
    /// <remarks>
    /// Class precedence is kept mutually exclusive per cell - only one background-color class is
    /// ever added - since two Tailwind utility classes both setting a background don't override in
    /// class-attribute order the way inline styles would; whichever is defined later in the
    /// compiled stylesheet wins regardless of attribute order, so stacking them here would make the
    /// visual result depend on Tailwind's internal ordering rather than this component's intent.
    /// </remarks>
    private string GetButtonClasses(bool isSelected, bool isToday, bool isRangeStart, bool isRangeEnd, bool isInRange, bool isDisabled) =>
        new ClassBuilder("day")
        .AddClass(options.Theme.Interaction.PointerCursor, !isDisabled)
        .AddClass(options.Theme.Interaction.DisabledOpacity, isDisabled)
        .AddClass(options.Theme.Interaction.DisabledCursor, isDisabled)
        .AddClass(GetRangeAwareRoundedClass(isRangeStart, isRangeEnd, isInRange))
        .AddClass(theme.ButtonClass)
        .AddClass(theme.HoverClass, !isDisabled && !isSelected)
        .AddClass(theme.ActiveClass, isToday && !isSelected && !isInRange && !isDisabled)
        .AddClass(theme.RangeClass, isInRange && !isSelected && !isDisabled)
        .AddClass(theme.SelectedClass, isSelected && !isDisabled)
        .Build();

    /// <summary>
    /// Gets the accessible name for a day-of-month button, giving it full date context (e.g.
    /// "August 14, 2026") instead of just the bare day number, and prefixing it with the day's
    /// selection/range state when applicable.
    /// </summary>
    private static string GetDayAriaLabel(DateTime date, bool isSelected, bool isRangeStart, bool isRangeEnd, bool isInRange)
    {
        if (isRangeStart) return $"Start of selected range, {date:MMMM d, yyyy}";
        if (isRangeEnd) return $"End of selected range, {date:MMMM d, yyyy}";
        if (isInRange) return $"In selected range, {date:MMMM d, yyyy}";
        return isSelected ? $"Selected, {date:MMMM d, yyyy}" : date.ToString("MMMM d, yyyy");
    }

    /// <summary>
    /// Splits the month's day cells (including leading days from the previous month) into rows of 7,
    /// so the calendar can be rendered as a real &lt;table&gt; with &lt;tr&gt; rows - giving assistive
    /// tech a structural association between each day cell and its weekday column header, instead of
    /// two visually-aligned but structurally unrelated CSS grids. The final row is padded with null
    /// cells (rendered as empty, aria-hidden placeholders) so every row has exactly 7 columns.
    /// </summary>
    private List<List<(int DayNumber, bool IsCurrentMonth)?>> GetWeekRows()
    {
        var daysInMonth = DateTime.DaysInMonth(Value.Year, Value.Month);
        var firstOfMonth = new DateTime(Value.Year, Value.Month, 1, 0, 0, 0, Value.Kind);
        // sunday = 0 - saturday = 6
        var offset = (int)firstOfMonth.DayOfWeek;

        var prevMonth = Value.AddMonths(-1);
        var prevDaysInMonth = DateTime.DaysInMonth(prevMonth.Year, prevMonth.Month);
        var prevStart = prevDaysInMonth - offset + 1;

        List<(int DayNumber, bool IsCurrentMonth)?> cells = [];

        for (var i = 0; i < offset; i++)
        {
            cells.Add((prevStart + i, false));
        }

        for (var day = 1; day <= daysInMonth; day++)
        {
            cells.Add((day, true));
        }

        while (cells.Count % 7 != 0)
        {
            cells.Add(null);
        }

        List<List<(int DayNumber, bool IsCurrentMonth)?>> rows = [];
        for (var i = 0; i < cells.Count; i += 7)
        {
            rows.Add(cells.GetRange(i, 7));
        }

        return rows;
    }

    private async Task SelectDateAsync(DateTime dateTime)
    {
        Value = dateTime;

        if (ValueChanged.HasDelegate)
        {
            await ValueChanged.InvokeAsync(Value);
        }
    }

    /// <summary>
    /// Routes a day-cell click through the <see cref="IsDayDisabled(DateTime)"/> check. Disabled
    /// cells are still real, focusable buttons (see <see cref="GetButtonClasses"/>'s remarks on why
    /// the native <c>disabled</c> attribute isn't used), so the no-op guard lives here rather than
    /// on the element itself.
    /// </summary>
    private Task HandleDayClick(DateTime dateTime, bool isDisabled) =>
        isDisabled ? Task.CompletedTask : SelectDateAsync(dateTime);
}
