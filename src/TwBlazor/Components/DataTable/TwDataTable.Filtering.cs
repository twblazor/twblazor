// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using System.Globalization;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using TwBlazor.Localization;
using TwBlazor.Utilities;

namespace TwBlazor.Components;

public partial class TwDataTable<TItem>
{
    private string? filterColumn;
    private TwDataTableColumnFilter filterDraft = new();
    private bool filterPanelOpening;
    private bool filterScriptOpen;
    private ElementReference filterPanelRef;

    private string filterPanelId => $"{Id}-filter";

    private string filterHeadingId => $"{Id}-filter-heading";

    private string FilterButtonId(TwDataTableColumn<TItem> column) => $"{Id}-col-{column.Name}-filter";

    private TwDataTableColumn<TItem>? openFilterColumn => FindColumn(filterColumn);

    private string filterFormClasses => new ClassBuilder(options.Theme.Display.Flex)
        .AddClass(options.Theme.Flexbox.Col)
        .AddClass(options.Theme.Spacing.Gap.Md).Build();

    private string filterButtonsClasses => new ClassBuilder(options.Theme.Display.Flex)
        .AddClass(options.Theme.Spacing.Gap.Sm).Build();

    private TwDataTableColumnFilter? FilterOf(string columnName) =>
        columnStates.FirstOrDefault(s => s.Name == columnName)?.Filter;

    private bool IsFiltered(TwDataTableColumn<TItem> column) => FilterOf(column.Name)?.IsActive == true;

    private string FilterButtonLabel(TwDataTableColumn<TItem> column) => IsFiltered(column)
        ? localizer[TwStrings.DataTable_FilterColumnActive, column.Title]
        : localizer[TwStrings.DataTable_FilterColumn, column.Title];

    private Dictionary<string, object> FilterButtonAttributes(TwDataTableColumn<TItem> column) => new()
    {
        ["aria-haspopup"] = "dialog",
        ["aria-expanded"] = filterColumn == column.Name ? "true" : "false",
        ["aria-controls"] = filterPanelId
    };

    private Dictionary<string, object> FilterLogicAttributes(TwDataTableFilterLogic logic) => new()
    {
        ["aria-pressed"] = filterDraft.Logic == logic ? "true" : "false"
    };

    /// <summary>
    /// Gets every filtered column's filter, keyed by column name, for an <see cref="ItemsProvider"/>.
    /// </summary>
    private Dictionary<string, TwDataTableColumnFilter> ActiveFilters() => columnStates
        .Where(s => s.Filter?.IsActive == true && columnsByName.ContainsKey(s.Name))
        .ToDictionary(s => s.Name, s => s.Filter!.Clone());

    /// <summary>
    /// Works out the kind of values a column filters, from its <see cref="TwDataTableColumn{TItem}.FilterType"/> or
    /// else from the first value it has.
    /// </summary>
    private TwDataTableFilterType ResolveFilterType(TwDataTableColumn<TItem> column)
    {
        if (column.FilterType != TwDataTableFilterType.Auto)
        {
            return column.FilterType;
        }

        var type = SampleValueType(column);
        return type switch
        {
            null => TwDataTableFilterType.Text,
            _ when type == typeof(bool) => TwDataTableFilterType.Boolean,
            _ when type == typeof(DateTime) || type == typeof(DateTimeOffset) || type == typeof(DateOnly) => TwDataTableFilterType.Date,
            { IsEnum: true } => TwDataTableFilterType.Enum,
            _ when IsNumberType(type) => TwDataTableFilterType.Number,
            _ => TwDataTableFilterType.Text
        };
    }

    private Type? SampleValueType(TwDataTableColumn<TItem> column)
    {
        if (column.PropertySelector is not { } selector)
        {
            return null;
        }

        var rows = usesProvider ? loadedItems : Items;
        return rows.Where(r => !EqualityComparer<TItem>.Default.Equals(r, default))
            .Select(r => selector(r))
            .FirstOrDefault(v => v is not null)?.GetType();
    }

    private static bool IsNumberType(Type type) => type == typeof(int) || type == typeof(long) || type == typeof(short)
        || type == typeof(byte) || type == typeof(decimal) || type == typeof(double) || type == typeof(float)
        || type == typeof(uint) || type == typeof(ulong) || type == typeof(ushort) || type == typeof(sbyte);

    private IReadOnlyList<TwDataTableFilterOperator> OperatorsFor(TwDataTableColumn<TItem> column)
    {
        if (column.FilterOperators is { Count: > 0 } chosen)
        {
            return chosen;
        }

        return ResolveFilterType(column) switch
        {
            TwDataTableFilterType.Number or TwDataTableFilterType.Date => TwDataTableFilterOperatorSets.Range,
            TwDataTableFilterType.Boolean or TwDataTableFilterType.Enum => TwDataTableFilterOperatorSets.Choice,
            _ => TwDataTableFilterOperatorSets.Text
        };
    }

    private string OperatorText(TwDataTableFilterOperator op) => localizer[$"DataTable_Operator{op}"];

    private List<FilterOperatorOption> OperatorOptions(TwDataTableColumn<TItem> column) =>
        [.. OperatorsFor(column).Select(op => new FilterOperatorOption(op, OperatorText(op)))];

    private List<FilterValueOption> ValueOptions(TwDataTableColumn<TItem> column)
    {
        if (ResolveFilterType(column) == TwDataTableFilterType.Boolean)
        {
            return [new("true", localizer[TwStrings.DataTable_Yes]), new("false", localizer[TwStrings.DataTable_No])];
        }

        return SampleValueType(column) is { IsEnum: true } enumType
            ? [.. Enum.GetNames(enumType).Select(name => new FilterValueOption(name, name))]
            : [];
    }

    private static bool NeedsValue(TwDataTableFilterOperator op) =>
        op is not (TwDataTableFilterOperator.IsEmpty or TwDataTableFilterOperator.IsNotEmpty);

    /// <summary>
    /// Whether a row passes every column filter.
    /// </summary>
    private static bool PassesFilters(TItem item, IReadOnlyList<(TwDataTableColumn<TItem> Column, TwDataTableColumnFilter Filter, TwDataTableFilterType Type)> filters) =>
        filters.All(f => Matches(f.Column.PropertySelector!(item), f.Filter, f.Type));

    private static bool Matches(object? value, TwDataTableColumnFilter filter, TwDataTableFilterType type)
    {
        var first = filter.First.IsActive;
        var second = filter.Second?.IsActive == true;

        if (first && second)
        {
            var a = Evaluate(value, filter.First, type);
            var b = Evaluate(value, filter.Second!, type);
            return filter.Logic == TwDataTableFilterLogic.And ? a && b : a || b;
        }

        if (first)
        {
            return Evaluate(value, filter.First, type);
        }

        return !second || Evaluate(value, filter.Second!, type);
    }

    private static bool IsEmptyValue(object? value) => value is null || (value is string s && string.IsNullOrEmpty(s));

    /// <summary>
    /// Compares a cell's value with one condition. A condition whose value cannot be read for the column's type
    /// matches every row, so a half typed value never empties the table.
    /// </summary>
    private static bool Evaluate(object? value, TwDataTableFilterCondition condition, TwDataTableFilterType type)
    {
        switch (condition.Operator)
        {
            case TwDataTableFilterOperator.IsEmpty:
                return IsEmptyValue(value);
            case TwDataTableFilterOperator.IsNotEmpty:
                return !IsEmptyValue(value);
        }

        var target = condition.Value!.Trim();
        return type switch
        {
            TwDataTableFilterType.Number => CompareNumber(value, target, condition.Operator),
            TwDataTableFilterType.Date => CompareDate(value, target, condition.Operator),
            TwDataTableFilterType.Boolean or TwDataTableFilterType.Enum => CompareChoice(value, target, condition.Operator),
            _ => CompareText(value, target, condition.Operator)
        };
    }

    private static bool CompareText(object? value, string target, TwDataTableFilterOperator op)
    {
        var text = value?.ToString() ?? string.Empty;
        const StringComparison comparison = StringComparison.CurrentCultureIgnoreCase;
        return op switch
        {
            TwDataTableFilterOperator.Contains => text.Contains(target, comparison),
            TwDataTableFilterOperator.DoesNotContain => !text.Contains(target, comparison),
            TwDataTableFilterOperator.StartsWith => text.StartsWith(target, comparison),
            TwDataTableFilterOperator.EndsWith => text.EndsWith(target, comparison),
            TwDataTableFilterOperator.IsNotEqualTo => !string.Equals(text, target, comparison),
            _ => string.Equals(text, target, comparison)
        };
    }

    private static bool CompareNumber(object? value, string target, TwDataTableFilterOperator op)
    {
        if (!decimal.TryParse(target, NumberStyles.Number, CultureInfo.InvariantCulture, out var number))
        {
            return true;
        }

        // A cell with no number, or one decimal cannot hold, only passes a "not equal to" condition.
        if (value is null || (value is double d && !double.IsFinite(d)) || (value is float f && !float.IsFinite(f)))
        {
            return op == TwDataTableFilterOperator.IsNotEqualTo;
        }

        decimal cell;
        try
        {
            cell = Convert.ToDecimal(value, CultureInfo.InvariantCulture);
        }
        catch (Exception exception) when (exception is FormatException or InvalidCastException or OverflowException)
        {
            return op == TwDataTableFilterOperator.IsNotEqualTo;
        }

        return MatchesComparison(cell.CompareTo(number), op);
    }

    private static bool CompareDate(object? value, string target, TwDataTableFilterOperator op)
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
            _ => null
        };

        return cell is { } day ? MatchesComparison(day.CompareTo(date), op) : op == TwDataTableFilterOperator.IsNotEqualTo;
    }

    private static bool CompareChoice(object? value, string target, TwDataTableFilterOperator op)
    {
        var equal = value is bool b
            ? bool.TryParse(target, out var expected) && b == expected
            : string.Equals(value?.ToString(), target, StringComparison.Ordinal);
        return op == TwDataTableFilterOperator.IsNotEqualTo ? !equal : equal;
    }

    private static bool MatchesComparison(int comparison, TwDataTableFilterOperator op) => op switch
    {
        TwDataTableFilterOperator.IsNotEqualTo => comparison != 0,
        TwDataTableFilterOperator.IsGreaterThan => comparison > 0,
        TwDataTableFilterOperator.IsGreaterThanOrEqualTo => comparison >= 0,
        TwDataTableFilterOperator.IsLessThan => comparison < 0,
        TwDataTableFilterOperator.IsLessThanOrEqualTo => comparison <= 0,
        _ => comparison == 0
    };

    /// <summary>
    /// Opens a column's filter, starting from its current filter or the column's first condition.
    /// </summary>
    private async Task OpenFilterAsync(TwDataTableColumn<TItem> column)
    {
        if (filterColumn == column.Name)
        {
            await CloseFilterAsync();
            return;
        }

        if (filterColumn is not null)
        {
            await CloseFilterScriptAsync();
        }

        var operators = OperatorsFor(column);
        filterDraft = FilterOf(column.Name)?.Clone() ?? new TwDataTableColumnFilter { First = new() { Operator = operators[0] } };
        filterDraft.Second ??= new TwDataTableFilterCondition { Operator = operators[0] };
        filterColumn = column.Name;
        filterPanelOpening = true;
    }

    private async Task SyncFilterPanelAsync()
    {
        if (!filterPanelOpening || filterColumn is null || openFilterColumn is not { } column)
        {
            return;
        }

        filterPanelOpening = false;
        filterScriptOpen = true;
        selfReference ??= DotNetObjectReference.Create(this);
        await InvokeScriptAsync("twDataTable.openFilter", filterPanelRef, FilterButtonId(column), selfReference);
    }

    /// <summary>
    /// Closes the open column filter without changing it, and returns focus to its button. The Escape key and a
    /// click outside the filter call this too.
    /// </summary>
    /// <returns>A task that completes once the filter is closed.</returns>
    [JSInvokable("Close")]
    public async Task CloseFilterAsync()
    {
        if (filterColumn is null)
        {
            return;
        }

        var column = openFilterColumn;
        await CloseFilterScriptAsync();
        filterColumn = null;
        filterPanelOpening = false;
        pendingFocusId = column is null ? null : FilterButtonId(column);
        await InvokeAsync(StateHasChanged);
    }

    private async Task CloseFilterScriptAsync()
    {
        if (!filterScriptOpen)
        {
            return;
        }

        filterScriptOpen = false;
        await InvokeScriptAsync("twDataTable.closeFilter", filterPanelRef);
    }

    private void SetFilterLogic(TwDataTableFilterLogic logic) => filterDraft.Logic = logic;

    private async Task ApplyFilterAsync()
    {
        if (openFilterColumn is not { } column)
        {
            return;
        }

        var filter = filterDraft.Clone();
        if (filter.Second is { IsActive: false })
        {
            filter.Second = null;
        }

        await SetColumnFilterAsync(column, filter.IsActive ? filter : null);
    }

    private async Task ClearFilterAsync()
    {
        if (openFilterColumn is { } column)
        {
            await SetColumnFilterAsync(column, null);
        }
    }

    private async Task SetColumnFilterAsync(TwDataTableColumn<TItem> column, TwDataTableColumnFilter? filter)
    {
        if (columnStates.FirstOrDefault(s => s.Name == column.Name) is { } state)
        {
            state.Filter = filter;
        }

        await CloseFilterAsync();
        RefreshView(resetPage: true);

        Announce(() => filter is null
            ? localizer[TwStrings.DataTable_FilterRemoved, column.Title]
            : localizer.Plural(MatchingCount(), TwStrings.DataTable_FilterAppliedOne, TwStrings.DataTable_FilterAppliedOther, column.Title, MatchingCount()));

        await NotifyStateChangedAsync();
    }

    private int MatchingCount() => usesProvider ? providerTotalCount : viewItems.Count;

    /// <summary>
    /// A condition the filter offers, with its localized name.
    /// </summary>
    private sealed record FilterOperatorOption(TwDataTableFilterOperator Operator, string Text);

    /// <summary>
    /// A value a true or false, or enum, filter offers.
    /// </summary>
    private sealed record FilterValueOption(string Value, string Text);
}
