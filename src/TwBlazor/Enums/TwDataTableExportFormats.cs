// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

namespace TwBlazor.Enums;

/// <summary>
/// The file formats a <see cref="Components.TwDataTable{TItem}"/> offers export buttons for. Combine them with
/// <c>|</c>.
/// </summary>
[Flags]
public enum TwDataTableExportFormats
{
    /// <summary>
    /// No export buttons.
    /// </summary>
    None = 0,

    /// <summary>
    /// A comma separated values file, which opens in any spreadsheet or text editor.
    /// </summary>
    Csv = 1,

    /// <summary>
    /// An Excel workbook (<c>.xlsx</c>), which keeps numbers, dates and true or false values as their own types.
    /// </summary>
    Excel = 2,

    /// <summary>
    /// Both CSV and Excel.
    /// </summary>
    All = Csv | Excel
}
