// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

namespace TwBlazor.Services.Encoding;

/// <summary>
/// Reed-Solomon error correction over GF(2^8) with the QR code polynomial
/// x^8 + x^4 + x^3 + x^2 + 1 (0x11D). These are the recovery codewords that let a scanner rebuild a
/// damaged or covered part of the code.
/// </summary>
internal static class ReedSolomon
{
    private static readonly byte[] _exp = new byte[512];
    private static readonly byte[] _log = new byte[256];
    private static readonly byte[]?[] _divisors = new byte[]?[maxDegree + 1];

    private const int maxDegree = 30;

    static ReedSolomon()
    {
        var value = 1;
        for (var i = 0; i < 255; i++)
        {
            _exp[i] = (byte)value;
            _log[value] = (byte)i;
            value <<= 1;
            if (value > 0xFF)
            {
                value ^= 0x11D;
            }
        }

        // Doubling the table lets Multiply skip the modulo 255.
        for (var i = 255; i < _exp.Length; i++)
        {
            _exp[i] = _exp[i - 255];
        }
    }

    /// <summary>
    /// Multiplies two field elements.
    /// </summary>
    /// <param name="x">The first element.</param>
    /// <param name="y">The second element.</param>
    /// <returns>The product in GF(2^8).</returns>
    internal static byte Multiply(byte x, byte y) => x == 0 || y == 0 ? (byte)0 : _exp[_log[x] + _log[y]];

    /// <summary>
    /// Gets the generator polynomial of a given degree, leading term omitted, highest power first.
    /// Divisors are cached because only a handful of degrees are ever used.
    /// </summary>
    /// <param name="degree">The number of error correction codewords per block, from 1 to 30.</param>
    /// <returns>The divisor coefficients.</returns>
    internal static byte[] GetDivisor(int degree)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(degree, 1);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(degree, maxDegree);

        var cached = Volatile.Read(ref _divisors[degree]);
        if (cached is not null)
        {
            return cached;
        }

        var result = new byte[degree];
        result[degree - 1] = 1;
        byte root = 1;
        for (var i = 0; i < degree; i++)
        {
            for (var j = 0; j < degree; j++)
            {
                result[j] = Multiply(result[j], root);
                if (j + 1 < degree)
                {
                    result[j] ^= result[j + 1];
                }
            }

            root = Multiply(root, 0x02);
        }

        Volatile.Write(ref _divisors[degree], result);
        return result;
    }

    /// <summary>
    /// Computes the error correction codewords for one block of data.
    /// </summary>
    /// <param name="data">The data codewords of the block.</param>
    /// <param name="divisor">The generator polynomial from <see cref="GetDivisor"/>.</param>
    /// <param name="destination">Receives the remainder; must be as long as <paramref name="divisor"/>.</param>
    internal static void ComputeRemainder(ReadOnlySpan<byte> data, ReadOnlySpan<byte> divisor, Span<byte> destination)
    {
        destination.Clear();
        foreach (var b in data)
        {
            var factor = (byte)(b ^ destination[0]);
            destination[1..].CopyTo(destination);
            destination[^1] = 0;
            for (var i = 0; i < divisor.Length; i++)
            {
                destination[i] ^= Multiply(divisor[i], factor);
            }
        }
    }
}
