using Bunit;
using GetStartedPage = TwBlazor.Docs.Pages.GetStarted;

namespace TwBlazor.Docs.Tests.Pages;

public class GetStartedPageTests : DocsTestBase
{
    [Fact]
    public void ProjectTemplateTab_IsTheDefaultAndShowsTheSingleCommandFlow()
    {
        // Arrange
        var cut = TestContext.Render<GetStartedPage>();

        // Assert - the template is the first, active tab
        var tabs = cut.FindAll("button[role='tab']");
        Assert.Equal("Project template", tabs[0].TextContent.Trim());
        Assert.Equal("true", tabs[0].GetAttribute("aria-selected"));

        var panel = cut.Find("div[role='tabpanel']");
        Assert.Contains("dotnet tool install -g twblazor.templates", panel.TextContent, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("twblazor new MyApp", panel.TextContent);
        Assert.Contains("dotnet run --project MyApp --launch-profile http", panel.TextContent);
        Assert.DoesNotContain("--twblazor-project", panel.TextContent);
        Assert.DoesNotContain("Until twblazor v3 is published", panel.TextContent);
    }

    [Fact]
    public void ProjectTemplateTab_UpdatesTheCommandsFromTheRenderModeAndLayoutCards()
    {
        // Arrange
        var cut = TestContext.Render<GetStartedPage>();

        // Act - choose WebAssembly, then the blank layout
        var webAssembly = cut.Find("div[role='tabpanel']").QuerySelectorAll("button[aria-pressed]")
            .First(button => button.QuerySelector("span.font-semibold")?.TextContent.Trim() == "WebAssembly");
        webAssembly.Click();

        var blank = cut.Find("div[role='tabpanel']").QuerySelectorAll("button[aria-pressed]")
            .First(button => button.QuerySelector("span.font-semibold")?.TextContent.Trim() == "Blank");
        blank.Click();

        // Assert - both the one-line command and the create-only command follow the cards
        Assert.Contains(
            "dotnet tool install -g twblazor.templates; twblazor new MyApp -i wasm -t blank",
            cut.Markup);
        Assert.Contains(
            "twblazor new MyApp -i wasm -t blank",
            cut.Markup);
    }
}
