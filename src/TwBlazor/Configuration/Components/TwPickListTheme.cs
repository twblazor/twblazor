// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;

namespace TwBlazor.Configuration.Components;

/// <summary>
/// Theme configuration for the pick list component (<see cref="TwBlazor.Components.TwPickList{TItem}"/>).
/// Override any property to customize pick list styles globally.
/// </summary>
[ExcludeFromCodeCoverage]
public class TwPickListTheme
{
    /// <summary>
    /// Gets or sets the base classes for the root element hosting the source column, transfer
    /// button column, and target column.
    /// </summary>
    public required string Container { get; set; }

    /// <summary>
    /// Gets or sets the classes for a single list's column (its header and list box together).
    /// </summary>
    public required string Column { get; set; }

    /// <summary>
    /// Gets or sets the classes for the row above a list containing its label and, when
    /// <see cref="TwBlazor.Components.TwPickList{TItem}.AllowReorder"/> is set, its reorder buttons.
    /// </summary>
    public required string ColumnHeader { get; set; }

    /// <summary>
    /// Gets or sets the classes for a column's label text.
    /// </summary>
    public required string Label { get; set; }

    /// <summary>
    /// Gets or sets the classes for the wrapper around a column's up/down reorder buttons.
    /// </summary>
    public required string ReorderButtons { get; set; }

    /// <summary>
    /// Gets or sets the classes for a move-up or move-down button in a column header.
    /// </summary>
    public string ReorderButton { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the classes for a button that moves items between the two lists.
    /// </summary>
    public string TransferButton { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the classes for the wrapper around the transfer (chevron) buttons between the
    /// two columns.
    /// </summary>
    public required string TransferButtonColumn { get; set; }

    /// <summary>
    /// Gets or sets the base classes for a list's <c>&lt;ul role="listbox"&gt;</c> element.
    /// </summary>
    public required string ListBox { get; set; }

    /// <summary>
    /// Gets or sets the classes applied to a list box when
    /// <see cref="TwBlazor.Components.TwPickList{TItem}.Disabled"/> is true.
    /// </summary>
    public required string ListBoxDisabled { get; set; }

    /// <summary>
    /// Gets or sets the base classes for a single <c>&lt;li role="option"&gt;</c> item row.
    /// </summary>
    public required string Item { get; set; }

    /// <summary>
    /// Gets or sets the classes applied to an item row when it is selected.
    /// </summary>
    public required string ItemSelected { get; set; }

    /// <summary>
    /// Gets or sets the classes for the placeholder row shown when a list has no items.
    /// </summary>
    public required string EmptyState { get; set; }
}
