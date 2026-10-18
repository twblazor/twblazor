// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using TwBlazor.Enums;
using TwBlazor.Localization;
using TwBlazor.Utilities;

namespace TwBlazor.Components;

public partial class TwDataTable<TItem> : IAsyncDisposable
{
    private List<TwDataTableColumnState> columnStates = [];
    private List<TwDataTableColumn<TItem>> visibleColumns = [];
    private Dictionary<string, TwDataTableColumn<TItem>> columnsByName = [];
    private string? columnSignature;
    private string? pendingFocusId;
    private string? activeMenuColumn;
    private ElementReference rootRef;
    private DotNetObjectReference<TwDataTable<TItem>>? selfReference;

    /// <summary>
    /// Whether each column header has an options button that opens a <see cref="TwMenu"/> for sorting, grouping,
    /// moving and hiding the column, showing any hidden column again, and resetting the columns. Only applies when
    /// <see cref="Columns"/> is used. Defaults to <c>false</c>.
    /// </summary>
    [Parameter]
    public bool ColumnMenu { get; set; }

    /// <summary>
    /// Whether people can resize the columns by dragging the border at the end of a column's header, or by focusing
    /// that border and pressing the arrow keys (<see cref="TwTable.ResizableColumns"/>). Only applies when
    /// <see cref="Columns"/> is used. Defaults to <c>false</c>.
    /// </summary>
    [Parameter]
    public bool ResizableColumns { get; set; }

    private bool effectiveResizableColumns => ResizableColumns && hasColumns;

    private string headerCellRowClasses => new ClassBuilder(options.Theme.Display.Flex)
        .AddClass(options.Theme.Flexbox.Align.Center)
        .AddClass(options.Theme.Spacing.Gap.Sm).Build();

    private string headerActionsClasses => new ClassBuilder(options.Theme.Display.Flex)
        .AddClass(options.Theme.Flexbox.Align.Center)
        .AddClass(options.Theme.Spacing.PushEnd).Build();

    private string HeaderCellClasses(TwDataTableColumn<TItem> column) => new ClassBuilder(theme.HeaderCellPadding)
        .AddClass(column.HeaderClass ?? string.Empty).Build();

    private string ColumnElementId(TwDataTableColumn<TItem> column) => $"{Id}-col-{column.Name}";

    private string ColumnMenuId(TwDataTableColumn<TItem> column) => $"{Id}-col-{column.Name}-menu";

    private RenderFragment? effectiveColumnGroup =>
        hasColumns && (effectiveResizableColumns || visibleColumns.Any(c => !string.IsNullOrWhiteSpace(ColumnWidth(c))))
            ? builder =>
            {
                foreach (var column in visibleColumns)
                {
                    builder.OpenComponent<TwTableColumn>(0);
                    builder.SetKey(column.Name);
                    builder.AddComponentParameter(1, nameof(TwTableColumn.Id), ColumnElementId(column));
                    builder.AddComponentParameter(2, nameof(TwTableColumn.Width), ColumnWidth(column));
                    builder.AddComponentParameter(3, nameof(TwTableColumn.Label), column.Title);
                    builder.AddComponentParameter(4, nameof(TwTableColumn.Resizable), column.Resizable);
                    builder.AddComponentParameter(5, "data-column", column.Name);
                    builder.CloseComponent();
                }

                if (showActionsColumn)
                {
                    builder.OpenComponent<TwTableColumn>(6);
                    builder.SetKey("tw-actions");
                    builder.AddComponentParameter(7, nameof(TwTableColumn.Id), $"{Id}-col-actions");
                    builder.AddComponentParameter(8, nameof(TwTableColumn.Resizable), false);
                    builder.CloseComponent();
                }
            }
            : ColumnGroup;

    private string? ColumnWidth(TwDataTableColumn<TItem> column) =>
        columnStates.FirstOrDefault(s => s.Name == column.Name)?.Width ?? column.Width;

    /// <summary>
    /// Rebuilds the column state when the set of column definitions changes, keeping what a person already
    /// changed for the columns that are still there.
    /// </summary>
    private void SyncColumns()
    {
        if (!hasColumns)
        {
            columnsByName = [];
            columnStates = [];
            visibleColumns = [];
            columnSignature = null;
            return;
        }

        columnsByName = Columns!
            .GroupBy(c => c.Name)
            .ToDictionary(g => g.Key, g => g.First());

        var signature = string.Join('\u001f', Columns!.Select(c => c.Name));
        if (signature != columnSignature)
        {
            columnSignature = signature;
            columnStates = MergeColumnStates(columnStates);
        }

        RebuildVisibleColumns();
    }

    private List<TwDataTableColumnState> DefaultColumnStates() =>
        [.. columnsByName.Values.Select(c => new TwDataTableColumnState { Name = c.Name, Visible = c.Visible, Width = c.Width })];

    /// <summary>
    /// Lines saved column settings up with the column definitions: settings for columns that are gone are
    /// dropped, new columns are added at the end with their defaults, and at least one column stays visible.
    /// </summary>
    private List<TwDataTableColumnState> MergeColumnStates(IEnumerable<TwDataTableColumnState> saved)
    {
        var merged = saved
            .Where(s => s is not null && columnsByName.ContainsKey(s.Name))
            .DistinctBy(s => s.Name)
            .Select(s => s.Clone())
            .ToList();

        foreach (var column in columnsByName.Values.Where(c => merged.All(s => s.Name != c.Name)))
        {
            merged.Add(new TwDataTableColumnState { Name = column.Name, Visible = column.Visible, Width = column.Width });
        }

        foreach (var state in merged)
        {
            var column = columnsByName[state.Name];
            if (!column.Hideable)
            {
                state.Visible = true;
            }

            // A filter saved for a column that can no longer be filtered would hide rows nobody can unhide.
            if (!column.Filterable)
            {
                state.Filter = null;
            }
        }

        if (merged.Count > 0 && !merged.Any(s => s.Visible))
        {
            merged[0].Visible = true;
        }

        return merged;
    }

    private void RebuildVisibleColumns()
    {
        visibleColumns = [.. columnStates.Where(s => s.Visible).Select(s => columnsByName[s.Name])];
    }

    private bool CanToggleColumn(TwDataTableColumnState state) =>
        columnsByName[state.Name].Hideable && (!state.Visible || columnStates.Count(s => s.Visible) > 1);

    private bool CanHideColumn(TwDataTableColumn<TItem> column) =>
        columnStates.FirstOrDefault(s => s.Name == column.Name) is { } state && state.Visible && CanToggleColumn(state);

    private void OnColumnMenuOpenChanged(string columnName, bool open)
    {
        activeMenuColumn = open ? columnName : null;
    }

    private async Task SetColumnVisibleAsync(string name, bool visible)
    {
        var state = columnStates.FirstOrDefault(s => s.Name == name);
        if (state is null || state.Visible == visible || !CanToggleColumn(state))
        {
            return;
        }

        // Hiding the column whose menu is open removes the menu's button, so focus moves to a neighbour's.
        if (!visible && activeMenuColumn == name)
        {
            var index = visibleColumns.FindIndex(c => c.Name == name);
            var neighbour = visibleColumns.ElementAtOrDefault(index + 1) ?? visibleColumns.ElementAtOrDefault(index - 1);
            pendingFocusId = neighbour is null ? null : $"{ColumnMenuId(neighbour)}-trigger";
        }

        state.Visible = visible;
        RebuildVisibleColumns();

        var title = columnsByName[name].Title;
        sortStatusMessage = visible
            ? localizer[TwStrings.DataTable_ColumnShown, title]
            : localizer[TwStrings.DataTable_ColumnHidden, title];

        await NotifyStateChangedAsync();
    }

    /// <summary>
    /// Moves a column one place towards the left or the right of the screen, whichever way the text runs.
    /// </summary>
    private async Task MoveColumnAsync(string name, bool towardsLeft)
    {
        var offset = towardsLeft != isRightToLeft ? -1 : 1;
        var from = columnStates.FindIndex(s => s.Name == name);
        var to = from + offset;
        if (from < 0 || to < 0 || to >= columnStates.Count)
        {
            return;
        }

        var state = columnStates[from];
        columnStates.RemoveAt(from);
        columnStates.Insert(to, state);
        RebuildVisibleColumns();

        sortStatusMessage = localizer[TwStrings.DataTable_ColumnMoved, columnsByName[name].Title, to + 1, columnStates.Count];

        await NotifyStateChangedAsync();
    }

    /// <summary>
    /// Whether a column can move one place towards the left or the right of the screen.
    /// </summary>
    private bool CanMoveColumn(string name, bool towardsLeft)
    {
        var index = columnStates.FindIndex(s => s.Name == name);
        var earlier = towardsLeft != isRightToLeft;
        return earlier ? index > 0 : index >= 0 && index < columnStates.Count - 1;
    }

    private async Task ResetColumnsAsync()
    {
        var filtered = columnStates.Any(s => s.Filter is not null);
        columnStates = MergeColumnStates(DefaultColumnStates());
        RebuildVisibleColumns();

        if (filtered)
        {
            RefreshView(resetPage: true);
        }

        sortStatusMessage = localizer[TwStrings.DataTable_ColumnsReset];
        await NotifyStateChangedAsync();
    }

    /// <summary>
    /// Keeps the widths a person resized the columns to, so they are saved with the table's state.
    /// </summary>
    private async Task OnColumnsResizedAsync(IReadOnlyDictionary<string, string> widths)
    {
        var changed = false;
        foreach (var column in visibleColumns)
        {
            if (widths.TryGetValue(ColumnElementId(column), out var width)
                && columnStates.FirstOrDefault(s => s.Name == column.Name) is { } state
                && state.Width != width)
            {
                state.Width = width;
                changed = true;
            }
        }

        if (changed)
        {
            await NotifyStateChangedAsync();
        }
    }

    private async Task SetSortAsync(string columnName, SortDirection direction)
    {
        var column = FindColumn(columnName);
        if (column is not { IsSortable: true, PropertySelector: not null })
        {
            return;
        }

        currentSortColumn = direction == SortDirection.None ? null : columnName;
        currentSortDirection = direction;
        sortStatusMessage = direction switch
        {
            SortDirection.Ascending => localizer[TwStrings.DataTable_SortedAscending, column.Title],
            SortDirection.Descending => localizer[TwStrings.DataTable_SortedDescending, column.Title],
            _ => localizer[TwStrings.DataTable_SortRemoved, column.Title]
        };

        RefreshView(resetPage: true);
        await NotifyStateChangedAsync();
    }

    private bool IsSortedBy(string columnName) => currentSortColumn == columnName && currentSortDirection != SortDirection.None;

    /// <inheritdoc />
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);

        if (firstRender && usesStorage)
        {
            await LoadStoredStateAsync();
        }

        await RefreshProviderRowsIfRequestedAsync();
        await RegisterEditEscapeAsync();
        await SyncFilterPanelAsync();

        if (pendingFocusId is { } focusId)
        {
            pendingFocusId = null;
            await InvokeScriptAsync("twDataTable.focus", focusId);
        }
    }

    private async Task InvokeScriptAsync(string identifier, params object?[] args)
    {
        try
        {
            await jsRuntime.InvokeVoidAsync(identifier, args);
        }
        catch (JSDisconnectedException)
        {
            // The circuit disconnected before the script could run.
        }
        catch (InvalidOperationException)
        {
            // JS interop is not available (prerendering).
        }
    }

    /// <summary>
    /// Removes the listeners the table registered for an open filter or an edited row, and releases the reference
    /// the scripts held to it.
    /// </summary>
    /// <returns>A task that completes once the table is cleaned up.</returns>
    public async ValueTask DisposeAsync()
    {
        await CloseFilterScriptAsync();

        if (escapeRegistered)
        {
            await InvokeScriptAsync("twDialog.unregisterEscape", editingRowRef);
        }

        selfReference?.Dispose();
        GC.SuppressFinalize(this);
    }
}
