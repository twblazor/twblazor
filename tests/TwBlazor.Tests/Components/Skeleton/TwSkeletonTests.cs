using Bunit;
using Microsoft.JSInterop;
using TwBlazor.Components;
using TwBlazor.Enums;
using TwBlazor.Models;

namespace TwBlazor.Tests.Components.Skeleton;

public class TwSkeletonTests : TwBlazorTestBase
{
    #region Standalone placeholder

    [Fact]
    public void TwSkeleton_Renders_TextPlaceholder_ByDefault()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwSkeleton>();

        // Assert
        var div = cut.Find("div");
        Assert.Contains("animate-pulse", div.GetAttribute("class"));
        Assert.Contains("w-full", div.GetAttribute("class"));
        Assert.Contains("h-4", div.GetAttribute("class"));
    }

    [Fact]
    public void TwSkeleton_GeneratesId_WhenNotProvided()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwSkeleton>();

        // Assert
        var div = cut.Find("div");
        Assert.NotNull(div.GetAttribute("id"));
        Assert.StartsWith("skeleton-", div.GetAttribute("id"));
    }

    [Fact]
    public void TwSkeleton_AppliesCustomClass()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwSkeleton>(p => p.Add(x => x.Class, "my-skeleton-class"));

        // Assert
        Assert.Contains("my-skeleton-class", cut.Find("div").GetAttribute("class"));
    }

    [Theory]
    [InlineData(SkeletonType.Text, "h-4")]
    [InlineData(SkeletonType.Rectangle, "h-24")]
    [InlineData(SkeletonType.Circle, "size-12")]
    public void TwSkeleton_AppliesSkeletonType(SkeletonType skeletonType, string expectedClass)
    {
        // Arrange & Act
        var cut = TestContext.Render<TwSkeleton>(p => p.Add(x => x.SkeletonType, skeletonType));

        // Assert
        Assert.Contains(expectedClass, cut.Find("div").GetAttribute("class"));
    }

    [Fact]
    public void TwSkeleton_Circle_IsAlwaysFullyRounded_RegardlessOfRoundedParameter()
    {
        // Arrange & Act - a circle overridden to "square" corners would no longer be a circle, so this
        // shape ignores the Rounded parameter entirely.
        var cut = TestContext.Render<TwSkeleton>(p => p
            .Add(x => x.SkeletonType, SkeletonType.Circle)
            .Add(x => x.Rounded, Rounded.None));

        // Assert
        Assert.Contains("rounded-full", cut.Find("div").GetAttribute("class"));
    }

    [Theory]
    [InlineData(Rounded.None, "rounded-none")]
    [InlineData(Rounded.Full, "rounded-full")]
    public void TwSkeleton_Rectangle_AppliesRoundedParameter(Rounded rounded, string expectedClass)
    {
        // Arrange & Act
        var cut = TestContext.Render<TwSkeleton>(p => p
            .Add(x => x.SkeletonType, SkeletonType.Rectangle)
            .Add(x => x.Rounded, rounded));

        // Assert
        Assert.Contains(expectedClass, cut.Find("div").GetAttribute("class"));
    }

    [Theory]
    [InlineData(SkeletonAnimation.Pulse, "animate-pulse")]
    [InlineData(SkeletonAnimation.Wave, "tw-wave")]
    public void TwSkeleton_AppliesAnimationClass(SkeletonAnimation animation, string expectedClass)
    {
        // Arrange & Act
        var cut = TestContext.Render<TwSkeleton>(p => p.Add(x => x.Animation, animation));

        // Assert
        Assert.Contains(expectedClass, cut.Find("div").GetAttribute("class"));
    }

    [Fact]
    public void TwSkeleton_Animation_None_AppliesNoAnimationClass()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwSkeleton>(p => p.Add(x => x.Animation, SkeletonAnimation.None));

        // Assert
        var classes = cut.Find("div").GetAttribute("class");
        Assert.DoesNotContain("animate-pulse", classes);
        Assert.DoesNotContain("tw-wave", classes);
    }

    [Fact]
    public void TwSkeleton_AppliesWidthAndHeight_AsInlineStyle()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwSkeleton>(p => p
            .Add(x => x.SkeletonType, SkeletonType.Circle)
            .Add(x => x.Width, "50px")
            .Add(x => x.Height, "50px"));

        // Assert
        var style = cut.Find("div").GetAttribute("style");
        Assert.Contains("width:50px", style);
        Assert.Contains("height:50px", style);
    }

    [Fact]
    public void TwSkeleton_CombinesWidthHeight_WithExplicitStyle()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwSkeleton>(p => p
            .Add(x => x.Width, "200px")
            .Add(x => x.Style, "margin-top:4px"));

        // Assert
        var style = cut.Find("div").GetAttribute("style");
        Assert.Contains("width:200px", style);
        Assert.Contains("margin-top:4px", style);
    }

    [Fact]
    public void TwSkeleton_HasAccessibleBusyState_WhileLoading()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwSkeleton>();

        // Assert
        var div = cut.Find("div");
        Assert.Equal("status", div.GetAttribute("role"));
        Assert.Equal("true", div.GetAttribute("aria-busy"));
        Assert.Equal("Loading", div.GetAttribute("aria-label"));
    }

    [Fact]
    public void TwSkeleton_UsesExplicitAriaLabel_OverGenericFallback()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwSkeleton>(p => p.Add(x => x.AriaLabel, "Loading profile"));

        // Assert
        Assert.Equal("Loading profile", cut.Find("div").GetAttribute("aria-label"));
    }

    [Fact]
    public void TwSkeleton_Loading_False_WithNoChildContent_RendersEmpty()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwSkeleton>(p => p.Add(x => x.Loading, false));

        // Assert
        var div = cut.Find("div");
        Assert.Empty(div.Children);
        Assert.Null(div.GetAttribute("role"));
        Assert.Null(div.GetAttribute("aria-busy"));
    }

    #endregion

    #region ChildContent

    [Fact]
    public void TwSkeleton_Loading_False_RendersChildContent()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwSkeleton>(p => p
            .Add(x => x.Loading, false)
            .Add(x => x.ChildContent, RenderFragmentBuilder("Real content")));

        // Assert
        Assert.Contains("Real content", cut.Markup);
    }

    [Fact]
    public void TwSkeleton_Loading_True_WithChildContent_HidesRealContentBehindMeasuringWrapper()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwSkeleton>(p => p
            .Add(x => x.Loading, true)
            .Add(x => x.ChildContent, RenderFragmentBuilder("Real content")));

        // Assert - content is still rendered (so it can be measured) but hidden from view
        var wrapper = cut.Find("div > div");
        Assert.Contains("invisible", wrapper.GetAttribute("class"));
        Assert.Contains("Real content", wrapper.TextContent);
    }

    [Fact]
    public void TwSkeleton_Loading_True_WithChildContent_RegistersJsObserver()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwSkeleton>(p => p
            .Add(x => x.Loading, true)
            .Add(x => x.ChildContent, RenderFragmentBuilder("Real content")));

        // Assert
        Assert.Contains(TestContext.JSInterop.Invocations, i => i.Identifier == "twSkeleton.observe");
    }

    [Fact]
    public void TwSkeleton_Loading_False_WithChildContent_DoesNotRegisterJsObserver()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwSkeleton>(p => p
            .Add(x => x.Loading, false)
            .Add(x => x.ChildContent, RenderFragmentBuilder("Real content")));

        // Assert
        Assert.DoesNotContain(TestContext.JSInterop.Invocations, i => i.Identifier == "twSkeleton.observe");
    }

    [Fact]
    public async Task TwSkeleton_OnRectsMeasured_RendersGeneratedPlaceholderBlocks()
    {
        // Arrange
        var cut = TestContext.Render<TwSkeleton>(p => p
            .Add(x => x.Loading, true)
            .Add(x => x.ChildContent, RenderFragmentBuilder("Real content")));

        var rects = new List<SkeletonRect>
        {
            new() { Top = 0, Left = 0, Width = 120, Height = 16, Shape = "text" },
            new() { Top = 20, Left = 0, Width = 48, Height = 48, Shape = "circle" },
            new() { Top = 80, Left = 0, Width = 200, Height = 100, Shape = "rect" }
        };

        // Act
        await cut.InvokeAsync(() => cut.Instance.OnRectsMeasured(rects));

        // Assert
        var generated = cut.FindAll("div[style*='position:absolute']");
        Assert.Equal(3, generated.Count);

        var textRect = generated.Single(d => d.GetAttribute("style")!.Contains("width:120px"));
        Assert.Contains("rounded-full", textRect.GetAttribute("class"));

        var circleRect = generated.Single(d => d.GetAttribute("style")!.Contains("width:48px"));
        Assert.Contains("rounded-full", circleRect.GetAttribute("class"));

        var rectRect = generated.Single(d => d.GetAttribute("style")!.Contains("width:200px"));
        Assert.Contains("bg-[oklch(95%_0_0)]", rectRect.GetAttribute("class"));
    }

    [Fact]
    public async Task TwSkeleton_TogglingLoadingToFalse_UnobservesAndClearsGeneratedBlocks()
    {
        // Arrange
        var cut = TestContext.Render<TwSkeleton>(p => p
            .Add(x => x.Loading, true)
            .Add(x => x.ChildContent, RenderFragmentBuilder("Real content")));

        await cut.InvokeAsync(() => cut.Instance.OnRectsMeasured(
        [
            new SkeletonRect { Top = 0, Left = 0, Width = 120, Height = 16, Shape = "text" }
        ]));

        // Act
        cut.Render(p => p.Add(x => x.Loading, false));

        // Assert
        Assert.Contains(TestContext.JSInterop.Invocations, i => i.Identifier == "twSkeleton.unobserve");
        Assert.Contains("Real content", cut.Markup);
        Assert.Empty(cut.FindAll("div[style*='position:absolute']"));
    }

    [Fact]
    public void TwSkeleton_StartObserving_SwallowsJSDisconnectedException()
    {
        // Arrange
        TestContext.JSInterop.SetupVoid("twSkeleton.observe", _ => true)
            .SetException(new JSDisconnectedException("Circuit disconnected"));

        // Act & Assert - should not throw/propagate during rendering
        var cut = TestContext.Render<TwSkeleton>(p => p
            .Add(x => x.Loading, true)
            .Add(x => x.ChildContent, RenderFragmentBuilder("Real content")));
        Assert.NotNull(cut.Find("div"));
    }

    [Fact]
    public void TwSkeleton_StopObserving_SwallowsJSDisconnectedException()
    {
        // Arrange - starts observing normally, then the unobserve call (triggered by the Loading
        // transition below, not disposal) throws.
        var cut = TestContext.Render<TwSkeleton>(p => p
            .Add(x => x.Loading, true)
            .Add(x => x.ChildContent, RenderFragmentBuilder("Real content")));
        TestContext.JSInterop.SetupVoid("twSkeleton.unobserve", _ => true)
            .SetException(new JSDisconnectedException("Circuit disconnected"));

        // Act & Assert - should not throw
        cut.Render(p => p.Add(x => x.Loading, false));
        Assert.Contains("Real content", cut.Markup);
    }

    [Fact]
    public void TwSkeleton_StopObserving_SwallowsInvalidOperationException()
    {
        // Arrange
        var cut = TestContext.Render<TwSkeleton>(p => p
            .Add(x => x.Loading, true)
            .Add(x => x.ChildContent, RenderFragmentBuilder("Real content")));
        TestContext.JSInterop.SetupVoid("twSkeleton.unobserve", _ => true)
            .SetException(new InvalidOperationException("JS interop unavailable"));

        // Act & Assert - should not throw
        cut.Render(p => p.Add(x => x.Loading, false));
        Assert.Contains("Real content", cut.Markup);
    }

    #endregion

    #region Disposal (JS interop)

    [Fact]
    public async Task DisposeAsync_UnobservesJsSide_WhenObserving()
    {
        // Arrange
        var cut = TestContext.Render<TwSkeleton>(p => p
            .Add(x => x.Loading, true)
            .Add(x => x.ChildContent, RenderFragmentBuilder("Real content")));

        // Act
        await cut.Instance.DisposeAsync();

        // Assert
        Assert.Contains(TestContext.JSInterop.Invocations, i => i.Identifier == "twSkeleton.unobserve");
    }

    [Fact]
    public async Task DisposeAsync_DoesNothing_WhenNeverObserved()
    {
        // Arrange - standalone placeholder never registers a JS observer.
        var cut = TestContext.Render<TwSkeleton>();

        // Act & Assert - should not throw
        await cut.Instance.DisposeAsync();
        Assert.DoesNotContain(TestContext.JSInterop.Invocations, i => i.Identifier == "twSkeleton.unobserve");
    }

    [Fact]
    public async Task DisposeAsync_SwallowsJSDisconnectedException()
    {
        // Arrange
        var cut = TestContext.Render<TwSkeleton>(p => p
            .Add(x => x.Loading, true)
            .Add(x => x.ChildContent, RenderFragmentBuilder("Real content")));
        TestContext.JSInterop.SetupVoid("twSkeleton.unobserve", _ => true)
            .SetException(new JSDisconnectedException("Circuit disconnected"));

        // Act & Assert - should not throw
        await cut.Instance.DisposeAsync();
        Assert.NotNull(cut.Instance);
    }

    [Fact]
    public async Task DisposeAsync_SwallowsInvalidOperationException()
    {
        // Arrange
        var cut = TestContext.Render<TwSkeleton>(p => p
            .Add(x => x.Loading, true)
            .Add(x => x.ChildContent, RenderFragmentBuilder("Real content")));
        TestContext.JSInterop.SetupVoid("twSkeleton.unobserve", _ => true)
            .SetException(new InvalidOperationException("JS interop unavailable"));

        // Act & Assert - should not throw
        await cut.Instance.DisposeAsync();
        Assert.NotNull(cut.Instance);
    }

    #endregion
}
