using Bunit;
using System.Text.Json;
using TwBlazor.Components;
using TwBlazor.Enums;

namespace TwBlazor.Tests.Components.DataTable;

public class TwDataTableStateTests : TwBlazorTestBase
{
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web);

    public TwDataTableStateTests()
    {
        TestContext.JSInterop.Setup<bool>("twDevice.prefersNativePicker").SetResult(true);
    }

    private static List<Product> Products() =>
    [
        new("Kettle", "Kitchen", 30),
        new("Armchair", "Furniture", 250),
        new("Blender", "Kitchen", 60),
        new("Desk", "Furniture", 180),
        new("Toaster", "Kitchen", 25)
    ];

    private static List<TwDataTableColumn<Product>> Columns() =>
    [
        new() { Name = "name", Title = "Name", PropertySelector = p => p.Name, IsSortable = true },
        new() { Name = "category", Title = "Category", PropertySelector = p => p.Category },
        new() { Name = "price", Title = "Price", PropertySelector = p => p.Price, IsSortable = true }
    ];

    private IRenderedComponent<TwDataTable<Product>> Render(
        Action<ComponentParameterCollectionBuilder<TwDataTable<Product>>>? configure = null,
        List<Product>? items = null) =>
        TestContext.Render<TwDataTable<Product>>(parameters =>
        {
            parameters
                .Add(p => p.Id, "products")
                .Add(p => p.Items, items ?? Products())
                .Add(p => p.Columns, Columns())
                .Add(p => p.Searchable, true)
                .Add(p => p.RowsPerPage, 2);
            configure?.Invoke(parameters);
        });

    private static List<string> FirstCells(IRenderedComponent<TwDataTable<Product>> cut) =>
        [.. cut.FindAll("tbody tr").Select(r => r.Children[0].TextContent.Trim())];

    private static List<string> Headers(IRenderedComponent<TwDataTable<Product>> cut) =>
        [.. cut.FindAll("thead th").Select(th => th.QuerySelector("span")!.TextContent.Trim())];

    [Fact]
    public async Task Search_KeepsTheActiveSort()
    {
        // Arrange
        var cut = Render(p => p.Add(x => x.EnablePagination, false));
        cut.Find("[aria-label='Sort by Name']").Click();

        // Act
        await cut.InvokeAsync(() => cut.Find("#products-search").Change("Kitchen"));

        // Assert
        Assert.Equal(["Blender", "Kettle", "Toaster"], FirstCells(cut));
    }

    [Fact]
    public void ItemsChange_KeepsTheActiveSort()
    {
        // Arrange
        var cut = Render(p => p.Add(x => x.EnablePagination, false));
        cut.Find("[aria-label='Sort by Name']").Click();

        // Act
        cut.Render(p => p.Add(x => x.Items, [.. Products(), new Product("Clock", "Decor", 40)]));

        // Assert
        Assert.Equal(["Armchair", "Blender", "Clock", "Desk", "Kettle", "Toaster"], FirstCells(cut));
    }

    [Fact]
    public void EmptyTable_ShowsTheLocalizedNoDataMessage_AcrossEveryColumn()
    {
        // Arrange & Act
        var cut = Render(items: []);

        // Assert
        var cell = cut.Find("tbody td");
        Assert.Equal("No data available.", cell.TextContent.Trim());
        Assert.Equal("3", cell.GetAttribute("colspan"));
    }

    [Fact]
    public async Task SearchWithNoMatches_ShowsTheNoResultsMessage()
    {
        // Arrange
        var cut = Render();

        // Act
        await cut.InvokeAsync(() => cut.Find("#products-search").Change("zzz"));

        // Assert
        Assert.Equal("No results match your search", cut.Find("tbody td").TextContent.Trim());
    }

    [Fact]
    public async Task GetState_DescribesTheSortSearchPageAndColumns()
    {
        // Arrange
        var cut = Render();
        cut.Find("[aria-label='Sort by Price']").Click();
        cut.Find("[aria-label='Sort by Price, currently ascending']").Click();
        await cut.InvokeAsync(() => cut.Find("#products-search").Change("Kitchen"));

        // Act
        var state = cut.Instance.GetState();

        // Assert
        Assert.Equal("price", state.SortColumn);
        Assert.Equal(SortDirection.Descending, state.SortDirection);
        Assert.Equal("Kitchen", state.SearchQuery);
        Assert.Equal(1, state.Page);
        Assert.Equal(2, state.RowsPerPage);
        Assert.Equal(["name", "category", "price"], state.Columns.Select(c => c.Name));
        Assert.All(state.Columns, c => Assert.True(c.Visible));
    }

    [Fact]
    public void StateParameter_RestoresSortSearchPageAndColumns()
    {
        // Arrange
        var state = new TwDataTableState
        {
            SortColumn = "price",
            SortDirection = SortDirection.Ascending,
            SearchQuery = "Kitchen",
            Page = 2,
            RowsPerPage = 2,
            Columns =
            [
                new() { Name = "price", Width = "6rem" },
                new() { Name = "name" },
                new() { Name = "category", Visible = false }
            ]
        };

        // Act
        var cut = Render(p => p.Add(x => x.State, state));

        // Assert - Kitchen sorted by price is Toaster, Kettle, Blender, so page 2 holds Blender.
        Assert.Equal(["Price", "Name"], Headers(cut));
        Assert.Equal(["60"], FirstCells(cut));
        Assert.Equal("Kitchen", cut.Find("#products-search").GetAttribute("value"));
        Assert.Equal("width:6rem;", cut.Find("colgroup col").GetAttribute("style"));
    }

    [Fact]
    public void StateParameter_IgnoresUnknownColumnsAndAppendsNewOnes()
    {
        // Arrange
        var state = new TwDataTableState
        {
            Columns = [new() { Name = "removed" }, new() { Name = "price" }, new() { Name = "name" }]
        };

        // Act
        var cut = Render(p => p.Add(x => x.State, state));

        // Assert
        Assert.Equal(["Price", "Name", "Category"], Headers(cut));
    }

    [Fact]
    public void StateParameter_IgnoresASortOnAColumnThatCannotSort()
    {
        // Arrange
        var state = new TwDataTableState { SortColumn = "category", SortDirection = SortDirection.Ascending };

        // Act
        var cut = Render(p => p.Add(x => x.State, state).Add(x => x.EnablePagination, false));

        // Assert
        Assert.Null(cut.Instance.GetState().SortColumn);
        Assert.Equal("Kettle", FirstCells(cut)[0]);
    }

    [Fact]
    public void StateParameter_ClampsAPageBeyondTheLastOne()
    {
        // Arrange & Act
        var cut = Render(p => p.Add(x => x.State, new TwDataTableState { Page = 99 }));

        // Assert - 5 rows at 2 a page is 3 pages.
        Assert.Equal(3, cut.Instance.GetState().Page);
    }

    [Fact]
    public void StateParameter_KeepsAtLeastOneColumnVisible()
    {
        // Arrange
        var state = new TwDataTableState
        {
            Columns = [new() { Name = "name", Visible = false }, new() { Name = "category", Visible = false }, new() { Name = "price", Visible = false }]
        };

        // Act
        var cut = Render(p => p.Add(x => x.State, state));

        // Assert
        Assert.Equal(["Name"], Headers(cut));
    }

    [Fact]
    public void StateChanged_IsRaisedWithASnapshot_WhenAPersonSorts()
    {
        // Arrange
        TwDataTableState? raised = null;
        var cut = Render(p => p.Add(x => x.StateChanged, (TwDataTableState s) => raised = s));

        // Act
        cut.Find("[aria-label='Sort by Name']").Click();

        // Assert
        Assert.NotNull(raised);
        Assert.Equal("name", raised.SortColumn);
        Assert.Equal(SortDirection.Ascending, raised.SortDirection);
    }

    [Fact]
    public void StateChanged_IsRaised_WhenThePageChanges()
    {
        // Arrange
        TwDataTableState? raised = null;
        var cut = Render(p => p.Add(x => x.StateChanged, (TwDataTableState s) => raised = s));

        // Act
        cut.Find("nav[aria-label='Table pagination'] button[aria-label='Next page']").Click();

        // Assert
        Assert.Equal(2, raised?.Page);
    }

    [Fact]
    public void BoundState_IsNotAppliedAgain_WhenTheParentPassesBackTheRaisedSnapshot()
    {
        // Arrange - simulate @bind-State: the parent stores the snapshot and passes it straight back.
        TwDataTableState? bound = null;
        IRenderedComponent<TwDataTable<Product>>? cut = null;
        cut = Render(p => p.Add(x => x.StateChanged, (TwDataTableState s) =>
        {
            bound = s;
            cut!.Render(q => q.Add(x => x.State, bound));
        }));

        // Act
        cut.Find("nav[aria-label='Table pagination'] button[aria-label='Next page']").Click();

        // Assert - still on page 2 with the rows the person moved to.
        Assert.Equal(2, cut.Instance.GetState().Page);
        Assert.Equal(["Blender", "Desk"], FirstCells(cut));
    }

    [Fact]
    public async Task SetStateAsync_AppliesTheStateAndRaisesStateChanged()
    {
        // Arrange
        TwDataTableState? raised = null;
        var cut = Render(p => p.Add(x => x.StateChanged, (TwDataTableState s) => raised = s).Add(x => x.EnablePagination, false));

        // Act
        await cut.InvokeAsync(() => cut.Instance.SetStateAsync(new TwDataTableState { SortColumn = "price", SortDirection = SortDirection.Descending }));

        // Assert
        Assert.Equal("Armchair", FirstCells(cut)[0]);
        Assert.Equal("price", raised?.SortColumn);
    }

    [Fact]
    public async Task SetStateAsync_RejectsNull()
    {
        // Arrange
        var cut = Render();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => cut.Instance.SetStateAsync(null!));
    }

    [Fact]
    public async Task ResetStateAsync_ReturnsToTheParameters()
    {
        // Arrange
        var cut = Render(p => p.Add(x => x.State, new TwDataTableState
        {
            SortColumn = "price",
            SortDirection = SortDirection.Ascending,
            SearchQuery = "Kitchen",
            Columns = [new() { Name = "price" }, new() { Name = "name", Visible = false }]
        }));

        // Act
        await cut.InvokeAsync(() => cut.Instance.ResetStateAsync());

        // Assert
        var state = cut.Instance.GetState();
        Assert.Null(state.SortColumn);
        Assert.Null(state.SearchQuery);
        Assert.Equal(["Name", "Category", "Price"], Headers(cut));
    }

    [Fact]
    public void Clone_CopiesEveryValue_WithoutSharingTheColumns()
    {
        // Arrange
        var state = new TwDataTableState
        {
            SortColumn = "name",
            SortDirection = SortDirection.Descending,
            SearchQuery = "a",
            Page = 3,
            RowsPerPage = 25,
            Columns = [new() { Name = "name", Visible = false, Width = "10rem" }]
        };

        // Act
        var copy = state.Clone();
        copy.Columns[0].Width = "20rem";

        // Assert
        Assert.Equal("name", copy.SortColumn);
        Assert.Equal(SortDirection.Descending, copy.SortDirection);
        Assert.Equal("a", copy.SearchQuery);
        Assert.Equal(3, copy.Page);
        Assert.Equal(25, copy.RowsPerPage);
        Assert.False(copy.Columns[0].Visible);
        Assert.Equal("10rem", state.Columns[0].Width);
    }

    [Fact]
    public void StateStorageKey_RestoresTheSavedState_AfterTheFirstRender()
    {
        // Arrange
        var saved = JsonSerializer.Serialize(
            new TwDataTableState { SortColumn = "name", SortDirection = SortDirection.Descending },
            _jsonOptions);
        TestContext.JSInterop.Setup<string?>("twDataTable.loadState", "products-table").SetResult(saved);

        // Act
        var cut = Render(p => p.Add(x => x.StateStorageKey, "products-table").Add(x => x.EnablePagination, false));

        // Assert
        Assert.Equal("Toaster", FirstCells(cut)[0]);
    }

    [Fact]
    public void StateStorageKey_SavesTheState_WhenAPersonChangesIt()
    {
        // Arrange
        TestContext.JSInterop.Setup<string?>("twDataTable.loadState", "products-table").SetResult(null);
        var cut = Render(p => p.Add(x => x.StateStorageKey, "products-table"));

        // Act
        cut.Find("[aria-label='Sort by Name']").Click();

        // Assert
        var save = Assert.Single(TestContext.JSInterop.Invocations, i => i.Identifier == "twDataTable.saveState");
        Assert.Equal("products-table", save.Arguments[0]);
        var stored = JsonSerializer.Deserialize<TwDataTableState>((string)save.Arguments[1]!, _jsonOptions);
        Assert.Equal("name", stored?.SortColumn);
    }

    [Fact]
    public void StateStorageKey_IgnoresSavedStateThatIsNotValidJson()
    {
        // Arrange
        TestContext.JSInterop.Setup<string?>("twDataTable.loadState", "products-table").SetResult("{not json");

        // Act
        var cut = Render(p => p.Add(x => x.StateStorageKey, "products-table").Add(x => x.EnablePagination, false));

        // Assert
        Assert.Equal("Kettle", FirstCells(cut)[0]);
    }

    [Fact]
    public void StateStorageKey_IsIgnored_WhileTheStateParameterIsSet()
    {
        // Arrange & Act
        var cut = Render(p => p
            .Add(x => x.StateStorageKey, "products-table")
            .Add(x => x.State, new TwDataTableState()));
        cut.Find("[aria-label='Sort by Name']").Click();

        // Assert
        Assert.DoesNotContain(TestContext.JSInterop.Invocations, i => i.Identifier.StartsWith("twDataTable.", StringComparison.Ordinal) && i.Identifier != "twDataTable.focus");
    }

    public record Product(string Name, string Category, int Price);
}
