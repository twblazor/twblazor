// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

namespace TwBlazor.Enums;

/// <summary>
/// Defines which edge of the screen a <see cref="TwBlazor.Components.TwDrawer"/> slides in from.
/// </summary>
public enum DrawerPosition
{
    /// <summary>
    /// The end edge: the right in a left to right layout, the left in a right to left one. The default.
    /// </summary>
    End,

    /// <summary>
    /// The start edge: the left in a left to right layout, the right in a right to left one.
    /// </summary>
    Start,

    /// <summary>
    /// The top edge.
    /// </summary>
    Top,

    /// <summary>
    /// The bottom edge, as a bottom sheet.
    /// </summary>
    Bottom
}
