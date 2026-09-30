// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using Microsoft.AspNetCore.Components;
using System.Reflection;
using TwBlazor.Configuration.Components;
using TwBlazor.Enums;
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
    /// </summary>
    [Parameter]
    public string SearchLabel { get; set; } = "Search";

    /// <summary>
    /// Placeholder text for the search field.
    /// </summary>
    [Parameter]
    public string SearchPlaceholder { get; set; } = "Search…";

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
    /// Whether the table has pagination.
    /// </summary>
    [Parameter]
    public bool Pageable { get; set; } = true;

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
    /// compact) buttons. Defaults to <c>false</c>. Only applies when <see cref="Pageable"/> is <c>true</c>.
    /// </summary>
    [Parameter]
    public bool ComfortablePagination { get; set; } = false;

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

    private List<TItem> filteredItems = [];
    private List<TItem> displayedRows = [];
    private int previousRowsPerPage = 10;
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

    private string searchRowClasses => new ClassBuilder(options.Theme.Display.Flex)
        .AddClass(options.Theme.Flexbox.Justify.End)
        .AddClass(options.Theme.Spacing.MarginBottom.Xl).Build();

    private string searchColumnClasses => new ClassBuilder(options.Theme.Sizing.FullWidth)
        .AddClass(theme.SearchColumn).Build();

    private string headerCellRowClasses => new ClassBuilder(options.Theme.Display.Flex)
        .AddClass(options.Theme.Flexbox.Align.Center)
        .AddClass(options.Theme.Spacing.Gap.Sm).Build();

    private string sortIconClasses => new ClassBuilder(theme.SortIcon)
        .AddClass(options.Theme.Spacing.PushEnd).Build();

    private string paginationClasses => new ClassBuilder(options.Theme.Spacing.MarginTop.Xl)
        .AddClass(options.Theme.Sizing.FullWidth)
        .AddClass(options.Theme.Flexbox.Justify.Between).Build();

    protected override void OnInitialized()
    {
        base.OnInitialized();
        filteredItems = [.. Items];
    }

    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        // Handle RowsPerPage changes
        if (previousRowsPerPage != RowsPerPage)
        {
            previousRowsPerPage = RowsPerPage;
            currentPage = 1;
        }

        // If Items changed externally, update filtered items
        if (!filteredItems.SequenceEqual(Items))
        {
            filteredItems = [.. Items];

            // Reapply search if there was one
            if (!string.IsNullOrWhiteSpace(currentSearchQuery))
            {
                ApplySearch(currentSearchQuery);
            }
        }

        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        totalCount = filteredItems.Count;

        displayedRows = Pageable
            ? filteredItems
                .Skip((currentPage - 1) * RowsPerPage)
                .Take(RowsPerPage)
                .ToList()
            : [.. filteredItems];
    }

    private int TotalPages()
    {
        if (totalCount == 0 || RowsPerPage == 0)
            return 1;

        return (int)Math.Ceiling(totalCount / (decimal)RowsPerPage);
    }

    private void MoveFirst()
    {
        currentPage = 1;
        UpdateDisplay();
    }

    private void OnActivePageChanged(int page)
    {
        currentPage = page;
        UpdateDisplay();
    }

    private async Task OnSearchAsync(string? value)
    {
        currentSearchQuery = value;

        if (string.IsNullOrWhiteSpace(value))
        {
            ClearSearch();
            return;
        }

        ApplySearch(value);
        await Task.CompletedTask;
    }

    private void ApplySearch(string query)
    {
        filteredItems = Items.Where(item => !EqualityComparer<TItem>.Default.Equals(item, default) && ItemMatchesQuery(item, query)).ToList();

        MoveFirst();
        UpdateDisplay();
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

    private void ClearSearch()
    {
        filteredItems = [.. Items];
        currentPage = 1;
        UpdateDisplay();
    }

    private void ToggleSort(string columnName)
    {
        var column = Columns?.FirstOrDefault(c => c.Name == columnName);
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
            SortDirection.Ascending => $"Sorted by {column.Title}, ascending",
            SortDirection.Descending => $"Sorted by {column.Title}, descending",
            _ => $"Sort by {column.Title} removed"
        };

        ApplySort();
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
            return $"Sort by {column.Title}";

        return currentSortDirection switch
        {
            SortDirection.Ascending => $"Sort by {column.Title}, currently ascending",
            SortDirection.Descending => $"Sort by {column.Title}, currently descending",
            _ => $"Sort by {column.Title}"
        };
    }

    private void ApplySort()
    {
        if (string.IsNullOrEmpty(currentSortColumn) || currentSortDirection == SortDirection.None)
        {
            // Reset to original order (or maintain search results)
            if (!string.IsNullOrWhiteSpace(currentSearchQuery))
            {
                ApplySearch(currentSearchQuery);
            }
            else
            {
                filteredItems = [.. Items];
            }
            currentPage = 1;
            UpdateDisplay();
            return;
        }

        var column = Columns?.FirstOrDefault(c => c.Name == currentSortColumn);
        if (column?.PropertySelector == null)
            return;

        var propertySelector = column.PropertySelector;

        filteredItems = currentSortDirection == SortDirection.Ascending
            ? filteredItems.OrderBy(item => propertySelector(item)).ToList()
            : filteredItems.OrderByDescending(item => propertySelector(item)).ToList();

        currentPage = 1;
        UpdateDisplay();
    }

    private Icon GetSortIcon(string columnName)
    {
        var column = Columns?.FirstOrDefault(c => c.Name == columnName);
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
        if (RowsPerPage != newRowsPerPage)
        {
            RowsPerPage = newRowsPerPage;
            currentPage = 1;
            UpdateDisplay();
            await RowsPerPageChanged.InvokeAsync(newRowsPerPage);
        }
    }

}
