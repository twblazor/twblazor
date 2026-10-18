// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using System.Text.Json;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using TwBlazor.Enums;

namespace TwBlazor.Components;

public partial class TwDataTable<TItem>
{
    [Inject] private IJSRuntime jsRuntime { get; set; } = null!;

    private TwDataTableState? lastAppliedState;

    /// <summary>
    /// Gets or sets the table's state: its sort, search, page, page size and columns. Bind it with
    /// <c>@bind-State</c> to keep a copy that follows every change, then save that copy wherever suits you and pass
    /// it back later to restore the table. When it is <c>null</c> the table starts from its parameters.
    /// </summary>
    [Parameter]
    public TwDataTableState? State { get; set; }

    /// <summary>
    /// Raised with a new <see cref="TwDataTableState"/> each time a person sorts, searches, changes page or page size,
    /// or changes the columns.
    /// </summary>
    [Parameter]
    public EventCallback<TwDataTableState> StateChanged { get; set; }

    /// <summary>
    /// Gets or sets a key to save the table's state under in the browser's local storage. The table then comes
    /// back the way the person left it on their next visit. Use a key that is unique to the table, such as
    /// <c>"orders-table"</c>. Ignored while <see cref="State"/> is set, so state you manage yourself always wins.
    /// </summary>
    /// <remarks>Local storage can only be read once the page is interactive, so the saved state is applied just
    /// after the first render.</remarks>
    [Parameter]
    public string? StateStorageKey { get; set; }

    private bool usesStorage => !string.IsNullOrWhiteSpace(StateStorageKey) && State is null;

    /// <summary>
    /// Gets a snapshot of the table's current state. Changing the snapshot does not change the table; pass it to
    /// <see cref="SetStateAsync"/> to do that.
    /// </summary>
    /// <returns>A new <see cref="TwDataTableState"/>.</returns>
    public TwDataTableState GetState() => new()
    {
        SortColumn = currentSortDirection == SortDirection.None ? null : currentSortColumn,
        SortDirection = currentSortColumn is null ? SortDirection.None : currentSortDirection,
        SearchQuery = currentSearchQuery,
        Page = currentPage,
        RowsPerPage = rowsPerPage,
        Columns = [.. columnStates.Select(c => c.Clone())]
    };

    /// <summary>
    /// Restores the table to <paramref name="state"/>, saves it when <see cref="StateStorageKey"/> is set and raises
    /// <see cref="StateChanged"/>.
    /// </summary>
    /// <param name="state">The state to restore. Columns and sorts the table does not have are ignored.</param>
    /// <returns>A task that completes once the state is applied.</returns>
    public async Task SetStateAsync(TwDataTableState state)
    {
        ArgumentNullException.ThrowIfNull(state);

        await InvokeAsync(async () =>
        {
            ApplyState(state);
            RefreshView();
            StateHasChanged();
            await NotifyStateChangedAsync();
        });
    }

    /// <summary>
    /// Puts the sort, search, page, page size and columns back to how the parameters define them, and forgets any
    /// state saved under <see cref="StateStorageKey"/>.
    /// </summary>
    /// <returns>A task that completes once the table is reset.</returns>
    public async Task ResetStateAsync()
    {
        await InvokeAsync(async () =>
        {
            ApplyState(new TwDataTableState { Columns = DefaultColumnStates() });
            RefreshView();
            StateHasChanged();
            await NotifyStateChangedAsync();
        });
    }

    /// <summary>
    /// Applies <see cref="State"/> when the parent passes a state the table has not seen before, so a bound state
    /// that the table itself raised is not applied a second time.
    /// </summary>
    /// <returns><c>true</c> when a state was applied.</returns>
    private bool ApplyIncomingState()
    {
        if (State is null || ReferenceEquals(State, lastAppliedState))
        {
            return false;
        }

        lastAppliedState = State;
        ApplyState(State);
        return true;
    }

    private void ApplyState(TwDataTableState state)
    {
        var sortColumn = FindColumn(state.SortColumn);
        var canSort = sortColumn is { IsSortable: true, PropertySelector: not null } && state.SortDirection != SortDirection.None;
        currentSortColumn = canSort ? sortColumn!.Name : null;
        currentSortDirection = canSort ? state.SortDirection : SortDirection.None;

        currentSearchQuery = string.IsNullOrWhiteSpace(state.SearchQuery) ? null : state.SearchQuery;
        rowsPerPage = state.RowsPerPage is > 0 ? state.RowsPerPage.Value : RowsPerPage;
        currentPage = Math.Max(1, state.Page);

        if (hasColumns)
        {
            columnStates = MergeColumnStates(state.Columns);
            RebuildVisibleColumns();
        }
    }

    private async Task NotifyStateChangedAsync()
    {
        var snapshot = GetState();
        lastAppliedState = snapshot;

        await StateChanged.InvokeAsync(snapshot);

        if (usesStorage)
        {
            await SaveStateAsync(snapshot);
        }
    }

    private async Task SaveStateAsync(TwDataTableState snapshot)
    {
        try
        {
            await jsRuntime.InvokeVoidAsync("twDataTable.saveState", StateStorageKey, JsonSerializer.Serialize(snapshot, TwDataTableStateJson.Options));
        }
        catch (JSDisconnectedException)
        {
            // The circuit has gone, so there is nobody left to restore the state for.
        }
        catch (InvalidOperationException)
        {
            // JS interop is not available (prerendering).
        }
    }

    private async Task LoadStoredStateAsync()
    {
        string? json;
        try
        {
            json = await jsRuntime.InvokeAsync<string?>("twDataTable.loadState", StateStorageKey);
        }
        catch (JSDisconnectedException)
        {
            return;
        }
        catch (InvalidOperationException)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(json))
        {
            return;
        }

        TwDataTableState? stored;
        try
        {
            stored = JsonSerializer.Deserialize<TwDataTableState>(json, TwDataTableStateJson.Options);
        }
        catch (JsonException)
        {
            // Saved by an older or different table; start from the parameters instead.
            return;
        }

        if (stored is null)
        {
            return;
        }

        ApplyState(stored);
        lastAppliedState = null;
        RefreshView();
        StateHasChanged();
    }
}
