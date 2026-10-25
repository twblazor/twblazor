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
        Assert.Contains("dotnet tool install -g TwBlazor.Templates", panel.TextContent);
        Assert.Contains("twblazor new MyApp", panel.TextContent);
        Assert.Contains("dotnet run --project MyApp --launch-profile http", panel.TextContent);
        Assert.DoesNotContain("--twblazor-project", panel.TextContent);
        Assert.DoesNotContain("Until twblazor v3 is published", panel.TextContent);
    }
}
