using AngleSharp.Dom;
using Bunit;
using Microsoft.AspNetCore.Components;
using TwBlazor.Components;
using TwBlazor.Enums;

namespace TwBlazor.Tests.Components.DataTable;

public class TwDataTableGroupingTests : TwBlazorTestBase
{
    public TwDataTableGroupingTests()
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
        new() { Name = "category", Title = "Category", PropertySelector = p => p.Category, IsSortable = true, Groupable = true },
        new() { Name = "price", Title = "Price", PropertySelector = p => p.Price, IsSortable = true, Groupable = true, CellFormatter = p => p.Price >= 100 ? "Expensive" : "Cheap" }
    ];

    private IRenderedComponent<TwDataTable<Product>> Render(
        Action<ComponentParameterCollectionBuilder<TwDataTable<Product>>>? configure = null,
        List<Product>? items = null,
        List<TwDataTableColumn<Product>>? columns = null,
        bool pageable = false) =>
        TestContext.Render<TwDataTable<Product>>(parameters =>
        {
            parameters
                .Add(p => p.Id, "products")
                .Add(p => p.Items, items ?? Products())
                .Add(p => p.Columns, columns ?? Columns())
                .Add(p => p.Pageable, pageable);
            configure?.Invoke(parameters);
        });

    private static List<string> GroupLabels(IRenderedComponent<TwDataTable<Product>> cut) =>
        [.. cut.FindAll("tbody th[scope='rowgroup'] button span:first-of-type").Select(s => s.TextContent.Trim())];

    private static List<string> RowNames(IElement body) =>
        [.. body.QuerySelectorAll("tr").Skip(1).Select(r => r.Children[0].TextContent.Trim())];

    private static string Status(IRenderedComponent<TwDataTable<Product>> cut) =>
        cut.Find("[role='status']").TextContent.Trim();

    [Fact]
    public void GroupBySelect_IsHidden_WhenNoColumnIsGroupable()
    {
        // Arrange
        var columns = Columns();
        columns.ForEach(c => c.Groupable = false);

        // Act
        var cut = Render(columns: columns);

        // Assert
        Assert.Empty(cut.FindAll("#products-group-by"));
    }

    [Fact]
    public void GroupBySelect_ListsTheGroupableColumns_AfterNoGrouping()
    {
        // Act
        var cut = Render();

        // Assert
        var options = cut.FindAll("#products-group-by option").Select(o => o.TextContent.Trim()).ToList();
        Assert.Equal(["No grouping", "Category", "Price"], options);
    }

    [Fact]
    public void GroupBy_RendersAHeaderedBodyPerGroup_InAscendingOrder()
    {
        // Act
        var cut = Render(p => p.Add(x => x.GroupBy, "category"));

        // Assert
        var bodies = cut.FindAll("tbody");
        Assert.Equal(2, bodies.Count);
        Assert.Equal(["Furniture", "Kitchen"], GroupLabels(cut));
        Assert.Equal(["Armchair", "Desk"], RowNames(bodies[0]));
        Assert.Equal(["Kettle", "Blender", "Toaster"], RowNames(bodies[1]));
    }

    [Fact]
    public void GroupHeader_SpansTheVisibleColumns_AndHeadsItsRowGroup()
    {
        // Act
        var cut = Render(p => p.Add(x => x.GroupBy, "category"));

        // Assert
        var header = cut.Find("tbody th");
        Assert.Equal("rowgroup", header.GetAttribute("scope"));
        Assert.Equal("3", header.GetAttribute("colspan"));
        Assert.Contains("2 rows", header.TextContent);
    }

    [Fact]
    public void GroupHeader_CountsASingleRowInTheSingular()
    {
        // Act
        var cut = Render(p => p.Add(x => x.GroupBy, "category"), items: [new("Lamp", "Lighting", 40)]);

        // Assert
        Assert.Contains("1 row", cut.Find("tbody th").TextContent);
    }

    [Fact]
    public void GroupBy_KeepsTheTableSortInsideEachGroup()
    {
        // Arrange
        var cut = Render(p => p.Add(x => x.GroupBy, "category"));

        // Act
        cut.Find("[aria-label='Sort by Name']").Click();

        // Assert
        var bodies = cut.FindAll("tbody");
        Assert.Equal(["Blender", "Kettle", "Toaster"], RowNames(bodies[1]));
    }

    [Fact]
    public void SortingTheGroupedColumnDescending_ReversesTheGroups()
    {
        // Arrange
        var cut = Render(p => p.Add(x => x.GroupBy, "category"));

        // Act
        cut.Find("[aria-label='Sort by Category']").Click();
        cut.Find("[aria-label='Sort by Category, currently ascending']").Click();

        // Assert
        Assert.Equal(["Kitchen", "Furniture"], GroupLabels(cut));
    }

    [Fact]
    public void GroupLabels_UseTheCellFormatter()
    {
        // Act - grouping by price keys on the number, so each price is its own group, labelled by the formatter.
        var cut = Render(p => p.Add(x => x.GroupBy, "price"));

        // Assert
        Assert.Equal(["Cheap", "Cheap", "Cheap", "Expensive", "Expensive"], GroupLabels(cut));
    }

    [Fact]
    public void RowsWithoutAValue_FormTheFirstGroup()
    {
        // Act
        var cut = Render(p => p.Add(x => x.GroupBy, "category"), items: [new("Kettle", "Kitchen", 30), new("Mystery", null!, 5)]);

        // Assert
        Assert.Equal(["(No value)", "Kitchen"], GroupLabels(cut));
    }

    [Fact]
    public void GroupBy_IgnoresAColumnThatIsNotGroupable()
    {
        // Act
        var cut = Render(p => p.Add(x => x.GroupBy, "name"));

        // Assert
        Assert.Empty(cut.FindAll("th[scope='rowgroup']"));
        Assert.Single(cut.FindAll("tbody"));
    }

    [Fact]
    public void ChoosingAColumnInTheGroupBySelect_GroupsTheRows_AndAnnouncesIt()
    {
        // Arrange
        var cut = Render();

        // Act
        cut.Find("#products-group-by").Change("2");

        // Assert
        Assert.Equal(["Furniture", "Kitchen"], GroupLabels(cut));
        Assert.Equal("Grouped by Category", Status(cut));
    }

    [Fact]
    public void ChoosingNoGrouping_RemovesTheGroups()
    {
        // Arrange
        var cut = Render(p => p.Add(x => x.GroupBy, "category"));

        // Act
        cut.Find("#products-group-by").Change("1");

        // Assert
        Assert.Empty(cut.FindAll("th[scope='rowgroup']"));
        Assert.Equal(5, cut.FindAll("tbody tr").Count);
        Assert.Equal("Grouping removed", Status(cut));
    }

    [Fact]
    public void GroupToggle_CollapsesAndExpandsTheGroup()
    {
        // Arrange
        var cut = Render(p => p.Add(x => x.GroupBy, "category"));
        var toggle = cut.Find("tbody th button");
        Assert.Equal("true", toggle.GetAttribute("aria-expanded"));

        // Act
        toggle.Click();

        // Assert
        var collapsed = cut.FindAll("tbody")[0];
        Assert.Equal("false", collapsed.QuerySelector("button")!.GetAttribute("aria-expanded"));
        Assert.Empty(RowNames(collapsed));
        Assert.Equal("Furniture collapsed", Status(cut));

        // Act
        cut.Find("tbody th button").Click();

        // Assert
        Assert.Equal(["Armchair", "Desk"], RowNames(cut.FindAll("tbody")[0]));
        Assert.Equal("Furniture expanded", Status(cut));
    }

    [Fact]
    public void CollapseAndExpandAll_ChangeEveryGroup()
    {
        // Arrange
        var cut = Render(p => p.Add(x => x.GroupBy, "category"));

        // Act
        cut.Find("#products-collapse-groups").Click();

        // Assert
        Assert.All(cut.FindAll("tbody th button"), b => Assert.Equal("false", b.GetAttribute("aria-expanded")));
        Assert.Equal("All groups collapsed", Status(cut));

        // Act
        cut.Find("#products-expand-groups").Click();

        // Assert
        Assert.All(cut.FindAll("tbody th button"), b => Assert.Equal("true", b.GetAttribute("aria-expanded")));
        Assert.Equal("All groups expanded", Status(cut));
    }

    [Fact]
    public void ExpandAndCollapseAllButtons_OnlyShowWhileGrouped()
    {
        // Act
        var cut = Render();

        // Assert
        Assert.Empty(cut.FindAll("#products-expand-groups, #products-collapse-groups"));
    }

    [Fact]
    public void Paging_CountsGroupHeaders_AndRepeatsTheHeaderOfAGroupThatContinues()
    {
        // Arrange - the layout is Furniture, Armchair, Desk, Kitchen, Kettle, Blender, Toaster.
        var cut = Render(p => p.Add(x => x.GroupBy, "category").Add(x => x.RowsPerPage, 3), pageable: true);

        // Act
        cut.Find("nav[aria-label='Table pagination'] button[aria-label='Next page']").Click();
        cut.Find("nav[aria-label='Table pagination'] button[aria-label='Next page']").Click();

        // Assert - page 3 holds only Toaster, under a repeated Kitchen header.
        Assert.Equal(["Kitchen"], GroupLabels(cut));
        Assert.Equal(["Toaster"], RowNames(cut.Find("tbody")));
        Assert.Contains("3 rows", cut.Find("tbody th").TextContent);
    }

    [Fact]
    public void AllGroupsCollapsed_StillShowsTheGroupHeaders()
    {
        // Act
        var cut = Render(p => p.Add(x => x.State, new TwDataTableState { GroupBy = "category", CollapsedGroups = ["Furniture", "Kitchen"] }));

        // Assert
        Assert.Equal(["Furniture", "Kitchen"], GroupLabels(cut));
        Assert.DoesNotContain("No data available.", cut.Markup);
    }

    [Fact]
    public void NoRows_ShowsTheEmptyMessage_WhileGrouped()
    {
        // Act
        var cut = Render(p => p.Add(x => x.GroupBy, "category"), items: []);

        // Assert
        Assert.Equal("No data available.", cut.Find("tbody td").TextContent.Trim());
    }

    [Fact]
    public void GroupHeaderTemplate_ShowsSummariesForTheGroup()
    {
        // Arrange
        RenderFragment<TwDataTableGroup<Product>> template = group => builder =>
            builder.AddContent(0, $"Total {group.Items.Sum(p => p.Price)}, key {group.Key}, expanded {group.Expanded}");

        // Act
        var cut = Render(p => p.Add(x => x.GroupBy, "category").Add(x => x.GroupHeaderTemplate, template));

        // Assert
        Assert.Contains("Total 430, key Furniture, expanded True", cut.FindAll("tbody th")[0].TextContent);
    }

    [Fact]
    public void State_RecordsAndRestoresTheGrouping()
    {
        // Arrange
        TwDataTableState? raised = null;
        var cut = Render(p => p.Add(x => x.GroupBy, "category").Add(x => x.StateChanged, (TwDataTableState s) => raised = s));

        // Act
        cut.Find("tbody th button").Click();

        // Assert
        Assert.Equal("category", raised?.GroupBy);
        Assert.Equal(["Furniture"], raised?.CollapsedGroups);

        // Act - a fresh table restores it.
        var restored = Render(p => p.Add(x => x.State, raised));

        // Assert
        Assert.Equal("false", restored.Find("tbody th button").GetAttribute("aria-expanded"));
    }

    [Fact]
    public async Task ResetStateAsync_ReturnsToTheGroupByParameter()
    {
        // Arrange
        var cut = Render(p => p.Add(x => x.GroupBy, "category"));
        cut.Find("#products-group-by").Change("1");

        // Act
        await cut.InvokeAsync(() => cut.Instance.ResetStateAsync());

        // Assert
        Assert.Equal(["Furniture", "Kitchen"], GroupLabels(cut));
    }

    [Fact]
    public void ChangingTheGroupByParameter_RegroupsTheRows()
    {
        // Arrange
        var cut = Render(p => p.Add(x => x.GroupBy, "category"));

        // Act
        cut.Render(p => p.Add(x => x.GroupBy, null));

        // Assert
        Assert.Empty(cut.FindAll("th[scope='rowgroup']"));
    }

    [Fact]
    public void GroupKeysThatCannotBeCompared_AreOrderedByTheirText()
    {
        // Arrange - a mix of numbers and text cannot be compared directly.
        List<TwDataTableColumn<Product>> columns =
        [
            new() { Name = "name", Title = "Name", PropertySelector = p => p.Name },
            new() { Name = "mixed", Title = "Mixed", Groupable = true, PropertySelector = p => p.Price > 100 ? "large" : (object)p.Price }
        ];

        // Act
        var cut = Render(p => p.Add(x => x.GroupBy, "mixed"), columns: columns);

        // Assert
        Assert.Contains("large", GroupLabels(cut));
        Assert.Equal(4, GroupLabels(cut).Count);
    }

    public record Product(string Name, string Category, int Price);
}
