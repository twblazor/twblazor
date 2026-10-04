using TwBlazor.Docs.Compiler;

namespace TwBlazor.Docs.Tests.Build;

public class ThemeTemplateExtractorTests
{
    private const string Source = """
        using System.Reflection.Metadata;
        using TwBlazor.Configuration;
        using TwBlazor.Enums;

        namespace Sample;

        public static class Theme
        {
            public static TwBlazorTheme CreateDefaultTheme()
            {
                #region utility classes

                var display = new TwBlazorDisplay
                {
                    Block = "block"
                };

                // Shared padding scale.
                var spacing = new TwBlazorSpacing
                {
                    Gap = new()
                    {
                        Sm = "gap-1"
                    }
                };

                var transition = new TwBlazorTransition
                {
                    Colors = "transition-colors"
                };
                transition.ColorsFast = $"{transition.Colors} duration-200";

                var overlayTheme = new TwOverlayTheme
                {
                    DialogBackground = "bg-white"
                };

                #endregion

                return new TwBlazorTheme
                {
                    Display = display,
                    Components =
                    [
                        overlayTheme,
                        new TwAlertTheme
                        {
                            Padding = "py-4 px-6"
                        },
                        new TwCardTheme
                        {
                            Container = $"{display.Block} px-6"
                        }
                    ]
                };
            }
        }
        """;

    private static ThemeTemplate Extract() => ThemeTemplateExtractor.Extract(Source);

    [Fact]
    public void Extract_ReadsUsings_ExceptTheHotReloadOne()
    {
        var template = Extract();

        Assert.Equal(["TwBlazor.Configuration", "TwBlazor.Enums"], template.Usings);
    }

    [Fact]
    public void Extract_SplitsTheUtilityRegionIntoNamedStatements()
    {
        var template = Extract();

        Assert.Equal(["display", "spacing", "transition", "overlayTheme"], template.Statements.Select(s => s.Name));
    }

    [Fact]
    public void Extract_KeepsALeadingCommentWithItsStatement()
    {
        var spacing = Extract().Statements.Single(s => s.Name == "spacing");

        Assert.StartsWith("        // Shared padding scale.", spacing.Text);
    }

    [Fact]
    public void Extract_MergesFollowUpAssignmentsIntoTheirVariable()
    {
        var transition = Extract().Statements.Single(s => s.Name == "transition");

        Assert.Contains("transition.ColorsFast = ", transition.Text);
    }

    [Fact]
    public void Extract_SplitsTheComponentsList_ResolvingVariableEntriesToTheirType()
    {
        var blocks = Extract().Blocks;

        Assert.Equal(["TwOverlayTheme", "TwAlertTheme", "TwCardTheme"], blocks.Select(b => b.TypeName));
        Assert.Equal("                overlayTheme", blocks[0].Text);
    }

    [Fact]
    public void Extract_KeepsEachBlocksBody_WithoutItsTrailingComma()
    {
        var alert = Extract().Blocks.Single(b => b.TypeName == "TwAlertTheme");

        Assert.Contains("Padding = \"py-4 px-6\"", alert.Text);
        Assert.EndsWith("}", alert.Text);
    }

    [Fact]
    public void Extract_CapturesTheFixedStartAndEndOfTheReturnedTheme()
    {
        var template = Extract();

        Assert.StartsWith("        return new TwBlazorTheme", template.ReturnPrefix);
        Assert.EndsWith("[", template.ReturnPrefix);
        Assert.Contains("Display = display,", template.ReturnPrefix);
        Assert.StartsWith("            ]", template.ReturnSuffix);
        Assert.EndsWith("};", template.ReturnSuffix);
    }

    [Fact]
    public void Extract_Throws_WhenTheUtilityRegionIsMissing()
    {
        Assert.Throws<InvalidOperationException>(() => ThemeTemplateExtractor.Extract("public class Empty { }"));
    }

    [Fact]
    public void ExtractFile_ReadsTheRealDefaultTheme()
    {
        var template = ThemeTemplateExtractor.ExtractFile(Paths.ThemeCsFilePath);

        Assert.NotEmpty(template.Statements);
        Assert.Contains(template.Blocks, b => b.TypeName == "TwButtonTheme");
        Assert.Contains(template.Blocks, b => b.TypeName == "TwOverlayTheme");
        Assert.Contains(template.Statements, s => s.Name == "neutralSurface");
    }

    [Fact]
    public void ExtractFile_FindsAThemeBlockForEveryThemeTypeInTheRealTheme()
    {
        // Every "new TwXxxTheme" in the Components list must have been picked up, or a component would
        // silently never be selectable in the builder.
        var theme = File.ReadAllText(Paths.ThemeCsFilePath);
        var template = ThemeTemplateExtractor.ExtractFile(Paths.ThemeCsFilePath);

        var inList = System.Text.RegularExpressions.Regex.Matches(theme, @"^                new (Tw\w+Theme)\b", System.Text.RegularExpressions.RegexOptions.Multiline)
            .Select(m => m.Groups[1].Value);

        Assert.All(inList, type => Assert.Contains(template.Blocks, b => b.TypeName == type));
    }
}
