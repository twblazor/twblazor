// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using System.Globalization;
using System.Text;
using TwBlazor.Enums;

namespace TwBlazor.Spreadsheet;

/// <summary>
/// Turns cell values into the text shown in a cell, and moves the references of a copied formula.
/// </summary>
internal static class SheetFormatting
{
    /// <summary>
    /// Gets the text a cell shows for its value.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="format">The cell's number format, used when the value is a number.</param>
    /// <param name="culture">The culture numbers are shown in.</param>
    /// <returns>The text.</returns>
    public static string Display(SheetValue value, SheetNumberFormat format, CultureInfo culture) => value.Kind switch
    {
        SheetValueKind.Number => FormatNumber(value.Number, format, culture),
        SheetValueKind.Boolean => value.Number != 0 ? "TRUE" : "FALSE",
        SheetValueKind.Empty => string.Empty,
        _ => value.Text ?? string.Empty,
    };

    /// <summary>
    /// Gets the alignment a cell with <see cref="SheetCellAlignment.General"/> uses for its value: numbers at the end,
    /// true or false values and errors in the middle, and text at the start.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>The alignment.</returns>
    public static SheetCellAlignment GeneralAlignment(SheetValue value) => value.Kind switch
    {
        SheetValueKind.Number => SheetCellAlignment.End,
        SheetValueKind.Boolean or SheetValueKind.Error => SheetCellAlignment.Center,
        _ => SheetCellAlignment.Start,
    };

    private static string FormatNumber(double number, SheetNumberFormat format, CultureInfo culture) => format switch
    {
        SheetNumberFormat.Number => number.ToString("N2", culture),
        SheetNumberFormat.Currency => number.ToString("C2", culture),
        SheetNumberFormat.Percent => number.ToString("P2", culture),
        _ => number.ToString("G10", culture),
    };

    /// <summary>
    /// Moves the relative references of a formula, as when it is pasted somewhere else: <c>=A1+B1</c> pasted one row
    /// down becomes <c>=A2+B2</c>. A part fixed with <c>$</c>, as in <c>$A$1</c>, stays where it is, and a reference
    /// moved off the sheet becomes <c>#REF!</c>.
    /// </summary>
    /// <param name="raw">The text typed into the cell. Anything that is not a formula is returned unchanged.</param>
    /// <param name="rows">The rows to move down, or up when negative.</param>
    /// <param name="columns">The columns to move right, or left when negative.</param>
    /// <returns>The moved formula.</returns>
    public static string? ShiftFormula(string? raw, int rows, int columns)
    {
        if (!SheetCalculator.IsFormula(raw) || (rows == 0 && columns == 0))
        {
            return raw;
        }

        List<FormulaToken> tokens;
        try
        {
            tokens = FormulaLexer.Tokenize(raw![1..]);
        }
        catch (FormulaSyntaxException)
        {
            return raw;
        }

        var result = new StringBuilder("=");
        var formula = raw![1..];
        var position = 0;
        foreach (var token in tokens.Where(t => t.Kind == FormulaTokenKind.Reference))
        {
            result.Append(formula, position, token.Start - position);
            result.Append(ShiftReference(token.Text, rows, columns));
            position = token.Start + token.Length;
        }

        result.Append(formula, position, formula.Length - position);
        return result.ToString();
    }

    /// <summary>
    /// Changes the sheet name in the references of a formula after a sheet was renamed, so <c>=Budget!B4</c> becomes
    /// <c>='Budget 2026'!B4</c>.
    /// </summary>
    /// <param name="raw">The text typed into the cell. Anything that is not a formula is returned unchanged.</param>
    /// <param name="oldName">The sheet's old name, matched without regard to case.</param>
    /// <param name="newName">The sheet's new name.</param>
    /// <returns>The formula with the new name.</returns>
    public static string? RenameSheet(string? raw, string oldName, string newName)
    {
        if (!SheetCalculator.IsFormula(raw))
        {
            return raw;
        }

        List<FormulaToken> tokens;
        try
        {
            tokens = FormulaLexer.Tokenize(raw![1..]);
        }
        catch (FormulaSyntaxException)
        {
            return raw;
        }

        var formula = raw![1..];
        var matches = tokens.Where(t => t.Kind == FormulaTokenKind.SheetPrefix && string.Equals(t.Text, oldName, StringComparison.OrdinalIgnoreCase)).ToList();
        if (matches.Count == 0)
        {
            return raw;
        }

        var result = new StringBuilder("=");
        var position = 0;
        foreach (var token in matches)
        {
            result.Append(formula, position, token.Start - position);
            result.Append(QuoteSheetName(newName)).Append('!');
            position = token.Start + token.Length;
        }

        result.Append(formula, position, formula.Length - position);
        return result.ToString();
    }

    /// <summary>
    /// Writes a sheet name the way a formula refers to it: as it is when it is a plain word, or in single quotes when it
    /// holds spaces or other characters, or could be read as a cell reference.
    /// </summary>
    /// <param name="name">The sheet name.</param>
    /// <returns>The name, quoted when needed.</returns>
    public static string QuoteSheetName(string name)
    {
        var plain = name.Length > 0 && (char.IsAsciiLetter(name[0]) || name[0] == '_')
            && name.All(c => char.IsAsciiLetterOrDigit(c) || c is '_' or '.')
            && !CellAddress.TryParse(name, out _);
        return plain ? name : $"'{name.Replace("'", "''", StringComparison.Ordinal)}'";
    }

    /// <summary>
    /// Changes the references of a formula after rows or columns were inserted or deleted on a sheet, as a spreadsheet
    /// does: references after the change move with their cells, a range that loses some of its cells shrinks, and a
    /// reference to a deleted cell becomes <c>#REF!</c>. References fixed with <c>$</c> move too, since their cells
    /// moved.
    /// </summary>
    /// <param name="raw">The text typed into the cell. Anything that is not a formula is returned unchanged.</param>
    /// <param name="onChangedSheet">Whether the formula is on the sheet that changed, where references without a
    /// sheet name point.</param>
    /// <param name="sheetName">The name of the sheet that changed.</param>
    /// <param name="rows"><see langword="true"/> for rows, <see langword="false"/> for columns.</param>
    /// <param name="index">The zero based row or column where the change starts.</param>
    /// <param name="count">The number inserted, or deleted when negative.</param>
    /// <returns>The changed formula.</returns>
    public static string? AdjustForStructure(string? raw, bool onChangedSheet, string sheetName, bool rows, int index, int count)
    {
        if (!SheetCalculator.IsFormula(raw) || count == 0)
        {
            return raw;
        }

        List<FormulaToken> tokens;
        try
        {
            tokens = FormulaLexer.Tokenize(raw![1..]);
        }
        catch (FormulaSyntaxException)
        {
            return raw;
        }

        var formula = raw![1..];
        var result = new StringBuilder("=");
        var position = 0;
        for (var i = 0; i < tokens.Count; i++)
        {
            if (tokens[i].Kind != FormulaTokenKind.Reference)
            {
                continue;
            }

            var prefix = i > 0 && tokens[i - 1].Kind == FormulaTokenKind.SheetPrefix ? tokens[i - 1].Text : null;
            var affected = prefix is null ? onChangedSheet : string.Equals(prefix, sheetName, StringComparison.OrdinalIgnoreCase);
            var isRange = i + 2 < tokens.Count && tokens[i + 1].Kind == FormulaTokenKind.Colon && tokens[i + 2].Kind == FormulaTokenKind.Reference;
            var last = isRange ? tokens[i + 2] : tokens[i];
            if (!affected)
            {
                i = isRange ? i + 2 : i;
                continue;
            }

            result.Append(formula, position, tokens[i].Start - position);
            result.Append(isRange
                ? AdjustRange(tokens[i].Text, last.Text, rows, index, count)
                : AdjustSingle(tokens[i].Text, rows, index, count));
            position = last.Start + last.Length;
            i = isRange ? i + 2 : i;
        }

        result.Append(formula, position, formula.Length - position);
        return result.ToString();
    }

    private static string AdjustSingle(string reference, bool rows, int index, int count)
    {
        var moved = Move(Position(reference, rows), index, count, Edge.Single);
        return moved is { } p ? WithPosition(reference, rows, p) : SheetValue.ReferenceError;
    }

    private static string AdjustRange(string first, string second, bool rows, int index, int count)
    {
        var start = Move(Position(first, rows), index, count, Edge.Start);
        var end = Move(Position(second, rows), index, count, Edge.End);
        if (start is not { } s || end is not { } e || e < s)
        {
            return SheetValue.ReferenceError;
        }

        return $"{WithPosition(first, rows, s)}:{WithPosition(second, rows, e)}";
    }

    /// <summary>
    /// Which end of a range a reference is, which decides where it goes when its own row or column is deleted.
    /// </summary>
    private enum Edge
    {
        Single,
        Start,
        End,
    }

    private static int? Move(int position, int index, int count, Edge edge)
    {
        if (count > 0)
        {
            return position >= index ? position + count : position;
        }

        var deleted = -count;
        if (position < index)
        {
            return position;
        }

        if (position >= index + deleted)
        {
            return position - deleted;
        }

        return edge switch
        {
            Edge.Start => index,
            Edge.End => index - 1,
            _ => null,
        };
    }

    private static int Position(string reference, bool rows)
    {
        var address = CellAddress.Parse(reference);
        return rows ? address.Row : address.Column;
    }

    private static string WithPosition(string reference, bool rows, int position)
    {
        var absoluteColumn = reference[0] == '$';
        var absoluteRow = reference.IndexOf('$', 1) > 0;
        var address = CellAddress.Parse(reference);
        var row = rows ? position : address.Row;
        var column = rows ? address.Column : position;
        if (row < 0 || column < 0 || row >= CellAddress.MaxRows || column >= CellAddress.MaxColumns)
        {
            return SheetValue.ReferenceError;
        }

        return (absoluteColumn ? "$" : string.Empty) + CellAddress.ColumnName(column)
            + (absoluteRow ? "$" : string.Empty) + (row + 1).ToString(CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Gets a hex colour that is safe to put in a style, or <see langword="null"/> when the text is not one.
    /// </summary>
    /// <param name="color">The colour, such as <c>#FEF3C7</c>.</param>
    /// <returns>The colour, or <see langword="null"/>.</returns>
    public static string? SafeColor(string? color)
    {
        if (string.IsNullOrEmpty(color) || color[0] != '#' || color.Length is not (4 or 7 or 9))
        {
            return null;
        }

        return color.Skip(1).All(char.IsAsciiHexDigit) ? color : null;
    }

    /// <summary>
    /// Gets a font family that is safe to put in a style, or <see langword="null"/> when it holds anything other than
    /// letters, digits, spaces, hyphens, commas and quotes.
    /// </summary>
    /// <param name="family">The font family, such as <c>Times New Roman</c>.</param>
    /// <returns>The font family, or <see langword="null"/>.</returns>
    public static string? SafeFontFamily(string? family)
    {
        var trimmed = family?.Trim();
        if (string.IsNullOrEmpty(trimmed) || trimmed.Length > 100)
        {
            return null;
        }

        return trimmed.All(c => char.IsLetterOrDigit(c) || c is ' ' or '-' or ',' or '\'' or '"') ? trimmed : null;
    }

    /// <summary>
    /// Keeps a font size between 6 and 72 points.
    /// </summary>
    /// <param name="size">The size in points.</param>
    /// <returns>The size, or <see langword="null"/> when none was given or it is not a number.</returns>
    public static double? SafeFontSize(double? size) =>
        size is { } s && double.IsFinite(s) ? Math.Clamp(s, 6, 72) : null;

    private static string ShiftReference(string reference, int rows, int columns)
    {
        var absoluteColumn = reference[0] == '$';
        var rowDollar = reference.IndexOf('$', 1);
        var absoluteRow = rowDollar > 0;
        var address = CellAddress.Parse(reference);

        var row = absoluteRow ? address.Row : address.Row + rows;
        var column = absoluteColumn ? address.Column : address.Column + columns;
        if (row < 0 || column < 0 || row >= CellAddress.MaxRows || column >= CellAddress.MaxColumns)
        {
            return SheetValue.ReferenceError;
        }

        return (absoluteColumn ? "$" : string.Empty) + CellAddress.ColumnName(column)
            + (absoluteRow ? "$" : string.Empty) + (row + 1).ToString(CultureInfo.InvariantCulture);
    }
}
