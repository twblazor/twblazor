// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using TwBlazor.Enums;

namespace TwBlazor.Models;

/// <summary>
/// One condition of a <see cref="ColumnFilter"/>, such as "is greater than 100".
/// </summary>
[ExcludeFromCodeCoverage]
public sealed class FilterCondition
{
    /// <summary>
    /// Gets or sets how the value is compared with <see cref="Value"/>.
    /// </summary>
    public FilterOperator Operator { get; set; }

    /// <summary>
    /// Gets or sets the value to compare with, written the same way in every culture: numbers with a <c>.</c> for the
    /// decimal point, dates as <c>yyyy-MM-dd</c>, <c>true</c> or <c>false</c>, and enum values by name. A condition
    /// whose operator needs a value is ignored while the value is empty.
    /// </summary>
    public string? Value { get; set; }

    /// <summary>
    /// Gets whether the condition takes part in filtering: it needs no value, or it has one.
    /// </summary>
    public bool IsActive => Operator is FilterOperator.IsEmpty or FilterOperator.IsNotEmpty
        || !string.IsNullOrWhiteSpace(Value);

    /// <summary>
    /// Creates a copy of the condition.
    /// </summary>
    /// <returns>A new <see cref="FilterCondition"/> with the same values.</returns>
    public FilterCondition Clone() => new() { Operator = Operator, Value = Value };
}
