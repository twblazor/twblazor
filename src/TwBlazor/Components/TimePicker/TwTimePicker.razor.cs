// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using Microsoft.AspNetCore.Components;
using System.Globalization;
using TwBlazor.Configuration.Components;
using TwBlazor.Localization;
using TwBlazor.Utilities;

namespace TwBlazor.Components;

/// <summary>
/// Represents a time picker component that allows users to select and edit time values, supporting
/// both 12-hour and 24-hour formats.
/// </summary>
/// <remarks>Use the TwTimePicker component to provide an interactive UI for selecting time values in
/// Blazor applications. The component supports two-way binding for the selected TimeOnly value and can be 
/// configured to display time in either 12-hour (AM/PM) or 24-hour format.</remarks>
public partial class TwTimePicker : TwPopoverPickerComponentBase
{
    private const string twentyFourHourFormat = "HH:mm";

    private const string twelveHourFormat = "hh:mm tt";

    private TwTimePickerTheme theme => options.Theme.Components.Require<TwTimePickerTheme>();

    /// <summary>
    /// The string format used to display the <see cref="SelectedTime"/>, default value is 'HH:mm'.
    /// </summary>
    private string format { get; set; }

    /// <summary>
    /// Reference to the trigger <see cref="TwTextfield{T}"/> instance, used to focus its actual
    /// &lt;input&gt; element directly - see <see cref="TwPopoverPickerComponentBase.triggerInputRef"/>.
    /// </summary>
    private TwTextfield<string>? trigger;

    /// <inheritdoc />
    protected override ElementReference? triggerInputRef => trigger?.InputRef;

    /// <summary>
    /// Gets or sets a value indicating whether the time should be displayed in 12-hour format.
    /// </summary>
    /// <remarks>Set this property to <see langword="true"/> to use 12-hour time representation (with AM/PM);
    /// otherwise, 24-hour format will be used.</remarks>
    [Parameter] public bool? Is12HourFormat { get; set; }

    /// <summary>
    /// Gets whether a 12 hour clock is used: <see cref="Is12HourFormat"/> when set, otherwise the localization options.
    /// </summary>
    private bool effectiveIs12HourFormat => Is12HourFormat ?? localizer.Formats.Use12HourInput;

    /// <summary>
    /// Gets or sets the step, in minutes, that the picker's step buttons and Arrow Up/Arrow Down move the time
    /// by. Steps under an hour move the minutes (e.g. 15 gives :00, :15, :30, :45); steps of an hour or more
    /// move the hours (e.g. 120 steps two hours at a time) and the minutes show as fixed text. Must divide an hour or, for
    /// whole hours, a day. Defaults to 1. Typed times are not rounded to the step.
    /// </summary>
    [Parameter] public int Step { get; set; } = 1;

    /// <summary>
    /// The selected time value.
    /// </summary>
    [Parameter] public TimeOnly SelectedTime { get; set; } = TimeOnly.FromDateTime(DateTime.Now);

    /// <summary>
    /// The selected bound time value.
    /// </summary>
    [Parameter] public EventCallback<TimeOnly> SelectedTimeChanged { get; set; }

    /// <summary>
    /// The <see cref="string" /> value displayed in the textbox to the user.
    /// </summary>
    [Parameter] public string? Value { get; set; }

    /// <summary>
    /// The <see cref="string" /> bound value displayed in the textbox to the user.
    /// </summary>
    [Parameter] public EventCallback<string> ValueChanged { get; set; }
    /// <summary>
    /// The placeholder text to display when no time is selected.
    /// Defaults to the localized <see cref="TwStrings.TimePicker_Placeholder"/> text ("Select a time").
    /// </summary>
    [Parameter] public string? Placeholder { get; set; }

    private string effectivePlaceholder => Placeholder ?? localizer[TwStrings.TimePicker_Placeholder];

    /// <summary>
    /// Initializes a new instance of the <see cref="TwTimePicker"/> class.
    /// </summary>
    public TwTimePicker()
    {
        format = twentyFourHourFormat;
    }

    /// <summary>
    /// Gets the culture used to format/parse <see cref="Value"/>. The native browser time input
    /// always sends/receives its value in an invariant "HH:mm" format regardless of locale, so that
    /// path must stay culture-invariant; the custom popover instead formats using
    /// <see cref="CultureInfo.CurrentCulture"/> so a value the user sees rendered in their locale
    /// (e.g. a localized AM/PM designator) also parses back correctly when they type it in - which
    /// requires parsing with that same culture rather than always <see cref="CultureInfo.InvariantCulture"/>.
    /// </summary>
    private CultureInfo effectiveCulture => UseNativePicker ? CultureInfo.InvariantCulture : localizer.Formats.Culture;

    /// <summary>
    /// Gets the format the custom popover types and parses: the localized time input format when one is
    /// configured, otherwise the 12 or 24 hour default.
    /// </summary>
    private string popoverFormat
    {
        get
        {
            if (localizer.Formats.TimeInput is { } configured)
            {
                return configured;
            }

            return effectiveIs12HourFormat ? twelveHourFormat : twentyFourHourFormat;
        }
    }

    protected override Task OnParametersSetAsync()
    {
        TimeStepHelpers.ValidateStep(Step);

        // The native time input always needs a 24-hour HH:mm value no matter what effectiveIs12HourFormat
        // says, since the browser renders its own localized 12/24-hour UI on top of that.
        format = UseNativePicker ? twentyFourHourFormat : popoverFormat;
        Value = SelectedTime.ToString(format, effectiveCulture);
        return base.OnParametersSetAsync();
    }

    /// <summary>
    /// Determines, via <see cref="TwPopoverPickerComponentBase.PreferNativePicker"/> or JS-based device
    /// detection, whether the browser's native time picker should be used instead of the custom popover, then re-renders if that changes the
    /// input's format/type.
    /// </summary>
    /// <remarks>
    /// The native <c>&lt;input type="time"&gt;</c> control always renders using the device's own
    /// locale/region setting (e.g. iOS's "24-Hour Time" toggle) and cannot be forced into a specific
    /// 12/24-hour display - so auto-detection only opts into the native picker when the caller hasn't
    /// requested a specific 12-hour display via <see cref="Is12HourFormat"/>, otherwise the custom
    /// popover (which does honor it) is used instead. An explicit <see cref="TwPopoverPickerComponentBase.PreferNativePicker"/>
    /// still always wins, since that's a deliberate opt-in that accepts the native control's device-driven format.
    /// </remarks>
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);

        if (firstRender)
        {
            UseNativePicker = PreferNativePicker ?? (!effectiveIs12HourFormat && await DeviceDetector.PrefersNativePickerAsync(JSRuntime));
            if (UseNativePicker)
            {
                format = twentyFourHourFormat;
                Value = SelectedTime.ToString(format, effectiveCulture);
                StateHasChanged();
            }
        }

        // Arm the Tab focus trap and background inert-ing whenever the panel is open. Re-run on
        // every render rather than gating behind a one-shot "just opened" flag - see the matching
        // remarks on TwDatePicker.OnAfterRenderAsync for why. Deliberately does not move focus into
        // the panel - the trigger is a text-editable combobox (typing a time directly is a
        // first-class input method here, not just a fallback), so focus has to stay on the input for
        // that to work. Users move into the panel explicitly, same as any combobox-with-popup: Tab,
        // a click, or an arrow key.
        if (isFocused && PanelRef.Context != null)
        {
            await RegisterPanelScrollBehaviorAsync(PanelRef);
            await ApplyPanelTrapAsync(PanelRef);
        }
    }

    /// <summary>
    /// Handles time changes from the TwTimePickerBody component.
    /// </summary>
    private async Task OnTimePickerChanged(TimeOnly newTime)
    {
        SelectedTime = newTime;
        if (SelectedTimeChanged.HasDelegate)
            await SelectedTimeChanged.InvokeAsync(SelectedTime);

        Value = SelectedTime.ToString(format, effectiveCulture);
        if (ValueChanged.HasDelegate)
            await ValueChanged.InvokeAsync(Value);

        StateHasChanged();
    }

    /// <summary>
    /// Handles text value changes from the input field.
    /// </summary>
    /// <remarks>
    /// This handler fires on blur (<see cref="TwTextfield{T}"/>'s <c>BindEvent</c> defaults to
    /// "onchange"), meaning the browser has already moved focus away from the trigger - e.g. by the
    /// user tabbing to the next field. Forcing focus back here (as the other close paths correctly
    /// do) would fight that, so this path only clears the captured token and otherwise leaves focus
    /// alone. If the entered text can't be parsed as a time, it's left in place (rather than silently
    /// discarded) and <see cref="TwBlazorInputComponentBase.Invalid"/>/<see
    /// cref="TwBlazorInputComponentBase.ErrorMessage"/> are set so the field renders an accessible
    /// (<c>aria-invalid</c>/<c>role="alert"</c>) error.
    /// </remarks>
    private async Task OnTextValueChanged(string? value)
    {
        if (ReadOnly || Disabled)
            return;

        await ClosePanelAfterTextCommitAsync();
        await UnregisterOutsideClickAsync();

        Value = value;
        if (ValueChanged.HasDelegate)
            await ValueChanged.InvokeAsync(Value);

        if (string.IsNullOrWhiteSpace(value))
        {
            return;
        }

        if (!TimeOnly.TryParse(value, effectiveCulture, out var parsedTime))
        {
            Invalid = true;
            ErrorMessage = localizer[TwStrings.TimePicker_Invalid, new TimeOnly(9, 30).ToString(format, effectiveCulture)];
            return;
        }

        Invalid = false;
        ErrorMessage = string.Empty;
        SelectedTime = parsedTime;

        if (SelectedTimeChanged.HasDelegate)
            await SelectedTimeChanged.InvokeAsync(SelectedTime);
    }

    /// <summary>
    /// Gets the attributes forwarded onto the trigger input: the popover ARIA attributes, or the native
    /// <c>step</c> (in seconds) so the browser's own time picker honours <see cref="Step"/>.
    /// </summary>
    private Dictionary<string, object> inputAttributes
    {
        get
        {
            var attributes = triggerAttributes;
            if (UseNativePicker && Step != 1)
            {
                attributes["step"] = Step * 60;
            }

            return attributes;
        }
    }

    private string classes => new ClassBuilder(theme.PickerRoot).AddClass(Class).Build();

    // Safari (iOS) renders native time inputs with a special control path that can ignore
    // percentage widths; appearance-none drops it into normal box-model layout without
    // affecting the native picker UI that opens on tap.
    private string textfieldClasses => new ClassBuilder(theme.TextfieldPadding)
        .AddClass(theme.NativeInputAppearance, UseNativePicker).Build();

    private string bodyClasses => new ClassBuilder("time")
        .AddClass(theme.BodySurface)
        .AddClass(popoverBuilder.GetSurfaceClasses(Rounded, Shadow))
        .Build();
}
