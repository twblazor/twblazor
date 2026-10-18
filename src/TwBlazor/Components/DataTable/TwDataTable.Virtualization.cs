// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using System.Globalization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web.Virtualization;
using TwBlazor.Enums;
using TwBlazor.Localization;

namespace TwBlazor.Components;

public partial class TwDataTable<TItem>
{
    private const string defaultVirtualHeight = "32rem";

    private Virtualize<IndexedRow>? virtualizeRef;
    private List<IndexedRow> virtualRows = [];
    private List<IndexedLine> virtualLines = [];
    private List<TItem> loadedItems = [];
    private int providerTotalCount;
    private bool providerHasLoaded;
    private bool loadRequested;
    private bool isLoading;
    private Func<string>? pendingAnnouncement;
    private TwDataTableItemsProvider<TItem>? lastItemsProvider;
    private CancellationTokenSource? loadCancellation;

    /// <summary>
    /// Whether to render only the rows scrolled into view, so a table of thousands of rows stays fast. The rows
    /// scroll inside the table, which is <see cref="Height"/> tall, and the pagination is replaced by scrolling.
    /// Only applies when <see cref="Columns"/> is used. Defaults to <c>false</c>.
    /// </summary>
    /// <remarks>Every row should be about <see cref="ItemSize"/> pixels tall, so keep cell content to one line. The
    /// table tells assistive technology how many rows it has and where each rendered row sits, so a screen reader
    /// still reads "row 5000 of 10000".</remarks>
    [Parameter]
    public bool Virtualize { get; set; }

    /// <summary>
    /// Gets or sets the height of a row in pixels when <see cref="Virtualize"/> is set, used to work out which rows
    /// are in view before they render. Defaults to <c>41</c>, the height of a row in the default theme.
    /// </summary>
    [Parameter]
    public float ItemSize { get; set; } = 41f;

    /// <summary>
    /// Gets or sets how many extra rows to render above and below the ones in view when <see cref="Virtualize"/> is
    /// set, so fast scrolling does not show gaps. Defaults to <c>5</c>.
    /// </summary>
    [Parameter]
    public int OverscanCount { get; set; } = 5;

    /// <summary>
    /// Gets or sets the tallest the table can grow before its rows scroll inside it, as any CSS length. Defaults to
    /// <c>32rem</c> when <see cref="Virtualize"/> is set, and to no limit otherwise.
    /// </summary>
    [Parameter]
    public string? Height { get; set; }

    /// <summary>
    /// Whether the header row stays in view while the rows scroll. Always on when <see cref="Virtualize"/> is set.
    /// Defaults to <c>false</c>.
    /// </summary>
    [Parameter]
    public bool StickyHeader { get; set; }

    /// <summary>
    /// Gets or sets a function that loads rows on demand, such as from a database or an API, in place of
    /// <see cref="Items"/>. The table passes it the page, sort and search, so the work happens where the data lives.
    /// Grouping is not available, and an export of every row asks the function for all of them.
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

    private bool isPaged => Pageable && !Virtualize;

    private bool isVirtualized => Virtualize && hasColumns;

    private string? effectiveHeight => Height ?? (isVirtualized ? defaultVirtualHeight : null);

    private bool effectiveStickyHeader => StickyHeader || isVirtualized;

    // The header row is row 1, so data rows start at 2.
    private int virtualRowCount => (usesProvider ? providerTotalCount : isGrouped ? lines.Count : viewItems.Count) + 1;

    private Dictionary<string, object> effectiveTableAttributes
    {
        get
        {
            var attributes = new Dictionary<string, object>(TableAttributes);
            if (isVirtualized)
            {
                attributes["aria-rowcount"] = virtualRowCount.ToString(CultureInfo.InvariantCulture);
            }
            if (isLoading)
            {
                attributes["aria-busy"] = "true";
            }
            return attributes;
        }
    }

    private string effectiveLoadingText => localizer[TwStrings.DataTable_Loading];

    private static string RowIndex(int index) => (index + 2).ToString(CultureInfo.InvariantCulture);

    /// <summary>
    /// Asks <see cref="ItemsProvider"/> for the rows again, keeping the page, sort and search. Call it after the
    /// data changes somewhere the table cannot see.
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

            await LoadAsync();
        });
    }

    /// <summary>
    /// Decides whether the rows need loading after the parameters change: on the first render, when the
    /// <see cref="ItemsProvider"/> itself changes, or when the view changed in a way the loaded rows cannot follow.
    /// </summary>
    private void SyncItemsProvider()
    {
        if (!ReferenceEquals(ItemsProvider, lastItemsProvider))
        {
            lastItemsProvider = ItemsProvider;
            loadRequested = usesProvider;
            providerHasLoaded = false;
        }
    }

    /// <summary>
    /// Builds the indexed rows <see cref="Virtualize{TItem}"/> renders, so each row knows its place for
    /// <c>aria-rowindex</c>.
    /// </summary>
    private void BuildVirtualRows()
    {
        if (!isVirtualized || usesProvider)
        {
            virtualRows = [];
            virtualLines = [];
            return;
        }

        if (isGrouped)
        {
            virtualRows = [];
            virtualLines = [.. lines.Select((line, index) => new IndexedLine(index, line))];
        }
        else
        {
            virtualLines = [];
            virtualRows = [.. viewItems.Select((item, index) => new IndexedRow(index, item))];
        }
    }

    private async Task LoadIfRequestedAsync(bool firstRender)
    {
        if (!loadRequested || !usesProvider)
        {
            return;
        }

        loadRequested = false;

        // On its first render Virtualize asks for its rows itself.
        if (isVirtualized && firstRender)
        {
            return;
        }

        await LoadAsync();
    }

    private async Task LoadAsync()
    {
        if (isVirtualized)
        {
            if (virtualizeRef is not null)
            {
                await virtualizeRef.RefreshDataAsync();
                StateHasChanged();
            }
            return;
        }

        await LoadPageAsync();
    }

    private TwDataTableItemsRequest BuildRequest(int startIndex, int? count, CancellationToken cancellationToken, bool ignoreSearch = false) => new()
    {
        StartIndex = startIndex,
        Count = count,
        SortColumn = currentSortDirection == SortDirection.None ? null : currentSortColumn,
        SortDirection = currentSortColumn is null ? SortDirection.None : currentSortDirection,
        SearchQuery = ignoreSearch ? null : currentSearchQuery,
        CancellationToken = cancellationToken
    };

    private async Task LoadPageAsync()
    {
        loadCancellation?.Cancel();
        loadCancellation?.Dispose();
        loadCancellation = new CancellationTokenSource();
        var token = loadCancellation.Token;

        isLoading = true;
        StateHasChanged();

        TwDataTableItemsResult<TItem> result;
        try
        {
            var request = isPaged
                ? BuildRequest((currentPage - 1) * rowsPerPage, rowsPerPage, token)
                : BuildRequest(0, null, token);
            result = await ItemsProvider!(request);
        }
        catch (OperationCanceledException) when (token.IsCancellationRequested)
        {
            // A newer load replaced this one.
            return;
        }

        if (token.IsCancellationRequested)
        {
            return;
        }

        loadedItems = [.. result.Items];
        providerTotalCount = Math.Max(0, result.TotalCount);
        providerHasLoaded = true;
        isLoading = false;

        var previousPage = currentPage;
        UpdateDisplay();
        if (currentPage != previousPage)
        {
            // The page was past the end of the data, which may have shrunk; load the last page instead.
            await LoadPageAsync();
            return;
        }

        AnnouncePending();
        StateHasChanged();
    }

    private async ValueTask<ItemsProviderResult<IndexedRow>> ProvideVirtualRowsAsync(ItemsProviderRequest request)
    {
        var result = await ItemsProvider!(BuildRequest(request.StartIndex, request.Count, request.CancellationToken));
        var total = Math.Max(0, result.TotalCount);

        var countChanged = total != providerTotalCount;
        providerTotalCount = total;
        providerHasLoaded = true;
        loadedItems = [.. result.Items];
        totalCount = total;

        if (countChanged || pendingAnnouncement is not null)
        {
            AnnouncePending();
            await InvokeAsync(StateHasChanged);
        }

        return new ItemsProviderResult<IndexedRow>(result.Items.Select((item, i) => new IndexedRow(request.StartIndex + i, item)), total);
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

    /// <summary>
    /// A row and its zero based place among all the rows, for <c>aria-rowindex</c>.
    /// </summary>
    private readonly record struct IndexedRow(int Index, TItem Item);

    /// <summary>
    /// A grouped layout line and its zero based place among all the lines, for <c>aria-rowindex</c>.
    /// </summary>
    private readonly record struct IndexedLine(int Index, DataTableLine Line);
}
