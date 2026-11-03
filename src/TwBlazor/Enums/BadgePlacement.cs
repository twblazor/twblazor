// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

namespace TwBlazor.Enums;

/// <summary>
/// Defines which corner of its content a <see cref="TwBlazor.Components.TwBadge"/> overlays.
/// </summary>
public enum BadgePlacement
{
    /// <summary>
    /// The top-end corner: the top right in a left to right layout, the top left in a right to left one. The default.
    /// </summary>
    TopEnd,

    /// <summary>
    /// The top-start corner: the top left in a left to right layout, the top right in a right to left one.
    /// </summary>
    TopStart,

    /// <summary>
    /// The bottom-end corner: the bottom right in a left to right layout, the bottom left in a right to left one.
    /// </summary>
    BottomEnd,

    /// <summary>
    /// The bottom-start corner: the bottom left in a left to right layout, the bottom right in a right to left one.
    /// </summary>
    BottomStart
}
