// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using System.Globalization;
using System.Text;

namespace TwBlazor.Spreadsheet;

/// <summary>
/// The functions a formula can call, such as <c>SUM</c> and <c>IF</c>. Each one follows the spreadsheet function of
/// the same name.
/// </summary>
internal static class FormulaFunctions
{
    private delegate SheetValue Function(IReadOnlyList<FormulaNode> arguments, IFormulaContext context);

    private static readonly Dictionary<string, (int Min, int Max, Function Run)> _functions = new(StringComparer.Ordinal)
    {
        ["SUM"] = (1, int.MaxValue, (a, c) => Aggregate(a, c, values => values.Sum())),
        ["PRODUCT"] = (1, int.MaxValue, (a, c) => Aggregate(a, c, values => values.Aggregate(1d, (x, y) => x * y))),
        ["AVERAGE"] = (1, int.MaxValue, (a, c) => Aggregate(a, c, values => values.Count == 0 ? double.NaN : values.Average(), SheetValue.DivideByZero)),
        ["MIN"] = (1, int.MaxValue, (a, c) => Aggregate(a, c, values => values.Count == 0 ? 0 : values.Min())),
        ["MAX"] = (1, int.MaxValue, (a, c) => Aggregate(a, c, values => values.Count == 0 ? 0 : values.Max())),
        ["COUNT"] = (1, int.MaxValue, Count),
        ["COUNTA"] = (1, int.MaxValue, CountNonEmpty),
        ["COUNTIF"] = (2, 2, CountIf),
        ["SUMIF"] = (2, 3, SumIf),
        ["IF"] = (2, 3, If),
        ["IFERROR"] = (2, 2, IfError),
        ["AND"] = (1, int.MaxValue, (a, c) => Logical(a, c, all: true)),
        ["OR"] = (1, int.MaxValue, (a, c) => Logical(a, c, all: false)),
        ["NOT"] = (1, 1, Not),
        ["ROUND"] = (1, 2, (a, c) => Round(a, c, MidpointRounding.AwayFromZero)),
        ["ROUNDUP"] = (1, 2, (a, c) => Round(a, c, MidpointRounding.AwayFromZero, away: true)),
        ["ROUNDDOWN"] = (1, 2, (a, c) => Round(a, c, MidpointRounding.ToZero)),
        ["ABS"] = (1, 1, (a, c) => Numeric(a, c, Math.Abs)),
        ["INT"] = (1, 1, (a, c) => Numeric(a, c, Math.Floor)),
        ["SQRT"] = (1, 1, (a, c) => Numeric(a, c, x => x < 0 ? double.NaN : Math.Sqrt(x))),
        ["POWER"] = (2, 2, (a, c) => FormulaEvaluator.Arithmetic(FormulaEvaluator.Evaluate(a[0], c), FormulaEvaluator.Evaluate(a[1], c), "^")),
        ["MOD"] = (2, 2, Mod),
        ["PI"] = (0, 0, (_, _) => SheetValue.FromNumber(Math.PI)),
        ["NA"] = (0, 0, (_, _) => SheetValue.Error(SheetValue.NotAvailable)),
        ["LEN"] = (1, 1, (a, c) => Text(a, c, s => SheetValue.FromNumber(s.Length))),
        ["UPPER"] = (1, 1, (a, c) => Text(a, c, s => SheetValue.FromText(s.ToUpperInvariant()))),
        ["LOWER"] = (1, 1, (a, c) => Text(a, c, s => SheetValue.FromText(s.ToLowerInvariant()))),
        ["TRIM"] = (1, 1, (a, c) => Text(a, c, s => SheetValue.FromText(string.Join(' ', s.Split(' ', StringSplitOptions.RemoveEmptyEntries))))),
        ["CONCAT"] = (1, int.MaxValue, Concat),
        ["CONCATENATE"] = (1, int.MaxValue, Concat),
        ["LEFT"] = (1, 2, (a, c) => Substring(a, c, fromEnd: false)),
        ["RIGHT"] = (1, 2, (a, c) => Substring(a, c, fromEnd: true)),
        ["MID"] = (3, 3, Mid),
    };

    /// <summary>
    /// Gets the names of the functions a formula can call, in alphabetical order.
    /// </summary>
    public static IEnumerable<string> Names => _functions.Keys.Order(StringComparer.Ordinal);

    /// <summary>
    /// Calls a function.
    /// </summary>
    /// <param name="call">The call.</param>
    /// <param name="context">Where to read cells from.</param>
    /// <returns>The result, <see cref="SheetValue.NameError"/> for an unknown function, or
    /// <see cref="SheetValue.ValueError"/> for the wrong number of arguments.</returns>
    public static SheetValue Call(CallNode call, IFormulaContext context)
    {
        if (!_functions.TryGetValue(call.Name, out var function))
        {
            return SheetValue.Error(SheetValue.NameError);
        }

        if (call.Arguments.Count < function.Min || call.Arguments.Count > function.Max)
        {
            return SheetValue.Error(SheetValue.ValueError);
        }

        return function.Run(call.Arguments, context);
    }

    /// <summary>
    /// Collects the numbers of the arguments: numbers in cells (text and empty cells are skipped), and values typed
    /// into the formula, which must read as numbers.
    /// </summary>
    private static bool TryCollectNumbers(IReadOnlyList<FormulaNode> arguments, IFormulaContext context, List<double> numbers, out SheetValue error)
    {
        error = default;
        foreach (var argument in arguments)
        {
            foreach (var (value, fromCell) in FormulaEvaluator.Expand(argument, context))
            {
                if (value.IsError)
                {
                    error = value;
                    return false;
                }

                if (fromCell)
                {
                    if (value.Kind == SheetValueKind.Number)
                    {
                        numbers.Add(value.Number);
                    }

                    continue;
                }

                if (!value.TryGetNumber(out var number, out error))
                {
                    return false;
                }

                numbers.Add(number);
            }
        }

        return true;
    }

    private static SheetValue Aggregate(IReadOnlyList<FormulaNode> arguments, IFormulaContext context, Func<List<double>, double> aggregate, string emptyError = SheetValue.NumberError)
    {
        var numbers = new List<double>();
        if (!TryCollectNumbers(arguments, context, numbers, out var error))
        {
            return error;
        }

        var result = aggregate(numbers);
        return double.IsNaN(result) ? SheetValue.Error(emptyError) : SheetValue.FromNumber(result);
    }

    private static SheetValue Count(IReadOnlyList<FormulaNode> arguments, IFormulaContext context) =>
        SheetValue.FromNumber(arguments.SelectMany(a => FormulaEvaluator.Expand(a, context)).Count(v =>
            v.Value.Kind == SheetValueKind.Number || (!v.FromCell && v.Value.TryGetNumber(out _, out _) && v.Value.Kind != SheetValueKind.Empty)));

    private static SheetValue CountNonEmpty(IReadOnlyList<FormulaNode> arguments, IFormulaContext context) =>
        SheetValue.FromNumber(arguments.SelectMany(a => FormulaEvaluator.Expand(a, context)).Count(v => v.Value.Kind != SheetValueKind.Empty));

    private static SheetValue CountIf(IReadOnlyList<FormulaNode> arguments, IFormulaContext context)
    {
        var criterion = FormulaEvaluator.Evaluate(arguments[1], context);
        if (criterion.IsError)
        {
            return criterion;
        }

        var matches = FormulaEvaluator.Expand(arguments[0], context).Count(v => Criterion.Matches(v.Value, criterion));
        return SheetValue.FromNumber(matches);
    }

    private static SheetValue SumIf(IReadOnlyList<FormulaNode> arguments, IFormulaContext context)
    {
        var criterion = FormulaEvaluator.Evaluate(arguments[1], context);
        if (criterion.IsError)
        {
            return criterion;
        }

        var tested = FormulaEvaluator.Expand(arguments[0], context).ToList();
        var summed = arguments.Count == 3 ? FormulaEvaluator.Expand(arguments[2], context).ToList() : tested;
        var total = 0d;
        for (var i = 0; i < tested.Count && i < summed.Count; i++)
        {
            if (!Criterion.Matches(tested[i].Value, criterion))
            {
                continue;
            }

            if (summed[i].Value.IsError)
            {
                return summed[i].Value;
            }

            if (summed[i].Value.Kind == SheetValueKind.Number)
            {
                total += summed[i].Value.Number;
            }
        }

        return SheetValue.FromNumber(total);
    }

    private static SheetValue If(IReadOnlyList<FormulaNode> arguments, IFormulaContext context)
    {
        if (!FormulaEvaluator.Evaluate(arguments[0], context).TryGetBoolean(out var condition, out var error))
        {
            return error;
        }

        if (condition)
        {
            return FormulaEvaluator.Evaluate(arguments[1], context);
        }

        return arguments.Count == 3 ? FormulaEvaluator.Evaluate(arguments[2], context) : SheetValue.FromBoolean(false);
    }

    private static SheetValue IfError(IReadOnlyList<FormulaNode> arguments, IFormulaContext context)
    {
        var value = FormulaEvaluator.Evaluate(arguments[0], context);
        return value.IsError ? FormulaEvaluator.Evaluate(arguments[1], context) : value;
    }

    private static SheetValue Logical(IReadOnlyList<FormulaNode> arguments, IFormulaContext context, bool all)
    {
        var seen = false;
        var result = all;
        foreach (var (value, fromCell) in arguments.SelectMany(a => FormulaEvaluator.Expand(a, context)))
        {
            if (value.IsError)
            {
                return value;
            }

            if (fromCell && value.Kind is SheetValueKind.Text or SheetValueKind.Empty)
            {
                continue;
            }

            if (!value.TryGetBoolean(out var flag, out var error))
            {
                return error;
            }

            seen = true;
            result = all ? result && flag : result || flag;
        }

        return seen ? SheetValue.FromBoolean(result) : SheetValue.Error(SheetValue.ValueError);
    }

    private static SheetValue Not(IReadOnlyList<FormulaNode> arguments, IFormulaContext context) =>
        FormulaEvaluator.Evaluate(arguments[0], context).TryGetBoolean(out var flag, out var error) ? SheetValue.FromBoolean(!flag) : error;

    private static SheetValue Numeric(IReadOnlyList<FormulaNode> arguments, IFormulaContext context, Func<double, double> function) =>
        FormulaEvaluator.Evaluate(arguments[0], context).TryGetNumber(out var number, out var error) ? SheetValue.FromNumber(function(number)) : error;

    private static SheetValue Round(IReadOnlyList<FormulaNode> arguments, IFormulaContext context, MidpointRounding mode, bool away = false)
    {
        if (!FormulaEvaluator.Evaluate(arguments[0], context).TryGetNumber(out var number, out var error))
        {
            return error;
        }

        var digits = 0d;
        if (arguments.Count == 2 && !FormulaEvaluator.Evaluate(arguments[1], context).TryGetNumber(out digits, out error))
        {
            return error;
        }

        var places = (int)Math.Truncate(digits);
        var scale = Math.Pow(10, places);
        var scaled = number * scale;
        var rounded = away
            ? Math.Sign(scaled) * Math.Ceiling(Math.Round(Math.Abs(scaled), 9))
            : Math.Round(Math.Round(scaled, 9), mode);
        return SheetValue.FromNumber(rounded / scale);
    }

    private static SheetValue Mod(IReadOnlyList<FormulaNode> arguments, IFormulaContext context)
    {
        if (!FormulaEvaluator.Evaluate(arguments[0], context).TryGetNumber(out var number, out var error)
            || !FormulaEvaluator.Evaluate(arguments[1], context).TryGetNumber(out var divisor, out error))
        {
            return error;
        }

        // The result takes the sign of the divisor, as in a spreadsheet, unlike the % operator in C#.
        return divisor == 0 ? SheetValue.Error(SheetValue.DivideByZero) : SheetValue.FromNumber(number - (divisor * Math.Floor(number / divisor)));
    }

    private static SheetValue Text(IReadOnlyList<FormulaNode> arguments, IFormulaContext context, Func<string, SheetValue> function)
    {
        var value = FormulaEvaluator.Evaluate(arguments[0], context);
        return value.IsError ? value : function(value.AsText());
    }

    private static SheetValue Concat(IReadOnlyList<FormulaNode> arguments, IFormulaContext context)
    {
        var text = new StringBuilder();
        foreach (var (value, _) in arguments.SelectMany(a => FormulaEvaluator.Expand(a, context)))
        {
            if (value.IsError)
            {
                return value;
            }

            text.Append(value.AsText());
        }

        return SheetValue.FromText(text.ToString());
    }

    private static SheetValue Substring(IReadOnlyList<FormulaNode> arguments, IFormulaContext context, bool fromEnd)
    {
        var value = FormulaEvaluator.Evaluate(arguments[0], context);
        if (value.IsError)
        {
            return value;
        }

        var count = 1d;
        if (arguments.Count == 2 && !FormulaEvaluator.Evaluate(arguments[1], context).TryGetNumber(out count, out var error))
        {
            return error;
        }

        if (count < 0)
        {
            return SheetValue.Error(SheetValue.ValueError);
        }

        var text = value.AsText();
        var length = (int)Math.Min(text.Length, Math.Floor(count));
        return SheetValue.FromText(fromEnd ? text[^length..] : text[..length]);
    }

    private static SheetValue Mid(IReadOnlyList<FormulaNode> arguments, IFormulaContext context)
    {
        var value = FormulaEvaluator.Evaluate(arguments[0], context);
        if (value.IsError)
        {
            return value;
        }

        if (!FormulaEvaluator.Evaluate(arguments[1], context).TryGetNumber(out var start, out var error)
            || !FormulaEvaluator.Evaluate(arguments[2], context).TryGetNumber(out var count, out error))
        {
            return error;
        }

        if (start < 1 || count < 0)
        {
            return SheetValue.Error(SheetValue.ValueError);
        }

        var text = value.AsText();
        var from = (int)Math.Min(text.Length, Math.Floor(start) - 1);
        var length = (int)Math.Min(text.Length - from, Math.Floor(count));
        return SheetValue.FromText(text.Substring(from, length));
    }

    /// <summary>
    /// A condition such as <c>"&gt;5"</c> or <c>"apples"</c> for <c>COUNTIF</c> and <c>SUMIF</c>.
    /// </summary>
    private static class Criterion
    {
        private static readonly string[] _operators = ["<=", ">=", "<>", "<", ">", "="];

        /// <summary>
        /// Gets whether a cell's value meets a condition: a number or text to be equal to, optionally after a
        /// comparison operator. Text compares without regard to case, and <c>*</c> and <c>?</c> stand for any text and
        /// any one character.
        /// </summary>
        public static bool Matches(SheetValue value, SheetValue criterion)
        {
            if (value.IsError)
            {
                return false;
            }

            if (criterion.Kind != SheetValueKind.Text)
            {
                return value.Kind == criterion.Kind && FormulaEvaluator.CompareValues(value, criterion) == 0;
            }

            var text = criterion.Text ?? string.Empty;
            var op = _operators.FirstOrDefault(o => text.StartsWith(o, StringComparison.Ordinal)) ?? "=";
            var operand = text.StartsWith(op, StringComparison.Ordinal) ? text[op.Length..] : text;

            if (double.TryParse(operand, NumberStyles.Float, CultureInfo.InvariantCulture, out var number))
            {
                return value.Kind == SheetValueKind.Number && FormulaEvaluator.Compare(value, SheetValue.FromNumber(number), op).Number != 0;
            }

            if (op is "=" or "<>")
            {
                var equal = value.Kind != SheetValueKind.Number && Wildcard(value.AsText(), operand);
                return op == "=" ? equal : !equal;
            }

            return value.Kind == SheetValueKind.Text && FormulaEvaluator.Compare(value, SheetValue.FromText(operand), op).Number != 0;
        }

        private static bool Wildcard(string text, string pattern) => Wildcard(text.AsSpan(), pattern.AsSpan());

        private static bool Wildcard(ReadOnlySpan<char> text, ReadOnlySpan<char> pattern)
        {
            while (pattern.Length > 0)
            {
                if (pattern[0] == '*')
                {
                    var rest = pattern[1..];
                    for (var i = 0; i <= text.Length; i++)
                    {
                        if (Wildcard(text[i..], rest))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                if (text.Length == 0 || (pattern[0] != '?' && char.ToUpperInvariant(pattern[0]) != char.ToUpperInvariant(text[0])))
                {
                    return false;
                }

                text = text[1..];
                pattern = pattern[1..];
            }

            return text.Length == 0;
        }
    }
}
