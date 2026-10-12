using Bunit;
using TwBlazor.Configuration;
using TwBlazor.Enums;
using DirectionProviderPage = TwBlazor.Docs.Pages.DirectionProvider.DirectionProvider;

namespace TwBlazor.Docs.Tests.Pages;

public class DirectionProviderPageTests : DocsTestBase
{
    private IRenderedComponent<DirectionProviderPage> Render() => TestContext.Render<DirectionProviderPage>();

    [Fact]
    public void RightToLeftExample_IsRenderedRightToLeft()
    {
        var cut = Render();

        var example = cut.Find("[dir='rtl']");

        Assert.NotNull(example.QuerySelector("[role='tablist']"));
        Assert.NotNull(example.QuerySelector("nav"));
    }

    [Fact]
    public void MixedExample_NestsALeftToRightIsland_InsideARightToLeftProvider()
    {
        var cut = Render();

        var island = cut.Find("[dir='rtl'] [dir='ltr']");

        Assert.Contains("This alert reads left to right.", island.TextContent);
    }

    [Fact]
    public void Examples_NameOptionsAndValuesThatExist()
    {
        var cut = Render();

        var code = string.Join('\n', cut.FindAll("pre code").Select(block => block.TextContent));

        Assert.Contains($"options.Localization.{nameof(TwLocalizationOptions.Direction)} = {nameof(TextDirection)}.{nameof(TextDirection.RightToLeft)}", code);
        Assert.Contains("<TwDirectionProvider>", code);
        Assert.Contains("<TwToastProvider />", code);
        Assert.Contains("<TwDialogProvider />", code);
        Assert.Contains("Localizer.Direction", code);
    }

    [Fact]
    public void LogicalClassesTable_PairsEachPhysicalClassWithItsLogicalOne()
    {
        var cut = Render();

        var rows = cut.FindAll("table[aria-label='Logical Tailwind classes'] tbody tr")
            .Select(row => (Physical: row.QuerySelector("th code")!.TextContent, Logical: row.QuerySelector("td code")!.TextContent))
            .ToList();

        Assert.NotEmpty(rows);
        Assert.All(rows, row =>
        {
            Assert.Matches("left|right|[mp][lr]-|-[lr](-|,|$)", row.Physical);
            Assert.DoesNotMatch("left|right|[mp][lr]-", row.Logical);
        });
    }

    [Fact]
    public void BehaviourTable_SaysWhatIsNotMirrored()
    {
        var cut = Render();

        var notMirrored = cut.FindAll("table[aria-label='How components behave right to left'] tbody tr")
            .Where(row => row.QuerySelector("td")!.TextContent.StartsWith("Not mirrored.", StringComparison.Ordinal))
            .Select(row => row.QuerySelector("th")!.TextContent)
            .ToList();

        Assert.Equal(["Time pickers", "Colour picker", "Charts"], notMirrored);
    }
}
