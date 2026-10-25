using TwBlazor.Configuration;
using TwBlazor.Enums;
using ThemeBase = TwBlazor.Theme.Theme;

namespace TwBlazor.Tests.Configuration;

/// <summary>
/// Tests the reusable layout scales: how each looks up a class, and that the default theme fills them in.
/// </summary>
public class TwBlazorLayoutScalesTests
{
    private static readonly TwBlazorTheme _theme = ThemeBase.CreateDefaultTheme();

    [Theory]
    [InlineData(Breakpoint.Base, "a")]
    [InlineData(Breakpoint.Sm, "b")]
    [InlineData(Breakpoint.Md, "c")]
    [InlineData(Breakpoint.Lg, "d")]
    [InlineData(Breakpoint.Xl, "e")]
    [InlineData(Breakpoint.Xl2, "f")]
    public void ResponsiveClass_ReturnsTheClassForEachBreakpoint(Breakpoint breakpoint, string expected)
    {
        var classes = new TwResponsiveClass("a", "b", "c", "d", "e", "f");

        Assert.Equal(expected, classes.Get(breakpoint));
    }

    [Fact]
    public void ResponsiveClass_StartsEmpty()
    {
        var classes = new TwResponsiveClass();

        Assert.All(Enum.GetValues<Breakpoint>(), breakpoint => Assert.Equal(string.Empty, classes.Get(breakpoint)));
    }

    [Fact]
    public void ResponsiveScale_LooksUpSteps_FullAndMissingValues()
    {
        var scale = new TwResponsiveScale
        {
            [2] = new("span-2", "sm:span-2", "md:span-2", "lg:span-2", "xl:span-2", "2xl:span-2"),
            Full = new("full", "sm:full", "md:full", "lg:full", "xl:full", "2xl:full")
        };

        Assert.Equal("md:span-2", scale.Get(2, Breakpoint.Md));
        Assert.Equal("lg:full", scale.Get(GridSpan.Full, Breakpoint.Lg));
        Assert.Equal(string.Empty, scale.Get(3, Breakpoint.Base));
        Assert.Equal(string.Empty, scale.Get(null, Breakpoint.Base));
    }

    [Fact]
    public void ClassScale_LooksUpSteps()
    {
        var scale = new TwClassScale { [1] = "order-1" };

        Assert.Equal("order-1", scale.Get(1));
        Assert.Equal(string.Empty, scale.Get(2));
        Assert.Equal(string.Empty, scale.Get(null));
    }

    [Fact]
    public void SpacingStepScale_LooksUpWholeAndHalfSteps()
    {
        var scale = new TwSpacingStepScale { [0.5] = "gap-0.5", [4] = "gap-4" };

        Assert.Equal("gap-0.5", scale.Get(0.5));
        Assert.Equal("gap-4", scale.Get(4));
        Assert.Equal(string.Empty, scale.Get(4.5));
        Assert.Equal(string.Empty, scale.Get(null));
    }

    [Theory]
    [InlineData(ContainerSize.Xs, "max-w-xs")]
    [InlineData(ContainerSize.Sm, "max-w-sm")]
    [InlineData(ContainerSize.Md, "max-w-md")]
    [InlineData(ContainerSize.Lg, "max-w-lg")]
    [InlineData(ContainerSize.Xl, "max-w-xl")]
    [InlineData(ContainerSize.Xl2, "max-w-2xl")]
    [InlineData(ContainerSize.Xl3, "max-w-3xl")]
    [InlineData(ContainerSize.Xl4, "max-w-4xl")]
    [InlineData(ContainerSize.Xl5, "max-w-5xl")]
    [InlineData(ContainerSize.Xl6, "max-w-6xl")]
    [InlineData(ContainerSize.Xl7, "max-w-7xl")]
    [InlineData(ContainerSize.Prose, "max-w-prose")]
    [InlineData(ContainerSize.Full, "max-w-full")]
    [InlineData(ContainerSize.None, "max-w-none")]
    public void MaxWidthScale_InTheDefaultTheme_HasEverySize(ContainerSize size, string expected)
    {
        Assert.Equal(expected, _theme.Sizing.MaxWidth.Get(size));
    }

    [Theory]
    [InlineData(FlexDirection.Row, "sm:flex-row")]
    [InlineData(FlexDirection.RowReverse, "sm:flex-row-reverse")]
    [InlineData(FlexDirection.Column, "sm:flex-col")]
    [InlineData(FlexDirection.ColumnReverse, "sm:flex-col-reverse")]
    public void ResponsiveDirection_InTheDefaultTheme_HasEveryDirection(FlexDirection direction, string expected)
    {
        Assert.Equal(expected, _theme.Flexbox.ResponsiveDirection.Get(direction, Breakpoint.Sm));
        Assert.Equal(string.Empty, _theme.Flexbox.ResponsiveDirection.Get(null, Breakpoint.Sm));
    }

    [Fact]
    public void BasisAutoFlowAndAutoSizeScales_ReturnNothingForNull()
    {
        Assert.Equal(string.Empty, _theme.Flexbox.Basis.Get(null));
        Assert.Equal(string.Empty, _theme.Grid.AutoFlow.Get(null));
        Assert.Equal(string.Empty, _theme.Grid.AutoRows.Get(null));
    }

    [Fact]
    public void TheDefaultTheme_WritesOutEveryGridStep_AtEveryBreakpoint()
    {
        var grid = _theme.Grid;

        Assert.Equal(Enumerable.Range(1, 12), grid.Columns.Keys.Order());
        Assert.Equal(Enumerable.Range(1, 6), grid.Rows.Keys.Order());
        Assert.Equal(Enumerable.Range(1, 12), grid.ColumnSpan.Keys.Order());
        Assert.Equal(Enumerable.Range(1, 6), grid.RowSpan.Keys.Order());
        Assert.Equal(Enumerable.Range(1, 13), grid.ColumnStart.Keys.Order());
        Assert.Equal(Enumerable.Range(1, 7), grid.RowStart.Keys.Order());
        Assert.Equal("2xl:col-span-full", grid.ColumnSpan.Full.Xl2);
        Assert.Equal("row-span-full", grid.RowSpan.Full.Base);

        string[] prefixes = ["", "sm:", "md:", "lg:", "xl:", "2xl:"];
        Assert.All(grid.Columns, pair => Assert.Equal(
            prefixes.Select(prefix => $"{prefix}grid-cols-{pair.Key}"),
            Enum.GetValues<Breakpoint>().Select(pair.Value.Get)));
    }

    [Fact]
    public void TheDefaultTheme_ListsTheSameSpacingSteps_ForEveryScale()
    {
        var steps = _theme.Spacing.Steps;
        var expected = steps.Gap.Keys.Order().ToList();

        Assert.Contains(0.5, expected);
        Assert.Contains(24, expected);
        Assert.All(new[] { steps.GapX, steps.GapY, steps.PaddingX, steps.PaddingY, steps.PaddingTop, steps.PaddingBottom }, scale => Assert.Equal(expected, scale.Keys.Order()));
        Assert.Equal("gap-x-2.5", steps.GapX.Get(2.5));
        Assert.Equal("pb-12", steps.PaddingBottom.Get(12));
    }
}
