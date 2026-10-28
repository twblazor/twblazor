// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using Microsoft.AspNetCore.Components;
using System.Reflection;
using TwBlazor.Configuration.Components;
using TwBlazor.Enums;
using TwBlazor.Localization;
using TwBlazor.Utilities;

namespace TwBlazor.Components;


/// <summary>
/// Represents a generic data table component for displaying and managing a collection of items with features such as
/// search, pagination, and customizable rendering.
/// </summary>
/// <remarks>This component allows for flexible rendering of table headers and rows, supports pagination, and
/// provides options for styling and behavior customization. It is designed to work seamlessly with Blazor
/// components.</remarks>
/// <typeparam name="TItem">The type of items displayed in the table.</typeparam>
public partial class TwDataTable<TItem> : TwBlazorComponentBase
{
    private TwTableTheme theme => options.Theme.Components.Require<TwTableTheme>();

    /// <summary>
    /// Whether the table has search functionality.
    /// </summary>
    [Parameter]
    public bool Searchable { get; set; } = false;

    /// <summary>
    /// Label for the search field.
    /// Defaults to the localized <see cref="TwStrings.DataTable_SearchLabel"/> text ("Search").
    /// </summary>
    [Parameter]
    public string? SearchLabel { get; set; }

    private string effectiveSearchLabel => SearchLabel ?? localizer[TwStrings.DataTable_SearchLabel];

    /// <summary>
    /// Placeholder text for the search field.
    /// Defaults to the localized <see cref="TwStrings.DataTable_SearchPlaceholder"/> text ("Search…").
    /// </summary>
    [Parameter]
    public string? SearchPlaceholder { get; set; }

    private string effectiveSearchPlaceholder => SearchPlaceholder ?? localizer[TwStrings.DataTable_SearchPlaceholder];

    /// <summary>
    /// The data items to display in the table.
    /// </summary>
    [Parameter]
    public List<TItem> Items { get; set; } = [];

    /// <summary>
    /// Event callback when Items parameter changes.
    /// </summary>
    [Parameter]
    public EventCallback<List<TItem>> ItemsChanged { get; set; }

    /// <summary>
    /// RenderFragment for table header cells.
    /// </summary>
    [Parameter]
    public RenderFragment? TableHeader { get; set; }

    /// <summary>
    /// RenderFragment for table row cells, receives the row item.
    /// </summary>
    [Parameter]
    public RenderFragment<TItem>? TableRow { get; set; }

    /// <summary>
    /// Content to display when there are no items.
    /// </summary>
    [Parameter]
    public RenderFragment? EmptyContent { get; set; }

    /// <summary>
    /// Additional CSS classes for table rows.
    /// </summary>
    [Parameter]
    public string? RowClass { get; set; }

    /// <summary>
    /// If true, manually define <c>&lt;tr&gt;</c> elements in TableRow. Otherwise, they are auto-generated.
    /// </summary>
    [Parameter]
    public bool DefineTableRows { get; set; } = false;

    /// <summary>
    /// Whether the rows are split into pages, with a pagination below the table (<see cref="TwTable.EnablePagination"/>).
    /// Defaults to <c>true</c>. Ignored while <see cref="EnableVirtualization"/> is set.
    /// </summary>
    [Parameter]
    public bool EnablePagination { get; set; } = true;

    /// <summary>
    /// Number of rows per page.
    /// </summary>
    [Parameter]
    public int RowsPerPage { get; set; } = 10;

    /// <summary>
    /// Event callback when RowsPerPage parameter changes.
    /// </summary>
    [Parameter]
    public EventCallback<int> RowsPerPageChanged { get; set; }

    /// <summary>
    /// Options for rows per page dropdown.
    /// </summary>
    [Parameter]
    public int[] RowsPerPageOptions { get; set; } = [5, 10, 25, 50, 100];

    /// <summary>
    /// Whether to render the pagination controls with more comfortable, regular-size buttons instead of the default dense (smaller, more
    /// compact) buttons. Defaults to <c>false</c>. Only applies when <see cref="EnablePagination"/> is <c>true</c>.
    /// </summary>
    [Parameter]
    public bool ComfortablePagination { get; set; } = false;

    /// <summary>
    /// Optional column definitions for a manually defined table (<see cref="TableHeader"/> and <see cref="TableRow"/>),
    /// passed to <see cref="TwTable.ColumnGroup"/>. Use <see cref="TwTableColumn"/> to set column widths. When
    /// <see cref="Columns"/> is used, set <see cref="TwDataTableColumn{TItem}.Width"/> on each column instead.
    /// </summary>
    [Parameter]
    public RenderFragment? ColumnGroup { get; set; }

    /// <summary>
    /// Whether column widths come from the column definitions and header row instead of the cell content
    /// (<see cref="TwTable.FixedLayout"/>). Columns then keep the same width as the rows change from page to page,
    /// or as a search filters them. Defaults to <c>false</c>.
    /// </summary>
    [Parameter]
    public bool FixedLayout { get; set; }

    /// <summary>
    /// Whether to show striped rows.
    /// </summary>
    [Parameter]
    public bool Striped { get; set; } = true;

    /// <summary>
    /// Whether to show hover effect on rows.
    /// </summary>
    [Parameter]
    public bool Hoverable { get; set; } = true;

    /// <summary>
    /// Whether to show borders.
    /// </summary>
    [Parameter]
    public bool Bordered { get; set; } = false;

    /// <summary>
    /// Additional CSS classes for the table header.
    /// </summary>
    [Parameter]
    public string? HeaderClass { get; set; }

    /// <summary>
    /// Additional CSS classes for the header row (tr element) when using Columns.
    /// Applied on top of the shared <see cref="TwBlazor.Configuration.Components.TwTableTheme.Header"/> theme token, so
    /// customizing that theme property affects this header row too.
    /// </summary>
    [Parameter]
    public string? HeaderRowClass { get; set; }

    /// <summary>
    /// Additional CSS classes for the table body.
    /// </summary>
    [Parameter]
    public string? BodyClass { get; set; }

    /// <summary>
    /// Additional CSS classes for the table element itself.
    /// </summary>
    [Parameter]
    public string? TableClass { get; set; }

    /// <summary>
    /// Additional attributes for the table element.
    /// </summary>
    [Parameter]
    public Dictionary<string, object> TableAttributes { get; set; } = [];

    /// <summary>
    /// Defines the columns for the table. When provided, headers will be auto-generated with sorting support.
    /// Leave null to use manual TableHeader and TableRow fragments.
    /// </summary>
    [Parameter]
    public List<TwDataTableColumn<TItem>>? Columns { get; set; }

    /// <summary>
    /// Optional caption content for the table, forwarded to the underlying <see cref="TwTable"/>. Provides an
    /// accessible name/description for the table that is programmatically associated with it via
    /// <c>&lt;caption&gt;</c>. Visually hidden by default unless <see cref="CaptionVisible"/> is <c>true</c>.
    /// </summary>
    [Parameter]
    public RenderFragment? Caption { get; set; }

    /// <summary>
    /// Whether the <see cref="Caption"/> should be visible. Defaults to <c>false</c>, rendering the caption
    /// visually hidden (but still available to assistive technology).
    /// </summary>
    [Parameter]
    public bool CaptionVisible { get; set; } = false;

    private List<TItem> viewItems = [];
    private List<TItem> displayedRows = [];
    private List<TItem> lastItems = [];
    private int rowsPerPage = 10;
    private int? lastRowsPerPageParameter;
    private int currentPage = 1;
    private int totalCount;
    private string? currentSearchQuery;
    private string? currentSortColumn;
    private SortDirection currentSortDirection = SortDirection.None;
    private string? sortStatusMessage;

    private string rootClasses => new ClassBuilder(Class).Build();

    private string headerRowClasses => new ClassBuilder()
        .AddClass(theme.Header)
        .AddClass(HeaderRowClass ?? string.Empty)
        .Build();

    private string toolbarClasses => new ClassBuilder(options.Theme.Display.Flex)
        .AddClass(options.Theme.Flexbox.Wrap)
        .AddClass(options.Theme.Flexbox.Align.End)
        .AddClass(options.Theme.Spacing.Gap.Get(2))
        .AddClass(options.Theme.Spacing.MarginBottom.Get(4)).Build();

    // The search always comes last in the toolbar, pushed to the end of the row.
    private string searchColumnClasses => new ClassBuilder(options.Theme.Sizing.FullWidth)
        .AddClass(theme.SearchColumn)
        .AddClass(options.Theme.Spacing.PushEnd).Build();

    private string toolbarActionsClasses => new ClassBuilder(options.Theme.Display.Flex)
        .AddClass(options.Theme.Flexbox.Wrap)
        .AddClass(options.Theme.Spacing.Gap.Get(1)).Build();

    private string sortIconClasses => theme.SortIcon;

    private bool hasColumns => Columns is { Count: > 0 };

    private bool showToolbar => Searchable || showExportButtons || showGroupBySelect;

    // The empty row has to cross every column. Without column definitions the count is unknown, and a colspan
    // larger than the table is harmless.
    private int emptyStateColumnSpan => hasColumns ? Math.Max(renderedColumnCount, 1) : 100;

    private string effectiveEmptyMessage => string.IsNullOrWhiteSpace(currentSearchQuery) && ActiveFilters().Count == 0
        ? localizer[TwStrings.DataTable_NoData]
        : localizer[TwStrings.DataTable_NoSearchResults];

    /// <inheritdoc />
    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        if (lastRowsPerPageParameter != RowsPerPage)
        {
            lastRowsPerPageParameter = RowsPerPage;
            rowsPerPage = RowsPerPage;
            currentPage = 1;
        }

        SyncColumns();

        var groupingChanged = GroupBy != lastGroupByParameter;
        SyncGroupByParameter();
        SyncItemsProvider();

        var stateApplied = ApplyIncomingState();

        if (stateApplied || groupingChanged || !lastItems.SequenceEqual(Items))
        {
            lastItems = [.. Items];
            RefreshView();
        }
        else
        {
            UpdateDisplay();
        }
    }

    /// <summary>
    /// Runs the rows through the column filters, the search and the sort, then takes the current page. Each step
    /// works on the result of the one before, so a search keeps the sort and a sort keeps the search and filters.
    /// </summary>
    /// <param name="resetPage">Whether to go back to the first page, as a new search, filter or sort does.</param>
    private void RefreshView(bool resetPage = false)
    {
        if (usesProvider)
        {
            // The provider filters, searches and sorts; the table only asks it for the rows again. Moving back to the
            // first page asks for them already, so the rows are only refreshed when the page stays the same.
            var pageChanges = resetPage && currentPage != 1;
            if (resetPage)
            {
                currentPage = 1;
            }

            refreshProviderRows |= !pageChanges;
            UpdateDisplay();
            return;
        }

        var rows = Items.Where(item => !EqualityComparer<TItem>.Default.Equals(item, default));

        var filters = AllColumns()
            .Where(c => c.PropertySelector is not null && FilterOf(c.Name)?.IsActive == true)
            .Select(c => (Column: c, Filter: FilterOf(c.Name)!, Type: ResolveFilterType(c)))
            .ToList();
        if (filters.Count > 0)
        {
            rows = rows.Where(item => PassesFilters(item, filters));
        }

        if (!string.IsNullOrWhiteSpace(currentSearchQuery))
        {
            var query = currentSearchQuery;
            rows = rows.Where(item => ItemMatchesQuery(item, query));
        }

        var sortColumn = currentSortDirection == SortDirection.None ? null : FindColumn(currentSortColumn);
        if (sortColumn is { IsSortable: true, PropertySelector: { } selector })
        {
            rows = currentSortDirection == SortDirection.Ascending
                ? rows.OrderBy(selector)
                : rows.OrderByDescending(selector);
        }

        viewItems = rows.ToList();
        BuildGroups();

        if (resetPage)
        {
            currentPage = 1;
        }

        UpdateDisplay();
    }

    /// <summary>
    /// Gets every column definition, shown or hidden, so a hidden column's filter still applies.
    /// </summary>
    private Dictionary<string, TwDataTableColumn<TItem>>.ValueCollection AllColumns() => columnsByName.Values;

    private void UpdateDisplay()
    {
        if (usesProvider)
        {
            totalCount = providerTotalCount;
            displayedRows = loadedItems;

            // Until the first rows arrive the total is unknown, so a restored page is kept rather than clamped.
            if (providerHasLoaded)
            {
                currentPage = Math.Clamp(currentPage, 1, TotalPages());
            }
            return;
        }

        if (isGrouped)
        {
            totalCount = lines.Count;
            currentPage = Math.Clamp(currentPage, 1, TotalPages());

            var pageLines = isPaged ? lines.Skip((currentPage - 1) * rowsPerPage).Take(rowsPerPage).ToList() : lines;
            displayedRows = [.. pageLines.Where(l => !l.IsHeader).Select(l => l.Item!)];
            return;
        }

        totalCount = viewItems.Count;
        currentPage = Math.Clamp(currentPage, 1, TotalPages());

        displayedRows = isPaged
            ? viewItems
                .Skip((currentPage - 1) * rowsPerPage)
                .Take(rowsPerPage)
                .ToList()
            : [.. viewItems];
    }

    private int TotalPages()
    {
        if (totalCount == 0 || rowsPerPage <= 0)
            return 1;

        return (int)Math.Ceiling(totalCount / (decimal)rowsPerPage);
    }

    private TwDataTableColumn<TItem>? FindColumn(string? name) =>
        name is null ? null : Columns?.FirstOrDefault(c => c.Name == name);

    private async Task OnActivePageChanged(int page)
    {
        currentPage = page;
        UpdateDisplay();
        Announce(() => localizer.Plural(displayedRows.Count, TwStrings.DataTable_PageStatusOne, TwStrings.DataTable_PageStatusOther, currentPage, TotalPages(), displayedRows.Count));
        await NotifyStateChangedAsync();
    }

    private async Task OnSearchAsync(string? value)
    {
        currentSearchQuery = string.IsNullOrWhiteSpace(value) ? null : value;
        RefreshView(resetPage: true);

        // Worked out when announced, so a provider's total is the one its new rows arrived with.
        Announce(() => SearchStatusMessage(usesProvider ? providerTotalCount : viewItems.Count));

        await NotifyStateChangedAsync();
    }

    /// <summary>
    /// Describes the result of a search for assistive technology.
    /// </summary>
    /// <param name="matchingCount">How many rows match the search.</param>
    /// <returns>The message to announce.</returns>
    private string SearchStatusMessage(int matchingCount)
    {
        if (currentSearchQuery is null)
        {
            return localizer[TwStrings.DataTable_SearchCleared];
        }

        if (matchingCount == 0)
        {
            return localizer[TwStrings.DataTable_NoSearchResults];
        }

        return localizer.Plural(matchingCount, TwStrings.DataTable_MatchingOne, TwStrings.DataTable_MatchingOther, matchingCount);
    }

    private static bool ItemMatchesQuery(TItem item, string query)
    {
        if (EqualityComparer<TItem>.Default.Equals(item, default)) return false;

        var itemType = item!.GetType();
        var properties = itemType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

        List<string> searchableValues = [];

        foreach (var property in properties)
        {
            try
            {
                var value = property.GetValue(item);
                if (value != null)
                {
                    searchableValues.Add(value.ToString() ?? string.Empty);
                }
            }
            catch (TargetException)
            {
                // Skip properties that cannot be read
                continue;
            }
            catch (TargetParameterCountException)
            {
                // Skip indexed properties
                continue;
            }
            catch (MethodAccessException)
            {
                // Skip inaccessible properties
                continue;
            }
            catch (TargetInvocationException)
            {
                // Skip properties that throw during access
                continue;
            }
        }

        var queryParts = query.Split([' ', ',', '.', ';'], StringSplitOptions.RemoveEmptyEntries);

        // Check if any query part matches any property value (case-insensitive contains)
        return queryParts.Any(queryPart =>
            searchableValues.Any(value =>
                value.Contains(queryPart, StringComparison.OrdinalIgnoreCase)));
    }

    private async Task OnSortClickedAsync(string columnName)
    {
        ToggleSort(columnName);
        await NotifyStateChangedAsync();
    }

    private void ToggleSort(string columnName)
    {
        var column = FindColumn(columnName);
        if (column == null || !column.IsSortable || column.PropertySelector == null)
            return;

        // Toggle sort direction: None -> Ascending -> Descending -> None
        if (currentSortColumn == columnName)
        {
            currentSortDirection = currentSortDirection switch
            {
                SortDirection.None => SortDirection.Ascending,
                SortDirection.Ascending => SortDirection.Descending,
                SortDirection.Descending => SortDirection.None,
                _ => SortDirection.None
            };
        }
        else
        {
            currentSortColumn = columnName;
            currentSortDirection = SortDirection.Ascending;
        }

        sortStatusMessage = currentSortDirection switch
        {
            SortDirection.Ascending => localizer[TwStrings.DataTable_SortedAscending, column.Title],
            SortDirection.Descending => localizer[TwStrings.DataTable_SortedDescending, column.Title],
            _ => localizer[TwStrings.DataTable_SortRemoved, column.Title]
        };

        RefreshView(resetPage: true);
    }

    /// <summary>
    /// Returns the value for the <c>aria-sort</c> attribute of a sortable column's <c>&lt;th&gt;</c>.
    /// </summary>
    private string GetAriaSort(string columnName)
    {
        if (currentSortColumn != columnName)
            return "none";

        return currentSortDirection switch
        {
            SortDirection.Ascending => "ascending",
            SortDirection.Descending => "descending",
            _ => "none"
        };
    }

    /// <summary>
    /// Returns an accessible label for a column's sort toggle button that reflects the current sort state and
    /// what activating it will do next (Ascending -> Descending -> unsorted).
    /// </summary>
    private string GetSortAriaLabel(TwDataTableColumn<TItem> column)
    {
        if (currentSortColumn != column.Name || currentSortDirection == SortDirection.None)
            return localizer[TwStrings.DataTable_SortBy, column.Title];

        return currentSortDirection switch
        {
            SortDirection.Ascending => localizer[TwStrings.DataTable_SortByAscending, column.Title],
            SortDirection.Descending => localizer[TwStrings.DataTable_SortByDescending, column.Title],
            _ => localizer[TwStrings.DataTable_SortBy, column.Title]
        };
    }

    private Icon GetSortIcon(string columnName)
    {
        var column = FindColumn(columnName);
        if (column == null || !column.IsSortable)
            return Icon.Chevron_Expand;

        if (currentSortColumn != columnName || currentSortDirection == SortDirection.None)
            return Icon.Chevron_Expand;

        var isNumeric = IsNumericColumn(column);

        if (isNumeric)
        {
            return currentSortDirection == SortDirection.Ascending ? Icon.Sort_Numeric_Up : Icon.Sort_Numeric_Down;
        }

        return currentSortDirection == SortDirection.Ascending ? Icon.Sort_Alpha_Up : Icon.Sort_Alpha_Down;
    }

    private bool IsNumericColumn(TwDataTableColumn<TItem> column)
    {
        if (column.PropertySelector == null)
            return false;

        // Try to get the return type from the property selector
        try
        {
            var sampleItem = Items.FirstOrDefault();
            if (!EqualityComparer<TItem>.Default.Equals(sampleItem, default))
            {
                var value = column.PropertySelector(sampleItem!);
                if (value == null)
                    return false;

                var type = value.GetType();

                // Check if the type is a numeric type
                return type == typeof(int) ||
                       type == typeof(long) ||
                       type == typeof(short) ||
                       type == typeof(byte) ||
                       type == typeof(decimal) ||
                       type == typeof(double) ||
                       type == typeof(float) ||
                       type == typeof(uint) ||
                       type == typeof(ulong) ||
                       type == typeof(ushort) ||
                       type == typeof(sbyte);
            }
        }
        catch (TargetException)
        {
            // If we can't determine the type, default to non-numeric
        }
        catch (TargetParameterCountException)
        {
            // If property requires parameters, default to non-numeric
        }
        catch (MethodAccessException)
        {
            // If property is inaccessible, default to non-numeric
        }
        catch (TargetInvocationException)
        {
            // If property throws during access, default to non-numeric
        }

        return false;
    }

    private async Task OnRowsPerPageChangedAsync(int newRowsPerPage)
    {
        if (rowsPerPage != newRowsPerPage)
        {
            rowsPerPage = newRowsPerPage;
            currentPage = 1;
            UpdateDisplay();
            await RowsPerPageChanged.InvokeAsync(newRowsPerPage);
            await NotifyStateChangedAsync();
        }
    }
}
