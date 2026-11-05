using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using TwBlazor.Docs.Layout;
using TwBlazor.Docs.Services;

namespace TwBlazor.Docs.Tests.Layout;

public class ComponentsMenuTests : DocsTestBase
{
    [Fact]
    public void Trigger_IsHiddenBelowTheDesktopBreakpoint_AndStartsClosed()
    {
        // Arrange & Act
        var cut = TestContext.Render<ComponentsMenu>();

        // Assert
        var root = cut.Find("#components-menu");
        Assert.Contains("hidden", root.ClassList);
        Assert.Contains("lg:inline-flex", root.ClassList);
        Assert.Equal("false", cut.Find("#components-menu-trigger").GetAttribute("aria-expanded"));
        Assert.Empty(cut.FindAll("#components-menu-panel"));
    }

    [Fact]
    public void Opening_ShowsATileForEveryCategory_WithItsComponentCount()
    {
        // Arrange
        var cut = TestContext.Render<ComponentsMenu>();
        var categories = ComponentCatalog.LoadCategories();

        // Act
        cut.Find("#components-menu-trigger").Click();

        // Assert
        Assert.Equal("true", cut.Find("#components-menu-trigger").GetAttribute("aria-expanded"));
        var tiles = cut.FindAll("[id^='components-menu-tile-']");
        Assert.Equal(categories.Count, tiles.Count);
        var forms = cut.Find("#components-menu-tile-forms");
        var formsCount = ComponentCatalog.LoadLeafEntries().Count(entry => entry.Category == "Forms");
        Assert.Contains($"{formsCount} components", forms.TextContent);
    }

    [Fact]
    public void ChoosingACategory_ListsItsComponents_AndBackReturnsToTheTiles()
    {
        // Arrange
        var cut = TestContext.Render<ComponentsMenu>();
        cut.Find("#components-menu-trigger").Click();

        // Act
        cut.Find("#components-menu-tile-forms").Click();

        // Assert
        Assert.NotNull(cut.Find("#components-menu-body a[href='/textfield']"));
        Assert.Empty(cut.FindAll("[id^='components-menu-tile-']"));

        cut.Find("#components-menu-body button[aria-label='All categories']").Click();
        Assert.NotEmpty(cut.FindAll("[id^='components-menu-tile-']"));
    }

    [Fact]
    public void Searching_ListsMatchingComponents_OrSaysThereAreNone()
    {
        // Arrange
        var cut = TestContext.Render<ComponentsMenu>();
        cut.Find("#components-menu-trigger").Click();

        // Act
        cut.Find("#components-menu-search").Input("date pick");

        // Assert
        var links = cut.FindAll("#components-menu-body a");
        Assert.NotEmpty(links);
        Assert.All(links, link => Assert.Contains("date", link.GetAttribute("href"), StringComparison.Ordinal));

        cut.Find("#components-menu-search").Input("zzzz");
        Assert.Contains("No components match your search.", cut.Find("#components-menu-body").TextContent);
    }

    [Fact]
    public void FollowingALink_ClosesThePanel()
    {
        // Arrange
        var cut = TestContext.Render<ComponentsMenu>();
        cut.Find("#components-menu-trigger").Click();

        // Act
        TestContext.Services.GetRequiredService<NavigationManager>().NavigateTo("/button");

        // Assert
        cut.WaitForAssertion(() => Assert.Empty(cut.FindAll("#components-menu-panel")));
    }
}
