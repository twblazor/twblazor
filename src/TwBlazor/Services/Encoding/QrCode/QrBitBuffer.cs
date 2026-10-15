// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

namespace TwBlazor.Services.Encoding;

/// <summary>
/// An append-only sequence of bits, packed most significant bit first.
/// </summary>
internal sealed class QrBitBuffer
{
    private byte[] bytes;

    /// <summary>
    /// Initializes a new instance of the <see cref="QrBitBuffer"/> class.
    /// </summary>
    /// <param name="capacityBits">The number of bits to reserve space for.</param>
    internal QrBitBuffer(int capacityBits = 64)
    {
        bytes = new byte[Math.Max(1, (capacityBits + 7) / 8)];
    }

    /// <summary>
    /// Gets the number of bits appended so far.
    /// </summary>
    internal int length { get; private set; }

    /// <summary>
    /// Gets the bit at a position.
    /// </summary>
    /// <param name="index">The zero-based bit position.</param>
    /// <returns><see langword="true"/> when the bit is set.</returns>
    internal bool this[int index]
    {
        get
        {
            ArgumentOutOfRangeException.ThrowIfNegative(index);
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, length);
            return ((bytes[index >> 3] >> (7 - (index & 7))) & 1) != 0;
        }
    }

    /// <summary>
    /// Appends the lowest <paramref name="count"/> bits of a value, most significant first.
    /// </summary>
    /// <param name="value">The value whose low bits are appended.</param>
    /// <param name="count">The number of bits, from 0 to 31.</param>
    internal void Append(int value, int count)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(count);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(count, 31);
        if ((value >>> count) != 0)
        {
            throw new ArgumentOutOfRangeException(nameof(value), "The value does not fit in the given number of bits.");
        }

        EnsureCapacity(length + count);
        for (var i = count - 1; i >= 0; i--)
        {
            if (((value >> i) & 1) != 0)
            {
                bytes[length >> 3] |= (byte)(0x80 >> (length & 7));
            }

            length++;
        }
    }

    /// <summary>
    /// Appends every bit of another buffer.
    /// </summary>
    /// <param name="other">The buffer to copy.</param>
    internal void Append(QrBitBuffer other)
    {
        EnsureCapacity(length + other.length);
        for (var i = 0; i < other.length; i++)
        {
            Append(other[i] ? 1 : 0, 1);
        }
    }

    /// <summary>
    /// Copies the bits into bytes. The length must be a whole number of bytes.
    /// </summary>
    /// <returns>The packed bytes.</returns>
    internal byte[] ToBytes()
    {
        if ((length & 7) != 0)
        {
            throw new InvalidOperationException("The buffer does not hold a whole number of bytes.");
        }

        return bytes.AsSpan(0, length >> 3).ToArray();
    }

    private void EnsureCapacity(int bits)
    {
        var needed = (bits + 7) / 8;
        if (needed > bytes.Length)
        {
            Array.Resize(ref bytes, Math.Max(needed, bytes.Length * 2));
        }
    }
}
