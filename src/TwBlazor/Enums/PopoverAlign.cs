// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

namespace TwBlazor.Enums;

/// <summary>
/// Defines how a <see cref="TwBlazor.Components.TwPopover"/> panel lines up with its trigger along the side it opens on.
/// </summary>
public enum PopoverAlign
{
    /// <summary>
    /// Lines up with the trigger's starting edge: its left edge in a left to right layout, or its top edge when the
    /// panel opens to the side. The default.
    /// </summary>
    Start,

    /// <summary>
    /// Centred on the trigger.
    /// </summary>
    Center,

    /// <summary>
    /// Lines up with the trigger's ending edge.
    /// </summary>
    End
}
