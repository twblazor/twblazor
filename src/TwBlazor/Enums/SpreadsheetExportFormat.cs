// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

namespace TwBlazor.Enums;

/// <summary>
/// Defines a file format a <see cref="TwBlazor.Components.TwSpreadsheet"/> workbook can be exported as.
/// </summary>
public enum SpreadsheetExportFormat
{
    /// <summary>
    /// An Excel workbook (<c>.xlsx</c>) with every sheet, the formulas, their last results and the cell formatting.
    /// It opens in Microsoft Excel, Google Sheets, Numbers and LibreOffice.
    /// </summary>
    Excel,

    /// <summary>
    /// Comma separated values (<c>.csv</c>) of the sheet being shown, holding the results of its formulas.
    /// </summary>
    Csv,
}
