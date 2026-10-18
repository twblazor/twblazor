// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using System.Globalization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web.Virtualization;

namespace TwBlazor.Components;

/// <summary>
/// The typed rows of a <see cref="TwTable"/>. Place it in the table's <see cref="TwTable.TableBody"/> and give it the
/// rows, and the table pages them (<see cref="TwTable.EnablePagination"/>) or renders only the ones in view
/// (<see cref="TwTable.EnableVirtualization"/>).
/// </summary>
/// <remarks>Without either option every row renders, so it also works as a plain typed loop. The rows can come from
/// <see cref="Items"/>, or be loaded a page or a scrolled window at a time from <see cref="ItemsProvider"/>.</remarks>
/// <typeparam name="TItem">The type of the rows.</typeparam>
public partial class TwTableRows<TItem> : ComponentBase, ITwTableRows, IAsyncDisposable
{
    private List<TwTableRow<TItem>> allRows = [];
    private List<TwTableRow<TItem>> viewRows = [];
    private Virtualize<TwTableRow<TItem>>? virtualizeRef;
    private TwTableRowsProvider<TItem>? lastProvider;
    private (int Start, int? Count)? loadedWindow;
    private bool loadRequested;
    private bool loading;
    private int totalCount;
    private CancellationTokenSource? loadCancellation;

    [CascadingParameter] private TwTable? table { get; set; }

    /// <summary>
    /// Gets or sets the rows. Leave it unset when <see cref="ItemsProvider"/> loads them instead.
    /// </summary>
    [Parameter]
    public IEnumerable<TItem>? Items { get; set; }

    /// <summary>
    /// Gets or sets a function that loads rows on demand, such as from a database or an API, in place of
    /// <see cref="Items"/>. It is asked for one page at a time with <see cref="TwTable.EnablePagination"/>, for each
    /// scrolled window with <see cref="TwTable.EnableVirtualization"/>, and for every row otherwise.
    /// </summary>
    [Parameter]
    public TwTableRowsProvider<TItem>? ItemsProvider { get; set; }

    /// <summary>
    /// Gets or sets the markup for one row, a <c>&lt;tr&gt;</c>, given the row's item.
    /// </summary>
    [Parameter]
    public RenderFragment<TItem>? ChildContent { get; set; }

    /// <summary>
    /// Gets or sets the markup for one row, a <c>&lt;tr&gt;</c>, given the row's item, its place among all the rows,
    /// and the <c>aria-rowindex</c> to put on it. Use it in place of <see cref="ChildContent"/> with
    /// <see cref="TwTable.EnableVirtualization"/>, so screen readers know where each rendered row sits.
    /// </summary>
    [Parameter]
    public RenderFragment<TwTableRow<TItem>>? RowTemplate { get; set; }

    /// <summary>
    /// Gets or sets the markup for every row on the current page at once, in place of a template per row. Use it
    /// inside <see cref="TwTable.RowGroups"/> to split a page into several <c>&lt;tbody&gt;</c> elements. Not used
    /// with <see cref="TwTable.EnableVirtualization"/>.
    /// </summary>
    [Parameter]
    public RenderFragment<TwTableRowsPage<TItem>>? PageTemplate { get; set; }

    /// <summary>
    /// Gets or sets what to render when there are no rows, such as a row with a message.
    /// </summary>
    [Parameter]
    public RenderFragment? EmptyContent { get; set; }

    /// <summary>
    /// Gets or sets what to render while <see cref="ItemsProvider"/> loads rows that are not on screen yet.
    /// </summary>
    [Parameter]
    public RenderFragment? LoadingContent { get; set; }

    /// <summary>
    /// Raised each time <see cref="ItemsProvider"/> returns rows, with the rows and the total count.
    /// </summary>
    [Parameter]
    public EventCallback<TwTableRowsResult<TItem>> ItemsLoaded { get; set; }

    /// <summary>
    /// Gets the rows currently on screen: the current page, or every row when the table is not paged. While
    /// virtualized, the rows <see cref="ItemsProvider"/> last returned.
    /// </summary>
    public IReadOnlyList<TItem> CurrentItems => [.. viewRows.Select(r => r.Item)];

    /// <summary>
    /// Gets how many rows there are in total, across every page.
    /// </summary>
    public int TotalCount => totalCount;

    private bool usesProvider => ItemsProvider is not null;

    private bool isVirtualized => table?.EnableVirtualization == true;

    private bool isPaged => table?.isPaged == true;

    /// <inheritdoc />
    protected override void OnInitialized()
    {
        table?.RegisterRows(this);
    }

    /// <inheritdoc />
    protected override void OnParametersSet()
    {
        if (usesProvider)
        {
            if (!ReferenceEquals(ItemsProvider, lastProvider))
            {
                lastProvider = ItemsProvider;
                loadedWindow = null;
            }

            // Virtualize asks for its own rows; otherwise load the window the table now shows.
            if (!isVirtualized && loadedWindow != RequestedWindow())
            {
                loadRequested = true;
            }

            Report();
            return;
        }

        ReadItems();
        Report();
    }

    private void ReadItems()
    {
        var items = Items switch
        {
            null => [],
            IReadOnlyList<TItem> list => list,
            _ => [.. Items]
        };

        totalCount = items.Count;
        allRows = [.. items.Select((item, index) => new TwTableRow<TItem>(item, index, AriaRowIndex(index)))];
        BuildView();
    }

    /// <inheritdoc />
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (loadRequested)
        {
            loadRequested = false;
            await LoadAsync();
        }
    }

    /// <summary>
    /// Shows the table's new page or page size.
    /// </summary>
    void ITwTableRows.OnTableViewChanged()
    {
        if (usesProvider)
        {
            if (!isVirtualized)
            {
                loadRequested = true;
            }
        }
        else
        {
            BuildView();
        }

        StateHasChanged();
    }

    /// <summary>
    /// Asks <see cref="ItemsProvider"/> for the rows on screen again, such as after the data changed or the sort
    /// or filter the provider applies changed. Without a provider, shows <see cref="Items"/> again.
    /// </summary>
    /// <returns>A task that completes once the rows are loaded.</returns>
    public async Task RefreshDataAsync()
    {
        await InvokeAsync(async () =>
        {
            if (!usesProvider)
            {
                ReadItems();
                Report();
                StateHasChanged();
                return;
            }

            if (isVirtualized)
            {
                if (virtualizeRef is not null)
                {
                    await virtualizeRef.RefreshDataAsync();
                }
                return;
            }

            await LoadAsync();
        });
    }

    private string? AriaRowIndex(int index) =>
        isVirtualized ? (index + 1 + (table?.HeaderRowCount ?? 1)).ToString(CultureInfo.InvariantCulture) : null;

    private (int Start, int? Count) RequestedWindow() =>
        isPaged ? (table!.pageStart, table.effectivePageSize) : (0, null);

    private void BuildView()
    {
        if (usesProvider)
        {
            return;
        }

        // Virtualize takes every row and renders the ones in view itself.
        viewRows = isPaged
            ? [.. allRows.Skip(table!.pageStart).Take(table.effectivePageSize)]
            : allRows;
    }

    private void Report() => table?.ReportRows(totalCount, loading, RowTemplate is not null);

    private async Task LoadAsync()
    {
        if (loadCancellation is not null)
        {
            await loadCancellation.CancelAsync();
            loadCancellation.Dispose();
        }

        loadCancellation = new CancellationTokenSource();
        var token = loadCancellation.Token;
        var window = RequestedWindow();

        loading = true;
        Report();
        StateHasChanged();

        TwTableRowsResult<TItem> result;
        try
        {
            result = await ItemsProvider!(new TwTableRowsRequest { StartIndex = window.Start, Count = window.Count, CancellationToken = token });
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

        loadedWindow = window;
        loading = false;
        totalCount = Math.Max(0, result.TotalCount);
        viewRows = [.. result.Items.Select((item, i) => new TwTableRow<TItem>(item, window.Start + i, null))];
        Report();
        await ItemsLoaded.InvokeAsync(result);
        StateHasChanged();
    }

    private async ValueTask<ItemsProviderResult<TwTableRow<TItem>>> ProvideVirtualRowsAsync(ItemsProviderRequest request)
    {
        loading = true;
        Report();

        var result = await ItemsProvider!(new TwTableRowsRequest
        {
            StartIndex = request.StartIndex,
            Count = request.Count,
            CancellationToken = request.CancellationToken
        });

        loading = false;
        totalCount = Math.Max(0, result.TotalCount);
        viewRows = [.. result.Items.Select((item, i) => new TwTableRow<TItem>(item, request.StartIndex + i, AriaRowIndex(request.StartIndex + i)))];
        Report();
        await ItemsLoaded.InvokeAsync(result);

        return new ItemsProviderResult<TwTableRow<TItem>>(viewRows, totalCount);
    }

    private RenderFragment RenderRow(TwTableRow<TItem> row) => RowTemplate is not null
        ? RowTemplate(row)
        : ChildContent?.Invoke(row.Item) ?? (_ => { });

    /// <summary>
    /// Stops any load still running and leaves the table.
    /// </summary>
    /// <returns>A task that completes once the load is cancelled.</returns>
    public async ValueTask DisposeAsync()
    {
        table?.UnregisterRows(this);

        if (loadCancellation is not null)
        {
            await loadCancellation.CancelAsync();
            loadCancellation.Dispose();
        }

        GC.SuppressFinalize(this);
    }
}
