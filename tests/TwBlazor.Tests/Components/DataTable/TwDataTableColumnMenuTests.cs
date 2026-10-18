using AngleSharp.Dom;
using Bunit;
using TwBlazor.Components;
using TwBlazor.Configuration.Components;
using TwBlazor.Enums;

namespace TwBlazor.Tests.Components.DataTable;

public class TwDataTableColumnMenuTests : TwBlazorTestBase
{
    public TwDataTableColumnMenuTests()
    {
        TestContext.JSInterop.Setup<bool>("twDevice.prefersNativePicker").SetResult(true);
    }

    private TwTableTheme tableTheme => Theme.Components.Require<TwTableTheme>();

    private static readonly List<Product> _products =
    [
        new("Kettle", "Kitchen", 30),
        new("Armchair", "Furniture", 250),
        new("Blender", "Kitchen", 60)
    ];

    private static List<TwDataTableColumn<Product>> Columns() =>
    [
        new() { Name = "name", Title = "Name", PropertySelector = p => p.Name, IsSortable = true, Hideable = false },
        new() { Name = "category", Title = "Category", PropertySelector = p => p.Category, Groupable = true },
        new() { Name = "price", Title = "Price", PropertySelector = p => p.Price, IsSortable = true }
    ];

    private IRenderedComponent<TwDataTable<Product>> Render(
        Action<ComponentParameterCollectionBuilder<TwDataTable<Product>>>? configure = null,
        bool columnMenu = true) =>
        TestContext.Render<TwDataTable<Product>>(parameters =>
        {
            parameters
                .Add(p => p.Id, "products")
                .Add(p => p.Items, _products)
                .Add(p => p.Columns, Columns())
                .Add(p => p.EnablePagination, false)
                .Add(p => p.ColumnMenu, columnMenu);
            configure?.Invoke(parameters);
        });

    private static List<string> Headers(IRenderedComponent<TwDataTable<Product>> cut) =>
        [.. cut.FindAll("thead th").Select(th => th.QuerySelector("span")!.TextContent.Trim())];

    private static List<string> FirstCells(IRenderedComponent<TwDataTable<Product>> cut) =>
        [.. cut.FindAll("tbody tr").Select(r => r.Children[0].TextContent.Trim())];

    private static string Status(IRenderedComponent<TwDataTable<Product>> cut) =>
        cut.Find("[role='status']").TextContent.Trim();

    private static void OpenMenu(IRenderedComponent<TwDataTable<Product>> cut, string column) =>
        cut.Find($"#products-col-{column}-menu-trigger").Click();

    private static IElement MenuItem(IRenderedComponent<TwDataTable<Product>> cut, string column, string label) =>
        cut.FindAll($"#products-col-{column}-menu [role^='menuitem']").Single(i => i.TextContent.Trim() == label);

    private static List<string> MenuItems(IRenderedComponent<TwDataTable<Product>> cut, string column) =>
        [.. cut.FindAll($"#products-col-{column}-menu-menu > [role='menuitem']").Select(i => i.TextContent.Trim())];

    [Fact]
    public void ColumnMenu_IsOff_ByDefault()
    {
        // Act
        var cut = Render(columnMenu: false);

        // Assert
        Assert.Empty(cut.FindAll("[aria-haspopup='menu']"));
    }

    [Fact]
    public void ColumnMenu_AddsAnOptionsButton_ToEveryColumn()
    {
        // Act
        var cut = Render();

        // Assert
        var trigger = cut.Find("#products-col-price-menu-trigger");
        Assert.Equal("Price column options", trigger.GetAttribute("aria-label"));
        Assert.Equal("menu", trigger.GetAttribute("aria-haspopup"));
        Assert.Equal(3, cut.FindAll("thead [aria-haspopup='menu']").Count);
    }

    [Fact]
    public void Menu_OffersTheActionsThatSuitTheColumn()
    {
        // Arrange
        var cut = Render();

        // Act
        OpenMenu(cut, "price");

        // Assert - price sorts but does not group.
        Assert.Equal(["Sort ascending", "Sort descending", "Move left", "Move right", "Hide column", "Reset columns"], MenuItems(cut, "price"));

        // Act
        OpenMenu(cut, "category");

        // Assert - category groups but does not sort.
        Assert.Equal(["Group by this column", "Move left", "Move right", "Hide column", "Reset columns"], MenuItems(cut, "category"));
    }

    [Fact]
    public void Menu_ListsEveryColumn_AsAToggle()
    {
        // Arrange
        var cut = Render();

        // Act
        OpenMenu(cut, "price");

        // Assert
        var group = cut.Find("#products-col-price-menu [role='group']");
        Assert.Contains("Columns", group.TextContent);
        var toggles = group.QuerySelectorAll("[role='menuitemcheckbox']");
        Assert.Equal(["Name", "Category", "Price"], toggles.Select(t => t.TextContent.Trim()));
        Assert.All(toggles, t => Assert.Equal("true", t.GetAttribute("aria-checked")));
        Assert.Equal("true", toggles[0].GetAttribute("aria-disabled"));
    }

    [Fact]
    public void SortAscending_SortsTheRows_AndClosesTheMenu()
    {
        // Arrange
        var cut = Render();
        OpenMenu(cut, "price");

        // Act
        MenuItem(cut, "price", "Sort descending").Click();

        // Assert
        Assert.Equal(["Armchair", "Blender", "Kettle"], FirstCells(cut));
        Assert.Equal("descending", cut.FindAll("thead th")[2].GetAttribute("aria-sort"));
        Assert.Equal("Sorted by Price, descending", Status(cut));
        Assert.Empty(cut.FindAll("#products-col-price-menu-menu"));
    }

    [Fact]
    public void RemoveSort_IsOffered_WhileTheColumnIsSorted()
    {
        // Arrange
        var cut = Render();
        OpenMenu(cut, "name");
        MenuItem(cut, "name", "Sort ascending").Click();
        OpenMenu(cut, "name");

        // Act
        MenuItem(cut, "name", "Remove sort").Click();

        // Assert
        Assert.Equal(["Kettle", "Armchair", "Blender"], FirstCells(cut));
        Assert.Equal("Sort by Name removed", Status(cut));
    }

    [Fact]
    public void GroupByThisColumn_GroupsTheRows_AndCanStop()
    {
        // Arrange
        var cut = Render();
        OpenMenu(cut, "category");

        // Act
        MenuItem(cut, "category", "Group by this column").Click();

        // Assert
        Assert.Equal(2, cut.FindAll("th[scope='rowgroup']").Count);

        // Act
        OpenMenu(cut, "category");
        MenuItem(cut, "category", "Stop grouping").Click();

        // Assert
        Assert.Empty(cut.FindAll("th[scope='rowgroup']"));
    }

    [Fact]
    public void MoveRight_ReordersTheColumns_AndTheFirstCannotMoveLeft()
    {
        // Arrange
        var cut = Render();
        OpenMenu(cut, "name");
        Assert.Equal("true", MenuItem(cut, "name", "Move left").GetAttribute("aria-disabled"));

        // Act
        MenuItem(cut, "name", "Move right").Click();

        // Assert
        Assert.Equal(["Category", "Name", "Price"], Headers(cut));
        Assert.Equal("Name moved to position 2 of 3", Status(cut));
    }

    [Fact]
    public void MoveLeft_MovesTheColumnLater_InARightToLeftLayout()
    {
        // Arrange
        var cut = Render(p => p.AddCascadingValue<TextDirection?>(TextDirection.RightToLeft));
        OpenMenu(cut, "name");

        // Act - in a right to left table the first column is on the right, so left moves it later.
        MenuItem(cut, "name", "Move left").Click();

        // Assert
        Assert.Equal(["Category", "Name", "Price"], Headers(cut));
    }

    [Fact]
    public void HideColumn_HidesIt_AndMovesFocusToTheNextColumnsMenu()
    {
        // Arrange
        var cut = Render();
        OpenMenu(cut, "category");

        // Act
        MenuItem(cut, "category", "Hide column").Click();

        // Assert
        Assert.Equal(["Name", "Price"], Headers(cut));
        Assert.Equal("Category column hidden", Status(cut));
        var focus = TestContext.JSInterop.Invocations.Last(i => i.Identifier == "twDataTable.focus");
        Assert.Equal("products-col-price-menu-trigger", focus.Arguments[0]);
    }

    [Fact]
    public void AColumnThatIsNotHideable_HasNoHideItem()
    {
        // Arrange
        var cut = Render();

        // Act
        OpenMenu(cut, "name");

        // Assert
        Assert.DoesNotContain("Hide column", MenuItems(cut, "name"));
    }

    [Fact]
    public void ColumnToggles_ShowAHiddenColumnAgain()
    {
        // Arrange
        var cut = Render();
        OpenMenu(cut, "price");
        cut.FindAll("#products-col-price-menu [role='menuitemcheckbox']")[1].Click();
        Assert.Equal(["Name", "Price"], Headers(cut));

        // Act
        cut.FindAll("#products-col-price-menu [role='menuitemcheckbox']")[1].Click();

        // Assert
        Assert.Equal(["Name", "Price", "Category"], Headers(cut).OrderBy(h => h == "Category").ToList());
        Assert.Equal("Category column shown", Status(cut));
    }

    [Fact]
    public void ResetColumns_PutsTheColumnsBack()
    {
        // Arrange
        var cut = Render();
        OpenMenu(cut, "name");
        MenuItem(cut, "name", "Move right").Click();
        OpenMenu(cut, "price");

        // Act
        MenuItem(cut, "price", "Reset columns").Click();

        // Assert
        Assert.Equal(["Name", "Category", "Price"], Headers(cut));
        Assert.Equal("Columns reset", Status(cut));
    }

    [Fact]
    public void ChangingTheColumns_RaisesStateChanged()
    {
        // Arrange
        TwDataTableState? raised = null;
        var cut = Render(p => p.Add(x => x.StateChanged, (TwDataTableState s) => raised = s));
        OpenMenu(cut, "price");

        // Act
        MenuItem(cut, "price", "Hide column").Click();

        // Assert
        Assert.False(raised?.Columns.Single(c => c.Name == "price").Visible);
    }

    [Fact]
    public void ResizableColumns_GivesTheTableResizableColumns_NamedAfterTheirTitles()
    {
        // Act
        var cut = Render(p => p.Add(x => x.ResizableColumns, true), columnMenu: false);

        // Assert
        var col = cut.Find("#products-col-price");
        Assert.Equal("true", col.GetAttribute("data-tw-resizable"));
        Assert.Equal("Price", col.GetAttribute("data-tw-label"));
        Assert.NotEmpty(cut.FindAll("[data-tw-resize-layer]"));
        Assert.Contains(tableTheme.FixedLayout, cut.Find("table").GetAttribute("class"));
    }

    [Fact]
    public void AColumnThatIsNotResizable_IsLeftUnmarked()
    {
        // Arrange
        var columns = Columns();
        columns[2].Resizable = false;

        // Act
        var cut = TestContext.Render<TwDataTable<Product>>(p => p
            .Add(x => x.Id, "products")
            .Add(x => x.Items, _products)
            .Add(x => x.Columns, columns)
            .Add(x => x.ResizableColumns, true));

        // Assert
        Assert.Null(cut.Find("#products-col-price").GetAttribute("data-tw-resizable"));
    }

    [Fact]
    public async Task ResizingAColumn_SavesItsWidth_InTheState()
    {
        // Arrange
        TwDataTableState? raised = null;
        var cut = Render(p => p.Add(x => x.ResizableColumns, true).Add(x => x.StateChanged, (TwDataTableState s) => raised = s), columnMenu: false);
        var table = cut.FindComponent<TwTable>();

        // Act
        await cut.InvokeAsync(() => table.Instance.CommitColumnWidths(new() { ["products-col-price"] = 180, ["products-col-name"] = 220 }));

        // Assert
        Assert.Equal("180px", raised?.Columns.Single(c => c.Name == "price").Width);
        Assert.Equal("220px", cut.Instance.GetState().Columns.Single(c => c.Name == "name").Width);
        Assert.Equal("width:180px;", cut.Find("#products-col-price").GetAttribute("style"));
    }

    [Fact]
    public void Toolbar_PutsTheSearchLast()
    {
        // Act
        var cut = Render(p => p
            .Add(x => x.Searchable, true)
            .Add(x => x.Exportable, true)
            .Add(x => x.GroupBy, "category"));

        // Assert
        var toolbar = cut.Find("#products-search").Closest("div.flex")!.ParentElement!.ParentElement!;
        Assert.NotNull(toolbar.LastElementChild!.QuerySelector("#products-search"));
        Assert.NotNull(toolbar.QuerySelector("#products-export-csv"));
        Assert.Contains(Theme.Spacing.PushEnd, cut.Find("#products-search").Closest($"div[class*='{tableTheme.SearchColumn.Split(' ')[0]}']")!.GetAttribute("class"));
    }

    public record Product(string Name, string Category, decimal Price);
}
