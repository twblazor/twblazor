// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

namespace TwBlazor.Enums;

/// <summary>
/// Which rows of a <see cref="Components.TwDataTable{TItem}"/> an export includes.
/// </summary>
public enum TwDataTableExportScope
{
    /// <summary>
    /// Every row that matches the search, in the current sort order, across all pages.
    /// </summary>
    Filtered,

    /// <summary>
    /// Only the rows on the current page.
    /// </summary>
    CurrentPage,

    /// <summary>
    /// Every row in <see cref="Components.TwDataTable{TItem}.Items"/>, in its original order, ignoring the search.
    /// </summary>
    All
}
