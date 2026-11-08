// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;

namespace TwBlazor.Models;

/// <summary>
/// A sheet of a <see cref="Components.TwSpreadsheet"/> workbook: its name and the cells that hold something.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed class Sheet
{
    /// <summary>
    /// Gets or sets the name shown on the sheet's tab, which formulas on other sheets use to refer to it, as in
    /// <c>=Budget!B4</c>. Defaults to <c>Sheet1</c>.
    /// </summary>
    public string Name { get; set; } = "Sheet1";

    /// <summary>
    /// Gets or sets the cells that hold something, keyed by their address, such as <c>A1</c> or <c>C12</c>. A cell
    /// that is not in the dictionary is empty.
    /// </summary>
    /// <remarks>The sheet edits this dictionary in place as the user types. Addresses are matched without regard to
    /// case.</remarks>
    [SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "A sheet is built with an object initializer, often from stored data.")]
    public Dictionary<string, SheetCell> Cells { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Gets or sets the number of rows at the top that stay in view while the sheet scrolls down. Defaults to 0.
    /// </summary>
    public int FrozenRows { get; set; }

    /// <summary>
    /// Gets or sets the number of columns at the start that stay in view while the sheet scrolls across. Defaults to
    /// 0.
    /// </summary>
    public int FrozenColumns { get; set; }

    /// <summary>
    /// Gets or sets the widths of the columns that are not the spreadsheet's
    /// <see cref="Components.TwSpreadsheet.ColumnWidth"/>, in pixels, keyed by their letters, such as <c>A</c>.
    /// </summary>
    /// <remarks>The spreadsheet adds to this dictionary as the user resizes columns.</remarks>
    [SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "A sheet is built with an object initializer, often from stored data.")]
    public Dictionary<string, int> ColumnWidths { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Gets or sets the heights of the rows that are not 32 pixels, in pixels, keyed by their row number, starting
    /// at 1.
    /// </summary>
    /// <remarks>The spreadsheet adds to this dictionary as the user resizes rows, or picks a text size too large for a
    /// row.</remarks>
    [SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "A sheet is built with an object initializer, often from stored data.")]
    public Dictionary<int, int> RowHeights { get; set; } = [];

    /// <summary>
    /// Gets or sets the tables on the sheet: ranges with a heading row, banded rows and a filter on each column.
    /// </summary>
    [SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "A sheet is built with an object initializer, often from stored data.")]
    public List<SheetTable> Tables { get; set; } = [];
}
