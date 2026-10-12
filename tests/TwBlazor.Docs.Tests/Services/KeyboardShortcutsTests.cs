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
        Assert.Contains("not interactive", cut.Find(".docs-text").TextContent);
    }

    [Fact]
    public void Card_Summary_UsesTheSameTextStyleAsTheRestOfThePage()
    {
        // Arrange & Act
        var cut = TestContext.Render<KeyboardNavigationCard>(parameters => parameters.Add(p => p.Path, "/tabs"));

        // Assert
        var summary = cut.Find(".docs-text");
        Assert.Equal(KeyboardShortcuts.ForPath("/tabs")!.Summary, summary.TextContent);
        Assert.Contains("text-gray-900", summary.GetAttribute("class"));
        Assert.DoesNotContain("text-gray-600", summary.GetAttribute("class"));
    }

    [Fact]
    public void Card_ActionText_MatchesTheThemeConfigurationTable()
    {
        // Arrange & Act
        var cut = TestContext.Render<KeyboardNavigationCard>(parameters => parameters.Add(p => p.Path, "/tabs"));

        // Assert
        Assert.All(cut.FindAll("tbody td"), cell => Assert.Contains("text-xs text-gray-600 dark:text-gray-400", cell.GetAttribute("class")));
    }

    [Theory]
    [InlineData("Arrow Up", "bi-arrow-up")]
    [InlineData("Arrow Down", "bi-arrow-down")]
    [InlineData("Arrow Left", "bi-arrow-left")]
    [InlineData("Arrow Right", "bi-arrow-right")]
    public void Card_ArrowKeys_ShowAnIcon_AndKeepTheirNameForScreenReaders(string key, string iconClass)
    {
        // Arrange & Act
        var cut = TestContext.Render<KeyboardNavigationCard>(parameters => parameters.Add(p => p.Path, "/tabs"));

        // Assert
        var cap = cut.FindAll("kbd").First(k => k.TextContent == key);
        var icon = cap.QuerySelector("i")!;
        Assert.Contains(iconClass, icon.ClassList);
        Assert.Equal("true", icon.GetAttribute("aria-hidden"));
        Assert.Equal(key, cap.QuerySelector(".sr-only")!.TextContent);
    }

    [Fact]
    public void Card_ShiftCombination_RendersEachKeyAsItsOwnCap_WithAShiftIcon()
    {
        // Arrange & Act
        var cut = TestContext.Render<KeyboardNavigationCard>(parameters => parameters.Add(p => p.Path, "/date-picker"));

        // Assert
        var shift = cut.FindAll("kbd").First(k => k.TextContent == "Shift");
        Assert.Contains("bi-shift", shift.QuerySelector("i")!.ClassList);
        Assert.Equal("Shift plus Tab", ReadAccessibleText(shift.ParentElement!));
        Assert.Equal("true", shift.ParentElement!.QuerySelector("span[aria-hidden]")!.GetAttribute("aria-hidden"));
        Assert.Equal(2, shift.ParentElement.QuerySelectorAll("kbd").Length);
    }

    [Fact]
    public void Card_EnterKey_ShowsAReturnIcon_BeforeItsName()
    {
        // Arrange & Act
        var cut = TestContext.Render<KeyboardNavigationCard>(parameters => parameters.Add(p => p.Path, "/date-picker"));

        // Assert
        var enter = cut.FindAll("kbd").First(k => k.TextContent == "Enter");
        var icon = enter.QuerySelector("i")!;
        Assert.Contains("bi-arrow-return-left", icon.ClassList);
        Assert.Equal("true", icon.GetAttribute("aria-hidden"));
        Assert.Same(icon, enter.FirstChild);
        Assert.Null(enter.QuerySelector("svg"));
    }

    [Fact]
    public void Card_KeyColumn_IsOnlyAsWideAsItsKeys()
    {
        // Arrange & Act
        var cut = TestContext.Render<KeyboardNavigationCard>(parameters => parameters.Add(p => p.Path, "/tabs"));

        // Assert - the descriptions get the rest of the row
        Assert.Contains("w-px", cut.Find("thead th").ClassList);
        Assert.All(cut.FindAll("tbody th > span"), keys => Assert.Contains("sm:flex-nowrap", keys.ClassList));
    }

    [Fact]
    public void Card_AlternativeKeys_AreJoinedWithOr_ForScreenReaders()
    {
        // Arrange & Act
        var cut = TestContext.Render<KeyboardNavigationCard>(parameters => parameters.Add(p => p.Path, "/tabs"));

        // Assert - without the joining word a row header reads as a run of unrelated key names
        var rowHeaders = cut.FindAll("tbody th").Select(ReadAccessibleText).ToList();
        Assert.Contains("Arrow Right or Arrow Down", rowHeaders);
        Assert.Contains("Home or End", rowHeaders);
        Assert.Contains("Tab", rowHeaders);
    }

    [Fact]
    public void Card_KeyCombination_IsReadAsPlus_ForScreenReaders()
    {
        // Arrange & Act
        var cut = TestContext.Render<KeyboardNavigationCard>(parameters => parameters.Add(p => p.Path, "/date-picker"));

        // Assert
        Assert.Contains(cut.FindAll("tbody th").Select(ReadAccessibleText), text => text == "Tab or Shift plus Tab");
    }

    [Fact]
    public void Card_KeyIcons_AreAllHiddenFromScreenReaders()
    {
        // Arrange & Act
        var cut = TestContext.Render<KeyboardNavigationCard>(parameters => parameters.Add(p => p.Path, "/date-picker"));

        // Assert - every key is named in text, so its icon is decoration
        var icons = cut.FindAll("kbd i");
        Assert.NotEmpty(icons);
        Assert.All(icons, icon => Assert.Equal("true", icon.GetAttribute("aria-hidden")));
        Assert.All(cut.FindAll("kbd"), cap => Assert.False(string.IsNullOrWhiteSpace(cap.TextContent)));
    }

    [Fact]
    public void Card_RendersNothing_OnAPageThatIsNotAComponent()
    {
        // Arrange & Act
        var cut = TestContext.Render<KeyboardNavigationCard>(parameters => parameters.Add(p => p.Path, "/get-started"));

        // Assert
        Assert.Equal(string.Empty, cut.Markup.Trim());
    }

    private static string ReadAloud(AngleSharp.Dom.IElement element) =>
        string.Join(' ', element.TextContent.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));

    /// <summary>
    /// The text a screen reader is given for an element: its text content without anything marked <c>aria-hidden</c>.
    /// </summary>
    private static string ReadAccessibleText(AngleSharp.Dom.IElement element)
    {
        var clone = (AngleSharp.Dom.IElement)element.Clone();
        foreach (var hidden in clone.QuerySelectorAll("[aria-hidden='true']"))
        {
            hidden.Remove();
        }

        return ReadAloud(clone);
    }
}
