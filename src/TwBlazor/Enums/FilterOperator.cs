// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

namespace TwBlazor.Enums;

/// <summary>
/// How a <see cref="TwBlazor.Models.FilterCondition"/> compares a value with the value a person entered, in the column
/// filters of <see cref="TwBlazor.Components.TwDataTable{TItem}"/> and the table filters of
/// <see cref="TwBlazor.Components.TwSpreadsheet"/>.
/// </summary>
public enum FilterOperator
{
    /// <summary>The text contains the value, ignoring case.</summary>
    Contains,

    /// <summary>The text does not contain the value, ignoring case.</summary>
    DoesNotContain,

    /// <summary>The text starts with the value, ignoring case.</summary>
    StartsWith,

    /// <summary>The text ends with the value, ignoring case.</summary>
    EndsWith,

    /// <summary>The value equals the one entered. Text is compared ignoring case, and dates by day.</summary>
    IsEqualTo,

    /// <summary>The value does not equal the one entered.</summary>
    IsNotEqualTo,

    /// <summary>The number or date is greater than, or after, the value.</summary>
    IsGreaterThan,

    /// <summary>The number or date is greater than or equal to the value.</summary>
    IsGreaterThanOrEqualTo,

    /// <summary>The number or date is less than, or before, the value.</summary>
    IsLessThan,

    /// <summary>The number or date is less than or equal to the value.</summary>
    IsLessThanOrEqualTo,

    /// <summary>There is no value, or empty text. Needs no value of its own.</summary>
    IsEmpty,

    /// <summary>There is a value. Needs no value of its own.</summary>
    IsNotEmpty,
}
