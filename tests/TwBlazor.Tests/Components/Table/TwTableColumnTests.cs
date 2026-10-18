using Bunit;
using Microsoft.AspNetCore.Components;
using TwBlazor.Components;
using TwBlazor.Configuration.Components;

namespace TwBlazor.Tests.Components.Table;

public class TwTableColumnTests : TwBlazorTestBase
{
    private TwTableTheme tableTheme => Theme.Components.Require<TwTableTheme>();

    private static RenderFragment Columns(params (string? Width, int Span)[] columns) => builder =>
    {
        foreach (var (width, span) in columns)
        {
            builder.OpenComponent<TwTableColumn>(0);
            builder.AddComponentParameter(1, nameof(TwTableColumn.Width), width);
            builder.AddComponentParameter(2, nameof(TwTableColumn.Span), span);
            builder.CloseComponent();
        }
    };

    [Fact]
    public void TwTable_WithoutColumnGroup_RendersNoColgroup()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwTable>();

        // Assert
        Assert.Empty(cut.FindAll("colgroup"));
    }

    [Fact]
    public void TwTable_WithColumnGroup_RendersAColPerColumn()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwTable>(parameters => parameters
            .Add(p => p.ColumnGroup, Columns(("12rem", 1), (null, 1), ("6rem", 1))));

        // Assert
        var cols = cut.FindAll("table > colgroup > col");
        Assert.Equal(3, cols.Count);
        Assert.Equal("width:12rem;", cols[0].GetAttribute("style"));
        Assert.Null(cols[1].GetAttribute("style"));
        Assert.Equal("width:6rem;", cols[2].GetAttribute("style"));
    }

    [Fact]
    public void TwTable_ColumnGroup_RendersAfterCaptionAndBeforeHeader()
    {
        // Arrange - HTML requires the caption first, then any colgroup, then the table sections.
        RenderFragment caption = builder => builder.AddContent(0, "Products");
        RenderFragment header = builder =>
        {
            builder.OpenElement(0, "tr");
            builder.CloseElement();
        };

        // Act
        var cut = TestContext.Render<TwTable>(parameters => parameters
            .Add(p => p.Caption, caption)
            .Add(p => p.TableHeader, header)
            .Add(p => p.ColumnGroup, Columns(("10rem", 1))));

        // Assert
        var children = cut.Find("table").Children.Select(c => c.TagName.ToLowerInvariant()).ToList();
        Assert.Equal(["caption", "colgroup", "thead"], children);
    }

    [Fact]
    public void TwTableColumn_WithSpan_RendersSpanAttribute()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwTable>(parameters => parameters
            .Add(p => p.ColumnGroup, Columns(("8rem", 2), (null, 1))));

        // Assert
        var cols = cut.FindAll("col");
        Assert.Equal("2", cols[0].GetAttribute("span"));
        Assert.Null(cols[1].GetAttribute("span"));
    }

    [Fact]
    public void TwTableColumn_CombinesWidthWithStyleAndClass()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwTableColumn>(parameters => parameters
            .Add(p => p.Width, " 25% ")
            .Add(p => p.Style, "background-color:red")
            .Add(p => p.Class, "custom-col")
            .Add(p => p.Id, "first-col"));

        // Assert
        var col = cut.Find("col");
        Assert.Equal("width:25%;background-color:red", col.GetAttribute("style"));
        Assert.Equal("custom-col", col.GetAttribute("class"));
        Assert.Equal("first-col", col.GetAttribute("id"));
    }

    [Fact]
    public void TwTableColumn_WithoutWidthOrClass_RendersBareCol()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwTableColumn>();

        // Assert
        var col = cut.Find("col");
        Assert.Null(col.GetAttribute("style"));
        Assert.Null(col.GetAttribute("class"));
        Assert.Null(col.GetAttribute("span"));
    }

    [Fact]
    public void TwTable_FixedLayout_AddsFixedLayoutClass()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwTable>(parameters => parameters
            .Add(p => p.FixedLayout, true));

        // Assert
        Assert.Contains(tableTheme.FixedLayout, cut.Find("table").GetAttribute("class"));
    }

    [Fact]
    public void TwTable_DefaultLayout_IsAutomatic()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwTable>();

        // Assert
        Assert.DoesNotContain(tableTheme.FixedLayout, cut.Find("table").GetAttribute("class"));
    }
}
