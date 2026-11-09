// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using System.Text;
using TwBlazor.Enums;
using TwBlazor.Localization;
using TwBlazor.Models;
using TwBlazor.Spreadsheet;

namespace TwBlazor.Components;

/// <summary>
/// Changing cells: editing, the clipboard, undo and redo, and formatting.
/// </summary>
public partial class TwSpreadsheet
{
    private const string boldFormat = "Bold";

    private const string italicFormat = "Italic";

    private const string underlineFormat = "Underline";

    /// <summary>
    /// The most changes kept for undo.
    /// </summary>
    private const int maxUndo = 100;

    /// <summary>
    /// Where a cell is being edited.
    /// </summary>
    private enum EditSource
    {
        None,
        Cell,
        Bar,
    }

    /// <summary>
    /// How the arrow keys behave in a cell being edited: in <see cref="Enter"/> mode, started by typing, they finish
    /// the edit and move; in <see cref="Edit"/> mode, started with <kbd>Enter</kbd>, <kbd>F2</kbd> or a double-click,
    /// they move the caret. <kbd>F2</kbd> switches between the two, as in a spreadsheet.
    /// </summary>
    private enum EditMode
    {
        Enter,
        Edit,
    }

    /// <summary>
    /// A change to one cell, kept for undo and redo. <see langword="null"/> stands for an empty cell.
    /// </summary>
    private sealed record CellChange(int Sheet, string Address, SheetCell? Before, SheetCell? After);

    /// <summary>
    /// One step of undo and redo: changes to cells, or the whole workbook as it was before its shape changed.
    /// </summary>
    private sealed record HistoryEntry(List<CellChange>? Changes, List<SheetSnapshot>? Snapshot, RemovedSheet? Removed = null);

    /// <summary>
    /// A sheet that was deleted and where it was, so undo can put it back.
    /// </summary>
    private sealed record RemovedSheet(int Index, Sheet Sheet);

    /// <summary>
    /// The cells last copied, so pasting them back keeps their formulas and formatting.
    /// </summary>
    private sealed record CopiedBlock(CellRange Source, Dictionary<(int Row, int Column), SheetCell> Cells, string Text);

    private static readonly char[] _lineBreaks = ['\r', '\n'];

    private bool editing;

    private EditSource editSource;

    private EditMode editMode;

    private string editText = string.Empty;

    private int? pointStart;

    private bool focusEditor;

    private bool takeTyped;

    private string editorTarget = string.Empty;

    private CopiedBlock? copied;

    private readonly List<HistoryEntry> _undoStack = [];

    private readonly List<HistoryEntry> _redoStack = [];

    private bool editingCell => editing && editSource == EditSource.Cell;

    private bool canUndo => canEdit && _undoStack.Count > 0;

    private bool canRedo => canEdit && _redoStack.Count > 0;

    /// <summary>
    /// Whether a click on a cell puts its reference into the formula being typed: the text is a formula ending in an
    /// operator, an opening bracket or a comma, or a reference was just put in.
    /// </summary>
    private bool isPointing => editing && editText.StartsWith('=')
        && (pointStart is not null || (editText.Length > 0 && "=+-*/^(,;:&<>".Contains(editText[^1])));

    private SheetCell? activeCell => CellAt(activeSheet, anchor);

    private IEnumerable<string> activeFormats
    {
        get
        {
            var cell = activeCell;
            if (cell?.Bold == true)
            {
                yield return boldFormat;
            }

            if (cell?.Italic == true)
            {
                yield return italicFormat;
            }

            if (cell?.Underline == true)
            {
                yield return underlineFormat;
            }
        }
    }

    private SheetCellAlignment activeAlignment => activeCell?.Alignment ?? SheetCellAlignment.General;

    private FormatOption? selectedFormat => formatOptions.FirstOrDefault(o => o.Format == activeCell?.Format);

    #region Public methods

    /// <summary>
    /// Undoes the last change.
    /// </summary>
    /// <returns>A task that completes once the change has been undone.</returns>
    public Task UndoAsync() => StepHistoryAsync(_undoStack, _redoStack, useAfter: false);

    /// <summary>
    /// Redoes the last change that was undone.
    /// </summary>
    /// <returns>A task that completes once the change has been redone.</returns>
    public Task RedoAsync() => StepHistoryAsync(_redoStack, _undoStack, useAfter: true);

    #endregion

    #region Editing

    /// <summary>
    /// Starts editing the active cell, after the user typed a character into the grid. Called by
    /// <c>twSpreadsheet.attach</c>, which keeps the characters typed until the cell's field is ready and then puts them in.
    /// </summary>
    /// <returns>A task that completes once the cell's field is shown.</returns>
    [JSInvokable("BeginTyping")]
    public Task BeginTypingAsync()
    {
        if (!canEdit)
        {
            return Task.CompletedTask;
        }

        StartEdit(EditSource.Cell, EditMode.Enter, string.Empty);
        takeTyped = true;
        StateHasChanged();
        return Task.CompletedTask;
    }

    /// <summary>
    /// Starts editing the active cell with its current contents, the caret at the end.
    /// </summary>
    private void BeginEdit()
    {
        if (!canEdit)
        {
            return;
        }

        StartEdit(EditSource.Cell, EditMode.Edit, activeCell?.Value ?? string.Empty);
        StateHasChanged();
    }

    private void StartEdit(EditSource source, EditMode mode, string text)
    {
        editing = true;
        editSource = source;
        editMode = mode;
        editText = text;
        pointStart = null;
        focusEditor = source == EditSource.Cell;
        editorTarget = editorId;
    }

    private void OnEditorInput(ChangeEventArgs e)
    {
        editText = e.Value?.ToString() ?? string.Empty;
        pointStart = null;
    }

    private void OnFormulaBarFocus()
    {
        if (!editing && canEdit)
        {
            StartEdit(EditSource.Bar, EditMode.Edit, activeCell?.Value ?? string.Empty);
        }
    }

    private void OnFormulaBarInput(ChangeEventArgs e)
    {
        if (!canEdit)
        {
            return;
        }

        if (!editing)
        {
            StartEdit(EditSource.Bar, EditMode.Edit, string.Empty);
        }

        editText = e.Value?.ToString() ?? string.Empty;
        pointStart = null;
    }

    private async Task OnFormulaBarBlurAsync()
    {
        if (editSource == EditSource.Bar && !isPointing)
        {
            await CommitEditAsync();
        }
    }

    private async Task OnFormulaBarKeyDownAsync(KeyboardEventArgs e)
    {
        if (!editing || editSource != EditSource.Bar)
        {
            return;
        }

        switch (e.Key)
        {
            case "Enter":
                await CommitEditAsync();
                focusGrid = true;
                await MoveAsync(e.ShiftKey ? -1 : 1, 0, extend: false, jump: false);
                break;
            case "Tab":
                await CommitEditAsync();
                focusGrid = true;
                await MoveAsync(0, e.ShiftKey ? -1 : 1, extend: false, jump: false);
                break;
            case "Escape":
                CancelEdit();
                focusGrid = true;
                break;
        }
    }

    private async Task OnEditorKeyDownAsync(KeyboardEventArgs e)
    {
        switch (ToLogicalKey(e.Key))
        {
            case "Enter":
                await FinishEditAsync(e.ShiftKey ? -1 : 1, 0);
                break;
            case "Tab":
                await FinishEditAsync(0, e.ShiftKey ? -1 : 1);
                break;
            case "Escape":
                CancelEdit();
                focusGrid = true;
                break;
            case "F2":
                editMode = editMode == EditMode.Enter ? EditMode.Edit : EditMode.Enter;
                break;
            case "ArrowUp" when editMode == EditMode.Enter:
                await FinishEditAsync(-1, 0);
                break;
            case "ArrowDown" when editMode == EditMode.Enter:
                await FinishEditAsync(1, 0);
                break;
            case "ArrowLeft" when editMode == EditMode.Enter:
                await FinishEditAsync(0, -1);
                break;
            case "ArrowRight" when editMode == EditMode.Enter:
                await FinishEditAsync(0, 1);
                break;
        }
    }

    private async Task FinishEditAsync(int rows, int columns)
    {
        await CommitEditAsync();
        focusGrid = true;
        await MoveAsync(rows, columns, extend: false, jump: false);
    }

    private async Task OnEditorBlurAsync()
    {
        if (editSource == EditSource.Cell && !isPointing)
        {
            await CommitEditAsync();
        }
    }

    /// <summary>
    /// Puts what was typed into the active cell and stops editing. Does nothing when no cell is being edited.
    /// </summary>
    private async Task CommitEditAsync()
    {
        if (!editing)
        {
            return;
        }

        var text = editText;
        CancelEdit();

        var value = string.IsNullOrEmpty(text) ? null : text;
        if (!string.Equals(value, activeCell?.Value, StringComparison.Ordinal))
        {
            await ApplyAsync(activeSheet, [Change(activeSheet, anchor, c => c.Value = value)]);
        }
    }

    private void CancelEdit()
    {
        editing = false;
        editSource = EditSource.None;
        editText = string.Empty;
        pointStart = null;
        focusEditor = false;
        takeTyped = false;
    }

    /// <summary>
    /// Puts a reference into the formula being typed, replacing the one put in by the same press and drag.
    /// </summary>
    private void InsertReference(CellRange range)
    {
        pointStart ??= editText.Length;
        editText = editText[..pointStart.Value] + range;
        focusEditor = true;
        editorTarget = editSource == EditSource.Bar ? formulaInputId : editorId;
        takeTyped = false;
        StateHasChanged();
    }

    #endregion

    #region Clipboard

    /// <summary>
    /// Copies the selection to the clipboard as tab separated text, and keeps the cells so pasting them back keeps
    /// their formulas and formatting. Cutting copies and then clears the cells.
    /// </summary>
    private async Task CopyAsync(bool cut)
    {
        var range = selection;
        var text = new StringBuilder();
        var cells = new Dictionary<(int Row, int Column), SheetCell>();
        for (var row = range.Start.Row; row <= range.End.Row; row++)
        {
            for (var column = range.Start.Column; column <= range.End.Column; column++)
            {
                var address = new CellAddress(row, column);
                if (column > range.Start.Column)
                {
                    text.Append('\t');
                }

                text.Append(GetDisplayText(address.ToString()));
                if (CellAt(activeSheet, address) is { } cell)
                {
                    cells[(row - range.Start.Row, column - range.Start.Column)] = Clone(cell)!;
                }
            }

            if (row < range.End.Row)
            {
                text.Append('\n');
            }
        }

        copied = new CopiedBlock(range, cells, text.ToString());
        try
        {
            await jsRuntime.InvokeVoidAsync("twSpreadsheet.copyText", copied.Text);
        }
        catch (JSException)
        {
            // The browser refused the clipboard; pasting within the sheet still works from the kept cells.
        }

        statusMessage = localizer[TwStrings.Spreadsheet_Copied, range.ToString()];
        if (cut && canEdit)
        {
            await ClearAsync(announce: false);
        }

        StateHasChanged();
    }

    /// <summary>
    /// Pastes text from the clipboard at the active cell, called by <c>twSpreadsheet.attach</c>. Cells copied from this
    /// spreadsheet keep their formulas, moved to their new place, and their formatting; other text is split into cells
    /// at tabs and line breaks. A single value fills the whole selection.
    /// </summary>
    /// <param name="text">The pasted text.</param>
    /// <returns>A task that completes once the cells have changed.</returns>
    [JSInvokable("PasteText")]
    public async Task PasteTextAsync(string? text)
    {
        if (!canEdit || text is null || editing)
        {
            return;
        }

        var changes = copied is not null && SameText(copied.Text, text) ? PasteCopied(copied) : PastePlain(text);
        if (changes.Count == 0)
        {
            return;
        }

        var addresses = changes.Select(c => CellAddress.Parse(c.Address)).ToList();
        var pasted = new CellRange(
            new CellAddress(addresses.Min(a => a.Row), addresses.Min(a => a.Column)),
            new CellAddress(addresses.Max(a => a.Row), addresses.Max(a => a.Column)));

        await ApplyAsync(activeSheet, changes);
        await SetSelectionAsync(pasted.Start, pasted.End, scroll: false, active: anchor);
        statusMessage = localizer[TwStrings.Spreadsheet_Pasted, pasted.ToString()];
        StateHasChanged();
    }

    private static bool SameText(string first, string second) =>
        string.Equals(first.Replace("\r\n", "\n", StringComparison.Ordinal).TrimEnd('\n'), second.Replace("\r\n", "\n", StringComparison.Ordinal).TrimEnd('\n'), StringComparison.Ordinal);

    private List<CellChange> PasteCopied(CopiedBlock block)
    {
        var target = selection;
        var fill = block.Source.IsSingleCell && !target.IsSingleCell;
        var origins = fill ? target.Cells() : [target.Start];
        var changes = new List<CellChange>();
        foreach (var origin in origins)
        {
            for (var row = 0; row < block.Source.RowCount; row++)
            {
                for (var column = 0; column < block.Source.ColumnCount; column++)
                {
                    var to = origin.Offset(row, column);
                    if (to.Row >= Rows || to.Column >= Columns)
                    {
                        continue;
                    }

                    var source = block.Source.Start.Offset(row, column);
                    block.Cells.TryGetValue((row, column), out var cell);
                    var value = SheetFormatting.ShiftFormula(cell?.Value, to.Row - source.Row, to.Column - source.Column);
                    changes.Add(Change(activeSheet, to, c => CopyInto(c, cell, value)));
                }
            }
        }

        return changes;
    }

    private List<CellChange> PastePlain(string text)
    {
        var lines = text.Replace("\r\n", "\n", StringComparison.Ordinal).TrimEnd('\n').Split('\n');
        var grid = lines.Select(line => line.Split('\t')).ToList();
        var single = grid.Count == 1 && grid[0].Length == 1;
        var changes = new List<CellChange>();

        if (single)
        {
            var value = string.IsNullOrEmpty(grid[0][0]) ? null : grid[0][0];
            changes.AddRange(selection.Cells().Select(cell => Change(activeSheet, cell, c => c.Value = value)));
            return changes;
        }

        for (var row = 0; row < grid.Count; row++)
        {
            for (var column = 0; column < grid[row].Length; column++)
            {
                var to = anchor.Offset(row, column);
                if (to.Row >= Rows || to.Column >= Columns)
                {
                    continue;
                }

                var value = string.IsNullOrEmpty(grid[row][column]) ? null : grid[row][column].TrimEnd(_lineBreaks);
                changes.Add(Change(activeSheet, to, c => c.Value = value));
            }
        }

        return changes;
    }

    private static void CopyInto(SheetCell target, SheetCell? source, string? value)
    {
        target.Value = value;
        target.Bold = source?.Bold == true;
        target.Italic = source?.Italic == true;
        target.Underline = source?.Underline == true;
        target.Alignment = source?.Alignment ?? SheetCellAlignment.General;
        target.Format = source?.Format ?? SheetNumberFormat.General;
        target.Background = source?.Background;
        target.Color = source?.Color;
        target.FontFamily = source?.FontFamily;
        target.FontSize = source?.FontSize;
    }

    /// <summary>
    /// Empties the selected cells, keeping their formatting.
    /// </summary>
    private async Task ClearAsync(bool announce = true)
    {
        if (!canEdit)
        {
            return;
        }

        var changes = selection.Cells()
            .Where(cell => CellAt(activeSheet, cell)?.Value is not null)
            .Select(cell => Change(activeSheet, cell, c => c.Value = null))
            .ToList();

        await ApplyAsync(activeSheet, changes);
        if (announce && changes.Count > 0)
        {
            statusMessage = localizer[TwStrings.Spreadsheet_Cleared, selection.ToString()];
        }
    }

    #endregion

    #region Formatting

    private async Task ToggleFormatAsync(string format)
    {
        if (!canEdit)
        {
            return;
        }

        var on = !activeFormats.Contains(format);
        await ApplyAsync(activeSheet, [.. selection.Cells().Select(cell => Change(activeSheet, cell, c =>
        {
            switch (format)
            {
                case boldFormat:
                    c.Bold = on;
                    break;
                case italicFormat:
                    c.Italic = on;
                    break;
                default:
                    c.Underline = on;
                    break;
            }
        }))]);
    }

    private async Task OnFormatsChangedAsync(IEnumerable<string> values)
    {
        var pressed = values.ToHashSet(StringComparer.Ordinal);
        var current = activeFormats.ToHashSet(StringComparer.Ordinal);
        foreach (var format in new[] { boldFormat, italicFormat, underlineFormat }.Where(f => pressed.Contains(f) != current.Contains(f)).Take(1))
        {
            await ToggleFormatAsync(format);
        }
    }

    private async Task SetAlignmentAsync(SheetCellAlignment alignment)
    {
        if (canEdit)
        {
            await ApplyAsync(activeSheet, [.. selection.Cells().Select(cell => Change(activeSheet, cell, c => c.Alignment = alignment))]);
        }
    }

    private async Task SetNumberFormatAsync(FormatOption? option)
    {
        if (canEdit)
        {
            await ApplyAsync(activeSheet, [.. selection.Cells().Select(cell => Change(activeSheet, cell, c => c.Format = option?.Format ?? SheetNumberFormat.General))]);
        }
    }

    #endregion

    #region Changes and history

    private CellChange Change(int sheet, CellAddress address, Action<SheetCell> change)
    {
        var before = CellAt(sheet, address);
        var after = Clone(before) ?? new SheetCell();
        change(after);
        return new CellChange(sheet, address.ToString(), Clone(before), IsBlank(after) ? null : after);
    }

    /// <summary>
    /// Writes changes into the workbook, keeps them for undo and reports them. Changes that leave a cell as it was are
    /// dropped.
    /// </summary>
    private async Task ApplyAsync(int sheet, List<CellChange> changes, bool record = true)
    {
        var real = changes.Where(c => !SameCell(c.Before, c.After)).ToList();
        if (real.Count == 0)
        {
            return;
        }

        foreach (var change in real)
        {
            Write(change.Sheet, change.Address, change.After);
        }

        if (record)
        {
            _undoStack.Add(new HistoryEntry(real, null));
            if (_undoStack.Count > maxUndo)
            {
                _undoStack.RemoveAt(0);
            }

            _redoStack.Clear();
        }

        Recalculate();
        StateHasChanged();
        await OnCellsChange.InvokeAsync(new SheetChangeEventArgs { Sheet = Sheets[sheet], Addresses = [.. real.Select(c => c.Address)] });
    }

    private async Task StepHistoryAsync(List<HistoryEntry> from, List<HistoryEntry> to, bool useAfter)
    {
        if (!canEdit || from.Count == 0)
        {
            return;
        }

        await CommitEditAsync();
        var entry = from[^1];
        from.RemoveAt(from.Count - 1);

        if (entry.Removed is { } removed)
        {
            to.Add(entry);
            if (useAfter)
            {
                Sheets.Remove(removed.Sheet);
            }
            else
            {
                Sheets.Insert(Math.Min(removed.Index, Sheets.Count), removed.Sheet);
            }

            Recalculate();
            var show = useAfter ? Math.Min(removed.Index, Sheets.Count - 1) : Sheets.IndexOf(removed.Sheet);
            activeSheet = -1;
            await ActivateSheetAsync(show);
            return;
        }

        if (entry.Snapshot is { } saved)
        {
            to.Add(new HistoryEntry(null, Snapshot()));
            Restore(saved);
            Recalculate();
            anchor = rangeStart = extent = Clamp(anchor);
            StateHasChanged();
            await OnCellsChange.InvokeAsync(new SheetChangeEventArgs { Sheet = sheet, Addresses = [.. sheet.Cells.Keys] });
            return;
        }

        to.Add(entry);
        var changes = entry.Changes!;

        var sheetIndex = changes[0].Sheet;
        if (sheetIndex >= Sheets.Count)
        {
            return;
        }

        if (sheetIndex != activeSheet)
        {
            await ActivateSheetAsync(sheetIndex);
        }

        foreach (var change in changes)
        {
            Write(change.Sheet, change.Address, Clone(useAfter ? change.After : change.Before));
        }

        Recalculate();
        var addresses = changes.Select(c => CellAddress.Parse(c.Address)).ToList();
        await SetSelectionAsync(
            new CellAddress(addresses.Min(a => a.Row), addresses.Min(a => a.Column)),
            new CellAddress(addresses.Max(a => a.Row), addresses.Max(a => a.Column)));
        await OnCellsChange.InvokeAsync(new SheetChangeEventArgs { Sheet = Sheets[sheetIndex], Addresses = [.. changes.Select(c => c.Address)] });
    }

    private void Write(int sheet, string address, SheetCell? cell)
    {
        var cells = Sheets[sheet].Cells;

        // A dictionary built without a comparer may hold the address in another case; it is replaced, not added twice.
        var existing = cells.Keys.FirstOrDefault(k => string.Equals(k, address, StringComparison.OrdinalIgnoreCase));
        if (existing is not null && !string.Equals(existing, address, StringComparison.Ordinal))
        {
            cells.Remove(existing);
        }

        if (cell is null)
        {
            cells.Remove(address);
        }
        else
        {
            cells[address] = Clone(cell)!;
        }
    }

    private static SheetCell? Clone(SheetCell? cell) => cell is null ? null : new SheetCell
    {
        Value = cell.Value,
        Bold = cell.Bold,
        Italic = cell.Italic,
        Underline = cell.Underline,
        Alignment = cell.Alignment,
        Format = cell.Format,
        Background = cell.Background,
        Color = cell.Color,
        FontFamily = cell.FontFamily,
        FontSize = cell.FontSize,
    };

    private static bool IsBlank(SheetCell cell) =>
        string.IsNullOrEmpty(cell.Value) && !cell.Bold && !cell.Italic && !cell.Underline
        && cell.Alignment == SheetCellAlignment.General && cell.Format == SheetNumberFormat.General
        && string.IsNullOrEmpty(cell.Background) && string.IsNullOrEmpty(cell.Color) && string.IsNullOrEmpty(cell.FontFamily) && cell.FontSize is null;

    private static bool SameCell(SheetCell? first, SheetCell? second)
    {
        if (first is null || second is null)
        {
            return (first is null || IsBlank(first)) && (second is null || IsBlank(second));
        }

        return string.Equals(first.Value ?? string.Empty, second.Value ?? string.Empty, StringComparison.Ordinal)
            && first.Bold == second.Bold && first.Italic == second.Italic && first.Underline == second.Underline
            && first.Alignment == second.Alignment && first.Format == second.Format
            && string.Equals(first.Background, second.Background, StringComparison.OrdinalIgnoreCase)
            && string.Equals(first.Color, second.Color, StringComparison.OrdinalIgnoreCase)
            && string.Equals(first.FontFamily, second.FontFamily, StringComparison.Ordinal)
            && SheetFormatting.SameFontSize(first.FontSize, second.FontSize);
    }

    #endregion
}
