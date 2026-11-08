// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using System.Globalization;

namespace TwBlazor.Spreadsheet;

/// <summary>A part of a parsed formula.</summary>
internal abstract record FormulaNode;

/// <summary>A number.</summary>
/// <param name="Value">The number.</param>
internal sealed record NumberNode(double Value) : FormulaNode;

/// <summary>Text in quotes.</summary>
/// <param name="Value">The text.</param>
internal sealed record TextNode(string Value) : FormulaNode;

/// <summary><c>TRUE</c> or <c>FALSE</c>.</summary>
/// <param name="Value">The value.</param>
internal sealed record BooleanNode(bool Value) : FormulaNode;

/// <summary>An error literal, such as <c>#N/A</c>.</summary>
/// <param name="Code">The error code.</param>
internal sealed record ErrorNode(string Code) : FormulaNode;

/// <summary>A reference to one cell.</summary>
/// <param name="Sheet">The sheet named before the reference, or <see langword="null"/> for the formula's own sheet.</param>
/// <param name="Address">The cell.</param>
internal sealed record ReferenceNode(string? Sheet, CellAddress Address) : FormulaNode;

/// <summary>A reference to a range of cells.</summary>
/// <param name="Sheet">The sheet named before the range, or <see langword="null"/> for the formula's own sheet.</param>
/// <param name="Range">The cells.</param>
internal sealed record RangeNode(string? Sheet, CellRange Range) : FormulaNode;

/// <summary>A minus or plus sign in front of a value.</summary>
/// <param name="Operator">The sign.</param>
/// <param name="Operand">The value.</param>
internal sealed record UnaryNode(string Operator, FormulaNode Operand) : FormulaNode;

/// <summary>A percent sign after a value, which divides it by 100.</summary>
/// <param name="Operand">The value.</param>
internal sealed record PercentNode(FormulaNode Operand) : FormulaNode;

/// <summary>An operator between two values.</summary>
/// <param name="Operator">The operator.</param>
/// <param name="Left">The value before it.</param>
/// <param name="Right">The value after it.</param>
internal sealed record BinaryNode(string Operator, FormulaNode Left, FormulaNode Right) : FormulaNode;

/// <summary>A function call.</summary>
/// <param name="Name">The function's name in upper case.</param>
/// <param name="Arguments">The arguments.</param>
internal sealed record CallNode(string Name, IReadOnlyList<FormulaNode> Arguments) : FormulaNode;

/// <summary>
/// Reads a formula into a tree of <see cref="FormulaNode"/>, with the operator precedence of a spreadsheet: comparison,
/// then <c>&amp;</c>, then <c>+</c> and <c>-</c>, then <c>*</c> and <c>/</c>, then <c>^</c>, then a leading sign,
/// then <c>%</c>.
/// </summary>
internal sealed class FormulaParser
{
    private readonly List<FormulaToken> _tokens;

    private int position;

    private FormulaParser(List<FormulaToken> tokens) => _tokens = tokens;

    private FormulaToken current => _tokens[position];

    /// <summary>
    /// Reads a formula.
    /// </summary>
    /// <param name="formula">The formula, without its leading <c>=</c>.</param>
    /// <returns>The formula's tree.</returns>
    /// <exception cref="FormulaSyntaxException">The formula cannot be read.</exception>
    public static FormulaNode Parse(string formula)
    {
        var parser = new FormulaParser(FormulaLexer.Tokenize(formula));
        if (parser.current.Kind == FormulaTokenKind.End)
        {
            throw new FormulaSyntaxException("The formula is empty.");
        }

        var node = parser.ParseComparison();
        if (parser.current.Kind != FormulaTokenKind.End)
        {
            throw new FormulaSyntaxException($"Unexpected '{parser.current.Text}'.");
        }

        return node;
    }

    private FormulaToken Advance() => _tokens[position++];

    private bool IsOperator(params string[] operators) =>
        current.Kind == FormulaTokenKind.Operator && operators.Contains(current.Text, StringComparer.Ordinal);

    private FormulaNode ParseComparison()
    {
        var left = ParseConcatenation();
        while (IsOperator("=", "<>", "<", ">", "<=", ">="))
        {
            var op = Advance().Text;
            left = new BinaryNode(op, left, ParseConcatenation());
        }

        return left;
    }

    private FormulaNode ParseConcatenation()
    {
        var left = ParseAdditive();
        while (IsOperator("&"))
        {
            Advance();
            left = new BinaryNode("&", left, ParseAdditive());
        }

        return left;
    }

    private FormulaNode ParseAdditive()
    {
        var left = ParseMultiplicative();
        while (IsOperator("+", "-"))
        {
            var op = Advance().Text;
            left = new BinaryNode(op, left, ParseMultiplicative());
        }

        return left;
    }

    private FormulaNode ParseMultiplicative()
    {
        var left = ParsePower();
        while (IsOperator("*", "/"))
        {
            var op = Advance().Text;
            left = new BinaryNode(op, left, ParsePower());
        }

        return left;
    }

    private FormulaNode ParsePower()
    {
        var left = ParseUnary();
        while (IsOperator("^"))
        {
            Advance();
            left = new BinaryNode("^", left, ParseUnary());
        }

        return left;
    }

    private FormulaNode ParseUnary()
    {
        if (IsOperator("-", "+"))
        {
            var op = Advance().Text;
            return new UnaryNode(op, ParseUnary());
        }

        return ParsePercent();
    }

    private FormulaNode ParsePercent()
    {
        var node = ParsePrimary();
        while (IsOperator("%"))
        {
            Advance();
            node = new PercentNode(node);
        }

        return node;
    }

    private FormulaNode ParsePrimary()
    {
        var token = Advance();
        switch (token.Kind)
        {
            case FormulaTokenKind.Number:
                return new NumberNode(double.Parse(token.Text, NumberStyles.Float, CultureInfo.InvariantCulture));
            case FormulaTokenKind.Text:
                return new TextNode(token.Text);
            case FormulaTokenKind.Error:
                return new ErrorNode(token.Text);
            case FormulaTokenKind.SheetPrefix:
                if (current.Kind != FormulaTokenKind.Reference)
                {
                    throw new FormulaSyntaxException("A sheet name must be followed by a cell reference.");
                }

                return ParseReference(token.Text, Advance());
            case FormulaTokenKind.Reference:
                return ParseReference(null, token);
            case FormulaTokenKind.Name:
                return ParseName(token);
            case FormulaTokenKind.OpenParen:
                var inner = ParseComparison();
                Expect(FormulaTokenKind.CloseParen);
                return inner;
            default:
                throw new FormulaSyntaxException(token.Kind == FormulaTokenKind.End ? "The formula ends too soon." : $"Unexpected '{token.Text}'.");
        }
    }

    private FormulaNode ParseReference(string? sheet, FormulaToken token)
    {
        var start = CellAddress.Parse(token.Text);
        if (current.Kind != FormulaTokenKind.Colon)
        {
            return new ReferenceNode(sheet, start);
        }

        Advance();
        var end = Advance();
        if (end.Kind != FormulaTokenKind.Reference)
        {
            throw new FormulaSyntaxException("A range must end with a cell reference.");
        }

        return new RangeNode(sheet, new CellRange(start, CellAddress.Parse(end.Text)));
    }

    private FormulaNode ParseName(FormulaToken token)
    {
        var name = token.Text.ToUpperInvariant();
        if (current.Kind != FormulaTokenKind.OpenParen)
        {
            return name switch
            {
                "TRUE" => new BooleanNode(true),
                "FALSE" => new BooleanNode(false),
                _ => new ErrorNode(SheetValue.NameError),
            };
        }

        Advance();
        var arguments = new List<FormulaNode>();
        if (current.Kind != FormulaTokenKind.CloseParen)
        {
            arguments.Add(ParseComparison());
            while (current.Kind == FormulaTokenKind.Comma)
            {
                Advance();
                arguments.Add(ParseComparison());
            }
        }

        Expect(FormulaTokenKind.CloseParen);
        return new CallNode(name, arguments);
    }

    private void Expect(FormulaTokenKind kind)
    {
        if (current.Kind != kind)
        {
            throw new FormulaSyntaxException(current.Kind == FormulaTokenKind.End ? "The formula ends too soon." : $"Unexpected '{current.Text}'.");
        }

        Advance();
    }
}
