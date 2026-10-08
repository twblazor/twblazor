// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;

namespace TwBlazor.Configuration.Components;

/// <summary>
/// Theme configuration for the date picker components (<see cref="TwBlazor.Components.TwDatePicker"/> and
/// <see cref="TwBlazor.Components.TwDateRangePicker"/>, which reuses the same panel/header/day-button
/// styling for visual consistency between the two). Override any property to customize date picker
/// styles globally.
/// </summary>
[ExcludeFromCodeCoverage]
public class TwDatePickerTheme
{
    /// <summary>
    /// Gets or sets the classes for the header that displays navigation between days, months, and
    /// decades. Its background comes from the shared <see cref="TwPopoverTheme.Background"/>.
    /// </summary>
    public required string Header { get; set; }

    /// <summary>
    /// Gets or sets the classes for the header row that displays the weekdays (e.g. Mon, Tue, Wed).
    /// </summary>
    public required string WeekdaysHeader { get; set; }

    /// <summary>
    /// Gets or sets the positioning/layout/typography classes for the picker panel - background,
    /// border, rounded corners, and shadow come from the shared <see cref="TwOverlayTheme"/> via
    /// <see cref="Builders.PopoverBuilder"/> instead of being baked in here.
    /// </summary>
    public required string Base { get; set; }

    /// <summary>
    /// Gets or sets the classes for active day, month, and year selection buttons.
    /// </summary>
    public required string ActiveClass { get; set; }

    /// <summary>
    /// Gets or sets the classes for a selected day, month, or year (and the start and end of a range).
    /// </summary>
    public required string SelectedClass { get; set; }

    /// <summary>
    /// Gets or sets the hover classes for an unselected day, month, year, or step tab.
    /// </summary>
    public required string HoverClass { get; set; }

    /// <summary>
    /// Gets or sets the classes for the active Start/End step tab.
    /// </summary>
    public required string StageTabActive { get; set; }

    /// <summary>
    /// Gets or sets the classes for the day, month, and year selection buttons.
    /// </summary>
    public required string ButtonClass { get; set; }

    /// <summary>
    /// Gets or sets the classes for days that fall strictly between a selected range's start and
    /// end (exclusive of the endpoints) in <see cref="TwBlazor.Components.TwDateRangePicker"/>.
    /// </summary>
    public required string RangeClass { get; set; }

    /// <summary>
    /// Gets or sets the classes for the per-calendar "Month Year" caption shown above each grid
    /// when <see cref="TwBlazor.Components.TwDateRangePicker"/> displays two months side by side.
    /// </summary>
    public required string RangeMonthCaptionClass { get; set; }

    /// <summary>
    /// Gets or sets the text color classes for a lead-in day from the previous month (the
    /// greyed-out, non-interactive cells shown before day 1). Not applied when the day falls
    /// within a selected range - see <see cref="RangeClass"/> - so it stays legible against that
    /// tint instead of fighting it with a second, muted color.
    /// </summary>
    public required string PrevMonthClass { get; set; }

    /// <summary>
    /// Gets or sets the default .NET custom date format string used by <see cref="TwBlazor.Components.TwDatePicker"/>
    /// and <see cref="TwBlazor.Components.TwDateRangePicker"/> when their own <c>Format</c> parameter
    /// isn't explicitly set. Change this to switch the whole app's default date format in one place
    /// instead of passing <c>Format</c> to every picker individually.
    /// </summary>
    public required string DefaultFormat { get; set; }

    /// <summary>
    /// Gets or sets the default .NET custom date/time format string (24-hour) used by
    /// <see cref="TwBlazor.Components.TwDateTimePicker"/> and <see cref="TwBlazor.Components.TwDateTimeRangePicker"/>
    /// when their own <c>Format</c> parameter isn't explicitly set and <c>Is12HourFormat</c> is
    /// <see langword="false"/>.
    /// </summary>
    public required string DefaultDateTimeFormat { get; set; }

    /// <summary>
    /// Gets or sets the default .NET custom date/time format string (12-hour, with an AM/PM
    /// designator) used by <see cref="TwBlazor.Components.TwDateTimePicker"/> and
    /// <see cref="TwBlazor.Components.TwDateTimeRangePicker"/> when their own <c>Format</c> parameter
    /// isn't explicitly set and <c>Is12HourFormat</c> is <see langword="true"/>.
    /// </summary>
    public required string DefaultDateTimeFormat12Hour { get; set; }

    /// <summary>
    /// Gets or sets the default separator used between the start and end date/time by
    /// <see cref="TwBlazor.Components.TwDateRangePicker"/> and <see cref="TwBlazor.Components.TwDateTimeRangePicker"/>
    /// when their own <c>RangeSeparator</c> parameter isn't explicitly set.
    /// </summary>
    public required string DefaultRangeSeparator { get; set; }

    /// <summary>
    /// Gets or sets the classes for the calendar icon's clickable wrapper within the trigger text field,
    /// shared by <see cref="TwBlazor.Components.TwDatePicker"/>, <see cref="TwBlazor.Components.TwDateRangePicker"/>
    /// and <see cref="TwBlazor.Components.TwDateTimeRangePicker"/>.
    /// </summary>
    public required string IconTriggerWrapper { get; set; }

    /// <summary>
    /// Gets or sets the classes for the calendar glyph itself.
    /// </summary>
    public required string IconGlyph { get; set; }

    /// <summary>
    /// Gets or sets the padding classes for the trigger text field, leaving room for the calendar icon.
    /// </summary>
    public required string TextfieldPadding { get; set; }

    /// <summary>
    /// Gets or sets the classes suppressing native browser chrome on the trigger field when the
    /// device's native date/time picker is active (see remarks on the callers' own <c>textfieldClasses</c>).
    /// </summary>
    public required string NativeInputAppearance { get; set; }

    /// <summary>
    /// Gets or sets the width classes for the popover panel's single-month layout, used by
    /// <see cref="TwBlazor.Components.TwDatePicker"/> and <see cref="TwBlazor.Components.TwDateTimeRangePicker"/>.
    /// <see cref="TwBlazor.Components.TwDateRangePicker"/> widens this further for its two-month layout.
    /// </summary>
    public required string PanelWidth { get; set; }

    /// <summary>
    /// Gets or sets the classes for the header row containing the previous/title/next navigation controls.
    /// </summary>
    public required string HeaderControls { get; set; }

    /// <summary>
    /// Gets or sets the classes for the popover body's outer wrapper.
    /// </summary>
    public required string Body { get; set; }

    /// <summary>
    /// Gets or sets the classes for the year quick-pick grid shown in the decade view.
    /// </summary>
    public required string YearsGrid { get; set; }

    /// <summary>
    /// Gets or sets the classes for the month quick-pick grid shown in the year view.
    /// </summary>
    public required string MonthsGrid { get; set; }

    /// <summary>
    /// Gets or sets the classes for the layout wrapping <see cref="TwBlazor.Components.TwDateRangePicker"/>'s
    /// two side-by-side month grids (stacked on narrow screens).
    /// </summary>
    public required string DualMonthLayout { get; set; }

    /// <summary>
    /// Gets or sets the classes for each column in <see cref="DualMonthLayout"/>.
    /// </summary>
    public required string DualMonthColumn { get; set; }

    /// <summary>
    /// Gets or sets the classes for the day-of-month grid's <c>&lt;table&gt;</c> element.
    /// </summary>
    public required string DayGrid { get; set; }

    /// <summary>
    /// Gets or sets the padding classes for each weekday column header cell.
    /// </summary>
    public required string DayHeaderCellPadding { get; set; }

    /// <summary>
    /// Gets or sets the classes for the weekday abbreviation shown in each column header.
    /// </summary>
    public required string DayAbbreviation { get; set; }

    /// <summary>
    /// Gets or sets the classes for the Start/End step tab row shown in <see cref="TwBlazor.Components.TwDateTimeRangePicker"/>'s popover.
    /// </summary>
    public required string RangeStageTabsContainer { get; set; }

    /// <summary>
    /// Gets or sets the text color for an inactive Start/End step tab.
    /// </summary>
    public required string RangeStageTabInactive { get; set; }

    /// <summary>
    /// Gets or sets the max-height/scroll classes applied to every popover panel, so a tall calendar
    /// stays reachable when there isn't room to show it in full (e.g. a phone with the keyboard open).
    /// </summary>
    public required string PanelMaxHeight { get; set; }

    /// <summary>
    /// Gets or sets the max-width/scroll classes applied to <see cref="TwBlazor.Components.TwDateRangePicker"/>'s
    /// wider two-month panel, layered on top of <see cref="PanelMaxHeight"/>.
    /// </summary>
    public required string PanelMaxWidth { get; set; }

    /// <summary>
    /// Gets or sets the extra panel width classes applied only in <see cref="TwBlazor.Components.TwDateRangePicker"/>'s
    /// day view, where both months are shown side by side.
    /// </summary>
    public required string DualMonthPanelWidth { get; set; }

    /// <summary>
    /// Gets or sets the base classes for a Start/End step tab button, shared with
    /// <see cref="TwBlazor.Components.TwDateTimeRangePicker"/>'s picker.
    /// </summary>
    public required string StageTabBase { get; set; }

    /// <summary>
    /// Gets or sets the classes for the header's previous/next navigation icon buttons.
    /// </summary>
    public required string HeaderNavButton { get; set; }
}
