// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using System.Globalization;
using System.Text;
using TwBlazor.Enums;
using TwBlazor.Export.Spreadsheet;
using TwBlazor.Models;
using TwBlazor.Spreadsheet;

namespace TwBlazor.Components;

/// <summary>
/// Writes a <see cref="TwSpreadsheet"/> workbook out as an Excel file or as CSV. <see cref="TwSpreadsheet"/> uses it for its export
/// menu, and it can be called on its own, for example to build a file on the server from stored sheets.
/// </summary>
/// <remarks>Formulas are worked out with the same engine as the component, so a file built on the server holds the
/// same results the user saw.</remarks>
public static class TwSpreadsheetExport
{
    /// <summary>
    /// The media type of a CSV file.
    /// </summary>
    public const string CsvContentType = "text/csv";

    /// <summary>
    /// The media type of an Excel workbook.
    /// </summary>
    public const string ExcelContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

    private static readonly char[] _csvSpecialCharacters = [',', '"', '\r', '\n'];

    /// <summary>
    /// Writes the workbook as an Excel file with one worksheet per sheet. Formulas are kept, with their results saved
    /// alongside, and each cell keeps its bold, italic, underline, alignment and number format.
    /// </summary>
    /// <param name="sheets">The sheets, in tab order.</param>
    /// <param name="culture">The culture whose currency symbol currency cells are saved with, or
    /// <see langword="null"/> for the current culture.</param>
    /// <returns>The contents of an <c>.xlsx</c> file.</returns>
    public static byte[] ToExcel(IReadOnlyList<Sheet> sheets, CultureInfo? culture = null)
    {
        ArgumentNullException.ThrowIfNull(sheets);
        return XlsxWorkbookWriter.Write(sheets, culture ?? CultureInfo.CurrentCulture);
    }

    /// <summary>
    /// Writes one sheet as CSV text, from <c>A1</c> to the last row and column that hold something. Formulas are
    /// written as their results, numbers in an invariant format so any spreadsheet reads them the same way, and text
    /// that a spreadsheet would run as a formula is prefixed with an apostrophe.
    /// </summary>
    /// <param name="sheets">The sheets, so formulas that refer to other sheets can be worked out.</param>
    /// <param name="sheetIndex">The index of the sheet to write. Defaults to the first.</param>
    /// <returns>The CSV text, with a line break after every row.</returns>
    public static string ToCsv(IReadOnlyList<Sheet> sheets, int sheetIndex = 0)
    {
        ArgumentNullException.ThrowIfNull(sheets);
        ArgumentOutOfRangeException.ThrowIfNegative(sheetIndex);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(sheetIndex, sheets.Count);

        var calculator = new SheetCalculator(() => sheets);
        var cells = XlsxWorkbookWriter.SortedCells(sheets[sheetIndex]).ToList();
        var csv = new StringBuilder();
        if (cells.Count == 0)
        {
            return string.Empty;
        }

        var lastRow = cells.Max(c => c.Address.Row);
        var lastColumn = cells.Max(c => c.Address.Column);
        for (var row = 0; row <= lastRow; row++)
        {
            for (var column = 0; column <= lastColumn; column++)
            {
                if (column > 0)
                {
                    csv.Append(',');
                }

                csv.Append(CsvField(calculator.GetValue(sheetIndex, new CellAddress(row, column))));
            }

            csv.Append("\r\n");
        }

        return csv.ToString();
    }

    /// <summary>
    /// Writes the workbook as a file in <paramref name="format"/>, with the file name, extension and media type to save
    /// it with.
    /// </summary>
    /// <param name="sheets">The sheets, in tab order.</param>
    /// <param name="format">The format.</param>
    /// <param name="fileName">The file name without its extension. Characters a file name cannot hold are replaced.
    /// Defaults to <c>workbook</c>.</param>
    /// <param name="sheetIndex">For CSV, which holds one sheet, the index of the sheet to write.</param>
    /// <param name="culture">The culture whose currency symbol currency cells are saved with, or
    /// <see langword="null"/> for the current culture.</param>
    /// <returns>The file.</returns>
    public static ExportFile Export(IReadOnlyList<Sheet> sheets, SpreadsheetExportFormat format, string fileName = "workbook", int sheetIndex = 0, CultureInfo? culture = null)
    {
        var safeName = TwTextEditorExport.SafeFileName(string.IsNullOrWhiteSpace(fileName) ? "workbook" : fileName.Trim());
        return format switch
        {
            SpreadsheetExportFormat.Excel => new ExportFile { FileName = $"{safeName}.xlsx", ContentType = ExcelContentType, Content = ToExcel(sheets, culture) },
            SpreadsheetExportFormat.Csv => new ExportFile { FileName = $"{safeName}.csv", ContentType = CsvContentType, Content = [.. Encoding.UTF8.GetPreamble(), .. new UTF8Encoding(false).GetBytes(ToCsv(sheets, sheetIndex))] },
            _ => throw new ArgumentOutOfRangeException(nameof(format), format, null),
        };
    }

    /// <summary>
    /// Gets the extension a format's files are saved with, such as <c>.xlsx</c>.
    /// </summary>
    /// <param name="format">The format.</param>
    /// <returns>The extension, with its dot.</returns>
    public static string ExtensionOf(SpreadsheetExportFormat format) => format switch
    {
        SpreadsheetExportFormat.Excel => ".xlsx",
        SpreadsheetExportFormat.Csv => ".csv",
        _ => throw new ArgumentOutOfRangeException(nameof(format), format, null),
    };

    private static string CsvField(SheetValue value)
    {
        var text = value.Kind switch
        {
            SheetValueKind.Number => value.Number.ToString("R", CultureInfo.InvariantCulture),
            SheetValueKind.Text => GuardFormula(value.Text ?? string.Empty),
            _ => value.AsText(),
        };

        return text.IndexOfAny(_csvSpecialCharacters) >= 0
            ? $"\"{text.Replace("\"", "\"\"", StringComparison.Ordinal)}\""
            : text;
    }

    /// <summary>
    /// Stops text that starts like a formula from running when the file is opened in a spreadsheet (CSV injection).
    /// </summary>
    private static string GuardFormula(string text) =>
        text.Length > 0 && text[0] is '=' or '+' or '-' or '@' or '\t' or '\r' ? $"'{text}" : text;
}
