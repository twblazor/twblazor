using Bunit;
using TwBlazor.Components;
using TwBlazor.Enums;

namespace TwBlazor.Tests.Components.Spinner;

public class TwSpinnerTests : TwBlazorTestBase
{
    [Fact]
    public void TwSpinner_Renders_WithDefaultValues()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwSpinner>();

        // Assert
        var root = cut.Find("output");
        Assert.NotNull(root);
        Assert.Contains("animate-spin", cut.Find("span[aria-hidden='true']").GetAttribute("class"));
    }

    [Fact]
    public void TwSpinner_GeneratesId_WhenNotProvided()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwSpinner>();

        // Assert
        var root = cut.Find("output");
        var id = root.GetAttribute("id");
        Assert.NotNull(id);
        Assert.StartsWith("spinner-", id);
        Assert.DoesNotContain("`", id);
    }

    [Fact]
    public void TwSpinner_UsesProvidedId()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwSpinner>(parameters => parameters
            .Add(p => p.Id, "custom-spinner-id"));

        // Assert
        var root = cut.Find("output");
        Assert.Equal("custom-spinner-id", root.GetAttribute("id"));
    }

    [Fact]
    public void TwSpinner_GeneratesUniqueIds_ForMultipleInstances()
    {
        // Arrange & Act
        var cut1 = TestContext.Render<TwSpinner>();
        var cut2 = TestContext.Render<TwSpinner>();

        // Assert
        var id1 = cut1.Find("output").GetAttribute("id");
        var id2 = cut2.Find("output").GetAttribute("id");
        Assert.NotEqual(id1, id2);
    }

    [Fact]
    public void TwSpinner_AppliesCustomClass()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwSpinner>(parameters => parameters
            .Add(p => p.Class, "custom-spinner-class"));

        // Assert
        var root = cut.Find("output");
        Assert.Contains("custom-spinner-class", root.GetAttribute("class"));
    }

    [Fact]
    public void TwSpinner_RendersDefaultLabel_ScreenReaderOnly()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwSpinner>();

        // Assert
        var labelSpans = cut.FindAll("span").Where(s => s.TextContent.Contains("Loading...")).ToList();
        var labelSpan = Assert.Single(labelSpans);
        Assert.Contains("sr-only", labelSpan.GetAttribute("class"));
    }

    [Fact]
    public void TwSpinner_RendersCustomLabel()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwSpinner>(parameters => parameters
            .Add(p => p.Label, "Fetching results..."));

        // Assert
        Assert.Contains("Fetching results...", cut.Markup);
    }

    [Fact]
    public void TwSpinner_ShowLabel_MakesLabelVisible()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwSpinner>(parameters => parameters
            .Add(p => p.Label, "Please wait")
            .Add(p => p.ShowLabel, true));

        // Assert
        var labelSpans = cut.FindAll("span").Where(s => s.TextContent.Contains("Please wait")).ToList();
        var labelSpan = Assert.Single(labelSpans);
        Assert.DoesNotContain("sr-only", labelSpan.GetAttribute("class"));
    }

    [Fact]
    public void TwSpinner_ShowLabel_DefaultsToFalse_LabelIsScreenReaderOnly()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwSpinner>();

        // Assert
        var labelSpans = cut.FindAll("span").Where(s => s.TextContent.Contains("Loading...")).ToList();
        Assert.Contains("sr-only", labelSpans[0].GetAttribute("class"));
    }

    [Theory]
    [InlineData(SpinnerSize.Small, "size-5")]
    [InlineData(SpinnerSize.Medium, "size-8")]
    [InlineData(SpinnerSize.Large, "size-12")]
    public void TwSpinner_AppliesSizeClasses(SpinnerSize size, string expectedClass)
    {
        // Arrange & Act
        var cut = TestContext.Render<TwSpinner>(parameters => parameters
            .Add(p => p.Size, size));

        // Assert
        var indicator = cut.Find("span[aria-hidden='true']");
        Assert.Contains(expectedClass, indicator.GetAttribute("class"));
    }

    [Theory]
    [InlineData(Color.Danger, "border-t-red-600")]
    [InlineData(Color.Accent, "border-t-fuchsia-600")]
    [InlineData(Color.Warning, "border-t-yellow-500")]
    [InlineData(Color.Success, "border-t-green-600")]
    [InlineData(Color.Primary, "border-t-purple-600")]
    [InlineData(Color.Info, "border-t-blue-600")]
    public void TwSpinner_AppliesColorClasses(Color color, string expectedClass)
    {
        // Arrange & Act
        var cut = TestContext.Render<TwSpinner>(parameters => parameters
            .Add(p => p.Color, color));

        // Assert
        var indicator = cut.Find("span[aria-hidden='true']");
        Assert.Contains(expectedClass, indicator.GetAttribute("class"));
    }

    [Fact]
    public void TwSpinner_DefaultsToPurple_WhenColorNotProvided()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwSpinner>();

        // Assert
        var indicator = cut.Find("span[aria-hidden='true']");
        Assert.Contains("border-t-purple-600", indicator.GetAttribute("class"));
    }

    [Theory]
    [InlineData(Color.Danger)]
    [InlineData(Color.Accent)]
    [InlineData(Color.Warning)]
    [InlineData(Color.Success)]
    [InlineData(Color.Primary)]
    [InlineData(Color.Info)]
    public void TwSpinner_UsesSharedNeutralTrack_ForSemanticColors(Color color)
    {
        // Arrange & Act
        var cut = TestContext.Render<TwSpinner>(parameters => parameters
            .Add(p => p.Color, color));

        // Assert
        var classes = cut.Find("span[aria-hidden='true']").GetAttribute("class");
        Assert.Contains("border-[oklch(92%_0_0)]", classes);
        Assert.DoesNotContain("border-white/25", classes);
        Assert.DoesNotContain("border-gray-900/15", classes);
    }

    [Fact]
    public void TwSpinner_ColorLight_UsesWhiteArc_AndItsOwnTrack()
    {
        // Arrange & Act - Light is for placing on a dark surface, so its arc/track must not depend on
        // the shared neutral Track (which is too close to a white arc to read against).
        var cut = TestContext.Render<TwSpinner>(parameters => parameters
            .Add(p => p.Color, Color.Light));

        // Assert
        var classes = cut.Find("span[aria-hidden='true']").GetAttribute("class");
        Assert.Contains("border-t-white", classes);
        Assert.Contains("border-white/25", classes);
        Assert.DoesNotContain("border-gray-200", classes);
    }

    [Fact]
    public void TwSpinner_ColorDark_UsesDarkArc_AndItsOwnTrack()
    {
        // Arrange & Act - Dark is for placing on a light surface, so its arc/track must not depend on
        // the shared neutral Track (which is too close to a near-black arc to read against).
        var cut = TestContext.Render<TwSpinner>(parameters => parameters
            .Add(p => p.Color, Color.Dark));

        // Assert
        var classes = cut.Find("span[aria-hidden='true']").GetAttribute("class");
        Assert.Contains("border-t-gray-900", classes);
        Assert.Contains("border-gray-900/15", classes);
        Assert.DoesNotContain("border-gray-200", classes);
    }

    [Fact]
    public void TwSpinner_AppliesAriaLabel_WhenProvided()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwSpinner>(parameters => parameters
            .Add(p => p.AriaLabel, "Loading dashboard"));

        // Assert
        var root = cut.Find("output");
        Assert.Equal("Loading dashboard", root.GetAttribute("aria-label"));
    }

    [Fact]
    public void TwSpinner_AppliesAriaLabelledBy_WhenProvided()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwSpinner>(parameters => parameters
            .Add(p => p.AriaLabelledBy, "section-heading"));

        // Assert
        var root = cut.Find("output");
        Assert.Equal("section-heading", root.GetAttribute("aria-labelledby"));
    }

    [Fact]
    public void TwSpinner_ForwardsUnmatchedAttributes()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwSpinner>(parameters => parameters
            .AddUnmatched("data-testid", "loading-spinner"));

        // Assert
        var root = cut.Find("output");
        Assert.Equal("loading-spinner", root.GetAttribute("data-testid"));
    }

    [Fact]
    public void TwSpinner_IndicatorIsAriaHidden()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwSpinner>();

        // Assert - the animated indicator is purely decorative, the accessible name comes from the label span
        var indicator = cut.Find("span[aria-hidden='true']");
        Assert.Equal("true", indicator.GetAttribute("aria-hidden"));
    }
}
