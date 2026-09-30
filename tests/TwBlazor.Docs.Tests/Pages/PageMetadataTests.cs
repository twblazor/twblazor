using System.Text.RegularExpressions;
using TwBlazor.Docs.Compiler;

namespace TwBlazor.Docs.Tests.Pages;

public partial class PageMetadataTests
{
    // Search engines cut a meta description off at about this length, and SEO audit tools flag anything longer.
    private const int MaxDescriptionLength = 160;

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
            description.Length <= MaxDescriptionLength,
            $"{page}: description is {description.Length} characters, over the {MaxDescriptionLength} limit: \"{description}\"");
    }

    [GeneratedRegex("""<(?:PageContainer|PageSeo)\b[^>]*?\bDescription="(?<description>[^"]*)"[^>]*/?>""", RegexOptions.Singleline)]
    private static partial Regex DescriptionAttributePattern();
}
