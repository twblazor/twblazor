// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using System.Globalization;
using TwBlazor.Enums;
using TwBlazor.Models;

namespace TwBlazor.Filtering;

/// <summary>
/// Decides whether a value passes a <see cref="ColumnFilter"/>. <see cref="Components.TwDataTable{TItem}"/> and
/// <see cref="Components.TwSpreadsheet"/> both filter with it, so a condition means the same in each.
/// </summary>
internal static class ColumnFilterEvaluator
{
    /// <summary>
    /// Gets the conditions text columns offer by default.
    /// </summary>
    public static IReadOnlyList<FilterOperator> TextOperators { get; } =
    [
        FilterOperator.Contains,
        FilterOperator.DoesNotContain,
        FilterOperator.StartsWith,
        FilterOperator.EndsWith,
        FilterOperator.IsEqualTo,
        FilterOperator.IsNotEqualTo,
        FilterOperator.IsEmpty,
        FilterOperator.IsNotEmpty,
    ];

    /// <summary>
    /// Gets the conditions number and date columns offer by default.
    /// </summary>
    public static IReadOnlyList<FilterOperator> RangeOperators { get; } =
    [
        FilterOperator.IsEqualTo,
        FilterOperator.IsNotEqualTo,
        FilterOperator.IsGreaterThan,
        FilterOperator.IsGreaterThanOrEqualTo,
        FilterOperator.IsLessThan,
        FilterOperator.IsLessThanOrEqualTo,
        FilterOperator.IsEmpty,
        FilterOperator.IsNotEmpty,
    ];

    /// <summary>
    /// Gets the conditions true or false and list columns offer by default.
    /// </summary>
    public static IReadOnlyList<FilterOperator> ChoiceOperators { get; } =
    [
        FilterOperator.IsEqualTo,
        FilterOperator.IsNotEqualTo,
    ];

    /// <summary>
    /// Gets the conditions a kind of column offers by default.
    /// </summary>
    /// <param name="type">The kind of values.</param>
    /// <returns>The conditions.</returns>
    public static IReadOnlyList<FilterOperator> OperatorsFor(FilterType type) => type switch
    {
        FilterType.Number or FilterType.Date => RangeOperators,
        FilterType.Boolean or FilterType.Enum => ChoiceOperators,
        _ => TextOperators,
    };

    /// <summary>
    /// Gets whether a condition needs a value typed in.
    /// </summary>
    /// <param name="op">The condition.</param>
    /// <returns><see langword="false"/> for <see cref="FilterOperator.IsEmpty"/> and
    /// <see cref="FilterOperator.IsNotEmpty"/>.</returns>
    public static bool NeedsValue(FilterOperator op) => op is not (FilterOperator.IsEmpty or FilterOperator.IsNotEmpty);

    /// <summary>
    /// Gets whether a value passes a filter. Conditions that are not active are ignored.
    /// </summary>
    /// <param name="value">The value, such as a number, a date, <see langword="true"/> or text.</param>
    /// <param name="filter">The filter.</param>
    /// <param name="type">The kind of values the column holds.</param>
    /// <returns><see langword="true"/> when the value passes.</returns>
    public static bool Matches(object? value, ColumnFilter filter, FilterType type)
    {
        var first = filter.First.IsActive;
        var second = filter.Second?.IsActive == true;

        if (first && second)
        {
            var a = Evaluate(value, filter.First, type);
            var b = Evaluate(value, filter.Second!, type);
            return filter.Logic == FilterLogic.And ? a && b : a || b;
        }

        if (first)
        {
            return Evaluate(value, filter.First, type);
        }

        return !second || Evaluate(value, filter.Second!, type);
    }

    private static bool IsEmptyValue(object? value) => value is null || (value is string s && string.IsNullOrEmpty(s));

    /// <summary>
    /// Compares a value with one condition. A condition whose value cannot be read for the column's type matches
    /// everything, so a half typed value never hides every row.
    /// </summary>
    private static bool Evaluate(object? value, FilterCondition condition, FilterType type)
    {
        switch (condition.Operator)
        {
            case FilterOperator.IsEmpty:
                return IsEmptyValue(value);
            case FilterOperator.IsNotEmpty:
                return !IsEmptyValue(value);
        }

        var target = condition.Value!.Trim();
        return type switch
        {
            FilterType.Number => CompareNumber(value, target, condition.Operator),
            FilterType.Date => CompareDate(value, target, condition.Operator),
            FilterType.Boolean or FilterType.Enum => CompareChoice(value, target, condition.Operator),
            _ => CompareText(value, target, condition.Operator),
        };
    }

    private static bool CompareText(object? value, string target, FilterOperator op)
    {
        var text = value?.ToString() ?? string.Empty;
        const StringComparison comparison = StringComparison.CurrentCultureIgnoreCase;
        return op switch
        {
            FilterOperator.Contains => text.Contains(target, comparison),
            FilterOperator.DoesNotContain => !text.Contains(target, comparison),
            FilterOperator.StartsWith => text.StartsWith(target, comparison),
            FilterOperator.EndsWith => text.EndsWith(target, comparison),
            FilterOperator.IsNotEqualTo => !string.Equals(text, target, comparison),
            _ => string.Equals(text, target, comparison),
        };
    }

    private static bool CompareNumber(object? value, string target, FilterOperator op)
    {
        if (!decimal.TryParse(target, NumberStyles.Number, CultureInfo.InvariantCulture, out var number))
        {
            return true;
        }

        // A value with no number, or one decimal cannot hold, only passes a "not equal to" condition.
        if (value is null || (value is double d && !double.IsFinite(d)) || (value is float f && !float.IsFinite(f)))
        {
            return op == FilterOperator.IsNotEqualTo;
        }

        decimal cell;
        try
        {
            cell = Convert.ToDecimal(value, CultureInfo.InvariantCulture);
        }
        catch (Exception exception) when (exception is FormatException or InvalidCastException or OverflowException)
        {
            return op == FilterOperator.IsNotEqualTo;
        }

        return MatchesComparison(cell.CompareTo(number), op);
    }

    private static bool CompareDate(object? value, string target, FilterOperator op)
    {
        if (!DateOnly.TryParseExact(target, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
        {
            return true;
        }

        DateOnly? cell = value switch
        {
            DateTime d => DateOnly.FromDateTime(d),
            DateTimeOffset d => DateOnly.FromDateTime(d.DateTime),
            DateOnly d => d,
            _ => null,
        };

        return cell is { } day ? MatchesComparison(day.CompareTo(date), op) : op == FilterOperator.IsNotEqualTo;
    }

    private static bool CompareChoice(object? value, string target, FilterOperator op)
    {
        var equal = value is bool b
            ? bool.TryParse(target, out var expected) && b == expected
            : string.Equals(value?.ToString(), target, StringComparison.Ordinal);
        return op == FilterOperator.IsNotEqualTo ? !equal : equal;
    }

    private static bool MatchesComparison(int comparison, FilterOperator op) => op switch
    {
        FilterOperator.IsNotEqualTo => comparison != 0,
        FilterOperator.IsGreaterThan => comparison > 0,
        FilterOperator.IsGreaterThanOrEqualTo => comparison >= 0,
        FilterOperator.IsLessThan => comparison < 0,
        FilterOperator.IsLessThanOrEqualTo => comparison <= 0,
        _ => comparison == 0,
    };
}
