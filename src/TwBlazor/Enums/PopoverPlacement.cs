// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

namespace TwBlazor.Enums;

/// <summary>
/// Defines which side of its trigger a <see cref="TwBlazor.Components.TwPopover"/> panel opens on. When there is not
/// enough room on that side, the panel flips to the opposite one.
/// </summary>
public enum PopoverPlacement
{
    /// <summary>
    /// Below the trigger. The default.
    /// </summary>
    Bottom,

    /// <summary>
    /// Above the trigger.
    /// </summary>
    Top,

    /// <summary>
    /// Before the trigger: to its left in a left to right layout, to its right in a right to left one.
    /// </summary>
    Start,

    /// <summary>
    /// After the trigger: to its right in a left to right layout, to its left in a right to left one.
    /// </summary>
    End
}
