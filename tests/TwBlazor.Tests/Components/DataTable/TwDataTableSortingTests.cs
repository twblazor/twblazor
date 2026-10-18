using Bunit;
using TwBlazor.Components;

namespace TwBlazor.Tests.Components.DataTable;

public class TwDataTableSortingTests : TwBlazorTestBase
{
    [Fact]
    public void TwDataTable_SortableColumn_ShowsSortIcon()
    {
        // Arrange
        var products = GetTestProducts();
        List<TwDataTableColumn<TestProduct>> columns =
        [
            new() { Name = "name", Title = "Name", PropertySelector = p => p.Name, IsSortable = true }
        ];

        // Act
        var cut = TestContext.Render<TwDataTable<TestProduct>>(parameters => parameters
            .Add(p => p.Items, products)
            .Add(p => p.Columns, columns));

        // Assert
        var sortIcons = cut.FindAll("th i");
        Assert.NotEmpty(sortIcons);
    }

    [Fact]
    public void TwDataTable_NonSortableColumn_HidesIcon()
    {
        // Arrange
        var products = GetTestProducts();
        List<TwDataTableColumn<TestProduct>> columns =
        [
            new() { Name = "name", Title = "Name", PropertySelector = p => p.Name, IsSortable = false }
        ];

        // Act
        var cut = TestContext.Render<TwDataTable<TestProduct>>(parameters => parameters
            .Add(p => p.Items, products)
            .Add(p => p.Columns, columns));

        // Assert
        var sortIcons = cut.FindAll("th i");
        Assert.Empty(sortIcons);
    }

    [Fact]
    public void TwDataTable_SortAscending_OrdersDataCorrectly()
    {
        // Arrange
        var products = GetUnsortedProducts();
        List<TwDataTableColumn<TestProduct>> columns =
        [
            new() { Name = "name", Title = "Name", PropertySelector = p => p.Name, IsSortable = true },
            new() { Name = "price", Title = "Price", PropertySelector = p => p.Price, IsSortable = true }
        ];

        var cut = TestContext.Render<TwDataTable<TestProduct>>(parameters => parameters
            .Add(p => p.Items, products)
            .Add(p => p.Columns, columns));

        // Act - Click sort button to sort ascending
        var sortButton = cut.Find("th button");
        sortButton.Click();

        // Assert - First row should be "Alpha"
        var firstRow = cut.FindAll("tbody tr")[0];
        Assert.Contains("Alpha", firstRow.TextContent);
    }

    [Fact]
    public void TwDataTable_SortDescending_OrdersDataCorrectly()
    {
        // Arrange
        var products = GetUnsortedProducts();
        List<TwDataTableColumn<TestProduct>> columns =
        [
            new() { Name = "name", Title = "Name", PropertySelector = p => p.Name, IsSortable = true }
        ];

        var cut = TestContext.Render<TwDataTable<TestProduct>>(parameters => parameters
            .Add(p => p.Items, products)
            .Add(p => p.Columns, columns));

        // Act - Click twice to sort descending
        var sortButton = cut.Find("th button");
        sortButton.Click();
        sortButton.Click();

        // Assert - First row should be "Zeta"
        var firstRow = cut.FindAll("tbody tr")[0];
        Assert.Contains("Zeta", firstRow.TextContent);
    }

    [Fact]
    public void TwDataTable_SortNone_ResetsToOriginalOrder()
    {
        // Arrange
        var products = GetUnsortedProducts();
        List<TwDataTableColumn<TestProduct>> columns =
        [
            new() { Name = "name", Title = "Name", PropertySelector = p => p.Name, IsSortable = true }
        ];

        var cut = TestContext.Render<TwDataTable<TestProduct>>(parameters => parameters
            .Add(p => p.Items, products)
            .Add(p => p.Columns, columns));

        // Act - Click three times to reset
        var sortButton = cut.Find("th button");
        sortButton.Click(); // Ascending
        sortButton.Click(); // Descending
        sortButton.Click(); // None

        // Assert - Should be back to original order (Charlie first)
        var firstRow = cut.FindAll("tbody tr")[0];
        Assert.Contains("Charlie", firstRow.TextContent);
    }

    [Fact]
    public void TwDataTable_SortNumericColumn_UsesNumericIcons()
    {
        // Arrange
        var products = GetTestProducts();
        List<TwDataTableColumn<TestProduct>> columns =
        [
            new() { Name = "price", Title = "Price", PropertySelector = p => p.Price, IsSortable = true }
        ];

        var cut = TestContext.Render<TwDataTable<TestProduct>>(parameters => parameters
            .Add(p => p.Items, products)
            .Add(p => p.Columns, columns));

        // Act - Click to sort
        var sortButton = cut.Find("th button");
        sortButton.Click();

        // Assert - Should show numeric sort icon (bi-sort-numeric-up)
        var icon = cut.Find("th i");
        Assert.Contains("bi-sort-numeric-up", icon.GetAttribute("class"));
    }

    [Fact]
    public void TwDataTable_SortAlphaColumn_UsesAlphaIcons()
    {
        // Arrange
        var products = GetTestProducts();
        List<TwDataTableColumn<TestProduct>> columns =
        [
            new() { Name = "name", Title = "Name", PropertySelector = p => p.Name, IsSortable = true }
        ];

        var cut = TestContext.Render<TwDataTable<TestProduct>>(parameters => parameters
            .Add(p => p.Items, products)
            .Add(p => p.Columns, columns));

        // Act - Click to sort
        var sortButton = cut.Find("th button");
        sortButton.Click();

        // Assert - Should show alpha sort icon (bi-sort-alpha-up)
        var icon = cut.Find("th i");
        Assert.Contains("bi-sort-alpha-up", icon.GetAttribute("class"));
    }

    [Fact]
    public void TwDataTable_UnsortedColumn_ShowsChevronExpand()
    {
        // Arrange
        var products = GetTestProducts();
        List<TwDataTableColumn<TestProduct>> columns =
        [
            new() { Name = "name", Title = "Name", PropertySelector = p => p.Name, IsSortable = true }
        ];

        // Act
        var cut = TestContext.Render<TwDataTable<TestProduct>>(parameters => parameters
            .Add(p => p.Items, products)
            .Add(p => p.Columns, columns));

        // Assert - Should show chevron-expand by default
        var icon = cut.Find("th i");
        Assert.Contains("bi-chevron-expand", icon.GetAttribute("class"));
    }

    [Fact]
    public void TwDataTable_SortingWithPagination_ResetsToFirstPage()
    {
        // Arrange
        var products = GetManyTestProducts(20);
        List<TwDataTableColumn<TestProduct>> columns =
        [
            new() { Name = "name", Title = "Name", PropertySelector = p => p.Name, IsSortable = true }
        ];

        var cut = TestContext.Render<TwDataTable<TestProduct>>(parameters => parameters
            .Add(p => p.Items, products)
            .Add(p => p.Columns, columns)
            .Add(p => p.EnablePagination, true)
            .Add(p => p.RowsPerPage, 5));

        // Navigate to page 2 - TwPagination renders Previous/page-numbers/Next as a single link list; Next is last.
        var paginationLinks = cut.FindAll("nav[aria-label='Table pagination'] ul li button");
        paginationLinks[^1].Click();

        // Act - Click sort
        var sortButton = cut.Find("th button");
        sortButton.Click();

        // Assert - Should be back on page 1
        paginationLinks = cut.FindAll("nav[aria-label='Table pagination'] ul li button");
        var activeLink = paginationLinks.Single(a => a.GetAttribute("aria-current") == "page");
        Assert.Equal("1", activeLink.TextContent.Replace("page ", string.Empty).Trim());
    }

    [Fact]
    public void TwDataTable_SortingWithSearch_SortsFilteredResults()
    {
        // Arrange
        List<TestProduct> products =
        [
            new() { Id = 1, Name = "Apple Laptop", Price = 999m },
            new() { Id = 2, Name = "Apple Phone", Price = 799m },
            new() { Id = 3, Name = "Samsung Phone", Price = 699m }
        ];

        List<TwDataTableColumn<TestProduct>> columns =
        [
            new() { Name = "name", Title = "Name", PropertySelector = p => p.Name, IsSortable = true },
            new() { Name = "price", Title = "Price", PropertySelector = p => p.Price, IsSortable = true }
        ];

        var cut = TestContext.Render<TwDataTable<TestProduct>>(parameters => parameters
            .Add(p => p.Items, products)
            .Add(p => p.Columns, columns)
            .Add(p => p.Searchable, true));

        // Search for "Apple"
        var searchInput = cut.Find("input");
        searchInput.Change("Apple");

        // Act - Sort by price
        var sortButtons = cut.FindAll("th button");
        var priceSortButton = sortButtons[1]; // Second button is price
        priceSortButton.Click();

        // Assert - Should show Apple Phone first (lower price)
        var firstRow = cut.FindAll("tbody tr")[0];
        Assert.Contains("Apple Phone", firstRow.TextContent);
        Assert.Contains("799", firstRow.TextContent);
    }

    [Fact]
    public void TwDataTable_MultipleSortableColumns_OnlyOneSortedAtTime()
    {
        // Arrange
        var products = GetTestProducts();
        List<TwDataTableColumn<TestProduct>> columns =
        [
            new() { Name = "name", Title = "Name", PropertySelector = p => p.Name, IsSortable = true },
            new() { Name = "price", Title = "Price", PropertySelector = p => p.Price, IsSortable = true }
        ];

        var cut = TestContext.Render<TwDataTable<TestProduct>>(parameters => parameters
            .Add(p => p.Items, products)
            .Add(p => p.Columns, columns));

        // Act - Sort by name, then by price
        var sortButtons = cut.FindAll("th button");
        sortButtons[0].Click(); // Sort name ascending
        sortButtons[1].Click(); // Sort price ascending

        // Assert - First column should show chevron-expand, second should show sort icon
        var icons = cut.FindAll("th i");
        Assert.Contains("bi-chevron-expand", icons[0].GetAttribute("class"));
        Assert.Contains("bi-sort-numeric-up", icons[1].GetAttribute("class"));
    }

    // Helper methods
    private static List<TestProduct> GetTestProducts()
    {
        return
        [
            new() { Id = 1, Name = "Laptop", Price = 999.99m, Stock = 10 },
            new() { Id = 2, Name = "Mouse", Price = 29.99m, Stock = 50 },
            new() { Id = 3, Name = "Keyboard", Price = 79.99m, Stock = 25 }
        ];
    }

    private static List<TestProduct> GetUnsortedProducts()
    {
        return
        [
            new() { Id = 1, Name = "Charlie", Price = 50m },
            new() { Id = 2, Name = "Alpha", Price = 30m },
            new() { Id = 3, Name = "Zeta", Price = 70m },
            new() { Id = 4, Name = "Beta", Price = 40m }
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
                Price = 10m + (i * 5m),
                Stock = i * 2
            });
        }
        return products;
    }

    public class TestProduct
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Stock { get; set; }
    }
}
