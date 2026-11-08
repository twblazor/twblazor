// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

namespace TwBlazor.Spreadsheet;

/// <summary>
/// Where a formula reads its cells from.
/// </summary>
internal interface IFormulaContext
{
    /// <summary>
    /// Gets the value of a cell.
    /// </summary>
    /// <param name="sheet">The sheet named in the reference, or <see langword="null"/> for the formula's own sheet.</param>
    /// <param name="address">The cell.</param>
    /// <returns>The cell's value, or <see cref="SheetValue.ReferenceError"/> when the sheet does not exist.</returns>
    SheetValue GetCell(string? sheet, CellAddress address);

    /// <summary>
    /// Gets whether a sheet exists.
    /// </summary>
    /// <param name="sheet">The sheet named in the reference, or <see langword="null"/> for the formula's own sheet.</param>
    /// <returns><see langword="true"/> when the sheet exists.</returns>
    bool HasSheet(string? sheet);
}

/// <summary>
/// Works out the value of a parsed formula.
/// </summary>
internal static class FormulaEvaluator
{
    /// <summary>
    /// The most cells one range may cover, so a formula such as <c>=SUM(A1:XFD1048576)</c> cannot stall the page.
    /// </summary>
    public const int MaxRangeCells = 100_000;

    /// <summary>
    /// Works out the value of a formula.
    /// </summary>
    /// <param name="node">The formula's tree.</param>
    /// <param name="context">Where to read cells from.</param>
    /// <returns>The value.</returns>
    public static SheetValue Evaluate(FormulaNode node, IFormulaContext context) => node switch
    {
        NumberNode number => SheetValue.FromNumber(number.Value),
        TextNode text => SheetValue.FromText(text.Value),
        BooleanNode boolean => SheetValue.FromBoolean(boolean.Value),
        ErrorNode error => SheetValue.Error(error.Code),
        ReferenceNode reference => context.GetCell(reference.Sheet, reference.Address),
        RangeNode range => range.Range.IsSingleCell ? context.GetCell(range.Sheet, range.Range.Start) : SheetValue.Error(SheetValue.ValueError),
        UnaryNode unary => EvaluateUnary(unary, context),
        PercentNode percent => Arithmetic(Evaluate(percent.Operand, context), SheetValue.FromNumber(100), "/"),
        BinaryNode binary => EvaluateBinary(binary, context),
        CallNode call => FormulaFunctions.Call(call, context),
        _ => SheetValue.Error(SheetValue.ValueError),
    };

    /// <summary>
    /// Gets the values an argument stands for: every cell of a range or a reference, or the single value of anything
    /// else. Values from cells are marked, since functions such as <c>SUM</c> skip text in cells but not text typed
    /// into the formula.
    /// </summary>
    /// <param name="node">The argument.</param>
    /// <param name="context">Where to read cells from.</param>
    /// <returns>The values, each with whether it came from a cell.</returns>
    public static IEnumerable<(SheetValue Value, bool FromCell)> Expand(FormulaNode node, IFormulaContext context)
    {
        switch (node)
        {
            case RangeNode range when !context.HasSheet(range.Sheet):
                yield return (SheetValue.Error(SheetValue.ReferenceError), false);
                break;
            case RangeNode range when (long)range.Range.RowCount * range.Range.ColumnCount > MaxRangeCells:
                yield return (SheetValue.Error(SheetValue.NumberError), false);
                break;
            case RangeNode range:
                foreach (var address in range.Range.Cells())
                {
                    yield return (context.GetCell(range.Sheet, address), true);
                }

                break;
            case ReferenceNode reference:
                yield return (context.GetCell(reference.Sheet, reference.Address), true);
                break;
            default:
                yield return (Evaluate(node, context), false);
                break;
        }
    }

    private static SheetValue EvaluateUnary(UnaryNode unary, IFormulaContext context)
    {
        var value = Evaluate(unary.Operand, context);
        if (!value.TryGetNumber(out var number, out var error))
        {
            return error;
        }

        return SheetValue.FromNumber(unary.Operator == "-" ? -number : number);
    }

    private static SheetValue EvaluateBinary(BinaryNode binary, IFormulaContext context)
    {
        var left = Evaluate(binary.Left, context);
        var right = Evaluate(binary.Right, context);
        return binary.Operator switch
        {
            "&" => Concatenate(left, right),
            "=" or "<>" or "<" or ">" or "<=" or ">=" => Compare(left, right, binary.Operator),
            _ => Arithmetic(left, right, binary.Operator),
        };
    }

    private static SheetValue Concatenate(SheetValue left, SheetValue right)
    {
        if (left.IsError)
        {
            return left;
        }

        return right.IsError ? right : SheetValue.FromText(left.AsText() + right.AsText());
    }

    /// <summary>
    /// Applies an arithmetic operator to two values.
    /// </summary>
    /// <param name="left">The value before the operator.</param>
    /// <param name="right">The value after the operator.</param>
    /// <param name="op">The operator: <c>+ - * / ^</c>.</param>
    /// <returns>The result, or the first error.</returns>
    public static SheetValue Arithmetic(SheetValue left, SheetValue right, string op)
    {
        if (!left.TryGetNumber(out var a, out var error) || !right.TryGetNumber(out var b, out error))
        {
            return error;
        }

        return op switch
        {
            "+" => SheetValue.FromNumber(a + b),
            "-" => SheetValue.FromNumber(a - b),
            "*" => SheetValue.FromNumber(a * b),
            "/" => b == 0 ? SheetValue.Error(SheetValue.DivideByZero) : SheetValue.FromNumber(a / b),
            "^" => SheetValue.FromNumber(Math.Pow(a, b)),
            _ => SheetValue.Error(SheetValue.ValueError),
        };
    }

    /// <summary>
    /// Compares two values the way a spreadsheet does: numbers by size, text alphabetically without regard to case,
    /// and any number before any text, before any true or false value. An empty cell counts as 0, empty text or
    /// <c>FALSE</c>, whichever it is compared with.
    /// </summary>
    /// <param name="left">The value before the operator.</param>
    /// <param name="right">The value after the operator.</param>
    /// <param name="op">The operator: <c>= &lt;&gt; &lt; &gt; &lt;= &gt;=</c>.</param>
    /// <returns>The result, or the first error.</returns>
    public static SheetValue Compare(SheetValue left, SheetValue right, string op)
    {
        if (left.IsError)
        {
            return left;
        }

        if (right.IsError)
        {
            return right;
        }

        var order = CompareValues(left, right);
        return SheetValue.FromBoolean(op switch
        {
            "=" => order == 0,
            "<>" => order != 0,
            "<" => order < 0,
            ">" => order > 0,
            "<=" => order <= 0,
            _ => order >= 0,
        });
    }

    /// <summary>
    /// Orders two values that are not errors, see <see cref="Compare"/>.
    /// </summary>
    /// <param name="left">The first value.</param>
    /// <param name="right">The second value.</param>
    /// <returns>Less than 0, 0 or more than 0, as for <see cref="IComparer{T}"/>.</returns>
    public static int CompareValues(SheetValue left, SheetValue right)
    {
        left = EmptyAs(left, right);
        right = EmptyAs(right, left);

        var leftRank = Rank(left);
        var rightRank = Rank(right);
        if (leftRank != rightRank)
        {
            return leftRank.CompareTo(rightRank);
        }

        return left.Kind == SheetValueKind.Text
            ? string.Compare(left.Text, right.Text, StringComparison.OrdinalIgnoreCase)
            : left.Number.CompareTo(right.Number);
    }

    private static SheetValue EmptyAs(SheetValue value, SheetValue other)
    {
        if (value.Kind != SheetValueKind.Empty)
        {
            return value;
        }

        return other.Kind switch
        {
            SheetValueKind.Text => SheetValue.FromText(string.Empty),
            SheetValueKind.Boolean => SheetValue.FromBoolean(false),
            _ => SheetValue.FromNumber(0),
        };
    }

    private static int Rank(SheetValue value) => value.Kind switch
    {
        SheetValueKind.Number => 0,
        SheetValueKind.Text => 1,
        _ => 2,
    };
}
