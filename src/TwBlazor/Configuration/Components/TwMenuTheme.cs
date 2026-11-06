// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;

namespace TwBlazor.Configuration.Components;

/// <summary>
/// Theme configuration for menus (<see cref="TwBlazor.Components.TwMenu"/>,
/// <see cref="TwBlazor.Components.TwContextMenu"/> and their items and submenus), which share one look.
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

    /// <summary>
    /// Gets or sets the classes for the keyboard shortcut hint at the end of an item (e.g. <c>Ctrl+C</c>). It is
    /// pushed to the end of the item, so the hints of several items line up.
    /// </summary>
    public string ItemShortcut { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the classes added to a destructive item, such as Delete, which marks it in a warning colour.
    /// Colour is never the only sign, so give the item a clear label too.
    /// </summary>
    public string ItemDestructive { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the classes added to an inset item, which indents its text to line up with items that have an
    /// icon or a tick.
    /// </summary>
    public string ItemInset { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the classes added to the item that opens a submenu, including its look while the submenu is open
    /// (it has <c>aria-expanded="true"</c> then).
    /// </summary>
    public string SubmenuTrigger { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the classes for the arrow at the end of the item that opens a submenu.
    /// </summary>
    public string SubmenuIcon { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the classes for the area a <see cref="TwBlazor.Components.TwContextMenu"/> opens its menu from.
    /// When the area is focusable it needs a visible focus indicator.
    /// </summary>
    public string ContextArea { get; set; } = string.Empty;
}
