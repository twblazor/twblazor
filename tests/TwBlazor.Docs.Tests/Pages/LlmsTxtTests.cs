using System.Text.RegularExpressions;
using TwBlazor.Docs.Compiler;
using TwBlazor.Docs.Services;

namespace TwBlazor.Docs.Tests.Pages;

/// <summary>
/// Generates <c>llms.txt</c>, <c>llms-full.txt</c> and the Markdown pages from the real docs, the same way the
/// build does, and checks AI assistants get a complete and accurate picture: every component is listed, every
/// link works, parameters come from the library and no Razor leaks into the text.
/// </summary>
public partial class LlmsTxtTests
{
    private const string SiteUrl = "https://twblazor.com";

    private static readonly Lazy<(LlmsOutput Output, IReadOnlyList<PageRoute> Routes)> _generated = new(Generate);

    private static LlmsOutput Output => _generated.Value.Output;

    private static (LlmsOutput, IReadOnlyList<PageRoute>) Generate()
    {
        var examples = new Dictionary<string, string>(StringComparer.Ordinal);
        Dictionary<string, string>[] sources =
        [
            CodeExampleExtractor.Extract(Paths.PagesPath),
            RegionSnippetExtractor.Extract(Paths.PagesPath),
            RegionSnippetExtractor.Extract(Paths.LayoutPath),
            RegionSnippetExtractor.ExtractFile(Paths.ThemeCsFilePath),
            RegionSnippetExtractor.ExtractFile(Paths.ServerProgramCsFilePath),
        ];

        foreach (var (name, code) in sources.SelectMany(source => source))
            examples.TryAdd(name, code);

        var routes = PageRouteScanner.Scan(Paths.PagesPath, "/showcase/");
        var output = LlmsGenerator.Generate(
            File.ReadAllText(Paths.LlmsTemplatePath),
            File.ReadAllText(Paths.ComponentsJsonPath),
            [.. routes.Select(r => (r.Route, File.ReadAllText(r.SourcePath)))],
            examples,
            ComponentParameterScanner.Scan(Paths.LibrarySourcePath));

        return (output, routes);
    }

    private static IEnumerable<string> ProseLines(string markdown)
    {
        var inFence = false;
        foreach (var line in markdown.Split('\n'))
        {
            if (line.StartsWith("```", StringComparison.Ordinal))
            {
                inFence = !inFence;
                continue;
            }

            if (!inFence)
                yield return line;
        }
    }

    [Fact]
    public void LlmsTxt_FollowsTheLlmsTxtFormat_WithATitleThenASummary()
    {
        var lines = Output.LlmsTxt.Split('\n');

        Assert.Equal("# twblazor", lines[0]);
        Assert.Equal(string.Empty, lines[1]);
        Assert.StartsWith("> ", lines[2]);
        Assert.DoesNotContain("{{", Output.LlmsTxt);
    }

    [Fact]
    public void LlmsTxt_ListsEveryComponent_LinkingToItsMarkdownPage()
    {
        var components = ComponentCatalog.LoadLeafEntries().ToList();

        Assert.NotEmpty(components); // guards against the catalog read silently returning nothing

        var missing = components
            .Where(leaf => !Output.LlmsTxt.Contains($"]({SiteUrl}{leaf.Entry.Url}.md) `{leaf.Entry.Name}`", StringComparison.Ordinal))
            .Select(leaf => $"{leaf.Entry.Name} ({leaf.Entry.Url})")
            .ToList();

        Assert.True(missing.Count == 0, $"Components missing from llms.txt: {string.Join(", ", missing)}");
    }

    [Fact]
    public void EveryDocsPage_HasAMarkdownCopy()
    {
        var missing = _generated.Value.Routes
            .Where(r => !LlmsGenerator.ExcludedRoutes.Contains(r.Route))
            .Select(r => LlmsGenerator.MarkdownPath(r.Route).TrimStart('/'))
            .Where(path => !Output.Pages.ContainsKey(path))
            .ToList();

        Assert.True(missing.Count == 0, $"Pages without a Markdown copy: {string.Join(", ", missing)}");
    }

    [Fact]
    public void EverySiteLink_ReachesARealPageOrGeneratedFile()
    {
        var routes = _generated.Value.Routes.Select(r => r.Route).ToHashSet(StringComparer.Ordinal);
        var files = Output.Pages.Keys.Select(path => "/" + path).Append("/llms.txt").Append("/llms-full.txt").ToHashSet(StringComparer.Ordinal);

        var broken = SiteLinkPattern().Matches(Output.LlmsFullTxt)
            .Select(match => match.Groups["path"].Value.TrimEnd('.'))
            .Where(path => !routes.Contains(path) && !files.Contains(path))
            .Distinct()
            .ToList();

        Assert.True(broken.Count == 0, $"Links to pages that do not exist: {string.Join(", ", broken)}");
    }

    [Fact]
    public void InstallCommand_NamesTheSameVersionAsGetStarted()
    {
        var getStarted = File.ReadAllText(Path.Combine(Paths.PagesPath, "GetStarted.razor"));
        var expected = InstallVersionPattern().Match(getStarted).Groups["version"].Value;

        Assert.NotEmpty(expected);
        Assert.Contains($"dotnet add package twblazor --version {expected}\n", Output.LlmsTxt);
    }

    [Fact]
    public void ThemeRules_SendAssistantsToTheThemeBuilder()
    {
        Assert.Contains("## Rules for AI assistants", Output.LlmsTxt);
        Assert.Contains($"{SiteUrl}/theme", Output.LlmsTxt);
        Assert.Contains("Theme.CreateTheme", Output.LlmsTxt);
        Assert.Contains("@source", Output.LlmsTxt);
    }

    [Fact]
    public void ComponentPages_ListTheLibrarysOwnParameters_AndTheirCompanions()
    {
        var button = Output.Pages["button.md"];

        Assert.StartsWith("# TwButton\n", button);
        Assert.Contains("## Parameters", button);
        Assert.Contains("| `Label` | `string` |", button);
        Assert.Contains("### TwButtonGroup", button);
        Assert.Contains("## Keyboard navigation", button);
        Assert.Contains("Styled by `TwButtonTheme`", button);
    }

    [Fact]
    public void Pages_WriteCodeSamplesInFull()
    {
        var getStarted = Output.Pages["get-started.md"];

        Assert.Contains("```bash\ndotnet new blazor -o {YourProjectName}\n```", getStarted);
        Assert.Contains("<TwToastProvider />", getStarted);
        Assert.Contains("public static TwBlazorTheme CreateDefaultTheme()", getStarted);
    }

    [Fact]
    public void Pages_LeakNoRazorOrHtml_OutsideCodeSamples()
    {
        var leaks = Output.Pages
            .SelectMany(page => ProseLines(page.Value)
                .Where(line => !line.StartsWith("| `", StringComparison.Ordinal))
                .Select(line => InlineCodePattern().Replace(line, string.Empty))
                .Where(line => MarkupLeakPattern().IsMatch(line))
                .Select(line => $"{page.Key}: {line}"))
            .ToList();

        Assert.True(leaks.Count == 0, $"Razor or HTML in the generated prose:\n{string.Join('\n', leaks.Take(20))}");
    }

    [Fact]
    public void Output_NeverUsesAnEmDash()
    {
        Assert.DoesNotContain('—', Output.LlmsFullTxt);
    }

    [GeneratedRegex(@"https://twblazor\.com(?<path>/[\w\-/.]*)")]
    private static partial Regex SiteLinkPattern();

    [GeneratedRegex(@"dotnet add package twblazor --version (?<version>\S+)")]
    private static partial Regex InstallVersionPattern();

    [GeneratedRegex("`[^`]*`")]
    private static partial Regex InlineCodePattern();

    [GeneratedRegex(@"<[A-Za-z/]|@[A-Za-z(]|^[{}]$|\]\(\)|\[\]\(")]
    private static partial Regex MarkupLeakPattern();
}
