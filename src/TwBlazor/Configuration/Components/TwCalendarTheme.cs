// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;

namespace TwBlazor.Configuration.Components;

/// <summary>
/// Theme configuration for the schedule/agenda component (<see cref="TwBlazor.Components.TwCalendar{T}"/>).
/// Override any property to customize schedule styles globally.
/// </summary>
[ExcludeFromCodeCoverage]
public class TwCalendarTheme
{
    /// <summary>
    /// Gets or sets the classes for the root container.
    /// </summary>
    public required string Container { get; set; }

    /// <summary>
    /// Gets or sets the classes for the header row (date label, search/nav/view/add controls).
    /// </summary>
    public required string Header { get; set; }

    /// <summary>
    /// Gets or sets the classes for the header's left-hand date/title block.
    /// </summary>
    public required string HeaderDateGroup { get; set; }

    /// <summary>
    /// Gets or sets the classes for the row pairing the header's title with its week-number
    /// <see cref="TwBlazor.Components.TwChip"/>.
    /// </summary>
    public required string HeaderTitleRow { get; set; }

    /// <summary>
    /// Gets or sets the classes for the header's title (e.g. "September 2026").
    /// </summary>
    public required string HeaderTitle { get; set; }

    /// <summary>
    /// Gets or sets the classes for the header's subtitle showing the current week's date range
    /// (e.g. "14 Sep 2026 - 20 Sep 2026").
    /// </summary>
    public required string HeaderSubtitle { get; set; }

    /// <summary>
    /// Gets or sets the classes for the Previous/Next navigation icon buttons.
    /// </summary>
    public required string NavButton { get; set; }

    /// <summary>
    /// Gets or sets the classes layered onto <see cref="NavButton"/> for the active view's icon in
    /// the Day/Week/Month view switcher.
    /// </summary>
    public required string ViewButtonActive { get; set; }

    /// <summary>
    /// Gets or sets the classes for the "add event" button shown when
    /// <see cref="TwBlazor.Components.TwCalendar{T}.Editable"/> is <see langword="true"/>.
    /// </summary>
    public required string AddButton { get; set; }

    /// <summary>
    /// Gets or sets the classes for the search icon button shown when
    /// <see cref="TwBlazor.Components.TwCalendar{T}.OnSearch"/> has a delegate.
    /// </summary>
    public required string SearchButton { get; set; }

    /// <summary>
    /// Gets or sets the classes for the Day/Week view's scrollable time-grid area. Combined with an
    /// inline <c>max-height</c> derived from <see cref="TwBlazor.Components.TwCalendar{T}.MaxHeight"/>,
    /// so this should only carry the scroll behavior itself (e.g. <c>overflow-y-auto</c>), not a
    /// fixed height.
    /// </summary>
    public required string ScrollContainer { get; set; }

    /// <summary>
    /// Gets or sets the classes for the Day/Week view's left-hand time-of-day label gutter.
    /// </summary>
    public required string TimeGutter { get; set; }

    /// <summary>
    /// Gets or sets the classes for a single time label within <see cref="TimeGutter"/>.
    /// </summary>
    public required string TimeGutterLabel { get; set; }

    /// <summary>
    /// Gets or sets the classes for the current-time label overlaid on <see cref="TimeGutter"/> (e.g.
    /// "4:09 pm") when today falls within the displayed Day/Week view - see
    /// <see cref="TwBlazor.Components.TwCalendarDayView{T}"/>/<see cref="TwBlazor.Components.TwCalendarWeekView{T}"/>.
    /// </summary>
    public required string NowIndicatorLabel { get; set; }

    /// <summary>
    /// Gets or sets the classes for the current-time horizontal line drawn across the Day/Week view's
    /// time grid.
    /// </summary>
    public required string NowIndicatorLine { get; set; }

    /// <summary>
    /// Gets or sets the classes for the dot marking today's column on <see cref="NowIndicatorLine"/>
    /// (only meaningfully distinct from the line's own left edge in the Week view, where the line
    /// spans every day column but the dot marks specifically which one is today).
    /// </summary>
    public required string NowIndicatorDot { get; set; }

    /// <summary>
    /// Gets or sets the classes for a single 30-minute slot row's background/divider.
    /// </summary>
    public required string SlotRow { get; set; }

    /// <summary>
    /// Gets or sets the classes added to a slot row on hover/focus when it's clickable (i.e.
    /// <see cref="TwBlazor.Components.TwCalendar{T}.Editable"/> is <see langword="true"/>).
    /// </summary>
    public required string SlotRowHover { get; set; }

    /// <summary>
    /// Gets or sets the classes for a day column's header in the Week view (e.g. "Mon 16").
    /// </summary>
    public required string DayColumnHeader { get; set; }

    /// <summary>
    /// Gets or sets the classes for the Week view header's full label (e.g. "Mon 16"). On a phone it is
    /// visually hidden, staying as the button's accessible name, and the compact label is shown instead.
    /// </summary>
    public required string DayColumnHeaderLabel { get; set; }

    /// <summary>
    /// Gets or sets the classes for the Week view header's compact label, shown only on a phone: the
    /// weekday stacked over the day number, so seven columns fit across a narrow screen.
    /// </summary>
    public required string DayColumnHeaderCompact { get; set; }

    /// <summary>
    /// Gets or sets the classes for the short weekday name in the compact header label.
    /// </summary>
    public required string DayColumnHeaderWeekday { get; set; }

    /// <summary>
    /// Gets or sets the classes for the day number in the compact header label.
    /// </summary>
    public required string DayColumnHeaderDayNumber { get; set; }

    /// <summary>
    /// Gets or sets the classes layered on <see cref="DayColumnHeaderDayNumber"/> for today's date.
    /// </summary>
    public required string DayColumnHeaderDayNumberToday { get; set; }

    /// <summary>
    /// Gets or sets the classes for the vertical divider between adjacent day columns in the Week view.
    /// </summary>
    public required string DayColumnDivider { get; set; }

    /// <summary>
    /// Gets or sets the classes for an event card rendered in the Day/Week time grid - a rounded,
    /// left-accent-bordered, top-left-aligned card (default styling; overridden per-event via inline
    /// style when <see cref="TwBlazor.Components.Schedule{T}.Color"/> is set - see
    /// <see cref="TwBlazor.Components.TwCalendarColors"/>).
    /// </summary>
    public required string EventChip { get; set; }

    /// <summary>
    /// Gets or sets the classes for an event card's title line (the event's name).
    /// </summary>
    public required string EventChipTitle { get; set; }

    /// <summary>
    /// Gets or sets the classes for an event card's subtitle line (its start time).
    /// </summary>
    public required string EventChipTime { get; set; }

    /// <summary>
    /// Gets or sets the classes layered on <see cref="EventChip"/> in the Week view, where a column is only a
    /// few characters wide on a phone: a thinner accent edge and tighter padding below the <c>sm</c> breakpoint.
    /// </summary>
    public required string EventChipCompact { get; set; }

    /// <summary>
    /// Gets or sets the classes layered on <see cref="EventChipTitle"/> in the Week view: smaller text on a phone.
    /// </summary>
    public required string EventChipTitleCompact { get; set; }

    /// <summary>
    /// Gets or sets the classes layered on <see cref="EventChipTime"/> in the Week view: the start time is
    /// hidden on a phone, where the chip's position shows it and its accessible name still states it.
    /// </summary>
    public required string EventChipTimeCompact { get; set; }

    /// <summary>
    /// Gets or sets the classes layered onto <see cref="EventChip"/> for a read-only event.
    /// </summary>
    public required string EventChipReadOnly { get; set; }

    /// <summary>
    /// Gets or sets the classes layered onto <see cref="EventChip"/>/<see cref="MonthEventRow"/> for
    /// an event that can be dragged to reschedule it (<see cref="TwBlazor.Components.TwCalendar{T}.Editable"/>
    /// and not <see cref="TwBlazor.Components.Schedule{T}.ReadOnly"/>).
    /// </summary>
    public required string EventChipDraggable { get; set; }

    /// <summary>
    /// Gets or sets the classes for the dashed placeholder shown where a dragged event will land when
    /// released, in the Day/Week time grid and the Month view's day cells. Positioning and
    /// <c>pointer-events-none</c> are applied by the views themselves.
    /// </summary>
    public required string DropPlaceholder { get; set; }

    /// <summary>
    /// Gets or sets the classes layered onto today's Week view column or Month view cell for a couple of
    /// seconds after "Today" is pressed: a tint plus a pulse animation to draw the eye to it.
    /// </summary>
    public required string TodayHighlight { get; set; }

    /// <summary>
    /// Gets or sets the classes for the all-day strip above the Day/Week time grid
    /// (<see cref="TwBlazor.Components.TwCalendarAllDayRow{T}"/>): its bottom divider.
    /// </summary>
    public required string AllDayRow { get; set; }

    /// <summary>
    /// Gets or sets the classes for the strip's "All day" label, which sits in the same left-hand
    /// column as <see cref="TimeGutter"/> so the strip lines up with the time grid beneath it.
    /// </summary>
    public required string AllDayLabel { get; set; }

    /// <summary>
    /// Gets or sets the classes for the strip's grid of day columns and event bars.
    /// </summary>
    public required string AllDayGrid { get; set; }

    /// <summary>
    /// Gets or sets the classes that inset an all-day bar slightly from its column edges, so bars on
    /// neighboring days don't touch.
    /// </summary>
    public required string AllDayChipInset { get; set; }

    /// <summary>
    /// Gets or sets the classes layered onto <see cref="MonthEventRow"/> for a bar that spans several
    /// days (in the all-day strip or the Month view) so it paints above the cells it crosses.
    /// </summary>
    public required string SegmentSpan { get; set; }

    /// <summary>
    /// Gets or sets the classes for a bar that continues from an earlier day than the first one shown
    /// (for example from the previous week): a square left edge.
    /// </summary>
    public required string SegmentContinuesBefore { get; set; }

    /// <summary>
    /// Gets or sets the classes for a bar that continues past the last day shown (for example into the
    /// next week): a square right edge.
    /// </summary>
    public required string SegmentContinuesAfter { get; set; }

    /// <summary>
    /// Gets or sets the classes for the invisible placeholder that keeps a Month view cell's own
    /// events below the multi-day bar passing through it. Its height must match
    /// <see cref="MonthEventRow"/>'s rendered height.
    /// </summary>
    public required string SegmentSpacer { get; set; }

    /// <summary>
    /// Gets or sets the classes for the Month view's <c>&lt;table&gt;</c> grid.
    /// </summary>
    public required string MonthGrid { get; set; }

    /// <summary>
    /// Gets or sets the classes for a single day cell in the Month view.
    /// </summary>
    public required string MonthCell { get; set; }

    /// <summary>
    /// Gets or sets the classes layered onto <see cref="MonthCell"/> for a lead-in/trailing day
    /// from the previous or next month.
    /// </summary>
    public required string MonthCellPrevNext { get; set; }

    /// <summary>
    /// Gets or sets the classes for a Month view cell's day-of-month number - block-level, so it
    /// always sits on its own row above the cell's event list rather than inline with it.
    /// </summary>
    public required string MonthDayNumber { get; set; }

    /// <summary>
    /// Gets or sets the classes for the column stacking a Month view cell's events below its day
    /// number.
    /// </summary>
    public required string MonthEventList { get; set; }

    /// <summary>
    /// Gets or sets the classes for a single event row inside <see cref="MonthEventList"/>: a
    /// rounded, left-accent-bordered, tinted card (matching <see cref="EventChip"/>'s default
    /// styling; overridden per-event via inline style when
    /// <see cref="TwBlazor.Components.Schedule{T}.Color"/> is set - see
    /// <see cref="TwBlazor.Components.TwCalendarColors"/>) holding the event's name and start time
    /// on one truncated line.
    /// </summary>
    public required string MonthEventRow { get; set; }

    /// <summary>
    /// Gets or sets the classes for a Month view event row's name text, which is smaller on a phone.
    /// </summary>
    public required string MonthEventName { get; set; }

    /// <summary>
    /// Gets or sets the classes for a Month view event row's start-time text. It is hidden on a phone, where a
    /// cell is only a few characters wide; the row's accessible name still states the time.
    /// </summary>
    public required string MonthEventTime { get; set; }

    /// <summary>
    /// Gets or sets the classes for the "+N more" overflow label shown in a Month view cell.
    /// </summary>
    public required string MonthOverflowLabel { get; set; }

    /// <summary>
    /// Gets or sets the classes for the field stack in the built-in event create/edit/view dialog
    /// (<see cref="TwBlazor.Components.TwCalendarEventDialog{T}"/>).
    /// </summary>
    public required string EventDialogFields { get; set; }

    /// <summary>
    /// Gets or sets the classes for the event dialog's footer button row.
    /// </summary>
    public required string EventDialogActions { get; set; }

    /// <summary>
    /// Gets or sets the classes for the thin accent bar at the top of the edit dialog's form. Its
    /// background switches to the event's own <see cref="TwBlazor.Components.Schedule{T}.Color"/> via
    /// inline style as the user picks one, so this only needs to carry the fallback color.
    /// </summary>
    public required string EventDialogAccent { get; set; }

    /// <summary>
    /// Gets or sets the classes for the Start/End row in the edit dialog: one column on narrow
    /// screens, two side by side from the <c>sm</c> breakpoint.
    /// </summary>
    public required string EventDialogDateRow { get; set; }

    /// <summary>
    /// Gets or sets the classes for the read-only dialog's highlight card holding the event's date and
    /// time: a rounded, left-accent-bordered, tinted block (tinted with the event's color via inline
    /// style when one is set).
    /// </summary>
    public required string EventDialogSummary { get; set; }

    /// <summary>
    /// Gets or sets the classes for the calendar icon inside <see cref="EventDialogSummary"/>.
    /// </summary>
    public required string EventDialogSummaryIcon { get; set; }

    /// <summary>
    /// Gets or sets the classes for one label/value pair (e.g. "Start") in the read-only event
    /// dialog's body.
    /// </summary>
    public required string EventDialogReadOnlyField { get; set; }

    /// <summary>
    /// Gets or sets the classes for a read-only field's label text.
    /// </summary>
    public required string EventDialogReadOnlyLabel { get; set; }

    /// <summary>
    /// Gets or sets the classes for a read-only field's value text.
    /// </summary>
    public required string EventDialogReadOnlyValue { get; set; }
}
