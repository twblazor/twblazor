// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using TwBlazor.Enums;

namespace TwBlazor.Barcoding;

/// <summary>
/// The EAN/UPC retail family (ISO/IEC 15420): EAN-13, EAN-8 and UPC-A. Each is a fixed number of digits ending
/// in a check digit, split into two halves by guard bars.
/// </summary>
internal sealed class EanUpcSymbology : IBarcodeSymbology
{
    private const string edgeGuard = "101";
    private const string centreGuard = "01010";

    // The "L" (odd parity) pattern of each digit. "R" is its inverse and "G" is "R" reversed.
    private static readonly string[] _leftOdd =
    [
        "0001101", "0011001", "0010011", "0111101", "0100011", "0110001", "0101111", "0111011", "0110111", "0001011"
    ];

    // For EAN-13, the first digit is not drawn; it picks which left-hand digits use the "G" (even parity) set.
    private static readonly string[] _firstDigitParity =
    [
        "LLLLLL", "LLGLGG", "LLGGLG", "LLGGGL", "LGLLGG", "LGGLLG", "LGGGLG", "LGLGLG", "LGLGGL", "LGGLGL"
    ];

    /// <summary>
    /// Initializes a new instance of the <see cref="EanUpcSymbology"/> class.
    /// </summary>
    /// <param name="format">One of <see cref="BarcodeFormat.Ean13"/>, <see cref="BarcodeFormat.Ean8"/> or <see cref="BarcodeFormat.UpcA"/>.</param>
    internal EanUpcSymbology(BarcodeFormat format)
    {
        Format = format;
        length = format switch
        {
            BarcodeFormat.Ean13 => 13,
            BarcodeFormat.Ean8 => 8,
            BarcodeFormat.UpcA => 12,
            _ => throw new ArgumentOutOfRangeException(nameof(format), format, "Not an EAN or UPC format.")
        };
    }

    /// <inheritdoc />
    public BarcodeFormat Format { get; }

    private int length { get; }

    /// <summary>
    /// Works out the check digit for the data digits of an EAN or UPC code.
    /// </summary>
    /// <param name="digits">The digits without the check digit.</param>
    /// <returns>The check digit, from 0 to 9.</returns>
    internal static int GetCheckDigit(ReadOnlySpan<char> digits)
    {
        var sum = 0;
        for (var i = 0; i < digits.Length; i++)
        {
            // Weights alternate 3, 1, 3... starting from the rightmost data digit.
            var weight = ((digits.Length - i) % 2) == 1 ? 3 : 1;
            sum += (digits[i] - '0') * weight;
        }

        return (10 - (sum % 10)) % 10;
    }

    /// <inheritdoc />
    public Barcode Encode(string value)
    {
        var name = Format switch
        {
            BarcodeFormat.Ean13 => "EAN-13",
            BarcodeFormat.Ean8 => "EAN-8",
            _ => "UPC-A"
        };

        if (!value.All(char.IsAsciiDigit) || (value.Length != length && value.Length != length - 1))
        {
            throw new BarcodeValueException($"{name} takes {length - 1} digits, or {length} digits including the check digit.");
        }

        var check = (char)('0' + GetCheckDigit(value.AsSpan(0, length - 1)));
        if (value.Length == length && value[^1] != check)
        {
            throw new BarcodeValueException($"The check digit of {value} should be {check}.");
        }

        var digits = value[..(length - 1)] + check;

        // UPC-A is drawn exactly like an EAN-13 whose first digit is 0.
        var drawn = Format == BarcodeFormat.UpcA ? "0" + digits : digits;
        var parity = drawn.Length == 13 ? _firstDigitParity[drawn[0] - '0'] : new string('L', 4);
        var left = drawn.Length == 13 ? drawn.Substring(1, 6) : drawn[..4];
        var right = drawn.Length == 13 ? drawn[7..] : drawn[4..];

        var writer = new BarcodeModuleWriter((drawn.Length * 7) + 11);
        writer.AppendBits(edgeGuard);
        for (var i = 0; i < left.Length; i++)
        {
            var odd = _leftOdd[left[i] - '0'];
            writer.AppendBits(parity[i] == 'L' ? odd : new string(Invert(odd).Reverse().ToArray()));
        }

        writer.AppendBits(centreGuard);
        foreach (var digit in right)
        {
            writer.AppendBits(Invert(_leftOdd[digit - '0']));
        }

        writer.AppendBits(edgeGuard);
        return new Barcode(Format, digits, writer.ToArray(), Format == BarcodeFormat.Ean8 ? 7 : 11);
    }

    private static string Invert(string bits) => string.Concat(bits.Select(bit => bit == '1' ? '0' : '1'));
}
