// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Globalization;
using TwBlazor.Configuration.Components;
using TwBlazor.Utilities;

namespace TwBlazor.Components;

/// <summary>
/// Represents a date range picker component that lets users select a start and end date from a
/// calendar popover showing two months side by side (one on narrow/mobile screens), or by typing
/// both dates directly.
/// </summary>
/// <remarks>
/// Built from the same pieces <see cref="TwDatePicker"/> is built from -
/// <see cref="TwPopoverPickerComponentBase"/> for its open/close/focus-trap mechanics,
/// <see cref="DatePicker.TwDatePickerHeader"/>/<see cref="DatePicker.TwDatePickerBody"/> for its
/// panel chrome, <see cref="DatePicker.TwDatePickerDayView"/> (extended with range-highlighting
/// support) for its day grids, and <see cref="TwDatePickerTheme"/> for styling - so the two
/// pickers look and behave consistently. There is no native-picker path here: the browser's
/// native date input has no concept of a range, so the custom popover is always used regardless
/// of <see cref="TwPopoverPickerComponentBase.PreferNativePicker"/> (that inherited parameter has
/// no effect on this component). Thread safety is not guaranteed; use the component only within
/// the Blazor UI thread.
/// </remarks>
public partial class TwDateRangePicker : TwPopoverPickerComponentBase
{
    private TwDatePickerTheme theme => options.Theme.Components.Require<TwDatePickerTheme>();

    /// <summary>
    /// Reference to the trigger <see cref="TwTextfield{T}"/> instance, used to focus its actual
    /// &lt;input&gt; element directly - see <see cref="TwPopoverPickerComponentBase.triggerInputRef"/>.
    /// </summary>
    private TwTextfield<string>? trigger;

    /// <inheritdoc />
    protected override ElementReference? triggerInputRef => trigger?.InputRef;

    /// <summary>
    /// Gets or sets the underlying view used to display and interact with the picker.
    /// </summary>
    private DateRangePickerView view { get; set; }

    /// <summary>
    /// The left-hand displayed month. Purely internal navigation state (not a bound parameter) -
    /// seeded from <see cref="SelectedRange"/>'s start (or <see cref="MinDate"/>, or today) so the
    /// calendar opens on a sensible month, then moves independently as the user navigates.
    /// </summary>
    private DateTime anchorMonth;

    /// <summary>
    /// The placeholder text to display when no range is selected.
    /// </summary>
    /// <remarks>
    /// If not set, defaults to two lower-cased copies of <see cref="resolvedFormat"/> joined by
    /// <see cref="resolvedRangeSeparator"/> (e.g. "dd/mm/yyyy - dd/mm/yyyy"), so the placeholder
    /// itself communicates the exact pattern typed input is parsed against.
    /// </remarks>
    [Parameter] public string? Placeholder { get; set; }

    private string effectivePlaceholder => Placeholder ??
        $"{resolvedFormat.ToLower(CultureInfo.CurrentCulture)}{resolvedRangeSeparator}{resolvedFormat.ToLower(CultureInfo.CurrentCulture)}";

    /// <summary>
    /// The selected date range: <c>Key</c> is the start date, <c>Value</c> is the end date. Either
    /// (or both) may be <see langword="null"/> - both null means no range has been started yet,
    /// and a non-null <c>Key</c> with a null <c>Value</c> means only the start has been picked so
    /// far. <c>Key</c> is always less than or equal to <c>Value</c> when both are set - this
    /// component swaps the two dates as needed so callers never need to sort them themselves.
    /// </summary>
    [Parameter] public KeyValuePair<DateTime?, DateTime?> SelectedRange { get; set; }

    /// <summary>
    /// The bound <see cref="SelectedRange"/> value; invoked whenever it changes, including after
    /// only the range's start has been picked (<c>Value</c> still <see langword="null"/>) so
    /// callers can reflect an in-progress selection.
    /// </summary>
    [Parameter] public EventCallback<KeyValuePair<DateTime?, DateTime?>> SelectedRangeChanged { get; set; }

    /// <summary>
    /// The <see cref="string"/> value displayed in the textbox to the user.
    /// </summary>
    [Parameter] public string? Value { get; set; }

    /// <summary>
    /// The <see cref="string"/> bound value displayed in the textbox to the user.
    /// </summary>
    [Parameter] public EventCallback<string> ValueChanged { get; set; }

    /// <summary>
    /// The .NET custom date format string used to display and parse each date in
    /// <see cref="SelectedRange"/>, applied identically to both the start and the end date. Any
    /// valid pattern works here, e.g. 'MM/dd/yyyy' for US-style dates or 'dd-MM-yyyy' for a
    /// dash-separated one. Leave unset to fall back to <see cref="TwDatePickerTheme.DefaultFormat"/>
    /// (default 'dd/MM/yyyy').
    /// </summary>
    [Parameter] public string? Format { get; set; }

    /// <summary>
    /// The format actually used: <see cref="Format"/> when explicitly set, otherwise
    /// <see cref="TwDatePickerTheme.DefaultFormat"/>.
    /// </summary>
    private string resolvedFormat => Format ?? theme.DefaultFormat;

    /// <summary>
    /// The separator inserted between the two dates in <see cref="Value"/>. A locale-neutral symbol
    /// (like the default, " - ") reads better than a word like "to" that would need translating for
    /// non-English users. The surrounding spaces also mean an everyday dash-containing
    /// <see cref="Format"/> like "dd-MM-yyyy" (no spaces around its own "-") doesn't collide with
    /// it when typed text is split back apart - only a Format that itself embeds " - " (spaces
    /// around a dash) would, so avoid that specific combination if you override either parameter.
    /// Leave unset to fall back to <see cref="TwDatePickerTheme.DefaultRangeSeparator"/>.
    /// </summary>
    [Parameter] public string? RangeSeparator { get; set; }

    /// <summary>
    /// The separator actually used: <see cref="RangeSeparator"/> when explicitly set, otherwise
    /// <see cref="TwDatePickerTheme.DefaultRangeSeparator"/>.
    /// </summary>
    private string resolvedRangeSeparator => RangeSeparator ?? theme.DefaultRangeSeparator;

    /// <summary>
    /// The earliest selectable date (inclusive). Days before this are disabled in the calendar and
    /// rejected when typed; the Previous month button is also disabled once it's reached.
    /// </summary>
    [Parameter] public DateTime? MinDate { get; set; }

    /// <summary>
    /// The latest selectable date (inclusive). Days after this are disabled in the calendar and
    /// rejected when typed; the Next month button is also disabled once it's reached.
    /// </summary>
    [Parameter] public DateTime? MaxDate { get; set; }

    /// <summary>
    /// Set when the panel's view switches (year/month/day) while it's already open, so the next
    /// <see cref="OnAfterRenderAsync"/> reclaims focus inside the panel - see
    /// <see cref="TwDatePicker.pendingViewFocus"/>'s remarks for why this is needed.
    /// </summary>
    private bool pendingViewFocus;

    private string classes => new ClassBuilder(options.Theme.Position.Relative)
        .AddClass(options.Theme.Display.Flex)
        .AddClass(options.Theme.Flexbox.Col)
        .AddClass(RootClass)
        .AddClass(Class).Build();

    private string textfieldClasses => new ClassBuilder(theme.TextfieldPadding).Build();

    private string panelWidthClasses => new ClassBuilder(theme.PanelWidth)
        .AddClass(theme.DualMonthPanelWidth, view == DateRangePickerView.Day)
        .Build();

    // The two-month panel (up to ~552px, see panelWidthClasses) is wider than TwDatePicker's
    // single-month one and can otherwise overflow a narrow trigger's container even after
    // twPicker.positionPanel flips it to the other viewport edge (that check is viewport-relative,
    // not aware of a narrower positioned/overflow ancestor) - capping the width and letting it
    // scroll horizontally keeps it fully reachable instead of silently clipping off-screen.
    private string datepickerContainerClasses => new ClassBuilder($"{theme.PanelMaxWidth} {theme.PanelMaxHeight}")
        .AddClass(popoverBuilder.GetSurfaceClasses(Rounded, Shadow))
        .AddClass(theme.Base)
        .Build();

    /// <summary>
    /// Gets or sets the CSS class names to apply to the body element of the component.
    /// </summary>
    [Parameter] public string BodyClasses { get; set; } = string.Empty;

    private string yearsGridClasses => new ClassBuilder("years-of-the-decade")
        .AddClass(theme.YearsGrid).Build();

    private string monthsGridClasses => new ClassBuilder("months-of-the-year")
        .AddClass(theme.MonthsGrid).Build();

    private string bodyClasses => new ClassBuilder()
        .AddClass("decade", view == DateRangePickerView.Year)
        .AddClass("months", view == DateRangePickerView.Month)
        .AddClass("days", view == DateRangePickerView.Day)
        .AddClass(BodyClasses).Build();

    /// <summary>
    /// Gets whether the Previous month button should be disabled: <see cref="MinDate"/> is set and
    /// the left-hand displayed month is already at (or before) it.
    /// </summary>
    private bool isPreviousMonthDisabled => MinDate.HasValue
        && new DateTime(anchorMonth.Year, anchorMonth.Month, 1, 0, 0, 0, DateTimeKind.Unspecified) <= new DateTime(MinDate.Value.Year, MinDate.Value.Month, 1, 0, 0, 0, DateTimeKind.Unspecified);

    /// <summary>
    /// Gets whether the Next month button should be disabled: <see cref="MaxDate"/> is set and the
    /// right-hand displayed month is already at (or after) it.
    /// </summary>
    private bool isNextMonthDisabled
    {
        get
        {
            if (!MaxDate.HasValue) return false;
            var rightMonth = anchorMonth.AddMonths(1);
            return new DateTime(rightMonth.Year, rightMonth.Month, 1, 0, 0, 0, DateTimeKind.Unspecified) >= new DateTime(MaxDate.Value.Year, MaxDate.Value.Month, 1, 0, 0, 0, DateTimeKind.Unspecified);
        }
    }

    /// <summary>
    /// Gets the CSS classes for the month/year quick-pick buttons present in the dialog.
    /// </summary>
    private string GetButtonClasses(string name, bool isSelected) =>
        new ClassBuilder($"{name} cursor-pointer")
        .AddClass(roundedBuilder.GetRounded())
        .AddClass(theme.ButtonClass)
        .AddClass(theme.HoverClass, !isSelected)
        .AddClass(theme.SelectedClass, isSelected)
        .Build();

    /// <summary>
    /// Initializes the component, seeding the displayed month and the trigger's text value.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown when <see cref="Format"/> is explicitly set to an empty string.</exception>
    protected override void OnInitialized()
    {
        if (Format is not null)
        {
            ArgumentException.ThrowIfNullOrEmpty(Format);
        }
        base.OnInitialized();
        anchorMonth = SelectedRange.Key ?? MinDate ?? DateTime.Today;
        Value = FormatRange(SelectedRange);
    }

    /// <summary>
    /// Arms the panel's Tab focus trap/background inert-ing when it first opens, and reclaims
    /// focus inside it after a year/month/day view switch - same mechanics as
    /// <see cref="TwDatePicker.OnAfterRenderAsync"/>, minus its native-picker detection (there is
    /// no native input here).
    /// </summary>
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);

        if (pendingFocusDate is { } focusDate)
        {
            pendingFocusDate = null;
            var monthView = MonthIndex(focusDate) == MonthIndex(anchorMonth) ? firstMonthView : secondMonthView;
            monthView?.FocusDay(focusDate.Day);
        }

        if (isFocused && PanelRef.Context != null)
        {
            // Re-run on every render rather than gating behind a one-shot "just opened" flag - see
            // the matching remarks on TwDatePicker.OnAfterRenderAsync for why.
            await RegisterPanelScrollBehaviorAsync(PanelRef);
            await ApplyPanelTrapAsync(PanelRef);

            if (pendingViewFocus)
            {
                pendingViewFocus = false;
                await JSRuntime.InvokeVoidAsync("twDialog.focusPanel", PanelRef);
            }
        }
    }

    /// <summary>
    /// Handles text input changes and attempts to parse two dates, separated by
    /// <see cref="RangeSeparator"/>, using <see cref="Format"/>.
    /// </summary>
    /// <remarks>
    /// If either half can't be parsed, or a parsed date falls outside <see cref="MinDate"/>/
    /// <see cref="MaxDate"/>, the input is left as-is and <see cref="TwBlazorInputComponentBase.Invalid"/>/
    /// <see cref="TwBlazorInputComponentBase.ErrorMessage"/> are set instead of silently discarding
    /// what the user typed - same approach as <see cref="TwDatePicker.OnTextValueChanged"/>.
    /// </remarks>
    private async Task OnTextValueChanged(string? text)
    {
        if (ReadOnly || Disabled)
            return;

        await ClosePanelAfterTextCommitAsync();

        if (string.IsNullOrWhiteSpace(text))
        {
            return;
        }

        var parts = text.Split(resolvedRangeSeparator, StringSplitOptions.TrimEntries);
        var start = default(DateTime);
        var end = default(DateTime);
        var success = parts.Length == 2
            && DateTime.TryParseExact(parts[0], resolvedFormat, CultureInfo.CurrentCulture, DateTimeStyles.None, out start)
            && DateTime.TryParseExact(parts[1], resolvedFormat, CultureInfo.CurrentCulture, DateTimeStyles.None, out end)
            && !IsOutOfBounds(start) && !IsOutOfBounds(end);

        if (!success)
        {
            Invalid = true;
            ErrorMessage = $"Enter a valid date range, for example {FormatRange(new KeyValuePair<DateTime?, DateTime?>(DateTime.Today, DateTime.Today.AddDays(7)))}";
            Value = text;
            return;
        }

        Invalid = false;
        ErrorMessage = string.Empty;

        var ordered = start <= end
            ? new KeyValuePair<DateTime?, DateTime?>(start, end)
            : new KeyValuePair<DateTime?, DateTime?>(end, start);
        SelectedRange = ordered;
        Value = FormatRange(ordered);
        anchorMonth = ordered.Key ?? anchorMonth;

        var selectedRange = SelectedRange;
        var value = Value;

        if (SelectedRangeChanged.HasDelegate)
        {
            await SelectedRangeChanged.InvokeAsync(selectedRange);
        }

        if (ValueChanged.HasDelegate)
        {
            await ValueChanged.InvokeAsync(value);
        }
    }

    private bool IsOutOfBounds(DateTime date) =>
        (MinDate.HasValue && date.Date < MinDate.Value.Date) || (MaxDate.HasValue && date.Date > MaxDate.Value.Date);

    /// <summary>
    /// Formats a range for display: empty when no start (<c>Key</c>) is set, just the start when
    /// the end (<c>Value</c>) isn't picked yet, otherwise both joined by <see cref="resolvedRangeSeparator"/>.
    /// </summary>
    private string FormatRange(KeyValuePair<DateTime?, DateTime?> range)
    {
        if (range.Key is null)
        {
            return string.Empty;
        }

        var start = range.Key.Value.ToString(resolvedFormat, CultureInfo.CurrentCulture);
        if (range.Value is null)
        {
            return start;
        }

        return $"{start}{resolvedRangeSeparator}{range.Value.Value.ToString(resolvedFormat, CultureInfo.CurrentCulture)}";
    }

    /// <summary>
    /// Moves the displayed months back by one, unless <see cref="isPreviousMonthDisabled"/>.
    /// </summary>
    private DatePicker.TwDatePickerDayView? firstMonthView;
    private DatePicker.TwDatePickerDayView? secondMonthView;

    // The date to focus once the months the keyboard just moved to have rendered.
    private DateTime? pendingFocusDate;

    private static int MonthIndex(DateTime date) => date.Year * 12 + date.Month;

    // Two months are showing. A keyboard move between them only moves focus; a move off either end slides
    // the pair along so the destination month is visible, unless MinDate/MaxDate rule it out.
    private void OnDayNavigationRequested(DateTime target)
    {
        var targetIndex = MonthIndex(target);
        if ((MinDate.HasValue && targetIndex < MonthIndex(MinDate.Value))
            || (MaxDate.HasValue && targetIndex > MonthIndex(MaxDate.Value)))
        {
            return;
        }

        var firstIndex = MonthIndex(anchorMonth);
        if (targetIndex < firstIndex)
        {
            anchorMonth = anchorMonth.AddMonths(targetIndex - firstIndex);
        }
        else if (targetIndex > firstIndex + 1)
        {
            anchorMonth = anchorMonth.AddMonths(targetIndex - firstIndex - 1);
        }

        pendingFocusDate = target;
    }

    private void PreviousMonth()
    {
        if (!isPreviousMonthDisabled)
        {
            anchorMonth = anchorMonth.AddMonths(-1);
        }
    }

    /// <summary>
    /// Advances the displayed months by one, unless <see cref="isNextMonthDisabled"/>.
    /// </summary>
    private void NextMonth()
    {
        if (!isNextMonthDisabled)
        {
            anchorMonth = anchorMonth.AddMonths(1);
        }
    }

    private void NextDecade() => anchorMonth = anchorMonth.AddYears(10);

    private void PreviousDecade() => anchorMonth = anchorMonth.AddYears(-10);

    private void NextYear() => anchorMonth = anchorMonth.AddYears(1);

    private void PreviousYear() => anchorMonth = anchorMonth.AddYears(-1);

    /// <summary>
    /// Selects a month and switches the view to the day view. Always normalizes to the 1st of the
    /// month, since <see cref="anchorMonth"/> only ever needs to identify a month, not a specific
    /// day - unlike <see cref="DatePicker.TwDatePickerCalendar.PreserveAnchorDay"/> mode, which
    /// preserves the actual selected day (used by <see cref="TwDatePicker"/>'s own calendar).
    /// </summary>
    private void SelectMonth(DateTime selectedMonth)
    {
        anchorMonth = new DateTime(anchorMonth.Year, selectedMonth.Month, 1, 0, 0, 0, anchorMonth.Kind);
        view = DateRangePickerView.Day;
        pendingViewFocus = true;
    }

    /// <summary>
    /// Selects a year and switches the view to the month view.
    /// </summary>
    private void SelectYear(DateTime selectedYear)
    {
        anchorMonth = new DateTime(selectedYear.Year, anchorMonth.Month, 1, 0, 0, 0, anchorMonth.Kind);
        view = DateRangePickerView.Month;
        pendingViewFocus = true;
    }

    /// <summary>
    /// Switches the picker to the specified view.
    /// </summary>
    private void SwitchView(DateRangePickerView datePickerView)
    {
        view = datePickerView;
        pendingViewFocus = true;
    }

    /// <summary>
    /// Selects a day clicked in either of the two calendar grids, running the range's
    /// pick-start/pick-end state machine.
    /// </summary>
    /// <remarks>
    /// The first click after no range (or after a completed one) starts a fresh range and leaves
    /// the panel open for the second pick. The second click completes it - swapping the two dates
    /// if it lands before the start - then closes the panel and restores focus, mirroring
    /// <see cref="TwDatePicker.SelectDateAsync(DateTime)"/>'s completed-selection path.
    /// <see cref="DatePicker.TwDatePickerDayView"/>'s own <see cref="MinDate"/>/<see cref="MaxDate"/>
    /// disabling already prevents an out-of-bounds day from reaching this method via a click, so
    /// there's no redundant bounds re-check here.
    /// </remarks>
    private async Task SelectRangeDateAsync(DateTime day)
    {
        var current = SelectedRange;
        var completesRange = current.Key.HasValue && !current.Value.HasValue;

        KeyValuePair<DateTime?, DateTime?> next;
        if (!completesRange)
        {
            next = new KeyValuePair<DateTime?, DateTime?>(day, null);
        }
        else if (day < current.Key!.Value)
        {
            next = new KeyValuePair<DateTime?, DateTime?>(day, current.Key);
        }
        else
        {
            next = new KeyValuePair<DateTime?, DateTime?>(current.Key, day);
        }

        SelectedRange = next;
        Value = FormatRange(next);
        Invalid = false;
        ErrorMessage = string.Empty;

        // Captured before either callback fires, same reentrancy guard TwDatePicker.SelectDateAsync uses.
        var selectedRange = SelectedRange;
        var value = Value;

        if (completesRange)
        {
            if (isFocused)
            {
                await ReleasePanelTrapAsync();
            }
            isFocused = false;
            await RestoreFocusAsync();
        }

        if (SelectedRangeChanged.HasDelegate)
        {
            await SelectedRangeChanged.InvokeAsync(selectedRange);
        }

        if (ValueChanged.HasDelegate)
        {
            await ValueChanged.InvokeAsync(value);
        }
    }

    /// <summary>
    /// Defines the available views for the date range picker component.
    /// </summary>
    private enum DateRangePickerView
    {
        /// <summary>
        /// Day selection view showing two calendar grids of days side by side.
        /// </summary>
        Day,

        /// <summary>
        /// Month selection view showing a grid of months in a year.
        /// </summary>
        Month,

        /// <summary>
        /// Year selection view showing a grid of years in a decade.
        /// </summary>
        Year
    }
}
