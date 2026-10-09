// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using TwBlazor.Enums;

namespace TwBlazor.Models;

/// <summary>
/// Represents a navigation item in a menu or sidebar.
/// </summary>
[ExcludeFromCodeCoverage]
public class NavigationItem
{
    /// <summary>
    /// Gets or sets the element id for this navigation item.
    /// </summary>
    /// <remarks>Rendered as the <c>id</c> attribute on the item's link (or toggle button, for a parent
    /// item). When not supplied, a parent item falls back to an id derived from the item instance so its
    /// toggle button can still be linked to its collapsible children via <c>aria-controls</c>.</remarks>
    public string? Id { get; set; }

    /// <summary>
    /// Gets or sets the URL this navigation item links to.
    /// </summary>
    /// <remarks>Left <see langword="null"/> for a parent item whose only purpose is to group
    /// <see cref="NavigationItems"/>. Also used to determine whether the item is the current page
    /// (see <see cref="IsActive"/>) by comparing it against the current URL.</remarks>
    public string? Href { get; set; }

    /// <summary>
    /// Gets or sets the icon rendered alongside the item's <see cref="Label"/>.
    /// </summary>
    public Icon? Icon { get; set; }

    /// <summary>
    /// Gets or sets the display text for this navigation item.
    /// </summary>
    public string? Label { get; set; }

    /// <summary>
    /// Gets or sets the nested navigation items rendered as this item's collapsible children.
    /// </summary>
    /// <remarks>A non-empty list makes this a parent item: it renders as a toggle button (rather than a
    /// link) that expands or collapses this list instead of navigating anywhere itself.</remarks>
    public List<NavigationItem> NavigationItems { get; set; } = [];

    /// <summary>
    /// Gets or sets whether this item's <see cref="NavigationItems"/> are collapsed.
    /// </summary>
    /// <remarks>Only relevant when <see cref="NavigationItems"/> is non-empty. Defaults to
    /// <see langword="true"/> so nested items start collapsed until the user expands the parent.</remarks>
    public bool Collapsed { get; set; } = true;

    /// <summary>
    /// Gets or sets whether this item belongs in a top navigation bar rather than a sidebar.
    /// </summary>
    /// <remarks>Not read by any TwBlazor component itself - it's carried through unchanged (e.g. by
    /// <see cref="TwBlazor.Components.TwSidebar"/>'s search filtering) for consumers who render one shared
    /// list of items and need to split it between a top nav and a sidebar themselves.</remarks>
    public bool TopNavigation { get; set; }

    /// <summary>
    /// Gets or sets whether this item is excluded from rendering.
    /// </summary>
    public bool Hidden { get; set; }


    /// <summary>
    /// Gets or sets whether this item should be marked as the active page.
    /// </summary>
    /// <remarks>Takes precedence over the automatic URL-based match against <see cref="Href"/>, so it can
    /// be used to force an item active for routes that don't match <see cref="Href"/> exactly.</remarks>
    public bool IsActive { get; set; }
}
