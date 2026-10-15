// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using TwBlazor.Enums;

namespace TwBlazor.QrCoding;

/// <summary>
/// The fixed capacity tables and layout formulas from the QR code standard (ISO/IEC 18004).
/// </summary>
internal static class QrTables
{
    /// <summary>
    /// The smallest QR code version (21x21 modules).
    /// </summary>
    internal const int minVersion = 1;

    /// <summary>
    /// The largest QR code version (177x177 modules).
    /// </summary>
    internal const int maxVersion = 40;

    // Indexed [level][version]; index 0 is unused so a version can be used directly.
    private static readonly int[][] _eccCodewordsPerBlock =
    [
        [-1, 7, 10, 15, 20, 26, 18, 20, 24, 30, 18, 20, 24, 26, 30, 22, 24, 28, 30, 28, 28, 28, 28, 30, 30, 26, 28, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30],
        [-1, 10, 16, 26, 18, 24, 16, 18, 22, 22, 26, 30, 22, 22, 24, 24, 28, 28, 26, 26, 26, 26, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28],
        [-1, 13, 22, 18, 26, 18, 24, 18, 22, 20, 24, 28, 26, 24, 20, 30, 24, 28, 28, 26, 30, 28, 30, 30, 30, 30, 28, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30],
        [-1, 17, 28, 22, 16, 22, 28, 26, 26, 24, 28, 24, 28, 22, 24, 24, 30, 28, 28, 26, 28, 30, 24, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30]
    ];

    private static readonly int[][] _errorCorrectionBlocks =
    [
        [-1, 1, 1, 1, 1, 1, 2, 2, 2, 2, 4, 4, 4, 4, 4, 6, 6, 6, 6, 7, 8, 8, 9, 9, 10, 12, 12, 12, 13, 14, 15, 16, 17, 18, 19, 19, 20, 21, 22, 24, 25],
        [-1, 1, 1, 1, 2, 2, 4, 4, 4, 5, 5, 5, 8, 9, 9, 10, 10, 11, 13, 14, 16, 17, 17, 18, 20, 21, 23, 25, 26, 28, 29, 31, 33, 35, 37, 38, 40, 43, 45, 47, 49],
        [-1, 1, 1, 2, 2, 4, 4, 6, 6, 8, 8, 8, 10, 12, 16, 12, 17, 16, 18, 21, 20, 23, 23, 25, 27, 29, 34, 34, 35, 38, 40, 43, 45, 48, 51, 53, 56, 59, 62, 65, 68],
        [-1, 1, 1, 2, 4, 4, 4, 5, 6, 8, 8, 11, 11, 16, 16, 18, 16, 19, 21, 25, 25, 25, 34, 30, 32, 35, 37, 40, 42, 45, 48, 51, 54, 57, 60, 63, 66, 70, 74, 77, 81]
    ];

    /// <summary>
    /// Gets the width and height, in modules, of a version.
    /// </summary>
    /// <param name="version">The version, from 1 to 40.</param>
    /// <returns>The number of modules along one side.</returns>
    internal static int GetSize(int version) => (version * 4) + 17;

    /// <summary>
    /// Gets the number of error correction codewords added to each block.
    /// </summary>
    /// <param name="version">The version, from 1 to 40.</param>
    /// <param name="level">The error correction level.</param>
    /// <returns>The error correction codewords per block.</returns>
    internal static int GetEccCodewordsPerBlock(int version, QrErrorCorrection level) => _eccCodewordsPerBlock[(int)level][version];

    /// <summary>
    /// Gets the number of blocks the data is split into before error correction is added.
    /// </summary>
    /// <param name="version">The version, from 1 to 40.</param>
    /// <param name="level">The error correction level.</param>
    /// <returns>The number of blocks.</returns>
    internal static int GetBlockCount(int version, QrErrorCorrection level) => _errorCorrectionBlocks[(int)level][version];

    /// <summary>
    /// Gets the number of modules left for data and error correction once the fixed patterns are drawn.
    /// </summary>
    /// <param name="version">The version, from 1 to 40.</param>
    /// <returns>The number of data modules, including any remainder bits.</returns>
    internal static int GetRawDataModules(int version)
    {
        var result = (((16 * version) + 128) * version) + 64;
        if (version >= 2)
        {
            var alignmentCount = (version / 7) + 2;
            result -= (((25 * alignmentCount) - 10) * alignmentCount) - 55;
            if (version >= 7)
            {
                result -= 36;
            }
        }

        return result;
    }

    /// <summary>
    /// Gets the number of 8-bit data codewords a version holds at an error correction level.
    /// </summary>
    /// <param name="version">The version, from 1 to 40.</param>
    /// <param name="level">The error correction level.</param>
    /// <returns>The data capacity in codewords.</returns>
    internal static int GetDataCodewords(int version, QrErrorCorrection level) =>
        (GetRawDataModules(version) / 8) - (GetEccCodewordsPerBlock(version, level) * GetBlockCount(version, level));

    /// <summary>
    /// Gets the centre coordinates of the alignment patterns, used for both rows and columns.
    /// </summary>
    /// <param name="version">The version, from 1 to 40.</param>
    /// <returns>The ascending centre positions, or an empty array for version 1.</returns>
    internal static int[] GetAlignmentPositions(int version)
    {
        if (version == 1)
        {
            return [];
        }

        var count = (version / 7) + 2;
        var step = (((version * 8) + (count * 3) + 5) / ((count * 4) - 4)) * 2;
        var result = new int[count];
        result[0] = 6;
        for (int i = count - 1, position = GetSize(version) - 7; i >= 1; i--, position -= step)
        {
            result[i] = position;
        }

        return result;
    }

    /// <summary>
    /// Gets the two bits the format information uses to identify an error correction level.
    /// </summary>
    /// <param name="level">The error correction level.</param>
    /// <returns>The format bits for the level.</returns>
    internal static int GetFormatBits(QrErrorCorrection level) => level switch
    {
        QrErrorCorrection.Low => 1,
        QrErrorCorrection.Medium => 0,
        QrErrorCorrection.Quartile => 3,
        _ => 2
    };
}
