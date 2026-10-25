using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using TwBlazor.Configuration;
using TwBlazor.Docs.Services;
using TwBlazor.Docs.Services.ThemeBuilder;
using TwBlazor.Enums;

namespace TwBlazor.Docs.Tests.Services;

/// <summary>
/// Compiles the <c>Theme.cs</c> the theme builder generates, so a change to the default theme or to the
/// generator that leaves the copied file broken fails a test instead of a visitor's build.
/// </summary>
public class ThemeCodeCompilationTests
{
    private static readonly ThemeTemplate _template = ThemeTemplate.Default;

    private static readonly Dictionary<string, string> _defaultColors = ThemeColorFamily.All.ToDictionary(f => f.Name, f => f.DefaultHex);

    // Every component the builder offers, the same list its checkboxes come from.
    private static readonly string[] _allComponents = [.. ComponentCatalog.LoadLeafEntries().Select(leaf => leaf.Entry.Name).Where(name => name.Length > 0)];

    private static readonly string[] _gridOnly = ["TwGrid"];
    private static readonly string[] _flexOnly = ["TwFlex"];
    private static readonly string[] _containerOnly = ["TwContainer"];
    private static readonly string[] _buttonOnly = ["TwButton"];
    private static readonly string[] _dataTableOnly = ["TwDataTable"];
    private static readonly string[] _nothing = [];

    public static TheoryData<string[]> Selections => [_allComponents, _gridOnly, _flexOnly, _containerOnly, _buttonOnly, _dataTableOnly, _nothing];

    private static IReadOnlyList<Diagnostic> CompileErrors(string code)
    {
        var platform = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!).Split(Path.PathSeparator);
        var references = platform
            .Append(typeof(TwBlazorTheme).Assembly.Location)
            .Select(path => MetadataReference.CreateFromFile(path));

        var compilation = CSharpCompilation.Create(
            "GeneratedTheme",
            [CSharpSyntaxTree.ParseText(code)],
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, nullableContextOptions: NullableContextOptions.Enable));

        return [.. compilation.GetDiagnostics().Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)];
    }

    [Theory]
    [MemberData(nameof(Selections))]
    public void TheGeneratedTheme_Compiles_ForAnySelection(string[] components)
    {
        var code = ThemeCodeGenerator.Generate(_template, _template.ResolveThemeTypes(components), _defaultColors);

        var errors = CompileErrors(code);

        Assert.True(errors.Count == 0, string.Join('\n', errors.Select(error => error.ToString())));
    }

    [Fact]
    public void TheGeneratedTheme_Compiles_WithCustomColorsSurfacesAndDefaults()
    {
        var colors = new Dictionary<string, string>(_defaultColors) { ["Primary"] = "#0f766e", ["Accent"] = "#e11d48" };
        var defaults = new ThemeDefaults(Rounded.Full, Shadow.Lg, InputVariant.Filled, ButtonVariant.Outlined);
        var surfaces = new ThemeSurfaces("#fafaf9", "#1c1917", "#f5f5f4", "#0c0a09");
        var everything = _template.ResolveThemeTypes(_allComponents);

        var code = ThemeCodeGenerator.Generate(_template, everything, colors, defaults, surfaces: surfaces);

        var errors = CompileErrors(code);
        Assert.True(errors.Count == 0, string.Join('\n', errors.Select(error => error.ToString())));
        Assert.Contains("var tokens = new TwBlazorTokens", code);
        Assert.Contains("var grid = new TwBlazorGrid", code);
        Assert.Contains("new TwGridTheme", code);
    }

    [Fact]
    public void ALayoutComponent_BringsTheSharedScalesItUses()
    {
        var code = ThemeCodeGenerator.Generate(_template, _template.ResolveThemeTypes(["TwGrid"]), _defaultColors);

        Assert.Contains("new TwGridTheme", code);
        Assert.Contains("var spacing = new TwBlazorSpacing", code);
        Assert.Contains("Grid = grid,", code);
        Assert.DoesNotContain("new TwButtonTheme", code);
    }
}
