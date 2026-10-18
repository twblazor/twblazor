// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using System.Text;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using TwBlazor.Enums;
using TwBlazor.Localization;

namespace TwBlazor.Components;

public partial class TwDataTable<TItem>
{
    /// <summary>
    /// Whether to show buttons that download the table as a file. Only applies when <see cref="Columns"/> is
    /// used. Defaults to <c>false</c>.
    /// </summary>
    [Parameter]
    public bool Exportable { get; set; }

    /// <summary>
    /// Gets or sets which file formats get an export button. Defaults to <see cref="TwDataTableExportFormats.All"/>.
    /// </summary>
    [Parameter]
    public TwDataTableExportFormats ExportFormats { get; set; } = TwDataTableExportFormats.All;

    /// <summary>
    /// Gets or sets which rows the export buttons include. Defaults to <see cref="TwDataTableExportScope.Filtered"/>:
    /// every row that matches the search, in the order it is sorted.
    /// </summary>
    [Parameter]
    public TwDataTableExportScope ExportScope { get; set; } = TwDataTableExportScope.Filtered;

    /// <summary>
    /// Gets or sets the name of the downloaded file, without its extension. It also names the Excel worksheet.
    /// Defaults to <c>export</c>.
    /// </summary>
    [Parameter]
    public string ExportFileName { get; set; } = "export";

    private bool showExportButtons => Exportable && hasColumns && ExportFormats != TwDataTableExportFormats.None;

    /// <summary>
    /// Writes the table's rows as CSV text. The columns are the ones on screen, in the order they are shown,
    /// leaving out any column whose <see cref="TwDataTableColumn{TItem}.Exportable"/> is <c>false</c>.
    /// </summary>
    /// <param name="scope">Which rows to write, or <c>null</c> for <see cref="ExportScope"/>.</param>
    /// <returns>The CSV text.</returns>
    public string GetCsv(TwDataTableExportScope? scope = null) =>
        TwDataTableExport.ToCsv(ExportRows(scope ?? ExportScope), visibleColumns);

    /// <summary>
    /// Writes the table's rows as an Excel workbook, with the same columns as <see cref="GetCsv"/>.
    /// </summary>
    /// <param name="scope">Which rows to write, or <c>null</c> for <see cref="ExportScope"/>.</param>
    /// <returns>The contents of an <c>.xlsx</c> file.</returns>
    public byte[] GetExcel(TwDataTableExportScope? scope = null) =>
        TwDataTableExport.ToExcel(ExportRows(scope ?? ExportScope), visibleColumns, ExportFileName);

    /// <summary>
    /// Downloads the table's rows as a CSV file named after <see cref="ExportFileName"/>.
    /// </summary>
    /// <param name="scope">Which rows to download, or <c>null</c> for <see cref="ExportScope"/>.</param>
    /// <returns>A task that completes once the download has been handed to the browser.</returns>
    public async Task ExportCsvAsync(TwDataTableExportScope? scope = null)
    {
        var rows = ExportRows(scope ?? ExportScope);

        // The byte order mark tells Excel the file is UTF-8, so accented and non Latin text opens correctly.
        var bytes = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(TwDataTableExport.ToCsv(rows, visibleColumns))).ToArray();
        await DownloadAsync($"{ExportFileName}.csv", TwDataTableExport.CsvContentType, bytes, rows.Count);
    }

    /// <summary>
    /// Downloads the table's rows as an Excel workbook named after <see cref="ExportFileName"/>.
    /// </summary>
    /// <param name="scope">Which rows to download, or <c>null</c> for <see cref="ExportScope"/>.</param>
    /// <returns>A task that completes once the download has been handed to the browser.</returns>
    public async Task ExportExcelAsync(TwDataTableExportScope? scope = null)
    {
        var rows = ExportRows(scope ?? ExportScope);
        await DownloadAsync($"{ExportFileName}.xlsx", TwDataTableExport.ExcelContentType, TwDataTableExport.ToExcel(rows, visibleColumns, ExportFileName), rows.Count);
    }

    private List<TItem> ExportRows(TwDataTableExportScope scope) => scope switch
    {
        TwDataTableExportScope.CurrentPage => [.. displayedRows],
        TwDataTableExportScope.All => [.. Items.Where(item => !EqualityComparer<TItem>.Default.Equals(item, default))],
        _ => [.. viewItems]
    };

    private async Task DownloadAsync(string fileName, string contentType, byte[] content, int rowCount)
    {
        using var stream = new MemoryStream(content);
        using var streamReference = new DotNetStreamReference(stream);

        try
        {
            await jsRuntime.InvokeVoidAsync("twDataTable.download", fileName, contentType, streamReference);
        }
        catch (JSDisconnectedException)
        {
            return;
        }
        catch (InvalidOperationException)
        {
            return;
        }

        sortStatusMessage = localizer.Plural(rowCount, TwStrings.DataTable_ExportedOne, TwStrings.DataTable_ExportedOther, rowCount);
        await InvokeAsync(StateHasChanged);
    }
}
