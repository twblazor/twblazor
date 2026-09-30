using TwBlazor.Docs.Services;

namespace TwBlazor.Docs.Tests.Services;

public class XmlDocSummaryTests
{
    // Not read on this (non-browser) code path, but GetSummaryAsync always requires one.
    private static readonly Uri _unusedWasmUri = new("https://example.invalid/TwBlazor.xml");

    private static async Task<IReadOnlyList<DescriptionSegment>> GetSummaryAsync(string propertyName)
    {
        var property = typeof(XmlDocSummaryFixtures).GetProperty(propertyName)
            ?? throw new ArgumentException($"No such property: {propertyName}", nameof(propertyName));
        return await XmlDocSummary.GetSummaryAsync(property, _unusedWasmUri);
    }

    [Fact]
    public async Task GetSummaryAsync_PlainText_ReturnsSingleTextSegment()
    {
        var segments = await GetSummaryAsync(nameof(XmlDocSummaryFixtures.PlainText));

        var segment = Assert.Single(segments);
        Assert.Equal("Plain text only, no markup.", segment.Text);
        Assert.Null(segment.Url);
    }

    [Fact]
    public async Task GetSummaryAsync_NoDocComment_ReturnsEmpty()
    {
        var segments = await GetSummaryAsync(nameof(XmlDocSummaryFixtures.NoSummaryProperty));

        Assert.Empty(segments);
    }

    [Fact]
    public async Task GetSummaryAsync_SeeCrefIntoTwBlazor_LinksToApiDocs()
    {
        var segments = await GetSummaryAsync(nameof(XmlDocSummaryFixtures.LinkToTwBlazorType));

        Assert.Equal(3, segments.Count);
        Assert.Equal(("Links to ", (string?)null), (segments[0].Text, segments[0].Url));
        Assert.Equal("Color", segments[1].Text);
        Assert.Equal($"{XmlDocSummary.ApiBaseUrl}api/TwBlazor.Enums.Color.html", segments[1].Url);
        Assert.Equal((", a type this docs site actually publishes.", (string?)null), (segments[2].Text, segments[2].Url));
    }

    [Fact]
    public async Task GetSummaryAsync_SeeAlsoCrefOutsideTwBlazor_HasNoUrl()
    {
        var segments = await GetSummaryAsync(nameof(XmlDocSummaryFixtures.LinkToExternalType));

        Assert.Equal(3, segments.Count);
        Assert.Equal("See also ", segments[0].Text);
        // A resolvable cref that isn't under the TwBlazor namespace still renders its short name,
        // just without a docs URL - there is no page for it on this site.
        Assert.Equal("String", segments[1].Text);
        Assert.Null(segments[1].Url);
        Assert.Equal(", a BCL type with no page on this site.", segments[2].Text);
    }

    [Fact]
    public async Task GetSummaryAsync_SeeLangword_InlinesTheKeyword()
    {
        var segments = await GetSummaryAsync(nameof(XmlDocSummaryFixtures.LangwordReference));

        var segment = Assert.Single(segments);
        Assert.Equal("Pass null to clear the value.", segment.Text);
        Assert.Null(segment.Url);
    }

    [Fact]
    public async Task GetSummaryAsync_ParamrefAndTypeparamref_InlineTheirNames()
    {
        var segments = await GetSummaryAsync(nameof(XmlDocSummaryFixtures.ParamAndTypeParamRefs));

        var segment = Assert.Single(segments);
        Assert.Equal("The value parameter is echoed via T.", segment.Text);
    }

    [Fact]
    public async Task GetSummaryAsync_CollapsesInternalWhitespaceAndTrimsOuterEdges()
    {
        var segments = await GetSummaryAsync(nameof(XmlDocSummaryFixtures.WhitespaceCollapsing));

        var segment = Assert.Single(segments);
        Assert.Equal("Collapses internal whitespace and trims the outer edges.", segment.Text);
    }

    [Fact]
    public async Task GetSummaryAsync_TextSurroundingALink_PreservesTheSpaceAdjacentToIt()
    {
        var segments = await GetSummaryAsync(nameof(XmlDocSummaryFixtures.LeadingAndTrailingText));

        Assert.Equal(3, segments.Count);
        Assert.Equal("Leading text before ", segments[0].Text);
        Assert.Equal("Color", segments[1].Text);
        // Only the very first/last segment's outer edge is trimmed - this middle-adjacent run keeps
        // its leading space so it doesn't run into the linked word right before it.
        Assert.Equal(" and trailing text after it.", segments[2].Text);
    }

    [Fact]
    public async Task GetSummaryAsync_NestedInlineElements_KeepTheirInnerText()
    {
        var segments = await GetSummaryAsync(nameof(XmlDocSummaryFixtures.NestedInlineElements));

        var segment = Assert.Single(segments);
        Assert.Equal("Wrapped in code and a paragraph tags, whose inner text still counts.", segment.Text);
    }
}
