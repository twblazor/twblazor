// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using TwBlazor.Enums;

namespace TwBlazor.Components;

/// <summary>
/// A snapshot of everything a person can change about a <see cref="TwDataTable{TItem}"/>: the sort, the search, the
/// page, the page size and the columns. Save it anywhere (a database, the URL, browser storage) and hand it back
/// through <see cref="TwDataTable{TItem}.State"/> or <see cref="TwDataTable{TItem}.SetStateAsync"/> to restore the table.
/// </summary>
/// <remarks>The snapshot only holds plain values, so it serializes to JSON as it is. Columns are matched by
/// <see cref="TwDataTableColumn{TItem}.Name"/>: a saved column that no longer exists is ignored, and a new column
/// that the snapshot does not mention is added at the end with its default settings.</remarks>
public sealed class TwDataTableState
{
    /// <summary>
    /// Gets or sets the <see cref="TwDataTableColumn{TItem}.Name"/> of the sorted column, or <c>null</c> when the
    /// rows are in their original order.
    /// </summary>
    public string? SortColumn { get; set; }

    /// <summary>
    /// Gets or sets the direction of the sort. Ignored when <see cref="SortColumn"/> is <c>null</c>.
    /// </summary>
    public SortDirection SortDirection { get; set; }

    /// <summary>
    /// Gets or sets the text in the search field, or <c>null</c> when nothing is searched for.
    /// </summary>
    public string? SearchQuery { get; set; }

    /// <summary>
    /// Gets or sets the one based page number. Defaults to <c>1</c>.
    /// </summary>
    public int Page { get; set; } = 1;

    /// <summary>
    /// Gets or sets the number of rows on a page, or <c>null</c> to use
    /// <see cref="TwDataTable{TItem}.RowsPerPage"/>.
    /// </summary>
    public int? RowsPerPage { get; set; }

    /// <summary>
    /// Gets or sets the columns in display order, with whether each is shown and how wide it is.
    /// </summary>
    public List<TwDataTableColumnState> Columns { get; set; } = [];

    /// <summary>
    /// Creates a deep copy of this snapshot, so changing the copy never changes the table.
    /// </summary>
    /// <returns>A new <see cref="TwDataTableState"/> with the same values.</returns>
    public TwDataTableState Clone() => new()
    {
        SortColumn = SortColumn,
        SortDirection = SortDirection,
        SearchQuery = SearchQuery,
        Page = Page,
        RowsPerPage = RowsPerPage,
        Columns = [.. Columns.Select(c => c.Clone())]
    };
}

/// <summary>
/// The saved settings of one <see cref="TwDataTableColumn{TItem}"/> in a <see cref="TwDataTableState"/>.
/// </summary>
public sealed class TwDataTableColumnState
{
    /// <summary>
    /// Gets or sets the <see cref="TwDataTableColumn{TItem}.Name"/> of the column these settings belong to.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets whether the column is shown. Defaults to <c>true</c>.
    /// </summary>
    public bool Visible { get; set; } = true;

    /// <summary>
    /// Gets or sets the width of the column as a CSS length, or <c>null</c> to share the space that is left.
    /// </summary>
    public string? Width { get; set; }

    /// <summary>
    /// Creates a copy of these settings.
    /// </summary>
    /// <returns>A new <see cref="TwDataTableColumnState"/> with the same values.</returns>
    public TwDataTableColumnState Clone() => new() { Name = Name, Visible = Visible, Width = Width };
}
