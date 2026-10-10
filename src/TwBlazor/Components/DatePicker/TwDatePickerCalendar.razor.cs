// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using Microsoft.AspNetCore.Components;
using TwBlazor.Configuration.Components;
using TwBlazor.Utilities;

namespace TwBlazor.Components.DatePicker;

/// <summary>
/// The reusable year/month/day calendar navigation - header, quick-pick grids, and day grid -
/// shared by every popover date picker in the library (<see cref="TwBlazor.Components.TwDatePicker"/>,
/// <see cref="TwBlazor.Components.TwDateTimeRangePicker"/>). Deliberately excludes the trigger
/// textfield, popover open/close mechanics, and any time editor - those stay with each picker,
/// which composes this purely for its calendar portion.
/// </summary>
/// <remarks>
/// <see cref="TwBlazor.Components.TwDateRangePicker"/> is the one exception: its two-month, single
/// shared-header layout doesn't decompose into one instance of this component, so it keeps its own
/// (structurally similar but distinct) rendering.
/// </remarks>
public partial class TwDatePickerCalendar : TwBlazorComponentBase
{
    private TwDatePickerTheme theme => options.Theme.Components.Require<TwDatePickerTheme>();

    /// <summary>
    /// The month/year/decade currently displayed - a pure navigation position, independent of the
    /// actual selection (<see cref="SelectedDate"/>/<see cref="Range"/>). Browsing with the header's
    /// Previous/Next controls, or drilling through the quick-pick grids, only moves this; the real
    /// selection changes only when a day is actually clicked.
    /// </summary>
    [Parameter, EditorRequired] public DateTime AnchorDate { get; set; }

    /// <summary>
    /// The bound <see cref="AnchorDate"/> value; invoked whenever navigation (Previous/Next, or a
    /// month/year quick-pick) moves it.
    /// </summary>
    [Parameter] public EventCallback<DateTime> AnchorDateChanged { get; set; }

    /// <summary>
    /// Whether jumping to a different month/year (via the quick-pick grids) keeps
    /// <see cref="AnchorDate"/>'s existing day and time-of-day, or resets both to the 1st at
    /// midnight. Default is <see langword="true"/> (keep them) - the right choice for a single-date
    /// picker, where the anchor is centered on a specific day. Pass <see langword="false"/> for a
    /// picker where the anchor only ever needs to identify a month, never a specific day.
    /// </summary>
    [Parameter] public bool PreserveAnchorDay { get; set; } = true;

    /// <summary>
    /// The currently displayed view (day grid, month quick-pick, or year quick-pick).
    /// </summary>
    [Parameter] public DatePickerCalendarView View { get; set; }

    /// <summary>
    /// The bound <see cref="View"/> value; invoked whenever the displayed view switches.
    /// </summary>
    [Parameter] public EventCallback<DatePickerCalendarView> ViewChanged { get; set; }

    /// <summary>
    /// The reference date used to highlight a day (in single-select mode) or a month/year quick-pick
    /// button as selected. In range mode (<see cref="Range"/> set), pass whichever side of the range
    /// is relevant to the current step - the day grid itself renders both range endpoints
    /// independently of this.
    /// </summary>
    [Parameter] public DateTime? SelectedDate { get; set; }

    /// <summary>
    /// When set, switches the day grid from single-day selection to range highlighting - forwarded
    /// directly to <see cref="TwDatePickerDayView.Range"/>.
    /// </summary>
    [Parameter] public KeyValuePair<DateTime?, DateTime?>? Range { get; set; }

    /// <summary>
    /// The earliest selectable date (inclusive). Days before this render disabled in the day grid,
    /// and the Previous month button is disabled once it's reached. <see langword="null"/> (the
    /// default) leaves the lower bound unrestricted.
    /// </summary>
    [Parameter] public DateTime? MinDate { get; set; }

    /// <summary>
    /// The latest selectable date (inclusive). Days after this render disabled in the day grid, and
    /// the Next month button is disabled once it's reached. <see langword="null"/> (the default)
    /// leaves the upper bound unrestricted.
    /// </summary>
    [Parameter] public DateTime? MaxDate { get; set; }

    /// <summary>
    /// Whether the month/year quick-pick grids mark the actual current year/month with
    /// <see cref="TwDatePickerTheme.ActiveClass"/>, matching the day grid's own "today" indicator.
    /// Default is <see langword="true"/>.
    /// </summary>
    [Parameter] public bool ShowTodayIndicator { get; set; } = true;

    /// <summary>
    /// Forwarded to <see cref="TwDatePickerDayView.ShowMonthCaption"/> - set by callers (like a
    /// range picker) that need each day grid to label its own month.
    /// </summary>
    [Parameter] public bool ShowMonthCaption { get; set; }

    /// <summary>
    /// Gets or sets the CSS class names to apply to the body element of whichever view is current.
    /// </summary>
    [Parameter] public string BodyClasses { get; set; } = string.Empty;

    /// <summary>
    /// Invoked when a day is clicked in the day grid.
    /// </summary>
    [Parameter, EditorRequired] public EventCallback<DateTime> DaySelected { get; set; }

    /// <summary>
    /// Invoked after any interaction that swaps out the panel's focusable content - a view switch,
    /// or a month/year quick-pick - so the host popover can reclaim focus inside its own panel
    /// element, mirroring the <c>pendingViewFocus</c> mechanic every popover picker in this library
    /// already used individually before this component existed.
    /// </summary>
    [Parameter] public EventCallback Navigated { get; set; }

    private string yearsGridClasses => new ClassBuilder("years-of-the-decade")
        .AddClass(theme.YearsGrid).Build();

    private string monthsGridClasses => new ClassBuilder("months-of-the-year")
        .AddClass(theme.MonthsGrid).Build();

    private string bodyClasses => new ClassBuilder()
        .AddClass("decade", View == DatePickerCalendarView.Year)
        .AddClass("months", View == DatePickerCalendarView.Month)
        .AddClass("days", View == DatePickerCalendarView.Day)
        .AddClass(BodyClasses).Build();

    /// <summary>
    /// Gets "today" for comparison against the year/month quick-pick grids and honors
    /// <see cref="AnchorDate"/>'s <see cref="DateTime.Kind"/> the same way
    /// <see cref="TwDatePickerDayView"/> does for its own today indicator.
    /// </summary>
    private DateTime today => (AnchorDate.Kind == DateTimeKind.Utc ? DateTime.UtcNow : DateTime.Now).Date;

    private bool isPreviousMonthDisabled => MinDate.HasValue
        && new DateTime(AnchorDate.Year, AnchorDate.Month, 1, 0, 0, 0, DateTimeKind.Unspecified) <= new DateTime(MinDate.Value.Year, MinDate.Value.Month, 1, 0, 0, 0, DateTimeKind.Unspecified);

    private bool isNextMonthDisabled => MaxDate.HasValue
        && new DateTime(AnchorDate.Year, AnchorDate.Month, 1, 0, 0, 0, DateTimeKind.Unspecified) >= new DateTime(MaxDate.Value.Year, MaxDate.Value.Month, 1, 0, 0, 0, DateTimeKind.Unspecified);

    /// <summary>
    /// Gets the CSS classes for the month/year quick-pick buttons present in the dialog.
    /// </summary>
    private string GetButtonClasses(string name, bool isSelected, bool isToday) =>
        new ClassBuilder($"{name} cursor-pointer")
        .AddClass(roundedBuilder.GetRounded())
        .AddClass(theme.ButtonClass)
        .AddClass(theme.HoverClass, !isSelected)
        .AddClass(theme.ActiveClass, isToday && !isSelected)
        .AddClass(theme.SelectedClass, isSelected)
        .Build();

    private async Task SetAnchorDateAsync(DateTime date)
    {
        AnchorDate = date;
        if (AnchorDateChanged.HasDelegate)
        {
            await AnchorDateChanged.InvokeAsync(date);
        }
    }

    private async Task SetViewAsync(DatePickerCalendarView newView)
    {
        View = newView;
        if (ViewChanged.HasDelegate)
        {
            await ViewChanged.InvokeAsync(newView);
        }
    }

    private TwDatePickerDayView? dayView;

    // The day to focus once the page the keyboard just turned to has rendered.
    private int? pendingFocusDay;

    private static DateTime FirstOfMonth(DateTime date) => new(date.Year, date.Month, 1, 0, 0, 0, DateTimeKind.Unspecified);

    // Turns the page to the month the keyboard moved into, unless MinDate/MaxDate rule that month out.
    private async Task OnDayNavigationRequested(DateTime target)
    {
        var targetMonth = FirstOfMonth(target);
        if ((MinDate.HasValue && targetMonth < FirstOfMonth(MinDate.Value))
            || (MaxDate.HasValue && targetMonth > FirstOfMonth(MaxDate.Value)))
        {
            return;
        }

        pendingFocusDay = target.Day;
        await SetAnchorDateAsync(new DateTime(target.Year, target.Month, target.Day, AnchorDate.Hour, AnchorDate.Minute, AnchorDate.Second, AnchorDate.Kind));
    }

    /// <inheritdoc />
    protected override void OnAfterRender(bool firstRender)
    {
        base.OnAfterRender(firstRender);

        if (pendingFocusDay is { } day && dayView is not null)
        {
            pendingFocusDay = null;
            dayView.FocusDay(day);
        }
    }

    private Task PreviousMonth() => isPreviousMonthDisabled ? Task.CompletedTask : SetAnchorDateAsync(AnchorDate.AddMonths(-1));

    private Task NextMonth() => isNextMonthDisabled ? Task.CompletedTask : SetAnchorDateAsync(AnchorDate.AddMonths(1));

    private Task NextDecade() => SetAnchorDateAsync(AnchorDate.AddYears(10));

    private Task PreviousDecade() => SetAnchorDateAsync(AnchorDate.AddYears(-10));

    private Task NextYear() => SetAnchorDateAsync(AnchorDate.AddYears(1));

    private Task PreviousYear() => SetAnchorDateAsync(AnchorDate.AddYears(-1));

    /// <summary>
    /// Selects a month and switches to the day view - preserving the existing day/time-of-day when
    /// <see cref="PreserveAnchorDay"/>, otherwise normalizing to the 1st at midnight.
    /// </summary>
    private async Task SelectMonth(DateTime selectedMonth)
    {
        var newAnchor = PreserveAnchorDay
            ? new DateTime(AnchorDate.Year, selectedMonth.Month, AnchorDate.Day, AnchorDate.Hour, AnchorDate.Minute, AnchorDate.Second, AnchorDate.Kind)
            : new DateTime(AnchorDate.Year, selectedMonth.Month, 1, 0, 0, 0, AnchorDate.Kind);

        await SetAnchorDateAsync(newAnchor);
        await SetViewAsync(DatePickerCalendarView.Day);
        await RaiseNavigatedAsync();
    }

    /// <summary>
    /// Selects a year and switches to the month view - see <see cref="SelectMonth"/>'s remarks for
    /// <see cref="PreserveAnchorDay"/>'s effect.
    /// </summary>
    private async Task SelectYear(DateTime selectedYear)
    {
        var newAnchor = PreserveAnchorDay
            ? new DateTime(selectedYear.Year, AnchorDate.Month, AnchorDate.Day, AnchorDate.Hour, AnchorDate.Minute, AnchorDate.Second, AnchorDate.Kind)
            : new DateTime(selectedYear.Year, AnchorDate.Month, 1, 0, 0, 0, AnchorDate.Kind);

        await SetAnchorDateAsync(newAnchor);
        await SetViewAsync(DatePickerCalendarView.Month);
        await RaiseNavigatedAsync();
    }

    private async Task SwitchView(DatePickerCalendarView newView)
    {
        await SetViewAsync(newView);
        await RaiseNavigatedAsync();
    }

    private async Task RaiseNavigatedAsync()
    {
        if (Navigated.HasDelegate)
        {
            await Navigated.InvokeAsync();
        }
    }

    private Task OnDaySelected(DateTime date) =>
        DaySelected.HasDelegate ? DaySelected.InvokeAsync(date) : Task.CompletedTask;
}

/// <summary>
/// Defines the available views for <see cref="TwDatePickerCalendar"/>.
/// </summary>
public enum DatePickerCalendarView
{
    /// <summary>
    /// Day selection view showing a calendar grid of days in a month.
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
