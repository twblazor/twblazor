// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using Microsoft.AspNetCore.Components;
using TwBlazor.Enums;
using TwBlazor.Localization;

namespace TwBlazor.Components;

public partial class TwDataTable<TItem>
{
    private TwTableRows<TItem>? providerRows;
    private TwTableRowsProvider<TItem>? rowsProvider;
    private List<TItem> loadedItems = [];
    private int providerTotalCount;
    private bool providerHasLoaded;
    private bool refreshProviderRows;
    private Func<string>? pendingAnnouncement;
    private TwDataTableItemsProvider<TItem>? lastItemsProvider;

    /// <summary>
    /// Whether to render only the rows scrolled into view, so a table of thousands of rows stays fast
    /// (<see cref="TwTable.EnableVirtualization"/>). The rows scroll inside the table, which is <see cref="Height"/>
    /// tall, and the pagination is replaced by scrolling. Defaults to <c>false</c>.
    /// </summary>
    /// <remarks>Every row should be about <see cref="ItemSize"/> pixels tall, so keep cell content to one line. With
    /// <see cref="Columns"/> the table tells assistive technology how many rows it has and where each rendered row
    /// sits, so a screen reader still reads "row 5000 of 10000".</remarks>
    [Parameter]
    public bool EnableVirtualization { get; set; }

    /// <summary>
    /// Gets or sets the height of a row in pixels when <see cref="EnableVirtualization"/> is set, used to work out
    /// which rows are in view before they render. Defaults to <c>41</c>, the height of a row in the default theme.
    /// </summary>
    [Parameter]
    public float ItemSize { get; set; } = 41f;

    /// <summary>
    /// Gets or sets how many extra rows to render above and below the ones in view when
    /// <see cref="EnableVirtualization"/> is set, so fast scrolling does not show gaps. Defaults to <c>5</c>.
    /// </summary>
    [Parameter]
    public int OverscanCount { get; set; } = 5;

    /// <summary>
    /// Gets or sets the tallest the table can grow before its rows scroll inside it, as any CSS length. Defaults to
    /// <c>32rem</c> when <see cref="EnableVirtualization"/> is set, and to no limit otherwise.
    /// </summary>
    [Parameter]
    public string? Height { get; set; }

    /// <summary>
    /// Whether the header row stays in view while the rows scroll. Always on when
    /// <see cref="EnableVirtualization"/> is set. Defaults to <c>false</c>.
    /// </summary>
    [Parameter]
    public bool StickyHeader { get; set; }

    /// <summary>
    /// Gets or sets a function that loads rows on demand, such as from a database or an API, in place of
    /// <see cref="Items"/>. The table passes it the page, sort, search and column filters, so the work happens where
    /// the data lives. Grouping is not available, and an export of every row asks the function for all of them.
    /// </summary>
    [Parameter]
    public TwDataTableItemsProvider<TItem>? ItemsProvider { get; set; }

    /// <summary>
    /// Gets or sets what to show while <see cref="ItemsProvider"/> loads the first rows. Defaults to the localized
    /// <see cref="TwStrings.DataTable_Loading"/> text ("Loading…").
    /// </summary>
    [Parameter]
    public RenderFragment? LoadingContent { get; set; }

    private bool usesProvider => ItemsProvider is not null;

    private bool isPaged => EnablePagination && !EnableVirtualization;

    private bool isVirtualized => EnableVirtualization;

    private bool effectiveStickyHeader => StickyHeader || isVirtualized;

    private string effectiveLoadingText => localizer[TwStrings.DataTable_Loading];

    /// <summary>
    /// Asks <see cref="ItemsProvider"/> for the rows again, keeping the page, sort, search and filters. Call it after
    /// the data changes somewhere the table cannot see. Without a provider, shows <see cref="Items"/> again.
    /// </summary>
    /// <returns>A task that completes once the rows are loaded.</returns>
    public async Task RefreshDataAsync()
    {
        await InvokeAsync(async () =>
        {
            if (!usesProvider)
            {
                RefreshView();
                StateHasChanged();
                return;
            }

            if (providerRows is not null)
            {
                await providerRows.RefreshDataAsync();
            }
        });
    }

    /// <summary>
    /// Asks for the rows again when the <see cref="ItemsProvider"/> itself changes.
    /// </summary>
    private void SyncItemsProvider()
    {
        if (!ReferenceEquals(ItemsProvider, lastItemsProvider))
        {
            refreshProviderRows = lastItemsProvider is not null && usesProvider;
            lastItemsProvider = ItemsProvider;
            providerHasLoaded = false;
        }

        // A method group makes a new delegate on every call, which the rows would read as a new provider.
        rowsProvider ??= ProvideRowsAsync;
    }

    private async Task RefreshProviderRowsIfRequestedAsync()
    {
        if (refreshProviderRows && providerRows is not null)
        {
            refreshProviderRows = false;
            await providerRows.RefreshDataAsync();
        }
    }

    private TwDataTableItemsRequest BuildRequest(int startIndex, int? count, CancellationToken cancellationToken, bool ignoreSearch = false) => new()
    {
        StartIndex = startIndex,
        Count = count,
        SortColumn = currentSortDirection == SortDirection.None ? null : currentSortColumn,
        SortDirection = currentSortColumn is null ? SortDirection.None : currentSortDirection,
        SearchQuery = ignoreSearch ? null : currentSearchQuery,
        Filters = ignoreSearch ? [] : ActiveFilters(),
        CancellationToken = cancellationToken
    };

    /// <summary>
    /// Adapts the table's <see cref="ItemsProvider"/> to the <see cref="TwTableRows{TItem}"/> that pages or
    /// virtualizes the rows, adding the sort, search and filters.
    /// </summary>
    private async ValueTask<TwTableRowsResult<TItem>> ProvideRowsAsync(TwTableRowsRequest request)
    {
        var result = await ItemsProvider!(BuildRequest(request.StartIndex, request.Count, request.CancellationToken));
        return new TwTableRowsResult<TItem>(result.Items, result.TotalCount);
    }

    private void OnRowsLoaded(TwTableRowsResult<TItem> result)
    {
        loadedItems = [.. result.Items];
        providerTotalCount = Math.Max(0, result.TotalCount);
        providerHasLoaded = true;
        UpdateDisplay();
        AnnouncePending();
    }

    /// <summary>
    /// Announces a change whose wording depends on rows that had to be loaded first, such as how many rows a search
    /// found.
    /// </summary>
    private void AnnouncePending()
    {
        if (pendingAnnouncement is { } announce)
        {
            pendingAnnouncement = null;
            sortStatusMessage = announce();
        }
    }

    private void Announce(Func<string> message)
    {
        if (usesProvider)
        {
            pendingAnnouncement = message;
        }
        else
        {
            sortStatusMessage = message();
        }
    }
}
