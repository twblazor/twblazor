// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using Microsoft.AspNetCore.Components;
using System.Globalization;
using TwBlazor.Localization;
using TwBlazor.Utilities;

namespace TwBlazor.Components;

public partial class TwTable
{
    private const string defaultVirtualHeight = "32rem";

    private readonly List<ITwTableRows> _rowsSources = [];
    private int currentPage = 1;
    private int pageSize = 10;
    private int? lastCurrentPageParameter;
    private int? lastPageSizeParameter;
    private int rowCount;
    private bool rowsBusy;
    private bool rowsProvideIndexes;

    /// <summary>
    /// Whether to split the rows of a <see cref="TwTableRows{TItem}"/> inside the table into pages, with a
    /// <see cref="TwPagination"/> below the table. Defaults to <c>false</c>. Ignored while
    /// <see cref="EnableVirtualization"/> is set, since the rows then scroll instead.
    /// </summary>
    [Parameter]
    public bool EnablePagination { get; set; }

    /// <summary>
    /// Gets or sets the one based page shown when <see cref="EnablePagination"/> is set. Bind it with
    /// <c>@bind-CurrentPage</c> to follow or change the page. Defaults to <c>1</c>.
    /// </summary>
    [Parameter]
    public int CurrentPage { get; set; } = 1;

    /// <summary>
    /// Raised when the page changes, including when it moves back because the rows ran out.
    /// </summary>
    [Parameter]
    public EventCallback<int> CurrentPageChanged { get; set; }

    /// <summary>
    /// Gets or sets how many rows a page holds when <see cref="EnablePagination"/> is set. Defaults to <c>10</c>.
    /// </summary>
    [Parameter]
    public int PageSize { get; set; } = 10;

    /// <summary>
    /// Raised when a person picks a different page size.
    /// </summary>
    [Parameter]
    public EventCallback<int> PageSizeChanged { get; set; }

    /// <summary>
    /// Gets or sets the page sizes a person can pick from. Defaults to 5, 10, 25, 50 and 100.
    /// </summary>
    [Parameter]
    public int[] PageSizeOptions { get; set; } = [5, 10, 25, 50, 100];

    /// <summary>
    /// Whether the pagination shows its page size list. Defaults to <c>true</c>.
    /// </summary>
    [Parameter]
    public bool ShowPageSize { get; set; } = true;

    /// <summary>
    /// Whether the pagination uses regular size buttons instead of the default compact ones. Defaults to
    /// <c>false</c>.
    /// </summary>
    [Parameter]
    public bool ComfortablePagination { get; set; }

    /// <summary>
    /// Whether the pagination announces the new page to screen readers. Defaults to <c>true</c>. Turn it off when
    /// you announce page changes yourself.
    /// </summary>
    [Parameter]
    public bool AnnouncePageChanges { get; set; } = true;

    /// <summary>
    /// Gets or sets the accessible name of the pagination. Defaults to the localized
    /// <see cref="TwStrings.Table_Pagination"/> text ("Table pagination").
    /// </summary>
    [Parameter]
    public string? PaginationLabel { get; set; }

    /// <summary>
    /// Whether to render only the rows of a <see cref="TwTableRows{TItem}"/> that are scrolled into view, so a table
    /// of thousands of rows stays fast. The rows scroll inside the table, which is <see cref="MaxHeight"/> tall
    /// (<c>32rem</c> unless set), and the header stays in view. Defaults to <c>false</c>.
    /// </summary>
    /// <remarks>Every row should be about <see cref="ItemSize"/> pixels tall, so keep cell content to one line.
    /// When the rows use <see cref="TwTableRows{TItem}.RowTemplate"/> and put its <c>AriaRowIndex</c> on each row,
    /// the table tells assistive technology how many rows it has, so a screen reader still reads "row 5000 of
    /// 10000".</remarks>
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
    /// Gets or sets how many rows the header holds, used to number the body rows for assistive technology when
    /// <see cref="EnableVirtualization"/> is set. Defaults to <c>1</c>.
    /// </summary>
    [Parameter]
    public int HeaderRowCount { get; set; } = 1;

    internal bool isPaged => EnablePagination && !EnableVirtualization;

    internal int pageStart => (currentPage - 1) * pageSize;

    internal int effectivePageSize => pageSize;

    private string? effectiveMaxHeight => MaxHeight ?? (EnableVirtualization ? defaultVirtualHeight : null);

    private bool effectiveStickyHeader => StickyHeader || EnableVirtualization;

    private string effectivePaginationLabel => PaginationLabel ?? localizer[TwStrings.Table_Pagination];

    private int totalPages => pageSize <= 0 || rowCount == 0 ? 1 : (int)Math.Ceiling(rowCount / (double)pageSize);

    private string? ariaRowCount => EnableVirtualization && rowsProvideIndexes
        ? (rowCount + HeaderRowCount).ToString(CultureInfo.InvariantCulture)
        : null;

    private string? ariaBusy => rowsBusy ? "true" : null;

    private string paginationClasses => new ClassBuilder(options.Theme.Spacing.MarginTop.Get(4))
        .AddClass(options.Theme.Sizing.FullWidth)
        .AddClass(options.Theme.Flexbox.Justify.Between).Build();

    /// <summary>
    /// Picks up a page or page size the parent passes in, the same way a bound value is.
    /// </summary>
    private void SyncPagingParameters()
    {
        if (lastPageSizeParameter != PageSize)
        {
            lastPageSizeParameter = PageSize;
            pageSize = PageSize > 0 ? PageSize : 10;
        }

        if (lastCurrentPageParameter != CurrentPage)
        {
            lastCurrentPageParameter = CurrentPage;
            var page = Math.Max(1, CurrentPage);
            if (page != currentPage)
            {
                currentPage = page;
                NotifyRowsSources();
            }
        }
    }

    internal void RegisterRows(ITwTableRows rows)
    {
        if (!_rowsSources.Contains(rows))
        {
            _rowsSources.Add(rows);
        }
    }

    internal void UnregisterRows(ITwTableRows rows) => _rowsSources.Remove(rows);

    /// <summary>
    /// Called by a <see cref="TwTableRows{TItem}"/> when it knows how many rows there are, whether they are still
    /// loading, and whether its rows carry their own index for assistive technology.
    /// </summary>
    internal void ReportRows(int count, bool busy, bool providesIndexes)
    {
        if (count == rowCount && busy == rowsBusy && providesIndexes == rowsProvideIndexes)
        {
            return;
        }

        rowCount = Math.Max(0, count);
        rowsBusy = busy;
        rowsProvideIndexes = providesIndexes;

        // The rows may have shrunk under the page, as when a search leaves fewer rows; move back to the last page.
        if (!busy && isPaged && currentPage > totalPages)
        {
            var lastPage = totalPages;
            _ = InvokeAsync(() => ChangePageAsync(lastPage));
        }

        StateHasChanged();
    }

    private async Task ChangePageAsync(int page)
    {
        var target = Math.Clamp(page, 1, totalPages);
        if (target == currentPage)
        {
            return;
        }

        currentPage = target;
        NotifyRowsSources();
        await CurrentPageChanged.InvokeAsync(target);
    }

    private async Task OnPageSizeChangedAsync(int size)
    {
        if (size <= 0 || size == pageSize)
        {
            return;
        }

        pageSize = size;
        var pageChanged = currentPage != 1;
        currentPage = 1;
        NotifyRowsSources();
        await PageSizeChanged.InvokeAsync(size);

        if (pageChanged)
        {
            await CurrentPageChanged.InvokeAsync(1);
        }
    }

    private void NotifyRowsSources()
    {
        foreach (var rows in _rowsSources)
        {
            rows.OnTableViewChanged();
        }
    }
}
