// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using Microsoft.AspNetCore.Components;
using TwBlazor.Enums;
using TwBlazor.Localization;
using TwBlazor.Utilities;

namespace TwBlazor.Components;

public partial class TwDataTable<TItem>
{
    private string? currentGroupBy;
    private string? lastGroupByParameter;
    private HashSet<string> collapsedGroups = [];
    private List<TwDataTableGroup<TItem>> groups = [];
    private List<DataTableLine> lines = [];

    /// <summary>
    /// Gets or sets the <see cref="TwDataTableColumn{TItem}.Name"/> of the column to group the rows by when the
    /// table first renders. The column needs <see cref="TwDataTableColumn{TItem}.Groupable"/> set. People can change
    /// the grouping from the Group by list, which appears once any column is groupable.
    /// </summary>
    [Parameter]
    public string? GroupBy { get; set; }

    /// <summary>
    /// Gets or sets extra content for each group header, shown after the group's name and row count, such as a
    /// total. The context is the <see cref="TwDataTableGroup{TItem}"/>.
    /// </summary>
    [Parameter]
    public RenderFragment<TwDataTableGroup<TItem>>? GroupHeaderTemplate { get; set; }

    /// <summary>
    /// Gets or sets the label of the Group by list. Defaults to the localized
    /// <see cref="TwStrings.DataTable_GroupBy"/> text ("Group by").
    /// </summary>
    [Parameter]
    public string? GroupByLabel { get; set; }

    private string effectiveGroupByLabel => GroupByLabel ?? localizer[TwStrings.DataTable_GroupBy];

    private bool isGrouped => currentGroupBy is not null && hasColumns && !usesProvider;

    private bool showGroupBySelect => hasColumns && !usesProvider && Columns!.Any(c => c.Groupable && c.PropertySelector is not null);

    private List<GroupOption> groupOptions =>
    [
        new(null, localizer[TwStrings.DataTable_NoGrouping]),
        .. Columns!.Where(c => c.Groupable && c.PropertySelector is not null).Select(c => new GroupOption(c.Name, c.Title))
    ];

    private GroupOption selectedGroupOption => groupOptions.FirstOrDefault(o => o.Name == currentGroupBy) ?? groupOptions[0];

    private string groupByColumnClasses => theme.GroupByColumn;

    private string groupHeaderRowClasses => new ClassBuilder(options.Theme.Display.Flex)
        .AddClass(options.Theme.Flexbox.Wrap)
        .AddClass(options.Theme.Flexbox.Align.Center)
        .AddClass(options.Theme.Flexbox.Justify.Between)
        .AddClass(options.Theme.Spacing.Gap.Get(2)).Build();

    private Icon GroupToggleIcon(TwDataTableGroup<TItem> group) =>
        group.Expanded ? Icon.Chevron_Down : DirectionalIcon(Icon.Chevron_Right, Icon.Chevron_Left);

    private string GroupRowCount(TwDataTableGroup<TItem> group) =>
        localizer.Plural(group.Items.Count, TwStrings.DataTable_GroupRowsOne, TwStrings.DataTable_GroupRowsOther, group.Items.Count);

    /// <summary>
    /// Picks up a change to the <see cref="GroupBy"/> parameter, the same way a new <see cref="RowsPerPage"/> is.
    /// </summary>
    private void SyncGroupByParameter()
    {
        if (GroupBy != lastGroupByParameter)
        {
            lastGroupByParameter = GroupBy;
            currentGroupBy = ValidGroupColumn(GroupBy);
            currentPage = 1;
        }
    }

    private string? ValidGroupColumn(string? name) =>
        FindColumn(name) is { Groupable: true, PropertySelector: not null } column ? column.Name : null;

    /// <summary>
    /// Splits the searched and sorted rows into groups, then lays them out as a list of header and row lines for
    /// paging. The groups follow the grouped column's sort when the table is sorted by it, and run in ascending
    /// order otherwise; rows keep the table's sort inside each group.
    /// </summary>
    private void BuildGroups()
    {
        groups = [];
        lines = [];

        if (!isGrouped || FindColumn(currentGroupBy) is not { PropertySelector: { } selector } column)
        {
            return;
        }

        var descending = currentSortColumn == column.Name && currentSortDirection == SortDirection.Descending;
        var buckets = viewItems
            .GroupBy(item => selector(item))
            .Select(g => (Key: g.Key, Items: (IReadOnlyList<TItem>)[.. g]));
        buckets = descending
            ? buckets.OrderByDescending(g => g.Key, DataTableGroupKeyComparer.Instance)
            : buckets.OrderBy(g => g.Key, DataTableGroupKeyComparer.Instance);

        foreach (var (key, items) in buckets)
        {
            var label = GroupLabel(column, key, items[0]);
            var group = new TwDataTableGroup<TItem>(key, label, items, !collapsedGroups.Contains(label));
            groups.Add(group);
            lines.Add(new DataTableLine(group, default, true));

            if (group.Expanded)
            {
                lines.AddRange(items.Select(item => new DataTableLine(group, item, false)));
            }
        }
    }

    private string GroupLabel(TwDataTableColumn<TItem> column, object? key, TItem first)
    {
        var label = column.CellFormatter is { } formatter ? formatter(first) : key?.ToString();
        return string.IsNullOrWhiteSpace(label) ? localizer[TwStrings.DataTable_GroupNoValue] : label;
    }

    /// <summary>
    /// Turns the lines on the current page into one segment per group. A page that starts part way through a group
    /// repeats that group's header, so every row on screen sits under the header it belongs to.
    /// </summary>
    private static List<DataTableGroupSegment> SegmentsOf(IEnumerable<DataTableLine> pageLines)
    {
        List<DataTableGroupSegment> segments = [];
        foreach (var line in pageLines)
        {
            if (segments.Count == 0 || !ReferenceEquals(segments[^1].Group, line.Group))
            {
                segments.Add(new DataTableGroupSegment(line.Group, []));
            }

            if (!line.IsHeader)
            {
                segments[^1].Items.Add(line.Item!);
            }
        }

        return segments;
    }

    /// <summary>
    /// Groups by a column from its column menu, or stops grouping when <paramref name="column"/> is <c>null</c>.
    /// </summary>
    private Task SetGroupByAsync(TwDataTableColumn<TItem>? column) =>
        OnGroupByChangedAsync(new GroupOption(column?.Name, column?.Title ?? string.Empty));

    /// <summary>
    /// Groups by a column from its column menu, or stops grouping when the table is already grouped by it.
    /// </summary>
    private Task ToggleGroupByAsync(TwDataTableColumn<TItem> column) =>
        SetGroupByAsync(currentGroupBy == column.Name ? null : column);

    private bool CanGroupColumn(TwDataTableColumn<TItem> column) =>
        column.Groupable && column.PropertySelector != null && !usesProvider;

    private string GroupMenuLabel(TwDataTableColumn<TItem> column) =>
        currentGroupBy == column.Name ? localizer[TwStrings.DataTable_StopGrouping] : localizer[TwStrings.DataTable_GroupByColumn];

    private async Task OnGroupByChangedAsync(GroupOption option)
    {
        var groupBy = ValidGroupColumn(option.Name);
        if (groupBy == currentGroupBy)
        {
            return;
        }

        currentGroupBy = groupBy;
        collapsedGroups = [];
        RefreshView(resetPage: true);

        sortStatusMessage = groupBy is null
            ? localizer[TwStrings.DataTable_GroupingRemoved]
            : localizer[TwStrings.DataTable_GroupedBy, option.Title];

        await NotifyStateChangedAsync();
    }

    private async Task ToggleGroupAsync(TwDataTableGroup<TItem> group)
    {
        var expand = !group.Expanded;
        if (expand)
        {
            collapsedGroups.Remove(group.Label);
        }
        else
        {
            collapsedGroups.Add(group.Label);
        }

        RefreshView();
        sortStatusMessage = expand
            ? localizer[TwStrings.DataTable_GroupExpanded, group.Label]
            : localizer[TwStrings.DataTable_GroupCollapsed, group.Label];

        await NotifyStateChangedAsync();
    }

    private async Task SetAllGroupsExpandedAsync(bool expanded)
    {
        collapsedGroups = expanded ? [] : [.. groups.Select(g => g.Label)];
        RefreshView(resetPage: true);
        sortStatusMessage = expanded
            ? localizer[TwStrings.DataTable_AllGroupsExpanded]
            : localizer[TwStrings.DataTable_AllGroupsCollapsed];

        await NotifyStateChangedAsync();
    }

    /// <summary>
    /// A group header or a data row in the grouped layout, in the order they are shown.
    /// </summary>
    private readonly record struct DataTableLine(TwDataTableGroup<TItem> Group, TItem? Item, bool IsHeader);

    /// <summary>
    /// The rows of one group that are on the current page, rendered as one <c>&lt;tbody&gt;</c>.
    /// </summary>
    private sealed record DataTableGroupSegment(TwDataTableGroup<TItem> Group, List<TItem> Items);

    /// <summary>
    /// An entry in the Group by list: a groupable column, or no grouping when <paramref name="Name"/> is <c>null</c>.
    /// </summary>
    private sealed record GroupOption(string? Name, string Title);
}
