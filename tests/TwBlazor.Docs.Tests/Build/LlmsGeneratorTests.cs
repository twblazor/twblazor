using TwBlazor.Docs.Compiler;

namespace TwBlazor.Docs.Tests.Build;

public class LlmsGeneratorTests
{
    private const string Template = "# twblazor\n\n> Summary.\n\nInstall `{{version}}`.\n\n{{docs}}\n\n{{components}}\n";

    private const string ComponentsJson = """
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

    private static LlmsOutput Generate() => LlmsGenerator.Generate(Template, ComponentsJson, _pages, new Dictionary<string, string>(), _components);

    [Fact]
    public void Generate_FillsTheTemplate_FromTheGetStartedVersionAndTheCatalog()
    {
        var llms = Generate().LlmsTxt;

        Assert.Contains("Install `3.0.1`.", llms);
        Assert.Contains("- [Get Started](https://twblazor.com/get-started.md): Install it.", llms);
        Assert.Contains("## Forms", llms);
        Assert.Contains("- [Button](https://twblazor.com/button.md) `TwButton`: TwButton is a button.", llms);
        Assert.Contains("- [Pickers: Date](https://twblazor.com/date-picker.md) `TwDatePicker`: Pick a date.", llms);
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
    public void Generate_Throws_WhenGetStartedNoLongerNamesAVersion()
    {
        (string, string)[] pages = [("/get-started", "<PageContainer Title=\"Get Started\"></PageContainer>")];

        Assert.Throws<InvalidOperationException>(() => LlmsGenerator.Generate(Template, ComponentsJson, pages, new Dictionary<string, string>(), _components));
    }

    [Theory]
    [InlineData("/button", "/button.md")]
    [InlineData("/chart/area", "/chart/area.md")]
    public void MarkdownPath_AppendsTheExtensionToTheRoute(string route, string expected)
    {
        Assert.Equal(expected, LlmsGenerator.MarkdownPath(route));
    }
}
