// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

namespace TwBlazor.Enums;

/// <summary>
/// Special values for the span parameters of <see cref="Components.TwGridItem"/>.
/// </summary>
public static class GridSpan
{
    /// <summary>
    /// Spans every column or row of the grid (<c>col-span-full</c> or <c>row-span-full</c>).
    /// </summary>
    public const int Full = -1;
}

/// <summary>
/// Special values for the <c>Order</c> parameter of <see cref="Components.TwFlexItem"/> and <see cref="Components.TwGridItem"/>.
/// </summary>
/// <remarks>
/// Order changes only where an item is drawn. Screen readers and the Tab key still follow the markup, so keep
/// the markup in a sensible reading order.
/// </remarks>
public static class ItemOrder
{
    /// <summary>
    /// Draws the item before every other item (<c>order-first</c>).
    /// </summary>
    public const int First = int.MinValue;

    /// <summary>
    /// Draws the item after every other item (<c>order-last</c>).
    /// </summary>
    public const int Last = int.MaxValue;
}
