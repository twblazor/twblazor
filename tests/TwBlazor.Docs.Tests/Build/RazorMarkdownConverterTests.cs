using TwBlazor.Docs.Compiler;

namespace TwBlazor.Docs.Tests.Build;

public class RazorMarkdownConverterTests
{
    private static readonly Dictionary<string, string> _examples = new(StringComparer.Ordinal)
    {
        ["ChipBasic"] = "<TwChip Label=\"Hello\" />",
        ["ChipBasicCode"] = "private bool open;",
        ["Providers"] = "<TwToastProvider />",
    };

    private static RazorPageContent Convert(string source) =>
        RazorMarkdownConverter.Convert(source, _examples, href => "https://twblazor.com" + href + ".md");

    [Fact]
    public void Convert_ReadsTheTitleDescriptionAndThemeFromThePageContainer()
    {
        var page = Convert("""
            @page "/chip"
            <PageContainer Title="TwChip" Path="/chip" Description="TwChip is a chip &amp; tag." ThemeType="typeof(TwChipTheme)">
            </PageContainer>
            """);

        Assert.Equal("TwChip", page.Title);
        Assert.Equal("TwChip is a chip & tag.", page.Description);
        Assert.Equal("TwChipTheme", page.ThemeType);
    }

    [Fact]
    public void Convert_TurnsEachPageCardIntoASectionWithItsProse()
    {
        var page = Convert("""
            <PageContainer Title="TwChip">
                <PageCard Title="Basic usage">
                    <Description>Set <code>Label</code> to show text.
                        Press <kbd>Enter</kbd> to <strong>select</strong> it.</Description>
                </PageCard>
            </PageContainer>
            """);

        Assert.Equal("## Basic usage\n\nSet `Label` to show text. Press `Enter` to **select** it.", page.Body);
    }

    [Fact]
    public void Convert_WritesCodeExamplesInFull_FromTheExtractedSnippets()
    {
        var page = Convert("""
            <PageContainer Title="TwChip">
                <PageCard Title="Basic">
                    <CodeExample Name="ChipBasic" CSharpName="ChipBasicCode">
                        <TwChip Label="@($"Live {count}")" OnClick="@(() => count++)" />
                    </CodeExample>
                </PageCard>
            </PageContainer>
            """);

        Assert.Contains("```razor\n<TwChip Label=\"Hello\" />\n```", page.Body);
        Assert.Contains("```csharp\nprivate bool open;\n```", page.Body);
        Assert.DoesNotContain("Live", page.Body);
        Assert.Equal(["TwChip"], page.ExampleComponents);
    }

    [Fact]
    public void Convert_ResolvesCodeBlocks_WhoseContentNestsQuotesOrNamesAField()
    {
        var page = Convert(""""
            <PageContainer Title="Setup">
                <TwCodeBlock Content="@CodeExamples.Get("Providers")" />
                <TwCodeBlock Content="@_install" Language="bash" />
                <TwCodeBlock Content="@Examples("[&>*]:p-1 [&>p]:p-2")" Language="csharp" />
            </PageContainer>

            @code {
                private const string _install = """
                    dotnet add package twblazor
                    """;
            }
            """");

        Assert.Equal("```razor\n<TwToastProvider />\n```\n\n```bash\ndotnet add package twblazor\n```", page.Body);
    }

    [Fact]
    public void Convert_ResolvesLinks_AndDropsLinksWithARuntimeTarget()
    {
        var page = Convert("""
            <PageContainer Title="TwBarChart">
                <Description>See <TwLink Href="/chart" Class="underline">TwChart</TwLink> or <a href="@url">@name</a>.</Description>
            </PageContainer>
            """);

        Assert.Equal("See [TwChart](https://twblazor.com/chart.md) or .", page.Body);
    }

    [Fact]
    public void Convert_KeepsEscapedAtSigns_AndDropsRuntimeExpressionsAndControlFlow()
    {
        var page = Convert("""
            <PageContainer Title="TwCheckbox">
                <p>Bind with <code>@@bind-Value</code>, mail team@example.com, value @value and @(a + b).</p>
                @foreach (var item in items)
                {
                    <p>@item.Name</p>
                }
            </PageContainer>
            """);

        Assert.Equal("Bind with `@bind-Value`, mail team@example.com, value and .", page.Body);
    }

    [Fact]
    public void Convert_NestsPageCardsUnderTheirTab()
    {
        var page = Convert("""
            <PageContainer Title="Get Started">
                <TwTabContainer>
                    <TwTab Label="Blazor Server">
                        <PageCard Title="1. Create the project"><p>Run it.</p></PageCard>
                    </TwTab>
                </TwTabContainer>
            </PageContainer>
            """);

        Assert.Equal("## Blazor Server\n\n### 1. Create the project\n\nRun it.", page.Body);
    }

    [Fact]
    public void Convert_WritesStaticTablesAsMarkdown_AndDropsTablesWhoseRowsOnlyExistAtRuntime()
    {
        var page = Convert("""
            <PageContainer Title="Localization">
                <table>
                    <tr><th>Key</th><th>Use | meaning</th></tr>
                    <tr><td><code>Ok</code></td><td>Confirms</td></tr>
                </table>
                <table>
                    <tr><th>Culture</th></tr>
                    @foreach (var language in languages)
                    {
                        <tr><td><code>@language.Culture</code></td></tr>
                    }
                </table>
            </PageContainer>
            """);

        Assert.Equal("| Key | Use \\| meaning |\n| --- | --- |\n| `Ok` | Confirms |", page.Body);
    }
}
