// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using Microsoft.AspNetCore.Components;
using System.Globalization;
using TwBlazor.Configuration.Components;
using TwBlazor.Utilities;

namespace TwBlazor.Components;

/// <summary>
/// Represents a time range picker that lets users select a start and end time-of-day pair from a
/// single popover, switching between the two via a Start/End tab at the top.
/// </summary>
/// <remarks>
/// Has one merged trigger field like <see cref="TwDateTimeRangePicker"/>/<see cref="TwDateRangePicker"/>,
/// but reuses <see cref="TimePicker.TwTimePickerBody"/> - the same body <see cref="TwTimePicker"/> uses -
/// for its actual editing surface instead of a calendar. Unlike the date-based range pickers, the two
/// sides are edited independently and applied immediately as the hour/minute steppers change (there's
/// no discrete "pick a day" action to hang a commit off), and typed/selected values are never reordered:
/// an end time earlier than the start (e.g. a "22:00 - 06:00" overnight shift) is a legitimate range
/// here, not a mistake to correct.
/// </remarks>
public partial class TwTimeRangePicker : TwPopoverPickerComponentBase
{
    private TwTimePickerTheme theme => options.Theme.Components.Require<TwTimePickerTheme>();

    private TwPopoverTheme popoverTheme => options.Theme.Components.Require<TwOverlayTheme>().Popover;

    /// <summary>
    /// Reference to the trigger <see cref="TwTextfield{T}"/> instance, used to focus its actual
    /// &lt;input&gt; element directly - see <see cref="TwPopoverPickerComponentBase.triggerInputRef"/>.
    /// </summary>
    private TwTextfield<string>? trigger;

    /// <inheritdoc />
    protected override ElementReference? triggerInputRef => trigger?.InputRef;

    /// <summary>
    /// Which half of the range is currently being edited. Reset to <see cref="TimeRangePickerStage.Start"/>
    /// whenever the panel opens (see <see cref="OnFocusAsync"/>) so every visit starts the same way,
    /// but can also be switched directly via the Start/End tabs (see <see cref="SwitchStage"/>).
    /// </summary>
    private TimeRangePickerStage stage;

    /// <summary>
    /// The placeholder text to display when no range is selected.
    /// </summary>
    [Parameter] public string? Placeholder { get; set; }

    private string effectivePlaceholder => Placeholder ??
        $"{format.ToLower(CultureInfo.CurrentCulture)}{RangeSeparator}{format.ToLower(CultureInfo.CurrentCulture)}";

    /// <summary>
    /// The selected time range: <c>Key</c> is the start, <c>Value</c> is the end. Either (or both)
    /// may be <see langword="null"/> - both null means no range has been started yet, and a
    /// non-null <c>Key</c> with a null <c>Value</c> means only the start has been picked so far.
    /// Unlike <see cref="TwDateTimeRangePicker.SelectedRange"/>, <c>Key</c> is not required to be
    /// less than or equal to <c>Value</c> - an end time earlier than the start represents a range
    /// that crosses midnight.
    /// </summary>
    [Parameter] public KeyValuePair<TimeOnly?, TimeOnly?> SelectedRange { get; set; }

    /// <summary>
    /// The bound <see cref="SelectedRange"/> value; invoked whenever it changes, including after
    /// only the start has been picked (<c>Value</c> still <see langword="null"/>) so callers can
    /// reflect an in-progress selection.
    /// </summary>
    [Parameter] public EventCallback<KeyValuePair<TimeOnly?, TimeOnly?>> SelectedRangeChanged { get; set; }

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
    /// The .NET custom time format string used to display and parse each side of
    /// <see cref="SelectedRange"/>. Leave unset to fall back to 'HH:mm' (or 'hh:mm tt' when
    /// <see cref="Is12HourFormat"/> is set).
    /// </summary>
    [Parameter] public string? Format { get; set; }

    /// <summary>
    /// The format actually used: <see cref="Format"/> when explicitly set, otherwise 'HH:mm'/'hh:mm tt'
    /// depending on <see cref="Is12HourFormat"/> - the same fallback <see cref="TwTimePicker"/> uses.
    /// </summary>
    private string format => Format ?? (Is12HourFormat ? "hh:mm tt" : "HH:mm");

    /// <summary>
    /// The separator inserted between the two times in <see cref="Value"/>. Default " - ".
    /// </summary>
    [Parameter] public string RangeSeparator { get; set; } = " - ";

    /// <summary>
    /// The side of <see cref="SelectedRange"/> currently bound to the time editor, falling back to
    /// the current time when that side hasn't been set yet - the same default <see cref="TwTimePicker.SelectedTime"/> uses.
    /// </summary>
    private TimeOnly stageTime => (stage == TimeRangePickerStage.Start ? SelectedRange.Key : SelectedRange.Value)
        ?? TimeOnly.FromDateTime(DateTime.Now);

    private string classes => new ClassBuilder(options.Theme.Position.Relative)
        .AddClass(options.Theme.Display.Flex)
        .AddClass(options.Theme.Flexbox.Col)
        .AddClass(RootClass)
        .AddClass(Class).Build();

    private string textfieldClasses => new ClassBuilder(theme.TextfieldPadding).Build();

    private string panelPositionClasses => new ClassBuilder(theme.PanelWrapper).Build();

    private string panelSurfaceClasses =>
        popoverBuilder.GetSurfaceClasses(Rounded, Shadow, popoverTheme.TimeRangeSize);

    private string stageTabsClasses => new ClassBuilder(theme.RangeStageTabsContainer).Build();

    /// <summary>
    /// Gets the CSS classes for the Start/End step tab buttons.
    /// </summary>
    private string GetStageTabClasses(TimeRangePickerStage tabStage)
    {
        var isActive = stage == tabStage;
        return new ClassBuilder(theme.StageTabBase)
            .AddClass(roundedBuilder.GetRounded())
            .AddClass(theme.StageTabHover, !isActive)
            .AddClass(theme.RangeStageTabInactive, !isActive)
            .AddClass(theme.StageTabActive, isActive)
            .Build();
    }

    /// <summary>
    /// Initializes the component, seeding the trigger's text value.
    /// </summary>
    protected override void OnInitialized()
    {
        base.OnInitialized();
        Value = FormatRange(SelectedRange);
    }

    /// <summary>
    /// Re-seeds the picker's step state every time the panel opens: always starts back at
    /// <see cref="TimeRangePickerStage.Start"/>, regardless of what was being edited last time.
    /// </summary>
    protected override async Task OnFocusAsync()
    {
        stage = TimeRangePickerStage.Start;
        await base.OnFocusAsync();
    }

    /// <summary>
    /// Arms the panel's Tab focus trap/background inert-ing when it first opens - same mechanics as
    /// <see cref="TwTimePicker.OnAfterRenderAsync"/>. Deliberately does not move focus into the panel,
    /// same rationale as <see cref="TwTimePicker"/>: the trigger stays a text-editable combobox.
    /// </summary>
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);

        // Re-run on every render rather than gating behind a one-shot "just opened" flag - see the
        // matching remarks on TwDatePicker.OnAfterRenderAsync for why.
        if (isFocused && PanelRef.Context != null)
        {
            await RegisterPanelScrollBehaviorAsync(PanelRef);
            await ApplyPanelTrapAsync(PanelRef);
        }
    }

    /// <summary>
    /// Handles text input changes and attempts to parse two times, separated by
    /// <see cref="RangeSeparator"/>, using <see cref="format"/>.
    /// </summary>
    /// <remarks>
    /// Unlike <see cref="TwDateTimeRangePicker.OnTextValueChanged"/>/<see cref="TwDateRangePicker.OnTextValueChanged"/>,
    /// a reversed pair is never swapped: a time range can legitimately cross midnight (e.g. "22:00 - 06:00"),
    /// so there's no way to tell an overnight range apart from a genuinely reversed one just from the values.
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

        var parts = text.Split(RangeSeparator, StringSplitOptions.TrimEntries);
        var start = default(TimeOnly);
        var end = default(TimeOnly);
        var success = parts.Length == 2
            && TimeOnly.TryParseExact(parts[0], format, CultureInfo.CurrentCulture, DateTimeStyles.None, out start)
            && TimeOnly.TryParseExact(parts[1], format, CultureInfo.CurrentCulture, DateTimeStyles.None, out end);

        if (!success)
        {
            Invalid = true;
            ErrorMessage = "Enter a valid time range";
            Value = text;
            return;
        }

        stage = TimeRangePickerStage.Start;
        await ApplyRangeAsync(new KeyValuePair<TimeOnly?, TimeOnly?>(start, end));
    }

    /// <summary>
    /// Formats a range for display: empty when no start (<c>Key</c>) is set, just the start when
    /// the end (<c>Value</c>) isn't picked yet, otherwise both joined by <see cref="RangeSeparator"/>.
    /// </summary>
    private string FormatRange(KeyValuePair<TimeOnly?, TimeOnly?> range)
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

        return $"{start}{RangeSeparator}{range.Value.Value.ToString(format, CultureInfo.CurrentCulture)}";
    }

    /// <summary>
    /// Manually switches to the given step - lets a user jump directly to either side rather than
    /// being limited to a fixed order.
    /// </summary>
    private void SwitchStage(TimeRangePickerStage newStage) => stage = newStage;

    /// <summary>
    /// Handles a change from the time editor, applying it to whichever side is currently active
    /// immediately - there's no separate "day" click to hang a commit off, unlike the date-based
    /// range pickers, so every stepper adjustment applies straight away.
    /// </summary>
    private Task OnStageTimeChanged(TimeOnly newTime) => stage == TimeRangePickerStage.Start
        ? ApplyRangeAsync(new KeyValuePair<TimeOnly?, TimeOnly?>(newTime, SelectedRange.Value))
        : ApplyRangeAsync(new KeyValuePair<TimeOnly?, TimeOnly?>(SelectedRange.Key, newTime));

    /// <summary>
    /// Applies a new range, updates the displayed <see cref="Value"/>, and invokes
    /// <see cref="SelectedRangeChanged"/>/<see cref="ValueChanged"/>.
    /// </summary>
    private async Task ApplyRangeAsync(KeyValuePair<TimeOnly?, TimeOnly?> range)
    {
        SelectedRange = range;
        Value = FormatRange(range);
        Invalid = false;
        ErrorMessage = string.Empty;

        // Captured before either callback fires, same reentrancy guard TwDateTimeRangePicker.ApplyRangeAsync uses.
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
    /// Defines which side of the range is currently being edited.
    /// </summary>
    private enum TimeRangePickerStage
    {
        /// <summary>
        /// The start time is being edited.
        /// </summary>
        Start,

        /// <summary>
        /// The end time is being edited.
        /// </summary>
        End
    }
}
