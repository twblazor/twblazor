using TwBlazor.Docs.Compiler;

namespace TwBlazor.Docs.Tests.Build;

public class LlmsGeneratorTests
{
    private const string llmsTemplate = "# twblazor\n\n> Summary.\n\n{{include:rules}}\n\nInstall `{{version}}`.\n\n{{docs}}\n\n{{skills}}\n\n{{components}}\n";

    private const string setupSkill = """
        ---
        name: twblazor-setup
        description: Sets twblazor up. Use when installing it.
        references: /get-started
        ---
        # Setup

        {{include:rules}}

        Read {{link:/get-started}} or {{link:/button}}.

        {{themes}}
        """;

    private const string componentsJson = """
        [
          { "category": "Forms", "items": [
            { "display": "Button", "name": "TwButton", "url": "/button" },
            { "display": "Pickers", "items": [ { "display": "Date", "name": "TwDatePicker", "url": "/date-picker" } ] }
          ] }
        ]
        """;

    private static readonly (string Route, string Source)[] _pages =
    [
        ("/", "<PageContainer Title=\"Home\" Description=\"Home page.\"></PageContainer>"),
        ("/get-started", "<PageContainer Title=\"Get Started\" Description=\"Install it.\"><PageCard Title=\"Install\"><p>Run <code>dotnet add package twblazor --version 3.0.1</code>.</p></PageCard></PageContainer>"),
        ("/button", "<PageContainer Title=\"TwButton\" Description=\"TwButton is a button.\" ThemeType=\"typeof(TwButtonTheme)\"><PageCard Title=\"Basic\"><p>See <a href=\"/get-started\">setup</a>.</p></PageCard></PageContainer>"),
        ("/date-picker", "<PageContainer Title=\"TwDatePicker\" Description=\"Pick a date.\"></PageContainer>"),
    ];

    private static readonly Dictionary<string, ComponentInfo> _components = new(StringComparer.Ordinal)
    {
        ["TwButton"] = new("TwButton", "Button", "A button.", [new("Label", "string", "Gets or sets the | label.", "TwButton")]),
    };

    private static LlmsTemplates Templates(string? skill = setupSkill, string llms = llmsTemplate) => new(
        llms,
        new Dictionary<string, string> { ["rules"] = "- Use the [guide]({{link:/get-started}})." },
        skill is null ? [] : new Dictionary<string, string> { ["twblazor-setup"] = skill });

    private static LlmsOutput Generate(LlmsTemplates? templates = null) =>
        LlmsGenerator.Generate(templates ?? Templates(), componentsJson, _pages, new Dictionary<string, string>(), _components);

    [Fact]
    public void Generate_FillsTheTemplate_FromTheGetStartedVersionTheCatalogAndThePartials()
    {
        var llms = Generate().LlmsTxt;

        Assert.Contains("- Use the [guide](https://twblazor.com/get-started.md).", llms);
        Assert.Contains("Install `3.0.1`.", llms);
        Assert.Contains("- [Get Started](https://twblazor.com/get-started.md): Install it.", llms);
        Assert.Contains("## Forms", llms);
        Assert.Contains("- [Button](https://twblazor.com/button.md) `TwButton`: TwButton is a button.", llms);
        Assert.Contains("- [Pickers: Date](https://twblazor.com/date-picker.md) `TwDatePicker`: Pick a date.", llms);
        Assert.Contains("- [twblazor-setup](https://twblazor.com/skills/twblazor-setup/SKILL.md): Sets twblazor up. Use when installing it.", llms);
        Assert.DoesNotContain("Home page.", llms);
    }

    [Fact]
    public void Generate_WritesAMarkdownPagePerRoute_ExceptTheHomePage()
    {
        Assert.Equal(["button.md", "date-picker.md", "get-started.md"], Generate().Pages.Keys);
    }

    [Fact]
    public void Generate_ComponentPage_CarriesItsLinksKeyboardParametersAndTheme()
    {
        var button = Generate().Pages["button.md"];

        Assert.StartsWith("# TwButton\n\n> TwButton is a button.\n\nDocumentation page: https://twblazor.com/button\n", button);
        Assert.Contains("See [setup](https://twblazor.com/get-started.md).", button);
        Assert.Contains("## Keyboard navigation", button);
        Assert.Contains("| `Label` | `string` | Gets or sets the \\| label. |", button);
        Assert.Contains("Styled by `TwButtonTheme`.", button);
    }

    [Fact]
    public void Generate_LlmsFullTxt_IsTheIndexFollowedByEveryPage()
    {
        var output = Generate();

        Assert.StartsWith(output.LlmsTxt, output.LlmsFullTxt);
        Assert.All(output.Pages.Values, page => Assert.Contains(page, output.LlmsFullTxt));
    }

    [Fact]
    public void Generate_Skill_WritesItsFrontmatterAndBundlesItsReferences()
    {
        var skill = Assert.Single(Generate().Skills);

        Assert.Equal("twblazor-setup", skill.Name);
        Assert.Equal(["SKILL.md", "references/get-started.md"], skill.Files.Keys);
        Assert.StartsWith("---\nname: twblazor-setup\ndescription: \"Sets twblazor up. Use when installing it.\"\n---\n\n# Setup\n", skill.Files["SKILL.md"]);
        Assert.Equal(Generate().Pages["get-started.md"], skill.Files["references/get-started.md"]);
    }

    [Fact]
    public void Generate_Skill_LinksToItsBundledCopy_AndToTheSiteForEverythingElse()
    {
        var skillMd = Assert.Single(Generate().Skills).Files["SKILL.md"];

        Assert.Contains("- Use the [guide](references/get-started.md).", skillMd);
        Assert.Contains("Read references/get-started.md or https://twblazor.com/button.md.", skillMd);
        Assert.Contains("| `TwButton` | `TwButtonTheme` |", skillMd);
    }

    [Fact]
    public void Generate_ComponentsReference_BundlesEveryCatalogPage()
    {
        var skill = setupSkill.Replace("references: /get-started", "references: components", StringComparison.Ordinal);

        var files = Assert.Single(Generate(Templates(skill)).Skills).Files.Keys;

        Assert.Equal(["SKILL.md", "references/button.md", "references/date-picker.md"], files);
    }

    [Theory]
    [InlineData("name: Twblazor-Setup")]
    [InlineData("name: another-name")]
    [InlineData("description: Uses <angle> brackets.")]
    [InlineData("references: /missing-page")]
    public void Generate_RejectsAnInvalidSkill(string line)
    {
        var key = line.Split(':')[0];
        var skill = string.Join('\n', setupSkill.Replace("\r\n", "\n").Split('\n').Select(l => l.StartsWith(key + ":", StringComparison.Ordinal) ? line : l));

        Assert.Throws<InvalidOperationException>(() => Generate(Templates(skill)));
    }

    [Fact]
    public void Generate_RejectsAnOverlongSkillDescription()
    {
        var skill = setupSkill.Replace("Sets twblazor up. Use when installing it.", new string('a', LlmsGenerator.MaxSkillDescriptionLength + 1), StringComparison.Ordinal);

        Assert.Throws<InvalidOperationException>(() => Generate(Templates(skill)));
    }

    [Theory]
    [InlineData("{{unknown}}")]
    [InlineData("{{include:missing}}")]
    public void Generate_RejectsAnUnknownPlaceholder(string placeholder)
    {
        Assert.Throws<InvalidOperationException>(() => Generate(Templates(llms: llmsTemplate + placeholder)));
    }

    [Fact]
    public void Generate_Throws_WhenGetStartedNoLongerNamesAVersion()
    {
        (string, string)[] pages = [("/get-started", "<PageContainer Title=\"Get Started\"></PageContainer>")];

        Assert.Throws<InvalidOperationException>(() => LlmsGenerator.Generate(Templates(skill: null), componentsJson, pages, new Dictionary<string, string>(), _components));
    }

    [Theory]
    [InlineData("/button", "/button.md")]
    [InlineData("/chart/area", "/chart/area.md")]
    public void MarkdownPath_AppendsTheExtensionToTheRoute(string route, string expected)
    {
        Assert.Equal(expected, LlmsGenerator.MarkdownPath(route));
    }
}
