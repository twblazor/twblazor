// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using TwBlazor.Enums;

namespace TwBlazor.QrCoding;

/// <summary>
/// An encoded QR code: a square grid of dark and light modules, ready to be drawn by any renderer.
/// </summary>
/// <remarks>
/// The matrix is immutable, so it can be cached and shared freely. Coordinates start at the top-left,
/// with <c>x</c> running right and <c>y</c> running down. It does not include the quiet zone; a renderer
/// adds that margin itself.
/// </remarks>
public sealed class QrMatrix
{
    private readonly bool[] _modules;

    internal QrMatrix(int version, QrErrorCorrection errorCorrection, int mask, bool[] modules)
    {
        Version = version;
        ErrorCorrection = errorCorrection;
        Mask = mask;
        Size = QrTables.GetSize(version);
        _modules = modules;
    }

    /// <summary>
    /// Gets the version, from 1 (21x21 modules) to 40 (177x177 modules).
    /// </summary>
    public int Version { get; }

    /// <summary>
    /// Gets the error correction level the code was encoded with. This can be higher than the level asked
    /// for when <see cref="QrEncodeOptions.BoostErrorCorrection"/> is on.
    /// </summary>
    public QrErrorCorrection ErrorCorrection { get; }

    /// <summary>
    /// Gets the mask pattern applied, from 0 to 7.
    /// </summary>
    public int Mask { get; }

    /// <summary>
    /// Gets the width and height of the grid, in modules.
    /// </summary>
    public int Size { get; }

    /// <summary>
    /// Gets whether the module at a position is dark.
    /// </summary>
    /// <param name="x">The column, from 0 at the left.</param>
    /// <param name="y">The row, from 0 at the top.</param>
    /// <returns><see langword="true"/> for a dark module; <see langword="false"/> for a light one or a position outside the grid.</returns>
    public bool this[int x, int y] => x >= 0 && y >= 0 && x < Size && y < Size && _modules[(y * Size) + x];

    /// <summary>
    /// Gets whether a position lies in one of the three finder patterns (the large squares in the corners),
    /// including the light separator around each. Renderers can use this to draw the corners in their own style.
    /// </summary>
    /// <param name="x">The column, from 0 at the left.</param>
    /// <param name="y">The row, from 0 at the top.</param>
    /// <returns><see langword="true"/> when the position belongs to a finder pattern.</returns>
    public bool IsFinderPattern(int x, int y)
    {
        if (x < 0 || y < 0 || x >= Size || y >= Size)
        {
            return false;
        }

        var nearLeft = x < 8;
        var nearTop = y < 8;
        return (nearLeft && nearTop) || (x >= Size - 8 && nearTop) || (nearLeft && y >= Size - 8);
    }
}
