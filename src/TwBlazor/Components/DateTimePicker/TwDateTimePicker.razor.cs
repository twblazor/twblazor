// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using Microsoft.AspNetCore.Components;
using TwBlazor.Configuration.Components;
using System.Globalization;
using TwBlazor.Localization;
using TwBlazor.Utilities;

namespace TwBlazor.Components;

/// <summary>
/// Represents a date and time picker component that allows users to select and edit date and time values, supporting
/// both 12-hour and 24-hour formats.
/// </summary>
/// <remarks>Use the TwDateTimePicker component to provide an interactive UI for selecting date and time values in
/// Blazor applications. The component supports two-way binding for both the selected DateTime value and its string
/// representation, and can be configured to display time in either 12-hour (AM/PM) or 24-hour format. The placeholder
/// text can be customized to guide users when no value is selected.</remarks>
public partial class TwDateTimePicker : TwBlazorTextInputComponentBase
{
    private TwDatePickerTheme theme => options.Theme.Components.Require<TwDatePickerTheme>();

    /// <summary>
    /// The .NET custom date/time format string used to display and parse <see cref="SelectedDateTime"/>.
    /// Leave unset to fall back to <see cref="TwDatePickerTheme.DefaultDateTimeFormat"/> or
    /// <see cref="TwDatePickerTheme.DefaultDateTimeFormat12Hour"/> depending on <see cref="Is12HourFormat"/>.
    /// </summary>
    [Parameter] public string? Format { get; set; }

    /// <summary>
    /// The format actually used: <see cref="Format"/> when explicitly set, otherwise whichever of
    /// <see cref="TwDatePickerTheme.DefaultDateTimeFormat"/>/<see cref="TwDatePickerTheme.DefaultDateTimeFormat12Hour"/>
    /// matches <see cref="Is12HourFormat"/>.
    /// </summary>
    private string format => Format ?? localizer.Formats.DateTimeInput ?? (effectiveIs12HourFormat ? theme.DefaultDateTimeFormat12Hour : theme.DefaultDateTimeFormat);

    /// <summary>
    /// Gets or sets a value indicating whether the time should be displayed in 12-hour format.
    /// </summary>
    /// <remarks>Set this property to <see langword="true"/> to use 12-hour time representation (with AM/PM);
    /// otherwise, 24-hour format will be used. Has no effect when <see cref="Format"/> is explicitly set.</remarks>
    [Parameter] public bool? Is12HourFormat { get; set; }

    /// <summary>
    /// Gets whether a 12 hour clock is used: <see cref="Is12HourFormat"/> when set, otherwise the localization options.
    /// </summary>
    private bool effectiveIs12HourFormat => Is12HourFormat ?? localizer.Formats.Use12HourInput;
    /// <summary>
    /// Gets or sets the step, in minutes, that the time step buttons and Arrow Up/Arrow Down move the time
    /// by. Steps under an hour move the minutes (e.g. 15 gives :00, :15, :30, :45); steps of an hour or more
    /// move the hours (e.g. 120 steps two hours at a time) and the minutes show as fixed text. Must divide an hour or, for
    /// whole hours, a day. Defaults to 1. Typed times are not rounded to the step.
    /// </summary>
    [Parameter] public int Step { get; set; } = 1;

    /// <summary>
    /// Gets the native <c>datetime-local</c> step in seconds, or <see langword="null"/> for the browser default.
    /// </summary>
    private string? nativeStep => Step == 1 ? null : (Step * 60).ToString(CultureInfo.InvariantCulture);

    /// <inheritdoc />
    protected override void OnParametersSet()
    {
        TimeStepHelpers.ValidateStep(Step);
        base.OnParametersSet();
    }

    /// <summary>
    /// The selected <see cref="DateTime"/> value.
    /// </summary>
    [Parameter] public DateTime SelectedDateTime { get; set; } = DateTime.UtcNow;
    /// <summary>
    /// The selected bound <see cref="DateTime"/> value.
    /// </summary>
    [Parameter] public EventCallback<DateTime> SelectedDateTimeChanged { get; set; }
    /// <summary>
    /// The <see cref="string" /> value displayed in the textbox to the user.
    /// </summary>
    [Parameter] public string? Value { get; set; }
    /// <summary>
    /// The <see cref="string" /> bound value displayed in the textbox to the user.
    /// </summary>
    [Parameter] public EventCallback<string> ValueChanged { get; set; }
    /// <summary>
    /// The placeholder text to display when no date is selected.
    /// Defaults to the localized <see cref="TwStrings.DateTimePicker_Placeholder"/> text ("Select a datetime").
    /// </summary>
    [Parameter] public string? Placeholder { get; set; }

    private string effectivePlaceholder => Placeholder ?? localizer[TwStrings.DateTimePicker_Placeholder];

    /// <summary>
    /// Overrides automatic device detection for whether the browser's native datetime picker should be used
    /// instead of the custom popover. Leave unset (<see langword="null"/>) to auto-detect based on the
    /// client platform (iOS and Android use the native picker by default).
    /// </summary>
    [Parameter] public bool? PreferNativePicker { get; set; }

    /// <summary>
    /// Gets the time portion of the SelectedDateTime as TimeOnly.
    /// </summary>
    private TimeOnly currentTime => TimeOnly.FromDateTime(SelectedDateTime);

    /// <summary>
    /// Relays a date picked in the underlying <see cref="TwDatePicker"/> (a calendar day click, or a
    /// typed/native value) to this component's own <see cref="SelectedDateTimeChanged"/> consumers.
    /// </summary>
    /// <remarks>
    /// <see cref="TwDatePicker.SelectedDateChanged"/> only updates whatever local field it's bound
    /// to - it does not, by itself, also invoke this component's own <see cref="SelectedDateTimeChanged"/>
    /// callback for its consumers, so that relay has to happen explicitly here.
    /// </remarks>
    private async Task OnDateChangedAsync(DateTime newDate)
    {
        SelectedDateTime = newDate;

        if (SelectedDateTimeChanged.HasDelegate)
            await SelectedDateTimeChanged.InvokeAsync(newDate);
    }

    /// <summary>
    /// Relays a new displayed <see cref="Value"/> from the underlying <see cref="TwDatePicker"/> to
    /// this component's own <see cref="ValueChanged"/> consumers - see <see cref="OnDateChangedAsync"/>'s
    /// remarks for why this can't just be a plain <c>@bind-Value</c>.
    /// </summary>
    private async Task OnValueChangedAsync(string newValue)
    {
        Value = newValue;

        if (ValueChanged.HasDelegate)
            await ValueChanged.InvokeAsync(newValue);
    }

    /// <summary>
    /// Handles time changes from the TwTimePickerBody component.
    /// </summary>
    private async Task OnTimePickerChanged(TimeOnly newTime)
    {
        // Combine the existing date with the new time
        var date = DateOnly.FromDateTime(SelectedDateTime);
        SelectedDateTime = DateTime.SpecifyKind(date.ToDateTime(newTime), SelectedDateTime.Kind);
        Value = SelectedDateTime.ToString(format, localizer.Formats.Culture);

        // Captured before either callback fires: a caller binding both SelectedDateTime and
        // Value can trigger a reentrant render from the first InvokeAsync that pushes its
        // (still stale) Value parameter back onto this component, clobbering the field before
        // it's read below - see the identical fix in TwDatePicker.SelectDateAsync.
        var selectedDateTime = SelectedDateTime;
        var value = Value;

        if (SelectedDateTimeChanged.HasDelegate)
            await SelectedDateTimeChanged.InvokeAsync(selectedDateTime);

        if (ValueChanged.HasDelegate)
            await ValueChanged.InvokeAsync(value);

        StateHasChanged();
    }
}