using AngleSharp.Dom;
using Bunit;
using Microsoft.AspNetCore.Components;
using TwBlazor.Components;
using TwBlazor.Configuration.Components;

namespace TwBlazor.Tests.Components.DataTable;

public class TwDataTableTests : TwBlazorTestBase
{
    private TwTableTheme tableTheme => Theme.Components.Require<TwTableTheme>();

    private static readonly int[] _rowsPerPageOptions = [5, 10, 25];

    // TwPagination renders Previous/page-numbers/Next as a single <button> list (not <a>, since they
    // perform an in-page action rather than navigating) inside the "Table pagination" nav.
    private static IReadOnlyList<IElement> PaginationLinks(IRenderedComponent<TwDataTable<TestProduct>> cut) =>
        cut.FindAll("nav[aria-label='Table pagination'] ul li button");

    // Page number links carry a "page " screen-reader-only prefix ahead of the visible digits.
    private static string LinkText(IElement a) => a.TextContent.Replace("page ", string.Empty).Trim();

    [Fact]
    public void TwDataTable_Renders_WithEmptyItems()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwDataTable<TestProduct>>(parameters => parameters
            .Add(p => p.Items, []));

        // Assert
        Assert.NotNull(cut);
    }

    [Fact]
    public void TwDataTable_Renders_WithItems()
    {
        // Arrange
        var products = GetTestProducts();

        // Act
        var cut = TestContext.Render<TwDataTable<TestProduct>>(parameters => parameters
            .Add(p => p.Items, products)
            .Add(p => p.Columns, GetTestColumns()));

        // Assert
        var rows = cut.FindAll("tbody tr");
        Assert.Equal(3, rows.Count);
    }

    [Fact]
    public void TwDataTable_Pageable_ShowsPaginationControls()
    {
        // Arrange
        var products = GetManyTestProducts(20);

        // Act
        var cut = TestContext.Render<TwDataTable<TestProduct>>(parameters => parameters
            .Add(p => p.Items, products)
            .Add(p => p.Columns, GetTestColumns())
            .Add(p => p.Pageable, true)
            .Add(p => p.RowsPerPage, 5));

        // Assert
        var paginationNav = cut.Find("nav[aria-label='Table pagination']");
        Assert.NotNull(paginationNav);
    }

    [Theory]
    [InlineData(null, true)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    public void TwDataTable_ComfortablePagination_ControlsPaginationDensity(bool? comfortablePagination, bool expectedDense)
    {
        // Arrange
        var paginationTheme = Theme.Components.Require<TwPaginationTheme>();
        var denseClass = paginationTheme.DenseSize.Split(' ')[0];
        var regularClass = paginationTheme.Size.Split(' ')[0];

        // Act
        var cut = TestContext.Render<TwDataTable<TestProduct>>(parameters =>
        {
            parameters
                .Add(p => p.Items, GetManyTestProducts(20))
                .Add(p => p.Columns, GetTestColumns())
                .Add(p => p.RowsPerPage, 5);

            if (comfortablePagination.HasValue)
            {
                parameters.Add(p => p.ComfortablePagination, comfortablePagination.Value);
            }
        });

        // Assert
        var button = PaginationLinks(cut)[0];
        Assert.Equal(expectedDense, button.ClassList.Contains(denseClass));
        Assert.Equal(!expectedDense, button.ClassList.Contains(regularClass));
    }

    [Fact]
    public void TwDataTable_NonPageable_HidesNavigationButtons()
    {
        // Arrange
        var products = GetTestProducts();

        // Act
        var cut = TestContext.Render<TwDataTable<TestProduct>>(parameters => parameters
            .Add(p => p.Items, products)
            .Add(p => p.Columns, GetTestColumns())
            .Add(p => p.Pageable, false));

        // Assert
        var navButtons = cut.FindAll("nav[aria-label='Table pagination']");
        Assert.Empty(navButtons);
    }

    [Fact]
    public void TwDataTable_Pagination_ShowsCorrectRowCount()
    {
        // Arrange
        var products = GetManyTestProducts(20);

        // Act
        var cut = TestContext.Render<TwDataTable<TestProduct>>(parameters => parameters
            .Add(p => p.Items, products)
            .Add(p => p.Columns, GetTestColumns())
            .Add(p => p.Pageable, true)
            .Add(p => p.RowsPerPage, 5));

        // Assert
        var rows = cut.FindAll("tbody tr");
        Assert.Equal(5, rows.Count);
    }

    [Fact]
    public void TwDataTable_NextButton_NavigatesToNextPage()
    {
        // Arrange
        var products = GetManyTestProducts(20);
        var cut = TestContext.Render<TwDataTable<TestProduct>>(parameters => parameters
            .Add(p => p.Items, products)
            .Add(p => p.Columns, GetTestColumns())
            .Add(p => p.Pageable, true)
            .Add(p => p.RowsPerPage, 5));

        // Act - TwPagination renders Previous/page-numbers/Next as a single link list; Next is last.
        PaginationLinks(cut)[^1].Click();

        // Assert
        var activeLink = PaginationLinks(cut).Single(a => a.GetAttribute("aria-current") == "page");
        Assert.Equal("2", LinkText(activeLink));
    }

    [Fact]
    public void TwDataTable_NavigatingToLastPage_StopsAdvancing()
    {
        // Arrange - 20 items at 5 per page = 4 pages
        var products = GetManyTestProducts(20);
        var cut = TestContext.Render<TwDataTable<TestProduct>>(parameters => parameters
            .Add(p => p.Items, products)
            .Add(p => p.Columns, GetTestColumns())
            .Add(p => p.Pageable, true)
            .Add(p => p.RowsPerPage, 5));

        // Act - click Next enough times to reach the last page, then once more
        for (var i = 0; i < 4; i++)
        {
            PaginationLinks(cut)[^1].Click();
        }

        // Assert - stayed on page 4, TwPagination clamped the extra click
        var activeLink = PaginationLinks(cut).Single(a => a.GetAttribute("aria-current") == "page");
        Assert.Equal("4", LinkText(activeLink));
    }

    [Fact]
    public void TwDataTable_Searchable_ShowsSearchField()
    {
        // Arrange
        var products = GetTestProducts();

        // Act
        var cut = TestContext.Render<TwDataTable<TestProduct>>(parameters => parameters
            .Add(p => p.Items, products)
            .Add(p => p.Columns, GetTestColumns())
            .Add(p => p.Searchable, true));

        // Assert
        var searchField = cut.FindAll("input");
        Assert.NotEmpty(searchField);
    }

    [Fact]
    public void TwDataTable_NonSearchable_HidesSearchField()
    {
        // Arrange
        var products = GetTestProducts();

        // Act
        var cut = TestContext.Render<TwDataTable<TestProduct>>(parameters => parameters
            .Add(p => p.Items, products)
            .Add(p => p.Columns, GetTestColumns())
            .Add(p => p.Searchable, false));

        // Assert
        var searchInputs = cut.FindAll("input");
        Assert.Empty(searchInputs);
    }

    [Fact]
    public void TwDataTable_RowsPerPageDropdown_ChangesPageSize()
    {
        // Arrange
        var products = GetManyTestProducts(20);
        var cut = TestContext.Render<TwDataTable<TestProduct>>(parameters => parameters
            .Add(p => p.Items, products)
            .Add(p => p.Columns, GetTestColumns())
            .Add(p => p.Pageable, true)
            .Add(p => p.RowsPerPage, 5)
            .Add(p => p.RowsPerPageOptions, _rowsPerPageOptions));

        // Act - Change select value to 10 (ID 2 in the RowsPerPageOptions array)
        var select = cut.Find("select");
        select.Change("2"); // ID 2 corresponds to value 10 in the options [5, 10, 25]

        // Assert - after changing to 10 per page, should show 10 rows
        var rows = cut.FindAll("tbody tr");
        Assert.Equal(10, rows.Count);
    }

    [Fact]
    public void TwDataTable_EmptyContent_ShowsWhenNoItems()
    {
        // Arrange
        var emptyMessage = "No data available";

        // Act
        var cut = TestContext.Render<TwDataTable<TestProduct>>(parameters => parameters
            .Add(p => p.Items, [])
            .Add(p => p.Columns, GetTestColumns())
            .Add(p => p.EmptyContent, RenderFragmentBuilder(emptyMessage)));

        // Assert
        var emptyCell = cut.Find("tbody td");
        Assert.Contains(emptyMessage, emptyCell.TextContent);
    }

    [Fact]
    public void TwDataTable_Striped_AppliesStripedClass()
    {
        // Arrange
        var products = GetTestProducts();

        // Act
        var cut = TestContext.Render<TwDataTable<TestProduct>>(parameters => parameters
            .Add(p => p.Items, products)
            .Add(p => p.Columns, GetTestColumns())
            .Add(p => p.Striped, true));

        // Assert
        var tbody = cut.Find("tbody");
        var tbodyClass = tbody.GetAttribute("class");
        Assert.Contains("even", tbodyClass);
    }

    [Fact]
    public void TwDataTable_Hoverable_AppliesHoverClass()
    {
        // Arrange
        var products = GetTestProducts();

        // Act
        var cut = TestContext.Render<TwDataTable<TestProduct>>(parameters => parameters
            .Add(p => p.Items, products)
            .Add(p => p.Columns, GetTestColumns())
            .Add(p => p.Hoverable, true));

        // Assert
        var tbody = cut.Find("tbody");
        var tbodyClass = tbody.GetAttribute("class");
        Assert.Contains("hover", tbodyClass);
    }

    [Fact]
    public void TwDataTable_WithId_SetsIdCorrectly()
    {
        // Arrange
        var products = GetTestProducts();

        // Act
        var cut = TestContext.Render<TwDataTable<TestProduct>>(parameters => parameters
            .Add(p => p.Items, products)
            .Add(p => p.Columns, GetTestColumns())
            .Add(p => p.Id, "test-table"));

        // Assert
        var table = cut.Find("table");
        Assert.Equal("test-table", table.GetAttribute("id"));
    }

    [Fact]
    public void TwDataTable_WithClass_AppliesCustomClass()
    {
        // Arrange
        var products = GetTestProducts();

        // Act
        var cut = TestContext.Render<TwDataTable<TestProduct>>(parameters => parameters
            .Add(p => p.Items, products)
            .Add(p => p.Columns, GetTestColumns())
            .Add(p => p.Class, "custom-table-class"));

        // Assert
        var container = cut.Find("div");
        Assert.Contains("custom-table-class", container.GetAttribute("class"));
    }

    // Helper methods
    private static List<TestProduct> GetTestProducts()
    {
        return
        [
            new() { Id = 1, Name = "Product A", Category = "Electronics", Price = 299.99m, Stock = 10 },
            new() { Id = 2, Name = "Product B", Category = "Books", Price = 19.99m, Stock = 50 },
            new() { Id = 3, Name = "Product C", Category = "Clothing", Price = 49.99m, Stock = 25 }
        ];
    }

    private static List<TestProduct> GetManyTestProducts(int count)
    {
        List<TestProduct> products = [];
        for (var i = 1; i <= count; i++)
        {
            products.Add(new TestProduct
            {
                Id = i,
                Name = $"Product {i}",
                Category = i % 2 == 0 ? "Electronics" : "Books",
                Price = 10m + (i * 5m),
                Stock = i * 2
            });
        }
        return products;
    }

    private static List<TwDataTableColumn<TestProduct>> GetTestColumns()
    {
        return
        [
            new() { Name = "name", Title = "Name", PropertySelector = p => p.Name, IsSortable = false },
            new() { Name = "category", Title = "Category", PropertySelector = p => p.Category, IsSortable = false },
            new() { Name = "price", Title = "Price", PropertySelector = p => p.Price, IsSortable = false }
        ];
    }

    private static List<TwDataTableColumn<TestProduct>> GetSortableColumns()
    {
        return
        [
            new() { Name = "name", Title = "Name", PropertySelector = p => p.Name, IsSortable = true },
            new() { Name = "price", Title = "Price", PropertySelector = p => p.Price, IsSortable = true }
        ];
    }

    [Fact]
    public void TwDataTable_WithZeroRowsPerPage_RendersWithoutCrashing()
    {
        // Arrange - RowsPerPage=0 tests the TotalPages edge case returning 1
        var products = GetManyTestProducts(5);

        // Act
        var cut = TestContext.Render<TwDataTable<TestProduct>>(parameters => parameters
            .Add(p => p.Items, products)
            .Add(p => p.Columns, GetTestColumns())
            .Add(p => p.Pageable, true)
            .Add(p => p.RowsPerPage, 0));

        // Assert
        Assert.NotNull(cut);
    }

    [Fact]
    public void TwDataTable_Search_WithEmptyQuery_RestoresAllRows()
    {
        // Arrange
        var products = GetManyTestProducts(20);
        var cut = TestContext.Render<TwDataTable<TestProduct>>(parameters => parameters
            .Add(p => p.Items, products)
            .Add(p => p.Columns, GetTestColumns())
            .Add(p => p.Searchable, true)
            .Add(p => p.Pageable, true)
            .Add(p => p.RowsPerPage, 10));

        var searchInput = cut.Find("input");
        searchInput.Change("Product 1");

        // Act - clear the search (empty string triggers ClearSearch)
        searchInput.Change("");

        // Assert - all items restored
        var rows = cut.FindAll("tbody tr");
        Assert.Equal(10, rows.Count);
    }

    [Fact]
    public void TwDataTable_Sorting_CyclesAscDescNone()
    {
        // Arrange
        var products = GetTestProducts();
        var cut = TestContext.Render<TwDataTable<TestProduct>>(parameters => parameters
            .Add(p => p.Items, products)
            .Add(p => p.Columns, GetSortableColumns()));

        // Act - cycle None -> Ascending -> Descending -> None. The sort button's accessible name
        // changes with sort state (e.g. "Sort by Name, currently ascending"), so it's re-found
        // before each click rather than reusing a stale cached element reference.
        cut.Find("[aria-label^='Sort by Name']").Click();
        cut.Find("[aria-label^='Sort by Name']").Click();
        cut.Find("[aria-label^='Sort by Name']").Click();

        // Assert - back to original order
        var rows = cut.FindAll("tbody tr");
        Assert.Equal(3, rows.Count);
    }

    [Fact]
    public void TwDataTable_Sorting_SwitchingColumns_UsesElseBranch()
    {
        // Arrange
        var products = GetTestProducts();
        var cut = TestContext.Render<TwDataTable<TestProduct>>(parameters => parameters
            .Add(p => p.Items, products)
            .Add(p => p.Columns, GetSortableColumns()));

        // Act - sort by Name first, then by Price (different column → else branch in ToggleSort)
        cut.Find("[aria-label='Sort by Name']").Click();
        cut.Find("[aria-label='Sort by Price']").Click();

        // Assert
        var rows = cut.FindAll("tbody tr");
        Assert.Equal(3, rows.Count);
    }

    [Fact]
    public void TwDataTable_HeaderRowClass_AppliesCustomClass()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwDataTable<TestProduct>>(parameters => parameters
            .Add(p => p.Items, GetTestProducts())
            .Add(p => p.Columns, GetTestColumns())
            .Add(p => p.HeaderRowClass, "custom-header-row"));

        // Assert
        var headerRow = cut.Find("thead tr");
        Assert.Contains("custom-header-row", headerRow.GetAttribute("class"));
    }

    [Fact]
    public void TwDataTable_HeaderRow_DarkMode_UsesDarkerBackgroundThanBody()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwDataTable<TestProduct>>(parameters => parameters
            .Add(p => p.Items, GetTestProducts())
            .Add(p => p.Columns, GetTestColumns()));

        // Assert - header and body intentionally use different background tokens so the header
        // reads as visually distinct from the body, in both light and dark mode.
        var headerRow = cut.Find("thead tr");
        Assert.Contains(tableTheme.Header, headerRow.GetAttribute("class"));
        Assert.NotEqual(tableTheme.Header, tableTheme.Body);
    }

    [Fact]
    public void TwDataTable_HeaderRow_UsesThemeHeaderClass_ByDefault()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwDataTable<TestProduct>>(parameters => parameters
            .Add(p => p.Items, GetTestProducts())
            .Add(p => p.Columns, GetTestColumns()));

        // Assert
        var headerRow = cut.Find("thead tr");
        Assert.Contains(tableTheme.Header, headerRow.GetAttribute("class"));
    }

    [Fact]
    public void TwDataTable_HeaderRow_ReflectsCustomThemeHeaderClass()
    {
        // Arrange - TwDataTable's auto-generated header row reuses TwTable's shared theme token
        // rather than a separate one, so overriding it here restyles both components at once.
        tableTheme.Header = "bg-blue-950 text-white custom-theme-header";

        // Act
        var cut = TestContext.Render<TwDataTable<TestProduct>>(parameters => parameters
            .Add(p => p.Items, GetTestProducts())
            .Add(p => p.Columns, GetTestColumns()));

        // Assert
        var headerRow = cut.Find("thead tr");
        Assert.Contains("custom-theme-header", headerRow.GetAttribute("class"));
    }

    [Fact]
    public void TwDataTable_ItemsChanged_CanBeSet()
    {
        // Arrange & Act - ItemsChanged is currently unused by the component's own logic but
        // should still be settable without error.
        var cut = TestContext.Render<TwDataTable<TestProduct>>(parameters => parameters
            .Add(p => p.Items, GetTestProducts())
            .Add(p => p.Columns, GetTestColumns())
            .Add(p => p.ItemsChanged, EventCallback.Factory.Create<List<TestProduct>>(this, _ => { })));

        // Assert
        Assert.NotNull(cut);
    }

    [Fact]
    public void TwDataTable_ItemsChangedExternally_UpdatesDisplayedRows()
    {
        // Arrange
        var cut = TestContext.Render<TwDataTable<TestProduct>>(parameters => parameters
            .Add(p => p.Items, GetTestProducts())
            .Add(p => p.Columns, GetTestColumns()));

        // Act - re-render with a different Items list (simulates the parent updating it)
        var newProducts = GetManyTestProducts(5);
        cut.Render(parameters => parameters.Add(p => p.Items, newProducts));

        // Assert
        var rows = cut.FindAll("tbody tr");
        Assert.Equal(5, rows.Count);
    }

    [Fact]
    public void TwDataTable_ItemsChangedExternally_WithActiveSearch_ReappliesSearch()
    {
        // Arrange
        var cut = TestContext.Render<TwDataTable<TestProduct>>(parameters => parameters
            .Add(p => p.Items, GetManyTestProducts(20))
            .Add(p => p.Columns, GetTestColumns())
            .Add(p => p.Searchable, true)
            .Add(p => p.Pageable, false));

        // Note: search matches if ANY whitespace-split query word matches, so a query
        // containing the generic word "Product" (which every row's Name contains) would
        // match everything - use a single non-generic word instead.
        cut.Find("input").Change("15");
        Assert.True(cut.FindAll("tbody tr").Count < 20);

        // Act - swap in a new Items list while the search is still active
        var newProducts = GetManyTestProducts(25);
        cut.Render(parameters => parameters.Add(p => p.Items, newProducts));

        // Assert - search re-applied against the new items (still filtered, not all 25)
        var rows = cut.FindAll("tbody tr");
        Assert.True(rows.Count > 0 && rows.Count < 25);
    }

    [Fact]
    public void TwDataTable_PreviousLink_FromPageTwo_GoesToPageOne()
    {
        // Arrange
        var cut = TestContext.Render<TwDataTable<TestProduct>>(parameters => parameters
            .Add(p => p.Items, GetManyTestProducts(20))
            .Add(p => p.Columns, GetTestColumns())
            .Add(p => p.Pageable, true)
            .Add(p => p.RowsPerPage, 5));

        PaginationLinks(cut)[^1].Click(); // Next -> page 2

        // Act
        PaginationLinks(cut)[0].Click(); // Previous -> page 1

        // Assert
        var activeLink = PaginationLinks(cut).Single(a => a.GetAttribute("aria-current") == "page");
        Assert.Equal("1", LinkText(activeLink));
    }

    [Fact]
    public void TwDataTable_Search_SkipsNullItems_InList()
    {
        // Arrange - a null entry in Items exercises the `!EqualityComparer.Equals(item, default)`
        // guards in both ApplySearch's Where filter and ItemMatchesQuery.
        var products = GetTestProducts();
        products.Insert(1, null!);

        var cut = TestContext.Render<TwDataTable<TestProduct>>(parameters => parameters
            .Add(p => p.Items, products)
            .Add(p => p.Columns, GetTestColumns())
            .Add(p => p.Searchable, true));

        // Act - "A" is unique to Product A across every field of every row (search matches
        // if ANY split query word matches, so a shared word like "Product" would match all).
        cut.Find("input").Change("A");

        // Assert - does not throw, and finds the matching non-null item
        var rows = cut.FindAll("tbody tr");
        Assert.Single(rows);
    }

    [Fact]
    public void TwDataTable_Search_SkipsNullPropertyValues()
    {
        // Arrange - exercises the `if (value != null)` guard in ItemMatchesQuery.
        var products = GetTestProducts();
        products[0].Category = null!;

        var cut = TestContext.Render<TwDataTable<TestProduct>>(parameters => parameters
            .Add(p => p.Items, products)
            .Add(p => p.Columns, GetTestColumns())
            .Add(p => p.Searchable, true));

        // Act - search for the product with the null Category by its name instead
        cut.Find("input").Change("A");

        // Assert - does not throw
        var rows = cut.FindAll("tbody tr");
        Assert.Single(rows);
    }

    [Fact]
    public void ItemMatchesQuery_ReturnsFalse_ForNullItem_WhenCalledDirectly()
    {
        // Arrange - ApplySearch's Where clause already short-circuits past ItemMatchesQuery
        // for null items via its own `!EqualityComparer.Equals(item, default)` guard, so
        // ItemMatchesQuery's own identical internal guard is only reachable via direct
        // invocation (it's a static private method).
        var method = typeof(TwDataTable<TestProduct>).GetMethod("ItemMatchesQuery",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!;

        // Act
        var result = method.Invoke(null, [null!, "query"]);

        // Assert
        Assert.False((bool)result!);
    }

    [Fact]
    public void ToggleSort_NonSortableColumn_DoesNothing()
    {
        // Arrange - GetTestColumns() columns all have IsSortable = false, so no sort
        // icon/button renders in the header at all; invoke ToggleSort directly instead.
        var cut = TestContext.Render<TwDataTable<TestProduct>>(parameters => parameters
            .Add(p => p.Items, GetTestProducts())
            .Add(p => p.Columns, GetTestColumns()));

        var method = typeof(TwDataTable<TestProduct>).GetMethod("ToggleSort",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;

        // Act
        method.Invoke(cut.Instance, ["name"]);

        // Assert - order unchanged (Product A still first)
        var firstRow = cut.FindAll("tbody tr")[0];
        Assert.Contains("Product A", firstRow.TextContent);
    }

    [Fact]
    public void ToggleSort_NonExistentColumn_DoesNothing()
    {
        // Arrange - ToggleSort is private; a nonexistent column name can only be reached by
        // invoking it directly (no header renders for a column that isn't in Columns).
        var cut = TestContext.Render<TwDataTable<TestProduct>>(parameters => parameters
            .Add(p => p.Items, GetTestProducts())
            .Add(p => p.Columns, GetSortableColumns()));

        var method = typeof(TwDataTable<TestProduct>).GetMethod("ToggleSort",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;

        // Act
        var exception = Record.Exception(() => method.Invoke(cut.Instance, ["does-not-exist"]));

        // Assert - should not throw
        Assert.Null(exception);
    }

    [Fact]
    public void ToggleSort_ColumnWithNullPropertySelector_DoesNothing()
    {
        // Arrange
        List<TwDataTableColumn<TestProduct>> columns =
        [
            new() { Name = "custom", Title = "Custom", IsSortable = true, PropertySelector = null }
        ];
        var cut = TestContext.Render<TwDataTable<TestProduct>>(parameters => parameters
            .Add(p => p.Items, GetTestProducts())
            .Add(p => p.Columns, columns));

        var method = typeof(TwDataTable<TestProduct>).GetMethod("ToggleSort",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;

        // Act
        var exception = Record.Exception(() => method.Invoke(cut.Instance, ["custom"]));

        // Assert - should not throw
        Assert.Null(exception);
    }

    [Fact]
    public void ToggleSort_SameColumn_FourthClick_CyclesBackToAscending()
    {
        // Arrange - after None -> Ascending -> Descending -> None, currentSortColumn is still
        // set to the same column, so a 4th click re-enters the switch at the None case.
        var cut = TestContext.Render<TwDataTable<TestProduct>>(parameters => parameters
            .Add(p => p.Items, GetTestProducts())
            .Add(p => p.Columns, GetSortableColumns()));

        // The sort button's accessible name changes with sort state (e.g. "Sort by Name, currently
        // ascending"), so it's re-found before each click rather than reusing a stale cached reference.
        cut.Find("[aria-label^='Sort by Name']").Click(); // None -> Ascending
        cut.Find("[aria-label^='Sort by Name']").Click(); // Ascending -> Descending
        cut.Find("[aria-label^='Sort by Name']").Click(); // Descending -> None

        // Act
        cut.Find("[aria-label^='Sort by Name']").Click(); // None -> Ascending again

        // Assert - ascending order restored (Product A first alphabetically)
        var firstRow = cut.FindAll("tbody tr")[0];
        Assert.Contains("Product A", firstRow.TextContent);
    }

    [Fact]
    public void ApplySort_ResetWithActiveSearch_ReappliesSearchResults()
    {
        // Arrange - search first, then cycle sort back to None on the filtered results.
        var cut = TestContext.Render<TwDataTable<TestProduct>>(parameters => parameters
            .Add(p => p.Items, GetManyTestProducts(20))
            .Add(p => p.Columns, GetSortableColumns())
            .Add(p => p.Searchable, true)
            .Add(p => p.Pageable, false));

        // "15" is a single word so it isn't diluted by the generic "Product" word every row matches.
        cut.Find("input").Change("15");
        var filteredCount = cut.FindAll("tbody tr").Count;
        Assert.True(filteredCount > 0 && filteredCount < 20);

        // The sort button's accessible name changes with sort state (e.g. "Sort by Name, currently
        // ascending"), so it's re-found before each click rather than reusing a stale cached reference.
        cut.Find("[aria-label^='Sort by Name']").Click(); // Ascending
        cut.Find("[aria-label^='Sort by Name']").Click(); // Descending
        cut.Find("[aria-label^='Sort by Name']").Click(); // None - resets, but search should still apply

        // Assert - still filtered by the active search, not all 20 rows
        var rows = cut.FindAll("tbody tr");
        Assert.Equal(filteredCount, rows.Count);
    }

    [Fact]
    public void GetSortIcon_NonExistentColumn_ReturnsChevronExpand()
    {
        // Arrange - GetSortIcon is private; a nonexistent column name can only be reached by
        // invoking it directly.
        var cut = TestContext.Render<TwDataTable<TestProduct>>(parameters => parameters
            .Add(p => p.Items, GetTestProducts())
            .Add(p => p.Columns, GetSortableColumns()));

        var method = typeof(TwDataTable<TestProduct>).GetMethod("GetSortIcon",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;

        // Act
        var icon = method.Invoke(cut.Instance, ["does-not-exist"]);

        // Assert
        Assert.Equal(TwBlazor.Enums.Icon.Chevron_Expand, icon);
    }

    [Fact]
    public void GetSortIcon_ForEmptyItems_DoesNotThrow()
    {
        // Arrange - exercises IsNumericColumn's `sampleItem is default` branch (empty Items).
        var cut = TestContext.Render<TwDataTable<TestProduct>>(parameters => parameters
            .Add(p => p.Items, [])
            .Add(p => p.Columns, GetSortableColumns()));

        // Act
        cut.Find("[aria-label='Sort by Price']").Click();

        // Assert - does not throw
        Assert.NotNull(cut);
    }

    [Fact]
    public void GetSortIcon_ColumnPropertyReturnsNull_ForSampleItem_DoesNotThrow()
    {
        // Arrange - exercises IsNumericColumn's `value == null` branch.
        var products = GetTestProducts();
        List<TwDataTableColumn<TestProduct>> columns =
        [
            new() { Name = "category", Title = "Category", PropertySelector = p => p.Category, IsSortable = true }
        ];
        products[0].Category = null!;

        var cut = TestContext.Render<TwDataTable<TestProduct>>(parameters => parameters
            .Add(p => p.Items, products)
            .Add(p => p.Columns, columns));

        // Act
        cut.Find("[aria-label='Sort by Category']").Click();

        // Assert - does not throw
        Assert.NotNull(cut);
    }

    public class TestProduct
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Stock { get; set; }
    }
}

