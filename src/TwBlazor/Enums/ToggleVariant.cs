// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

namespace TwBlazor.Enums;

/// <summary>
/// Defines how a <see cref="TwBlazor.Components.TwToggle"/> or <see cref="TwBlazor.Components.TwToggleGroup{T}"/>
/// is drawn while it is not pressed.
/// </summary>
public enum ToggleVariant
{
    /// <summary>
    /// No border or background until hovered or pressed, like a text button - the default. In a toggle group
    /// the items sit on a filled bar.
    /// </summary>
    Text,

    /// <summary>
    /// A visible border around the toggle. In a toggle group the border goes around the whole group instead.
    /// </summary>
    Outlined
}
