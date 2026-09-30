using TwBlazor.Docs.Layout;
using TwBlazor.Docs.Services;

namespace TwBlazor.Docs.Tests.Services;

public class SearchIndexTests
{
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Search_BlankTerm_ReturnsNoResults(string term)
    {
        var (components, themes, documentation) = SearchIndex.Search(term);

        Assert.Empty(components);
        Assert.Empty(themes);
        Assert.Empty(documentation);
    }

    [Fact]
    public void Search_MatchesComponentByTitle_CaseInsensitively()
    {
        var (components, _, _) = SearchIndex.Search("cHiP");

        var result = Assert.Single(components);
        Assert.Equal("Chip", result.Title);
        Assert.Equal("/chip", result.Url);
        Assert.Equal(SearchResultKind.Component, result.Kind);
        Assert.False(result.IsExternal);
    }

    [Fact]
    public void Search_MatchesDocumentationByTypeName_AndLinksToDocfx()
    {
        var (_, _, documentation) = SearchIndex.Search("TwChip");

        var result = Assert.Single(documentation);
        Assert.Equal("TwChip", result.Title);
        Assert.Equal("https://twblazor.github.io/twblazor/api/TwBlazor.Components.TwChip.html", result.Url);
        Assert.Equal(SearchResultKind.Documentation, result.Kind);
        Assert.True(result.IsExternal);
    }

    [Fact]
    public void Search_MatchesThemeByTypeName_AndLinksToTheOwningComponentPage()
    {
        var (_, themes, _) = SearchIndex.Search("TwChipTheme");

        var result = Assert.Single(themes);
        Assert.Equal("TwChipTheme", result.Title);
        Assert.Equal("Theme Configuration", result.Subtitle);
        Assert.Equal($"/chip#{ThemeConfigurationCard.AnchorId}", result.Url);
        Assert.Equal(SearchResultKind.Theme, result.Kind);
        // Theme results link to an internal docs page, not the external docfx site.
        Assert.False(result.IsExternal);
    }

    [Fact]
    public void Search_ThemeSharedBySiblingPages_ReturnsOnlyItsCanonicalPage()
    {
        // TwDatePickerTheme's table appears on /date-picker, /date-range-picker, /datetime-picker and
        // /datetime-range-picker, but only /date-picker (the canonical page) carries "theme" in
        // components.json, so it should surface exactly once rather than once per page.
        var (_, themes, _) = SearchIndex.Search("TwDatePickerTheme");

        var result = Assert.Single(themes);
        Assert.Equal($"/date-picker#{ThemeConfigurationCard.AnchorId}", result.Url);
    }

    [Fact]
    public void Search_ThemeSharedAcrossDifferentComponents_ReturnsItsCanonicalPage()
    {
        // TwInputTheme covers both TwSelect and TwTextfield, but only Textfield's entry carries
        // "theme": "TwInputTheme" - Select's own entry has none, since it just inherits the table.
        var (_, themes, _) = SearchIndex.Search("TwInputTheme");

        var result = Assert.Single(themes);
        Assert.Equal($"/textfield#{ThemeConfigurationCard.AnchorId}", result.Url);
    }

    [Fact]
    public void Search_TermMatchingSeveralComponents_ReturnsAllOfThem()
    {
        // "Date" matches "Date" and "Date Range" (their own display names) as well as "Datetime"
        // and "Datetime Range" (which start with it) - all flattened out from the "Date & Time
        // Pickers" group, whose own group heading is never itself a leaf result.
        var (components, _, _) = SearchIndex.Search("Date");

        Assert.True(components.Count >= 4);
        Assert.Contains(components, c => c.Url == "/date-picker");
        Assert.Contains(components, c => c.Url == "/date-range-picker");
        Assert.DoesNotContain(components, c => c.Title == "Date & Time Pickers");
    }

    [Fact]
    public void Search_NoMatches_ReturnsEmptyLists()
    {
        var (components, themes, documentation) = SearchIndex.Search("xyz-not-a-real-component");

        Assert.Empty(components);
        Assert.Empty(themes);
        Assert.Empty(documentation);
    }

    [Fact]
    public void Search_DocumentationResults_OnlyIncludeTypesThatActuallyExist()
    {
        // "table" matches both the "Table"/"Data Table" component display names and the
        // "TwTable"/"TwDataTable" type names - every documentation result's URL should resolve to a
        // real TwBlazor.Components type, since components.json's "name" field is hand-authored.
        var (components, _, documentation) = SearchIndex.Search("table");

        Assert.NotEmpty(components);
        Assert.NotEmpty(documentation);
        Assert.All(documentation, d => Assert.StartsWith("https://twblazor.github.io/twblazor/api/TwBlazor.Components.", d.Url));
    }

    [Fact]
    public void Search_ThemeResults_OnlyIncludeTypesThatActuallyExist()
    {
        // "theme" is a substring of every theme type name (e.g. "TwChipTheme") - every result's type
        // should resolve under TwBlazor.Configuration.Components, since components.json's "theme"
        // field is hand-authored.
        var (_, themes, _) = SearchIndex.Search("theme");

        Assert.NotEmpty(themes);
        Assert.All(themes, t => Assert.Equal("Theme Configuration", t.Subtitle));
    }
}
