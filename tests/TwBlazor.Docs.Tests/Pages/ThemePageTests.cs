using Bunit;
using TwBlazor.Enums;
using DocsTheme = TwBlazor.Docs.Pages.Theme;

namespace TwBlazor.Docs.Tests.Pages;

public class ThemePageTests : DocsTestBase
{
    [Fact]
    public void SwatchWrapper_UsesFixedContrastingBackground_ForLightAndDarkColors()
    {
        // Arrange & Act
        var cut = TestContext.Render<DocsTheme>();

        // Assert - Light/Dark swatches keep a fixed background regardless of page theme, since
        // they're demonstrating what those colors look like on their usual opposite backdrop.
        Assert.Equal("bg-gray-900 p-3 rounded-lg", WrapperClassFor(cut, Color.Light));
        Assert.Equal("bg-white p-3 rounded-lg", WrapperClassFor(cut, Color.Dark));
    }

    [Theory]
    [InlineData(Color.Primary)]
    [InlineData(Color.Accent)]
    [InlineData(Color.Success)]
    [InlineData(Color.Danger)]
    [InlineData(Color.Warning)]
    [InlineData(Color.Info)]
    public void SwatchWrapper_UsesPlainPadding_ForEveryOtherColor(Color color)
    {
        // Arrange & Act
        var cut = TestContext.Render<DocsTheme>();

        // Assert
        Assert.Equal("p-3", WrapperClassFor(cut, color));
    }

    private static string WrapperClassFor(IRenderedComponent<DocsTheme> cut, Color color)
    {
        var button = cut.FindAll("button").Single(b => b.TextContent.Trim() == color.ToString());
        var wrapper = button.ParentElement!;
        return wrapper.GetAttribute("class")!.Replace("flex flex-col items-center gap-2 ", string.Empty).Trim();
    }
}
