// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;

namespace TwBlazor.Models;

/// <summary>
/// A table on a <see cref="Sheet"/> of a <see cref="Components.TwSpreadsheet"/>: a range whose first row holds the
/// column headings, with banded rows and a filter on each column, in the manner of an Excel table.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed class SheetTable
{
    /// <summary>
    /// Gets or sets the table's name, such as <c>Table1</c>.
    /// </summary>
    public string Name { get; set; } = "Table1";

    /// <summary>
    /// Gets or sets the cells the table covers, heading row included, such as <c>A1:D20</c>.
    /// </summary>
    public string Range { get; set; } = "A1:A2";

    /// <summary>
    /// Gets or sets whether every other row of the table is shaded. Defaults to <see langword="true"/>.
    /// </summary>
    public bool Banded { get; set; } = true;

    /// <summary>
    /// Gets or sets whether each heading has a button that filters and sorts its column. Defaults to
    /// <see langword="true"/>.
    /// </summary>
    public bool ShowFilterButtons { get; set; } = true;

    /// <summary>
    /// Gets or sets the filters on the table's columns, keyed by the column's letters, such as <c>B</c>. Each lists the
    /// values, as the cells show them, whose rows stay in view; the other rows are hidden. An empty string stands for
    /// empty cells. A column without an entry is not filtered.
    /// </summary>
    [SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "A table is built with an object initializer, often from stored data.")]
    public Dictionary<string, List<string>> Filters { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Gets or sets condition filters on the table's columns, keyed by the column's letters, such as <c>B</c>: one or
    /// two conditions such as "is greater than 100", the same as a <see cref="Components.TwDataTable{TItem}"/> column
    /// filter. Rows whose value does not meet them are hidden, as well as rows <see cref="Filters"/> hides.
    /// </summary>
    [SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "A table is built with an object initializer, often from stored data.")]
    public Dictionary<string, ColumnFilter> Conditions { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}
