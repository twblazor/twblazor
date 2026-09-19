// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using TwBlazor.Configuration.Components;
using TwBlazor.Enums;
using TwBlazor.Utilities;

namespace TwBlazor.Components;

/// <summary>
/// A color picker component that supports multiple color formats (Hex, RGB, HSL) with optional alpha channel.
/// Provides both a visual color picker dialog and text input for manual color entry.
/// </summary>
/// <remarks>
/// Built on <see cref="TwPopoverPickerComponentBase"/>, the same popover open/close, focus-trap and
/// outside-click plumbing shared with <see cref="TwDatePicker"/>/<see cref="TwTimePicker"/>. Unlike those
/// pickers, the popover here opens from a click on the swatch button rather than focusing the trigger
/// textfield - <see cref="ShowDialogAsync"/> drives the shared <c>isFocused</c>/<c>PendingOpenFocus</c>
/// state directly instead of going through <see cref="TwPopoverPickerComponentBase.OnFocusAsync"/>.
/// </remarks>
public partial class TwColorPicker : TwPopoverPickerComponentBase
{
    private const string defaultColor = "#000000";

    /// <summary>
    /// Gets or sets the current color value. Supports Hex, RGB, and HSL formats.
    /// Default is "#000000" (black).
    /// </summary>
    [Parameter] public string Value { get; set; } = defaultColor;
    /// <summary>
    /// Gets or sets the event callback that is invoked when the color value changes.
    /// </summary>
    [Parameter] public EventCallback<string> ValueChanged { get; set; } = default!;
    /// <summary>
    /// Gets or sets the event callback that is invoked when the input receives focus.
    /// </summary>
    [Parameter] public EventCallback<FocusEventArgs> OnFocus { get; set; } = default!;
    /// <summary>
    /// Gets or sets the event name for binding. Default is "onchange".
    /// </summary>
    [Parameter] public string BindEvent { get; set; } = "onchange";
    /// <summary>
    /// Gets or sets whether to show and allow editing of the alpha (transparency) channel.
    /// Default is false.
    /// </summary>
    [Parameter] public bool ShowAlpha { get; set; } = false;
    /// <summary>
    /// Gets or sets whether to show the mode switch button to toggle between color formats.
    /// Default is false.
    /// </summary>
    [Parameter] public bool ShowModeSwitch { get; set; } = false;
    /// <summary>
    /// Gets or sets the output format for the color value (Hex, RGB, or HSL).
    /// Default is <see cref="ColorMode.Hex"/>.
    /// </summary>
    [Parameter] public ColorMode OutputFormat { get; set; } = ColorMode.Hex;

    private string displayValue = string.Empty;

    private TwColorPickerTheme colorPickerTheme => options.Theme.Components.Require<TwColorPickerTheme>();

    private string inputContainerClasses => new ClassBuilder(colorPickerTheme.InputContainer)
        .Build();

    private string previewClasses => new ClassBuilder(colorPickerTheme.Swatch)
        .AddClass(roundedBuilder.GetRounded(effectiveRounded))
        .AddClass(Disabled ? colorPickerTheme.SwatchDisabled : colorPickerTheme.SwatchHover)
        .Build();

    protected override void OnInitialized()
    {
        base.OnInitialized();
        displayValue = Value;
    }

    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        if (string.IsNullOrEmpty(RootId))
        {
            RootId = Guid.NewGuid().ToString("N");
        }

        displayValue = Value;
    }

    /// <summary>
    /// Determines, via <see cref="TwPopoverPickerComponentBase.PreferNativePicker"/> or JS-based device
    /// detection, whether the browser's native color input should be used for the swatch preview instead
    /// of the custom popover dialog.
    /// </summary>
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);

        if (firstRender)
        {
            UseNativePicker = PreferNativePicker ?? await DeviceDetector.PrefersNativePickerAsync(JSRuntime);
            if (UseNativePicker)
            {
                StateHasChanged();
            }
        }

        if (isFocused && PanelRef.Context != null)
        {
            // (Re-)arm the Tab focus trap, background inert-ing, and panel positioning on every
            // render the dialog is open for, rather than gating behind a one-shot "just opened" flag
            // - see the matching remarks on TwDatePicker.OnAfterRenderAsync for why.
            await RegisterPanelScrollBehaviorAsync(PanelRef);
            await JSRuntime.InvokeVoidAsync("twDialog.trapFocus", PanelRef);
            await JSRuntime.InvokeVoidAsync("twDialog.setBackgroundInert", InputRoot?.RootRef);

            // Unlike the trap/inert/positioning above, moving focus into the dialog must stay a
            // one-shot action gated on PendingOpenFocus - repeating it on every render would yank
            // focus back to the first focusable element whenever anything else re-renders this
            // component while the dialog is open (e.g. while the user is dragging a slider).
            if (PendingOpenFocus)
            {
                PendingOpenFocus = false;
                await JSRuntime.InvokeVoidAsync("twDialog.focusSurface", PanelRef);
            }
        }
    }

    /// <summary>
    /// Gets the accessible name for the custom color swatch, reflecting the currently selected color
    /// instead of a static, never-updating label.
    /// </summary>
    private string swatchAriaLabel => $"Selected color: {Value}";

    private string GetPlaceholder()
    {
        return OutputFormat switch
        {
            ColorMode.Rgb => ShowAlpha ? "rgba(r, g, b, a)" : "rgb(r, g, b)",
            ColorMode.Hsl => ShowAlpha ? "hsla(h, s%, l%, a)" : "hsl(h, s%, l%)",
            _ => ShowAlpha ? "#RRGGBBAA" : "#RRGGBB"
        };
    }

    private async Task HandleTextInputChangeAsync()
    {
        if (!string.IsNullOrWhiteSpace(displayValue))
        {
            Value = NormalizeColorValue(displayValue);

            if (ValueChanged.HasDelegate)
            {
                await ValueChanged.InvokeAsync(Value);
            }
        }
    }

    private string NormalizeColorValue(string colorValue)
    {
        if (string.IsNullOrWhiteSpace(colorValue)) return defaultColor;

        colorValue = colorValue.Trim();

        // If it's already RGB or HSL format, return as-is
        if (colorValue.StartsWith("rgb", StringComparison.OrdinalIgnoreCase) ||
            colorValue.StartsWith("hsl", StringComparison.OrdinalIgnoreCase))
        {
            return colorValue;
        }

        // Otherwise treat as hex and normalize
        return NormalizeHexValue(colorValue);
    }

    private string NormalizeHexValue(string hexValue)
    {
        if (string.IsNullOrWhiteSpace(hexValue)) return defaultColor;

        hexValue = hexValue.Trim();
        if (!hexValue.StartsWith('#')) hexValue = "#" + hexValue;

        var hex = hexValue.TrimStart('#');

        if (!ShowAlpha && hex.Length == 8)
        {
            hex = hex[..6];
        }

        return "#" + hex;
    }

    private string GetPreviewColor()
    {
        if (string.IsNullOrWhiteSpace(Value)) return defaultColor;

        // If it's RGB or HSL, convert to hex for preview
        if (Value.StartsWith("rgb", StringComparison.OrdinalIgnoreCase))
        {
            return ColorConverter.RgbToHex(Value, includeAlpha: ShowAlpha, fallbackValue: defaultColor);
        }
        else if (Value.StartsWith("hsl", StringComparison.OrdinalIgnoreCase))
        {
            return ColorConverter.HslToHex(Value, includeAlpha: ShowAlpha, fallbackValue: defaultColor);
        }

        return Value;
    }

    /// <summary>
    /// Opens the color picker dialog when the swatch button is activated. Unlike the textfield-triggered
    /// popovers (<see cref="TwDatePicker"/>/<see cref="TwTimePicker"/>), this drives the base class's
    /// shared open state directly rather than through <see cref="TwPopoverPickerComponentBase.OnFocusAsync"/>,
    /// since the swatch is a plain button rather than the combobox trigger those pickers use.
    /// </summary>
    private async Task ShowDialogAsync()
    {
        if (!Disabled && !ReadOnly)
        {
            // Capture whatever currently has focus (almost always the swatch, since clicking or
            // activating it is what triggers this) so it can be restored once the dialog closes.
            FocusReturnToken = await JSRuntime.InvokeAsync<string?>("twDialog.captureFocus");
            isFocused = true;
            PendingOpenFocus = true;
            await RegisterOutsideClickAsync();
        }
    }

    /// <summary>
    /// Handles color changes from the native &lt;input type="color"&gt; swatch, used instead of the custom
    /// dialog when <see cref="TwPopoverPickerComponentBase.UseNativePicker"/> is <see langword="true"/>.
    /// </summary>
    private async Task OnNativeColorChangedAsync(ChangeEventArgs e)
    {
        var newValue = e.Value?.ToString();
        if (string.IsNullOrWhiteSpace(newValue)) return;

        Value = FormatNativeColorOutput(newValue);
        displayValue = Value;
        if (ValueChanged.HasDelegate)
        {
            await ValueChanged.InvokeAsync(Value);
        }
    }

    /// <summary>
    /// Converts the plain 6-digit hex value the native &lt;input type="color"&gt; always emits into
    /// <see cref="OutputFormat"/>, mirroring what <see cref="ColorPicker.TwColorPickerBody"/>'s
    /// FormatColorOutput does for the custom dialog. Without this, <see cref="OutputFormat"/> was
    /// silently ignored on platforms that prefer the native picker (iOS/Android): the bound
    /// <see cref="Value"/> would always come out as hex even when a consumer asked for RGB or HSL,
    /// which broke anything downstream expecting that format (e.g. re-parsing it).
    /// </summary>
    /// <remarks>
    /// The native picker itself never carries an alpha channel, so there's nothing to preserve there -
    /// this only ever needs to convert the RGB/HSL channels themselves.
    /// </remarks>
    private string FormatNativeColorOutput(string hex) => OutputFormat switch
    {
        ColorMode.Rgb => ColorConverter.HexToRgb(hex),
        ColorMode.Hsl => ColorConverter.HexToHsl(hex),
        _ => hex
    };

    private async Task OnDialogValueChanged(string newValue)
    {
        Value = newValue;
        displayValue = newValue;
        if (ValueChanged.HasDelegate)
        {
            await ValueChanged.InvokeAsync(Value);
        }
        StateHasChanged();
    }

    private async Task OnDialogClose(bool confirmed)
    {
        await ReleasePanelTrapAsync();
        isFocused = false;
        await UnregisterOutsideClickAsync();
        await RestoreFocusAsync();
        StateHasChanged();
    }
}
