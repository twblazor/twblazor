using Bunit;
using TwBlazor.Docs.Layout;

namespace TwBlazor.Docs.Tests.Layout;

public class NavigationTests : DocsTestBase
{
    [Fact]
    public void Sidebar_ListsThemeAsATopLevelLink_BetweenGetStartedAndTheComponents()
    {
        // Arrange & Act
        var cut = TestContext.Render<Navigation>();

        // Assert
        var hrefs = cut.FindAll("a").Select(a => a.GetAttribute("href")).ToList();
        var getStarted = hrefs.IndexOf("/get-started");
        var theme = hrefs.IndexOf("/theme");
        var firstComponent = hrefs.IndexOf("/breadcrumb");

        Assert.True(getStarted >= 0 && theme > getStarted);
        Assert.True(firstComponent < 0 || theme < firstComponent);
    }

    [Fact]
    public void Sidebar_GroupsTheIntroductionPagesUnderAnExpandedGetStartedDropdown()
    {
        // Arrange & Act
        var cut = TestContext.Render<Navigation>();

        // Assert
        var toggle = cut.FindAll("nav[aria-label='sidebar navigation'] button[aria-expanded]").First(b => b.TextContent.Trim() == "Get Started");
        Assert.Equal("true", toggle.GetAttribute("aria-expanded"));

        var panel = cut.Find("#" + toggle.GetAttribute("aria-controls"));
        Assert.Equal(
            ["/get-started", "/tailwind-blazor", "/why-twblazor"],
            panel.QuerySelectorAll("a").Select(a => a.GetAttribute("href")));
    }

    [Fact]
    public void Sidebar_LinksToTheTailwindAndWhyTwBlazorGuides_BeforeTheComponents()
    {
        // Arrange & Act
        var cut = TestContext.Render<Navigation>();

        // Assert
        var hrefs = cut.FindAll("a").Select(a => a.GetAttribute("href")).ToList();
        var firstComponent = hrefs.IndexOf("/breadcrumb");
        Assert.True(hrefs.IndexOf("/tailwind-blazor") >= 0);
        Assert.True(hrefs.IndexOf("/why-twblazor") >= 0);
        Assert.True(firstComponent < 0 || hrefs.IndexOf("/why-twblazor") < firstComponent);
    }

    [Fact]
    public void Sidebar_HasNoSeparateThemeBuilderEntry()
    {
        // Arrange & Act
        var cut = TestContext.Render<Navigation>();

        // Assert
        Assert.DoesNotContain(cut.FindAll("a"), a => a.GetAttribute("href") == "/theme-builder");
        Assert.DoesNotContain("Theme customisation", cut.Markup);
    }
}
