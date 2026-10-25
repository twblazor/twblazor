using Bunit;
using TwBlazor.Components;
using TwBlazor.Configuration.Components;
using TwBlazor.Enums;

namespace TwBlazor.Tests.Components.Flex;

/// <summary>
/// Tests <see cref="TwFlex"/>: direction at each breakpoint, wrapping, alignment, gaps, sizing and dividers.
/// </summary>
public class TwFlexTests : TwBlazorTestBase
{
    private TwFlexTheme theme => Theme.Components.Require<TwFlexTheme>();

    private AngleSharp.Dom.IElement Render(Action<ComponentParameterCollectionBuilder<TwFlex>>? configure = null) =>
        TestContext.Render<TwFlex>(p => configure?.Invoke(p)).Find("[id]");

    [Fact]
    public void ByDefault_RendersAFlexDiv_WithNothingElseSet()
    {
        var element = Render();

        Assert.Equal("DIV", element.TagName);
        Assert.Equal([theme.Base], element.ClassList);
    }

    [Fact]
    public void Inline_RendersAnInlineFlexLayout()
    {
        var element = Render(p => p.Add(x => x.Inline, true));

        Assert.Contains(theme.Inline, element.ClassList);
        Assert.DoesNotContain(theme.Base, element.ClassList);
    }

    [Fact]
    public void Direction_AndItsBreakpointVariants_AddTheirPrefixedClasses()
    {
        var element = Render(p => p
            .Add(x => x.Direction, FlexDirection.Col)
            .Add(x => x.DirectionSm, FlexDirection.ColReverse)
            .Add(x => x.DirectionMd, FlexDirection.Row)
            .Add(x => x.DirectionLg, FlexDirection.RowReverse)
            .Add(x => x.DirectionXl, FlexDirection.Col)
            .Add(x => x.DirectionXl2, FlexDirection.Row));

        Assert.Contains("flex-col", element.ClassList);
        Assert.Contains("sm:flex-col-reverse", element.ClassList);
        Assert.Contains("md:flex-row", element.ClassList);
        Assert.Contains("lg:flex-row-reverse", element.ClassList);
        Assert.Contains("xl:flex-col", element.ClassList);
        Assert.Contains("2xl:flex-row", element.ClassList);
    }

    [Theory]
    [InlineData(FlexWrap.NoWrap, "flex-nowrap")]
    [InlineData(FlexWrap.Wrap, "flex-wrap")]
    [InlineData(FlexWrap.WrapReverse, "flex-wrap-reverse")]
    public void Wrap_AddsItsClass(FlexWrap wrap, string expected)
    {
        Assert.Contains(expected, Render(p => p.Add(x => x.Wrap, wrap)).ClassList);
    }

    [Theory]
    [InlineData(JustifyContent.Start, "justify-start")]
    [InlineData(JustifyContent.Center, "justify-center")]
    [InlineData(JustifyContent.End, "justify-end")]
    [InlineData(JustifyContent.Between, "justify-between")]
    [InlineData(JustifyContent.Around, "justify-around")]
    [InlineData(JustifyContent.Evenly, "justify-evenly")]
    [InlineData(JustifyContent.Stretch, "justify-stretch")]
    public void Justify_AddsItsClass(JustifyContent justify, string expected)
    {
        Assert.Contains(expected, Render(p => p.Add(x => x.Justify, justify)).ClassList);
    }

    [Theory]
    [InlineData(AlignItems.Start, "items-start")]
    [InlineData(AlignItems.Center, "items-center")]
    [InlineData(AlignItems.End, "items-end")]
    [InlineData(AlignItems.Stretch, "items-stretch")]
    [InlineData(AlignItems.Baseline, "items-baseline")]
    public void Align_AddsItsClass(AlignItems align, string expected)
    {
        Assert.Contains(expected, Render(p => p.Add(x => x.Align, align)).ClassList);
    }

    [Theory]
    [InlineData(AlignContent.Start, "content-start")]
    [InlineData(AlignContent.Center, "content-center")]
    [InlineData(AlignContent.End, "content-end")]
    [InlineData(AlignContent.Between, "content-between")]
    [InlineData(AlignContent.Around, "content-around")]
    [InlineData(AlignContent.Evenly, "content-evenly")]
    [InlineData(AlignContent.Stretch, "content-stretch")]
    public void AlignContent_AddsItsClass(AlignContent alignContent, string expected)
    {
        Assert.Contains(expected, Render(p => p.Add(x => x.AlignContent, alignContent)).ClassList);
    }

    [Fact]
    public void Gaps_UseTheSpacingSteps()
    {
        var element = Render(p => p.Add(x => x.Gap, 0.5).Add(x => x.ColGap, 5).Add(x => x.RowGap, 1));

        Assert.Contains("gap-0.5", element.ClassList);
        Assert.Contains("gap-x-5", element.ClassList);
        Assert.Contains("gap-y-1", element.ClassList);
    }

    [Fact]
    public void AGapOffTheScale_AddsNoGap()
    {
        var element = Render(p => p.Add(x => x.Gap, 13));

        Assert.DoesNotContain(element.ClassList, value => value.StartsWith("gap-", StringComparison.Ordinal));
    }

    [Fact]
    public void FullWidth_AndFullHeight_FillTheParent()
    {
        var element = Render(p => p.Add(x => x.FullWidth, true).Add(x => x.FullHeight, true));

        Assert.Contains(theme.FullWidth, element.ClassList);
        Assert.Contains(theme.FullHeight, element.ClassList);
    }

    [Theory]
    [InlineData(null, "divide-x", null)]
    [InlineData(FlexDirection.Row, "divide-x", null)]
    [InlineData(FlexDirection.RowReverse, "divide-x", "divide-x-reverse")]
    [InlineData(FlexDirection.Col, "divide-y", null)]
    [InlineData(FlexDirection.ColReverse, "divide-y", "divide-y-reverse")]
    public void Divided_DrawsLinesAcrossTheDirection(FlexDirection? direction, string expected, string? reverse)
    {
        var element = Render(p => p.Add(x => x.Divided, true).Add(x => x.Direction, direction));

        Assert.Contains(expected, element.ClassList);
        Assert.All(theme.DivideColor.Split(' '), value => Assert.Contains(value, element.ClassList));
        if (reverse is not null)
        {
            Assert.Contains(reverse, element.ClassList);
        }
    }

    [Fact]
    public void Divided_False_DrawsNoLines()
    {
        var element = Render(p => p.Add(x => x.Direction, FlexDirection.Col));

        Assert.DoesNotContain(element.ClassList, value => value.StartsWith("divide-", StringComparison.Ordinal));
    }

    [Fact]
    public void Element_RendersAListWithItsListRole()
    {
        var element = Render(p => p.Add(x => x.Element, LayoutElement.Ul).Add(x => x.AriaLabel, "Topics"));

        Assert.Equal("UL", element.TagName);
        Assert.Equal("list", element.GetAttribute("role"));
        Assert.Equal("Topics", element.GetAttribute("aria-label"));
    }

    [Fact]
    public void Class_OverridesTheThemesDividerColor()
    {
        var element = Render(p => p.Add(x => x.Divided, true).Add(x => x.Class, "divide-current/10"));

        Assert.Contains("divide-current/10", element.ClassList);
        Assert.DoesNotContain(theme.DivideColor.Split(' ')[0], element.ClassList);
    }
}
