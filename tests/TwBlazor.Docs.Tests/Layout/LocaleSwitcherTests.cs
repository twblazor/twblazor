using Bunit;
using System.Globalization;
using TwBlazor.Docs.Layout;
using TwBlazor.Docs.Services;
using TwBlazor.Enums;

namespace TwBlazor.Docs.Tests.Layout;

public class LocaleSwitcherTests : DocsTestBase
{
    [Fact]
    public void Navbar_HasALanguageAndDirectionButton()
    {
        // Arrange & Act
        var cut = TestContext.Render<Navigation>();

        // Assert
        var trigger = cut.Find("#locale-switcher-trigger");
        Assert.Equal("Language and text direction", trigger.GetAttribute("aria-label"));
        Assert.Equal("dialog", trigger.GetAttribute("aria-haspopup"));
    }

    [Fact]
    public void Opening_ListsEveryShippedLanguage()
    {
        // Arrange
        var cut = TestContext.Render<LocaleSwitcher>();

        // Act
        cut.Find("#locale-switcher-trigger").Click();

        // Assert
        var buttons = cut.FindAll("ul[aria-labelledby='locale-switcher-languages'] button");
        Assert.Equal(DocsLanguages.All.Count, buttons.Count);
        Assert.Contains(buttons, b => b.TextContent.Contains("العربية"));
    }

    [Fact]
    public void ChoosingADirection_SetsItOnTheSharedState()
    {
        // Arrange
        TextDirection? chosen = null;
        var state = new DocsDirectionState(d => { chosen = d; return Task.CompletedTask; });
        var cut = TestContext.Render<LocaleSwitcher>(p => p.AddCascadingValue(state));
        cut.Find("#locale-switcher-trigger").Click();

        // Act
        cut.Find("input[type='radio'][value='rtl']").Change("rtl");

        // Assert
        Assert.Equal(TextDirection.RightToLeft, chosen);
    }

    [Fact]
    public void ChoosingADirection_ClosesThePanel()
    {
        // Arrange
        var state = new DocsDirectionState(_ => Task.CompletedTask);
        var cut = TestContext.Render<LocaleSwitcher>(p => p.AddCascadingValue(state));
        cut.Find("#locale-switcher-trigger").Click();

        // Act
        cut.Find("input[type='radio'][value='ltr']").Change("ltr");

        // Assert
        Assert.Empty(cut.FindAll("#locale-switcher-panel"));
        Assert.Equal("false", cut.Find("#locale-switcher-trigger").GetAttribute("aria-expanded"));
    }

    [Fact]
    public void ChoosingALanguage_SavesItThroughTheScript()
    {
        // Arrange
        var cut = TestContext.Render<LocaleSwitcher>();
        cut.Find("#locale-switcher-trigger").Click();

        // Act
        cut.FindAll("ul[aria-labelledby='locale-switcher-languages'] button")
            .First(b => b.TextContent.Contains("Japanese"))
            .Click();

        // Assert
        var call = TestContext.JSInterop.VerifyInvoke("docsLocale.setCulture");
        Assert.Equal("ja", call.Arguments[0]);
    }

    [Theory]
    [InlineData("es-MX", "es")]
    [InlineData("ar-EG", "ar")]
    [InlineData("zh-Hant-HK", "zh-Hant")]
    [InlineData("nl", "en")]
    [InlineData("", "en")]
    public void Find_MatchesARegionalCultureToItsLanguage(string culture, string expected)
    {
        Assert.Equal(expected, DocsLanguages.Find(CultureInfo.GetCultureInfo(culture)).Code);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("en")]
    [InlineData("xx")]
    public void TryGetCulture_IgnoresTheDefaultAndUnknownCodes(string? code)
    {
        Assert.Null(DocsLanguages.TryGetCulture(code));
    }
}
