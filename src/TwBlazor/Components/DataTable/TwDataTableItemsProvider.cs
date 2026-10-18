// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using TwBlazor.Enums;

namespace TwBlazor.Components;

/// <summary>
/// Loads rows for a <see cref="TwDataTable{TItem}"/> on demand, such as from a database or an API, so the table never
/// holds more rows than it shows. The table asks for one page at a time, or for the rows scrolled into view when
/// <see cref="TwDataTable{TItem}.EnableVirtualization"/> is set.
/// </summary>
/// <typeparam name="TItem">The type of items displayed in the table.</typeparam>
/// <param name="request">Which rows to load, and the sort and search to load them with.</param>
/// <returns>The rows, and how many rows match in total.</returns>
public delegate ValueTask<TwDataTableItemsResult<TItem>> TwDataTableItemsProvider<TItem>(TwDataTableItemsRequest request);

/// <summary>
/// A request from a <see cref="TwDataTable{TItem}"/> for rows, passed to its
/// <see cref="TwDataTable{TItem}.ItemsProvider"/>.
/// </summary>
public sealed class TwDataTableItemsRequest
{
    /// <summary>
    /// Gets the zero based index of the first row to return, counted after the search and sort are applied.
    /// </summary>
    public int StartIndex { get; init; }

    /// <summary>
    /// Gets the most rows to return, or <c>null</c> for every row from <see cref="StartIndex"/> on, as an export asks.
    /// </summary>
    public int? Count { get; init; }

    /// <summary>
    /// Gets the <see cref="TwDataTableColumn{TItem}.Name"/> of the column to sort by, or <c>null</c> for no sort.
    /// </summary>
    public string? SortColumn { get; init; }

    /// <summary>
    /// Gets the direction to sort <see cref="SortColumn"/> in.
    /// </summary>
    public SortDirection SortDirection { get; init; }

    /// <summary>
    /// Gets the text in the search field, or <c>null</c> when nothing is searched for.
    /// </summary>
    public string? SearchQuery { get; init; }

    /// <summary>
    /// Gets each filtered column's filter, keyed by <see cref="TwDataTableColumn{TItem}.Name"/>. Empty when no column
    /// is filtered.
    /// </summary>
    public IReadOnlyDictionary<string, TwDataTableColumnFilter> Filters { get; init; } = new Dictionary<string, TwDataTableColumnFilter>();

    /// <summary>
    /// Gets a token that is cancelled when the table no longer needs the rows, for example because the person sorted
    /// again before they arrived.
    /// </summary>
    public CancellationToken CancellationToken { get; init; }
}

/// <summary>
/// The rows a <see cref="TwDataTableItemsProvider{TItem}"/> loaded.
/// </summary>
/// <typeparam name="TItem">The type of items displayed in the table.</typeparam>
/// <param name="Items">The rows asked for.</param>
/// <param name="TotalCount">How many rows match the search in total, across every page.</param>
public readonly record struct TwDataTableItemsResult<TItem>(IReadOnlyList<TItem> Items, int TotalCount);
