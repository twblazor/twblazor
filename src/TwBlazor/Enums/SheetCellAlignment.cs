// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

namespace TwBlazor.Enums;

/// <summary>
/// Defines how the content of a <see cref="TwBlazor.Components.TwSpreadsheet"/> cell lines up across the cell.
/// </summary>
public enum SheetCellAlignment
{
    /// <summary>
    /// Lines up by the kind of value, the same as a spreadsheet: numbers at the end of the cell, text at the start, and
    /// true or false values and errors in the middle.
    /// </summary>
    General,

    /// <summary>
    /// Lines up at the start of the cell: the left in a left to right layout.
    /// </summary>
    Start,

    /// <summary>
    /// Lines up in the middle of the cell.
    /// </summary>
    Center,

    /// <summary>
    /// Lines up at the end of the cell: the right in a left to right layout.
    /// </summary>
    End,
}
