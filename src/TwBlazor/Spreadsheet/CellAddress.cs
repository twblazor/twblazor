// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using System.Globalization;
using System.Text;

namespace TwBlazor.Spreadsheet;

/// <summary>
/// The position of a cell on a sheet, counted from zero: row 0, column 0 is <c>A1</c>.
/// </summary>
/// <param name="Row">The zero based row.</param>
/// <param name="Column">The zero based column.</param>
internal readonly record struct CellAddress(int Row, int Column)
{
    /// <summary>
    /// The largest column a reference can name, <c>XFD</c>, the same limit as Excel.
    /// </summary>
    public const int MaxColumns = 16384;

    /// <summary>
    /// The largest row a reference can name, the same limit as Excel.
    /// </summary>
    public const int MaxRows = 1048576;

    /// <summary>
    /// Gets the address in A1 notation, such as <c>C12</c>.
    /// </summary>
    /// <returns>The address.</returns>
    public override string ToString() => ColumnName(Column) + (Row + 1).ToString(CultureInfo.InvariantCulture);

    /// <summary>
    /// Moves the address by a number of rows and columns.
    /// </summary>
    /// <param name="rows">The rows to move down, or up when negative.</param>
    /// <param name="columns">The columns to move right, or left when negative.</param>
    /// <returns>The moved address.</returns>
    public CellAddress Offset(int rows, int columns) => new(Row + rows, Column + columns);

    /// <summary>
    /// Turns a zero based column index into its letters: 0 is A, 25 is Z and 26 is AA.
    /// </summary>
    /// <param name="column">The zero based column.</param>
    /// <returns>The column letters.</returns>
    public static string ColumnName(int column)
    {
        var letters = new StringBuilder();
        for (var n = column + 1; n > 0; n = (n - 1) / 26)
        {
            letters.Insert(0, (char)('A' + ((n - 1) % 26)));
        }

        return letters.ToString();
    }

    /// <summary>
    /// Reads an address in A1 notation, such as <c>b3</c> or <c>$B$3</c>, ignoring case and dollar signs.
    /// </summary>
    /// <param name="text">The text to read.</param>
    /// <param name="address">The address, when the text is one.</param>
    /// <returns><see langword="true"/> when the text is a whole, valid address.</returns>
    public static bool TryParse(ReadOnlySpan<char> text, out CellAddress address)
    {
        address = default;
        var i = 0;
        if (i < text.Length && text[i] == '$')
        {
            i++;
        }

        var column = 0;
        var letters = 0;
        while (i < text.Length && char.IsAsciiLetter(text[i]))
        {
            column = (column * 26) + (char.ToUpperInvariant(text[i]) - 'A' + 1);
            letters++;
            i++;
            if (letters > 3)
            {
                return false;
            }
        }

        if (letters == 0)
        {
            return false;
        }

        if (i < text.Length && text[i] == '$')
        {
            i++;
        }

        var digits = text[i..];
        if (digits.Length == 0 || digits[0] == '0' || !int.TryParse(digits, NumberStyles.None, CultureInfo.InvariantCulture, out var row))
        {
            return false;
        }

        if (column > MaxColumns || row > MaxRows)
        {
            return false;
        }

        address = new CellAddress(row - 1, column - 1);
        return true;
    }

    /// <summary>
    /// Reads an address in A1 notation.
    /// </summary>
    /// <param name="text">The text to read.</param>
    /// <returns>The address.</returns>
    /// <exception cref="FormatException">The text is not an address.</exception>
    public static CellAddress Parse(string text) =>
        TryParse(text, out var address) ? address : throw new FormatException($"'{text}' is not a cell address.");
}

/// <summary>
/// A rectangle of cells, kept with <see cref="Start"/> at its top left and <see cref="End"/> at its bottom right.
/// </summary>
internal readonly record struct CellRange
{
    /// <summary>
    /// Creates a range between two corners given in any order.
    /// </summary>
    /// <param name="first">One corner.</param>
    /// <param name="second">The opposite corner.</param>
    public CellRange(CellAddress first, CellAddress second)
    {
        Start = new CellAddress(Math.Min(first.Row, second.Row), Math.Min(first.Column, second.Column));
        End = new CellAddress(Math.Max(first.Row, second.Row), Math.Max(first.Column, second.Column));
    }

    /// <summary>
    /// Gets the top left cell.
    /// </summary>
    public CellAddress Start { get; }

    /// <summary>
    /// Gets the bottom right cell.
    /// </summary>
    public CellAddress End { get; }

    /// <summary>
    /// Gets the number of rows the range covers.
    /// </summary>
    public int RowCount => End.Row - Start.Row + 1;

    /// <summary>
    /// Gets the number of columns the range covers.
    /// </summary>
    public int ColumnCount => End.Column - Start.Column + 1;

    /// <summary>
    /// Gets whether the range is a single cell.
    /// </summary>
    public bool IsSingleCell => Start == End;

    /// <summary>
    /// Gets whether a cell is inside the range.
    /// </summary>
    /// <param name="address">The cell.</param>
    /// <returns><see langword="true"/> when the cell is inside.</returns>
    public bool Contains(CellAddress address) =>
        address.Row >= Start.Row && address.Row <= End.Row && address.Column >= Start.Column && address.Column <= End.Column;

    /// <summary>
    /// Gets every cell in the range, row by row.
    /// </summary>
    /// <returns>The cells.</returns>
    public IEnumerable<CellAddress> Cells()
    {
        for (var row = Start.Row; row <= End.Row; row++)
        {
            for (var column = Start.Column; column <= End.Column; column++)
            {
                yield return new CellAddress(row, column);
            }
        }
    }

    /// <summary>
    /// Gets the range in A1 notation, such as <c>A1:C4</c>, or a single address for a single cell.
    /// </summary>
    /// <returns>The range.</returns>
    public override string ToString() => IsSingleCell ? Start.ToString() : $"{Start}:{End}";
}
