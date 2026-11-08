// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Globalization;
using TwBlazor.Localization;
using TwBlazor.Models;
using TwBlazor.Spreadsheet;

namespace TwBlazor.Components;

/// <summary>
/// Changing the shape of a sheet (inserting and deleting rows and columns, sorting) and the font and colour controls.
/// </summary>
public partial class TwSpreadsheet
{
    private static readonly string[] _defaultFonts = ["Arial", "Calibri", "Cambria", "Courier New", "Georgia", "Segoe UI", "Times New Roman", "Verdana"];

    private static readonly double[] _defaultFontSizes = [8, 9, 10, 11, 12, 14, 16, 18, 20, 24];

    /// <summary>
    /// Gets or sets the fonts offered by the toolbar's font list, after a "Default" option that uses the theme's font.
    /// <see langword="null"/> (the default) offers Arial, Calibri, Cambria, Courier New, Georgia, Segoe UI, Times New
    /// Roman and Verdana.
    /// </summary>
    /// <remarks>Each font is shown in its own typeface in the list, and falls back to the browser's when it is not
    /// installed.</remarks>
    [Parameter] public IReadOnlyList<string>? Fonts { get; set; }

    /// <summary>
    /// Gets or sets the text sizes offered by the toolbar's size list, in points. <see langword="null"/> (the default)
    /// offers 8 to 24.
    /// </summary>
    /// <remarks>Rows keep their height, so text much larger than 18 points is cut off.</remarks>
    [Parameter] public IReadOnlyList<double>? FontSizes { get; set; }

    /// <summary>
    /// Gets or sets the colours offered by the toolbar's text color menu. <see langword="null"/> (the default) offers
    /// ten dark colours with names in the app's language.
    /// </summary>
    [Parameter] public IReadOnlyList<SpreadsheetColor>? TextColors { get; set; }

    /// <summary>
    /// Gets or sets the colours offered by the toolbar's fill color menu. <see langword="null"/> (the default) offers
    /// nine light colours with names in the app's language.
    /// </summary>
    /// <remarks>A filled cell without its own text colour gets black or white text, whichever reads better on the
    /// fill.</remarks>
    [Parameter] public IReadOnlyList<SpreadsheetColor>? FillColors { get; set; }

    private List<FontOption> fontOptions = [];

    private List<FontSizeOption> fontSizeOptions = [];

    private IReadOnlyList<SpreadsheetColor> textColors = [];

    private IReadOnlyList<SpreadsheetColor> fillColors = [];

    /// <summary>
    /// A font in the toolbar's list. A <see langword="null"/> family is the theme's own font.
    /// </summary>
    private sealed record FontOption(string? Family, string Name)
    {
        /// <inheritdoc />
        public override string ToString() => Name;
    }

    /// <summary>
    /// A text size in the toolbar's list. A <see langword="null"/> size is the theme's own size.
    /// </summary>
    private sealed record FontSizeOption(double? Size, string Name)
    {
        /// <inheritdoc />
        public override string ToString() => Name;
    }

    /// <summary>
    /// What one sheet looked like, kept for undoing a change to its shape.
    /// </summary>
    private sealed record SheetSnapshot(Dictionary<string, SheetCell> Cells, Dictionary<string, int> Widths, int FrozenRows, int FrozenColumns, Dictionary<int, int> Heights, List<SheetTable> Tables);

    private static SheetTable CloneTable(SheetTable table) => new()
    {
        Name = table.Name,
        Range = table.Range,
        Banded = table.Banded,
        ShowFilterButtons = table.ShowFilterButtons,
        Filters = table.Filters.ToDictionary(p => p.Key, p => p.Value.ToList(), StringComparer.OrdinalIgnoreCase),
        Conditions = table.Conditions.Where(p => p.Value is not null).ToDictionary(p => p.Key, p => p.Value.Clone(), StringComparer.OrdinalIgnoreCase),
    };

    private FontOption? selectedFont => fontOptions.FirstOrDefault(o => string.Equals(o.Family, activeCell?.FontFamily, StringComparison.OrdinalIgnoreCase));

    private FontSizeOption? selectedFontSize => fontSizeOptions.FirstOrDefault(o => o.Size == activeCell?.FontSize);

    private void BuildFormattingOptions()
    {
        fontOptions = [.. (Fonts ?? _defaultFonts).Where(f => SheetFormatting.SafeFontFamily(f) is not null).Distinct(StringComparer.OrdinalIgnoreCase).Select(f => new FontOption(f, f))];
        fontSizeOptions = [.. (FontSizes ?? _defaultFontSizes).Select(s => SheetFormatting.SafeFontSize(s)).OfType<double>().Distinct().Order().Select(s => new FontSizeOption(s, s.ToString(culture)))];

        textColors = TextColors ??
        [
            Named(TwStrings.Spreadsheet_ColorRed, "#B91C1C"), Named(TwStrings.Spreadsheet_ColorOrange, "#C2410C"),
            Named(TwStrings.Spreadsheet_ColorYellow, "#A16207"), Named(TwStrings.Spreadsheet_ColorGreen, "#15803D"),
            Named(TwStrings.Spreadsheet_ColorTeal, "#0F766E"), Named(TwStrings.Spreadsheet_ColorBlue, "#1D4ED8"),
            Named(TwStrings.Spreadsheet_ColorPurple, "#7E22CE"), Named(TwStrings.Spreadsheet_ColorPink, "#BE185D"),
            Named(TwStrings.Spreadsheet_ColorGray, "#4B5563"), Named(TwStrings.Spreadsheet_ColorBlack, "#000000"),
        ];

        fillColors = FillColors ??
        [
            Named(TwStrings.Spreadsheet_ColorRed, "#FEE2E2"), Named(TwStrings.Spreadsheet_ColorOrange, "#FFEDD5"),
            Named(TwStrings.Spreadsheet_ColorYellow, "#FEF9C3"), Named(TwStrings.Spreadsheet_ColorGreen, "#DCFCE7"),
            Named(TwStrings.Spreadsheet_ColorTeal, "#CCFBF1"), Named(TwStrings.Spreadsheet_ColorBlue, "#DBEAFE"),
            Named(TwStrings.Spreadsheet_ColorPurple, "#F3E8FF"), Named(TwStrings.Spreadsheet_ColorPink, "#FCE7F3"),
            Named(TwStrings.Spreadsheet_ColorGray, "#E5E7EB"),
        ];

        SpreadsheetColor Named(string key, string value) => new() { Name = localizer[key], Value = value };
    }

    #region Fonts and colours

    private Task SetFontAsync(FontOption? option) => FormatSelectionAsync(c => c.FontFamily = option?.Family);

    private async Task SetFontSizeAsync(FontSizeOption? option)
    {
        await FormatSelectionAsync(c => c.FontSize = option?.Size);
        if (canEdit && option?.Size is { } size)
        {
            FitRowsToFont(size);
        }
    }

    private Task SetTextColorAsync(string? color) => FormatSelectionAsync(c => c.Color = color);

    private Task SetFillColorAsync(string? color) => FormatSelectionAsync(c => c.Background = color);

    private async Task FormatSelectionAsync(Action<SheetCell> change)
    {
        if (canEdit)
        {
            await ApplyAsync(activeSheet, [.. selection.Cells().Select(cell => Change(activeSheet, cell, change))]);
        }
    }

    /// <summary>
    /// Picks black or white text for a fill, whichever has the higher contrast, so a filled cell stays readable in a
    /// light or a dark theme.
    /// </summary>
    /// <param name="background">A hex colour.</param>
    /// <returns>A hex colour for the text.</returns>
    internal static string ReadableTextOn(string background)
    {
        var hex = background[1..];
        if (hex.Length == 3)
        {
            hex = string.Concat(hex.Select(c => $"{c}{c}"));
        }

        static double Channel(string part)
        {
            var value = int.Parse(part, NumberStyles.HexNumber, CultureInfo.InvariantCulture) / 255d;
            return value <= 0.03928 ? value / 12.92 : Math.Pow((value + 0.055) / 1.055, 2.4);
        }

        var luminance = (0.2126 * Channel(hex[..2])) + (0.7152 * Channel(hex[2..4])) + (0.0722 * Channel(hex[4..6]));

        // Contrast with black is (L + 0.05) / 0.05, and with white 1.05 / (L + 0.05); they are equal at about 0.179.
        return luminance > 0.179 ? "#111827" : "#F9FAFB";
    }

    #endregion

    #region Rows and columns

    /// <summary>
    /// Inserts empty rows on the sheet being shown. Cells below move down, and formulas on every sheet follow them.
    /// </summary>
    /// <param name="row">The zero based row the new rows go before.</param>
    /// <param name="count">How many rows to insert.</param>
    /// <returns>A task that completes once the rows are in.</returns>
    public Task InsertRowsAsync(int row, int count = 1) => ChangeShapeAsync(rows: true, row, Math.Max(0, count));

    /// <summary>
    /// Deletes rows on the sheet being shown. Cells below move up, and formulas that referred to a deleted cell show
    /// <c>#REF!</c>.
    /// </summary>
    /// <param name="row">The zero based first row to delete.</param>
    /// <param name="count">How many rows to delete.</param>
    /// <returns>A task that completes once the rows are gone.</returns>
    public Task DeleteRowsAsync(int row, int count = 1) => ChangeShapeAsync(rows: true, row, -Math.Max(0, count));

    /// <summary>
    /// Inserts empty columns on the sheet being shown. Cells after them move along, and formulas on every sheet
    /// follow them.
    /// </summary>
    /// <param name="column">The zero based column the new columns go before.</param>
    /// <param name="count">How many columns to insert.</param>
    /// <returns>A task that completes once the columns are in.</returns>
    public Task InsertColumnsAsync(int column, int count = 1) => ChangeShapeAsync(rows: false, column, Math.Max(0, count));

    /// <summary>
    /// Deletes columns on the sheet being shown. Cells after them move back, and formulas that referred to a deleted
    /// cell show <c>#REF!</c>.
    /// </summary>
    /// <param name="column">The zero based first column to delete.</param>
    /// <param name="count">How many columns to delete.</param>
    /// <returns>A task that completes once the columns are gone.</returns>
    public Task DeleteColumnsAsync(int column, int count = 1) => ChangeShapeAsync(rows: false, column, -Math.Max(0, count));

    private Task InsertRowAboveAsync() => InsertRowsAsync(selection.Start.Row, selection.RowCount);

    private Task InsertRowBelowAsync() => InsertRowsAsync(selection.End.Row + 1, selection.RowCount);

    private Task InsertColumnBeforeAsync() => InsertColumnsAsync(selection.Start.Column, selection.ColumnCount);

    private Task InsertColumnAfterAsync() => InsertColumnsAsync(selection.End.Column + 1, selection.ColumnCount);

    private Task DeleteSelectedRowsAsync() => DeleteRowsAsync(selection.Start.Row, selection.RowCount);

    private Task DeleteSelectedColumnsAsync() => DeleteColumnsAsync(selection.Start.Column, selection.ColumnCount);

    private async Task ChangeShapeAsync(bool rows, int index, int count)
    {
        var limit = rows ? CellAddress.MaxRows : CellAddress.MaxColumns;
        if (!canEdit || count == 0 || index < 0 || index >= limit)
        {
            return;
        }

        await CommitEditAsync();
        PushSnapshot();

        var name = sheet.Name;
        var moved = new Dictionary<string, SheetCell>(StringComparer.OrdinalIgnoreCase);
        foreach (var (key, cell) in sheet.Cells)
        {
            if (!CellAddress.TryParse(key, out var address))
            {
                moved[key] = cell;
                continue;
            }

            var position = rows ? address.Row : address.Column;
            var target = MovedPosition(position, index, count);
            if (target is not { } p || p >= limit)
            {
                continue;
            }

            moved[(rows ? new CellAddress(p, address.Column) : new CellAddress(address.Row, p)).ToString()] = cell;
        }

        sheet.Cells = moved;
        for (var i = 0; i < Sheets.Count; i++)
        {
            foreach (var cell in Sheets[i].Cells.Values.Where(c => SheetCalculator.IsFormula(c?.Value)))
            {
                cell.Value = SheetFormatting.AdjustForStructure(cell.Value, i == activeSheet, name, rows, index, count);
            }
        }

        if (rows)
        {
            sheet.FrozenRows = FrozenAfter(sheet.FrozenRows, index, count);
            sheet.RowHeights = sheet.RowHeights
                .Select(p => (Row: MovedPosition(p.Key - 1, index, count), Height: p.Value))
                .Where(p => p.Row is { } r && r < CellAddress.MaxRows)
                .ToDictionary(p => p.Row!.Value + 1, p => p.Height);
        }
        else
        {
            sheet.FrozenColumns = FrozenAfter(sheet.FrozenColumns, index, count);
            sheet.ColumnWidths = MoveWidths(sheet.ColumnWidths, index, count);
        }

        MoveTables(rows, index, count);
        Recalculate();
        StateHasChanged();
        await OnCellsChange.InvokeAsync(new SheetChangeEventArgs { Sheet = sheet, Addresses = [.. sheet.Cells.Keys] });
    }

    private static int? MovedPosition(int position, int index, int count)
    {
        if (position < index)
        {
            return position;
        }

        if (count > 0)
        {
            return position + count;
        }

        return position >= index - count ? position + count : null;
    }

    private static int FrozenAfter(int frozen, int index, int count)
    {
        if (index >= frozen)
        {
            return frozen;
        }

        return count > 0 ? frozen + count : Math.Max(index, frozen + count);
    }

    private static Dictionary<string, int> MoveWidths(Dictionary<string, int> widths, int index, int count)
    {
        var moved = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        foreach (var (letters, width) in widths)
        {
            if (CellAddress.TryParse($"{letters}1", out var address) && MovedPosition(address.Column, index, count) is { } column && column < CellAddress.MaxColumns)
            {
                moved[CellAddress.ColumnName(column)] = width;
            }
        }

        return moved;
    }

    #endregion

    #region Sorting

    /// <summary>
    /// Sorts the rows of the selection by the active cell's column. With a single cell selected, the block of filled
    /// cells from <c>A1</c> is sorted, leaving out a first row of headings. Empty cells go last either way, and
    /// formulas move with their rows.
    /// </summary>
    /// <param name="descending">Whether to sort largest first.</param>
    /// <returns>A task that completes once the rows are sorted.</returns>
    public async Task SortAsync(bool descending = false)
    {
        if (!canEdit)
        {
            return;
        }

        await CommitEditAsync();
        var range = SortRange();
        if (range.RowCount < 2)
        {
            return;
        }

        await SortRowsAsync(range, anchor.Column, descending);
    }

    /// <summary>
    /// Sorts the rows of a range by one column, empty cells last, as one change that can be undone.
    /// </summary>
    private async Task SortRowsAsync(CellRange range, int key, bool descending)
    {
        var order = Enumerable.Range(range.Start.Row, range.RowCount)
            .Select(row => (Row: row, Value: calculator!.GetValue(activeSheet, new CellAddress(row, key))))
            .ToList();

        var sorted = order
            .OrderBy(r => r.Value.Kind == SheetValueKind.Empty ? 1 : 0)
            .ThenBy(r => r.Value, Comparer<SheetValue>.Create((a, b) =>
                a.Kind == SheetValueKind.Empty || b.Kind == SheetValueKind.Empty ? 0 : (descending ? -1 : 1) * FormulaEvaluator.CompareValues(a, b)))
            .ToList();

        var snapshot = range.Cells().ToDictionary(a => a, a => Clone(CellAt(activeSheet, a)));
        var changes = new List<CellChange>();
        for (var i = 0; i < sorted.Count; i++)
        {
            var target = range.Start.Row + i;
            var source = sorted[i].Row;
            for (var column = range.Start.Column; column <= range.End.Column; column++)
            {
                var from = snapshot[new CellAddress(source, column)];
                var value = SheetFormatting.ShiftFormula(from?.Value, target - source, 0);
                changes.Add(Change(activeSheet, new CellAddress(target, column), c => CopyInto(c, from, value)));
            }
        }

        await ApplyAsync(activeSheet, changes);
    }

    private CellRange SortRange()
    {
        if (!selection.IsSingleCell)
        {
            return selection;
        }

        if (TableAt(anchor) is { } found)
        {
            return new CellRange(found.Range.Start.Offset(1, 0), found.Range.End);
        }

        var last = LastUsedCell();
        var block = new CellRange(new CellAddress(0, 0), last);
        var firstRowIsHeadings = block.RowCount > 1
            && Enumerable.Range(0, block.ColumnCount).All(c => calculator!.GetValue(activeSheet, new CellAddress(0, c)).Kind is SheetValueKind.Text or SheetValueKind.Empty)
            && Enumerable.Range(1, block.RowCount - 1).Any(r => calculator!.GetValue(activeSheet, new CellAddress(r, anchor.Column)).Kind is not (SheetValueKind.Text or SheetValueKind.Empty));
        return firstRowIsHeadings ? new CellRange(new CellAddress(1, 0), last) : block;
    }

    #endregion

    #region Context menu

    private Task OnContextCopyAsync() => CopyAsync(cut: false);

    private Task OnContextCutAsync() => CopyAsync(cut: true);

    private async Task OnContextPasteAsync()
    {
        try
        {
            var text = await jsRuntime.InvokeAsync<string?>("twSpreadsheet.readText");
            await PasteTextAsync(text);
        }
        catch (JSException)
        {
            // The browser refused to read the clipboard; Ctrl + V still works.
        }
    }

    private Task OnContextClearAsync() => ClearAsync();

    /// <summary>
    /// Selects the cell under the pointer before the context menu opens, unless it is already in the selection,
    /// called by <c>twSpreadsheet.attach</c>.
    /// </summary>
    /// <param name="row">The zero based row, or -1 on a column heading.</param>
    /// <param name="column">The zero based column, or -1 on a row heading.</param>
    /// <returns>A task that completes once the selection has moved.</returns>
    [JSInvokable("PointerContext")]
    public async Task PointerContextAsync(int row, int column)
    {
        if (row >= 0 && column >= 0 && selection.Contains(new CellAddress(row, column)))
        {
            return;
        }

        if (row < 0 && column >= 0 && column >= selection.Start.Column && column <= selection.End.Column && selection.RowCount == Rows)
        {
            return;
        }

        if (column < 0 && row >= 0 && row >= selection.Start.Row && row <= selection.End.Row && selection.ColumnCount == Columns)
        {
            return;
        }

        await PointerDownAsync(row, column, shift: false);
    }

    #endregion

    #region History snapshots

    private List<SheetSnapshot> Snapshot() =>
    [
        .. Sheets.Select(s => new SheetSnapshot(
            s.Cells.ToDictionary(p => p.Key, p => Clone(p.Value)!, StringComparer.OrdinalIgnoreCase),
            new Dictionary<string, int>(s.ColumnWidths, StringComparer.OrdinalIgnoreCase),
            s.FrozenRows,
            s.FrozenColumns,
            new Dictionary<int, int>(s.RowHeights),
            [.. s.Tables.Select(CloneTable)])),
    ];

    private void PushSnapshot()
    {
        _undoStack.Add(new HistoryEntry(null, Snapshot()));
        if (_undoStack.Count > maxUndo)
        {
            _undoStack.RemoveAt(0);
        }

        _redoStack.Clear();
    }

    private void Restore(List<SheetSnapshot> snapshot)
    {
        for (var i = 0; i < snapshot.Count && i < Sheets.Count; i++)
        {
            Sheets[i].Cells = snapshot[i].Cells.ToDictionary(p => p.Key, p => Clone(p.Value)!, StringComparer.OrdinalIgnoreCase);
            Sheets[i].ColumnWidths = new Dictionary<string, int>(snapshot[i].Widths, StringComparer.OrdinalIgnoreCase);
            Sheets[i].FrozenRows = snapshot[i].FrozenRows;
            Sheets[i].FrozenColumns = snapshot[i].FrozenColumns;
            Sheets[i].RowHeights = new Dictionary<int, int>(snapshot[i].Heights);
            Sheets[i].Tables = [.. snapshot[i].Tables.Select(CloneTable)];
        }
    }

    #endregion
}
