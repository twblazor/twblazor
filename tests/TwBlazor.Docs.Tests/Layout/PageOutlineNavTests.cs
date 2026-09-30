using Bunit;
using TwBlazor.Docs.Layout;

namespace TwBlazor.Docs.Tests.Layout;

public class PageOutlineNavTests : DocsTestBase
{
    private static PageOutline OutlineOf(params string[] titles)
    {
        var outline = new PageOutline();

        foreach (var title in titles)
        {
            outline.Register(title);
        }

        return outline;
    }

    private IRenderedComponent<PageOutlineNav> Render(PageOutline outline) =>
        TestContext.Render<PageOutlineNav>(parameters => parameters
            .Add(p => p.Outline, outline)
            .Add(p => p.Path, "/card"));

    [Fact]
    public void Render_ListsEverySectionAsALinkToItsAnchor()
    {
        // Arrange & Act
        var cut = Render(OutlineOf("Basic", "Dense Alerts"));

        // Assert
        var links = cut.FindAll("a");
        Assert.Equal(["Basic", "Dense Alerts"], links.Select(link => link.TextContent));
        Assert.Equal(["/card#basic", "/card#dense-alerts"], links.Select(link => link.GetAttribute("href")));
    }

    [Fact]
    public void Render_LabelsTheNavigationLandmark()
    {
        // Arrange & Act
        var cut = Render(OutlineOf("Basic"));

        // Assert
        var nav = cut.Find("nav");
        Assert.Equal("On this page", cut.Find($"#{nav.GetAttribute("aria-labelledby")}").TextContent);
    }

    [Fact]
    public void Render_IsHiddenBelowTheDesktopBreakpoint()
    {
        // Arrange & Act
        var cut = Render(OutlineOf("Basic"));

        // Assert - hidden by default so phones and tablets never see it, shown again from xl up.
        var classes = cut.Find("nav").GetAttribute("class")!.Split(' ');
        Assert.Contains("hidden", classes);
        Assert.Contains("xl:block", classes);
    }

    [Fact]
    public void Render_ShowsNothing_WhenThePageHasNoSections()
    {
        // Arrange & Act
        var cut = Render(new PageOutline());

        // Assert
        Assert.Empty(cut.FindAll("nav"));
    }

    [Fact]
    public void Render_StartsHighlightingTheSectionBeingRead()
    {
        // Arrange & Act
        Render(OutlineOf("Basic", "Colors"));

        // Assert
        var call = Assert.Single(TestContext.JSInterop.Invocations, invocation => invocation.Identifier == "pageOutline.observe");
        Assert.Equal(["basic", "colors"], call.Arguments.Cast<string[]>().Single());
    }

    [Fact]
    public async Task Dispose_StopsTheScrollListener()
    {
        // Arrange
        Render(OutlineOf("Basic"));

        // Act
        await TestContext.DisposeAsync();

        // Assert
        Assert.Contains(TestContext.JSInterop.Invocations, invocation => invocation.Identifier == "pageOutline.dispose");
    }
}
