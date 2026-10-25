using Bunit;
using TwBlazor.Components;
using TwBlazor.Configuration.Components;
using TwBlazor.Enums;

namespace TwBlazor.Tests.Components.Container;

/// <summary>
/// Tests <see cref="TwContainer"/>: its width, centering, padding, surfaces and the element it renders.
/// </summary>
public class TwContainerTests : TwBlazorTestBase
{
    private TwContainerTheme theme => Theme.Components.Require<TwContainerTheme>();

    private AngleSharp.Dom.IElement Render(Action<ComponentParameterCollectionBuilder<TwContainer>>? configure = null) =>
        TestContext.Render<TwContainer>(p => configure?.Invoke(p)).Find("[id]");

    private static void AssertHasAll(AngleSharp.Dom.IElement element, string classes) =>
        Assert.All(classes.Split(' ', StringSplitOptions.RemoveEmptyEntries), value => Assert.Contains(value, element.ClassList));

    [Fact]
    public void ByDefault_RendersACenteredDiv_WithTheDefaultMaxWidthAndGutter()
    {
        var element = Render();

        Assert.Equal("DIV", element.TagName);
        AssertHasAll(element, theme.Base);
        AssertHasAll(element, theme.Centered);
        AssertHasAll(element, theme.Gutter);
        Assert.Contains(Theme.Sizing.MaxWidth.Get(theme.DefaultMaxWidth), element.ClassList);
        Assert.Null(element.GetAttribute("role"));
    }

    [Theory]
    [InlineData(ContainerSize.Xs, "max-w-xs")]
    [InlineData(ContainerSize.Xl2, "max-w-2xl")]
    [InlineData(ContainerSize.Xl5, "max-w-5xl")]
    [InlineData(ContainerSize.Prose, "max-w-prose")]
    [InlineData(ContainerSize.Full, "max-w-full")]
    [InlineData(ContainerSize.None, "max-w-none")]
    public void MaxWidth_UsesTheSharedMaxWidthScale(ContainerSize size, string expected)
    {
        var element = Render(p => p.Add(x => x.MaxWidth, size));

        Assert.Contains(expected, element.ClassList);
        Assert.DoesNotContain("max-w-7xl", element.ClassList);
    }

    [Fact]
    public void Centered_False_LeavesOutTheCenteringClasses()
    {
        var element = Render(p => p.Add(x => x.Centered, false));

        Assert.DoesNotContain(theme.Centered, element.ClassList);
    }

    [Fact]
    public void Gutter_ReplacesTheThemeGutter_WithThatSpacingStep()
    {
        var element = Render(p => p.Add(x => x.Gutter, 6));

        Assert.Contains("px-6", element.ClassList);
        Assert.DoesNotContain("lg:px-8", element.ClassList);
    }

    [Fact]
    public void Gutter_Zero_RemovesTheSidePadding()
    {
        var element = Render(p => p.Add(x => x.Gutter, 0));

        Assert.Contains("px-0", element.ClassList);
    }

    [Fact]
    public void Gutter_OffTheScale_AddsNoPadding()
    {
        var element = Render(p => p.Add(x => x.Gutter, 9));

        Assert.DoesNotContain(element.ClassList, value => value.StartsWith("px-", StringComparison.Ordinal) || value.Contains(":px-", StringComparison.Ordinal));
    }

    [Fact]
    public void PaddingY_PaddingTop_AndPaddingBottom_AddTheirSpacingSteps()
    {
        var element = Render(p => p
            .Add(x => x.PaddingY, 12)
            .Add(x => x.PaddingTop, 14)
            .Add(x => x.PaddingBottom, 0.5));

        Assert.Contains("py-12", element.ClassList);
        Assert.Contains("pt-14", element.ClassList);
        Assert.Contains("pb-0.5", element.ClassList);
    }

    [Theory]
    [InlineData(ContainerSurface.Default)]
    [InlineData(ContainerSurface.Subtle)]
    [InlineData(ContainerSurface.Elevated)]
    public void Surface_PaintsTheThemesSurface(ContainerSurface surface)
    {
        var expected = surface switch
        {
            ContainerSurface.Default => theme.SurfaceDefault,
            ContainerSurface.Subtle => theme.SurfaceSubtle,
            _ => theme.SurfaceElevated
        };

        AssertHasAll(Render(p => p.Add(x => x.Surface, surface)), expected);
    }

    [Fact]
    public void Surface_None_PaintsNoBackground()
    {
        var element = Render();

        Assert.DoesNotContain(element.ClassList, value => value.StartsWith("bg-", StringComparison.Ordinal));
    }

    [Fact]
    public void QueryContainer_MakesItAQueryContainer()
    {
        Assert.DoesNotContain(theme.QueryContainer, Render().ClassList);
        Assert.Contains(theme.QueryContainer, Render(p => p.Add(x => x.QueryContainer, true)).ClassList);
    }

    [Fact]
    public void Element_RendersTheChosenLandmark_NamedByItsHeading()
    {
        var element = Render(p => p
            .Add(x => x.Element, LayoutElement.Section)
            .Add(x => x.AriaLabelledBy, "offers-heading"));

        Assert.Equal("SECTION", element.TagName);
        Assert.Equal("offers-heading", element.GetAttribute("aria-labelledby"));
    }

    [Theory]
    [InlineData(LayoutElement.Ul)]
    [InlineData(LayoutElement.Ol)]
    public void AListElement_KeepsItsListRole(LayoutElement listElement)
    {
        var element = Render(p => p.Add(x => x.Element, listElement));

        Assert.Equal("list", element.GetAttribute("role"));
    }

    [Fact]
    public void AnExplicitRole_ReplacesTheListRole()
    {
        var element = Render(p => p
            .Add(x => x.Element, LayoutElement.Ul)
            .AddUnmatched("role", "menu"));

        Assert.Equal("menu", element.GetAttribute("role"));
    }

    [Fact]
    public void ShadowAndRounded_ApplyOnlyWhenSet()
    {
        var plain = Render();
        var decorated = Render(p => p.Add(x => x.Shadow, Shadow.Lg).Add(x => x.Rounded, Rounded.Lg));

        Assert.DoesNotContain(Theme.Shadows.Sm, plain.ClassList);
        Assert.DoesNotContain(Theme.Rounded.Lg, plain.ClassList);
        Assert.Contains(Theme.Shadows.Lg, decorated.ClassList);
        Assert.Contains(Theme.Rounded.Lg, decorated.ClassList);
    }

    [Fact]
    public void Class_IsMergedLast_SoItWinsOverTheTheme()
    {
        var element = Render(p => p.Add(x => x.Class, "max-w-md custom"));

        Assert.Contains("max-w-md", element.ClassList);
        Assert.Contains("custom", element.ClassList);
        Assert.DoesNotContain("max-w-7xl", element.ClassList);
    }

    [Fact]
    public void RendersItsContent_StyleAndAttributes()
    {
        var cut = TestContext.Render<TwContainer>(p => p
            .Add(x => x.Style, "color: red")
            .AddUnmatched("data-testid", "container")
            .AddChildContent("<p>Hello</p>"));
        var element = cut.Find("[data-testid='container']");

        Assert.Equal("color: red", element.GetAttribute("style"));
        Assert.Equal("Hello", element.QuerySelector("p")!.TextContent);
    }
}
