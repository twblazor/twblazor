using Bunit;
using Microsoft.JSInterop;
using TwBlazor.Components;
using TwBlazor.Models;

namespace TwBlazor.Tests.Components.OnThisPage;

public class TwOnThisPageTests : TwBlazorTestBase
{
    private static readonly IReadOnlyList<OnThisPageItem> _items =
    [
        new("basic", "Basic"),
        new("dense-alerts", "Dense Alerts")
    ];

    private IRenderedComponent<TwOnThisPage> Render(Action<ComponentParameterCollectionBuilder<TwOnThisPage>>? configure = null) =>
        TestContext.Render<TwOnThisPage>(parameters =>
        {
            parameters.Add(p => p.Items, _items);
            configure?.Invoke(parameters);
        });

    [Fact]
    public void Render_ListsEveryItemAsALinkToItsSection()
    {
        // Arrange & Act
        var cut = Render();

        // Assert
        var links = cut.FindAll("nav ul li a");
        Assert.Equal(["Basic", "Dense Alerts"], links.Select(link => link.TextContent));
        Assert.Equal(["#basic", "#dense-alerts"], links.Select(link => link.GetAttribute("href")));
        Assert.Equal(["basic", "dense-alerts"], links.Select(link => link.GetAttribute("data-tw-section-id")));
    }

    [Fact]
    public void Render_PrefixesLinksWithTheBasePath()
    {
        // Arrange & Act
        var cut = Render(parameters => parameters.Add(p => p.BasePath, "/card"));

        // Assert
        Assert.Equal(["/card#basic", "/card#dense-alerts"], cut.FindAll("a").Select(link => link.GetAttribute("href")));
    }

    [Fact]
    public void Render_HidesTheLandmark_WhenThereAreNoItems()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwOnThisPage>();

        // Assert - an empty navigation landmark must not be shown or announced
        var nav = cut.Find("nav");
        Assert.True(nav.HasAttribute("hidden"));
        Assert.Empty(nav.Children);
        Assert.DoesNotContain(TestContext.JSInterop.Invocations, invocation => invocation.Identifier == "twOnThisPage.observe");
    }

    [Fact]
    public void Render_NamesTheNavigationLandmarkAfterItsHeading()
    {
        // Arrange & Act
        var cut = Render();

        // Assert
        var nav = cut.Find("nav");
        var heading = cut.Find($"#{nav.GetAttribute("aria-labelledby")}");
        Assert.Equal("H2", heading.TagName);
        Assert.Equal("On this page", heading.TextContent);
        Assert.False(nav.HasAttribute("aria-label"));
        Assert.False(nav.HasAttribute("hidden"));
    }

    [Fact]
    public void Render_UsesTheGivenTitle()
    {
        // Arrange & Act
        var cut = Render(parameters => parameters.Add(p => p.Title, "Contents"));

        // Assert
        Assert.Equal("Contents", cut.Find("h2").TextContent);
    }

    [Theory]
    [InlineData(1, "H1")]
    [InlineData(3, "H3")]
    [InlineData(6, "H6")]
    [InlineData(0, "H1")]
    [InlineData(9, "H6")]
    public void Render_UsesTheGivenHeadingLevel_ClampedToAValidHeading(int level, string expectedTag)
    {
        // Arrange & Act
        var cut = Render(parameters => parameters.Add(p => p.HeadingLevel, level));

        // Assert
        var nav = cut.Find("nav");
        Assert.Equal(expectedTag, cut.Find($"#{nav.GetAttribute("aria-labelledby")}").TagName);
    }

    [Fact]
    public void Render_WithHiddenTitle_NamesTheLandmarkWithAriaLabelInstead()
    {
        // Arrange & Act
        var cut = Render(parameters => parameters.Add(p => p.HideTitle, true));

        // Assert - with no heading to point at, the landmark would otherwise be unnamed
        var nav = cut.Find("nav");
        Assert.Empty(cut.FindAll("h2"));
        Assert.Equal("On this page", nav.GetAttribute("aria-label"));
        Assert.False(nav.HasAttribute("aria-labelledby"));
    }

    [Fact]
    public void Render_WithHiddenTitle_UsesTheGivenTitleAsTheLabel()
    {
        // Arrange & Act
        var cut = Render(parameters => parameters
            .Add(p => p.HideTitle, true)
            .Add(p => p.Title, "Contents"));

        // Assert
        Assert.Equal("Contents", cut.Find("nav").GetAttribute("aria-label"));
    }

    [Fact]
    public void Render_PrefersAnExplicitAriaLabel()
    {
        // Arrange & Act
        var cut = Render(parameters => parameters.Add(p => p.AriaLabel, "Sections"));

        // Assert - one name only: aria-labelledby would otherwise win over the label the consumer asked for
        var nav = cut.Find("nav");
        Assert.Equal("Sections", nav.GetAttribute("aria-label"));
        Assert.False(nav.HasAttribute("aria-labelledby"));
    }

    [Fact]
    public void Render_PrefersAnExplicitAriaLabelledBy()
    {
        // Arrange & Act
        var cut = Render(parameters => parameters
            .Add(p => p.HideTitle, true)
            .Add(p => p.AriaLabelledBy, "my-heading"));

        // Assert
        var nav = cut.Find("nav");
        Assert.Equal("my-heading", nav.GetAttribute("aria-labelledby"));
        Assert.False(nav.HasAttribute("aria-label"));
    }

    [Fact]
    public void Render_LocalizesTheDefaultTitle()
    {
        // Arrange
        using var context = new BunitContext();
        context.JSInterop.Mode = JSRuntimeMode.Loose;
        context.Services.AddTwBlazor(options => options.Localization.Culture = System.Globalization.CultureInfo.GetCultureInfo("de"), TwBlazor.Theme.Theme.CreateDefaultTheme());

        // Act
        var cut = context.Render<TwOnThisPage>(parameters => parameters.Add(p => p.Items, _items));

        // Assert
        Assert.Equal("Auf dieser Seite", cut.Find("h2").TextContent);
    }

    [Fact]
    public void Render_AppliesIdClassStyleAndExtraAttributes()
    {
        // Arrange & Act
        var cut = Render(parameters => parameters
            .Add(p => p.Id, "toc")
            .Add(p => p.Class, "sticky top-6")
            .Add(p => p.Style, "width: 14rem")
            .AddUnmatched("data-testid", "outline"));

        // Assert
        var nav = cut.Find("nav");
        Assert.Equal("toc", nav.Id);
        Assert.Equal("toc-heading", cut.Find("h2").Id);
        Assert.Contains("sticky", nav.ClassList);
        Assert.Contains("text-sm", nav.ClassList);
        Assert.Equal("width: 14rem", nav.GetAttribute("style"));
        Assert.Equal("outline", nav.GetAttribute("data-testid"));
    }

    [Fact]
    public void Render_Links_ShowThePrimaryFocusRing_AndStyleTheCurrentSection()
    {
        // Arrange & Act
        var cut = Render();

        // Assert
        Assert.All(cut.FindAll("a"), link =>
        {
            Assert.Contains("focus-visible:ring-2", link.ClassList);
            Assert.Contains("focus-visible:ring-purple-500", link.ClassList);
            Assert.Contains("focus:outline-hidden", link.ClassList);
            Assert.Contains("aria-[current=location]:border-purple-600", link.ClassList);
            Assert.Contains("forced-colors:aria-[current=location]:border-[Highlight]", link.ClassList);
        });
    }

    [Fact]
    public void Render_StartsHighlightingTheSectionBeingRead()
    {
        // Arrange & Act
        var cut = Render(parameters => parameters.Add(p => p.Id, "toc"));

        // Assert
        var call = Assert.Single(TestContext.JSInterop.Invocations, invocation => invocation.Identifier == "twOnThisPage.observe");
        Assert.Equal("toc", call.Arguments[0]);
        Assert.Equal(["basic", "dense-alerts"], Assert.IsType<string[]>(call.Arguments[2]));
        Assert.Equal(140, call.Arguments[3]);
    }

    [Fact]
    public void Render_PassesTheOffsetToTheScrollListener()
    {
        // Arrange & Act
        Render(parameters => parameters.Add(p => p.Offset, 240));

        // Assert
        var call = Assert.Single(TestContext.JSInterop.Invocations, invocation => invocation.Identifier == "twOnThisPage.observe");
        Assert.Equal(240, call.Arguments[3]);
    }

    [Fact]
    public void Rerender_WithTheSameItems_DoesNotRegisterAgain()
    {
        // Arrange
        var cut = Render();

        // Act
        cut.Render(parameters => parameters.Add(p => p.Title, "Contents"));

        // Assert
        Assert.Single(TestContext.JSInterop.Invocations, invocation => invocation.Identifier == "twOnThisPage.observe");
    }

    [Fact]
    public void Rerender_WithDifferentItems_RegistersTheNewSections()
    {
        // Arrange
        var cut = Render();

        // Act
        cut.Render(parameters => parameters.Add(p => p.Items, [new OnThisPageItem("colors", "Colors")]));

        // Assert
        var calls = TestContext.JSInterop.Invocations.Where(invocation => invocation.Identifier == "twOnThisPage.observe").ToList();
        Assert.Equal(2, calls.Count);
        Assert.Equal(["colors"], Assert.IsType<string[]>(calls[1].Arguments[2]));
    }

    [Fact]
    public void Rerender_WithNoItems_StopsTheScrollListener()
    {
        // Arrange
        var cut = Render();

        // Act
        cut.Render(parameters => parameters.Add(p => p.Items, []));

        // Assert
        Assert.True(cut.Find("nav").HasAttribute("hidden"));
        Assert.Empty(cut.FindAll("a"));
        Assert.Single(TestContext.JSInterop.Invocations, invocation => invocation.Identifier == "twOnThisPage.dispose");
    }

    [Fact]
    public void Render_SwallowsJSDisconnectedException_WhenRegistering()
    {
        // Arrange
        TestContext.JSInterop.SetupVoid("twOnThisPage.observe", _ => true)
            .SetException(new JSDisconnectedException("Circuit disconnected"));

        // Act
        var cut = Render();

        // Assert
        Assert.NotNull(cut.Find("nav"));
    }

    [Fact]
    public void Render_SwallowsTaskCanceledException_WhenRegistering()
    {
        // Arrange
        TestContext.JSInterop.SetupVoid("twOnThisPage.observe", _ => true)
            .SetException(new TaskCanceledException());

        // Act
        var cut = Render();

        // Assert
        Assert.NotNull(cut.Find("nav"));
    }

    [Fact]
    public async Task DisposeAsync_StopsTheScrollListener()
    {
        // Arrange
        var cut = Render(parameters => parameters.Add(p => p.Id, "toc"));

        // Act
        await cut.Instance.DisposeAsync();

        // Assert
        var call = Assert.Single(TestContext.JSInterop.Invocations, invocation => invocation.Identifier == "twOnThisPage.dispose");
        Assert.Equal("toc", call.Arguments[0]);
    }

    [Fact]
    public async Task DisposeAsync_DoesNothing_WhenNothingWasRegistered()
    {
        // Arrange
        var cut = TestContext.Render<TwOnThisPage>();

        // Act
        await cut.Instance.DisposeAsync();

        // Assert
        Assert.DoesNotContain(TestContext.JSInterop.Invocations, invocation => invocation.Identifier == "twOnThisPage.dispose");
    }

    [Theory]
    [InlineData(typeof(JSDisconnectedException))]
    [InlineData(typeof(InvalidOperationException))]
    public async Task DisposeAsync_SwallowsExceptionsFromATornDownRuntime(Type exceptionType)
    {
        // Arrange
        var cut = Render();
        var exception = exceptionType == typeof(JSDisconnectedException)
            ? new JSDisconnectedException("Circuit disconnected")
            : (Exception)new InvalidOperationException("JS interop unavailable");
        TestContext.JSInterop.SetupVoid("twOnThisPage.dispose", _ => true).SetException(exception);

        // Act
        var thrown = await Record.ExceptionAsync(async () => await cut.Instance.DisposeAsync());

        // Assert
        Assert.Null(thrown);
    }
}
