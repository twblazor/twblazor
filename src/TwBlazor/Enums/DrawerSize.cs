// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

namespace TwBlazor.Enums;

/// <summary>
/// Defines how far a <see cref="TwBlazor.Components.TwDrawer"/> reaches into the screen: its width when it slides
/// in from the start or end edge, its height when it slides in from the top or bottom.
/// </summary>
public enum DrawerSize
{
    /// <summary>
    /// A narrow panel, for a short list of options or a few details.
    /// </summary>
    Small,

    /// <summary>
    /// A panel wide enough for a form. The default.
    /// </summary>
    Medium,

    /// <summary>
    /// A wide panel, for a detail view or a table.
    /// </summary>
    Large,

    /// <summary>
    /// The whole screen.
    /// </summary>
    Full
}
