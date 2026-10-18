// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

namespace TwBlazor.Components;

/// <summary>
/// Loads rows for a <see cref="TwTableRows{TItem}"/> on demand, such as from a database or an API, so the table never
/// holds more rows than it shows. With <see cref="TwTable.EnablePagination"/> it is asked for one page at a time,
/// and with <see cref="TwTable.EnableVirtualization"/> for the rows scrolled into view.
/// </summary>
/// <typeparam name="TItem">The type of the rows.</typeparam>
/// <param name="request">Which rows to load.</param>
/// <returns>The rows, and how many rows there are in total.</returns>
public delegate ValueTask<TwTableRowsResult<TItem>> TwTableRowsProvider<TItem>(TwTableRowsRequest request);

/// <summary>
/// A request from a <see cref="TwTableRows{TItem}"/> for rows, passed to its
/// <see cref="TwTableRows{TItem}.ItemsProvider"/>.
/// </summary>
public sealed class TwTableRowsRequest
{
    /// <summary>
    /// Gets the zero based index of the first row to return.
    /// </summary>
    public int StartIndex { get; init; }

    /// <summary>
    /// Gets the most rows to return, or <c>null</c> for every row from <see cref="StartIndex"/> on.
    /// </summary>
    public int? Count { get; init; }

    /// <summary>
    /// Gets a token that is cancelled when the rows are no longer needed, for example because the page changed again
    /// before they arrived.
    /// </summary>
    public CancellationToken CancellationToken { get; init; }
}

/// <summary>
/// The rows a <see cref="TwTableRowsProvider{TItem}"/> loaded.
/// </summary>
/// <typeparam name="TItem">The type of the rows.</typeparam>
/// <param name="Items">The rows asked for.</param>
/// <param name="TotalCount">How many rows there are in total, across every page.</param>
public readonly record struct TwTableRowsResult<TItem>(IReadOnlyList<TItem> Items, int TotalCount);

/// <summary>
/// One row of a <see cref="TwTableRows{TItem}"/>, the context of <see cref="TwTableRows{TItem}.RowTemplate"/>.
/// </summary>
/// <typeparam name="TItem">The type of the rows.</typeparam>
/// <param name="Item">The row's item.</param>
/// <param name="Index">The row's zero based place among all the rows, not just the ones on screen.</param>
/// <param name="AriaRowIndex">The value for the row's <c>aria-rowindex</c> attribute while the table is virtualized,
/// or <c>null</c> when every row is in the page and screen readers can count them themselves.</param>
public readonly record struct TwTableRow<TItem>(TItem Item, int Index, string? AriaRowIndex);

/// <summary>
/// The rows on the current page of a <see cref="TwTableRows{TItem}"/>, the context of
/// <see cref="TwTableRows{TItem}.PageTemplate"/>.
/// </summary>
/// <typeparam name="TItem">The type of the rows.</typeparam>
/// <param name="Rows">The rows to render, in order.</param>
public readonly record struct TwTableRowsPage<TItem>(IReadOnlyList<TwTableRow<TItem>> Rows);

/// <summary>
/// What <see cref="TwTable"/> needs from the <see cref="TwTableRows{TItem}"/> inside it, without knowing its row type.
/// </summary>
internal interface ITwTableRows
{
    /// <summary>
    /// Called when the table's page or page size changes, so the rows show the new page.
    /// </summary>
    void OnTableViewChanged();
}
