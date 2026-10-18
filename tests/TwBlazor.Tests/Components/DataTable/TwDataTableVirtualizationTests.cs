using Bunit;
using TwBlazor.Components;
using TwBlazor.Configuration.Components;
using TwBlazor.Enums;

namespace TwBlazor.Tests.Components.DataTable;

public class TwDataTableVirtualizationTests : TwBlazorTestBase
{
    public TwDataTableVirtualizationTests()
    {
        TestContext.JSInterop.Setup<bool>("twDevice.prefersNativePicker").SetResult(true);
    }

    private TwTableTheme tableTheme => Theme.Components.Require<TwTableTheme>();

    private static List<Product> Products(int count = 30) =>
        [.. Enumerable.Range(1, count).Select(i => new Product($"Product {i:D2}", i % 2 == 0 ? "Even" : "Odd", i))];

    private static List<TwDataTableColumn<Product>> Columns() =>
    [
        new() { Name = "name", Title = "Name", PropertySelector = p => p.Name, IsSortable = true },
        new() { Name = "category", Title = "Category", PropertySelector = p => p.Category, Groupable = true },
        new() { Name = "price", Title = "Price", PropertySelector = p => p.Price, IsSortable = true }
    ];

    private IRenderedComponent<TwDataTable<Product>> Render(
        Action<ComponentParameterCollectionBuilder<TwDataTable<Product>>>? configure = null,
        List<Product>? items = null,
        bool withColumns = true,
        bool virtualize = true) =>
        TestContext.Render<TwDataTable<Product>>(parameters =>
        {
            parameters
                .Add(p => p.Id, "products")
                .Add(p => p.Items, items ?? Products())
                .Add(p => p.Columns, withColumns ? Columns() : null)
                .Add(p => p.EnableVirtualization, virtualize);
            configure?.Invoke(parameters);
        });

    private static List<string> RowNames(IRenderedComponent<TwDataTable<Product>> cut) =>
        [.. cut.FindAll("tbody tr[aria-rowindex]").Where(r => r.QuerySelector("td") is not null).Select(r => r.Children[0].TextContent.Trim())];

    [Fact]
    public void Virtualize_ReplacesThePagination_WithAScrollingTable()
    {
        // Act
        var cut = Render();

        // Assert
        Assert.Empty(cut.FindAll("nav[aria-label='Table pagination']"));
        Assert.Equal("max-height:32rem", cut.Find("table").ParentElement!.GetAttribute("style"));
        Assert.Contains(tableTheme.StickyHeader, cut.Find("thead").GetAttribute("class"));
    }

    [Fact]
    public void Virtualize_TellsAssistiveTechnologyTheRowCountAndEachRowsPlace()
    {
        // Act
        var cut = Render(items: Products(3));

        // Assert
        Assert.Equal("4", cut.Find("table").GetAttribute("aria-rowcount"));
        Assert.Equal("1", cut.Find("thead tr").GetAttribute("aria-rowindex"));
        var rows = cut.FindAll("tbody tr[aria-rowindex]");
        Assert.Equal(["2", "3", "4"], rows.Select(r => r.GetAttribute("aria-rowindex")));
    }

    [Fact]
    public void Virtualize_FollowsTheSortAndSearch()
    {
        // Arrange
        var cut = Render(p => p.Add(x => x.Searchable, true), items: Products(12));

        // Act
        cut.Find("[aria-label='Sort by Price']").Click();
        cut.Find("[aria-label='Sort by Price, currently ascending']").Click();
        cut.Find("#products-search").Change("Odd");

        // Assert - the odd products, highest price first.
        Assert.Equal(["Product 11", "Product 09", "Product 07", "Product 05", "Product 03", "Product 01"], RowNames(cut));
        Assert.Equal("7", cut.Find("table").GetAttribute("aria-rowcount"));
    }

    [Fact]
    public void Height_OverridesTheDefault()
    {
        // Act
        var cut = Render(p => p.Add(x => x.Height, "60vh"));

        // Assert
        Assert.Equal("max-height:60vh", cut.Find("table").ParentElement!.GetAttribute("style"));
    }

    [Fact]
    public void Height_AndStickyHeader_WorkWithoutVirtualizing()
    {
        // Act
        var cut = Render(p => p.Add(x => x.Height, "20rem").Add(x => x.StickyHeader, true), virtualize: false);

        // Assert
        Assert.Equal("max-height:20rem", cut.Find("table").ParentElement!.GetAttribute("style"));
        Assert.Contains(tableTheme.StickyHeader, cut.Find("thead").GetAttribute("class"));
        Assert.Null(cut.Find("table").GetAttribute("aria-rowcount"));
        Assert.NotEmpty(cut.FindAll("nav[aria-label='Table pagination']"));
    }

    [Fact]
    public void Virtualize_ShowsTheEmptyMessage_WhenThereAreNoRows()
    {
        // Act
        var cut = Render(items: []);

        // Assert
        Assert.Equal("No data available.", cut.Find("tbody td").TextContent.Trim());
    }

    [Fact]
    public void Virtualize_WithGrouping_PutsHeadersAndRowsInOneBody()
    {
        // Act
        var cut = Render(p => p.Add(x => x.GroupBy, "category"), items: Products(4));

        // Assert - Even, 02, 04, Odd, 01, 03.
        Assert.Single(cut.FindAll("tbody"));
        var rows = cut.FindAll("tbody tr[aria-rowindex]");
        Assert.Equal(["2", "3", "4", "5", "6", "7"], rows.Select(r => r.GetAttribute("aria-rowindex")));
        var header = rows[0].QuerySelector("th")!;
        Assert.Null(header.GetAttribute("scope"));
        Assert.Contains("Even", header.TextContent);
        Assert.Equal("7", cut.Find("table").GetAttribute("aria-rowcount"));
    }

    [Fact]
    public void Virtualize_WithGrouping_CollapsesAGroup()
    {
        // Arrange
        var cut = Render(p => p.Add(x => x.GroupBy, "category"), items: Products(4));

        // Act
        cut.Find("tbody th button").Click();

        // Assert
        // Even's two rows are hidden, leaving the Even header, the Odd header and Odd's two rows.
        Assert.Equal(4, cut.FindAll("tbody tr[aria-rowindex]").Count);
    }

    [Fact]
    public void Virtualize_WithoutColumns_VirtualizesTheManualRows()
    {
        // Arrange
        Microsoft.AspNetCore.Components.RenderFragment<Product> row = product => builder =>
        {
            builder.OpenElement(0, "td");
            builder.AddContent(1, product.Name);
            builder.CloseElement();
        };

        // Act
        var cut = Render(p => p.Add(x => x.TableRow, row), withColumns: false);

        // Assert
        // Rows written with TableRow are virtualized too: every row (bUnit renders them all) between Virtualize's two
        // spacer rows, and no pagination.
        Assert.Equal(30, cut.FindAll("tbody tr td").Count);
        Assert.Equal(32, cut.FindAll("tbody tr").Count);
        Assert.Empty(cut.FindAll("nav[aria-label='Table pagination']"));
    }

    public record Product(string Name, string Category, int Price);
}

public class TwDataTableItemsProviderTests : TwBlazorTestBase
{
    public TwDataTableItemsProviderTests()
    {
        TestContext.JSInterop.Setup<bool>("twDevice.prefersNativePicker").SetResult(true);
    }

    private readonly List<TwDataTableItemsRequest> _requests = [];

    private static readonly List<Product> _all =
        [.. Enumerable.Range(1, 12).Select(i => new Product($"Product {i:D2}", i % 3 == 0 ? "Fizz" : "Other", i))];

    private static List<TwDataTableColumn<Product>> Columns() =>
    [
        new() { Name = "name", Title = "Name", PropertySelector = p => p.Name, IsSortable = true },
        new() { Name = "category", Title = "Category", PropertySelector = p => p.Category, Groupable = true },
        new() { Name = "price", Title = "Price", PropertySelector = p => p.Price, IsSortable = true }
    ];

    /// <summary>
    /// A provider that searches, sorts and pages <see cref="_all"/> the way a server would, and records each request.
    /// </summary>
    private ValueTask<TwDataTableItemsResult<Product>> Provide(TwDataTableItemsRequest request)
    {
        _requests.Add(request);
        IEnumerable<Product> rows = _all;
        if (request.SearchQuery is { } query)
        {
            rows = rows.Where(p => p.Name.Contains(query, StringComparison.OrdinalIgnoreCase) || p.Category.Contains(query, StringComparison.OrdinalIgnoreCase));
        }
        if (request.SortColumn == "price")
        {
            rows = request.SortDirection == SortDirection.Descending ? rows.OrderByDescending(p => p.Price) : rows.OrderBy(p => p.Price);
        }

        var matching = rows.ToList();
        var page = matching.Skip(request.StartIndex);
        if (request.Count is { } count)
        {
            page = page.Take(count);
        }

        return ValueTask.FromResult(new TwDataTableItemsResult<Product>([.. page], matching.Count));
    }

    private IRenderedComponent<TwDataTable<Product>> Render(
        Action<ComponentParameterCollectionBuilder<TwDataTable<Product>>>? configure = null,
        TwDataTableItemsProvider<Product>? provider = null) =>
        TestContext.Render<TwDataTable<Product>>(parameters =>
        {
            parameters
                .Add(p => p.Id, "products")
                .Add(p => p.Columns, Columns())
                .Add(p => p.ItemsProvider, provider ?? Provide)
                .Add(p => p.RowsPerPage, 5);
            configure?.Invoke(parameters);
        });

    private static List<string> RowNames(IRenderedComponent<TwDataTable<Product>> cut) =>
        [.. cut.FindAll("tbody tr").Select(r => r.Children[0].TextContent.Trim())];

    private static void NextPage(IRenderedComponent<TwDataTable<Product>> cut) =>
        cut.Find("nav[aria-label='Table pagination'] button[aria-label='Next page']").Click();

    [Fact]
    public void LoadsTheFirstPage_FromTheProvider()
    {
        // Act
        var cut = Render();

        // Assert
        var request = Assert.Single(_requests);
        Assert.Equal(0, request.StartIndex);
        Assert.Equal(5, request.Count);
        Assert.Equal(["Product 01", "Product 02", "Product 03", "Product 04", "Product 05"], RowNames(cut));
        // 12 rows at 5 a page, as the provider's total says.
        var pages = cut.FindAll("nav[aria-label='Table pagination'] li button")
            .Select(b => b.TextContent.Replace("page ", string.Empty).Trim())
            .Where(text => int.TryParse(text, out _));
        Assert.Equal(["1", "2", "3"], pages);
    }

    [Fact]
    public async Task ChangingPage_AsksForThatPage()
    {
        // Arrange
        var cut = Render();

        // Act
        NextPage(cut);
        await cut.WaitForStateAsync(() => _requests.Count == 2);

        // Assert
        Assert.Equal(5, _requests[^1].StartIndex);
        Assert.Equal("Product 06", RowNames(cut)[0]);
    }

    [Fact]
    public async Task Sorting_SendsTheSortAndReturnsToTheFirstPage()
    {
        // Arrange
        var cut = Render();
        NextPage(cut);
        await cut.WaitForStateAsync(() => _requests.Count == 2);

        // Act
        cut.Find("[aria-label='Sort by Price']").Click();
        cut.Find("[aria-label='Sort by Price, currently ascending']").Click();
        await cut.WaitForStateAsync(() => _requests.Count == 4);

        // Assert
        var request = _requests[^1];
        Assert.Equal("price", request.SortColumn);
        Assert.Equal(SortDirection.Descending, request.SortDirection);
        Assert.Equal(0, request.StartIndex);
        Assert.Equal("Product 12", RowNames(cut)[0]);
    }

    [Fact]
    public async Task Searching_SendsTheQuery_AndAnnouncesTheMatchesOnceLoaded()
    {
        // Arrange
        var cut = Render(p => p.Add(x => x.Searchable, true));

        // Act
        cut.Find("#products-search").Change("Fizz");
        await cut.WaitForStateAsync(() => _requests.Count == 2);

        // Assert
        Assert.Equal("Fizz", _requests[^1].SearchQuery);
        Assert.Equal(4, RowNames(cut).Count);
        Assert.Equal("Showing 4 matching rows", cut.Find("[role='status']").TextContent.Trim());
    }

    [Fact]
    public async Task WhileLoading_ShowsTheLoadingRow_AndMarksTheTableBusy()
    {
        // Arrange
        var pending = new TaskCompletionSource<TwDataTableItemsResult<Product>>();

        // Act
        var cut = Render(provider: _ => new ValueTask<TwDataTableItemsResult<Product>>(pending.Task));

        // Assert
        Assert.Equal("true", cut.Find("table").GetAttribute("aria-busy"));
        Assert.Equal("Loading…", cut.Find("tbody td").TextContent.Trim());

        // Act
        await cut.InvokeAsync(() => pending.SetResult(new([.. _all.Take(2)], 2)));

        // Assert
        await cut.WaitForAssertionAsync(() => Assert.Equal(["Product 01", "Product 02"], RowNames(cut)));
        Assert.Null(cut.Find("table").GetAttribute("aria-busy"));
    }

    [Fact]
    public async Task LoadingContent_ReplacesTheLoadingText()
    {
        // Arrange
        var pending = new TaskCompletionSource<TwDataTableItemsResult<Product>>();

        // Act
        var cut = Render(p => p.Add(x => x.LoadingContent, RenderFragmentBuilder("Fetching orders")),
            provider: _ => new ValueTask<TwDataTableItemsResult<Product>>(pending.Task));

        // Assert
        Assert.Equal("Fetching orders", cut.Find("tbody td").TextContent.Trim());
        await cut.InvokeAsync(() => pending.SetResult(new([], 0)));
    }

    [Fact]
    public async Task ANewLoad_CancelsTheOneStillRunning()
    {
        // Arrange
        var tokens = new List<CancellationToken>();
        var pending = new TaskCompletionSource<TwDataTableItemsResult<Product>>();
        var cut = Render(provider: request =>
        {
            tokens.Add(request.CancellationToken);
            return tokens.Count == 1 ? new ValueTask<TwDataTableItemsResult<Product>>(pending.Task) : Provide(request);
        });

        // Act
        cut.Find("[aria-label='Sort by Price']").Click();
        await cut.WaitForStateAsync(() => tokens.Count == 2);
        await cut.InvokeAsync(() => pending.SetResult(new([new("Stale", "Old", 0)], 1)));

        // Assert
        Assert.True(tokens[0].IsCancellationRequested);
        Assert.DoesNotContain("Stale", RowNames(cut));
    }

    [Fact]
    public async Task APageBeyondTheData_LoadsTheLastPageInstead()
    {
        // Act
        var cut = Render(p => p.Add(x => x.State, new TwDataTableState { Page = 9 }));

        // Assert - 12 rows at 5 a page ends on page 3.
        await cut.WaitForAssertionAsync(() => Assert.Equal(["Product 11", "Product 12"], RowNames(cut)));
        Assert.Equal(10, _requests[^1].StartIndex);
    }

    [Fact]
    public async Task RefreshDataAsync_AsksAgain()
    {
        // Arrange
        var cut = Render();

        // Act
        await cut.InvokeAsync(() => cut.Instance.RefreshDataAsync());

        // Assert
        Assert.Equal(2, _requests.Count);
    }

    [Fact]
    public void WithoutPaging_AsksForEveryRow()
    {
        // Act
        var cut = Render(p => p.Add(x => x.EnablePagination, false));

        // Assert
        Assert.Null(Assert.Single(_requests).Count);
        Assert.Equal(12, RowNames(cut).Count);
    }

    [Fact]
    public void Grouping_IsNotOffered()
    {
        // Act
        var cut = Render(p => p.Add(x => x.GroupBy, "category"));

        // Assert
        Assert.Empty(cut.FindAll("#products-group-by"));
        Assert.Empty(cut.FindAll("th[scope='rowgroup']"));
    }

    [Fact]
    public async Task ExportingTheFilteredRows_AsksTheProviderForAllOfThem()
    {
        // Arrange
        var cut = Render(p => p.Add(x => x.Searchable, true));
        cut.Find("#products-search").Change("Fizz");
        await cut.WaitForStateAsync(() => _requests.Count == 2);

        // Act
        await cut.InvokeAsync(() => cut.Instance.ExportCsvAsync());

        // Assert
        var request = _requests[^1];
        Assert.Equal(0, request.StartIndex);
        Assert.Null(request.Count);
        Assert.Equal("Fizz", request.SearchQuery);
        Assert.Equal("Exported 4 rows", cut.Find("[role='status']").TextContent.Trim());
    }

    [Fact]
    public async Task ExportingAllRows_IgnoresTheSearch()
    {
        // Arrange
        var cut = Render(p => p.Add(x => x.Searchable, true));
        cut.Find("#products-search").Change("Fizz");
        await cut.WaitForStateAsync(() => _requests.Count == 2);

        // Act
        await cut.InvokeAsync(() => cut.Instance.ExportExcelAsync(TwDataTableExportScope.All));

        // Assert
        Assert.Null(_requests[^1].SearchQuery);
        Assert.Equal("Exported 12 rows", cut.Find("[role='status']").TextContent.Trim());
    }

    [Fact]
    public void GetCsv_WritesTheLoadedRows()
    {
        // Arrange
        var cut = Render();

        // Act
        var lines = cut.Instance.GetCsv().Split("\r\n", StringSplitOptions.RemoveEmptyEntries);

        // Assert
        Assert.Equal(6, lines.Length);
    }

    [Fact]
    public void Virtualized_LoadsTheRowsInView_WithTheirPlaces()
    {
        // Act
        var cut = Render(p => p.Add(x => x.EnableVirtualization, true));

        // Assert
        Assert.NotEmpty(_requests);
        Assert.Equal("13", cut.Find("table").GetAttribute("aria-rowcount"));
        var first = cut.Find("tbody tr[aria-rowindex]");
        Assert.Equal("2", first.GetAttribute("aria-rowindex"));
        Assert.Equal("Product 01", first.Children[0].TextContent.Trim());
    }

    [Fact]
    public async Task Virtualized_Sorting_AsksForTheRowsAgain()
    {
        // Arrange
        var cut = Render(p => p.Add(x => x.EnableVirtualization, true));
        var before = _requests.Count;

        // Act
        cut.Find("[aria-label='Sort by Price']").Click();
        await cut.WaitForStateAsync(() => _requests.Count > before);

        // Assert
        Assert.Equal("price", _requests[^1].SortColumn);
    }

    [Fact]
    public void Virtualized_ShowsTheEmptyMessage_WhenNothingMatches()
    {
        // Act
        var cut = Render(p => p.Add(x => x.EnableVirtualization, true), provider: _ => ValueTask.FromResult(new TwDataTableItemsResult<Product>([], 0)));

        // Assert
        Assert.Equal("No data available.", cut.Find("tbody td").TextContent.Trim());
    }

    public record Product(string Name, string Category, int Price);
}
