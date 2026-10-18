// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

namespace TwBlazor.Components;

/// <summary>
/// How a <see cref="TwDataTableFilterCondition"/> compares a cell's value with the value a person entered.
/// </summary>
public enum TwDataTableFilterOperator
{
    /// <summary>The text contains the value, ignoring case.</summary>
    Contains,

    /// <summary>The text does not contain the value, ignoring case.</summary>
    DoesNotContain,

    /// <summary>The text starts with the value, ignoring case.</summary>
    StartsWith,

    /// <summary>The text ends with the value, ignoring case.</summary>
    EndsWith,

    /// <summary>The cell equals the value. Text is compared ignoring case, and dates by day.</summary>
    IsEqualTo,

    /// <summary>The cell does not equal the value.</summary>
    IsNotEqualTo,

    /// <summary>The number or date is greater than, or after, the value.</summary>
    IsGreaterThan,

    /// <summary>The number or date is greater than or equal to the value.</summary>
    IsGreaterThanOrEqualTo,

    /// <summary>The number or date is less than, or before, the value.</summary>
    IsLessThan,

    /// <summary>The number or date is less than or equal to the value.</summary>
    IsLessThanOrEqualTo,

    /// <summary>The cell has no value, or empty text. Needs no value of its own.</summary>
    IsEmpty,

    /// <summary>The cell has a value. Needs no value of its own.</summary>
    IsNotEmpty
}

/// <summary>
/// How the two conditions of a <see cref="TwDataTableColumnFilter"/> combine.
/// </summary>
public enum TwDataTableFilterLogic
{
    /// <summary>A row must match both conditions.</summary>
    And,

    /// <summary>A row must match at least one condition.</summary>
    Or
}

/// <summary>
/// The kind of values a column filters, which decides its conditions and the field a person types the value into.
/// </summary>
public enum TwDataTableFilterType
{
    /// <summary>Worked out from the column's values: text, a number, a date, true or false, or an enum.</summary>
    Auto,

    /// <summary>Text, compared ignoring case.</summary>
    Text,

    /// <summary>Numbers of any built in numeric type.</summary>
    Number,

    /// <summary>Dates, compared by day. <see cref="DateTime"/>, <see cref="DateTimeOffset"/> and <see cref="DateOnly"/>.</summary>
    Date,

    /// <summary>True or false values, offered as Yes and No.</summary>
    Boolean,

    /// <summary>The values of an enum, offered as a list.</summary>
    Enum
}

/// <summary>
/// One condition of a <see cref="TwDataTableColumnFilter"/>, such as "is greater than 100".
/// </summary>
public sealed class TwDataTableFilterCondition
{
    /// <summary>
    /// Gets or sets how the cell is compared with <see cref="Value"/>.
    /// </summary>
    public TwDataTableFilterOperator Operator { get; set; }

    /// <summary>
    /// Gets or sets the value to compare with, written the same way in every culture: numbers with a <c>.</c> for the
    /// decimal point, dates as <c>yyyy-MM-dd</c>, <c>true</c> or <c>false</c>, and enum values by name. A condition
    /// whose operator needs a value is ignored while the value is empty.
    /// </summary>
    public string? Value { get; set; }

    /// <summary>
    /// Gets whether the condition takes part in filtering: it needs no value, or it has one.
    /// </summary>
    public bool IsActive => Operator is TwDataTableFilterOperator.IsEmpty or TwDataTableFilterOperator.IsNotEmpty
        || !string.IsNullOrWhiteSpace(Value);

    /// <summary>
    /// Creates a copy of the condition.
    /// </summary>
    /// <returns>A new <see cref="TwDataTableFilterCondition"/> with the same values.</returns>
    public TwDataTableFilterCondition Clone() => new() { Operator = Operator, Value = Value };
}

/// <summary>
/// A column's filter: one condition, or two combined with <see cref="Logic"/>. It holds only plain values, so it is
/// saved with the rest of a <see cref="TwDataTableState"/> and passed to an
/// <see cref="TwDataTable{TItem}.ItemsProvider"/> in <see cref="TwDataTableItemsRequest.Filters"/>.
/// </summary>
public sealed class TwDataTableColumnFilter
{
    /// <summary>
    /// Gets or sets the first condition.
    /// </summary>
    public TwDataTableFilterCondition First { get; set; } = new();

    /// <summary>
    /// Gets or sets how the two conditions combine. Defaults to <see cref="TwDataTableFilterLogic.And"/>.
    /// </summary>
    public TwDataTableFilterLogic Logic { get; set; }

    /// <summary>
    /// Gets or sets the second condition, or <c>null</c> for one condition only.
    /// </summary>
    public TwDataTableFilterCondition? Second { get; set; }

    /// <summary>
    /// Gets whether either condition takes part in filtering.
    /// </summary>
    public bool IsActive => First.IsActive || Second?.IsActive == true;

    /// <summary>
    /// Creates a deep copy of the filter.
    /// </summary>
    /// <returns>A new <see cref="TwDataTableColumnFilter"/> with the same values.</returns>
    public TwDataTableColumnFilter Clone() => new() { First = First.Clone(), Logic = Logic, Second = Second?.Clone() };
}
