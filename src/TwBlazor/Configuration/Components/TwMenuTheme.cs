// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;

namespace TwBlazor.Configuration.Components;

/// <summary>
/// Theme configuration for menus (<see cref="TwBlazor.Components.TwMenu"/> and its items).
/// Override any property to customize menu styles globally.
/// </summary>
/// <remarks>
/// The button that opens a menu is a <see cref="TwBlazor.Components.TwButton"/>, or a
/// <see cref="TwBlazor.Components.TwIcon"/> button when the menu is icon only, so it takes its look from those themes.
/// </remarks>
[ExcludeFromCodeCoverage]
public class TwMenuTheme
{
    /// <summary>
    /// Gets or sets the classes for the element around the button and the menu.
    /// </summary>
    public required string Root { get; set; }

    /// <summary>
    /// Gets or sets the classes for the menu panel. The menu script positions it with fixed positioning, so it is not
    /// clipped by a scrolling container such as a table.
    /// </summary>
    public required string Panel { get; set; }

    /// <summary>
    /// Gets or sets the classes for a menu item, including its hover and focus states. Focus moves between items with
    /// the arrow keys, so the focused item needs a visible indicator.
    /// </summary>
    public required string Item { get; set; }

    /// <summary>
    /// Gets or sets the classes added to a disabled menu item.
    /// </summary>
    public required string ItemDisabled { get; set; }

    /// <summary>
    /// Gets or sets the classes for the icon at the start of a menu item.
    /// </summary>
    public required string ItemIcon { get; set; }

    /// <summary>
    /// Gets or sets the classes for the tick beside a checkbox item. It keeps its space while unticked, so the labels
    /// line up.
    /// </summary>
    public required string CheckIcon { get; set; }

    /// <summary>
    /// Gets or sets the classes added to the tick while a checkbox item is unticked, which hide it but keep its space
    /// (e.g. <c>invisible</c>).
    /// </summary>
    public required string CheckIconHidden { get; set; }

    /// <summary>
    /// Gets or sets the classes for the line between groups of items.
    /// </summary>
    public required string Divider { get; set; }

    /// <summary>
    /// Gets or sets the classes for the heading of a group of items.
    /// </summary>
    public required string GroupLabel { get; set; }
}
