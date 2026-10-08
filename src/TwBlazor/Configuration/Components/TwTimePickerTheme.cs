// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;

namespace TwBlazor.Configuration.Components;

/// <summary>
/// Theme configuration for the time picker components (<see cref="TwBlazor.Components.TwTimePicker"/>,
/// <see cref="TwBlazor.Components.TimePicker.TwTimePickerBody"/>).
/// Override any property to customize time picker styles globally.
/// </summary>
[ExcludeFromCodeCoverage]
public class TwTimePickerTheme
{
    /// <summary>
    /// Gets or sets the base classes for the picker's outer wrapper, positioning the trigger icon,
    /// text field, and popover panel.
    /// </summary>
    public required string PickerRoot { get; set; }

    /// <summary>
    /// Gets or sets the classes for the clock icon's clickable wrapper within the trigger text field.
    /// </summary>
    public required string IconWrapper { get; set; }

    /// <summary>
    /// Gets or sets the classes for the clock glyph itself.
    /// </summary>
    public required string IconGlyph { get; set; }

    /// <summary>
    /// Gets or sets the padding classes for the trigger text field, leaving room for the clock icon.
    /// </summary>
    public required string TextfieldPadding { get; set; }

    /// <summary>
    /// Gets or sets the classes suppressing native browser chrome on the trigger field when the
    /// device's native time picker is active.
    /// </summary>
    public required string NativeInputAppearance { get; set; }

    /// <summary>
    /// Gets or sets the positioning classes for the popover panel's wrapper, anchoring it beneath the
    /// trigger. Background/border/rounded/shadow/sizing are no longer baked in here - they come from
    /// the shared <see cref="TwOverlayTheme"/> via <see cref="Builders.PopoverBuilder"/>, applied to
    /// the panel's own surface element (<see cref="BodySurface"/> for the single picker,
    /// <see cref="TwOverlayTheme.TimeRangePopoverSize"/> for <see cref="TwBlazor.Components.TwTimeRangePicker"/>)
    /// instead of this wrapper.
    /// </summary>
    public required string PanelWrapper { get; set; }

    /// <summary>
    /// Gets or sets the classes for the Start/End step tab row shown in <see cref="TwBlazor.Components.TwTimeRangePicker"/>'s popover.
    /// </summary>
    public required string RangeStageTabsContainer { get; set; }

    /// <summary>
    /// Gets or sets the text color for an inactive Start/End step tab.
    /// </summary>
    public required string RangeStageTabInactive { get; set; }

    /// <summary>
    /// Gets or sets the classes for the active Start/End step tab.
    /// </summary>
    public required string StageTabActive { get; set; }

    /// <summary>
    /// Gets or sets the hover classes for an inactive Start/End step tab.
    /// </summary>
    public required string StageTabHover { get; set; }

    /// <summary>
    /// Gets or sets the base classes for a Start/End step tab button.
    /// </summary>
    public required string StageTabBase { get; set; }

    /// <summary>
    /// Gets or sets the padding/typography classes for the popover panel's surface, used by the single
    /// <see cref="TwBlazor.Components.TwTimePicker"/> - background/border/rounded/shadow come from the
    /// shared <see cref="TwOverlayTheme"/>.
    /// </summary>
    public required string BodySurface { get; set; }

    /// <summary>
    /// Gets or sets the base classes for the time picker body's own outer container.
    /// </summary>
    public required string BodyRoot { get; set; }

    /// <summary>
    /// Gets or sets the classes for the time picker body's inner content wrapper.
    /// </summary>
    public required string BodyInner { get; set; }

    /// <summary>
    /// Gets or sets the classes for the row laying out the hour, minute, and AM/PM controls.
    /// </summary>
    public required string ContentRow { get; set; }

    /// <summary>
    /// Gets or sets the classes for each hour/minute column (stepper buttons and number input).
    /// </summary>
    public required string Column { get; set; }

    /// <summary>
    /// Gets or sets the classes for the increment/decrement stepper buttons.
    /// </summary>
    public required string StepButton { get; set; }

    /// <summary>
    /// Gets or sets the classes for the wrapper around each hour/minute number input.
    /// </summary>
    public required string NumberWrapper { get; set; }

    /// <summary>
    /// Gets or sets the structural/typography classes for the hour/minute number inputs (sizing,
    /// alignment, font weight, text color, transitions). Excludes border, background, and rounding -
    /// those come from <see cref="Builders.InputVariantBuilder"/> so the inputs follow the same
    /// Default/Outlined/Filled variant as every other text input - and excludes hover/focus border and
    /// ring colors, which are resolved dynamically from the shared theme color tokens.
    /// </summary>
    public required string NumberInput { get; set; }

    /// <summary>
    /// Gets or sets the classes for the ":" separator between the hour and minute columns.
    /// </summary>
    public required string Separator { get; set; }

    /// <summary>
    /// Gets or sets the classes for the AM/PM toggle button's wrapper.
    /// </summary>
    public required string AmPmWrapper { get; set; }

    /// <summary>
    /// Gets or sets the typography classes applied to the AM/PM toggle <see cref="TwBlazor.Components.TwButton"/>.
    /// </summary>
    public required string AmPmButtonClass { get; set; }
}
