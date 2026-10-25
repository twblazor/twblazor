using Bunit;
using TwBlazor.Components;
using TwBlazor.Configuration.Components;
using TwBlazor.Enums;

namespace TwBlazor.Tests.Components.Flex;

/// <summary>
/// Tests <see cref="TwFlexItem"/>: flex size, grow and shrink, basis, self alignment, order and pushing.
/// </summary>
public class TwFlexItemTests : TwBlazorTestBase
{
    private TwFlexTheme theme => Theme.Components.Require<TwFlexTheme>();

    private AngleSharp.Dom.IElement Render(Action<ComponentParameterCollectionBuilder<TwFlexItem>>? configure = null) =>
        TestContext.Render<TwFlexItem>(p => configure?.Invoke(p)).Find("[id]");

    private AngleSharp.Dom.IElement RenderInFlex(FlexDirection? direction, Action<ComponentParameterCollectionBuilder<TwFlexItem>> configure) =>
        TestContext.Render<TwFlex>(p => p
            .Add(x => x.Direction, direction)
            .AddChildContent<TwFlexItem>(item => configure(item.Add(x => x.Id, "item"))))
            .Find("#item");

    [Fact]
    public void ByDefault_RendersADiv_WithNoLayoutClasses()
    {
        var element = Render();

        Assert.Equal("DIV", element.TagName);
        Assert.Empty(element.ClassList);
    }

    [Theory]
    [InlineData(FlexSize.Auto, "flex-auto")]
    [InlineData(FlexSize.Initial, "flex-initial")]
    [InlineData(FlexSize.None, "flex-none")]
    [InlineData(FlexSize.Fill, "flex-1")]
    public void Flex_AddsItsShorthand(FlexSize size, string expected)
    {
        Assert.Contains(expected, Render(p => p.Add(x => x.Flex, size)).ClassList);
    }

    [Theory]
    [InlineData(true, "grow")]
    [InlineData(false, "grow-0")]
    public void Grow_SwitchesGrowingOnOrOff(bool grow, string expected)
    {
        Assert.Contains(expected, Render(p => p.Add(x => x.Grow, grow)).ClassList);
    }

    [Theory]
    [InlineData(true, "shrink")]
    [InlineData(false, "shrink-0")]
    public void Shrink_SwitchesShrinkingOnOrOff(bool shrink, string expected)
    {
        Assert.Contains(expected, Render(p => p.Add(x => x.Shrink, shrink)).ClassList);
    }

    [Theory]
    [InlineData(FlexBasis.Auto, "basis-auto")]
    [InlineData(FlexBasis.Full, "basis-full")]
    [InlineData(FlexBasis.Half, "basis-1/2")]
    [InlineData(FlexBasis.Third, "basis-1/3")]
    [InlineData(FlexBasis.TwoThirds, "basis-2/3")]
    [InlineData(FlexBasis.Quarter, "basis-1/4")]
    [InlineData(FlexBasis.ThreeQuarters, "basis-3/4")]
    public void Basis_AddsItsClass(FlexBasis basis, string expected)
    {
        Assert.Contains(expected, Render(p => p.Add(x => x.Basis, basis)).ClassList);
    }

    [Theory]
    [InlineData(AlignSelf.Start, "self-start")]
    [InlineData(AlignSelf.Center, "self-center")]
    [InlineData(AlignSelf.End, "self-end")]
    [InlineData(AlignSelf.Stretch, "self-stretch")]
    [InlineData(AlignSelf.Baseline, "self-baseline")]
    public void AlignSelf_AddsItsClass(AlignSelf alignSelf, string expected)
    {
        Assert.Contains(expected, Render(p => p.Add(x => x.AlignSelf, alignSelf)).ClassList);
    }

    [Theory]
    [InlineData(1, "order-1")]
    [InlineData(12, "order-12")]
    [InlineData(ItemOrder.First, "order-first")]
    [InlineData(ItemOrder.Last, "order-last")]
    public void Order_AddsItsClass(int order, string expected)
    {
        Assert.Contains(expected, Render(p => p.Add(x => x.Order, order)).ClassList);
    }

    [Fact]
    public void AnOrderOffTheScale_AddsNoOrder()
    {
        Assert.DoesNotContain(Render(p => p.Add(x => x.Order, 13)).ClassList, value => value.StartsWith("order-", StringComparison.Ordinal));
    }

    [Fact]
    public void PushEnd_InARow_PushesToTheEnd()
    {
        var element = RenderInFlex(FlexDirection.Row, p => p.Add(x => x.PushEnd, true));

        Assert.Contains(theme.ItemPushEnd, element.ClassList);
        Assert.DoesNotContain(theme.ItemPushEndColumn, element.ClassList);
    }

    [Theory]
    [InlineData(FlexDirection.Column)]
    [InlineData(FlexDirection.ColumnReverse)]
    public void PushEnd_InAColumn_PushesToTheBottom(FlexDirection direction)
    {
        var element = RenderInFlex(direction, p => p.Add(x => x.PushEnd, true));

        Assert.Contains(theme.ItemPushEndColumn, element.ClassList);
        Assert.DoesNotContain(theme.ItemPushEnd, element.ClassList);
    }

    [Fact]
    public void PushEnd_OutsideAFlexLayout_PushesToTheEnd()
    {
        Assert.Contains(theme.ItemPushEnd, Render(p => p.Add(x => x.PushEnd, true)).ClassList);
    }

    [Fact]
    public void MinWidthZero_LetsTheItemShrinkBelowItsContent()
    {
        Assert.Contains(theme.ItemMinWidthZero, Render(p => p.Add(x => x.MinWidthZero, true)).ClassList);
    }

    [Fact]
    public void Element_RendersAListItem()
    {
        var element = Render(p => p.Add(x => x.Element, LayoutElement.Li));

        Assert.Equal("LI", element.TagName);
        Assert.Null(element.GetAttribute("role"));
    }
}
