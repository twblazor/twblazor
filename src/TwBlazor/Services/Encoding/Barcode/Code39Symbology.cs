// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using System.Buffers;
using TwBlazor.Enums;

namespace TwBlazor.Services.Encoding;

/// <summary>
/// Code 39 (ISO/IEC 16388). Each character is five bars and four spaces, three of them wide, framed by
/// <c>*</c> start and stop characters and separated by a narrow gap.
/// </summary>
internal sealed class Code39Symbology : IBarcodeSymbology
{
    /// <summary>
    /// The width of a wide element, in modules. The standard allows 2 to 3 times the narrow width; 3 reads best.
    /// </summary>
    internal const int wideWidth = 3;

    // Element widths for each character, bar first: 1 for wide, 0 for narrow.
    private static readonly Dictionary<char, string> _patterns = new()
    {
        ['0'] = "000110100", ['1'] = "100100001", ['2'] = "001100001", ['3'] = "101100000", ['4'] = "000110001",
        ['5'] = "100110000", ['6'] = "001110000", ['7'] = "000100101", ['8'] = "100100100", ['9'] = "001100100",
        ['A'] = "100001001", ['B'] = "001001001", ['C'] = "101001000", ['D'] = "000011001", ['E'] = "100011000",
        ['F'] = "001011000", ['G'] = "000001101", ['H'] = "100001100", ['I'] = "001001100", ['J'] = "000011100",
        ['K'] = "100000011", ['L'] = "001000011", ['M'] = "101000010", ['N'] = "000010011", ['O'] = "100010010",
        ['P'] = "001010010", ['Q'] = "000000111", ['R'] = "100000110", ['S'] = "001000110", ['T'] = "000010110",
        ['U'] = "110000001", ['V'] = "011000001", ['W'] = "111000000", ['X'] = "010010001", ['Y'] = "110010000",
        ['Z'] = "011010000", ['-'] = "010000101", ['.'] = "110000100", [' '] = "011000100", ['$'] = "010101000",
        ['/'] = "010100010", ['+'] = "010001010", ['%'] = "000101010", ['*'] = "010010100",
    };

    // Every character a value may hold; "*" only frames the barcode.
    private static readonly SearchValues<char> _dataCharacters = SearchValues.Create("0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ-. $/+%");

    /// <inheritdoc />
    public BarcodeFormat Format => BarcodeFormat.Code39;

    /// <summary>
    /// Gets the narrow (0) and wide (1) pattern of a character.
    /// </summary>
    /// <param name="c">The character, including the <c>*</c> start and stop character.</param>
    /// <returns>The pattern, or <see langword="null"/> when Code 39 cannot hold the character.</returns>
    internal static string? GetPattern(char c) => _patterns.GetValueOrDefault(c);

    /// <inheritdoc />
    public Barcode Encode(string value)
    {
        if (value.Length == 0)
        {
            throw new SymbolEncodingException("Code 39 needs at least one character.");
        }

        var invalid = value.AsSpan().IndexOfAnyExcept(_dataCharacters);
        if (invalid >= 0)
        {
            throw new SymbolEncodingException($"Code 39 only holds A-Z, 0-9, space and -.$/+%; '{value[invalid]}' is not one of them.");
        }

        var writer = new BarcodeModuleWriter((value.Length + 2) * 16);
        var framed = $"*{value}*";
        for (var i = 0; i < framed.Length; i++)
        {
            if (i > 0)
            {
                writer.AppendBits("0");
            }

            writer.AppendWidths(string.Concat(_patterns[framed[i]].Select(element => element == '1' ? (char)('0' + wideWidth) : '1')));
        }

        return new Barcode(Format, value, writer.ToArray(), 10);
    }
}
