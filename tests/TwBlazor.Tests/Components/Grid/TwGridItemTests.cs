using Bunit;
using TwBlazor.Components;
using TwBlazor.Configuration.Components;
using TwBlazor.Enums;

namespace TwBlazor.Tests.Components.Grid;

/// <summary>
/// Tests <see cref="TwGridItem"/>: spans and start lines at each breakpoint, named areas, self alignment and order.
/// </summary>
public class TwGridItemTests : TwBlazorTestBase
{
    private TwGridTheme theme => Theme.Components.Require<TwGridTheme>();

    private AngleSharp.Dom.IElement Render(Action<ComponentParameterCollectionBuilder<TwGridItem>>? configure = null) =>
        TestContext.Render<TwGridItem>(p => configure?.Invoke(p)).Find("[id]");

    [Fact]
    public void ByDefault_RendersADiv_WithTheItemClasses()
    {
        var element = Render();

        Assert.Equal("DIV", element.TagName);
        Assert.Equal([theme.Item], element.ClassList);
        Assert.Null(element.GetAttribute("style"));
    }

    [Fact]
    public void ColumnSpan_AndItsBreakpointVariants_AddTheirPrefixedClasses()
    {
        var element = Render(p => p
            .Add(x => x.ColumnSpan, GridSpan.Full)
            .Add(x => x.SmColumnSpan, 1)
            .Add(x => x.MdColumnSpan, 2)
            .Add(x => x.LgColumnSpan, 3)
            .Add(x => x.XlColumnSpan, 6)
            .Add(x => x.Xl2ColumnSpan, 12));

        Assert.Contains("col-span-full", element.ClassList);
        Assert.Contains("sm:col-span-1", element.ClassList);
        Assert.Contains("md:col-span-2", element.ClassList);
        Assert.Contains("lg:col-span-3", element.ClassList);
        Assert.Contains("xl:col-span-6", element.ClassList);
        Assert.Contains("2xl:col-span-12", element.ClassList);
    }

    [Fact]
    public void RowSpan_AndItsBreakpointVariants_AddTheirPrefixedClasses()
    {
        var element = Render(p => p
            .Add(x => x.RowSpan, 2)
            .Add(x => x.SmRowSpan, 3)
            .Add(x => x.MdRowSpan, GridSpan.Full)
            .Add(x => x.LgRowSpan, 4)
            .Add(x => x.XlRowSpan, 5)
            .Add(x => x.Xl2RowSpan, 6));

        Assert.Contains("row-span-2", element.ClassList);
        Assert.Contains("sm:row-span-3", element.ClassList);
        Assert.Contains("md:row-span-full", element.ClassList);
        Assert.Contains("lg:row-span-4", element.ClassList);
        Assert.Contains("xl:row-span-5", element.ClassList);
        Assert.Contains("2xl:row-span-6", element.ClassList);
    }

    [Fact]
    public void ColumnStart_AndItsBreakpointVariants_AddTheirPrefixedClasses()
    {
        var element = Render(p => p
            .Add(x => x.ColumnStart, 1)
            .Add(x => x.SmColumnStart, 2)
            .Add(x => x.MdColumnStart, 3)
            .Add(x => x.LgColumnStart, 4)
            .Add(x => x.XlColumnStart, 5)
            .Add(x => x.Xl2ColumnStart, 13));

        Assert.Contains("col-start-1", element.ClassList);
        Assert.Contains("sm:col-start-2", element.ClassList);
        Assert.Contains("md:col-start-3", element.ClassList);
        Assert.Contains("lg:col-start-4", element.ClassList);
        Assert.Contains("xl:col-start-5", element.ClassList);
        Assert.Contains("2xl:col-start-13", element.ClassList);
    }

    [Fact]
    public void RowStart_AndItsBreakpointVariants_AddTheirPrefixedClasses()
    {
        var element = Render(p => p
            .Add(x => x.RowStart, 1)
            .Add(x => x.SmRowStart, 2)
            .Add(x => x.MdRowStart, 3)
            .Add(x => x.LgRowStart, 4)
            .Add(x => x.XlRowStart, 5)
            .Add(x => x.Xl2RowStart, 7));

        Assert.Contains("row-start-1", element.ClassList);
        Assert.Contains("sm:row-start-2", element.ClassList);
        Assert.Contains("md:row-start-3", element.ClassList);
        Assert.Contains("lg:row-start-4", element.ClassList);
        Assert.Contains("xl:row-start-5", element.ClassList);
        Assert.Contains("2xl:row-start-7", element.ClassList);
    }

    [Fact]
    public void ValuesOffTheScale_AddNoClasses()
    {
        var element = Render(p => p
            .Add(x => x.ColumnSpan, 13)
            .Add(x => x.RowSpan, 7)
            .Add(x => x.ColumnStart, 14)
            .Add(x => x.RowStart, 0));

        Assert.Equal([theme.Item], element.ClassList);
    }

    [Fact]
    public void Area_PlacesTheItemInANamedArea()
    {
        var element = Render(p => p.Add(x => x.Area, "sidebar").Add(x => x.Style, "padding: 1rem"));

        Assert.Equal("grid-area: sidebar; padding: 1rem", element.GetAttribute("style"));
    }

    [Theory]
    [InlineData(JustifySelf.Start, "justify-self-start")]
    [InlineData(JustifySelf.Center, "justify-self-center")]
    [InlineData(JustifySelf.End, "justify-self-end")]
    [InlineData(JustifySelf.Stretch, "justify-self-stretch")]
    public void JustifySelf_AddsItsClass(JustifySelf justifySelf, string expected)
    {
        Assert.Contains(expected, Render(p => p.Add(x => x.JustifySelf, justifySelf)).ClassList);
    }

    [Fact]
    public void AlignSelf_AndOrder_AddTheirClasses()
    {
        var element = Render(p => p.Add(x => x.AlignSelf, AlignSelf.End).Add(x => x.Order, ItemOrder.Last));

        Assert.Contains("self-end", element.ClassList);
        Assert.Contains("order-last", element.ClassList);
    }

    [Fact]
    public void InsideAGrid_RendersAsItsChild()
    {
        var cut = TestContext.Render<TwGrid>(p => p
            .Add(x => x.Element, LayoutElement.Ul)
            .Add(x => x.Columns, 2)
            .AddChildContent<TwGridItem>(item => item.Add(x => x.Element, LayoutElement.Li).Add(x => x.ColumnSpan, 2)));

        var item = cut.Find("ul > li");
        Assert.Contains("col-span-2", item.ClassList);
    }
}
