// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using TwBlazor.Enums;

namespace TwBlazor.Models;

/// <summary>
/// A column's filter: one condition, or two combined with <see cref="Logic"/>. It holds only plain values, so it can be
/// saved, such as with the rest of a <see cref="Components.TwDataTableState"/>, passed to a
/// <see cref="Components.TwDataTable{TItem}.ItemsProvider"/>, or kept on a <see cref="SheetTable"/>.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed class ColumnFilter
{
    /// <summary>
    /// Gets or sets the first condition.
    /// </summary>
    public FilterCondition First { get; set; } = new();

    /// <summary>
    /// Gets or sets how the two conditions combine. Defaults to <see cref="FilterLogic.And"/>.
    /// </summary>
    public FilterLogic Logic { get; set; }

    /// <summary>
    /// Gets or sets the second condition, or <c>null</c> for one condition only.
    /// </summary>
    public FilterCondition? Second { get; set; }

    /// <summary>
    /// Gets whether either condition takes part in filtering.
    /// </summary>
    public bool IsActive => First.IsActive || Second?.IsActive == true;

    /// <summary>
    /// Creates a deep copy of the filter.
    /// </summary>
    /// <returns>A new <see cref="ColumnFilter"/> with the same values.</returns>
    public ColumnFilter Clone() => new() { First = First.Clone(), Logic = Logic, Second = Second?.Clone() };

    /// <summary>
    /// Gets whether a value passes the filter, with the same rules as the filters of
    /// <see cref="Components.TwDataTable{TItem}"/> and <see cref="Components.TwSpreadsheet"/>. Conditions that are not
    /// active are ignored, so an empty filter passes everything.
    /// </summary>
    /// <param name="value">The value, such as a number, a date, <see langword="true"/> or text.</param>
    /// <param name="type">The kind of values being filtered, which decides how they are compared. Defaults to
    /// <see cref="FilterType.Text"/>.</param>
    /// <returns><see langword="true"/> when the value passes.</returns>
    public bool Matches(object? value, FilterType type = FilterType.Text) =>
        Filtering.ColumnFilterEvaluator.Matches(value, this, type == FilterType.Auto ? FilterType.Text : type);
}
