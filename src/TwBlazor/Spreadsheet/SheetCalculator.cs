// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using System.Globalization;
using TwBlazor.Models;

namespace TwBlazor.Spreadsheet;

/// <summary>
/// Works out the value of every cell of a workbook, following formulas across cells and sheets. Results are kept until
/// <see cref="Invalidate"/> is called after a change.
/// </summary>
internal sealed class SheetCalculator
{
    private readonly Func<IReadOnlyList<Sheet>> _sheets;

    private readonly Dictionary<(int Sheet, CellAddress Address), SheetValue> _results = [];

    private readonly HashSet<(int Sheet, CellAddress Address)> _inProgress = [];

    private readonly Dictionary<string, FormulaNode?> _parsed = new(StringComparer.Ordinal);

    /// <summary>
    /// Creates a calculator over a workbook.
    /// </summary>
    /// <param name="sheets">Gets the workbook's sheets, read each time so the calculator follows a replaced list.</param>
    public SheetCalculator(Func<IReadOnlyList<Sheet>> sheets) => _sheets = sheets;

    /// <summary>
    /// Forgets every result, so the next read works them out again from the cells.
    /// </summary>
    public void Invalidate()
    {
        _results.Clear();
        if (_parsed.Count > 4096)
        {
            _parsed.Clear();
        }
    }

    /// <summary>
    /// Gets the value of a cell.
    /// </summary>
    /// <param name="sheet">The index of the sheet.</param>
    /// <param name="address">The cell.</param>
    /// <returns>The value, or <see cref="SheetValue.CircularError"/> for a formula that depends on itself.</returns>
    public SheetValue GetValue(int sheet, CellAddress address)
    {
        var workbook = _sheets();
        if (sheet < 0 || sheet >= workbook.Count)
        {
            return SheetValue.Error(SheetValue.ReferenceError);
        }

        var key = (sheet, address);
        if (_results.TryGetValue(key, out var cached))
        {
            return cached;
        }

        if (!_inProgress.Add(key))
        {
            return SheetValue.Error(SheetValue.CircularError);
        }

        try
        {
            var raw = workbook[sheet].Cells.TryGetValue(address.ToString(), out var cell) ? cell.Value : null;
            var value = Calculate(raw, sheet);
            _results[key] = value;
            return value;
        }
        finally
        {
            _inProgress.Remove(key);
        }
    }

    /// <summary>
    /// Gets whether text typed into a cell is a formula: it starts with <c>=</c> and has something after it.
    /// </summary>
    /// <param name="raw">The typed text.</param>
    /// <returns><see langword="true"/> for a formula.</returns>
    public static bool IsFormula(string? raw) => raw is { Length: > 1 } && raw[0] == '=';

    /// <summary>
    /// Reads text typed into a cell that is not a formula: a number (with an optional percent sign or thousands
    /// separators), <c>TRUE</c> or <c>FALSE</c>, or text. A leading apostrophe keeps the rest as text.
    /// </summary>
    /// <param name="raw">The typed text.</param>
    /// <returns>The value.</returns>
    public static SheetValue ParseLiteral(string? raw)
    {
        if (string.IsNullOrEmpty(raw))
        {
            return SheetValue.Empty;
        }

        if (raw[0] == '\'')
        {
            return SheetValue.FromText(raw[1..]);
        }

        var trimmed = raw.Trim();
        if (string.Equals(trimmed, "TRUE", StringComparison.OrdinalIgnoreCase))
        {
            return SheetValue.FromBoolean(true);
        }

        if (string.Equals(trimmed, "FALSE", StringComparison.OrdinalIgnoreCase))
        {
            return SheetValue.FromBoolean(false);
        }

        const NumberStyles styles = NumberStyles.Float | NumberStyles.AllowThousands;
        if (double.TryParse(trimmed, styles, CultureInfo.InvariantCulture, out var number))
        {
            return SheetValue.FromNumber(number);
        }

        if (trimmed.Length > 1 && trimmed[^1] == '%' && double.TryParse(trimmed[..^1], styles, CultureInfo.InvariantCulture, out number))
        {
            return SheetValue.FromNumber(number / 100);
        }

        return SheetValue.FromText(raw);
    }

    private SheetValue Calculate(string? raw, int sheet)
    {
        if (!IsFormula(raw))
        {
            return ParseLiteral(raw);
        }

        var formula = raw![1..];
        if (!_parsed.TryGetValue(formula, out var node))
        {
            try
            {
                node = FormulaParser.Parse(formula);
            }
            catch (FormulaSyntaxException)
            {
                node = null;
            }

            _parsed[formula] = node;
        }

        if (node is null)
        {
            return SheetValue.Error(SheetValue.SyntaxError);
        }

        var value = FormulaEvaluator.Evaluate(node, new Context(this, sheet));

        // A formula that only points at an empty cell shows 0, as in a spreadsheet.
        return value.Kind == SheetValueKind.Empty ? SheetValue.FromNumber(0) : value;
    }

    private int FindSheet(string name)
    {
        var workbook = _sheets();
        for (var i = 0; i < workbook.Count; i++)
        {
            if (string.Equals(workbook[i].Name, name, StringComparison.OrdinalIgnoreCase))
            {
                return i;
            }
        }

        return -1;
    }

    /// <summary>
    /// Reads cells for a formula on one sheet.
    /// </summary>
    private readonly struct Context(SheetCalculator calculator, int sheet) : IFormulaContext
    {
        public SheetValue GetCell(string? sheetName, CellAddress address)
        {
            var index = sheetName is null ? sheet : calculator.FindSheet(sheetName);
            return index < 0 ? SheetValue.Error(SheetValue.ReferenceError) : calculator.GetValue(index, address);
        }

        public bool HasSheet(string? sheetName) => sheetName is null || calculator.FindSheet(sheetName) >= 0;
    }
}
