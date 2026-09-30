using AngleSharp.Dom;
using Bunit;
using Microsoft.AspNetCore.Components;
using TwBlazor.Configuration.Components;
using TwBlazor.Docs.Layout;

namespace TwBlazor.Docs.Tests.Layout;

public class ThemeConfigurationCardTests : DocsTestBase
{
    [Fact]
    public void FriendlyTypeName_CoversPrimitivesNullablesEnumsAndGlobalTokenTypes()
    {
        // Arrange & Act
        var cut = TestContext.Render<ThemeConfigurationCard>(parameters => parameters
            .Add(p => p.ThemeType, typeof(ThemeConfigurationCardTestTheme)));
        cut.WaitForState(() => cut.Markup.Contains("StringValue"));

        // Assert
        Assert.Equal("string", TypeDisplayFor(cut, "StringValue"));
        Assert.Equal("bool", TypeDisplayFor(cut, "BoolValue"));
        Assert.Equal("int", TypeDisplayFor(cut, "IntValue"));
        Assert.Equal("double", TypeDisplayFor(cut, "DoubleValue"));
        Assert.Equal("int?", TypeDisplayFor(cut, "NullableIntValue"));
        Assert.Equal("ThemeConfigurationCardTestEnum", TypeDisplayFor(cut, "EnumValue"));

        // TwBlazorPalette is one of the hardcoded "global token" types, so it's split into the
        // second table instead of being listed as one of the component's own properties.
        Assert.Contains("Global Theme Configuration", cut.Markup);
        Assert.Equal("TwBlazorPalette", TypeDisplayFor(cut, "PaletteValue"));
    }

    [Fact]
    public void InheritedFrom_IsNotShown_WhenComponentIsTheCanonicalOwner()
    {
        // Arrange & Act
        var cut = TestContext.Render<ThemeConfigurationCard>(parameters => parameters
            .Add(p => p.ThemeType, typeof(TwButtonTheme))
            .Add(p => p.ComponentTitle, "TwButton"));
        cut.WaitForState(() => cut.Markup.Contains("ButtonUppercase"));

        // Assert
        Assert.DoesNotContain("Inherited from", cut.Markup);
    }

    [Fact]
    public void InheritedFrom_ShowsCanonicalOwner_WhenThemeIsReusedByAnotherComponent()
    {
        // Arrange & Act - TwButtonTheme's canonical owner is TwButton, so rendering it under a
        // different component's page should surface the "inherited from" callout.
        var cut = TestContext.Render<ThemeConfigurationCard>(parameters => parameters
            .Add(p => p.ThemeType, typeof(TwButtonTheme))
            .Add(p => p.ComponentTitle, "TwSelect"));
        cut.WaitForState(() => cut.Markup.Contains("Inherited from"));

        // Assert
        var link = cut.Find("a[href='/button']");
        Assert.Equal("TwButton", link.TextContent);
    }

    private static string TypeDisplayFor(IRenderedComponent<ThemeConfigurationCard> cut, string propertyName)
    {
        var row = cut.FindAll("tbody tr").Single(tr => tr.QuerySelector("td:first-child code")?.TextContent == propertyName);
        return row.QuerySelectorAll("td")[1].TextContent.Trim();
    }
}
