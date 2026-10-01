// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using TwBlazor.Enums;
using TwBlazor.Models;

namespace TwBlazor.Configuration.Components;

/// <summary>
/// Shared "box" styling (background, border, rounded corners, shadow) for the two families of
/// floating surface in the library: the modal dialog (<see cref="TwBlazor.Components.TwDialog"/>) and
/// every popover panel (<see cref="TwBlazor.Components.TwDatePicker"/>,
/// <see cref="TwBlazor.Components.TwDateRangePicker"/>, <see cref="TwBlazor.Components.TwDateTimeRangePicker"/>,
/// <see cref="TwBlazor.Components.TwTimePicker"/>, <see cref="TwBlazor.Components.TwTimeRangePicker"/>,
/// <see cref="TwBlazor.Components.TwColorPicker"/>).
/// </summary>
/// <remarks>
/// Width/height/max-height that genuinely varies per surface (e.g. the date picker's dual-month
/// layout) stays on that component's own theme class - see <see cref="TwDatePickerTheme.PanelWidth"/>.
/// Sizing that was just a single leftover literal with nothing else to vary (<see cref="TimeRangePopoverSize"/>,
/// <see cref="ColorPopoverSize"/>) is consolidated here too rather than getting its own one-property
/// theme class. Override any property to restyle every dialog/popover in the app from one place
/// instead of editing each component's theme individually.
/// </remarks>
[ExcludeFromCodeCoverage]
public class TwOverlayTheme
{
    /// <summary>
    /// Gets or sets the background for the modal dialog surface.
    /// </summary>
    public required string DialogBackground { get; set; }

    /// <summary>
    /// Gets or sets the default border radius for dialogs.
    /// </summary>
    /// <remarks>
    /// If not set, falls back to global <see cref="TwBlazorRounded.DefaultRounded"/>.
    /// Individual dialogs can override this via <see cref="TwDialogOptions.Rounded"/>.
    /// </remarks>
    public Rounded? DialogRounded { get; set; }

    /// <summary>
    /// Gets or sets the default shadow level for dialogs.
    /// </summary>
    /// <remarks>
    /// If not set, falls back to global <see cref="TwBlazorShadow.DefaultShadow"/>.
    /// Individual dialogs can override this via <see cref="TwDialogOptions.Shadow"/>.
    /// </remarks>
    public Shadow? DialogShadow { get; set; }

    /// <summary>
    /// Gets or sets the background for every popover panel surface (date/time/color picker panels).
    /// </summary>
    public required string PopoverBackground { get; set; }

    /// <summary>
    /// Gets or sets the border for every popover panel surface.
    /// </summary>
    public required string PopoverBorder { get; set; }

    /// <summary>
    /// Gets or sets the default border radius for popover panels.
    /// </summary>
    /// <remarks>
    /// If not set, falls back to global <see cref="TwBlazorRounded.DefaultRounded"/>. Individual
    /// pickers can still override this via their own <c>Rounded</c> parameter. Mirrors
    /// <see cref="DialogRounded"/>.
    /// </remarks>
    public Rounded? PopoverRounded { get; set; }

    /// <summary>
    /// Gets or sets the default shadow level for popover panels.
    /// </summary>
    /// <remarks>
    /// If not set, falls back to global <see cref="TwBlazorShadow.DefaultShadow"/>. Individual
    /// pickers can still override this via their own <c>Shadow</c> parameter. Mirrors
    /// <see cref="DialogShadow"/>.
    /// </remarks>
    public Shadow? PopoverShadow { get; set; }

    // Width/padding for the handful of popovers whose surface isn't wrapped by a component-specific
    // positioning element carrying its own size (contrast TwDatePickerTheme.PanelWidth, which the
    // date picker family already keeps on its own theme since it's genuinely per-view/multi-part).
    // Consolidated here rather than as one lonely property per component theme class.

    /// <summary>
    /// Gets or sets the width/padding classes for <see cref="TwBlazor.Components.TwTimeRangePicker"/>'s
    /// popover surface.
    /// </summary>
    public required string TimeRangePopoverSize { get; set; }

    /// <summary>
    /// Gets or sets the width/padding classes (plus the <c>tw-color-picker-dialog</c> marker class
    /// tests select on) for <see cref="TwBlazor.Components.TwColorPicker"/>'s popover surface.
    /// </summary>
    public required string ColorPopoverSize { get; set; }

    /// <summary>
    /// Gets or sets the classes for the wrapper rendered around any control that has a tooltip (for
    /// example <see cref="TwBlazor.Components.TwButton.Tooltip"/>). Must establish a positioning context
    /// and the <c>group</c> hook that <see cref="TooltipBubble"/>'s hover/focus variants key off.
    /// </summary>
    public required string TooltipWrapper { get; set; }

    /// <summary>
    /// Gets or sets the layout, typography and visibility classes for a tooltip bubble: positioned above
    /// its control, hidden until the <see cref="TooltipWrapper"/> is hovered or the control is
    /// keyboard-focused. The surface (background, border, rounded corners, shadow) comes from the popover
    /// properties above via <c>PopoverBuilder.GetTooltipClasses</c>.
    /// </summary>
    public required string TooltipBubble { get; set; }
}
