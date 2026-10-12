// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;

namespace TwBlazor.Configuration.Components;

/// <summary>
/// Theme configuration for the color picker components (<see cref="TwBlazor.Components.TwColorPicker"/>,
/// <see cref="TwBlazor.Components.ColorPicker.TwColorPickerBody"/>).
/// Override any property to customize color picker styles globally.
/// </summary>
[ExcludeFromCodeCoverage]
public class TwColorPickerTheme
{
    /// <summary>
    /// Gets or sets the base classes for the color swatch button/input shown next to the text field.
    /// </summary>
    public required string Swatch { get; set; }

    /// <summary>
    /// Gets or sets the classes applied to the swatch when the color picker is disabled.
    /// </summary>
    public required string SwatchDisabled { get; set; }

    /// <summary>
    /// Gets or sets the hover classes applied to the swatch when the color picker is enabled.
    /// </summary>
    public required string SwatchHover { get; set; }

    /// <summary>
    /// Gets or sets the classes for the container that lays out the swatch next to the text field.
    /// </summary>
    public required string InputContainer { get; set; }

    /// <summary>
    /// Gets or sets the positioning classes for the dialog's wrapper, anchoring it beneath the swatch.
    /// </summary>
    public required string DialogPosition { get; set; }

    /// <summary>
    /// Gets or sets the classes for the large color preview swatch shown at the top of the dialog.
    /// </summary>
    public required string PreviewSwatch { get; set; }

    /// <summary>
    /// Gets or sets the classes for the saturation/lightness selector square.
    /// </summary>
    public required string SelectorSquare { get; set; }

    /// <summary>
    /// Gets or sets the classes for the draggable thumb on the saturation/lightness selector square.
    /// </summary>
    public required string SelectorThumb { get; set; }

    /// <summary>
    /// Gets or sets the classes for the hue and alpha slider tracks.
    /// </summary>
    public required string SliderTrack { get; set; }

    /// <summary>
    /// Gets or sets the classes for the draggable thumb on the hue and alpha sliders.
    /// </summary>
    public required string SliderThumb { get; set; }

    /// <summary>
    /// Gets or sets the classes for the "Alpha" label and percentage readout beside the alpha slider.
    /// </summary>
    public required string AlphaLabel { get; set; }

    /// <summary>
    /// Gets or sets the classes for the action bar containing the Cancel/Confirm buttons.
    /// </summary>
    public required string ActionBar { get; set; }

    /// <summary>
    /// Gets or sets the classes for the control row holding the pick-from-screen button, the mode
    /// switch, the live swatch, the current mode's value field, and the alpha percentage readout.
    /// </summary>
    public required string ControlRow { get; set; }

    /// <summary>
    /// Gets or sets the classes for the dialog's outermost vertical layout wrapper.
    /// </summary>
    public required string Body { get; set; }

    /// <summary>
    /// Gets or sets the size, shape and keyboard focus ring classes for a draggable hue/alpha slider's
    /// clickable row (excluding positioning/layout, which are composed alongside it).
    /// </summary>
    public required string SliderRow { get; set; }

    /// <summary>
    /// Gets or sets the extra touch-handling class for the hue slider specifically - unlike the alpha
    /// slider, it also responds to native touch drag gestures (see <c>OnHueTouchMove</c>), so the
    /// browser's own touch scrolling/panning needs to be suppressed on it.
    /// </summary>
    public required string HueSliderTouch { get; set; }

    /// <summary>
    /// Gets or sets the classes for each equal-width column in the RGB/HSL numeric input row.
    /// </summary>
    public required string InputColumn { get; set; }

    /// <summary>
    /// Gets or sets the tracking classes for the mode-switch button's text.
    /// </summary>
    public required string ModeSwitchButton { get; set; }

    /// <summary>
    /// Gets or sets the text-alignment class for the alpha percentage readout beside the alpha slider.
    /// </summary>
    public required string AlphaReadoutAlign { get; set; }
}
