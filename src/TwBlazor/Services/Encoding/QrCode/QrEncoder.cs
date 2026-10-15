// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using TwBlazor.Enums;

namespace TwBlazor.Services.Encoding;

/// <summary>
/// Turns segments of data into a <see cref="QrMatrix"/>, following ISO/IEC 18004 (model 2 QR codes).
/// </summary>
/// <remarks>
/// The steps are: choose the smallest version that fits, write the segments and padding as codewords, split
/// them into blocks and add Reed-Solomon error correction, interleave the blocks, draw the fixed patterns,
/// lay the codewords into the grid, then apply the mask that is easiest to scan.
/// </remarks>
internal static class QrEncoder
{
    private const int penaltyRun = 3;
    private const int penaltyBox = 3;
    private const int penaltyFinderLike = 40;
    private const int penaltyBalance = 10;

    /// <summary>
    /// Encodes segments into a QR code.
    /// </summary>
    /// <param name="segments">The data, as one or more segments.</param>
    /// <param name="options">The encoding settings.</param>
    /// <returns>The encoded matrix.</returns>
    /// <exception cref="SymbolEncodingException">The data does not fit in <see cref="QrEncodeOptions.MaxVersion"/>.</exception>
    internal static QrMatrix Encode(IReadOnlyList<QrSegment> segments, QrEncodeOptions options)
    {
        Validate(options);

        var level = options.ErrorCorrection;
        var version = options.MinVersion;
        var usedBits = QrSegment.GetTotalBits(segments, version);
        while (usedBits == -1 || usedBits > QrTables.GetDataCodewords(version, level) * 8)
        {
            if (version >= options.MaxVersion)
            {
                throw new SymbolEncodingException(
                    $"The data needs more space than a version {options.MaxVersion} QR code holds at {level} error correction.");
            }

            version++;
            usedBits = QrSegment.GetTotalBits(segments, version);
        }

        if (options.BoostErrorCorrection)
        {
            level = GetHighestLevelThatFits(level, version, usedBits);
        }

        var codewords = BuildCodewords(segments, version, level);
        var interleaved = AddErrorCorrectionAndInterleave(codewords, version, level);
        return Draw(interleaved, version, level, options.Mask);
    }

    private static QrErrorCorrection GetHighestLevelThatFits(QrErrorCorrection level, int version, int usedBits) =>
        Enum.GetValues<QrErrorCorrection>()
            .Where(higher => higher > level && usedBits <= QrTables.GetDataCodewords(version, higher) * 8)
            .DefaultIfEmpty(level)
            .Max();

    private static void Validate(QrEncodeOptions options)
    {
        if (options.MinVersion < QrTables.minVersion || options.MaxVersion > QrTables.maxVersion || options.MinVersion > options.MaxVersion)
        {
            throw new ArgumentOutOfRangeException(nameof(options), $"Versions must satisfy {QrTables.minVersion} <= MinVersion <= MaxVersion <= {QrTables.maxVersion}.");
        }

        if (options.Mask is < 0 or > 7)
        {
            throw new ArgumentOutOfRangeException(nameof(options), "Mask must be between 0 and 7, or null to choose automatically.");
        }

        if (!Enum.IsDefined(options.ErrorCorrection))
        {
            throw new ArgumentOutOfRangeException(nameof(options), "ErrorCorrection is not a defined level.");
        }
    }

    /// <summary>
    /// Writes the segment headers and data, then the terminator and padding up to the data capacity.
    /// </summary>
    internal static byte[] BuildCodewords(IReadOnlyList<QrSegment> segments, int version, QrErrorCorrection level)
    {
        var capacityBits = QrTables.GetDataCodewords(version, level) * 8;
        var buffer = new QrBitBuffer(capacityBits);
        foreach (var segment in segments)
        {
            buffer.Append(QrSegment.GetModeBits(segment.mode), 4);
            buffer.Append(segment.characterCount, QrSegment.GetCharacterCountBits(segment.mode, version));
            buffer.Append(segment.data);
        }

        buffer.Append(0, Math.Min(4, capacityBits - buffer.length));
        buffer.Append(0, (8 - (buffer.length & 7)) & 7);
        var pad = 0xEC;
        while (buffer.length < capacityBits)
        {
            buffer.Append(pad, 8);
            pad ^= 0xEC ^ 0x11;
        }

        return buffer.ToBytes();
    }

    /// <summary>
    /// Splits the data into blocks, adds error correction to each, and interleaves them so damage in one
    /// area is spread across many blocks.
    /// </summary>
    internal static byte[] AddErrorCorrectionAndInterleave(byte[] data, int version, QrErrorCorrection level)
    {
        var blockCount = QrTables.GetBlockCount(version, level);
        var eccLength = QrTables.GetEccCodewordsPerBlock(version, level);
        var rawCodewords = QrTables.GetRawDataModules(version) / 8;
        var shortBlockCount = blockCount - (rawCodewords % blockCount);
        var shortBlockLength = rawCodewords / blockCount;
        var divisor = ReedSolomon.GetDivisor(eccLength);

        // Every block is stored at the long length; short blocks leave one unused slot before their error correction.
        var blocks = new byte[blockCount][];
        for (int i = 0, offset = 0; i < blockCount; i++)
        {
            var dataLength = shortBlockLength - eccLength + (i < shortBlockCount ? 0 : 1);
            var block = new byte[shortBlockLength + 1];
            data.AsSpan(offset, dataLength).CopyTo(block);
            ReedSolomon.ComputeRemainder(data.AsSpan(offset, dataLength), divisor, block.AsSpan(block.Length - eccLength));
            offset += dataLength;
            blocks[i] = block;
        }

        var result = new byte[rawCodewords];
        var index = 0;
        for (var i = 0; i < shortBlockLength + 1; i++)
        {
            for (var j = 0; j < blockCount; j++)
            {
                if (i != shortBlockLength - eccLength || j >= shortBlockCount)
                {
                    result[index++] = blocks[j][i];
                }
            }
        }

        return result;
    }

    private static QrMatrix Draw(byte[] codewords, int version, QrErrorCorrection level, int? fixedMask)
    {
        var grid = new Grid(version);
        grid.DrawFunctionPatterns(level);
        grid.DrawCodewords(codewords);

        var mask = fixedMask ?? ChooseMask(grid, level);
        grid.ApplyMask(mask);
        grid.DrawFormatBits(level, mask);
        return new QrMatrix(version, level, mask, grid.modules);
    }

    private static int ChooseMask(Grid grid, QrErrorCorrection level)
    {
        var best = 0;
        var lowest = int.MaxValue;
        for (var mask = 0; mask < 8; mask++)
        {
            grid.ApplyMask(mask);
            grid.DrawFormatBits(level, mask);
            var penalty = GetPenalty(grid.modules, grid.size);
            if (penalty < lowest)
            {
                best = mask;
                lowest = penalty;
            }

            // Masking is an XOR, so applying it again undoes it.
            grid.ApplyMask(mask);
        }

        return best;
    }

    /// <summary>
    /// Scores how hard a grid is to scan: long runs, 2x2 blocks, finder-like patterns and an uneven
    /// dark/light balance all add to it. Lower is better.
    /// </summary>
    internal static int GetPenalty(bool[] modules, int size)
    {
        var result = 0;
        Span<int> history = stackalloc int[7];
        for (var line = 0; line < size; line++)
        {
            result += GetLinePenalty(modules, size, line, true, history);
            result += GetLinePenalty(modules, size, line, false, history);
        }

        return result + GetBoxPenalty(modules, size) + GetBalancePenalty(modules, size);
    }

    /// <summary>
    /// Scores the long runs and finder-like patterns along one row or column.
    /// </summary>
    private static int GetLinePenalty(bool[] modules, int size, int line, bool horizontal, Span<int> history)
    {
        var result = 0;
        var runColor = false;
        var runLength = 0;
        history.Clear();
        for (var position = 0; position < size; position++)
        {
            var dark = horizontal ? modules[(line * size) + position] : modules[(position * size) + line];
            if (dark == runColor)
            {
                runLength++;
                if (runLength == 5)
                {
                    result += penaltyRun;
                }
                else if (runLength > 5)
                {
                    result++;
                }
            }
            else
            {
                AddRunToHistory(runLength, history, size);
                if (!runColor)
                {
                    result += CountFinderLikePatterns(history) * penaltyFinderLike;
                }

                runColor = dark;
                runLength = 1;
            }
        }

        return result + (TerminateAndCountFinderLikePatterns(runColor, runLength, history, size) * penaltyFinderLike);
    }

    private static int GetBoxPenalty(bool[] modules, int size)
    {
        var result = 0;
        for (var y = 0; y < size - 1; y++)
        {
            for (var x = 0; x < size - 1; x++)
            {
                var color = modules[(y * size) + x];
                if (color == modules[(y * size) + x + 1] && color == modules[((y + 1) * size) + x] && color == modules[((y + 1) * size) + x + 1])
                {
                    result += penaltyBox;
                }
            }
        }

        return result;
    }

    private static int GetBalancePenalty(bool[] modules, int size)
    {
        var darkCount = modules.Count(module => module);
        var total = size * size;
        var k = ((Math.Abs((darkCount * 20) - (total * 10)) + total - 1) / total) - 1;
        return k * penaltyBalance;
    }

    private static int CountFinderLikePatterns(ReadOnlySpan<int> history)
    {
        var n = history[1];
        var core = n > 0 && history[2] == n && history[3] == n * 3 && history[4] == n && history[5] == n;
        return (core && history[0] >= n * 4 && history[6] >= n ? 1 : 0) + (core && history[6] >= n * 4 && history[0] >= n ? 1 : 0);
    }

    private static int TerminateAndCountFinderLikePatterns(bool runColor, int runLength, Span<int> history, int size)
    {
        if (runColor)
        {
            AddRunToHistory(runLength, history, size);
            runLength = 0;
        }

        // The quiet zone beyond the edge counts as a light run.
        runLength += size;
        AddRunToHistory(runLength, history, size);
        return CountFinderLikePatterns(history);
    }

    private static void AddRunToHistory(int runLength, Span<int> history, int size)
    {
        if (history[0] == 0)
        {
            runLength += size;
        }

        history[..^1].CopyTo(history[1..]);
        history[0] = runLength;
    }

    /// <summary>
    /// The mutable grid used while a code is being drawn.
    /// </summary>
    private sealed class Grid
    {
        private readonly bool[] _isFunction;
        private readonly int _version;

        internal Grid(int version)
        {
            _version = version;
            size = QrTables.GetSize(version);
            modules = new bool[size * size];
            _isFunction = new bool[size * size];
        }

        internal int size { get; }

        internal bool[] modules { get; }

        internal void DrawFunctionPatterns(QrErrorCorrection level)
        {
            for (var i = 0; i < size; i++)
            {
                SetFunction(6, i, (i % 2) == 0);
                SetFunction(i, 6, (i % 2) == 0);
            }

            DrawFinder(3, 3);
            DrawFinder(size - 4, 3);
            DrawFinder(3, size - 4);

            var positions = QrTables.GetAlignmentPositions(_version);
            var count = positions.Length;
            for (var i = 0; i < count; i++)
            {
                for (var j = 0; j < count; j++)
                {
                    if (!IsFinderCorner(i, j, count - 1))
                    {
                        DrawAlignment(positions[i], positions[j]);
                    }
                }
            }

            // Reserve the format areas now; the real bits are written once the mask is known.
            DrawFormatBits(level, 0);
            DrawVersionBits();
        }

        internal void DrawFormatBits(QrErrorCorrection level, int mask)
        {
            var data = (QrTables.GetFormatBits(level) << 3) | mask;
            var remainder = data;
            for (var i = 0; i < 10; i++)
            {
                remainder = (remainder << 1) ^ ((remainder >> 9) * 0x537);
            }

            var bits = ((data << 10) | remainder) ^ 0x5412;

            for (var i = 0; i <= 5; i++)
            {
                SetFunction(8, i, GetBit(bits, i));
            }

            SetFunction(8, 7, GetBit(bits, 6));
            SetFunction(8, 8, GetBit(bits, 7));
            SetFunction(7, 8, GetBit(bits, 8));
            for (var i = 9; i < 15; i++)
            {
                SetFunction(14 - i, 8, GetBit(bits, i));
            }

            for (var i = 0; i < 8; i++)
            {
                SetFunction(size - 1 - i, 8, GetBit(bits, i));
            }

            for (var i = 8; i < 15; i++)
            {
                SetFunction(8, size - 15 + i, GetBit(bits, i));
            }

            SetFunction(8, size - 8, true);
        }

        internal void DrawCodewords(byte[] codewords)
        {
            var bit = 0;
            for (var column = size - 1; column >= 1; column -= 2)
            {
                // Column 6 is the vertical timing pattern, so from there on the zigzag shifts one column left.
                var right = column <= 6 ? column - 1 : column;
                bit = DrawColumnPair(codewords, right, bit);
            }
        }

        /// <summary>
        /// Fills the two columns ending at <paramref name="right"/>, zigzagging up or down, and returns the next bit to draw.
        /// </summary>
        private int DrawColumnPair(byte[] codewords, int right, int bit)
        {
            var totalBits = codewords.Length * 8;
            var upward = ((right + 1) & 2) == 0;
            for (var vertical = 0; vertical < size; vertical++)
            {
                var y = upward ? size - 1 - vertical : vertical;
                for (var j = 0; j < 2; j++)
                {
                    var index = (y * size) + right - j;
                    if (!_isFunction[index] && bit < totalBits)
                    {
                        modules[index] = ((codewords[bit >> 3] >> (7 - (bit & 7))) & 1) != 0;
                        bit++;
                    }
                }
            }

            return bit;
        }

        internal void ApplyMask(int mask)
        {
            for (var y = 0; y < size; y++)
            {
                for (var x = 0; x < size; x++)
                {
                    var index = (y * size) + x;
                    if (!_isFunction[index] && IsMasked(mask, x, y))
                    {
                        modules[index] = !modules[index];
                    }
                }
            }
        }

        private static bool IsMasked(int mask, int x, int y) => mask switch
        {
            0 => ((x + y) % 2) == 0,
            1 => (y % 2) == 0,
            2 => (x % 3) == 0,
            3 => ((x + y) % 3) == 0,
            4 => (((x / 3) + (y / 2)) % 2) == 0,
            5 => (((x * y) % 2) + ((x * y) % 3)) == 0,
            6 => ((((x * y) % 2) + ((x * y) % 3)) % 2) == 0,
            _ => ((((x + y) % 2) + ((x * y) % 3)) % 2) == 0
        };

        private void DrawVersionBits()
        {
            if (_version < 7)
            {
                return;
            }

            var remainder = _version;
            for (var i = 0; i < 12; i++)
            {
                remainder = (remainder << 1) ^ ((remainder >> 11) * 0x1F25);
            }

            var bits = (_version << 12) | remainder;
            for (var i = 0; i < 18; i++)
            {
                var bit = GetBit(bits, i);
                var a = size - 11 + (i % 3);
                var b = i / 3;
                SetFunction(a, b, bit);
                SetFunction(b, a, bit);
            }
        }

        private void DrawFinder(int centreX, int centreY)
        {
            for (var dy = -4; dy <= 4; dy++)
            {
                for (var dx = -4; dx <= 4; dx++)
                {
                    var distance = Math.Max(Math.Abs(dx), Math.Abs(dy));
                    var x = centreX + dx;
                    var y = centreY + dy;
                    if (x >= 0 && x < size && y >= 0 && y < size)
                    {
                        SetFunction(x, y, distance != 2 && distance != 4);
                    }
                }
            }
        }

        private void DrawAlignment(int centreX, int centreY)
        {
            for (var dy = -2; dy <= 2; dy++)
            {
                for (var dx = -2; dx <= 2; dx++)
                {
                    SetFunction(centreX + dx, centreY + dy, Math.Max(Math.Abs(dx), Math.Abs(dy)) != 1);
                }
            }
        }

        private void SetFunction(int x, int y, bool dark)
        {
            var index = (y * size) + x;
            modules[index] = dark;
            _isFunction[index] = true;
        }

        private static bool GetBit(int value, int index) => ((value >> index) & 1) != 0;

        // The three corners of the alignment grid that the finder patterns already occupy.
        private static bool IsFinderCorner(int i, int j, int last) => (i == 0 && (j == 0 || j == last)) || (i == last && j == 0);
    }
}
