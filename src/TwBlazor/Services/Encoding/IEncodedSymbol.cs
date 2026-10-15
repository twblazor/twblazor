// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

namespace TwBlazor.Services.Encoding;

/// <summary>
/// An encoded symbol: a grid of dark and light modules that a scanner reads, such as a <see cref="QrMatrix"/>
/// (a square grid) or a <see cref="Barcode"/> (a single row of bars).
/// </summary>
/// <remarks>
/// Every renderer works from this grid, so one renderer draws every kind of symbol. Coordinates start at the
/// top-left, with <c>x</c> running right and <c>y</c> running down. The grid does not include the quiet zone.
/// </remarks>
public interface IEncodedSymbol
{
    /// <summary>
    /// Gets the number of module columns.
    /// </summary>
    int Width { get; }

    /// <summary>
    /// Gets the number of module rows; 1 for a one-dimensional barcode, whose bars are stretched to any height.
    /// </summary>
    int Height { get; }

    /// <summary>
    /// Gets the light margin, in modules, the symbology needs around the symbol so scanners can find it. A
    /// one-dimensional barcode only needs it on the left and right.
    /// </summary>
    int QuietZone { get; }

    /// <summary>
    /// Gets whether the module at a position is dark.
    /// </summary>
    /// <param name="x">The column, from 0 at the left.</param>
    /// <param name="y">The row, from 0 at the top.</param>
    /// <returns><see langword="true"/> for a dark module; <see langword="false"/> for a light one or a position outside the grid.</returns>
    bool this[int x, int y] { get; }
}
