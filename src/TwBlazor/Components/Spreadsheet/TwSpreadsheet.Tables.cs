// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using TwBlazor.Enums;
using TwBlazor.Filtering;
using TwBlazor.Localization;
using TwBlazor.Models;
using TwBlazor.Spreadsheet;
using TwBlazor.Extensions;

namespace TwBlazor.Components;

/// <summary>
/// Tables and their filters, the sheet menu, and the dialog both of them open.
/// </summary>
public partial class TwSpreadsheet
{
    /// <summary>
    /// What the dialog is showing.
    /// </summary>
    private enum DialogKind
    {
        None,
        DeleteSheet,
        InsertTable,
        Filter,
    }

    private DialogKind dialog;

    private ElementReference dialogRef;

    private bool dialogOpened;

    private string? returnFocusId;

    private int deleteSheetIndex;

    private string tableRangeText = string.Empty;

    private bool tableHasHeaders = true;

    private bool tableRangeInvalid;

    private SheetTable? filterTable;

    private int filterColumn;

    private List<(string Value, string Label)> filterValues = [];

    private HashSet<string> filterDraft = new(StringComparer.Ordinal);

    private ColumnFilter filterConditionDraft = new();

    private string filterSearch = string.Empty;

    private string filterSearchId => $"{Id}-filter-search";

    /// <summary>
    /// The values the filter list shows: every value, or those whose text contains what was typed in the search box.
    /// </summary>
    private IEnumerable<(string Value, string Label)> visibleFilterValues => string.IsNullOrWhiteSpace(filterSearch)
        ? filterValues
        : filterValues.Where(v => v.Label.Contains(filterSearch.Trim(), StringComparison.CurrentCultureIgnoreCase));

    private FilterType filterType = FilterType.Text;

    /// <summary>
    /// Gets or sets the comparisons the table filters offer, in order, or <see langword="null"/> (the default) for the
    /// ones that suit each column: text comparisons such as "contains", or number comparisons such as "is greater
    /// than" for columns of numbers.
    /// </summary>
    [Parameter] public IReadOnlyList<FilterOperator>? FilterOperators { get; set; }

    /// <summary>
    /// Gets or sets whether the table filters offer conditions, such as "is greater than 100". Defaults to
    /// <see langword="true"/>.
    /// </summary>
    [Parameter] public bool ShowFilterConditions { get; set; } = true;

    /// <summary>
    /// Gets or sets whether the table filters list the column's values with a checkbox for each. Defaults to
    /// <see langword="true"/>.
    /// </summary>
    [Parameter] public bool ShowFilterValues { get; set; } = true;

    private string dialogId => $"{Id}-dialog";

    private string dialogTitleId => $"{Id}-dialog-title";

    private string dialogTextId => $"{Id}-dialog-text";

    private string tableRangeId => $"{Id}-table-range";

    private string tableRangeErrorId => $"{Id}-table-range-error";

    #region Public methods

    /// <summary>
    /// Turns a range of the sheet being shown into a table, with a heading row, banded rows and a filter on each
    /// column. Without headings, a row is inserted above the range for them, named <c>Column1</c>, <c>Column2</c> and
    /// so on.
    /// </summary>
    /// <param name="range">The range, such as <c>A1:D20</c>.</param>
    /// <param name="hasHeaders">Whether the first row of the range holds the column headings.</param>
    /// <returns>The new table, or <see langword="null"/> when the range is not valid or overlaps another table.</returns>
    public async Task<SheetTable?> AddTableAsync(string range, bool hasHeaders = true)
    {
        if (!canEdit || ParseRange(range) is not { } cells || sheet.Tables.Any(t => ParseRange(t.Range) is { } other && Overlaps(other, cells)))
        {
            return null;
        }

        await CommitEditAsync();
        if (hasHeaders)
        {
            PushSnapshot();
        }
        else
        {
            await ChangeShapeAsync(rows: true, cells.Start.Row, 1);
            for (var column = cells.Start.Column; column <= cells.End.Column; column++)
            {
                var address = new CellAddress(cells.Start.Row, column).ToString();
                sheet.Cells[address] = new SheetCell { Value = localizer[TwStrings.Spreadsheet_DefaultColumnName, column - cells.Start.Column + 1] };
            }

            cells = new CellRange(cells.Start, cells.End.Offset(1, 0));
        }

        var table = new SheetTable { Name = UniqueTableName(), Range = cells.ToString() };
        sheet.Tables.Add(table);
        Recalculate();
        statusMessage = localizer[TwStrings.Spreadsheet_TableCreated, table.Name];
        StateHasChanged();
        await OnCellsChange.InvokeAsync(new SheetChangeEventArgs { Sheet = sheet, Addresses = [.. cells.Cells().Select(c => c.ToString())] });
        return table;
    }

    /// <summary>
    /// Turns a table back into plain cells, keeping their contents, and shows every row its filters hid.
    /// </summary>
    /// <param name="table">The table, on the sheet being shown.</param>
    /// <returns><see langword="true"/> when the table was removed.</returns>
    public bool RemoveTable(SheetTable table)
    {
        if (!canEdit || !sheet.Tables.Contains(table))
        {
            return false;
        }

        PushSnapshot();
        sheet.Tables.Remove(table);
        Recalculate();
        StateHasChanged();
        return true;
    }

    #endregion

    #region Table layout

    /// <summary>
    /// Reads a range such as <c>A1:D20</c>, or a single cell.
    /// </summary>
    internal static CellRange? ParseRange(string? text)
    {
        var parts = (text ?? string.Empty).Split(':', StringSplitOptions.TrimEntries);
        if (parts.Length is < 1 or > 2 || !CellAddress.TryParse(parts[0], out var first))
        {
            return null;
        }

        var second = first;
        return parts.Length == 2 && !CellAddress.TryParse(parts[1], out second) ? null : new CellRange(first, second);
    }

    private static bool Overlaps(CellRange a, CellRange b) =>
        a.Start.Row <= b.End.Row && b.Start.Row <= a.End.Row && a.Start.Column <= b.End.Column && b.Start.Column <= a.End.Column;

    private (SheetTable Table, CellRange Range)? TableAt(CellAddress address)
    {
        foreach (var table in sheet.Tables)
        {
            if (ParseRange(table.Range) is { } range && range.Contains(address))
            {
                return (table, range);
            }
        }

        return null;
    }

    /// <summary>
    /// What a cell is in a table: its heading row, a shaded band, or neither.
    /// </summary>
    private (bool Header, bool Band) TableRole(CellAddress address)
    {
        if (TableAt(address) is not { } found)
        {
            return (false, false);
        }

        if (address.Row == found.Range.Start.Row)
        {
            return (true, false);
        }

        return (false, found.Table.Banded && (address.Row - found.Range.Start.Row) % 2 == 0);
    }

    private string UniqueTableName()
    {
        var names = Sheets.SelectMany(s => s.Tables).Select(t => t.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var n = 1;
        string name;
        do
        {
            name = localizer[TwStrings.Spreadsheet_DefaultTableName, n];
            n++;
        }
        while (names.Contains(name));

        return name;
    }

    /// <summary>
    /// Gets the rows a sheet's table filters hide: the body rows whose value in a filtered column is not one of the
    /// values the filter keeps.
    /// </summary>
    private HashSet<int> HiddenRowsOf(int sheetIndex)
    {
        var hidden = new HashSet<int>();
        if (sheetIndex < 0 || sheetIndex >= Sheets.Count)
        {
            return hidden;
        }

        foreach (var table in Sheets[sheetIndex].Tables.Where(t => t.Filters.Count > 0 || t.Conditions.Count > 0))
        {
            if (ParseRange(table.Range) is { } range)
            {
                HideRowsFailingConditions(hidden, sheetIndex, table, range);
                HideRowsWithoutKeptValues(hidden, sheetIndex, table, range);
            }
        }

        return hidden;
    }

    /// <summary>
    /// Adds the body rows of a table that fail one of its condition filters.
    /// </summary>
    private void HideRowsFailingConditions(HashSet<int> hidden, int sheetIndex, SheetTable table, CellRange range)
    {
        foreach (var (letters, condition) in table.Conditions.Where(c => c.Value?.IsActive == true))
        {
            if (TableColumn(letters, range) is not { } column)
            {
                continue;
            }

            var type = ColumnType(sheetIndex, range, column);
            HideBodyRows(hidden, range, row => ColumnFilterEvaluator.Matches(FilterValue(sheetIndex, new CellAddress(row, column)), condition, type));
        }
    }

    /// <summary>
    /// Adds the body rows of a table whose value in a column filtered by value is not one the filter keeps.
    /// </summary>
    private void HideRowsWithoutKeptValues(HashSet<int> hidden, int sheetIndex, SheetTable table, CellRange range)
    {
        foreach (var (letters, kept) in table.Filters)
        {
            if (TableColumn(letters, range) is not { } column)
            {
                continue;
            }

            var allowed = kept.ToHashSet(StringComparer.Ordinal);
            HideBodyRows(hidden, range, row => allowed.Contains(GetDisplayText(new CellAddress(row, column).ToString(), sheetIndex)));
        }
    }

    /// <summary>
    /// Gets the index of a table column from its letters, or <see langword="null"/> when it is outside the table.
    /// </summary>
    private static int? TableColumn(string letters, CellRange range) =>
        CellAddress.TryParse($"{letters}1", out var address) && address.Column >= range.Start.Column && address.Column <= range.End.Column
            ? address.Column
            : null;

    /// <summary>
    /// Adds every body row of a table, the rows under its heading, that does not pass <paramref name="keep"/>.
    /// </summary>
    private static void HideBodyRows(HashSet<int> hidden, CellRange range, Func<int, bool> keep)
    {
        for (var row = range.Start.Row + 1; row <= range.End.Row; row++)
        {
            if (!keep(row))
            {
                hidden.Add(row);
            }
        }
    }

    /// <summary>
    /// Works out what a table column holds, for its condition filter: numbers when every value is a number, true or
    /// false when every value is one, or else text.
    /// </summary>
    private FilterType ColumnType(int sheetIndex, CellRange range, int column)
    {
        var kinds = Enumerable.Range(range.Start.Row + 1, Math.Max(0, range.RowCount - 1))
            .Select(row => calculator!.GetValue(sheetIndex, new CellAddress(row, column)).Kind)
            .Where(kind => kind != SheetValueKind.Empty)
            .Distinct()
            .ToList();

        return kinds switch
        {
            [SheetValueKind.Number] => FilterType.Number,
            [SheetValueKind.Boolean] => FilterType.Boolean,
            _ => FilterType.Text,
        };
    }

    /// <summary>
    /// The value a condition filter compares: the number itself for a number, true or false, nothing for an empty
    /// cell, or else the text the cell shows.
    /// </summary>
    private object? FilterValue(int sheetIndex, CellAddress address)
    {
        var value = calculator!.GetValue(sheetIndex, address);
        return value.Kind switch
        {
            SheetValueKind.Number => value.Number,
            SheetValueKind.Boolean => value.Number != 0,
            SheetValueKind.Empty => null,
            _ => GetDisplayText(address.ToString(), sheetIndex),
        };
    }

    /// <summary>
    /// Moves tables after rows or columns were inserted or deleted, the same way formulas move; a table that loses its
    /// heading row or every column is removed. Filters keyed by column letters move with their columns.
    /// </summary>
    private void MoveTables(bool rows, int index, int count)
    {
        foreach (var table in sheet.Tables.ToList())
        {
            var headerRow = ParseRange(table.Range)?.Start.Row ?? -1;
            if (rows && count < 0 && headerRow >= index && headerRow < index - count)
            {
                sheet.Tables.Remove(table);
                continue;
            }

            var moved = SheetFormatting.AdjustForStructure($"={table.Range}", onChangedSheet: true, sheet.Name, rows, index, count);
            if (moved is null || !moved.Contains(':', StringComparison.Ordinal) || moved.Contains(SheetValue.ReferenceError, StringComparison.Ordinal))
            {
                sheet.Tables.Remove(table);
                continue;
            }

            table.Range = moved[1..];
            if (!rows)
            {
                table.Filters = MoveKeys(table.Filters, index, count);
                table.Conditions = MoveKeys(table.Conditions, index, count);
            }
        }
    }

    private static Dictionary<string, TValue> MoveKeys<TValue>(Dictionary<string, TValue> byColumn, int index, int count) =>
        byColumn
            .Select(p => (Valid: CellAddress.TryParse($"{p.Key}1", out var a), Column: MovedPosition(a.Column, index, count), p.Value))
            .Where(p => p.Valid && p.Column is not null)
            .ToDictionary(p => CellAddress.ColumnName(p.Column!.Value), p => p.Value, StringComparer.OrdinalIgnoreCase);

    #endregion

    #region Dialog

    private void OpenDialog(DialogKind kind, string? returnFocus)
    {
        dialog = kind;
        dialogOpened = false;
        returnFocusId = returnFocus;
        StateHasChanged();
    }

    /// <summary>
    /// Closes the dialog without doing anything, and puts focus back where it was. Called by the dialog's Escape
    /// handling in <c>twSpreadsheet.openDialog</c>, by Cancel and by a press on the backdrop.
    /// </summary>
    /// <returns>A task that completes once the dialog is closed.</returns>
    [JSInvokable("CloseDialog")]
    public async Task CloseDialogAsync()
    {
        if (dialog == DialogKind.None)
        {
            return;
        }

        try
        {
            await jsRuntime.InvokeVoidAsync("twSpreadsheet.closeDialog", dialogRef);
        }
        catch (JSDisconnectedException)
        {
            // The circuit is gone; there is nothing left to release.
        }

        dialog = DialogKind.None;
        filterTable = null;
        if (returnFocusId is null)
        {
            focusGrid = true;
        }
        else
        {
            focusElementId = returnFocusId;
        }

        // Also called from script (Escape in the dialog), where nothing renders for us.
#pragma warning disable BL0012
        StateHasChanged();
#pragma warning restore BL0012
    }

    private async Task OpenPendingDialogAsync()
    {
        if (dialog != DialogKind.None && !dialogOpened)
        {
            dialogOpened = true;
            await jsRuntime.TryInvokeVoidAsync("twSpreadsheet.openDialog", dialogRef, dotNetRef);
        }
    }

    #endregion

    #region Sheet menu

    private string sheetMenuId => $"{Id}-sheet-menu";

    private void RequestDeleteSheet()
    {
        if (canEdit && Sheets.Count > 1)
        {
            deleteSheetIndex = activeSheet;
            OpenDialog(DialogKind.DeleteSheet, TabId(activeSheet));
        }
    }

    private async Task ConfirmDeleteSheetAsync()
    {
        var index = deleteSheetIndex;
        var name = index < Sheets.Count ? Sheets[index].Name : string.Empty;
        returnFocusId = null;
        await CloseDialogAsync();
        if (await RemoveSheetAsync(index))
        {
            statusMessage = localizer[TwStrings.Spreadsheet_SheetDeleted, name];
            focusElementId = TabId(activeSheet);
            focusGrid = false;
        }
    }

    private void StartRenameActive() => StartRename(activeSheet);

    /// <summary>
    /// Moves the sheet being shown one place along the tabs.
    /// </summary>
    /// <param name="visualDirection">-1 for left and 1 for right, as the tabs appear; mirrored in a right to left
    /// layout.</param>
    private async Task MoveActiveSheetAsync(int visualDirection)
    {
        var target = activeSheet + (isRightToLeft ? -visualDirection : visualDirection);
        if (!canEdit || target < 0 || target >= Sheets.Count)
        {
            return;
        }

        await CommitEditAsync();
        (Sheets[activeSheet], Sheets[target]) = (Sheets[target], Sheets[activeSheet]);

        // Undo and redo point at sheets by index, which no longer line up.
        _undoStack.Clear();
        _redoStack.Clear();
        activeSheet = target;
        ActiveSheetIndex = target;
        Recalculate();
        focusElementId = TabId(target);
        StateHasChanged();
        await ActiveSheetIndexChanged.InvokeAsync(target);
    }

    private bool canMoveLeft => isRightToLeft ? activeSheet < Sheets.Count - 1 : activeSheet > 0;

    private bool canMoveRight => isRightToLeft ? activeSheet > 0 : activeSheet < Sheets.Count - 1;

    #endregion

    #region Insert table

    private Task OpenInsertTableAsync()
    {
        if (!canEdit)
        {
            return Task.CompletedTask;
        }

        tableRangeText = SelectedRange;
        tableHasHeaders = true;
        tableRangeInvalid = false;
        OpenDialog(DialogKind.InsertTable, null);
        return Task.CompletedTask;
    }

    private async Task CreateTableAsync()
    {
        var text = tableRangeText;
        var hasHeaders = tableHasHeaders;
        if (ParseRange(text) is not { } range || sheet.Tables.Any(t => ParseRange(t.Range) is { } other && Overlaps(other, range)))
        {
            tableRangeInvalid = true;
            return;
        }

        await CloseDialogAsync();
        if (await AddTableAsync(range.ToString(), hasHeaders) is { } table && ParseRange(table.Range) is { } created)
        {
            await SetSelectionAsync(created.Start, created.End, scroll: false);
            statusMessage = localizer[TwStrings.Spreadsheet_TableCreated, table.Name];
        }
    }

    private void OnTableRangeInput(ChangeEventArgs e)
    {
        tableRangeText = e.Value?.ToString() ?? string.Empty;
        tableRangeInvalid = false;
    }

    private void RemoveActiveTable()
    {
        if (TableAt(anchor) is { } found)
        {
            RemoveTable(found.Table);
        }
    }

    #endregion

    #region Filters

    private string FilterButtonLabel(SheetTable table, int column, CellRange range)
    {
        var heading = GetDisplayText(new CellAddress(range.Start.Row, column).ToString());
        return IsColumnFiltered(table, column)
            ? localizer[TwStrings.Spreadsheet_FilterColumnActive, heading]
            : localizer[TwStrings.Spreadsheet_FilterColumn, heading];
    }

    private static bool IsColumnFiltered(SheetTable table, int column)
    {
        var letters = CellAddress.ColumnName(column);
        return table.Filters.ContainsKey(letters) || (table.Conditions.TryGetValue(letters, out var condition) && condition?.IsActive == true);
    }

    private string filterHeading => filterTable is not null && ParseRange(filterTable.Range) is { } range
        ? GetDisplayText(new CellAddress(range.Start.Row, filterColumn).ToString())
        : string.Empty;

    private bool allFilterValuesChecked => filterValues.All(v => filterDraft.Contains(v.Value));

    private bool allVisibleFilterValuesChecked => visibleFilterValues.All(v => filterDraft.Contains(v.Value));

    private void OnFilterSearchInput(ChangeEventArgs e) => filterSearch = e.Value?.ToString() ?? string.Empty;

    /// <summary>
    /// Opens the filter of a table column: its sort buttons and a checkbox for each value in the column.
    /// </summary>
    private void OpenFilter(SheetTable table, int column, CellRange range)
    {
        filterTable = table;
        filterColumn = column;
        var texts = new HashSet<string>(StringComparer.Ordinal);
        var ordered = new List<(string Text, SheetValue Value)>();
        for (var row = range.Start.Row + 1; row <= range.End.Row; row++)
        {
            var address = new CellAddress(row, column);
            var text = GetDisplayText(address.ToString());
            if (texts.Add(text))
            {
                ordered.Add((text, calculator!.GetValue(activeSheet, address)));
            }
        }

        filterValues =
        [
            .. ordered
                .OrderBy(v => v.Value.Kind == SheetValueKind.Empty ? 1 : 0)
                .ThenBy(v => v.Value, Comparer<SheetValue>.Create((a, b) =>
                    a.Kind == SheetValueKind.Empty || b.Kind == SheetValueKind.Empty ? 0 : FormulaEvaluator.CompareValues(a, b)))
                .Select(v => (v.Text, v.Text.Length == 0 ? localizer[TwStrings.Spreadsheet_Blanks] : v.Text)),
        ];

        filterDraft = table.Filters.TryGetValue(CellAddress.ColumnName(column), out var kept)
            ? kept.Where(texts.Contains).ToHashSet(StringComparer.Ordinal)
            : texts;

        filterSearch = string.Empty;
        filterType = ColumnType(activeSheet, range, column);
        var operators = FilterOperators is { Count: > 0 } chosen ? chosen : ColumnFilterEvaluator.OperatorsFor(filterType);
        filterConditionDraft = table.Conditions.TryGetValue(CellAddress.ColumnName(column), out var saved) && saved is not null
            ? saved.Clone()
            : new ColumnFilter { First = new FilterCondition { Operator = operators[0] } };
        OpenDialog(DialogKind.Filter, null);
    }

    private void OpenFilterAt(CellAddress address)
    {
        if (TableAt(address) is { } found && found.Table.ShowFilterButtons && address.Row == found.Range.Start.Row)
        {
            OpenFilter(found.Table, address.Column, found.Range);
        }
    }

    private void ToggleFilterValue(string value, bool on)
    {
        if (on)
        {
            filterDraft.Add(value);
        }
        else
        {
            filterDraft.Remove(value);
        }
    }

    /// <summary>
    /// Checks or clears every value the list shows, which is all of them unless the search box narrows it.
    /// </summary>
    private void ToggleAllFilterValues(bool on)
    {
        foreach (var (value, _) in visibleFilterValues.ToList())
        {
            ToggleFilterValue(value, on);
        }
    }

    private async Task ApplyFilterAsync()
    {
        if (filterTable is { } table)
        {
            var letters = CellAddress.ColumnName(filterColumn);
            if (allFilterValuesChecked || !ShowFilterValues)
            {
                table.Filters.Remove(letters);
            }
            else
            {
                table.Filters[letters] = [.. filterValues.Select(v => v.Value).Where(filterDraft.Contains)];
            }

            var condition = filterConditionDraft.Clone();
            if (condition.Second is { IsActive: false })
            {
                condition.Second = null;
            }

            if (ShowFilterConditions && condition.IsActive)
            {
                table.Conditions[letters] = condition;
            }
            else
            {
                table.Conditions.Remove(letters);
            }

            AnnounceRowsShown(table);
        }

        await CloseDialogAsync();
    }

    private async Task ClearFilterAsync()
    {
        if (filterTable is { } table)
        {
            table.Filters.Remove(CellAddress.ColumnName(filterColumn));
            table.Conditions.Remove(CellAddress.ColumnName(filterColumn));
            AnnounceRowsShown(table);
        }

        await CloseDialogAsync();
    }

    private void AnnounceRowsShown(SheetTable table)
    {
        Recalculate();
        if (ParseRange(table.Range) is { } range)
        {
            var total = range.RowCount - 1;
            var shown = Enumerable.Range(range.Start.Row + 1, total).Count(r => !IsHidden(r));
            statusMessage = localizer[TwStrings.Spreadsheet_RowsShown, shown, total];
        }
    }

    private async Task SortFilterColumnAsync(bool descending)
    {
        if (filterTable is { } table && ParseRange(table.Range) is { } range && range.RowCount > 2)
        {
            await CloseDialogAsync();
            var body = new CellRange(range.Start.Offset(1, 0), range.End);
            await SortRowsAsync(body, filterColumn, descending);
        }
    }

    #endregion
}
