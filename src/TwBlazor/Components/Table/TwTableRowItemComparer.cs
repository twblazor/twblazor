// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

namespace TwBlazor.Components;

/// <summary>
/// Compares <see cref="TwTableRow{TItem}"/> values by their item alone, ignoring the row's index, so a virtualized
/// table recognises a row it already rendered after rows are added above it.
/// </summary>
/// <typeparam name="TItem">The type of the rows.</typeparam>
internal sealed class TwTableRowItemComparer<TItem> : IEqualityComparer<TwTableRow<TItem>>
{
    /// <summary>
    /// Gets the shared instance, which compares items with <see cref="EqualityComparer{T}.Default"/>.
    /// </summary>
    public static TwTableRowItemComparer<TItem> Instance { get; } = new();

    /// <summary>
    /// Determines whether two rows hold the same item.
    /// </summary>
    /// <param name="x">The first row.</param>
    /// <param name="y">The second row.</param>
    /// <returns><see langword="true"/> when both rows hold equal items.</returns>
    public bool Equals(TwTableRow<TItem> x, TwTableRow<TItem> y) => EqualityComparer<TItem>.Default.Equals(x.Item, y.Item);

    /// <summary>
    /// Gets a hash code for the row's item.
    /// </summary>
    /// <param name="obj">The row.</param>
    /// <returns>The item's hash code, or 0 for a <see langword="null"/> item.</returns>
    public int GetHashCode(TwTableRow<TItem> obj) => obj.Item is null ? 0 : EqualityComparer<TItem>.Default.GetHashCode(obj.Item);
}
