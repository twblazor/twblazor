using Bunit;
using Microsoft.AspNetCore.Components;
using TwBlazor.Components;
using TwBlazor.Configuration.Components;

namespace TwBlazor.Tests.Components.DataTable;

public class TwDataTableColumnWidthTests : TwBlazorTestBase
{
    public TwDataTableColumnWidthTests()
    {
        TestContext.JSInterop.Setup<bool>("twDevice.prefersNativePicker").SetResult(true);
    }

    private TwTableTheme tableTheme => Theme.Components.Require<TwTableTheme>();

    private static readonly List<Product> _products =
    [
        new("Keyboard", "Accessories"),
        new("Monitor", "Displays")
    ];

    [Fact]
    public void TwDataTable_ColumnsWithoutWidths_RendersNoColgroup()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwDataTable<Product>>(parameters => parameters
            .Add(p => p.Items, _products)
            .Add(p => p.Columns,
            [
                new() { Name = "name", Title = "Name", PropertySelector = p => p.Name },
                new() { Name = "category", Title = "Category", PropertySelector = p => p.Category }
            ]));

        // Assert
        Assert.Empty(cut.FindAll("colgroup"));
    }

    [Fact]
    public void TwDataTable_ColumnWidth_RendersAColForEveryColumn()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwDataTable<Product>>(parameters => parameters
            .Add(p => p.Items, _products)
            .Add(p => p.Columns,
            [
                new() { Name = "name", Title = "Name", PropertySelector = p => p.Name, Width = "14rem" },
                new() { Name = "category", Title = "Category", PropertySelector = p => p.Category }
            ]));

        // Assert - one col per column keeps the widths lined up with the right header cells.
        var cols = cut.FindAll("table > colgroup > col");
        Assert.Equal(2, cols.Count);
        Assert.Equal("width:14rem;", cols[0].GetAttribute("style"));
        Assert.Null(cols[1].GetAttribute("style"));
    }

    [Fact]
    public void TwDataTable_ManualColumnGroup_IsPassedToTheTable()
    {
        // Arrange
        RenderFragment columnGroup = builder =>
        {
            builder.OpenComponent<TwTableColumn>(0);
            builder.AddComponentParameter(1, nameof(TwTableColumn.Width), "9rem");
            builder.CloseComponent();
        };
        RenderFragment<Product> row = product => builder =>
        {
            builder.OpenElement(0, "td");
            builder.AddContent(1, product.Name);
            builder.CloseElement();
        };

        // Act
        var cut = TestContext.Render<TwDataTable<Product>>(parameters => parameters
            .Add(p => p.Items, _products)
            .Add(p => p.TableRow, row)
            .Add(p => p.ColumnGroup, columnGroup));

        // Assert
        Assert.Equal("width:9rem;", cut.Find("colgroup > col").GetAttribute("style"));
    }

    [Fact]
    public void TwDataTable_FixedLayout_IsPassedToTheTable()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwDataTable<Product>>(parameters => parameters
            .Add(p => p.Items, _products)
            .Add(p => p.FixedLayout, true)
            .Add(p => p.Columns,
            [
                new() { Name = "name", Title = "Name", PropertySelector = p => p.Name }
            ]));

        // Assert
        Assert.Contains(tableTheme.FixedLayout, cut.Find("table").GetAttribute("class"));
    }

    public record Product(string Name, string Category);
}
