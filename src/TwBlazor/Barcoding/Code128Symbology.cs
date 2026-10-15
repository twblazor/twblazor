// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using TwBlazor.Enums;

namespace TwBlazor.Barcoding;

/// <summary>
/// Code 128 (ISO/IEC 15417). Encodes ASCII 0-127, switching between code set A (control characters and upper
/// case), B (printable text) and C (two digits per symbol) to keep the barcode short.
/// </summary>
internal sealed class Code128Symbology : IBarcodeSymbology
{
    /// <summary>
    /// The symbol value that starts in code set A.
    /// </summary>
    internal const int startA = 103;

    /// <summary>
    /// The symbol value that starts in code set B.
    /// </summary>
    internal const int startB = 104;

    /// <summary>
    /// The symbol value that starts in code set C.
    /// </summary>
    internal const int startC = 105;

    /// <summary>
    /// The symbol value of the stop pattern.
    /// </summary>
    internal const int stop = 106;

    private const int switchToC = 99;
    private const int switchToB = 100;
    private const int switchToA = 101;

    // Bar and space widths for each symbol value, in modules: bar, space, bar, space, bar, space (and a final bar for stop).
    private static readonly string[] _patterns =
    [
        "212222", "222122", "222221", "121223", "121322", "131222", "122213", "122312", "132212", "221213",
        "221312", "231212", "112232", "122132", "122231", "113222", "123122", "123221", "223211", "221132",
        "221231", "213212", "223112", "312131", "311222", "321122", "321221", "312212", "322112", "322211",
        "212123", "212321", "232121", "111323", "131123", "131321", "112313", "132113", "132311", "211313",
        "231113", "231311", "112133", "112331", "132131", "113123", "113321", "133121", "313121", "211331",
        "231131", "213113", "213311", "213131", "311123", "311321", "331121", "312113", "312311", "332111",
        "314111", "221411", "431111", "111224", "111422", "121124", "121421", "141122", "141221", "112214",
        "112412", "122114", "122411", "142112", "142211", "241211", "221114", "413111", "241112", "134111",
        "111242", "121142", "121241", "114212", "124112", "124211", "411212", "421112", "421211", "212141",
        "214121", "412121", "111143", "111341", "131141", "114113", "114311", "411113", "411311", "113141",
        "114131", "311141", "411131", "211412", "211214", "211232", "2331112"
    ];

    private enum CodeSet
    {
        A,
        B,
        C
    }

    /// <inheritdoc />
    public BarcodeFormat Format => BarcodeFormat.Code128;

    /// <summary>
    /// Gets the bar and space widths of a symbol value.
    /// </summary>
    /// <param name="value">The symbol value, from 0 to 106.</param>
    /// <returns>The widths, in modules.</returns>
    internal static string GetPattern(int value) => _patterns[value];

    /// <inheritdoc />
    public Barcode Encode(string value)
    {
        var symbols = GetSymbols(value);
        var writer = new BarcodeModuleWriter((symbols.Count * 11) + 2);
        foreach (var symbol in symbols)
        {
            writer.AppendWidths(_patterns[symbol]);
        }

        var text = string.Concat(value.Where(c => !char.IsControl(c)));
        return new Barcode(Format, text, writer.ToArray(), 10);
    }

    /// <summary>
    /// Works out the symbol values for a value: start, data, code set switches, check symbol and stop.
    /// </summary>
    /// <param name="value">The ASCII text to encode.</param>
    /// <returns>The symbol values in order.</returns>
    /// <exception cref="BarcodeValueException">The value is empty or holds a character outside ASCII.</exception>
    internal static List<int> GetSymbols(string value)
    {
        if (value.Length == 0)
        {
            throw new BarcodeValueException("Code 128 needs at least one character.");
        }

        foreach (var c in value)
        {
            if (c > 127)
            {
                throw new BarcodeValueException($"Code 128 only holds ASCII characters; '{c}' is not one.");
            }
        }

        var symbols = new List<int>(value.Length + 4);
        var leadingDigits = CountDigits(value, 0);
        var set = leadingDigits >= 4 || (leadingDigits == value.Length && leadingDigits >= 2)
            ? CodeSet.C
            : IsControl(value[0]) ? CodeSet.A : CodeSet.B;
        symbols.Add(set switch { CodeSet.A => startA, CodeSet.B => startB, _ => startC });

        var i = 0;
        while (i < value.Length)
        {
            if (set == CodeSet.C)
            {
                if (CountDigits(value, i) >= 2)
                {
                    symbols.Add(((value[i] - '0') * 10) + (value[i + 1] - '0'));
                    i += 2;
                    continue;
                }

                set = IsControl(value[i]) ? CodeSet.A : CodeSet.B;
                symbols.Add(set == CodeSet.A ? switchToA : switchToB);
                continue;
            }

            var digits = CountDigits(value, i);
            if (digits >= 4)
            {
                // An odd run leaves its first digit in the current set so the rest pair up in set C.
                if ((digits % 2) == 1)
                {
                    symbols.Add(GetValue(set, value[i]));
                    i++;
                }

                set = CodeSet.C;
                symbols.Add(switchToC);
                continue;
            }

            var c = value[i];
            if (!IsInSet(set, c))
            {
                set = set == CodeSet.A ? CodeSet.B : CodeSet.A;
                symbols.Add(set == CodeSet.A ? switchToA : switchToB);
            }

            symbols.Add(GetValue(set, c));
            i++;
        }

        var checksum = symbols[0];
        for (var position = 1; position < symbols.Count; position++)
        {
            checksum += symbols[position] * position;
        }

        symbols.Add(checksum % 103);
        symbols.Add(stop);
        return symbols;
    }

    private static int CountDigits(string value, int start)
    {
        var end = start;
        while (end < value.Length && char.IsAsciiDigit(value[end]))
        {
            end++;
        }

        return end - start;
    }

    private static bool IsControl(char c) => c < 32;

    private static bool IsInSet(CodeSet set, char c) => set == CodeSet.A ? c < 96 : c >= 32;

    private static int GetValue(CodeSet set, char c) => set == CodeSet.A && c < 32 ? c + 64 : c - 32;
}
