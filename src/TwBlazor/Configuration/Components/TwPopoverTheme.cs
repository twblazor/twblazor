// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using TwBlazor.Enums;

namespace TwBlazor.Configuration.Components;

/// <summary>
/// Shared "box" styling (background, border, rounded corners, shadow) for every popover panel
/// (<see cref="TwBlazor.Components.TwDatePicker"/>, <see cref="TwBlazor.Components.TwDateRangePicker"/>,
/// <see cref="TwBlazor.Components.TwDateTimeRangePicker"/>, <see cref="TwBlazor.Components.TwTimePicker"/>,
/// <see cref="TwBlazor.Components.TwTimeRangePicker"/>, <see cref="TwBlazor.Components.TwColorPicker"/>) and the
/// <see cref="TwBlazor.Components.TwPopover"/> component, reached through <see cref="TwOverlayTheme.Popover"/>.
/// </summary>
/// <remarks>
/// Width/height/max-height that genuinely varies per surface (e.g. the date picker's dual-month
/// layout) stays on that component's own theme class - see <see cref="TwDatePickerTheme.PanelWidth"/>.
/// Sizing that was just a single leftover literal with nothing else to vary (<see cref="TimeRangeSize"/>,
/// <see cref="ColorSize"/>) is consolidated here rather than getting its own one-property theme class.
/// </remarks>
[ExcludeFromCodeCoverage]
public class TwPopoverTheme
{
    /// <summary>
    /// Gets or sets the background for every popover panel surface (date/time/color picker panels).
    /// </summary>
    public required string Background { get; set; }

    /// <summary>
    /// Gets or sets the border for every popover panel surface.
    /// </summary>
    public required string Border { get; set; }

    /// <summary>
    /// Gets or sets the default border radius for popover panels.
    /// </summary>
    /// <remarks>
    /// If not set, falls back to global <see cref="TwBlazorRounded.DefaultRounded"/>. Individual
    /// pickers can still override this via their own <c>Rounded</c> parameter. Mirrors
    /// <see cref="TwDialogTheme.Rounded"/>.
    /// </remarks>
    public Rounded? Rounded { get; set; }

    /// <summary>
    /// Gets or sets the default shadow level for popover panels.
    /// </summary>
    /// <remarks>
    /// If not set, falls back to global <see cref="TwBlazorShadow.DefaultShadow"/>. Individual
    /// pickers can still override this via their own <c>Shadow</c> parameter. Mirrors
    /// <see cref="TwDialogTheme.Shadow"/>.
    /// </remarks>
    public Shadow? Shadow { get; set; }

    /// <summary>
    /// Gets or sets the width/padding classes for <see cref="TwBlazor.Components.TwTimeRangePicker"/>'s
    /// popover surface.
    /// </summary>
    public required string TimeRangeSize { get; set; }

    /// <summary>
    /// Gets or sets the width/padding classes (plus the <c>tw-color-picker-dialog</c> marker class
    /// tests select on) for <see cref="TwBlazor.Components.TwColorPicker"/>'s popover surface.
    /// </summary>
    public required string ColorSize { get; set; }

    /// <summary>
    /// Gets or sets the classes for the element that wraps a <see cref="TwBlazor.Components.TwPopover"/>'s trigger and
    /// panel.
    /// </summary>
    /// <remarks>Optional, so themes written before <see cref="TwBlazor.Components.TwPopover"/> keep working.</remarks>
    public string Root { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the layout classes for a <see cref="TwBlazor.Components.TwPopover"/>'s panel: its stacking order,
    /// width, padding and text. The panel's box comes from <see cref="Background"/> and <see cref="Border"/>.
    /// </summary>
    /// <remarks>Optional, so themes written before <see cref="TwBlazor.Components.TwPopover"/> keep working.</remarks>
    public string Panel { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the classes for a <see cref="TwBlazor.Components.TwPopover"/>'s title.
    /// </summary>
    /// <remarks>Optional, so themes written before <see cref="TwBlazor.Components.TwPopover"/> keep working.</remarks>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the classes for a <see cref="TwBlazor.Components.TwPopover"/>'s close button, placed in the
    /// panel's top corner.
    /// </summary>
    /// <remarks>Optional, so themes written before <see cref="TwBlazor.Components.TwPopover"/> keep working.</remarks>
    public string CloseButton { get; set; } = string.Empty;
}
