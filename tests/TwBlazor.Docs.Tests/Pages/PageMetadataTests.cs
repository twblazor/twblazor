using System.Text.RegularExpressions;
using TwBlazor.Docs.Compiler;

namespace TwBlazor.Docs.Tests.Pages;

public partial class PageMetadataTests
{
    // Search engines cut a meta description off at about this length, and SEO audit tools flag anything longer.
    private const int maxDescriptionLength = 160;

    private static List<(string Page, string Description)> ReadDescriptions()
    {
        var descriptions = new List<(string, string)>();

        foreach (var file in Directory.EnumerateFiles(Paths.PagesPath, "*.razor", SearchOption.AllDirectories))
        {
            foreach (Match match in DescriptionAttributePattern().Matches(File.ReadAllText(file)))
            {
                descriptions.Add((Path.GetFileName(file), match.Groups["description"].Value));
            }
        }

        return descriptions;
    }

    public static TheoryData<string, string> Descriptions()
    {
        var data = new TheoryData<string, string>();

        foreach (var (page, description) in ReadDescriptions())
        {
            data.Add(page, description);
        }

        return data;
    }

    [Fact]
    public void EveryContentPageDeclaresADescription()
    {
        // Arrange & Act
        var pagesWithDescription = ReadDescriptions().Select(d => d.Page).Distinct().ToList();

        // Assert - a guard against the pattern above silently matching nothing and the length test passing vacuously.
        Assert.True(pagesWithDescription.Count >= 38, $"Only {pagesWithDescription.Count} pages declare a Description.");
    }

    [Theory]
    [MemberData(nameof(Descriptions))]
    public void Description_FitsWithinTheLengthSearchEnginesDisplay(string page, string description)
    {
        // Assert
        Assert.True(
            description.Length <= maxDescriptionLength,
            $"{page}: description is {description.Length} characters, over the {maxDescriptionLength} limit: \"{description}\"");
    }

    [Fact]
    public void PageText_WritesTheBrandNameInLowercase()
    {
        // Arrange - "twblazor" is always lowercase in prose; "TwBlazor" is only right in code (namespaces, assembly names).
        var files = Directory.EnumerateFiles(Paths.PagesPath, "*.razor", SearchOption.AllDirectories)
            .Concat(Directory.EnumerateFiles(Paths.LayoutPath, "*.razor", SearchOption.AllDirectories));

        // Act
        var offenders = files
            .SelectMany(file => File.ReadLines(file).Select((line, index) => (file, line, number: index + 1)))
            .Where(l => BrandNamePattern().IsMatch(l.line) && !UsingDirectivePattern().IsMatch(l.line))
            .Select(l => $"{Path.GetFileName(l.file)}:{l.number}: {l.line.Trim()}")
            .ToList();

        // Assert
        Assert.True(offenders.Count == 0, "Write the brand name as \"twblazor\":\n" + string.Join('\n', offenders));
    }

    // The brand written as a word on its own, not part of an identifier, path or namespace like TwBlazor.Models.
    [GeneratedRegex("""(?<![\w./"<{-])(?i:twblazor)(?![\w.])(?<!twblazor)""")]
    private static partial Regex BrandNamePattern();

    [GeneratedRegex("""^\s*@?using\s+TwBlazor\b""")]
    private static partial Regex UsingDirectivePattern();

    [GeneratedRegex("""<(?:PageContainer|PageSeo)\b[^>]*?\bDescription="(?<description>[^"]*)"[^>]*/?>""", RegexOptions.Singleline)]
    private static partial Regex DescriptionAttributePattern();
}
