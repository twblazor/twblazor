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
    [InlineData(FlexDirection.Col, "sm:flex-col")]
    [InlineData(FlexDirection.ColReverse, "sm:flex-col-reverse")]
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

        Assert.Equal(Enumerable.Range(1, 12), grid.Cols.Keys.Order());
        Assert.Equal(Enumerable.Range(1, 6), grid.Rows.Keys.Order());
        Assert.Equal(Enumerable.Range(1, 12), grid.ColSpan.Keys.Order());
        Assert.Equal(Enumerable.Range(1, 6), grid.RowSpan.Keys.Order());
        Assert.Equal(Enumerable.Range(1, 13), grid.ColStart.Keys.Order());
        Assert.Equal(Enumerable.Range(1, 7), grid.RowStart.Keys.Order());
        Assert.Equal("2xl:col-span-full", grid.ColSpan.Full.Xl2);
        Assert.Equal("row-span-full", grid.RowSpan.Full.Base);

        string[] prefixes = ["", "sm:", "md:", "lg:", "xl:", "2xl:"];
        Assert.All(grid.Cols, pair => Assert.Equal(
            prefixes.Select(prefix => $"{prefix}grid-cols-{pair.Key}"),
            Enum.GetValues<Breakpoint>().Select(pair.Value.Get)));
    }

    [Fact]
    public void TheDefaultTheme_ListsTheSameSpacingSteps_ForEveryScale()
    {
        var spacing = _theme.Spacing;
        var expected = spacing.Gap.Keys.Order().ToList();
        TwSpacingStepScale[] scales =
        [
            spacing.GapX, spacing.GapY, spacing.Padding, spacing.PaddingX, spacing.PaddingY, spacing.PaddingTop, spacing.PaddingBottom,
            spacing.PaddingStart, spacing.Margin, spacing.MarginTop, spacing.MarginBottom, spacing.MarginStart, spacing.MarginEnd
        ];

        Assert.Contains(0.5, expected);
        Assert.Contains(24, expected);
        Assert.All(scales, scale => Assert.Equal(expected, scale.Keys.Order()));
        Assert.Equal("gap-x-2.5", spacing.GapX.Get(2.5));
        Assert.Equal("pb-12", spacing.PaddingBottom.Get(12));
        Assert.Equal("ms-2", spacing.MarginStart.Get(2));
        Assert.Equal("p-0.5", spacing.Padding.Get(0.5));
    }

    [Theory]
    [InlineData("gap", 2, "gap-2")]
    [InlineData("p", 4, "p-4")]
    [InlineData("mt", 3, "mt-3")]
    public void SpacingSteps_HoldTheTailwindClassForTheirStep(string utility, double step, string expected)
    {
        var scale = utility switch
        {
            "gap" => _theme.Spacing.Gap,
            "p" => _theme.Spacing.Padding,
            _ => _theme.Spacing.MarginTop
        };

        Assert.Equal(expected, scale[step]);
    }

    [Fact]
    public void TheDefaultTheme_DeclaresEveryToken()
    {
        var tokens = _theme.Tokens;
        var empty = typeof(TwBlazorTokens).GetProperties()
            .Where(property => string.IsNullOrWhiteSpace((string?)property.GetValue(tokens)))
            .Select(property => property.Name);

        Assert.Empty(empty);
        Assert.Equal(tokens.Transparent, _theme.Colors.Transparent);
        Assert.Equal("text-[10px]", _theme.Typography.Size.Xxs);
        Assert.Equal("text-5xl", _theme.Typography.Size.Xl5);
        Assert.Equal("font-bold", _theme.Typography.Weight.Bold);
        Assert.Equal("truncate", _theme.Typography.Truncate);
    }
}
