// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using System.Globalization;
using System.Text;

namespace TwBlazor.Spreadsheet;

/// <summary>
/// The kinds of token a formula is made of.
/// </summary>
internal enum FormulaTokenKind
{
    /// <summary>A number, such as <c>12.5</c>.</summary>
    Number,

    /// <summary>Text in double quotes.</summary>
    Text,

    /// <summary>A cell reference, such as <c>B3</c> or <c>$B$3</c>.</summary>
    Reference,

    /// <summary>A sheet name followed by <c>!</c>, which qualifies the reference after it.</summary>
    SheetPrefix,

    /// <summary>A name: a function, <c>TRUE</c> or <c>FALSE</c>.</summary>
    Name,

    /// <summary>An error literal, such as <c>#N/A</c>.</summary>
    Error,

    /// <summary>An operator: <c>+ - * / ^ &amp; % = &lt;&gt; &lt; &gt; &lt;= &gt;=</c>.</summary>
    Operator,

    /// <summary>An opening bracket.</summary>
    OpenParen,

    /// <summary>A closing bracket.</summary>
    CloseParen,

    /// <summary>A comma between function arguments.</summary>
    Comma,

    /// <summary>The colon between the corners of a range.</summary>
    Colon,

    /// <summary>The end of the formula.</summary>
    End,
}

/// <summary>
/// A token of a formula, with where it sits in the formula's text.
/// </summary>
/// <param name="Kind">The kind of token.</param>
/// <param name="Text">The token's value: the number's text, the unquoted text, the sheet name or the operator.</param>
/// <param name="Start">The index in the formula where the token starts.</param>
/// <param name="Length">The number of characters the token covers in the formula.</param>
internal readonly record struct FormulaToken(FormulaTokenKind Kind, string Text, int Start, int Length);

/// <summary>
/// The exception thrown when a formula cannot be read.
/// </summary>
internal sealed class FormulaSyntaxException(string message) : Exception(message);

/// <summary>
/// Splits a formula, without its leading <c>=</c>, into tokens.
/// </summary>
internal static class FormulaLexer
{
    private static readonly string[] _errorLiterals =
    [
        SheetValue.DivideByZero, SheetValue.ValueError, SheetValue.ReferenceError, SheetValue.NameError,
        SheetValue.NumberError, SheetValue.NotAvailable,
    ];

    /// <summary>
    /// Splits a formula into tokens, ending with a <see cref="FormulaTokenKind.End"/> token.
    /// </summary>
    /// <param name="formula">The formula, without its leading <c>=</c>.</param>
    /// <returns>The tokens.</returns>
    /// <exception cref="FormulaSyntaxException">The formula holds something that is not a token.</exception>
    public static List<FormulaToken> Tokenize(string formula)
    {
        var tokens = new List<FormulaToken>();
        var i = 0;
        while (i < formula.Length)
        {
            var c = formula[i];
            if (char.IsWhiteSpace(c))
            {
                i++;
                continue;
            }

            var token = c switch
            {
                '"' => ReadText(formula, i),
                '\'' => ReadQuotedSheet(formula, i),
                '#' => ReadError(formula, i),
                '(' => new FormulaToken(FormulaTokenKind.OpenParen, "(", i, 1),
                ')' => new FormulaToken(FormulaTokenKind.CloseParen, ")", i, 1),
                ',' or ';' => new FormulaToken(FormulaTokenKind.Comma, ",", i, 1),
                ':' => new FormulaToken(FormulaTokenKind.Colon, ":", i, 1),
                '<' or '>' => ReadComparison(formula, i),
                '+' or '-' or '*' or '/' or '^' or '&' or '%' or '=' => new FormulaToken(FormulaTokenKind.Operator, c.ToString(), i, 1),
                _ when char.IsAsciiDigit(c) || (c == '.' && i + 1 < formula.Length && char.IsAsciiDigit(formula[i + 1])) => ReadNumber(formula, i),
                _ when char.IsAsciiLetter(c) || c is '$' or '_' => ReadWord(formula, i),
                _ => throw new FormulaSyntaxException($"Unexpected '{c}'."),
            };

            tokens.Add(token);
            i = token.Start + token.Length;
        }

        tokens.Add(new FormulaToken(FormulaTokenKind.End, string.Empty, formula.Length, 0));
        return tokens;
    }

    private static FormulaToken ReadText(string formula, int start)
    {
        var text = new StringBuilder();
        var i = start + 1;
        while (i < formula.Length)
        {
            if (formula[i] == '"')
            {
                if (i + 1 < formula.Length && formula[i + 1] == '"')
                {
                    text.Append('"');
                    i += 2;
                    continue;
                }

                return new FormulaToken(FormulaTokenKind.Text, text.ToString(), start, i + 1 - start);
            }

            text.Append(formula[i]);
            i++;
        }

        throw new FormulaSyntaxException("Text is missing its closing quote.");
    }

    private static FormulaToken ReadQuotedSheet(string formula, int start)
    {
        var name = new StringBuilder();
        var i = start + 1;
        while (i < formula.Length)
        {
            if (formula[i] == '\'')
            {
                if (i + 1 < formula.Length && formula[i + 1] == '\'')
                {
                    name.Append('\'');
                    i += 2;
                    continue;
                }

                if (i + 1 < formula.Length && formula[i + 1] == '!')
                {
                    return new FormulaToken(FormulaTokenKind.SheetPrefix, name.ToString(), start, i + 2 - start);
                }

                break;
            }

            name.Append(formula[i]);
            i++;
        }

        throw new FormulaSyntaxException("A quoted sheet name must be followed by '!'.");
    }

    private static FormulaToken ReadError(string formula, int start)
    {
        foreach (var literal in _errorLiterals)
        {
            if (string.Compare(formula, start, literal, 0, literal.Length, StringComparison.OrdinalIgnoreCase) == 0)
            {
                return new FormulaToken(FormulaTokenKind.Error, literal, start, literal.Length);
            }
        }

        throw new FormulaSyntaxException("Unknown error value.");
    }

    private static FormulaToken ReadComparison(string formula, int start)
    {
        var next = start + 1 < formula.Length ? formula[start + 1] : '\0';
        var op = (formula[start], next) switch
        {
            ('<', '=') => "<=",
            ('>', '=') => ">=",
            ('<', '>') => "<>",
            _ => formula[start].ToString(),
        };

        return new FormulaToken(FormulaTokenKind.Operator, op, start, op.Length);
    }

    private static FormulaToken ReadNumber(string formula, int start)
    {
        var i = start;
        while (i < formula.Length && (char.IsAsciiDigit(formula[i]) || formula[i] == '.'))
        {
            i++;
        }

        if (i < formula.Length && formula[i] is 'e' or 'E')
        {
            var exponent = i + 1;
            if (exponent < formula.Length && formula[exponent] is '+' or '-')
            {
                exponent++;
            }

            if (exponent < formula.Length && char.IsAsciiDigit(formula[exponent]))
            {
                i = exponent;
                while (i < formula.Length && char.IsAsciiDigit(formula[i]))
                {
                    i++;
                }
            }
        }

        var text = formula[start..i];
        if (!double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out _))
        {
            throw new FormulaSyntaxException($"'{text}' is not a number.");
        }

        return new FormulaToken(FormulaTokenKind.Number, text, start, i - start);
    }

    private static FormulaToken ReadWord(string formula, int start)
    {
        var i = start;
        while (i < formula.Length && (char.IsAsciiLetterOrDigit(formula[i]) || formula[i] is '$' or '_' or '.'))
        {
            i++;
        }

        var word = formula[start..i];
        if (i < formula.Length && formula[i] == '!')
        {
            return new FormulaToken(FormulaTokenKind.SheetPrefix, word, start, i + 1 - start);
        }

        var isCall = NextNonSpace(formula, i) == '(';
        if (!isCall && CellAddress.TryParse(word, out _))
        {
            return new FormulaToken(FormulaTokenKind.Reference, word, start, word.Length);
        }

        if (word.Contains('$', StringComparison.Ordinal))
        {
            throw new FormulaSyntaxException($"'{word}' is not a cell reference.");
        }

        return new FormulaToken(FormulaTokenKind.Name, word, start, word.Length);
    }

    private static char NextNonSpace(string formula, int index)
    {
        while (index < formula.Length && char.IsWhiteSpace(formula[index]))
        {
            index++;
        }

        return index < formula.Length ? formula[index] : '\0';
    }
}
