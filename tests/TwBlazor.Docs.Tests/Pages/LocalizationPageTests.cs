using Bunit;
using System.Globalization;
using System.Resources;
using TwBlazor.Configuration;
using TwBlazor.Localization;
using LocalizationPage = TwBlazor.Docs.Pages.Localization.Localization;

namespace TwBlazor.Docs.Tests.Pages;

public class LocalizationPageTests : DocsTestBase
{
    private static readonly ResourceManager _resources = new("TwBlazor.Resources.TwBlazorStrings", typeof(TwLocalizer).Assembly);

    private IRenderedComponent<LocalizationPage> Render() => TestContext.Render<LocalizationPage>();

    [Fact]
    public void LanguagesTable_ListsEnglishAndEveryShippedLanguage_AndEachOneHasAResourceFile()
    {
        var cut = Render();

        var cultures = cut.FindAll("table[aria-label='Supported languages'] tbody tr td:nth-of-type(1) code")
            .Select(code => code.TextContent)
            .ToList();

        Assert.Equal(["en", "zh-Hans", "es", "hi", "pt", "bn", "ru", "ja", "pa-Arab", "tr", "vi", "zh-Hant", "ar", "mr", "te", "ko", "ta", "ur", "id", "de", "fr", "jv", "fa", "it"], cultures);
        Assert.All(cultures.Skip(1), culture =>
            Assert.NotNull(_resources.GetResourceSet(CultureInfo.GetCultureInfo(culture), true, false)));
    }

    [Fact]
    public void Page_SaysTheTranslationsWereProducedByAi_AndLinksToContributing()
    {
        var cut = Render();

        var link = cut.Find("a[href$='#localization-contributing']");

        Assert.Contains("produced by AI", link.ParentElement!.TextContent);
        Assert.Single(cut.FindAll("[id='localization-contributing']"));
    }

    [Fact]
    public void FormatsTable_NamesOptionsThatExist()
    {
        var cut = Render();

        var options = cut.FindAll("table[aria-label='Date and time format options'] tbody tr th code")
            .SelectMany(code => code.TextContent.Split(", "))
            .ToList();

        Assert.NotEmpty(options);
        Assert.All(options, option => Assert.NotNull(typeof(TwDateTimeFormatOptions).GetProperty(option)));
    }

    [Fact]
    public void Examples_ConfigureLocalizationThroughAddTwBlazor()
    {
        var cut = Render();

        var code = string.Join('\n', cut.FindAll("pre code").Select(block => block.TextContent));

        Assert.Contains("options.Localization.AddResource(", code);
        Assert.Contains("options.Localization.Formats.FirstDayOfWeek", code);
        Assert.Contains("options.Localization.Culture", code);
        Assert.Contains("ITwLocalizer", code);
    }

    [Fact]
    public void ResourceKeysNamedOnThePage_Exist()
    {
        var cut = Render();

        var keys = cut.FindAll("p code, div code")
            .Select(code => code.TextContent)
            .Where(text => text.StartsWith("Pagination_", StringComparison.Ordinal)
                || text.StartsWith("Sidebar_", StringComparison.Ordinal)
                || text.StartsWith("Format_", StringComparison.Ordinal))
            .Distinct()
            .ToList();

        Assert.NotEmpty(keys);
        Assert.All(keys, key => Assert.NotNull(typeof(TwStrings).GetField(key)));
    }
}
