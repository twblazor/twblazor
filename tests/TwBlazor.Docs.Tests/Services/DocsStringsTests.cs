using Bunit;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using TwBlazor.Docs.Compiler;
using TwBlazor.Docs.Layout;
using TwBlazor.Docs.Pages;
using TwBlazor.Docs.Services;

namespace TwBlazor.Docs.Tests.Services;

/// <summary>
/// The docs site's own translations: <c>Resources/DocsStrings.*.resx</c> and the pages that use them.
/// </summary>
public partial class DocsStringsTests : DocsTestBase
{
    private static string resourcesPath => Path.Combine(Paths.DocsProjectPath, "Resources");

    public static TheoryData<string> Translations =>
        [.. DocsLanguages.All.Where(l => l.Code != DocsLanguages.DefaultCode).Select(l => l.Code)];

    [Theory]
    [MemberData(nameof(Translations))]
    public void EveryLanguageInTheSwitcher_HasEveryKey(string code)
    {
        // Arrange
        var english = Read("DocsStrings.resx");

        // Act
        var translated = Read($"DocsStrings.{code}.resx");

        // Assert
        Assert.Empty(english.Keys.Except(translated.Keys));
        Assert.Empty(translated.Keys.Except(english.Keys));
        Assert.DoesNotContain(english, e => e.Value.Length > 0 && translated[e.Key].Length == 0);
    }

    [Theory]
    [MemberData(nameof(Translations))]
    public void EveryTranslation_KeepsTheLinksCodeAndPlaceholdersOfTheEnglish(string code)
    {
        // Arrange
        var english = Read("DocsStrings.resx");
        var translated = Read($"DocsStrings.{code}.resx");

        // Act
        var differences = english
            .Where(e => translated.ContainsKey(e.Key))
            .Where(e => Shape(e.Value) != Shape(translated[e.Key]))
            .Select(e => e.Key)
            .ToList();

        // Assert
        Assert.Empty(differences);
    }

    [Fact]
    public void TheEnglishFile_MatchesTheEnglishWrittenInThePages()
    {
        // Arrange
        var english = Read("DocsStrings.resx");
        var sources = Directory.EnumerateFiles(Paths.DocsProjectPath, "*.*", SearchOption.AllDirectories)
            .Where(path => path.EndsWith(".razor", StringComparison.Ordinal) || path.EndsWith(".cs", StringComparison.Ordinal))
            .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.Ordinal));

        // Act
        var used = sources.SelectMany(path => UsedStrings(File.ReadAllText(path))).ToList();

        // Assert
        Assert.NotEmpty(used);
        Assert.All(used, pair =>
        {
            Assert.True(english.ContainsKey(pair.Key), $"{pair.Key} is missing from DocsStrings.resx");
            Assert.Equal(pair.English, english[pair.Key]);
        });
    }

    [Fact]
    public void Landing_InSpanish_IsTranslated()
    {
        // Arrange & Act
        var cut = RenderIn<Home>("es");

        // Assert
        Assert.Contains("para Blazor", cut.Find("h1").TextContent);
        Assert.Contains("Componentes accesibles y personalizables", cut.Markup);
    }

    [Fact]
    public void Landing_InEnglish_IsUnchanged()
    {
        // Arrange & Act
        var cut = RenderIn<Home>("en-GB");

        // Assert
        Assert.Contains("for Blazor", cut.Find("h1").TextContent);
        Assert.Contains("Accessible, themeable components for Blazor Server and WebAssembly.", cut.Markup);
    }

    [Fact]
    public void Sidebar_InSpanish_TranslatesItsSections_ButNotComponentNames()
    {
        // Arrange & Act
        var cut = RenderIn<Navigation>("es");

        // Assert
        var sidebar = cut.Find("nav[aria-label='sidebar navigation']").TextContent;
        Assert.Contains("Primeros pasos", sidebar);
        Assert.Contains("Formularios", sidebar);
        Assert.Equal("Breadcrumb", cut.Find("a[href='/breadcrumb']").TextContent.Trim());
    }

    [Fact]
    public void ADocsPage_StaysInEnglish()
    {
        // Arrange & Act
        var cut = RenderIn<GetStarted>("es");

        // Assert
        Assert.Equal("Get Started", cut.Find("h1").TextContent.Trim());
    }
    [Fact]
    public void Footer_InSpanish_IsTranslated()
    {
        // Arrange & Act
        var cut = RenderIn<SiteFooter>("es");

        // Assert
        Assert.Contains("twblazor es y siempre será gratuito y de código abierto.", cut.Find("footer").TextContent);
        Assert.Equal("Pie de página", cut.Find("nav").GetAttribute("aria-label"));
    }

    private IRenderedComponent<TComponent> RenderIn<TComponent>(string culture) where TComponent : Microsoft.AspNetCore.Components.IComponent
    {
        var previous = (CultureInfo.CurrentCulture, CultureInfo.CurrentUICulture);
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo(culture);
            return TestContext.Render<TComponent>();
        }
        finally
        {
            (CultureInfo.CurrentCulture, CultureInfo.CurrentUICulture) = previous;
        }
    }

    private static Dictionary<string, string> Read(string file) =>
        XDocument.Load(Path.Combine(resourcesPath, file))
            .Root!.Elements("data")
            .ToDictionary(d => (string)d.Attribute("name")!, d => (string?)d.Element("value") ?? string.Empty, StringComparer.Ordinal);

    // What a translation must keep from the English: its tags, its link targets, the code it quotes and its
    // placeholders. The words around them are free to change.
    private static string Shape(string value) => string.Join("\n",
        string.Join("|", TagRegex().Matches(value).Select(m => m.Groups[1].Value).Order(StringComparer.Ordinal)),
        string.Join("|", HrefRegex().Matches(value).Select(m => m.Groups[1].Value).Order(StringComparer.Ordinal)),
        string.Join("|", CodeRegex().Matches(value).Select(m => m.Groups[1].Value).Order(StringComparer.Ordinal)),
        string.Join("|", PlaceholderRegex().Matches(value).Select(m => m.Value).Order(StringComparer.Ordinal)));

    // Every <Loc Key="..">English</Loc> and Loc.Text("..", "English") with its English as the page writes it.
    private static IEnumerable<(string Key, string English)> UsedStrings(string source)
    {
        foreach (Match m in LocTagRegex().Matches(source))
            yield return (m.Groups["key"].Value, WhitespaceRegex().Replace(m.Groups["text"].Value, " ").Trim().Replace("@@", "@", StringComparison.Ordinal));

        foreach (Match m in LocTextRegex().Matches(source))
            yield return (m.Groups["key"].Value, m.Groups["text"].Value.Replace("\\\"", "\"", StringComparison.Ordinal));
    }

    [GeneratedRegex(@"<(\w+)")]
    private static partial Regex TagRegex();

    [GeneratedRegex("href=\"([^\"]*)\"")]
    private static partial Regex HrefRegex();

    [GeneratedRegex("<code>(.*?)</code>")]
    private static partial Regex CodeRegex();

    [GeneratedRegex(@"\{\d+\}")]
    private static partial Regex PlaceholderRegex();

    [GeneratedRegex("<Loc Key=\"(?<key>[^\"]+)\">(?<text>.*?)</Loc>", RegexOptions.Singleline)]
    private static partial Regex LocTagRegex();

    [GeneratedRegex("""Loc\.(?:Text|Format)\(\s*"(?<key>[^"]+)"\s*,\s*"(?<text>(?:[^"\\]|\\.)*)"\s*[,)]""")]
    private static partial Regex LocTextRegex();

    [GeneratedRegex(@"\s+")]
    private static partial Regex WhitespaceRegex();
}
