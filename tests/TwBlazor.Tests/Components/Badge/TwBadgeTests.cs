using Bunit;
using TwBlazor.Components;
using TwBlazor.Configuration.Components;
using TwBlazor.Enums;

namespace TwBlazor.Tests.Components.Badge;

public class TwBadgeTests : TwBlazorTestBase
{
    private TwBadgeTheme theme => Theme.Components.Require<TwBadgeTheme>();

    private static string PaletteValueFor(TwBadgeTheme theme, Color color) => color switch
    {
        Color.Primary => theme.Colors.Primary,
        Color.Accent => theme.Colors.Accent,
        Color.Success => theme.Colors.Success,
        Color.Danger => theme.Colors.Danger,
        Color.Warning => theme.Colors.Warning,
        Color.Info => theme.Colors.Info,
        Color.Light => theme.Colors.Light,
        Color.Dark => theme.Colors.Dark,
        _ => theme.Colors.Primary,
    };

    /// <summary>
    /// The background utility is the first class in a palette entry; the rest are text colors
    /// (and, for <see cref="Color.Dark"/>, a dark-mode background).
    /// </summary>
    private static string BackgroundClassFor(TwBadgeTheme theme, Color color) =>
        PaletteValueFor(theme, color).Split(' ', StringSplitOptions.RemoveEmptyEntries)[0];

    [Fact]
    public void TwBadge_Renders_WithDefaultValues()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwBadge>();

        // Assert
        var wrapper = cut.Find("span");
        Assert.NotNull(wrapper);
    }

    [Fact]
    public void TwBadge_Renders_WithChildContent()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwBadge>(parameters => parameters
            .Add(p => p.ChildContent, RenderFragmentBuilder("Wrapped")));

        // Assert
        var wrapper = cut.Find("span");
        Assert.Contains("Wrapped", wrapper.TextContent);
    }

    [Fact]
    public void TwBadge_Renders_WithCount()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwBadge>(parameters => parameters
            .Add(p => p.Count, 5));

        // Assert
        var badge = cut.Find("span[role='status']");
        Assert.Equal("5", badge.TextContent);
    }

    [Fact]
    public void TwBadge_DefaultColor_IsPrimary()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwBadge>(parameters => parameters
            .Add(p => p.Count, 1));

        // Assert
        var classes = cut.Find("span[role='status']").GetAttribute("class")!;
        Assert.Contains(BackgroundClassFor(theme, Color.Primary), classes);
        Assert.Contains(theme.Colors.Primary, classes);
        // Guards against the old red default creeping back in.
        Assert.Contains("bg-purple-600", classes);
        Assert.DoesNotContain("bg-red-", classes);
        Assert.DoesNotContain(BackgroundClassFor(theme, Color.Danger), classes);
    }

    [Fact]
    public void TwBadge_Count_CapsAtMax_DefaultMax()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwBadge>(parameters => parameters
            .Add(p => p.Count, 150));

        // Assert
        var badge = cut.Find("span[role='status']");
        Assert.Equal("99+", badge.TextContent);
    }

    [Fact]
    public void TwBadge_Count_CapsAtMax_CustomMax()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwBadge>(parameters => parameters
            .Add(p => p.Count, 25)
            .Add(p => p.Max, 10));

        // Assert
        var badge = cut.Find("span[role='status']");
        Assert.Equal("10+", badge.TextContent);
    }

    [Fact]
    public void TwBadge_Count_EqualsMax_IsNotCapped()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwBadge>(parameters => parameters
            .Add(p => p.Count, 99)
            .Add(p => p.Max, 99));

        // Assert
        var badge = cut.Find("span[role='status']");
        Assert.Equal("99", badge.TextContent);
    }

    [Fact]
    public void TwBadge_Renders_WithContent()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwBadge>(parameters => parameters
            .Add(p => p.Content, "New"));

        // Assert
        var badge = cut.Find("span[role='status']");
        Assert.Equal("New", badge.TextContent);
    }

    [Fact]
    public void TwBadge_Content_TakesPrecedenceOverCount()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwBadge>(parameters => parameters
            .Add(p => p.Content, "New")
            .Add(p => p.Count, 5));

        // Assert
        var badge = cut.Find("span[role='status']");
        Assert.Equal("New", badge.TextContent);
        Assert.DoesNotContain("5", badge.TextContent);
    }

    [Fact]
    public void TwBadge_Renders_WithDot()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwBadge>(parameters => parameters
            .Add(p => p.Dot, true));

        // Assert
        Assert.NotNull(cut.Find("span[aria-hidden='true']"));
        Assert.Empty(cut.FindAll("span[role='status']"));
    }

    [Fact]
    public void TwBadge_Dot_TakesPrecedenceOverContentAndCount()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwBadge>(parameters => parameters
            .Add(p => p.Dot, true)
            .Add(p => p.Content, "New")
            .Add(p => p.Count, 5));

        // Assert
        Assert.NotNull(cut.Find("span[aria-hidden='true']"));
        var wrapper = cut.Find("span");
        Assert.DoesNotContain("New", wrapper.TextContent);
        Assert.DoesNotContain("5", wrapper.TextContent);
    }

    [Fact]
    public void TwBadge_DoesNotRenderBadge_WhenEmpty()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwBadge>();

        // Assert
        Assert.Empty(cut.FindAll("span[role='status']"));
        Assert.Empty(cut.FindAll("span[aria-hidden='true']"));
    }

    [Fact]
    public void TwBadge_Dot_IsAriaHidden()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwBadge>(parameters => parameters
            .Add(p => p.Dot, true));

        // Assert
        var dot = cut.Find("span[aria-hidden='true']");
        Assert.Equal("true", dot.GetAttribute("aria-hidden"));
    }

    [Fact]
    public void TwBadge_Count_HasStatusRole()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwBadge>(parameters => parameters
            .Add(p => p.Count, 3));

        // Assert
        var badge = cut.Find("span[role='status']");
        Assert.Equal("status", badge.GetAttribute("role"));
    }

    [Theory]
    [InlineData(BadgePlacement.TopEnd)]
    [InlineData(BadgePlacement.TopStart)]
    [InlineData(BadgePlacement.BottomEnd)]
    [InlineData(BadgePlacement.BottomStart)]
    public void TwBadge_Placement_ForEach(BadgePlacement placement)
    {
        // Arrange
        var isTop = placement is BadgePlacement.TopStart or BadgePlacement.TopEnd;
        var isStart = placement is BadgePlacement.TopStart or BadgePlacement.BottomStart;

        var expectedVerticalInset = isTop ? "top-(--badge-inset)" : "bottom-(--badge-inset)";
        var expectedHorizontalInset = isStart ? "start-(--badge-inset)" : "end-(--badge-inset)";
        var expectedHorizontalTranslate = isStart ? "-translate-x-1/3" : "translate-x-1/3";
        var expectedVerticalTranslate = isTop ? "-translate-y-1/3" : "translate-y-1/3";

        // Act
        var cut = TestContext.Render<TwBadge>(parameters => parameters
            .Add(p => p.Count, 1)
            .Add(p => p.Placement, placement));

        // Assert
        var badge = cut.Find("span[role='status']");
        var classes = badge.GetAttribute("class")!;
        Assert.Contains(expectedVerticalInset, classes);
        Assert.Contains(expectedHorizontalInset, classes);
        Assert.Contains(expectedHorizontalTranslate, classes);
        Assert.Contains(expectedVerticalTranslate, classes);

        // Exact tokens (the class list also holds the RTL mirror, e.g. rtl:-translate-x-1/3,
        // whose text contains the opposite x-translate), so compare whole class names.
        var classList = badge.ClassList;
        Assert.Contains(expectedHorizontalTranslate, classList);
        Assert.Contains(expectedVerticalTranslate, classList);
        Assert.DoesNotContain(isStart ? "translate-x-1/3" : "-translate-x-1/3", classList);
        Assert.DoesNotContain(isTop ? "translate-y-1/3" : "-translate-y-1/3", classList);
    }

    [Theory]
    [InlineData(Color.Primary)]
    [InlineData(Color.Accent)]
    [InlineData(Color.Success)]
    [InlineData(Color.Danger)]
    [InlineData(Color.Warning)]
    [InlineData(Color.Info)]
    [InlineData(Color.Light)]
    [InlineData(Color.Dark)]
    public void TwBadge_Renders_WithColor(Color color)
    {
        // Arrange
        var expected = BackgroundClassFor(theme, color);

        // Act
        var cut = TestContext.Render<TwBadge>(parameters => parameters
            .Add(p => p.Count, 1)
            .Add(p => p.Color, color));

        // Assert
        var badge = cut.Find("span[role='status']");
        Assert.Contains(expected, badge.GetAttribute("class"));
        Assert.Contains(expected, badge.ClassList);
    }

    [Fact]
    public void TwBadge_Renders_WithId()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwBadge>(parameters => parameters
            .Add(p => p.Id, "test-badge"));

        // Assert
        var wrapper = cut.Find("span");
        Assert.Equal("test-badge", wrapper.GetAttribute("id"));
    }

    [Fact]
    public void TwBadge_Renders_WithClass()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwBadge>(parameters => parameters
            .Add(p => p.Class, "custom-class"));

        // Assert
        var wrapper = cut.Find("span");
        Assert.Contains("custom-class", wrapper.GetAttribute("class"));
    }

    [Fact]
    public void TwBadge_Renders_WithStyle()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwBadge>(parameters => parameters
            .Add(p => p.Style, "color: red;"));

        // Assert
        var wrapper = cut.Find("span");
        Assert.Contains("color: red", wrapper.GetAttribute("style"));
    }

    [Fact]
    public void TwBadge_Renders_WithUnmatchedAttributes()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwBadge>(parameters => parameters
            .AddUnmatched("data-testid", "badge-test"));

        // Assert
        Assert.NotNull(cut.Find("[data-testid='badge-test']"));
    }

    [Fact]
    public void TwBadge_Renders_WithAriaLabel()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwBadge>(parameters => parameters
            .Add(p => p.Count, 3)
            .Add(p => p.AriaLabel, "3 unread"));

        // Assert
        var badge = cut.Find("span[role='status']");
        Assert.Equal("3 unread", badge.GetAttribute("aria-label"));
    }
}
