// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;

namespace TwBlazor.Configuration.Components;

/// <summary>
/// Theme configuration for tooltips: <see cref="TwBlazor.Components.TwTooltip"/> and the <c>Tooltip</c>
/// parameter on <see cref="TwBlazor.Components.TwButton"/> and <see cref="TwBlazor.Components.TwIcon"/>.
/// Override any property to customize tooltip styles globally. The bubble's surface (background, border,
/// rounded corners, shadow) comes from <see cref="TwPopoverTheme"/>. Reached through <see cref="TwOverlayTheme.Tooltip"/>.
/// </summary>
[ExcludeFromCodeCoverage]
public class TwTooltipTheme
{
    /// <summary>
    /// Gets or sets the classes for the wrapper rendered around any control that has a tooltip. Must
    /// establish a positioning context and the <c>group</c> hook that <see cref="Bubble"/>'s hover/focus
    /// variants key off.
    /// </summary>
    public required string Wrapper { get; set; }

    /// <summary>
    /// Gets or sets the layout, typography and visibility classes for a tooltip bubble: hidden until the
    /// <see cref="Wrapper"/> is hovered or the control is keyboard-focused. Where the bubble sits comes from
    /// <see cref="Top"/>, <see cref="Bottom"/>, <see cref="Left"/> or <see cref="Right"/>.
    /// </summary>
    public required string Bubble { get; set; }

    /// <summary>
    /// Gets or sets the offset classes that place a tooltip bubble above its control
    /// (<see cref="TwBlazor.Enums.TooltipPlacement.Top"/>, the default).
    /// </summary>
    public required string Top { get; set; }

    /// <summary>
    /// Gets or sets the offset classes that place a tooltip bubble below its control.
    /// </summary>
    public required string Bottom { get; set; }

    /// <summary>
    /// Gets or sets the offset classes that place a tooltip bubble to the left of its control.
    /// </summary>
    public required string Left { get; set; }

    /// <summary>
    /// Gets or sets the offset classes that place a tooltip bubble to the right of its control.
    /// </summary>
    public required string Right { get; set; }
}
