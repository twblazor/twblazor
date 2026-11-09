// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using TwBlazor.Models;
using TwBlazor.Filtering;
using TwBlazor.Enums;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Globalization;
using TwBlazor.Localization;
using TwBlazor.Utilities;

namespace TwBlazor.Components;

public partial class TwDataTable<TItem>
{
    private string? filterColumn;
    private ColumnFilter filterDraft = new();
    private bool filterPanelOpening;
    private bool filterScriptOpen;
    private ElementReference filterPanelRef;

    private string filterPanelId => $"{Id}-filter";

    private string filterHeadingId => $"{Id}-filter-heading";

    private string FilterButtonId(TwDataTableColumn<TItem> column) => $"{Id}-col-{column.Name}-filter";

    private TwDataTableColumn<TItem>? openFilterColumn => FindColumn(filterColumn);

    private string filterFormClasses => new ClassBuilder(options.Theme.Display.Flex)
        .AddClass(options.Theme.Flexbox.Col)
        .AddClass(options.Theme.Spacing.Gap.Get(2)).Build();

    private string filterButtonsClasses => new ClassBuilder(options.Theme.Display.Flex)
        .AddClass(options.Theme.Spacing.Gap.Get(1)).Build();

    private ColumnFilter? FilterOf(string columnName) =>
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

    /// <summary>
    /// Gets every filtered column's filter, keyed by column name, for an <see cref="ItemsProvider"/>.
    /// </summary>
    private Dictionary<string, ColumnFilter> ActiveFilters() => columnStates
        .Where(s => s.Filter?.IsActive == true && columnsByName.ContainsKey(s.Name))
        .ToDictionary(s => s.Name, s => s.Filter!.Clone());

    /// <summary>
    /// Works out the kind of values a column filters, from its <see cref="TwDataTableColumn{TItem}.FilterType"/> or
    /// else from the first value it has.
    /// </summary>
    private FilterType ResolveFilterType(TwDataTableColumn<TItem> column)
    {
        if (column.FilterType != FilterType.Auto)
        {
            return column.FilterType;
        }

        var type = SampleValueType(column);
        return type switch
        {
            null => FilterType.Text,
            _ when type == typeof(bool) => FilterType.Boolean,
            _ when type == typeof(DateTime) || type == typeof(DateTimeOffset) || type == typeof(DateOnly) => FilterType.Date,
            { IsEnum: true } => FilterType.Enum,
            _ when IsNumberType(type) => FilterType.Number,
            _ => FilterType.Text
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

    private IReadOnlyList<FilterOperator> OperatorsFor(TwDataTableColumn<TItem> column)
    {
        if (column.FilterOperators is { Count: > 0 } chosen)
        {
            return chosen;
        }

        return ColumnFilterEvaluator.OperatorsFor(ResolveFilterType(column));
    }

    /// <summary>
    /// The values an enum column's filter offers, by name, or <see langword="null"/> for other columns, whose values
    /// the filter editor works out itself.
    /// </summary>
    private IReadOnlyList<KeyValuePair<string, string>>? ValueChoices(TwDataTableColumn<TItem> column) =>
        ResolveFilterType(column) == FilterType.Enum && SampleValueType(column) is { IsEnum: true } enumType
            ? [.. Enum.GetNames(enumType).Select(name => new KeyValuePair<string, string>(name, name))]
            : null;

    /// <summary>
    /// Whether a row passes every column filter.
    /// </summary>
    private static bool PassesFilters(TItem item, IReadOnlyList<(TwDataTableColumn<TItem> Column, ColumnFilter Filter, FilterType Type)> filters) =>
        filters.All(f => ColumnFilterEvaluator.Matches(f.Column.PropertySelector!(item), f.Filter, f.Type));

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
        filterDraft = FilterOf(column.Name)?.Clone() ?? new ColumnFilter { First = new() { Operator = operators[0] } };
        filterDraft.Second ??= new FilterCondition { Operator = operators[0] };
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

    private async Task SetColumnFilterAsync(TwDataTableColumn<TItem> column, ColumnFilter? filter)
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
}
