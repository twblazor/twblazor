// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;

namespace TwBlazor.Configuration.Components;

/// <summary>
/// Theme configuration for every floating surface in the library, grouped by kind: generic popover
/// panels (<see cref="Popover"/>), the modal dialog (<see cref="Dialog"/>) and tooltips
/// (<see cref="Tooltip"/>).
/// </summary>
/// <remarks>
/// This is the only overlay theme registered in <see cref="TwBlazorTheme.Components"/>; components retrieve
/// the group they need from it, e.g. <c>Require&lt;TwOverlayTheme&gt;().Popover</c>. Override a property on
/// any group to restyle every dialog/popover/tooltip in the app from one place instead of editing each
/// component's theme individually.
/// </remarks>
[ExcludeFromCodeCoverage]
public class TwOverlayTheme
{
    /// <summary>
    /// Gets or sets the generic popover panel styling shared by the date, time and color pickers.
    /// </summary>
    public required TwPopoverTheme Popover { get; set; }

    /// <summary>
    /// Gets or sets the modal dialog styling used by <see cref="TwBlazor.Components.TwDialog"/>.
    /// </summary>
    public required TwDialogTheme Dialog { get; set; }

    /// <summary>
    /// Gets or sets the tooltip styling used by <see cref="TwBlazor.Components.TwTooltip"/> and the
    /// <c>Tooltip</c> parameter on buttons and icons.
    /// </summary>
    public required TwTooltipTheme Tooltip { get; set; }
}
