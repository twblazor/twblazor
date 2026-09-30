using Bunit;
using Microsoft.AspNetCore.Components;
using TwBlazor.Docs.Layout;
using TwBlazor.Models;
using TwBlazor.Services;

namespace TwBlazor.Docs.Tests.Layout;

public class SearchDisplayTests : DocsTestBase
{
    [Fact]
    public void NoSearchTerm_ShowsPromptInsteadOfResults()
    {
        var cut = TestContext.Render<SearchDisplay>();

        Assert.Contains("Search across TwBlazor components, theme configuration, and API documentation.", cut.Markup);
        Assert.Empty(cut.FindAll("a[href]"));
    }

    [Fact]
    public void Search_NoMatches_ShowsNoResultsMessage()
    {
        var cut = TestContext.Render<SearchDisplay>();

        cut.Find("input").Input("xyz-not-a-real-component");

        Assert.Contains("No results for", cut.Markup);
        Assert.Empty(cut.FindAll("a[href]"));
    }

    [Fact]
    public void Search_ShowsSections_InComponentsThenThemeThenDocumentationOrder()
    {
        var cut = TestContext.Render<SearchDisplay>();

        // "chip" matches the "Chip" component, the "TwChipTheme" theme, and the "TwChip" documentation
        // entry - all three sections should appear, in this fixed hierarchy.
        cut.Find("input").Input("chip");

        var headings = cut.FindAll("h3").Select(h => h.TextContent).ToList();
        Assert.Equal(["Components", "Theme", "API Documentation"], headings);
    }

    [Fact]
    public void Search_ThemeResult_LinksToItsComponentPageWithoutExternalIcon()
    {
        var cut = TestContext.Render<SearchDisplay>();

        cut.Find("input").Input("TwChipTheme");

        var link = cut.Find($"a[href='/chip#{ThemeConfigurationCard.AnchorId}']");
        Assert.Equal("TwChipTheme", link.QuerySelector("span span")!.TextContent);
        Assert.Equal("Theme Configuration", link.QuerySelectorAll("span span")[1].TextContent);
        Assert.Null(link.GetAttribute("target"));
        Assert.Empty(link.QuerySelectorAll("i[aria-label='Opens in a new window']"));
    }

    [Fact]
    public void Search_ComponentResult_LinksInternallyWithoutExternalIcon()
    {
        var cut = TestContext.Render<SearchDisplay>();

        cut.Find("input").Input("chip");

        var link = cut.Find("a[href='/chip']");
        Assert.Equal("Chip", link.QuerySelector("span span")!.TextContent);
        Assert.Null(link.GetAttribute("target"));
        Assert.Empty(link.QuerySelectorAll("i[aria-label='Opens in a new window']"));
    }

    [Fact]
    public void Search_DocumentationResult_OpensInNewWindowWithExternalIcon()
    {
        var cut = TestContext.Render<SearchDisplay>();

        cut.Find("input").Input("TwChip");

        var link = cut.Find("a[href='https://twblazor.github.io/twblazor/api/TwBlazor.Components.TwChip.html']");
        Assert.Equal("_blank", link.GetAttribute("target"));
        Assert.Equal("noopener noreferrer", link.GetAttribute("rel"));
        Assert.NotEmpty(link.QuerySelectorAll("i[aria-label='Opens in a new window']"));
    }

    [Fact]
    public void SelectingAResult_ClosesTheDialog()
    {
        var reference = new FakeDialogReference();
        var dialogInstance = new TwDialogInstance(reference);

        var cut = TestContext.Render<SearchDisplay>(parameters => parameters
            .AddCascadingValue(dialogInstance));
        cut.Find("input").Input("chip");

        cut.Find("a[href='/chip']").Click();

        Assert.True(reference.Closed);
    }

    private sealed class FakeDialogReference : ITwDialogReference
    {
        public bool Closed { get; private set; }

        public Guid Id { get; } = Guid.NewGuid();
        public string? Title => null;
        public TwDialogOptions? Options => null;
        public RenderFragment? RenderFragment { get; set; }
        public Task<TwDialogResult?> Result => Task.FromResult<TwDialogResult?>(null);
        public TaskCompletionSource<bool> RenderCompleteTaskCompletionSource { get; } = new();
        public object? Dialog => null;

        public void Close() => Closed = true;
        public void Close(TwDialogResult? result) => Closed = true;
        public bool Dismiss(TwDialogResult? result) => true;
        public void InjectRenderFragment(RenderFragment renderFragment) { }
        public void InjectDialog(object instance) { }
        public void InjectOptions(TwDialogOptions options) { }
        public void InjectTitle(string? title) { }
        public Task<T?> GetReturnValueAsync<T>() => Task.FromResult<T?>(default);
    }
}
