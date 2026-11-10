// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using TwBlazor.Enums;
using TwBlazor.Localization;
using TwBlazor.Models;
using TwBlazor.Spreadsheet;

namespace TwBlazor.Components;

/// <summary>
/// Moving around the grid: the keyboard, and the pointer through <c>twSpreadsheet.attach</c>.
/// </summary>
public partial class TwSpreadsheet
{
    /// <summary>
    /// What a pointer press started on, which decides what dragging selects.
    /// </summary>
    private enum PointerTarget
    {
        Cell,
        Column,
        Row,
    }

    private PointerTarget pointerTarget;

    private CellAddress pointDragStart;

    /// <summary>
    /// Handles a key pressed in the grid or in a cell being edited. Printable keys pressed in the grid are not handled
    /// here: <c>twSpreadsheet.attach</c> collects them and calls <see cref="BeginTypingAsync"/>.
    /// </summary>
    private async Task OnGridKeyDownAsync(KeyboardEventArgs e)
    {
        if (editing && editSource == EditSource.Cell)
        {
            await OnEditorKeyDownAsync(e);
            return;
        }

        if (editing)
        {
            return;
        }

        var ctrl = e.CtrlKey || e.MetaKey;
        if (ctrl && await OnShortcutAsync(e))
        {
            return;
        }

        await OnNavigationKeyAsync(e, ctrl);
    }

    private async Task<bool> OnShortcutAsync(KeyboardEventArgs e)
    {
        switch (e.Key.ToUpperInvariant())
        {
            case "A":
                await SetSelectionAsync(new CellAddress(0, 0), new CellAddress(Rows - 1, Columns - 1), scroll: false);
                return true;
            case "Z" when e.ShiftKey:
            case "Y":
                await RedoAsync();
                return true;
            case "Z":
                await UndoAsync();
                return true;
            case "B":
                await ToggleFormatAsync(boldFormat);
                return true;
            case "I":
                await ToggleFormatAsync(italicFormat);
                return true;
            case "U":
                await ToggleFormatAsync(underlineFormat);
                return true;
            case "C":
                await CopyAsync(cut: false);
                return true;
            case "X":
                await CopyAsync(cut: true);
                return true;
            case " ":
                await SetSelectionAsync(new CellAddress(0, anchor.Column), new CellAddress(Rows - 1, anchor.Column), scroll: false, active: anchor);
                return true;
            default:
                return false;
        }
    }

    private async Task OnNavigationKeyAsync(KeyboardEventArgs e, bool ctrl)
    {
        switch (ToLogicalKey(e.Key))
        {
            case "ArrowDown" when e.AltKey:
                OpenFilterAt(anchor);
                break;
            case "ArrowUp":
                await MoveAsync(-1, 0, e.ShiftKey, ctrl);
                break;
            case "ArrowDown":
                await MoveAsync(1, 0, e.ShiftKey, ctrl);
                break;
            case "ArrowLeft":
                await MoveAsync(0, -1, e.ShiftKey, ctrl);
                break;
            case "ArrowRight":
                await MoveAsync(0, 1, e.ShiftKey, ctrl);
                break;
            case "PageUp":
                await MoveAsync(-pageRows, 0, e.ShiftKey, jump: false);
                break;
            case "PageDown":
                await MoveAsync(pageRows, 0, e.ShiftKey, jump: false);
                break;
            case "Home":
                await MoveToAsync(ctrl ? new CellAddress(0, 0) : new CellAddress(extent.Row, 0), e.ShiftKey);
                break;
            case "End":
                await MoveToAsync(ctrl ? LastUsedCell() : new CellAddress(extent.Row, LastUsedColumn(extent.Row)), e.ShiftKey);
                break;
            case "Enter" when e.ShiftKey:
                await MoveAsync(-1, 0, extend: false, jump: false);
                break;
            case "Enter":
            case "F2":
                BeginEdit();
                break;
            case "Delete":
            case "Backspace":
                await ClearAsync();
                break;
            case " " when e.ShiftKey:
                await SetSelectionAsync(new CellAddress(anchor.Row, 0), new CellAddress(anchor.Row, Columns - 1), scroll: false, active: anchor);
                break;
            case "Escape" when !selection.IsSingleCell:
                await SetSelectionAsync(anchor, anchor);
                break;
            case "Escape" when FullScreen:
                await SetFullScreenAsync(false);
                break;
        }
    }

    private async Task MoveAsync(int rows, int columns, bool extend, bool jump)
    {
        var from = extend ? extent : anchor;
        var to = jump ? JumpTarget(from, rows, columns) : Clamp(from.Offset(rows, columns));
        to = SkipHidden(to, Math.Sign(rows));
        await MoveToAsync(to, extend);
    }

    /// <summary>
    /// Moves past rows a table filter hides, in the direction of the move, or back the other way at the edge.
    /// </summary>
    private CellAddress SkipHidden(CellAddress address, int direction)
    {
        if (!IsHidden(address.Row))
        {
            return address;
        }

        foreach (var step in direction >= 0 ? new[] { 1, -1 } : [-1, 1])
        {
            for (var row = address.Row + step; row >= 0 && row < Rows; row += step)
            {
                if (!IsHidden(row))
                {
                    return new CellAddress(row, address.Column);
                }
            }
        }

        return address;
    }

    private Task MoveToAsync(CellAddress to, bool extend) =>
        extend ? SetSelectionAsync(rangeStart, to, active: anchor) : SetSelectionAsync(to, to);

    /// <summary>
    /// Finds where <kbd>Ctrl</kbd> with an arrow key goes, the same as a spreadsheet: to the end of the block of filled
    /// cells the move starts in, or else to the next filled cell, or else to the edge of the grid.
    /// </summary>
    internal CellAddress JumpTarget(CellAddress from, int rows, int columns)
    {
        var step = new CellAddress(Math.Sign(rows), Math.Sign(columns));
        bool Inside(CellAddress a) => a.Row >= 0 && a.Row < Rows && a.Column >= 0 && a.Column < Columns;

        var next = from.Offset(step.Row, step.Column);
        if (!Inside(next))
        {
            return from;
        }

        var current = from;
        if (HasValue(from) && HasValue(next))
        {
            while (Inside(next) && HasValue(next))
            {
                current = next;
                next = next.Offset(step.Row, step.Column);
            }

            return current;
        }

        current = next;
        while (!HasValue(current))
        {
            next = current.Offset(step.Row, step.Column);
            if (!Inside(next))
            {
                return current;
            }

            current = next;
        }

        return current;
    }

    private bool HasValue(CellAddress address) => !string.IsNullOrEmpty(CellAt(activeSheet, address)?.Value);

    private IEnumerable<CellAddress> FilledCells() =>
        ActiveSheet.Cells
            .Where(pair => !string.IsNullOrEmpty(pair.Value?.Value))
            .Select(pair => CellAddress.TryParse(pair.Key, out var address) ? address : (CellAddress?)null)
            .Where(address => address is { } a && a.Row < Rows && a.Column < Columns)
            .Select(address => address!.Value);

    private CellAddress LastUsedCell()
    {
        var filled = FilledCells().ToList();
        return filled.Count == 0 ? new CellAddress(0, 0) : new CellAddress(filled.Max(a => a.Row), filled.Max(a => a.Column));
    }

    private int LastUsedColumn(int row)
    {
        var columns = FilledCells().Where(a => a.Row == row).Select(a => a.Column).ToList();
        return columns.Count == 0 ? 0 : columns.Max();
    }

    /// <summary>
    /// Selects a range and reports it. A range of more than one cell is announced, since the grid only reads out the
    /// active cell.
    /// </summary>
    /// <param name="start">The corner that stays put while the selection is extended.</param>
    /// <param name="end">The corner that moves while the selection is extended.</param>
    /// <param name="scroll">Whether to scroll the active cell into view.</param>
    /// <param name="active">The active cell, or <see langword="null"/> for <paramref name="start"/>. A cell outside
    /// the range is ignored.</param>
    private async Task SetSelectionAsync(CellAddress start, CellAddress end, bool scroll = true, CellAddress? active = null)
    {
        var previous = selection;
        var previousAnchor = anchor;
        rangeStart = Clamp(start);
        extent = Clamp(end);
        anchor = active is { } cell && selection.Contains(Clamp(cell)) ? Clamp(cell) : rangeStart;
        scrollToActive = scroll;

        if (previous == selection && previousAnchor == anchor)
        {
            StateHasChanged();
            return;
        }

        statusMessage = selection.IsSingleCell ? string.Empty : localizer[TwStrings.Spreadsheet_RangeSelected, selection.ToString()];
        StateHasChanged();
        await OnSelectionChange.InvokeAsync(new SheetSelectionEventArgs { Sheet = ActiveSheet, ActiveCell = ActiveCell, Range = SelectedRange });
    }

    #region Callbacks from the browser

    /// <summary>
    /// Handles a press of the primary pointer button on the grid, called by <c>twSpreadsheet.attach</c>. A row of -1 is a
    /// column heading, a column of -1 is a row heading, and both -1 is the corner, which selects every cell. While a
    /// formula is being typed, pressing a cell puts its reference into the formula instead.
    /// </summary>
    /// <param name="row">The zero based row, or -1.</param>
    /// <param name="column">The zero based column, or -1.</param>
    /// <param name="shift">Whether <kbd>Shift</kbd> was held, which extends the selection.</param>
    /// <returns>A task that completes once the selection has moved.</returns>
    [JSInvokable("PointerDown")]
    public async Task PointerDownAsync(int row, int column, bool shift)
    {
        pointerTarget = (row, column) switch
        {
            (< 0, >= 0) => PointerTarget.Column,
            (>= 0, < 0) => PointerTarget.Row,
            _ => PointerTarget.Cell,
        };

        if (row >= 0 && column >= 0 && isPointing)
        {
            pointDragStart = Clamp(new CellAddress(row, column));
            InsertReference(new CellRange(pointDragStart, pointDragStart));
            return;
        }

        await CommitEditAsync();
        if (row < 0 && column < 0)
        {
            await SetSelectionAsync(new CellAddress(0, 0), new CellAddress(Rows - 1, Columns - 1), scroll: false);
            return;
        }

        var target = PointerCell(row, column);
        var start = pointerTarget switch
        {
            PointerTarget.Column => new CellAddress(0, shift ? anchor.Column : column),
            PointerTarget.Row => new CellAddress(shift ? anchor.Row : row, 0),
            _ => shift ? anchor : target,
        };

        await SetSelectionAsync(start, target, scroll: pointerTarget == PointerTarget.Cell, active: shift ? anchor : null);
    }

    /// <summary>
    /// Extends the selection while the pointer is dragged across the grid, called by <c>twSpreadsheet.attach</c>.
    /// </summary>
    /// <param name="row">The zero based row under the pointer, or -1 over a column heading.</param>
    /// <param name="column">The zero based column under the pointer, or -1 over a row heading.</param>
    /// <returns>A task that completes once the selection has moved.</returns>
    [JSInvokable("PointerExtend")]
    public async Task PointerExtendAsync(int row, int column)
    {
        if (isPointing && pointStart is not null)
        {
            InsertReference(new CellRange(pointDragStart, Clamp(new CellAddress(Math.Max(row, 0), Math.Max(column, 0)))));
            return;
        }

        // The cell under the pointer is already in view. Scrolling the active cell back into view would pull the grid
        // away from a drag that scrolls it to reach cells further out.
        await SetSelectionAsync(rangeStart, PointerCell(row, column), scroll: false, active: anchor);
    }

    /// <summary>
    /// Starts editing a cell, called by <c>twSpreadsheet.attach</c> when it is double-clicked.
    /// </summary>
    /// <param name="row">The zero based row.</param>
    /// <param name="column">The zero based column.</param>
    /// <returns>A task that completes once the cell is being edited.</returns>
    [JSInvokable("PointerDoubleClick")]
    public async Task PointerDoubleClickAsync(int row, int column)
    {
        if (row < 0 || column < 0)
        {
            return;
        }

        await SetSelectionAsync(new CellAddress(row, column), new CellAddress(row, column));
        BeginEdit();
    }

    private CellAddress PointerCell(int row, int column) => pointerTarget switch
    {
        PointerTarget.Column => Clamp(new CellAddress(Rows - 1, column)),
        PointerTarget.Row => Clamp(new CellAddress(row, Columns - 1)),
        _ => Clamp(new CellAddress(row, column)),
    };

    #endregion

}
