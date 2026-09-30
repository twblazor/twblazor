using TwBlazor.Docs.Services;

namespace TwBlazor.Docs.Tests.Services;

public class ComponentCatalogTests
{
    [Fact]
    public void LoadCategories_ReturnsNonEmptyCategoriesFromEmbeddedJson()
    {
        var categories = ComponentCatalog.LoadCategories();

        Assert.NotEmpty(categories);
        Assert.Contains(categories, c => c.Category == "Forms");
    }

    [Fact]
    public void LoadLeafEntries_ExpandsGroupEntries_RatherThanReturningTheGroupItself()
    {
        // "Date & Time Pickers" is a group under Forms with no Url/Name of its own - only its
        // children (TwDatePicker, TwTimePicker, etc.) are real, linkable components.
        var entries = ComponentCatalog.LoadLeafEntries().ToList();

        Assert.DoesNotContain(entries, e => e.Entry.Display == "Date & Time Pickers");
        Assert.Contains(entries, e => e.Entry.Name == "TwDatePicker" && e.Entry.Url == "/date-picker");
        Assert.Contains(entries, e => e.Entry.Name == "TwTimeRangePicker" && e.Entry.Url == "/time-range-picker");
    }

    [Fact]
    public void LoadLeafEntries_CarriesTheOwningCategoryAlongsideEachEntry()
    {
        var entries = ComponentCatalog.LoadLeafEntries().ToList();

        var chip = Assert.Single(entries, e => e.Entry.Name == "TwChip");
        Assert.Equal("Feedback", chip.Category);
    }

    [Fact]
    public void LoadLeafEntries_EveryLeafHasAUrl()
    {
        var entries = ComponentCatalog.LoadLeafEntries().ToList();

        Assert.NotEmpty(entries);
        Assert.All(entries, e => Assert.False(string.IsNullOrEmpty(e.Entry.Url)));
    }

    [Fact]
    public void LoadLeafEntries_RecordsTheThemeType_OnlyOnItsCanonicalPage()
    {
        var entries = ComponentCatalog.LoadLeafEntries().ToList();

        var chip = Assert.Single(entries, e => e.Entry.Name == "TwChip");
        Assert.Equal("TwChipTheme", chip.Entry.Theme);

        // TwInputTheme's table lives on /textfield - /select just reuses it, so its own entry
        // carries no "theme" value to avoid the same theme surfacing twice in search.
        var textfield = Assert.Single(entries, e => e.Entry.Name == "TwTextfield");
        Assert.Equal("TwInputTheme", textfield.Entry.Theme);

        var select = Assert.Single(entries, e => e.Entry.Name == "TwSelect");
        Assert.Empty(select.Entry.Theme);
    }
}
