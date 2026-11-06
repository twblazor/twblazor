// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

namespace TwBlazor.Components;

/// <summary>
/// The component that owns an open menu: a <see cref="TwMenu"/> or a <see cref="TwContextMenu"/>. It cascades itself
/// to its items, so choosing an item can close the whole menu, submenus included, whichever kind opened it.
/// </summary>
internal interface ITwMenuHost
{
    /// <summary>
    /// Closes the menu.
    /// </summary>
    /// <param name="returnFocus">Whether focus goes back to where it was before the menu opened.</param>
    /// <returns>A task that completes once the menu is closed.</returns>
    Task CloseMenuAsync(bool returnFocus);
}
