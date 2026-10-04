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
    public void Sidebar_HasNoSeparateThemeBuilderEntry()
    {
        // Arrange & Act
        var cut = TestContext.Render<Navigation>();

        // Assert
        Assert.DoesNotContain(cut.FindAll("a"), a => a.GetAttribute("href") == "/theme-builder");
        Assert.DoesNotContain("Theme customisation", cut.Markup);
    }
}
