using Bunit;
using TwBlazor.Components;
using TwBlazor.Configuration.Components;
using TwBlazor.Enums;

namespace TwBlazor.Tests.Components.Progress;

public class TwProgressTests : TwBlazorTestBase
{
    [Fact]
    public void TwProgress_Renders_WithDefaultValues()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwProgress<int>>(parameters => parameters
            .Add(p => p.Value, 40)
            .Add(p => p.Max, 100));

        // Assert
        var progress = cut.Find("progress");
        Assert.NotNull(progress);
        Assert.Equal("40", progress.GetAttribute("value"));
        Assert.Equal("100", progress.GetAttribute("max"));
    }

    [Fact]
    public void TwProgress_GeneratesId_WhenNotProvided()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwProgress<int>>(parameters => parameters
            .Add(p => p.Value, 40)
            .Add(p => p.Max, 100));

        // Assert
        var progress = cut.Find("progress");
        var id = progress.GetAttribute("id");
        Assert.NotNull(id);
        Assert.StartsWith("progress-", id);
        Assert.DoesNotContain("`", id); // Should not contain generic type indicator
    }

    [Fact]
    public void TwProgress_UsesProvidedId()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwProgress<int>>(parameters => parameters
            .Add(p => p.Id, "upload-progress")
            .Add(p => p.Value, 40)
            .Add(p => p.Max, 100));

        // Assert
        var progress = cut.Find("progress");
        Assert.Equal("upload-progress", progress.GetAttribute("id"));
    }

    [Fact]
    public void TwProgress_GeneratesUniqueIds_ForMultipleInstances()
    {
        // Arrange & Act
        var cut1 = TestContext.Render<TwProgress<int>>(parameters => parameters
            .Add(p => p.Value, 40)
            .Add(p => p.Max, 100));
        var cut2 = TestContext.Render<TwProgress<int>>(parameters => parameters
            .Add(p => p.Value, 40)
            .Add(p => p.Max, 100));

        // Assert
        var id1 = cut1.Find("progress").GetAttribute("id");
        var id2 = cut2.Find("progress").GetAttribute("id");
        Assert.NotEqual(id1, id2);
    }

    [Fact]
    public void TwProgress_RendersLabel_WhenLabelProvided_AndLinksItViaFor()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwProgress<int>>(parameters => parameters
            .Add(p => p.Id, "download-progress")
            .Add(p => p.Label, "Download Progress")
            .Add(p => p.Value, 40)
            .Add(p => p.Max, 100));

        // Assert
        var label = cut.Find("label");
        var progress = cut.Find("progress");
        Assert.Contains("Download Progress", label.TextContent);
        Assert.Equal("download-progress", label.GetAttribute("for"));
        Assert.Equal("download-progress", progress.GetAttribute("id"));
    }

    [Fact]
    public void TwProgress_AppliesCustomLabelClass()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwProgress<int>>(parameters => parameters
            .Add(p => p.Label, "Test Label")
            .Add(p => p.LabelClass, "text-blue-600")
            .Add(p => p.Value, 40)
            .Add(p => p.Max, 100));

        // Assert
        var label = cut.Find("label");
        Assert.Contains("text-blue-600", label.GetAttribute("class"));
    }

    [Fact]
    public void TwProgress_AppliesCustomClass()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwProgress<int>>(parameters => parameters
            .Add(p => p.Class, "custom-progress-class")
            .Add(p => p.Value, 40)
            .Add(p => p.Max, 100));

        // Assert
        var progress = cut.Find("progress");
        Assert.Contains("custom-progress-class", progress.GetAttribute("class"));
    }

    [Fact]
    public void TwProgress_Indeterminate_OmitsValueAttribute()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwProgress<int>>(parameters => parameters
            .Add(p => p.Indeterminate, true)
            .Add(p => p.Max, 100));

        // Assert
        var progress = cut.Find("progress");
        Assert.False(progress.HasAttribute("value"));
    }

    [Fact]
    public void TwProgress_NotIndeterminate_RendersValueAttribute_EvenAtZero()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwProgress<int>>(parameters => parameters
            .Add(p => p.Value, 0)
            .Add(p => p.Max, 100));

        // Assert
        var progress = cut.Find("progress");
        Assert.True(progress.HasAttribute("value"));
        Assert.Equal("0", progress.GetAttribute("value"));
    }

    [Fact]
    public void TwProgress_Indeterminate_AppliesAnimateClass()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwProgress<int>>(parameters => parameters
            .Add(p => p.Indeterminate, true)
            .Add(p => p.Max, 100));

        // Assert
        var progress = cut.Find("progress");
        Assert.Contains("indeterminate:animate-pulse", progress.GetAttribute("class"));
    }

    [Fact]
    public void TwProgress_Animated_AppliesAnimatedThemeClasses()
    {
        // Arrange
        var progressTheme = Theme.Components.Require<TwProgressTheme>();

        // Act
        var cut = TestContext.Render<TwProgress<int>>(parameters => parameters
            .Add(p => p.Animated, true)
            .Add(p => p.Value, 40)
            .Add(p => p.Max, 100));

        // Assert - the shimmer is a decorative overlay sized to the filled portion, not a class on the bar
        var overlay = cut.Find("progress + span");
        Assert.Equal(progressTheme.Animated, overlay.GetAttribute("class"));
        Assert.Contains("tw-wave", overlay.GetAttribute("class"));
        Assert.Equal("width: 40%", overlay.GetAttribute("style"));
        Assert.Equal("true", overlay.GetAttribute("aria-hidden"));
        Assert.DoesNotContain("tw-wave", cut.Find("progress").GetAttribute("class"));
    }

    [Fact]
    public void TwProgress_Animated_SizesOverlayToClampedPercentage()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwProgress<int>>(parameters => parameters
            .Add(p => p.Animated, true)
            .Add(p => p.Value, 150)
            .Add(p => p.Max, 100));

        // Assert
        Assert.Equal("width: 100%", cut.Find("progress + span").GetAttribute("style"));
    }

    [Fact]
    public void TwProgress_NotAnimated_DoesNotRenderShimmerOverlay()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwProgress<int>>(parameters => parameters
            .Add(p => p.Value, 40)
            .Add(p => p.Max, 100));

        // Assert
        Assert.Empty(cut.FindAll("progress + span"));
    }

    [Fact]
    public void TwProgress_Animated_DoesNotRenderShimmerOverlay_WhenIndeterminate()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwProgress<int>>(parameters => parameters
            .Add(p => p.Animated, true)
            .Add(p => p.Indeterminate, true)
            .Add(p => p.Max, 100));

        // Assert
        Assert.Empty(cut.FindAll("progress + span"));
    }

    [Fact]
    public void TwProgress_RendersWrapper_WithThemeClasses()
    {
        // Arrange
        var progressTheme = Theme.Components.Require<TwProgressTheme>();

        // Act
        var cut = TestContext.Render<TwProgress<int>>(parameters => parameters
            .Add(p => p.Value, 40)
            .Add(p => p.Max, 100));

        // Assert
        var wrapper = cut.Find("progress").ParentElement;
        Assert.NotNull(wrapper);
        Assert.Equal(progressTheme.Wrapper, wrapper.GetAttribute("class"));
    }

    [Fact]
    public void TwProgress_Animated_DoesNotChangeValueOrAriaBusy()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwProgress<int>>(parameters => parameters
            .Add(p => p.Animated, true)
            .Add(p => p.Value, 40)
            .Add(p => p.Max, 100));

        // Assert
        var progress = cut.Find("progress");
        Assert.Equal("40", progress.GetAttribute("value"));
        Assert.False(progress.HasAttribute("aria-busy"));
    }

    [Fact]
    public void TwProgress_Indeterminate_DefaultsAriaBusy_ToTrue()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwProgress<int>>(parameters => parameters
            .Add(p => p.Indeterminate, true)
            .Add(p => p.Max, 100));

        // Assert
        var progress = cut.Find("progress");
        Assert.Equal("true", progress.GetAttribute("aria-busy"));
    }

    [Fact]
    public void TwProgress_Determinate_DefaultsAriaBusy_ToFalse()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwProgress<int>>(parameters => parameters
            .Add(p => p.Value, 40)
            .Add(p => p.Max, 100));

        // Assert
        var progress = cut.Find("progress");
        Assert.False(progress.HasAttribute("aria-busy"));
    }

    [Fact]
    public void TwProgress_AriaBusy_ExplicitOverride_TakesPrecedenceOverIndeterminate()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwProgress<int>>(parameters => parameters
            .Add(p => p.Value, 40)
            .Add(p => p.Max, 100)
            .Add(p => p.AriaBusy, true));

        // Assert
        var progress = cut.Find("progress");
        Assert.Equal("true", progress.GetAttribute("aria-busy"));
    }

    [Fact]
    public void TwProgress_AriaBusy_ExplicitFalse_OverridesIndeterminateDefault()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwProgress<int>>(parameters => parameters
            .Add(p => p.Indeterminate, true)
            .Add(p => p.Max, 100)
            .Add(p => p.AriaBusy, false));

        // Assert
        var progress = cut.Find("progress");
        Assert.False(progress.HasAttribute("aria-busy"));
    }

    [Fact]
    public void TwProgress_SetsAriaDescribedBy_WhenProvided()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwProgress<int>>(parameters => parameters
            .Add(p => p.AriaDescribedBy, "progress-help")
            .Add(p => p.Value, 40)
            .Add(p => p.Max, 100));

        // Assert
        var progress = cut.Find("progress");
        Assert.Equal("progress-help", progress.GetAttribute("aria-describedby"));
    }

    [Fact]
    public void TwProgress_AriaDescribedBy_IsNull_WhenNotProvidedAndNotInvalid()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwProgress<int>>(parameters => parameters
            .Add(p => p.Value, 40)
            .Add(p => p.Max, 100));

        // Assert
        var progress = cut.Find("progress");
        Assert.False(progress.HasAttribute("aria-describedby"));
    }

    [Fact]
    public void TwProgress_CombinesAriaDescribedBy_WithErrorId_WhenBothPresent()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwProgress<int>>(parameters => parameters
            .Add(p => p.Id, "my-progress")
            .Add(p => p.AriaDescribedBy, "progress-help")
            .Add(p => p.Invalid, true)
            .Add(p => p.ErrorMessage, "Something went wrong")
            .Add(p => p.Value, 40)
            .Add(p => p.Max, 100));

        // Assert
        var progress = cut.Find("progress");
        Assert.Equal("progress-help my-progress-error", progress.GetAttribute("aria-describedby"));
    }

    [Fact]
    public void TwProgress_SetsAriaInvalid_WhenInvalid()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwProgress<int>>(parameters => parameters
            .Add(p => p.Invalid, true)
            .Add(p => p.ErrorMessage, "Failed")
            .Add(p => p.Value, 40)
            .Add(p => p.Max, 100));

        // Assert
        var progress = cut.Find("progress");
        Assert.Equal("true", progress.GetAttribute("aria-invalid"));

        var error = cut.Find("p[role='alert']");
        Assert.Contains("Failed", error.TextContent);
    }

    [Fact]
    public void TwProgress_DoesNotSetAriaInvalid_WhenNotInvalid()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwProgress<int>>(parameters => parameters
            .Add(p => p.Value, 40)
            .Add(p => p.Max, 100));

        // Assert
        var progress = cut.Find("progress");
        Assert.False(progress.HasAttribute("aria-invalid"));
    }

    [Fact]
    public void TwProgress_Disabled_AppliesDimmedOpacity()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwProgress<int>>(parameters => parameters
            .Add(p => p.Disabled, true)
            .Add(p => p.Value, 40)
            .Add(p => p.Max, 100));

        // Assert
        var progress = cut.Find("progress");
        Assert.Contains("opacity-40", progress.GetAttribute("class"));
    }

    [Fact]
    public void TwProgress_NotDisabled_DoesNotApplyDimmedOpacity()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwProgress<int>>(parameters => parameters
            .Add(p => p.Value, 40)
            .Add(p => p.Max, 100));

        // Assert
        var progress = cut.Find("progress");
        Assert.DoesNotContain("opacity-40", progress.GetAttribute("class"));
    }

    [Theory]
    [InlineData(ProgressSize.Small, "h-1.5")]
    [InlineData(ProgressSize.Medium, "h-2.5")]
    [InlineData(ProgressSize.Large, "h-4")]
    public void TwProgress_AppliesSizeClasses(ProgressSize size, string expectedClass)
    {
        // Arrange & Act
        var cut = TestContext.Render<TwProgress<int>>(parameters => parameters
            .Add(p => p.Size, size)
            .Add(p => p.Value, 40)
            .Add(p => p.Max, 100));

        // Assert
        var progress = cut.Find("progress");
        Assert.Contains(expectedClass, progress.GetAttribute("class"));
    }

    [Theory]
    [InlineData(Color.Danger, "bg-red-600")]
    [InlineData(Color.Accent, "bg-fuchsia-600")]
    [InlineData(Color.Warning, "bg-yellow-500")]
    [InlineData(Color.Success, "bg-green-600")]
    [InlineData(Color.Primary, "bg-purple-600")]
    [InlineData(Color.Info, "bg-blue-600")]
    public void TwProgress_AppliesColorClasses(Color color, string expectedClass)
    {
        // Arrange & Act
        var cut = TestContext.Render<TwProgress<int>>(parameters => parameters
            .Add(p => p.Color, color)
            .Add(p => p.Value, 40)
            .Add(p => p.Max, 100));

        // Assert
        var progress = cut.Find("progress");
        Assert.Contains($"[&::-webkit-progress-value]:{expectedClass}", progress.GetAttribute("class"));
        Assert.Contains($"[&::-moz-progress-bar]:{expectedClass}", progress.GetAttribute("class"));
    }

    [Theory]
    [InlineData(Color.Light)]
    [InlineData(Color.Dark)]
    public void TwProgress_AppliesColorClasses_ForLightAndDarkColors(Color color)
    {
        // Arrange - GetProgressColor's switch expression has a distinct branch per Color value; the
        // Theory above only exercises Danger/Accent/Warning/Success/Primary/Info, leaving Light/Dark
        // untested.
        var progressTheme = Theme.Components.Require<TwProgressTheme>();
        var expected = color == Color.Light ? progressTheme.Colors.Light : progressTheme.Colors.Dark;

        // Act
        var cut = TestContext.Render<TwProgress<int>>(parameters => parameters
            .Add(p => p.Color, color)
            .Add(p => p.Value, 40)
            .Add(p => p.Max, 100));

        // Assert
        var progress = cut.Find("progress");
        Assert.Contains(expected, progress.GetAttribute("class"));
    }

    [Fact]
    public void SizeClasses_FallsBackToMedium_ForUnrecognizedProgressSize()
    {
        // Arrange & Act - a ProgressSize value outside the defined enum members (Small/Medium/Large)
        // falls through to sizeClasses' `_ => theme.Medium` default case.
        var progressTheme = Theme.Components.Require<TwProgressTheme>();
        var cut = TestContext.Render<TwProgress<int>>(parameters => parameters
            .Add(p => p.Size, (ProgressSize)999)
            .Add(p => p.Value, 40)
            .Add(p => p.Max, 100));

        // Assert
        var progress = cut.Find("progress");
        Assert.Contains(progressTheme.Medium, progress.GetAttribute("class"));
    }

    [Fact]
    public void TwProgress_DefaultsToPurple_WhenColorNotProvided()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwProgress<int>>(parameters => parameters
            .Add(p => p.Value, 40)
            .Add(p => p.Max, 100));

        // Assert
        var progress = cut.Find("progress");
        Assert.Contains("[&::-webkit-progress-value]:bg-purple-600", progress.GetAttribute("class"));
    }

    [Fact]
    public void TwProgress_RendersPercentageFallbackText()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwProgress<int>>(parameters => parameters
            .Add(p => p.Value, 25)
            .Add(p => p.Max, 100));

        // Assert
        var progress = cut.Find("progress");
        Assert.Contains("25%", progress.TextContent);
    }

    [Fact]
    public void TwProgress_Indeterminate_RendersLoadingFallbackText()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwProgress<int>>(parameters => parameters
            .Add(p => p.Indeterminate, true)
            .Add(p => p.Max, 100));

        // Assert
        var progress = cut.Find("progress");
        Assert.Contains("Loading", progress.TextContent);
    }

    [Fact]
    public void Percentage_ReturnsZero_WhenMaxIsZero()
    {
        // Arrange & Act - the `if (max <= 0) return 0;` guard
        var cut = TestContext.Render<TwProgress<int>>(parameters => parameters
            .Add(p => p.Value, 5)
            .Add(p => p.Max, 0));

        // Assert
        var progress = cut.Find("progress");
        Assert.Contains("0%", progress.TextContent);
    }

    [Fact]
    public void Percentage_ClampsToHundred_WhenValueExceedsMax()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwProgress<int>>(parameters => parameters
            .Add(p => p.Value, 150)
            .Add(p => p.Max, 100));

        // Assert
        var progress = cut.Find("progress");
        Assert.Contains("100%", progress.TextContent);
    }

    [Fact]
    public void Percentage_ReturnsZero_WhenValuesAreNotConvertibleToDouble()
    {
        // Arrange & Act - Value/Max are strings that can't be parsed as doubles, so Convert.ToDouble
        // throws FormatException, caught by the percentage getter's `when` filter, falling back to 0.
        var cut = TestContext.Render<TwProgress<string>>(parameters => parameters
            .Add(p => p.Value, "abc")
            .Add(p => p.Max, "xyz"));

        // Assert
        var progress = cut.Find("progress");
        Assert.Contains("0%", progress.TextContent);
    }

    [Fact]
    public void TwProgress_WorksWithDecimalValues()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwProgress<decimal>>(parameters => parameters
            .Add(p => p.Value, 12.5m)
            .Add(p => p.Max, 50m));

        // Assert
        var progress = cut.Find("progress");
        Assert.Equal("12.5", progress.GetAttribute("value"));
        Assert.Contains("25%", progress.TextContent);
    }

    [Fact]
    public void TwProgress_WorksWithDoubleValues()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwProgress<double>>(parameters => parameters
            .Add(p => p.Value, 3.0)
            .Add(p => p.Max, 12.0));

        // Assert
        var progress = cut.Find("progress");
        Assert.Equal("3", progress.GetAttribute("value"));
        Assert.Contains("25%", progress.TextContent);
    }

    [Fact]
    public void TwProgress_ForwardsUnmatchedAttributes()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwProgress<int>>(parameters => parameters
            .Add(p => p.Value, 40)
            .Add(p => p.Max, 100)
            .Add(p => p.Attributes, new Dictionary<string, object> { ["data-testid"] = "upload-progress" }));

        // Assert
        var progress = cut.Find("progress");
        Assert.Equal("upload-progress", progress.GetAttribute("data-testid"));
    }
}
