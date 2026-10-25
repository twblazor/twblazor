using Bunit;
using TwBlazor.Components;
using TwBlazor.Configuration.Components;
using TwBlazor.Enums;

namespace TwBlazor.Tests.Components.Grid;

/// <summary>
/// Tests <see cref="TwGrid"/>: column and row counts at each breakpoint, auto fitting columns, templates, areas,
/// subgrid, gaps, auto flow and alignment.
/// </summary>
public class TwGridTests : TwBlazorTestBase
{
    private TwGridTheme theme => Theme.Components.Require<TwGridTheme>();

    private AngleSharp.Dom.IElement Render(Action<ComponentParameterCollectionBuilder<TwGrid>>? configure = null) =>
        TestContext.Render<TwGrid>(p => configure?.Invoke(p)).Find("[id]");

    [Fact]
    public void ByDefault_RendersAGridDiv_WithNoStyle()
    {
        var element = Render();

        Assert.Equal("DIV", element.TagName);
        Assert.Equal([theme.Base], element.ClassList);
        Assert.Null(element.GetAttribute("style"));
        Assert.Null(element.GetAttribute("role"));
    }

    [Fact]
    public void Inline_RendersAnInlineGrid()
    {
        var element = Render(p => p.Add(x => x.Inline, true));

        Assert.Contains(theme.Inline, element.ClassList);
        Assert.DoesNotContain(theme.Base, element.ClassList);
    }

    [Fact]
    public void Columns_AndTheirBreakpointVariants_AddTheirPrefixedClasses()
    {
        var element = Render(p => p
            .Add(x => x.Columns, 1)
            .Add(x => x.SmColumns, 2)
            .Add(x => x.MdColumns, 3)
            .Add(x => x.LgColumns, 4)
            .Add(x => x.XlColumns, 6)
            .Add(x => x.Xl2Columns, 12));

        Assert.Contains("grid-cols-1", element.ClassList);
        Assert.Contains("sm:grid-cols-2", element.ClassList);
        Assert.Contains("md:grid-cols-3", element.ClassList);
        Assert.Contains("lg:grid-cols-4", element.ClassList);
        Assert.Contains("xl:grid-cols-6", element.ClassList);
        Assert.Contains("2xl:grid-cols-12", element.ClassList);
    }

    [Fact]
    public void Rows_AndTheirBreakpointVariants_AddTheirPrefixedClasses()
    {
        var element = Render(p => p
            .Add(x => x.Rows, 2)
            .Add(x => x.SmRows, 3)
            .Add(x => x.MdRows, 4)
            .Add(x => x.LgRows, 5)
            .Add(x => x.XlRows, 6)
            .Add(x => x.Xl2Rows, 1));

        Assert.Contains("grid-rows-2", element.ClassList);
        Assert.Contains("sm:grid-rows-3", element.ClassList);
        Assert.Contains("md:grid-rows-4", element.ClassList);
        Assert.Contains("lg:grid-rows-5", element.ClassList);
        Assert.Contains("xl:grid-rows-6", element.ClassList);
        Assert.Contains("2xl:grid-rows-1", element.ClassList);
    }

    [Fact]
    public void CountsOffTheScale_AddNoClasses()
    {
        var element = Render(p => p.Add(x => x.Columns, 13).Add(x => x.Rows, 7));

        Assert.Equal([theme.Base], element.ClassList);
    }

    [Fact]
    public void MinColumnWidth_FitsColumnsToTheSpace_InsteadOfTheColumnCounts()
    {
        var element = Render(p => p.Add(x => x.MinColumnWidth, "14rem").Add(x => x.Columns, 3).Add(x => x.MdColumns, 2));

        Assert.Contains(theme.AutoFit, element.ClassList);
        Assert.DoesNotContain("grid-cols-3", element.ClassList);
        Assert.DoesNotContain("md:grid-cols-2", element.ClassList);
        Assert.Equal("--tw-grid-min: 14rem", element.GetAttribute("style"));
    }

    [Fact]
    public void AutoFill_KeepsEmptyColumns()
    {
        var element = Render(p => p.Add(x => x.MinColumnWidth, "10rem").Add(x => x.AutoFill, true));

        Assert.Contains(theme.AutoFill, element.ClassList);
        Assert.DoesNotContain(theme.AutoFit, element.ClassList);
    }

    [Fact]
    public void ColumnTemplate_SetsTheColumnsInline_AndWinsOverCountsAndMinColumnWidth()
    {
        var element = Render(p => p
            .Add(x => x.ColumnTemplate, "16rem 1fr")
            .Add(x => x.MinColumnWidth, "10rem")
            .Add(x => x.Columns, 3));

        Assert.Equal("grid-template-columns: 16rem 1fr", element.GetAttribute("style"));
        Assert.Equal([theme.Base], element.ClassList);
    }

    [Fact]
    public void RowTemplate_SetsTheRowsInline_AndWinsOverRowCounts()
    {
        var element = Render(p => p.Add(x => x.RowTemplate, "auto 1fr auto").Add(x => x.Rows, 3));

        Assert.Equal("grid-template-rows: auto 1fr auto", element.GetAttribute("style"));
        Assert.DoesNotContain("grid-rows-3", element.ClassList);
    }

    [Fact]
    public void Areas_NameTheCells_OneQuotedStringPerRow()
    {
        var element = Render(p => p.Add(x => x.Areas, ["header header", "sidebar main", " "]));

        Assert.Equal("grid-template-areas: \"header header\" \"sidebar main\"", element.GetAttribute("style"));
    }

    [Fact]
    public void InlineValues_CannotBreakOutOfTheirDeclaration()
    {
        var element = Render(p => p
            .Add(x => x.ColumnTemplate, "1fr; color: red")
            .Add(x => x.Areas, ["main\" } body { color: red"]));

        var style = element.GetAttribute("style")!;
        Assert.Equal(2, style.Split(';').Length);
        Assert.DoesNotContain('}', style);
        Assert.StartsWith("grid-template-columns: 1fr color: red; grid-template-areas: \"main  body  color: red\"", style);
    }

    [Fact]
    public void Style_IsAddedAfterTheGridsOwnDeclarations()
    {
        var element = Render(p => p.Add(x => x.ColumnTemplate, "1fr 1fr").Add(x => x.Style, "height: 10rem;"));

        Assert.Equal("grid-template-columns: 1fr 1fr; height: 10rem", element.GetAttribute("style"));
    }

    [Fact]
    public void Subgrid_SharesTheParentsTracks_InsteadOfItsOwn()
    {
        var element = Render(p => p
            .Add(x => x.SubgridColumns, true)
            .Add(x => x.SubgridRows, true)
            .Add(x => x.Columns, 3)
            .Add(x => x.Rows, 2)
            .Add(x => x.MinColumnWidth, "10rem"));

        Assert.Contains("grid-cols-subgrid", element.ClassList);
        Assert.Contains("grid-rows-subgrid", element.ClassList);
        Assert.DoesNotContain("grid-cols-3", element.ClassList);
        Assert.DoesNotContain("grid-rows-2", element.ClassList);
        Assert.Null(element.GetAttribute("style"));
    }

    [Fact]
    public void Gaps_UseTheSpacingSteps()
    {
        var element = Render(p => p.Add(x => x.Gap, 6).Add(x => x.ColumnGap, 4).Add(x => x.RowGap, 8));

        Assert.Contains("gap-6", element.ClassList);
        Assert.Contains("gap-x-4", element.ClassList);
        Assert.Contains("gap-y-8", element.ClassList);
    }

    [Theory]
    [InlineData(GridAutoFlow.Row, "grid-flow-row")]
    [InlineData(GridAutoFlow.Column, "grid-flow-col")]
    [InlineData(GridAutoFlow.Dense, "grid-flow-dense")]
    [InlineData(GridAutoFlow.RowDense, "grid-flow-row-dense")]
    [InlineData(GridAutoFlow.ColumnDense, "grid-flow-col-dense")]
    public void AutoFlow_AddsItsClass(GridAutoFlow flow, string expected)
    {
        Assert.Contains(expected, Render(p => p.Add(x => x.AutoFlow, flow)).ClassList);
    }

    [Theory]
    [InlineData(GridAutoSize.Auto, "auto-cols-auto", "auto-rows-auto")]
    [InlineData(GridAutoSize.Min, "auto-cols-min", "auto-rows-min")]
    [InlineData(GridAutoSize.Max, "auto-cols-max", "auto-rows-max")]
    [InlineData(GridAutoSize.Fr, "auto-cols-fr", "auto-rows-fr")]
    public void AutoColumns_AndAutoRows_SizeImplicitTracks(GridAutoSize size, string columns, string rows)
    {
        var element = Render(p => p.Add(x => x.AutoColumns, size).Add(x => x.AutoRows, size));

        Assert.Contains(columns, element.ClassList);
        Assert.Contains(rows, element.ClassList);
    }

    [Theory]
    [InlineData(JustifyItems.Start, "justify-items-start")]
    [InlineData(JustifyItems.Center, "justify-items-center")]
    [InlineData(JustifyItems.End, "justify-items-end")]
    [InlineData(JustifyItems.Stretch, "justify-items-stretch")]
    public void JustifyItems_AddsItsClass(JustifyItems justifyItems, string expected)
    {
        Assert.Contains(expected, Render(p => p.Add(x => x.JustifyItems, justifyItems)).ClassList);
    }

    [Fact]
    public void AlignItems_JustifyContent_AndAlignContent_AddTheirClasses()
    {
        var element = Render(p => p
            .Add(x => x.AlignItems, AlignItems.Center)
            .Add(x => x.JustifyContent, JustifyContent.Between)
            .Add(x => x.AlignContent, AlignContent.End));

        Assert.Contains("items-center", element.ClassList);
        Assert.Contains("justify-between", element.ClassList);
        Assert.Contains("content-end", element.ClassList);
    }

    [Fact]
    public void Element_RendersADescriptionList_WithoutAListRole()
    {
        var element = Render(p => p.Add(x => x.Element, LayoutElement.Dl));

        Assert.Equal("DL", element.TagName);
        Assert.Null(element.GetAttribute("role"));
    }

    [Fact]
    public void Class_IsMergedLast_SoItWinsOverTheParameters()
    {
        var element = Render(p => p.Add(x => x.Columns, 2).Add(x => x.Class, "grid-cols-5"));

        Assert.Contains("grid-cols-5", element.ClassList);
        Assert.DoesNotContain("grid-cols-2", element.ClassList);
    }
}
