// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using Microsoft.AspNetCore.Components;

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
    /// Gets or sets whether the column is shown when the table first renders, or after its columns are reset.
    /// Defaults to <c>true</c>. People can show or hide it from the column panel when
    /// <see cref="TwDataTable{TItem}.ColumnPanel"/> is enabled.
    /// </summary>
    public bool Visible { get; set; } = true;

    /// <summary>
    /// Gets or sets whether people can hide the column from the column panel. Defaults to <c>true</c>. Set it to
    /// <c>false</c> for a column that identifies each row, such as a name.
    /// </summary>
    public bool Hideable { get; set; } = true;

    /// <summary>
    /// Gets or sets whether people can resize the column when <see cref="TwDataTable{TItem}.ResizableColumns"/>
    /// is enabled. Defaults to <c>true</c>.
    /// </summary>
    public bool Resizable { get; set; } = true;

    /// <summary>
    /// Gets or sets whether the column is included when the table is exported to CSV or Excel. Defaults to
    /// <c>true</c>. Set it to <c>false</c> for a column of buttons or other content that only makes sense on screen.
    /// </summary>
    public bool Exportable { get; set; } = true;

    /// <summary>
    /// Gets or sets the value written for the column when the table is exported, in place of
    /// <see cref="PropertySelector"/>. Return a number, date or <see cref="bool"/> to keep its type in Excel, or a
    /// string for text.
    /// </summary>
    public Func<TItem, object?>? ExportFormatter { get; set; }

    /// <summary>
    /// Gets or sets whether people can group the rows by this column from the table's Group by list. Rows group by
    /// the <see cref="PropertySelector"/> value, and each group is labelled with <see cref="CellFormatter"/> when
    /// the column has one.
    /// </summary>
    public bool Groupable { get; set; }

    /// <summary>
    /// Gets or sets whether the column can be changed when a row is edited, with
    /// <see cref="TwDataTable{TItem}.Editable"/> set. The editor follows the value's type: a text field for text, a
    /// number field for numbers, a checkbox for <see cref="bool"/>, a date picker for <see cref="DateTime"/> and a
    /// list for an enum. Use <see cref="EditTemplate"/> for any other editor.
    /// </summary>
    public bool Editable { get; set; }

    /// <summary>
    /// Gets or sets how a saved value is written back to the row, such as <c>(product, value) =&gt; product.Price =
    /// (decimal)value!</c>. Without it the new values only reach <see cref="TwDataTable{TItem}.RowSaved"/>.
    /// </summary>
    public Action<TItem, object?>? ValueSetter { get; set; }

    /// <summary>
    /// Gets or sets a custom editor for the column, in place of the one chosen from the value's type.
    /// </summary>
    public RenderFragment<TwDataTableCellEditContext<TItem>>? EditTemplate { get; set; }

    /// <summary>
    /// Gets or sets a check run on the new value when a row is saved. Return a message saying what to fix, or
    /// <c>null</c> when the value is fine. The row is not saved while any column reports a problem.
    /// </summary>
    public Func<TItem, object?, string?>? Validator { get; set; }

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
