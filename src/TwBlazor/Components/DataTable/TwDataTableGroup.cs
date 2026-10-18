// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

namespace TwBlazor.Components;

/// <summary>
/// One group of rows in a <see cref="TwDataTable{TItem}"/> that is grouped by a column. It is the context of
/// <see cref="TwDataTable{TItem}.GroupHeaderTemplate"/>, so a group header can show totals or other summaries.
/// </summary>
/// <typeparam name="TItem">The type of items displayed in the table.</typeparam>
public sealed class TwDataTableGroup<TItem>
{
    internal TwDataTableGroup(object? key, string label, IReadOnlyList<TItem> items, bool expanded)
    {
        Key = key;
        Label = label;
        Items = items;
        Expanded = expanded;
    }

    /// <summary>
    /// Gets the value every row in the group shares for the grouped column.
    /// </summary>
    public object? Key { get; }

    /// <summary>
    /// Gets the text the group header shows for <see cref="Key"/>.
    /// </summary>
    public string Label { get; }

    /// <summary>
    /// Gets every row in the group that matches the search, in the current sort order, including rows on other pages.
    /// </summary>
    public IReadOnlyList<TItem> Items { get; }

    /// <summary>
    /// Gets whether the group's rows are shown.
    /// </summary>
    public bool Expanded { get; }
}
