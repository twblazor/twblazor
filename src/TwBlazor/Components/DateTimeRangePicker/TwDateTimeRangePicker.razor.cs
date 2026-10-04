// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Globalization;
using TwBlazor.Components.DatePicker;
using TwBlazor.Configuration.Components;
using TwBlazor.Utilities;

namespace TwBlazor.Components;

/// <summary>
/// Represents a date and time range picker that lets users select a start and end date/time pair
/// from a single popover, one step at a time: the start date and time first, then the end.
/// </summary>
/// <remarks>
/// <para>
/// Has one merged trigger field like <see cref="TwDateRangePicker"/>, but its popover only ever
/// shows one month plus one time editor at a time - like <see cref="TwDateTimePicker"/> - instead
/// of <see cref="TwDateRangePicker"/>'s two-month layout. That keeps each step small enough to use
/// comfortably on a phone.
/// </para>
/// <para>
/// It's built from the same pieces as the other pickers: <see cref="TwPopoverPickerComponentBase"/>
/// for open/close/focus-trap behavior, <see cref="DatePicker.TwDatePickerCalendar"/> for the
/// calendar itself (in range mode), and <see cref="TimePicker.TwTimePickerBody"/> for the time
/// editor.
/// </para>
/// <para>
/// While picking the end, days before the start simply render disabled rather than being picked
/// and swapped afterward - there's no natural "swap" once start and end are two separate steps.
/// </para>
/// </remarks>
public partial class TwDateTimeRangePicker : TwPopoverPickerComponentBase
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
    private DatePickerCalendarView view;

    /// <summary>
    /// Which half of the range is currently being picked. Reset to <see cref="DateTimeRangePickerStage.Start"/>
    /// whenever the panel opens (see <see cref="OnFocusAsync"/>) so every visit starts the same
    /// two-step flow from the beginning, regardless of what was picked last time - but can also be
    /// switched directly via the Start/End tabs (see <see cref="SwitchStage"/>), so a user isn't
    /// locked into picking strictly in order.
    /// </summary>
    private DateTimeRangePickerStage stage;

    /// <summary>
    /// The month currently displayed. Purely internal navigation state (not a bound parameter) -
    /// seeded from whichever side of <see cref="SelectedRange"/> is being picked (or <see cref="MinDate"/>,
    /// or today) each time the panel opens, then moves independently as the user navigates.
    /// </summary>
    private DateTime anchorDate;

    /// <summary>
    /// The time-of-day currently staged for whichever side of <see cref="SelectedRange"/> is being
    /// picked - edited via the time editor before/after clicking a day, and combined with that day
    /// when it's clicked. Re-seeded from the existing start/end time (or the current time) whenever
    /// the panel opens or the stage advances from start to end.
    /// </summary>
    private TimeOnly pendingTime;

    /// <summary>
    /// The placeholder text to display when no range is selected.
    /// </summary>
    [Parameter] public string? Placeholder { get; set; }

    private string effectivePlaceholder => Placeholder ??
        $"{format.ToLower(CultureInfo.CurrentCulture)}{resolvedRangeSeparator}{format.ToLower(CultureInfo.CurrentCulture)}";

    /// <summary>
    /// The selected date/time range: <c>Key</c> is the start, <c>Value</c> is the end. Either (or
    /// both) may be <see langword="null"/> - both null means no range has been started yet, and a
    /// non-null <c>Key</c> with a null <c>Value</c> means only the start has been picked so far.
    /// <c>Key</c> is always less than or equal to <c>Value</c> when both are set.
    /// </summary>
    [Parameter] public KeyValuePair<DateTime?, DateTime?> SelectedRange { get; set; }

    /// <summary>
    /// The bound <see cref="SelectedRange"/> value; invoked whenever it changes, including after
    /// only the start has been picked (<c>Value</c> still <see langword="null"/>) so callers can
    /// reflect an in-progress selection.
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
    /// Gets or sets a value indicating whether the time should be displayed in 12-hour format.
    /// Has no effect when <see cref="Format"/> is explicitly set.
    /// </summary>
    [Parameter] public bool Is12HourFormat { get; set; }

    /// <summary>
    /// The .NET custom date/time format string used to display and parse each side of
    /// <see cref="SelectedRange"/>. Leave unset to fall back to
    /// <see cref="TwDatePickerTheme.DefaultDateTimeFormat"/> or
    /// <see cref="TwDatePickerTheme.DefaultDateTimeFormat12Hour"/> depending on <see cref="Is12HourFormat"/>.
    /// </summary>
    [Parameter] public string? Format { get; set; }

    /// <summary>
    /// The separator inserted between the two date/times in <see cref="Value"/>. Leave unset to
    /// fall back to <see cref="TwDatePickerTheme.DefaultRangeSeparator"/> (default " - ").
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
    /// <see cref="OnAfterRenderAsync"/> reclaims focus inside the panel, and also when the stage
    /// advances from start to end (the day grid's contents change under the same view).
    /// </summary>
    private bool pendingViewFocus;

    /// <summary>
    /// The format actually used: <see cref="Format"/> when explicitly set, otherwise whichever of
    /// <see cref="TwDatePickerTheme.DefaultDateTimeFormat"/>/<see cref="TwDatePickerTheme.DefaultDateTimeFormat12Hour"/>
    /// matches <see cref="Is12HourFormat"/> - the same fallback <see cref="TwDateTimePicker"/> uses.
    /// </summary>
    private string format => Format ?? (Is12HourFormat ? theme.DefaultDateTimeFormat12Hour : theme.DefaultDateTimeFormat);

    /// <summary>
    /// The side of <see cref="SelectedRange"/> relevant to the current <see cref="stage"/> - used to
    /// decide which month/year quick-pick button (if any) renders as selected.
    /// </summary>
    private DateTime? stageDate => stage == DateTimeRangePickerStage.Start ? SelectedRange.Key : SelectedRange.Value;

    /// <summary>
    /// The effective lower bound for the day grid: <see cref="MinDate"/>, except while picking the
    /// end (<see cref="DateTimeRangePickerStage.End"/>) once a start has been picked, where it's
    /// raised to the start's date so the end can never land before it.
    /// </summary>
    private DateTime? effectiveMinDate
    {
        get
        {
            if (stage != DateTimeRangePickerStage.End || !SelectedRange.Key.HasValue)
            {
                return MinDate;
            }

            var startIsLaterThanMinDate = MinDate.HasValue && MinDate.Value.Date > SelectedRange.Key.Value.Date;
            return startIsLaterThanMinDate ? MinDate : SelectedRange.Key;
        }
    }

    private string classes => new ClassBuilder(options.Theme.Position.Relative)
        .AddClass(options.Theme.Display.Flex)
        .AddClass(options.Theme.Flexbox.Col)
        .AddClass(RootClass)
        .AddClass(Class).Build();

    private string textfieldClasses => new ClassBuilder(theme.TextfieldPadding).Build();

    // The popover's own container (TwDatePickerTheme.Base) only pads its sides and bottom (px-2
    // pb-2) - the top is deliberately left bare so the calendar header can sit flush against it as
    // a title bar. This tab row sits above that header instead, so it needs its own top padding to
    // avoid looking cramped against the popover's top edge; pt-2 matches the container's own px-2/
    // pb-2 spacing so the whole panel reads as evenly padded.
    private string stageTabsClasses => new ClassBuilder(theme.RangeStageTabsContainer).Build();

    private string datepickerContainerClasses => new ClassBuilder(theme.PanelMaxHeight)
        .AddClass(popoverBuilder.GetSurfaceClasses(Rounded, Shadow))
        .AddClass(theme.Base)
        .Build();

    /// <summary>
    /// Gets or sets the CSS class names to apply to the body element of the component.
    /// </summary>
    [Parameter] public string BodyClasses { get; set; } = string.Empty;

    /// <summary>
    /// Gets the CSS classes for the Start/End step tab buttons.
    /// </summary>
    private string GetStageTabClasses(DateTimeRangePickerStage tabStage)
    {
        var isActive = stage == tabStage;
        return new ClassBuilder(theme.StageTabBase)
            .AddClass(roundedBuilder.GetRounded())
            .AddClass(options.Theme.Colors.HoverColors.Primary, !isActive)
            .AddClass(theme.RangeStageTabInactive, !isActive)
            .AddClass(options.Theme.Colors.LightBackground.Light.Primary, isActive)
            .AddClass(options.Theme.Colors.DarkBackground.Light.Primary, isActive)
            .AddClass(options.Theme.Colors.TextColors.Medium.Primary, isActive)
            .AddClass(options.Theme.Colors.DarkTextColors.Medium.Primary, isActive)
            .Build();
    }

    /// <summary>
    /// Initializes the component, seeding the displayed month and the trigger's text value.
    /// </summary>
    protected override void OnInitialized()
    {
        base.OnInitialized();
        anchorDate = SelectedRange.Key ?? MinDate ?? DateTime.Today;
        Value = FormatRange(SelectedRange);
    }

    /// <summary>
    /// Re-seeds the picker's step state every time the panel opens: always starts back at
    /// <see cref="DateTimeRangePickerStage.Start"/>, re-anchors the displayed month to the current
    /// start (or falls back to <see cref="MinDate"/>/today), and stages the start's existing
    /// time-of-day (or now, if unset) into the time editor.
    /// </summary>
    protected override async Task OnFocusAsync()
    {
        stage = DateTimeRangePickerStage.Start;
        view = DatePickerCalendarView.Day;
        anchorDate = SelectedRange.Key ?? MinDate ?? DateTime.Today;
        pendingTime = SelectedRange.Key.HasValue ? TimeOnly.FromDateTime(SelectedRange.Key.Value) : TimeOnly.FromDateTime(DateTime.Now);
        await base.OnFocusAsync();
    }

    /// <summary>
    /// Arms the panel's Tab focus trap/background inert-ing when it first opens, and reclaims
    /// focus inside it after a view switch or stage change - same mechanics as
    /// <see cref="TwDatePicker.OnAfterRenderAsync"/>, minus its native-picker detection (there is
    /// no native input here).
    /// </summary>
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);

        if (isFocused && PanelRef.Context != null)
        {
            // Re-run on every render rather than gating behind a one-shot "just opened" flag - see
            // the matching remarks on TwDatePicker.OnAfterRenderAsync for why.
            await RegisterPanelScrollBehaviorAsync(PanelRef);
            await ApplyPanelTrapAsync(PanelRef);

            if (pendingViewFocus)
            {
                pendingViewFocus = false;
                await JSRuntime.InvokeVoidAsync("twDialog.focusSurface", PanelRef);
            }
        }
    }

    /// <summary>
    /// Handles text input changes and attempts to parse two date/times, separated by
    /// <see cref="RangeSeparator"/>, using <see cref="format"/>.
    /// </summary>
    /// <remarks>
    /// Unlike the panel's step-by-step flow, typing sets both sides of the range at once - same
    /// approach as <see cref="TwDateRangePicker.OnTextValueChanged"/>, including swapping a reversed
    /// pair rather than rejecting it (a single merged field has nowhere else for the values to go).
    /// </remarks>
    private async Task OnTextValueChanged(string? text)
    {
        if (ReadOnly || Disabled)
            return;

        if (isFocused)
        {
            await ReleasePanelTrapAsync();
        }
        isFocused = false;
        FocusReturnToken = null;

        if (string.IsNullOrWhiteSpace(text))
        {
            return;
        }

        var parts = text.Split(resolvedRangeSeparator, StringSplitOptions.TrimEntries);
        var start = default(DateTime);
        var end = default(DateTime);
        var success = parts.Length == 2
            && DateTime.TryParseExact(parts[0], format, CultureInfo.CurrentCulture, DateTimeStyles.None, out start)
            && DateTime.TryParseExact(parts[1], format, CultureInfo.CurrentCulture, DateTimeStyles.None, out end)
            && !IsOutOfBounds(start) && !IsOutOfBounds(end);

        if (!success)
        {
            Invalid = true;
            ErrorMessage = "Enter a valid date range";
            Value = text;
            return;
        }

        var ordered = start <= end
            ? new KeyValuePair<DateTime?, DateTime?>(start, end)
            : new KeyValuePair<DateTime?, DateTime?>(end, start);

        stage = DateTimeRangePickerStage.Start;
        anchorDate = ordered.Key!.Value;
        await ApplyRangeAsync(ordered);
    }

    private bool IsOutOfBounds(DateTime date) =>
        (MinDate.HasValue && date.Date < MinDate.Value.Date) || (MaxDate.HasValue && date.Date > MaxDate.Value.Date);

    /// <summary>
    /// Formats a range for display: empty when no start (<c>Key</c>) is set, just the start when
    /// the end (<c>Value</c>) isn't picked yet, otherwise both joined by <see cref="RangeSeparator"/>.
    /// </summary>
    private string FormatRange(KeyValuePair<DateTime?, DateTime?> range)
    {
        if (range.Key is null)
        {
            return string.Empty;
        }

        var start = range.Key.Value.ToString(format, CultureInfo.CurrentCulture);
        if (range.Value is null)
        {
            return start;
        }

        return $"{start}{resolvedRangeSeparator}{range.Value.Value.ToString(format, CultureInfo.CurrentCulture)}";
    }

    /// <summary>
    /// Combines a day (only its year/month/day are used) with a time-of-day into one
    /// <see cref="DateTime"/>, preserving <paramref name="date"/>'s <see cref="DateTime.Kind"/>.
    /// </summary>
    private static DateTime CombineDateAndTime(DateTime date, TimeOnly time) =>
        DateTime.SpecifyKind(DateOnly.FromDateTime(date).ToDateTime(time), date.Kind);

    /// <summary>
    /// Manually switches to the given step - used by the Start/End tabs so a user isn't limited to
    /// strictly picking start-then-end. Re-anchors the calendar to that side's existing value if it
    /// has one; otherwise leaves the calendar wherever it currently is, so free browsing isn't
    /// disrupted.
    /// </summary>
    private void SwitchStage(DateTimeRangePickerStage newStage)
    {
        stage = newStage;

        var reference = newStage == DateTimeRangePickerStage.Start ? SelectedRange.Key : SelectedRange.Value;
        if (reference.HasValue)
        {
            anchorDate = reference.Value;
            pendingTime = TimeOnly.FromDateTime(reference.Value);
        }

        view = DatePickerCalendarView.Day;
        pendingViewFocus = true;
    }

    /// <summary>
    /// Handles a change from the time editor, staging it for the day that's about to be clicked
    /// and, if the current stage's date is already set (e.g. re-adjusting an already-picked side),
    /// immediately recombining and applying it so the change is reflected right away.
    /// </summary>
    private Task OnPendingTimeChanged(TimeOnly newTime)
    {
        pendingTime = newTime;

        return stage switch
        {
            DateTimeRangePickerStage.Start when SelectedRange.Key.HasValue =>
                ApplyRangeAsync(new KeyValuePair<DateTime?, DateTime?>(CombineDateAndTime(SelectedRange.Key.Value, newTime), SelectedRange.Value)),
            DateTimeRangePickerStage.End when SelectedRange.Value.HasValue =>
                ApplyRangeAsync(new KeyValuePair<DateTime?, DateTime?>(SelectedRange.Key, CombineDateAndTime(SelectedRange.Value.Value, newTime))),
            _ => Task.CompletedTask
        };
    }

    /// <summary>
    /// Selects a day clicked in the calendar, combining it with <see cref="pendingTime"/>.
    /// </summary>
    /// <remarks>
    /// What happens next depends on the result, not just which step was active:
    /// <list type="bullet">
    /// <item>Picking the start always advances to the end step and keeps the panel open.</item>
    /// <item>Picking the end closes the panel only once both sides are set.</item>
    /// <item>
    /// If the end was picked with no start yet - possible via the Start/End tabs, see
    /// <see cref="SwitchStage"/> - it switches back to the start step instead of closing, so the
    /// range can still be completed.
    /// </item>
    /// </list>
    /// Out-of-bounds or too-early days never reach this method in the first place, since the
    /// calendar's own <see cref="effectiveMinDate"/>/<see cref="MaxDate"/> already disable them.
    /// </remarks>
    private async Task SelectDateAsync(DateTime day)
    {
        var pickedDateTime = CombineDateAndTime(day, pendingTime);
        var pickedStart = stage == DateTimeRangePickerStage.Start;

        KeyValuePair<DateTime?, DateTime?> next;
        if (pickedStart)
        {
            // A previously-picked end that's now before the new start can no longer stand -
            // dropped rather than silently reordered, since the user is about to be walked through
            // picking the end again anyway.
            var end = SelectedRange.Value.HasValue && SelectedRange.Value.Value >= pickedDateTime ? SelectedRange.Value : null;
            next = new KeyValuePair<DateTime?, DateTime?>(pickedDateTime, end);
        }
        else
        {
            next = new KeyValuePair<DateTime?, DateTime?>(SelectedRange.Key, pickedDateTime);
        }

        await ApplyRangeAsync(next);

        if (pickedStart)
        {
            stage = DateTimeRangePickerStage.End;
            anchorDate = pickedDateTime;
            pendingTime = next.Value.HasValue ? TimeOnly.FromDateTime(next.Value.Value) : pendingTime;
            view = DatePickerCalendarView.Day;
            pendingViewFocus = true;
        }
        else if (next.Key.HasValue)
        {
            // Both sides are now set - the range is complete, close the panel.
            if (isFocused)
            {
                await ReleasePanelTrapAsync();
            }
            isFocused = false;
            stage = DateTimeRangePickerStage.Start;
            await RestoreFocusAsync();
        }
        else
        {
            // The end was picked before a start exists (the user switched to the End tab first) -
            // switch back to the start step so the range can still be completed, keeping the panel open.
            SwitchStage(DateTimeRangePickerStage.Start);
        }
    }

    /// <summary>
    /// Applies a new range, updates the displayed <see cref="Value"/>, and invokes
    /// <see cref="SelectedRangeChanged"/>/<see cref="ValueChanged"/>.
    /// </summary>
    private async Task ApplyRangeAsync(KeyValuePair<DateTime?, DateTime?> range)
    {
        SelectedRange = range;
        Value = FormatRange(range);
        Invalid = false;
        ErrorMessage = string.Empty;

        // Captured before either callback fires, same reentrancy guard TwDateRangePicker.SelectRangeDateAsync uses.
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

    /// <summary>
    /// Defines which side of the range is currently being picked.
    /// </summary>
    private enum DateTimeRangePickerStage
    {
        /// <summary>
        /// The start date/time is being picked.
        /// </summary>
        Start,

        /// <summary>
        /// The end date/time is being picked.
        /// </summary>
        End
    }
}
