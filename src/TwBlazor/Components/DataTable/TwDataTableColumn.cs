// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

namespace TwBlazor.Components;

/// <summary>
/// Defines a column in the TwDataTable with its properties and behavior.
/// </summary>
/// <typeparam name="TItem">The type of items displayed in the table.</typeparam>
public class TwDataTableColumn<TItem>
{
    /// <summary>
    /// Gets or sets the unique name/key for the column.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the display title for the column header.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the property selector expression for accessing the column value.
    /// </summary>
    public Func<TItem, object?>? PropertySelector { get; set; }

    /// <summary>
    /// Gets or sets whether this column is sortable.
    /// </summary>
    public bool IsSortable { get; set; } = false;

    /// <summary>
    /// Gets or sets the width of the column as any CSS length, such as <c>12rem</c>, <c>160px</c> or <c>25%</c>.
    /// When any column has a width, the table renders a <see cref="TwTableColumn"/> for every column.
    /// </summary>
    public string? Width { get; set; }

    /// <summary>
    /// Gets or sets additional CSS classes for the header cell.
    /// </summary>
    public string? HeaderClass { get; set; }

    /// <summary>
    /// Gets or sets additional CSS classes for the data cells.
    /// </summary>
    public string? CellClass { get; set; }

    /// <summary>
    /// Gets or sets a custom render function for the cell content.
    /// </summary>
    public Func<TItem, string>? CellFormatter { get; set; }
}
