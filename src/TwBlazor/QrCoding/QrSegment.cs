// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using System.Text;

namespace TwBlazor.QrCoding;

/// <summary>
/// The ways a run of data can be packed into a QR code. Denser modes fit more characters in the same code.
/// </summary>
internal enum QrSegmentMode
{
    /// <summary>
    /// Digits 0-9 only, about 3.3 bits per character.
    /// </summary>
    Numeric,

    /// <summary>
    /// Digits, upper case A-Z, space and $%*+-./: only, 5.5 bits per character.
    /// </summary>
    Alphanumeric,

    /// <summary>
    /// Any bytes, 8 bits each. Text is stored as UTF-8.
    /// </summary>
    Byte
}

/// <summary>
/// A run of data in one <see cref="QrSegmentMode"/>, already packed into bits.
/// </summary>
internal sealed class QrSegment
{
    private const string alphanumericCharset = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ $%*+-./:";

    private QrSegment(QrSegmentMode mode, int characterCount, QrBitBuffer data)
    {
        this.mode = mode;
        this.characterCount = characterCount;
        this.data = data;
    }

    /// <summary>
    /// Gets the packing mode.
    /// </summary>
    internal QrSegmentMode mode { get; }

    /// <summary>
    /// Gets the number of characters (or bytes, in <see cref="QrSegmentMode.Byte"/>) the segment holds.
    /// </summary>
    internal int characterCount { get; }

    /// <summary>
    /// Gets the packed data bits, without the mode indicator or character count.
    /// </summary>
    internal QrBitBuffer data { get; }

    /// <summary>
    /// Builds the segments for some text, picking the densest single mode that can hold all of it.
    /// </summary>
    /// <param name="text">The text to encode.</param>
    /// <returns>The segments; empty for empty text.</returns>
    internal static IReadOnlyList<QrSegment> FromText(string text)
    {
        if (text.Length == 0)
        {
            return [];
        }

        if (IsNumeric(text))
        {
            return [MakeNumeric(text)];
        }

        if (IsAlphanumeric(text))
        {
            return [MakeAlphanumeric(text)];
        }

        return [MakeBytes(Encoding.UTF8.GetBytes(text))];
    }

    /// <summary>
    /// Gets whether every character is a digit 0-9.
    /// </summary>
    /// <param name="text">The text to check.</param>
    /// <returns><see langword="true"/> when the text can use <see cref="QrSegmentMode.Numeric"/>.</returns>
    internal static bool IsNumeric(string text) => text.All(char.IsAsciiDigit);

    /// <summary>
    /// Gets whether every character is in the alphanumeric set.
    /// </summary>
    /// <param name="text">The text to check.</param>
    /// <returns><see langword="true"/> when the text can use <see cref="QrSegmentMode.Alphanumeric"/>.</returns>
    internal static bool IsAlphanumeric(string text) => text.All(alphanumericCharset.Contains);

    /// <summary>
    /// Packs a string of digits, three digits to every 10 bits.
    /// </summary>
    /// <param name="digits">The digits to pack.</param>
    /// <returns>The numeric segment.</returns>
    internal static QrSegment MakeNumeric(string digits)
    {
        if (!IsNumeric(digits))
        {
            throw new ArgumentException("Numeric mode only holds the digits 0-9.", nameof(digits));
        }

        var buffer = new QrBitBuffer((digits.Length * 10 / 3) + 4);
        for (var i = 0; i < digits.Length; i += 3)
        {
            var count = Math.Min(3, digits.Length - i);
            buffer.Append(int.Parse(digits.AsSpan(i, count), provider: System.Globalization.CultureInfo.InvariantCulture), (count * 3) + 1);
        }

        return new QrSegment(QrSegmentMode.Numeric, digits.Length, buffer);
    }

    /// <summary>
    /// Packs alphanumeric text, two characters to every 11 bits.
    /// </summary>
    /// <param name="text">The text to pack.</param>
    /// <returns>The alphanumeric segment.</returns>
    internal static QrSegment MakeAlphanumeric(string text)
    {
        if (!IsAlphanumeric(text))
        {
            throw new ArgumentException("Alphanumeric mode only holds 0-9, A-Z, space and $%*+-./:.", nameof(text));
        }

        var buffer = new QrBitBuffer((text.Length * 11 / 2) + 6);
        var i = 0;
        for (; i + 1 < text.Length; i += 2)
        {
            buffer.Append((alphanumericCharset.IndexOf(text[i]) * 45) + alphanumericCharset.IndexOf(text[i + 1]), 11);
        }

        if (i < text.Length)
        {
            buffer.Append(alphanumericCharset.IndexOf(text[i]), 6);
        }

        return new QrSegment(QrSegmentMode.Alphanumeric, text.Length, buffer);
    }

    /// <summary>
    /// Packs raw bytes, eight bits each.
    /// </summary>
    /// <param name="data">The bytes to pack.</param>
    /// <returns>The byte segment.</returns>
    internal static QrSegment MakeBytes(ReadOnlySpan<byte> data)
    {
        var buffer = new QrBitBuffer(data.Length * 8);
        foreach (var b in data)
        {
            buffer.Append(b, 8);
        }

        return new QrSegment(QrSegmentMode.Byte, data.Length, buffer);
    }

    /// <summary>
    /// Gets the 4-bit mode indicator written before the segment.
    /// </summary>
    /// <param name="mode">The packing mode.</param>
    /// <returns>The mode indicator.</returns>
    internal static int GetModeBits(QrSegmentMode mode) => mode switch
    {
        QrSegmentMode.Numeric => 0x1,
        QrSegmentMode.Alphanumeric => 0x2,
        _ => 0x4
    };

    /// <summary>
    /// Gets how many bits the character count takes, which grows with the version.
    /// </summary>
    /// <param name="mode">The packing mode.</param>
    /// <param name="version">The version, from 1 to 40.</param>
    /// <returns>The width of the character count field.</returns>
    internal static int GetCharacterCountBits(QrSegmentMode mode, int version)
    {
        var band = version switch
        {
            <= 9 => 0,
            <= 26 => 1,
            _ => 2
        };
        return (mode, band) switch
        {
            (QrSegmentMode.Numeric, _) => 10 + (band * 2),
            (QrSegmentMode.Alphanumeric, _) => 9 + (band * 2),
            (_, 0) => 8,
            _ => 16
        };
    }

    /// <summary>
    /// Gets the total bits a list of segments needs at a version, including headers.
    /// </summary>
    /// <param name="segments">The segments.</param>
    /// <param name="version">The version, from 1 to 40.</param>
    /// <returns>The bit count, or -1 when a segment has too many characters for its count field.</returns>
    internal static int GetTotalBits(IReadOnlyList<QrSegment> segments, int version)
    {
        long total = 0;
        foreach (var segment in segments)
        {
            var countBits = GetCharacterCountBits(segment.mode, version);
            if (segment.characterCount >= (1 << countBits))
            {
                return -1;
            }

            total += 4L + countBits + segment.data.length;
            if (total > int.MaxValue)
            {
                return -1;
            }
        }

        return (int)total;
    }
}
