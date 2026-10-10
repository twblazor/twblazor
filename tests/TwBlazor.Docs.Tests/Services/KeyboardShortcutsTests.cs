using Bunit;
using TwBlazor.Docs.Layout;
using TwBlazor.Docs.Services;

namespace TwBlazor.Docs.Tests.Services;

public class KeyboardShortcutsTests : DocsTestBase
{
    public static TheoryData<string> ComponentPaths =>
        [.. ComponentCatalog.LoadLeafEntries().Select(leaf => leaf.Entry.Url)];

    [Theory]
    [MemberData(nameof(ComponentPaths))]
    public void EveryComponentPage_HasAKeyboardGuide(string path)
    {
        // Act
        var guide = KeyboardShortcuts.ForPath(path);

        // Assert - a new component must say how it is used from the keyboard, or say that it is not interactive
        Assert.NotNull(guide);
        Assert.False(string.IsNullOrWhiteSpace(guide.Summary));
        Assert.All(guide.Shortcuts, shortcut =>
        {
            Assert.NotEmpty(shortcut.Keys);
            Assert.All(shortcut.Keys, key => Assert.False(string.IsNullOrWhiteSpace(key)));
            Assert.EndsWith(".", shortcut.Action);
        });
    }

    [Fact]
    public void EveryDocumentedPath_IsARealComponentPage()
    {
        // Arrange
        var componentPaths = ComponentCatalog.LoadLeafEntries().Select(leaf => leaf.Entry.Url).ToHashSet(StringComparer.OrdinalIgnoreCase);

        // Assert - an entry left behind after a page is renamed or removed would never be shown
        Assert.All(KeyboardShortcuts.DocumentedPaths, path => Assert.Contains(path, componentPaths));
    }

    [Theory]
    [InlineData("/chart")]
    [InlineData("/chart/bar")]
    [InlineData("/CHART/Line")]
    public void EveryChartPage_SharesOneGuide(string path)
    {
        // Assert
        Assert.Same(KeyboardShortcuts.ForPath("/chart"), KeyboardShortcuts.ForPath(path));
    }

    [Theory]
    [InlineData("")]
    [InlineData("/get-started")]
    [InlineData("/charts-are-great")]
    public void PagesThatAreNotComponents_HaveNoGuide(string path)
    {
        // Assert
        Assert.Null(KeyboardShortcuts.ForPath(path));
    }

    [Theory]
    [InlineData("/date-picker")]
    [InlineData("/date-range-picker")]
    [InlineData("/datetime-picker")]
    [InlineData("/datetime-range-picker")]
    [InlineData("/time-picker")]
    [InlineData("/time-range-picker")]
    public void EveryPopoverPicker_DocumentsTheSameKeysForOpeningAndClosing(string path)
    {
        // Arrange
        var guide = KeyboardShortcuts.ForPath(path)!;

        // Assert - the pickers share one base class, so they must describe one way of working
        Assert.Contains(guide.Shortcuts, s => s.Keys.SequenceEqual(["Arrow Down"]) && s.Action.StartsWith("Opens the panel"));
        Assert.Contains(guide.Shortcuts, s => s.Keys.SequenceEqual(["Escape"]) && s.Action.StartsWith("Closes the panel"));
    }

    [Fact]
    public void Card_RendersTheGuideAsATable_NamedForTheComponent()
    {
        // Arrange & Act
        var cut = TestContext.Render<KeyboardNavigationCard>(parameters => parameters
            .Add(p => p.Path, "/tabs")
            .Add(p => p.ComponentTitle, "TwTabContainer"));

        // Assert
        Assert.Equal("Keyboard navigation", cut.Find("h2").TextContent);
        Assert.Equal("TwTabContainer keyboard controls", cut.Find("table").GetAttribute("aria-label"));
        Assert.Equal(KeyboardShortcuts.ForPath("/tabs")!.Shortcuts.Count, cut.FindAll("tbody tr").Count);
        Assert.Contains(cut.FindAll("kbd"), key => key.TextContent == "Home");
        Assert.All(cut.FindAll("tbody th"), header => Assert.Equal("row", header.GetAttribute("scope")));
    }

    [Fact]
    public void Card_ForAComponentWithNoKeys_ExplainsThatInsteadOfShowingAnEmptyTable()
    {
        // Arrange & Act
        var cut = TestContext.Render<KeyboardNavigationCard>(parameters => parameters.Add(p => p.Path, "/avatar"));

        // Assert
        Assert.Empty(cut.FindAll("table"));
        Assert.Contains("not interactive", cut.Find("p").TextContent);
    }

    [Fact]
    public void Card_RendersNothing_OnAPageThatIsNotAComponent()
    {
        // Arrange & Act
        var cut = TestContext.Render<KeyboardNavigationCard>(parameters => parameters.Add(p => p.Path, "/get-started"));

        // Assert
        Assert.Equal(string.Empty, cut.Markup.Trim());
    }
}
