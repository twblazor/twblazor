// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using TwBlazor.Localization;
using TwBlazor.Models;
using TwBlazor.Spreadsheet;

namespace TwBlazor.Components;

/// <summary>
/// The sheets: switching, adding, renaming and removing them.
/// </summary>
public partial class TwSpreadsheet
{
    private int? renamingSheet;

    private string renameText = string.Empty;

    private string renameInputId => $"{Id}-rename";

    #region Public methods

    /// <summary>
    /// Adds a sheet after the last one and shows it.
    /// </summary>
    /// <param name="name">The sheet's name, or <see langword="null"/> for the next free name such as <c>Sheet2</c>. A
    /// name already used gets a number added.</param>
    /// <returns>The new sheet.</returns>
    public async Task<Sheet> AddSheetAsync(string? name = null)
    {
        await CommitEditAsync();
        var added = new Sheet { Name = UniqueName(string.IsNullOrWhiteSpace(name) ? null : name.Trim(), null) };
        Sheets.Add(added);
        Recalculate();
        await ActivateSheetAsync(Sheets.Count - 1);
        return added;
    }

    /// <summary>
    /// Removes a sheet, which can be undone. The last sheet cannot be removed. Formulas that referred to it show
    /// <c>#REF!</c> until it is back.
    /// </summary>
    /// <param name="index">The index of the sheet.</param>
    /// <returns><see langword="true"/> when the sheet was removed.</returns>
    public async Task<bool> RemoveSheetAsync(int index)
    {
        if (index < 0 || index >= Sheets.Count || Sheets.Count == 1)
        {
            return false;
        }

        CancelEdit();
        var removed = Sheets[index];
        Sheets.RemoveAt(index);
        _undoStack.Add(new HistoryEntry(null, null, new RemovedSheet(index, removed)));
        _redoStack.Clear();
        Recalculate();

        var next = activeSheet > index || activeSheet >= Sheets.Count ? Math.Max(0, activeSheet - 1) : activeSheet;
        activeSheet = -1;
        await ActivateSheetAsync(next);
        return true;
    }

    /// <summary>
    /// Renames a sheet, and changes the formulas on every sheet that refer to it by name.
    /// </summary>
    /// <param name="index">The index of the sheet.</param>
    /// <param name="name">The new name.</param>
    /// <returns><see langword="true"/> when the sheet was renamed; <see langword="false"/> when the name is blank, or
    /// already used by another sheet.</returns>
    public Task<bool> RenameSheetAsync(int index, string name)
    {
        var trimmed = name?.Trim() ?? string.Empty;
        if (index < 0 || index >= Sheets.Count || trimmed.Length == 0
            || Sheets.Where((s, i) => i != index).Any(s => string.Equals(s.Name, trimmed, StringComparison.OrdinalIgnoreCase)))
        {
            return Task.FromResult(false);
        }

        var oldName = Sheets[index].Name;
        if (!string.Equals(oldName, trimmed, StringComparison.Ordinal))
        {
            foreach (var cell in Sheets.SelectMany(s => s.Cells.Values).Where(c => SheetCalculator.IsFormula(c?.Value)))
            {
                cell.Value = SheetFormatting.RenameSheet(cell.Value, oldName, trimmed);
            }

            Sheets[index].Name = trimmed;
            Recalculate();
            StateHasChanged();
        }

        return Task.FromResult(true);
    }

    #endregion

    private async Task ActivateSheetAsync(int index)
    {
        if (index == activeSheet || index < 0 || index >= Sheets.Count)
        {
            return;
        }

        await CommitEditAsync();
        renamingSheet = null;
        activeSheet = index;
        ActiveSheetIndex = index;
        rangeStart = extent = anchor = new CellAddress(0, 0);
        statusMessage = string.Empty;
        StateHasChanged();
        await ActiveSheetIndexChanged.InvokeAsync(index);
    }

    private string UniqueName(string? wanted, int? except)
    {
        bool Taken(string name) => Sheets.Where((s, i) => i != except).Any(s => string.Equals(s.Name, name, StringComparison.OrdinalIgnoreCase));

        if (wanted is not null && !Taken(wanted))
        {
            return wanted;
        }

        var n = wanted is null ? Sheets.Count + 1 : 2;
        string candidate;
        do
        {
            candidate = wanted is null ? localizer[TwStrings.Spreadsheet_DefaultSheetName, n] : $"{wanted} ({n})";
            n++;
        }
        while (Taken(candidate));

        return candidate;
    }

    private async Task OnAddSheetClickAsync()
    {
        await AddSheetAsync();
        focusElementId = TabId(activeSheet);
    }

    private async Task OnTabKeyDownAsync(KeyboardEventArgs e, int index)
    {
        var target = ToLogicalKey(e.Key) switch
        {
            "ArrowRight" => (index + 1) % Sheets.Count,
            "ArrowLeft" => (index - 1 + Sheets.Count) % Sheets.Count,
            "Home" => 0,
            "End" => Sheets.Count - 1,
            _ => -1,
        };

        if (target >= 0)
        {
            await ActivateSheetAsync(target);
            focusElementId = TabId(target);
            return;
        }

        if (e.Key == "F2")
        {
            StartRename(index);
        }
    }

    private void StartRename(int index)
    {
        if (!canEdit)
        {
            return;
        }

        renamingSheet = index;
        renameText = Sheets[index].Name;
        focusElementId = renameInputId;
    }

    private async Task OnTabDoubleClickAsync(int index)
    {
        await ActivateSheetAsync(index);
        StartRename(index);
    }

    private void OnRenameInput(ChangeEventArgs e) => renameText = e.Value?.ToString() ?? string.Empty;

    private async Task OnRenameKeyDownAsync(KeyboardEventArgs e)
    {
        if (e.Key == "Enter")
        {
            await CommitRenameAsync();
        }
        else if (e.Key == "Escape")
        {
            var index = renamingSheet;
            renamingSheet = null;
            focusElementId = index is { } i ? TabId(i) : null;
        }
    }

    private async Task CommitRenameAsync()
    {
        if (renamingSheet is not { } index)
        {
            return;
        }

        renamingSheet = null;
        await RenameSheetAsync(index, renameText);
        focusElementId = TabId(index);
    }

    private async Task OnRenameBlurAsync()
    {
        if (renamingSheet is not null)
        {
            await CommitRenameAsync();
            focusElementId = null;
        }
    }
}
