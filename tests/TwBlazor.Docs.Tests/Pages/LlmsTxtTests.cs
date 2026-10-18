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
    private const string siteUrl = "https://twblazor.com";

    private static readonly Lazy<(LlmsOutput Output, IReadOnlyList<PageRoute> Routes)> _generated = new(Generate);

    private static LlmsOutput generatedOutput => _generated.Value.Output;

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
            Paths.ReadLlmsTemplates(),
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
        var lines = generatedOutput.LlmsTxt.Split('\n');

        Assert.Equal("# twblazor", lines[0]);
        Assert.Equal(string.Empty, lines[1]);
        Assert.StartsWith("> ", lines[2]);
        Assert.DoesNotContain("{{", generatedOutput.LlmsTxt);
    }

    [Fact]
    public void LlmsTxt_ListsEveryComponent_LinkingToItsMarkdownPage()
    {
        var components = ComponentCatalog.LoadLeafEntries().ToList();

        Assert.NotEmpty(components); // guards against the catalog read silently returning nothing

        var missing = components
            .Where(leaf => !generatedOutput.LlmsTxt.Contains($"]({siteUrl}{leaf.Entry.Url}.md) `{leaf.Entry.Name}`", StringComparison.Ordinal))
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
            .Where(path => !generatedOutput.Pages.ContainsKey(path))
            .ToList();

        Assert.True(missing.Count == 0, $"Pages without a Markdown copy: {string.Join(", ", missing)}");
    }

    [Fact]
    public void EverySiteLink_ReachesARealPageOrGeneratedFile()
    {
        var routes = _generated.Value.Routes.Select(r => r.Route).ToHashSet(StringComparer.Ordinal);
        var files = generatedOutput.Pages.Keys.Select(path => "/" + path)
            .Concat(["/llms.txt", "/llms-full.txt", $"/skills/{LlmsGenerator.SkillsBundleName}.zip"])
            .Concat(generatedOutput.Skills.Select(s => $"/skills/{s.Name}/SKILL.md"))
            .ToHashSet(StringComparer.Ordinal);

        var broken = SiteLinkPattern().Matches(generatedOutput.LlmsFullTxt)
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
        Assert.Contains($"dotnet add package twblazor --version {expected}\n", generatedOutput.LlmsTxt);
    }

    [Fact]
    public void ThemeRules_SendAssistantsToTheThemeBuilder()
    {
        Assert.Contains("## Rules for AI assistants", generatedOutput.LlmsTxt);
        Assert.Contains($"{siteUrl}/theme", generatedOutput.LlmsTxt);
        Assert.Contains("Theme.CreateTheme", generatedOutput.LlmsTxt);
        Assert.Contains("@source", generatedOutput.LlmsTxt);
    }

    [Fact]
    public void ComponentPages_ListTheLibrarysOwnParameters_AndTheirCompanions()
    {
        var button = generatedOutput.Pages["button.md"];

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
        var getStarted = generatedOutput.Pages["get-started.md"];

        Assert.Contains("```bash\ndotnet new blazor -o {YourProjectName}\n```", getStarted);
        Assert.Contains("<TwToastProvider />", getStarted);
        Assert.Contains("public static TwBlazorTheme CreateDefaultTheme()", getStarted);
    }

    [Fact]
    public void Pages_LeakNoRazorOrHtml_OutsideCodeSamples()
    {
        var leaks = generatedOutput.Pages
            .SelectMany(page => ProseLines(page.Value)
                .Where(line => !line.StartsWith("| `", StringComparison.Ordinal))
                .Select(line => InlineCodePattern().Replace(line, "code"))
                .Where(line => MarkupLeakPattern().IsMatch(line))
                .Select(line => $"{page.Key}: {line}"))
            .ToList();

        Assert.True(leaks.Count == 0, $"Razor or HTML in the generated prose:\n{string.Join('\n', leaks.Take(20))}");
    }

    [Fact]
    public void Output_NeverUsesAnEmDash()
    {
        Assert.DoesNotContain('—', generatedOutput.LlmsFullTxt);
        Assert.All(generatedOutput.Skills.SelectMany(s => s.Files.Values), text => Assert.DoesNotContain('—', text));
    }

    [Fact]
    public void Skills_CoverSetupThemingComponentsAndLocalization()
    {
        Assert.Equal(["twblazor-components", "twblazor-localization", "twblazor-setup", "twblazor-theming"], generatedOutput.Skills.Select(s => s.Name));
    }

    [Fact]
    public void Skills_FollowTheAgentSkillsFormat()
    {
        Assert.All(generatedOutput.Skills, skill =>
        {
            var lines = skill.Files["SKILL.md"].Split('\n');

            Assert.Equal("---", lines[0]);
            Assert.Equal($"name: {skill.Name}", lines[1]);
            Assert.StartsWith("description: \"", lines[2]);
            Assert.Equal("---", lines[3]);
            Assert.InRange(skill.Description.Length, 1, LlmsGenerator.MaxSkillDescriptionLength);
            Assert.DoesNotContain("{{", skill.Files["SKILL.md"]);
        });
    }

    [Fact]
    public void ComponentsSkill_BundlesEveryComponentPage_AndLinksToTheBundledCopies()
    {
        var skill = generatedOutput.Skills.Single(s => s.Name == "twblazor-components");

        var missing = ComponentCatalog.LoadLeafEntries()
            .Where(leaf => !skill.Files.ContainsKey($"references{leaf.Entry.Url}.md") || !skill.Files["SKILL.md"].Contains($"](references{leaf.Entry.Url}.md) `{leaf.Entry.Name}`", StringComparison.Ordinal))
            .Select(leaf => leaf.Entry.Url)
            .ToList();

        Assert.True(missing.Count == 0, $"Components missing from the components skill: {string.Join(", ", missing)}");
    }

    [Fact]
    public void Skills_LinkOnlyToFilesTheyBundle()
    {
        var broken = generatedOutput.Skills
            .SelectMany(skill => SkillReferencePattern().Matches(skill.Files["SKILL.md"])
                .Select(m => m.Value)
                .Where(path => !skill.Files.ContainsKey(path))
                .Select(path => $"{skill.Name}: {path}"))
            .ToList();

        Assert.True(broken.Count == 0, $"Skills link to files they do not bundle: {string.Join(", ", broken)}");
    }

    [Fact]
    public void LlmsTxt_ListsEverySkill()
    {
        Assert.All(generatedOutput.Skills, skill => Assert.Contains($"]({siteUrl}/skills/{skill.Name}/SKILL.md): ", generatedOutput.LlmsTxt));
        Assert.Contains($"{siteUrl}/skills/{LlmsGenerator.SkillsBundleName}.zip", generatedOutput.LlmsTxt);
    }

    [GeneratedRegex(@"references/[\w\-/]+\.md")]
    private static partial Regex SkillReferencePattern();

    [GeneratedRegex(@"https://twblazor\.com(?<path>/[\w\-/.]*)")]
    private static partial Regex SiteLinkPattern();

    [GeneratedRegex(@"dotnet add package twblazor --version (?<version>\S+)")]
    private static partial Regex InstallVersionPattern();

    [GeneratedRegex("`[^`]*`")]
    private static partial Regex InlineCodePattern();

    [GeneratedRegex(@"<[A-Za-z/]|@[A-Za-z(]|^[{}]$|\]\(\)|\[\]\(")]
    private static partial Regex MarkupLeakPattern();
}
