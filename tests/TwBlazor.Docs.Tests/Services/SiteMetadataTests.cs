using TwBlazor.Docs.Services;

namespace TwBlazor.Docs.Tests.Services;

public class SiteMetadataTests
{
    [Theory]
    [InlineData("TwCard", "TwCard | twblazor - Blazor Card Component")]
    [InlineData("TwDataTable", "TwDataTable | twblazor - Blazor Data Table Component")]
    [InlineData("TwRadioButton", "TwRadioButton | twblazor - Blazor Radio Button Component")]
    public void ComponentTitle_SplitsTheTypeNameIntoWords(string componentName, string expected)
    {
        // Act & Assert
        Assert.Equal(expected, SiteMetadata.ComponentTitle(componentName));
    }

    [Theory]
    [InlineData("TwDateRangePicker", "TwDateRangePicker | twblazor - Blazor Date Range Picker")]
    [InlineData("TwDateTimeRangePicker", "TwDateTimeRangePicker | twblazor - Blazor Date Time Range Picker")]
    public void ComponentTitle_DropsTheTrailingComponentWord_WhenTheFullTitleWouldBeTruncated(string componentName, string expected)
    {
        // Act & Assert - the type name and the words people search for survive; only the filler goes.
        Assert.Equal(expected, SiteMetadata.ComponentTitle(componentName));
    }

    [Fact]
    public void ComponentTitle_KeepsTheComponentWord_WhenTheFullTitleJustFitsTheLimit()
    {
        // Arrange - 13 letters makes the full title 59 characters, under MaxTitleLength.
        // Act
        var title = SiteMetadata.ComponentTitle("TwAbcdefghijklm");

        // Assert
        Assert.Equal("TwAbcdefghijklm | twblazor - Blazor Abcdefghijklm Component", title);
        Assert.True(title.Length <= SiteMetadata.MaxTitleLength);
    }

    [Fact]
    public void ComponentTitle_DropsTheComponentWord_WhenTheFullTitleJustExceedsTheLimit()
    {
        // Arrange - 14 letters makes the full title 61 characters, one over MaxTitleLength.
        // Act
        var title = SiteMetadata.ComponentTitle("TwAbcdefghijklmn");

        // Assert
        Assert.Equal("TwAbcdefghijklmn | twblazor - Blazor Abcdefghijklmn", title);
    }

    [Fact]
    public void ComponentTitle_KeepsANameWithoutTheTwPrefixIntact()
    {
        // Act & Assert
        Assert.Equal("Widget | twblazor - Blazor Widget Component", SiteMetadata.ComponentTitle("Widget"));
    }

    [Fact]
    public void ComposeTitle_JoinsTheNameSiteNameAndSummary()
    {
        // Act & Assert
        Assert.Equal("Get Started | twblazor - Install the components", SiteMetadata.ComposeTitle("Get Started", "Install the components"));
    }

    [Theory]
    [InlineData("/", "https://twblazor.com/")]
    [InlineData("/card", "https://twblazor.com/card")]
    public void AbsoluteUrl_PrefixesTheSiteOrigin(string path, string expected)
    {
        // Act & Assert
        Assert.Equal(expected, SiteMetadata.AbsoluteUrl(path));
    }

    [Fact]
    public void FormatDate_IsCultureIndependent()
    {
        // Arrange
        var original = System.Globalization.CultureInfo.CurrentCulture;
        System.Globalization.CultureInfo.CurrentCulture = new System.Globalization.CultureInfo("de-DE");

        try
        {
            // Act & Assert - a German build machine must not turn this into "24 September 2026" spelled differently.
            Assert.Equal("24 September 2026", SiteMetadata.FormatDate(new DateOnly(2026, 9, 24)));
            Assert.Equal("2026-09-24", SiteMetadata.FormatIsoDate(new DateOnly(2026, 9, 24)));
        }
        finally
        {
            System.Globalization.CultureInfo.CurrentCulture = original;
        }
    }
}
