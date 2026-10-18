using Bunit;
using TwBlazor.Components;
using TwBlazor.Configuration.Components;

namespace TwBlazor.Tests.Components.DataTable;

public class TwDataTableColumnPanelTests : TwBlazorTestBase
{
    public TwDataTableColumnPanelTests()
    {
        TestContext.JSInterop.Setup<bool>("twDevice.prefersNativePicker").SetResult(true);
    }

    private TwTableTheme tableTheme => Theme.Components.Require<TwTableTheme>();

    private static readonly List<Product> _products =
    [
        new("Kettle", "Kitchen", 30),
        new("Armchair", "Furniture", 250)
    ];

    private static List<TwDataTableColumn<Product>> Columns() =>
    [
        new() { Name = "name", Title = "Name", PropertySelector = p => p.Name, Hideable = false },
        new() { Name = "category", Title = "Category", PropertySelector = p => p.Category },
        new() { Name = "price", Title = "Price", PropertySelector = p => p.Price, Width = "120px" }
    ];

    private IRenderedComponent<TwDataTable<Product>> Render(
        Action<ComponentParameterCollectionBuilder<TwDataTable<Product>>>? configure = null,
        List<TwDataTableColumn<Product>>? columns = null,
        bool withColumns = true,
        bool columnPanel = true) =>
        TestContext.Render<TwDataTable<Product>>(parameters =>
        {
            parameters
                .Add(p => p.Id, "products")
                .Add(p => p.Items, _products)
                .Add(p => p.Columns, withColumns ? columns ?? Columns() : null)
                .Add(p => p.ColumnPanel, columnPanel);
            configure?.Invoke(parameters);
        });

    private static List<string> Headers(IRenderedComponent<TwDataTable<Product>> cut) =>
        [.. cut.FindAll("thead th").Select(th => th.QuerySelector("span")!.TextContent.Trim())];

    private static string Status(IRenderedComponent<TwDataTable<Product>> cut) =>
        cut.Find("[role='status']").TextContent.Trim();

    private static void OpenPanel(IRenderedComponent<TwDataTable<Product>> cut) => cut.Find("#products-columns-toggle").Click();

    [Fact]
    public void ColumnsButton_IsHidden_ByDefault()
    {
        // Arrange & Act
        var cut = Render(columnPanel: false);

        // Assert
        Assert.Empty(cut.FindAll("#products-columns-toggle"));
    }

    [Fact]
    public void ColumnsButton_IsHidden_WithoutColumnDefinitions()
    {
        // Arrange & Act
        var cut = Render(withColumns: false);

        // Assert
        Assert.Empty(cut.FindAll("#products-columns-toggle"));
    }

    [Fact]
    public void ColumnsButton_IsADisclosure_ForThePanel()
    {
        // Arrange
        var cut = Render();
        var toggle = cut.Find("#products-columns-toggle");
        Assert.Equal("false", toggle.GetAttribute("aria-expanded"));
        Assert.Equal("products-columns", toggle.GetAttribute("aria-controls"));
        Assert.Empty(cut.FindAll("#products-columns"));

        // Act
        OpenPanel(cut);

        // Assert
        Assert.Equal("true", cut.Find("#products-columns-toggle").GetAttribute("aria-expanded"));
        var panel = cut.Find("#products-columns");
        Assert.Equal("group", panel.GetAttribute("role"));
        Assert.Equal("Columns", cut.Find($"#{panel.GetAttribute("aria-labelledby")}").TextContent.Trim());
    }

    [Fact]
    public void ColumnPanelLabel_ReplacesTheButtonText()
    {
        // Arrange & Act
        var cut = Render(p => p.Add(x => x.ColumnPanelLabel, "Choose columns"));

        // Assert
        Assert.Contains("Choose columns", cut.Find("#products-columns-toggle").TextContent);
    }

    [Fact]
    public void Escape_ClosesThePanel_AndReturnsFocusToTheButton()
    {
        // Arrange
        var cut = Render();
        OpenPanel(cut);

        // Act
        cut.Find("#products-columns").KeyDown("Escape");

        // Assert
        Assert.Empty(cut.FindAll("#products-columns"));
        var focus = TestContext.JSInterop.Invocations.Last(i => i.Identifier == "twDataTable.focus");
        Assert.Equal("products-columns-toggle", focus.Arguments[0]);
    }

    [Fact]
    public void UncheckingAColumn_HidesItsHeaderAndCells_AndAnnouncesIt()
    {
        // Arrange
        var cut = Render();
        OpenPanel(cut);

        // Act
        cut.Find("#products-column-1-visible").Change(false);

        // Assert
        Assert.Equal(["Name", "Price"], Headers(cut));
        Assert.All(cut.FindAll("tbody tr"), row => Assert.Equal(2, row.Children.Length));
        Assert.Equal("Category column hidden", Status(cut));
    }

    [Fact]
    public void CheckingAHiddenColumn_ShowsIt()
    {
        // Arrange
        var columns = Columns();
        columns[1].Visible = false;
        var cut = Render(columns: columns);
        Assert.Equal(["Name", "Price"], Headers(cut));
        OpenPanel(cut);

        // Act
        cut.Find("#products-column-1-visible").Change(true);

        // Assert
        Assert.Equal(["Name", "Category", "Price"], Headers(cut));
        Assert.Equal("Category column shown", Status(cut));
    }

    [Fact]
    public void AColumnThatIsNotHideable_CannotBeUnchecked()
    {
        // Arrange
        var cut = Render();

        // Act
        OpenPanel(cut);

        // Assert
        Assert.True(cut.Find("#products-column-0-visible").HasAttribute("disabled"));
    }

    [Fact]
    public void TheLastVisibleColumn_CannotBeUnchecked()
    {
        // Arrange
        var columns = Columns();
        columns[0].Hideable = true;
        var cut = Render(columns: columns);
        OpenPanel(cut);
        cut.Find("#products-column-1-visible").Change(false);
        cut.Find("#products-column-2-visible").Change(false);

        // Assert
        Assert.Equal(["Name"], Headers(cut));
        Assert.True(cut.Find("#products-column-0-visible").HasAttribute("disabled"));
    }

    [Fact]
    public void MoveDown_ReordersTheColumns_AndAnnouncesTheNewPosition()
    {
        // Arrange
        var cut = Render();
        OpenPanel(cut);

        // Act
        cut.Find("#products-column-0-later").Click();

        // Assert
        Assert.Equal(["Category", "Name", "Price"], Headers(cut));
        Assert.Equal("Name moved to position 2 of 3", Status(cut));
        Assert.Equal("Kitchen", cut.Find("tbody tr td").TextContent.Trim());
    }

    [Fact]
    public void MoveButtons_AtTheEnds_AreDisabled()
    {
        // Arrange & Act
        var cut = Render();
        OpenPanel(cut);

        // Assert
        Assert.True(cut.Find("#products-column-0-earlier").HasAttribute("disabled"));
        Assert.True(cut.Find("#products-column-2-later").HasAttribute("disabled"));
        Assert.False(cut.Find("#products-column-1-earlier").HasAttribute("disabled"));
    }

    [Fact]
    public void MovingAColumnToTheEnd_MovesFocusToTheButtonThatCanStillMoveIt()
    {
        // Arrange
        var cut = Render();
        OpenPanel(cut);

        // Act - the Price column was already second to last after this move, so "down" can go no further.
        cut.Find("#products-column-1-later").Click();

        // Assert
        var focus = TestContext.JSInterop.Invocations.Last(i => i.Identifier == "twDataTable.focus");
        Assert.Equal("products-column-2-earlier", focus.Arguments[0]);
    }

    [Fact]
    public void MovingAColumnBetweenTheEnds_KeepsFocusOnTheSameButton()
    {
        // Arrange
        var cut = Render();
        OpenPanel(cut);

        // Act
        cut.Find("#products-column-2-earlier").Click();

        // Assert
        var focus = TestContext.JSInterop.Invocations.Last(i => i.Identifier == "twDataTable.focus");
        Assert.Equal("products-column-1-earlier", focus.Arguments[0]);
    }

    [Fact]
    public void ResetColumns_RestoresTheDefinitions()
    {
        // Arrange
        var cut = Render();
        OpenPanel(cut);
        cut.Find("#products-column-0-later").Click();
        cut.Find("#products-column-2-visible").Change(false);

        // Act
        cut.Find("#products-columns-reset").Click();

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
        OpenPanel(cut);

        // Act
        cut.Find("#products-column-1-visible").Change(false);

        // Assert
        Assert.False(raised?.Columns.Single(c => c.Name == "category").Visible);
    }

    [Fact]
    public void ResizableColumns_ShowsADragHandleThatIsHiddenFromAssistiveTechnology()
    {
        // Arrange & Act
        var cut = Render(p => p.Add(x => x.ResizableColumns, true));

        // Assert
        var handles = cut.FindAll("[data-resize-column]");
        Assert.Equal(3, handles.Count);
        Assert.All(handles, h =>
        {
            Assert.Equal("true", h.GetAttribute("aria-hidden"));
            Assert.Null(h.GetAttribute("tabindex"));
        });
        Assert.Contains(tableTheme.FixedLayout, cut.Find("table").GetAttribute("class"));
        Assert.Equal(3, cut.FindAll("colgroup col").Count);
    }

    [Fact]
    public void ResizableColumns_AttachesTheDragScript()
    {
        // Arrange & Act
        Render(p => p.Add(x => x.ResizableColumns, true));

        // Assert
        Assert.Contains(TestContext.JSInterop.Invocations, i => i.Identifier == "twDataTable.attachResize");
    }

    [Fact]
    public void ResizableColumns_ShowsTheColumnsButton_EvenWithoutTheColumnPanel()
    {
        // Arrange
        var cut = Render(p => p.Add(x => x.ResizableColumns, true), columnPanel: false);

        // Act
        OpenPanel(cut);

        // Assert - only the resize buttons, no visibility or ordering controls.
        Assert.Empty(cut.FindAll("#products-columns input[type='checkbox']"));
        Assert.Empty(cut.FindAll("#products-column-0-later"));
        Assert.NotEmpty(cut.FindAll("#products-column-0-wider"));
    }

    [Fact]
    public void WiderButton_AddsToAPixelWidth_AndAnnouncesTheNewWidth()
    {
        // Arrange
        var cut = Render(p => p.Add(x => x.ResizableColumns, true));
        OpenPanel(cut);

        // Act
        cut.Find("#products-column-2-wider").Click();

        // Assert
        Assert.Equal("width:152px;", cut.FindAll("colgroup col")[2].GetAttribute("style"));
        Assert.Equal("Price is 152 pixels wide", Status(cut));
    }

    [Fact]
    public void NarrowerButton_MeasuresAColumnWithoutAPixelWidth()
    {
        // Arrange
        TestContext.JSInterop.Setup<int>("twDataTable.measureColumn", _ => true).SetResult(200);
        var cut = Render(p => p.Add(x => x.ResizableColumns, true));
        OpenPanel(cut);

        // Act
        cut.Find("#products-column-1-narrower").Click();

        // Assert
        Assert.Equal("width:168px;", cut.FindAll("colgroup col")[1].GetAttribute("style"));
    }

    [Fact]
    public async Task CommitColumnWidth_KeepsTheWidthWithinItsLimits()
    {
        // Arrange
        var cut = Render(p => p.Add(x => x.ResizableColumns, true));

        // Act
        await cut.InvokeAsync(() => cut.Instance.CommitColumnWidth("price", 3));

        // Assert
        Assert.Equal($"width:{TwDataTable<Product>.MinimumColumnWidth}px;", cut.FindAll("colgroup col")[2].GetAttribute("style"));
    }

    [Fact]
    public async Task CommitColumnWidth_IgnoresAColumnThatIsNotResizable()
    {
        // Arrange
        var columns = Columns();
        columns[2].Resizable = false;
        var cut = Render(p => p.Add(x => x.ResizableColumns, true), columns: columns);

        // Act
        await cut.InvokeAsync(() => cut.Instance.CommitColumnWidth("price", 300));
        await cut.InvokeAsync(() => cut.Instance.CommitColumnWidth("missing", 300));

        // Assert
        Assert.Equal("width:120px;", cut.FindAll("colgroup col")[2].GetAttribute("style"));
        Assert.Equal(2, cut.FindAll("[data-resize-column]").Count);
    }

    [Fact]
    public async Task DisposeAsync_DetachesTheDragScript()
    {
        // Arrange
        var cut = Render(p => p.Add(x => x.ResizableColumns, true));

        // Act
        await cut.Instance.DisposeAsync();

        // Assert
        Assert.Contains(TestContext.JSInterop.Invocations, i => i.Identifier == "twDataTable.detachResize");
    }

    public record Product(string Name, string Category, int Price);
}
