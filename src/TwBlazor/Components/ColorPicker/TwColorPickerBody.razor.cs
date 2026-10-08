// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using TwBlazor.Configuration.Components;
using TwBlazor.Enums;
using TwBlazor.Utilities;

namespace TwBlazor.Components.ColorPicker;

/// <summary>
/// The internal color picker dialog component that provides the visual color selection interface.
/// Includes a color selector, hue slider, optional alpha slider, and color input fields.
/// </summary>
public partial class TwColorPickerBody : TwBlazorComponentBase
{
    /// <summary>
    /// Gets or sets the JavaScript runtime instance used for interop operations.
    /// </summary>
    [Inject] public IJSRuntime JSRuntime { get; set; } = default!;

    private const string defaultColor = "#000000";
    /// <summary>
    /// Gets or sets the current color value. Supports Hex, RGB, and HSL formats.
    /// Default is defaultColor (black).
    /// </summary>
    [Parameter] public string Value { get; set; } = defaultColor;
    /// <summary>
    /// Gets or sets the visual variant of the input controls within the color picker.
    /// </summary>
    [Parameter] public InputVariant Variant { get; set; }
    /// <summary>
    /// Gets or sets whether to show and allow editing of the alpha (transparency) channel.
    /// When enabled, displays an alpha slider and includes alpha values in output.
    /// </summary>
    [Parameter] public bool ShowAlpha { get; set; }
    /// <summary>
    /// Gets or sets whether to show the mode switch button to toggle between Hex, RGB, and HSL input formats.
    /// Default is true.
    /// </summary>
    [Parameter] public bool ShowModeSwitch { get; set; } = true;
    /// <summary>
    /// Gets or sets the output format for the color value when notifying parent components.
    /// Default is <see cref="ColorMode.Hex"/>.
    /// </summary>
    [Parameter] public ColorMode OutputFormat { get; set; } = ColorMode.Hex;
    /// <summary>
    /// Gets or sets the event callback that is invoked when the color value changes.
    /// </summary>
    [Parameter] public EventCallback<string> ValueChanged { get; set; }
    /// <summary>
    /// Gets or sets the event callback that is invoked when the color picker dialog is closed.
    /// The boolean parameter indicates whether the selection was confirmed (true) or cancelled (false).
    /// </summary>
    [Parameter] public EventCallback<bool> OnClose { get; set; }

    private TwColorPickerTheme theme => options.Theme.Components.Require<TwColorPickerTheme>();

    private TwPopoverTheme popoverTheme => options.Theme.Components.Require<TwOverlayTheme>().Popover;

    private string dialogClasses => new ClassBuilder(popoverBuilder.GetSurfaceClasses(Rounded, Shadow, popoverTheme.ColorSize))
        .AddClass(Class)
        .Build();

    /// <summary>
    /// The same corner radius as <see cref="dialogClasses"/>, reused on the square-cornered elements
    /// inside the dialog (the selector square, the control row) so they visually scale with the
    /// dialog's own rounding instead of a hardcoded radius of their own. The small circular swatch and
    /// slider thumbs stay "rounded-full" regardless - a circle, not a corner-radius choice.
    /// </summary>
    private string roundedClass => roundedBuilder.GetRounded(effectiveRounded);

    private string selectorSquareClasses => new ClassBuilder(theme.SelectorSquare)
        .AddClass(roundedClass)
        .Build();

    private string controlRowClasses => new ClassBuilder(theme.ControlRow)
        .AddClass(roundedClass)
        .Build();

    private string sliderRowClasses => new ClassBuilder(options.Theme.Position.Relative)
        .AddClass(options.Theme.Display.Flex)
        .AddClass(options.Theme.Flexbox.Align.Center)
        .AddClass(options.Theme.Interaction.PointerCursor)
        .AddClass(theme.SliderRow)
        .Build();

    private string sliderContainerClasses => new ClassBuilder(options.Theme.Display.Flex)
        .AddClass(options.Theme.Flexbox.Align.Center)
        .AddClass(options.Theme.Spacing.Gap.Sm)
        .Build();

    private string inputRowClasses => new ClassBuilder(options.Theme.Display.Flex)
        .AddClass(options.Theme.Spacing.Gap.Sm)
        .Build();

    private string alphaReadoutClasses => new ClassBuilder(theme.AlphaLabel)
        .AddClass(options.Theme.Flexbox.ShrinkNone)
        .AddClass(theme.AlphaReadoutAlign)
        .Build();

    // Color state in HSL format (0-360 for hue, 0-1 for saturation and lightness)
    private double hue = 0;
    private double saturation = 1;
    private double lightness = 0.5;
    private double alpha = 1;

    // Color state in RGB format (0-255 for each channel)
    private int r, g, b;

    // Color state for HSL input display (0-360 for h, 0-100 for s and l)
    private int h, s, l;

    // Hex input field value
    private string hexInput = defaultColor;

    // Current display mode for color input fields
    private ColorMode currentMode = ColorMode.Hex;

    // Dragging state flags to prevent feedback loops during user interaction
    private bool isSelectorDragging = false;
    private bool isHueDragging = false;
    private bool isAlphaDragging = false;

    /// <summary>
    /// Reference to the saturation/lightness selector square, used by the parent <see cref="TwColorPicker"/>
    /// (via <c>twDialog.focusSurface</c>) as the first focusable control when the dialog opens.
    /// </summary>
    private ElementReference selectorRef = default;

    /// <summary>
    /// Reference to the hue slider strip, used to attach <c>twSlider.preventScrollKeys</c> so
    /// Arrow/Home/End change the hue without also scrolling the page.
    /// </summary>
    private ElementReference hueRef = default;

    /// <summary>
    /// Reference to the alpha slider strip (only rendered when <see cref="ShowAlpha"/> is true), used
    /// to attach <c>twSlider.preventScrollKeys</c> so Arrow/Home/End change the alpha without also
    /// scrolling the page.
    /// </summary>
    private ElementReference alphaRef = default;

    // Keyboard step sizes for the role="slider" controls below. Small enough for fine adjustment,
    // large enough that a handful of key presses meaningfully moves the value.
    private const double selectorStep = 0.05;
    private const double hueStep = 5;
    private const double alphaStep = 0.05;

    /// <summary>
    /// Fallback element dimensions (w-64 minus padding/borders) used only until the real, rendered
    /// sizes are measured via JS interop in <see cref="OnAfterRenderAsync"/>. Kept as a same-render
    /// starting point so drag math has a sane value before the first measurement round-trip
    /// completes, rather than dividing by zero.
    /// </summary>
    private double selectorWidth = 220;
    private double selectorHeight = 188;
    private double hueSliderWidth = 220;
    private double alphaSliderWidth = 124;

    /// <summary>
    /// Whether the browser supports the EyeDropper API, detected once via JS interop on first render.
    /// Gates the pick-from-screen button shown in the control row - browsers without it (Firefox,
    /// Safari as of this writing) simply never see the button rather than seeing one that errors out.
    /// </summary>
    private bool eyeDropperSupported;

    protected override void OnParametersSet()
    {
        // Don't re-parse color from value if we're currently interacting with the picker
        // This prevents feedback loops where HSL→RGB→HSL conversions cause hue drift
        if (!isSelectorDragging && !isHueDragging && !isAlphaDragging)
        {
            ParseColorFromValue();
        }
    }

    /// <summary>
    /// Attaches a native keydown listener (via <c>twSlider.preventScrollKeys</c>) to each
    /// role="slider" element so Arrow/Home/End change its value without also triggering the
    /// browser's native scroll for those keys. Deliberately not done via a Razor
    /// <c>@onkeydown:preventDefault</c> attribute, since that would unconditionally block every key
    /// - including Tab - on the element, trapping keyboard focus inside the picker.
    /// </summary>
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);

        if (firstRender)
        {
            await MeasureSlidersAsync();

            eyeDropperSupported = await JSRuntime.InvokeAsync<bool>("twColorPicker.supportsEyeDropper");
            if (eyeDropperSupported)
            {
                StateHasChanged();
            }
        }

        await JSRuntime.InvokeVoidAsync("twSlider.preventScrollKeys", selectorRef);
        await JSRuntime.InvokeVoidAsync("twSlider.preventScrollKeys", hueRef);
        if (ShowAlpha)
        {
            await JSRuntime.InvokeVoidAsync("twSlider.preventScrollKeys", alphaRef);
        }
    }

    /// <summary>
    /// Replaces the hardcoded fallback slider dimensions with their actual rendered size, so drag
    /// math (<see cref="UpdateSelectorPosition"/>, <see cref="UpdateHuePosition"/>,
    /// <see cref="UpdateAlphaPosition"/>) stays correct even if the picker dialog ends up a different
    /// size than the default (a resized/zoomed viewport, or a consumer overriding the dialog width).
    /// </summary>
    private async Task MeasureSlidersAsync()
    {
        var selectorSize = await JSRuntime.InvokeAsync<double[]>("twColorPicker.getSize", selectorRef);
        if (selectorSize is [> 0, > 0])
        {
            selectorWidth = selectorSize[0];
            selectorHeight = selectorSize[1];
        }

        var hueSize = await JSRuntime.InvokeAsync<double[]>("twColorPicker.getSize", hueRef);
        if (hueSize is [> 0, ..])
        {
            hueSliderWidth = hueSize[0];
        }

        if (ShowAlpha)
        {
            var alphaSize = await JSRuntime.InvokeAsync<double[]>("twColorPicker.getSize", alphaRef);
            if (alphaSize is [> 0, ..])
            {
                alphaSliderWidth = alphaSize[0];
            }
        }
    }

    private void ParseColorFromValue()
    {
        if (string.IsNullOrEmpty(Value))
        {
            Value = defaultColor;
        }

        string hexValue;

        // Convert any format to hex first using ColorConverter utility
        if (Value.StartsWith("rgb", StringComparison.OrdinalIgnoreCase))
        {
            // Parse RGB/RGBA format: rgb(255, 0, 0) or rgba(255, 0, 0, 1)
            hexValue = ColorConverter.RgbToHex(Value, includeAlpha: true, fallbackValue: defaultColor);
        }
        else if (Value.StartsWith("hsl", StringComparison.OrdinalIgnoreCase))
        {
            // Parse HSL/HSLA format: hsl(0, 100%, 50%) or hsla(0, 100%, 50%, 1)
            hexValue = ColorConverter.HslToHex(Value, includeAlpha: true, fallbackValue: defaultColor);
        }
        else if (Value.StartsWith('#'))
        {
            // Already in hex format
            hexValue = Value;
        }
        else
        {
            // Default to black if format is not recognized
            hexValue = defaultColor;
        }

        // Parse the hex value
        var hex = hexValue.TrimStart('#');

        if (hex.Length >= 6)
        {
            r = Convert.ToInt32(hex[..2], 16);
            g = Convert.ToInt32(hex[2..4], 16);
            b = Convert.ToInt32(hex[4..6], 16);

            if (hex.Length == 8)
            {
                alpha = Convert.ToInt32(hex[6..8], 16) / 255.0;
            }
            else
            {
                alpha = 1.0;
            }

            (hue, saturation, lightness) = ColorConverter.RgbToHsl(r, g, b);
            hexInput = hexValue[..Math.Min(7, hexValue.Length)];
            UpdateInputsFromCurrentColor();
        }
    }

    private void UpdateColorFromHsl()
    {
        (r, g, b) = ColorConverter.HslToRgb(hue, saturation, lightness);
        UpdateAndNotify();
    }

    private void UpdateAndNotify()
    {
        var output = FormatColorOutput();
        hexInput = $"#{r:X2}{g:X2}{b:X2}";
        ValueChanged.InvokeAsync(output);
    }

    private string FormatColorOutput()
    {
        return OutputFormat switch
        {
            ColorMode.Hex => FormatHex(),
            ColorMode.Rgb => FormatRgb(),
            ColorMode.Hsl => FormatHsl(),
            _ => FormatHex()
        };
    }

    private string FormatHex()
    {
        var hexColor = $"#{r:X2}{g:X2}{b:X2}";
        if (ShowAlpha)
        {
            hexColor += $"{(int)(alpha * 255):X2}";
        }
        return hexColor;
    }

    private string FormatRgb()
    {
        var hexValue = $"#{r:X2}{g:X2}{b:X2}";
        if (ShowAlpha)
        {
            hexValue += $"{(int)(alpha * 255):X2}";
        }
        return ColorConverter.HexToRgb(hexValue, includeAlpha: ShowAlpha);
    }

    private string FormatHsl()
    {
        var hexValue = $"#{r:X2}{g:X2}{b:X2}";
        if (ShowAlpha)
        {
            hexValue += $"{(int)(alpha * 255):X2}";
        }
        return ColorConverter.HexToHsl(hexValue, includeAlpha: ShowAlpha);
    }

    private string GetCurrentColor()
    {
        if (ShowAlpha)
        {
            return $"#{r:X2}{g:X2}{b:X2}{(int)(alpha * 255):X2}";
        }
        return $"#{r:X2}{g:X2}{b:X2}";
    }

    private string GetCurrentColorWithoutAlpha()
    {
        return $"#{r:X2}{g:X2}{b:X2}";
    }

    /// <summary>
    /// Converts a touch's viewport-relative <c>clientX</c>/<c>clientY</c> (all Blazor's
    /// <see cref="TouchPoint"/> exposes) into a position relative to <paramref name="el"/>, mirroring
    /// what <see cref="MouseEventArgs.OffsetX"/>/<see cref="MouseEventArgs.OffsetY"/> already give the
    /// mouse handlers for free. Requires a small JS round-trip since Blazor doesn't compute this itself.
    /// </summary>
    private async Task<(double X, double Y)> GetRelativeTouchPositionAsync(ElementReference el, TouchEventArgs e)
    {
        if (e.Touches.Length == 0)
        {
            return (0, 0);
        }

        var touch = e.Touches[0];
        var position = await JSRuntime.InvokeAsync<double[]>("twColorPicker.relativePosition", el, touch.ClientX, touch.ClientY);
        return (position[0], position[1]);
    }

    private void OnSelectorMouseDown(MouseEventArgs e)
    {
        isSelectorDragging = true;
        UpdateSelectorPosition(e.OffsetX, e.OffsetY);
    }

    private void OnSelectorMouseMove(MouseEventArgs e)
    {
        if (isSelectorDragging)
        {
            UpdateSelectorPosition(e.OffsetX, e.OffsetY);
        }
    }

    private void OnSelectorMouseUp()
    {
        isSelectorDragging = false;
    }

    private async Task OnSelectorTouchStart(TouchEventArgs e)
    {
        isSelectorDragging = true;
        (var x, var y) = await GetRelativeTouchPositionAsync(selectorRef, e);
        UpdateSelectorPosition(x, y);
    }

    private async Task OnSelectorTouchMove(TouchEventArgs e)
    {
        if (!isSelectorDragging)
        {
            return;
        }

        (var x, var y) = await GetRelativeTouchPositionAsync(selectorRef, e);
        UpdateSelectorPosition(x, y);
    }

    private void OnSelectorTouchEnd() => isSelectorDragging = false;

    private void UpdateSelectorPosition(double offsetX, double offsetY)
    {
        saturation = Math.Clamp(offsetX / selectorWidth, 0, 1);
        lightness = Math.Clamp(1 - (offsetY / selectorHeight), 0, 1);
        UpdateColorFromHsl();
    }

    /// <summary>
    /// Keyboard support for the saturation/lightness role="slider" square, required by the WAI-ARIA APG
    /// slider pattern: ArrowRight/ArrowLeft adjust saturation, ArrowUp/ArrowDown adjust lightness, and
    /// Home/End jump saturation to its minimum/maximum.
    /// </summary>
    private void OnSelectorKeyDown(KeyboardEventArgs e)
    {
        switch (e.Key)
        {
            case "ArrowRight":
                saturation = Math.Clamp(saturation + selectorStep, 0, 1);
                break;
            case "ArrowLeft":
                saturation = Math.Clamp(saturation - selectorStep, 0, 1);
                break;
            case "ArrowUp":
                lightness = Math.Clamp(lightness + selectorStep, 0, 1);
                break;
            case "ArrowDown":
                lightness = Math.Clamp(lightness - selectorStep, 0, 1);
                break;
            case "Home":
                saturation = 0;
                break;
            case "End":
                saturation = 1;
                break;
            default:
                return;
        }

        UpdateColorFromHsl();
    }

    private void OnHueMouseDown(MouseEventArgs e)
    {
        isHueDragging = true;
        UpdateHuePosition(e.OffsetX);
    }

    private void OnHueMouseMove(MouseEventArgs e)
    {
        if (isHueDragging)
        {
            UpdateHuePosition(e.OffsetX);
        }
    }

    private void OnHueMouseUp() => isHueDragging = false;

    private async Task OnHueTouchStart(TouchEventArgs e)
    {
        isHueDragging = true;
        (var x, var _) = await GetRelativeTouchPositionAsync(hueRef, e);
        UpdateHuePosition(x);
    }

    private async Task OnHueTouchMove(TouchEventArgs e)
    {
        if (!isHueDragging)
        {
            return;
        }

        (var x, var _) = await GetRelativeTouchPositionAsync(hueRef, e);
        UpdateHuePosition(x);
    }

    private void OnHueTouchEnd() => isHueDragging = false;

    private void UpdateHuePosition(double offsetX)
    {
        hue = Math.Clamp((offsetX / hueSliderWidth) * 360, 0, 360);
        UpdateColorFromHsl();
    }

    /// <summary>
    /// Keyboard support for the hue role="slider", required by the WAI-ARIA APG slider pattern:
    /// ArrowRight/ArrowUp increase, ArrowLeft/ArrowDown decrease, Home/End jump to min/max.
    /// </summary>
    private void OnHueKeyDown(KeyboardEventArgs e)
    {
        switch (e.Key)
        {
            case "ArrowRight":
            case "ArrowUp":
                hue = Math.Clamp(hue + hueStep, 0, 360);
                break;
            case "ArrowLeft":
            case "ArrowDown":
                hue = Math.Clamp(hue - hueStep, 0, 360);
                break;
            case "Home":
                hue = 0;
                break;
            case "End":
                hue = 360;
                break;
            default:
                return;
        }

        UpdateColorFromHsl();
    }

    private void OnAlphaMouseDown(MouseEventArgs e)
    {
        isAlphaDragging = true;
        UpdateAlphaPosition(e.OffsetX);
    }

    private void OnAlphaMouseMove(MouseEventArgs e)
    {
        if (isAlphaDragging)
        {
            UpdateAlphaPosition(e.OffsetX);
        }
    }

    private void OnAlphaMouseUp() => isAlphaDragging = false;

    private async Task OnAlphaTouchStart(TouchEventArgs e)
    {
        isAlphaDragging = true;
        (var x, var _) = await GetRelativeTouchPositionAsync(alphaRef, e);
        UpdateAlphaPosition(x);
    }

    private async Task OnAlphaTouchMove(TouchEventArgs e)
    {
        if (!isAlphaDragging)
        {
            return;
        }

        (var x, var _) = await GetRelativeTouchPositionAsync(alphaRef, e);
        UpdateAlphaPosition(x);
    }

    private void OnAlphaTouchEnd() => isAlphaDragging = false;

    private void UpdateAlphaPosition(double offsetX)
    {
        alpha = Math.Clamp(offsetX / alphaSliderWidth, 0, 1);
        UpdateAndNotify();
    }

    /// <summary>
    /// Keyboard support for the alpha role="slider", required by the WAI-ARIA APG slider pattern:
    /// ArrowRight/ArrowUp increase, ArrowLeft/ArrowDown decrease, Home/End jump to min/max.
    /// </summary>
    private void OnAlphaKeyDown(KeyboardEventArgs e)
    {
        switch (e.Key)
        {
            case "ArrowRight":
            case "ArrowUp":
                alpha = Math.Clamp(alpha + alphaStep, 0, 1);
                break;
            case "ArrowLeft":
            case "ArrowDown":
                alpha = Math.Clamp(alpha - alphaStep, 0, 1);
                break;
            case "Home":
                alpha = 0;
                break;
            case "End":
                alpha = 1;
                break;
            default:
                return;
        }

        UpdateAndNotify();
    }

    private void OnRInputChanged(string red)
    {
        var valid = int.TryParse(red, out r);

        if (!valid) return;

        r = Math.Clamp(r, 0, 255);
        (hue, saturation, lightness) = ColorConverter.RgbToHsl(r, g, b);
        UpdateAndNotify();
    }

    private void OnGInputChanged(string green)
    {
        var valid = int.TryParse(green, out g);

        if (!valid) return;

        g = Math.Clamp(g, 0, 255);
        (hue, saturation, lightness) = ColorConverter.RgbToHsl(r, g, b);
        UpdateAndNotify();
    }

    private void OnBInputChanged(string blue)
    {
        var valid = int.TryParse(blue, out b);

        if (!valid) return;

        b = Math.Clamp(b, 0, 255);
        (hue, saturation, lightness) = ColorConverter.RgbToHsl(r, g, b);
        UpdateAndNotify();
    }

    private void OnHexInputChanged(ChangeEventArgs e) => hexInput = e.Value?.ToString() ?? defaultColor;

    private void OnHexInputCommitted()
    {
        var hex = hexInput?.Trim() ?? defaultColor;
        if (!hex.StartsWith('#')) hex = "#" + hex;

        if (hex.Length >= 7)
        {
            try
            {
                var hexPart = hex.TrimStart('#');
                r = Convert.ToInt32(hexPart[..2], 16);
                g = Convert.ToInt32(hexPart[2..4], 16);
                b = Convert.ToInt32(hexPart[4..6], 16);
                (hue, saturation, lightness) = ColorConverter.RgbToHsl(r, g, b);
                UpdateAndNotify();
            }
            catch (FormatException)
            {
                hexInput = $"#{r:X2}{g:X2}{b:X2}";
            }
            catch (ArgumentException)
            {
                // Covers ArgumentOutOfRangeException as well as the plain ArgumentException
                // Convert.ToInt32 throws for a minus-sign segment (e.g. a hex value starting
                // with '-'), which otherwise propagated as an unhandled exception.
                hexInput = $"#{r:X2}{g:X2}{b:X2}";
            }
        }
    }

    /// <summary>
    /// Opens the browser's native EyeDropper tool (only rendered when <see cref="eyeDropperSupported"/>
    /// is true) and applies the picked color. The API always returns an opaque 6-digit hex string, so
    /// only RGB/HSL are updated - any existing alpha is left untouched, mirroring <see cref="OnRInputChanged"/>
    /// and friends which likewise only recompute HSL from RGB and notify.
    /// </summary>
    private async Task PickColorFromScreenAsync()
    {
        var picked = await JSRuntime.InvokeAsync<string?>("twColorPicker.openEyeDropper");
        if (string.IsNullOrWhiteSpace(picked)) return;

        var hex = picked.TrimStart('#');
        if (hex.Length < 6) return;

        r = Convert.ToInt32(hex[..2], 16);
        g = Convert.ToInt32(hex[2..4], 16);
        b = Convert.ToInt32(hex[4..6], 16);
        (hue, saturation, lightness) = ColorConverter.RgbToHsl(r, g, b);
        UpdateInputsFromCurrentColor();
        UpdateAndNotify();
    }

    private void OnCancel() => OnClose.InvokeAsync(false);

    private void OnConfirm() => OnClose.InvokeAsync(true);

    private void HandleModeSwitch()
    {
        currentMode = currentMode switch
        {
            ColorMode.Hex => ColorMode.Rgb,
            ColorMode.Rgb => ColorMode.Hsl,
            ColorMode.Hsl => ColorMode.Hex,
            _ => ColorMode.Hex
        };
        UpdateInputsFromCurrentColor();
    }

    private void UpdateInputsFromCurrentColor()
    {
        h = (int)Math.Round(hue);
        s = (int)Math.Round(saturation * 100);
        l = (int)Math.Round(lightness * 100);
    }

    private void OnHInputChanged(string hueValue)
    {
        if (int.TryParse(hueValue, out h))
        {
            h = Math.Clamp(h, 0, 360);
            hue = h;
            UpdateColorFromHsl();
        }
    }

    private void OnSInputChanged(string satValue)
    {
        if (int.TryParse(satValue, out s))
        {
            s = Math.Clamp(s, 0, 100);
            saturation = s / 100.0;
            UpdateColorFromHsl();
        }
    }

    private void OnLInputChanged(string lightValue)
    {
        if (int.TryParse(lightValue, out l))
        {
            l = Math.Clamp(l, 0, 100);
            lightness = l / 100.0;
            UpdateColorFromHsl();
        }
    }
}
