using TwBlazor.Docs.Services.ThemeBuilder;
using TwBlazor.Enums;

namespace TwBlazor.Docs.Tests.Services.ThemeBuilder;

public class ThemeCodeGeneratorTests
{
    private static readonly ThemeTemplate _template = new(
        ["TwBlazor.Configuration"],
        [
            new ThemeStatement("display", "        var display = new TwBlazorDisplay { Block = \"block\" };"),
            new ThemeStatement("unused", "        var unused = \"w-1\";"),
            new ThemeStatement("tone", "        var tone = \"bg-purple-600\";"),
            new ThemeStatement("derived", "        var derived = $\"{tone} hover:bg-purple-700/20\";"),
        ],
        "        return new TwBlazorTheme\n        {\n            Display = display,\n            Components =\n            [",
        [
            new ThemeBlock("TwCardTheme", "                new TwCardTheme { Container = $\"{display.Block} text-green-600\" }"),
            new ThemeBlock("TwAlertTheme", "                new TwAlertTheme { Colors = derived }"),
            new ThemeBlock("TwChipTheme", "                new TwChipTheme { Base = \"border-red-600\" }"),
        ],
        "            ]\n        };",
        new Dictionary<string, IReadOnlyList<string>>
        {
            ["TwCard"] = ["TwCardTheme"],
            ["TwAlert"] = ["TwAlertTheme"],
        });

    private static readonly IReadOnlyDictionary<string, string> _noColors = new Dictionary<string, string>();

    private static string Generate(string[] themeTypes, IReadOnlyDictionary<string, string>? colors = null) =>
        ThemeCodeGenerator.Generate(_template, themeTypes.ToHashSet(), colors ?? _noColors);

    [Fact]
    public void Generate_WrapsTheBodyInACompilableThemeClass()
    {
        var code = Generate(["TwCardTheme"]);

        Assert.StartsWith("using TwBlazor.Configuration;\n\npublic static class Theme\n{\n    public static TwBlazorTheme CreateTheme()\n    {\n", code);
        Assert.EndsWith("            ]\n        };\n    }\n}\n", code);
    }

    [Fact]
    public void Generate_IncludesOnlyTheChosenComponentThemes()
    {
        var code = Generate(["TwCardTheme"]);

        Assert.Contains("new TwCardTheme", code);
        Assert.DoesNotContain("new TwAlertTheme", code);
        Assert.DoesNotContain("new TwChipTheme", code);
    }

    [Fact]
    public void Generate_SeparatesBlocksWithCommas_InSourceOrder()
    {
        var code = Generate(["TwAlertTheme", "TwCardTheme"]);

        Assert.True(code.IndexOf("new TwCardTheme", StringComparison.Ordinal) < code.IndexOf("new TwAlertTheme", StringComparison.Ordinal));
        Assert.Contains("text-green-600\" },\n                new TwAlertTheme", code);
    }

    [Fact]
    public void Generate_DropsStatementsNothingReferences()
    {
        var code = Generate(["TwCardTheme"]);

        Assert.Contains("var display", code);
        Assert.DoesNotContain("var unused", code);
        Assert.DoesNotContain("var tone", code);
        Assert.DoesNotContain("var derived", code);
    }

    [Fact]
    public void Generate_KeepsStatementsNeededOnlyByOtherStatements()
    {
        var code = Generate(["TwAlertTheme"]);

        Assert.Contains("var derived", code);
        Assert.Contains("var tone", code);
        Assert.True(code.IndexOf("var tone", StringComparison.Ordinal) < code.IndexOf("var derived", StringComparison.Ordinal));
    }

    [Fact]
    public void Generate_WithNoComponents_StillProducesTheCoreTheme()
    {
        var code = Generate([]);

        Assert.Contains("Display = display,", code);
        Assert.DoesNotContain("Theme {", code);
    }

    [Fact]
    public void Generate_LeavesColorsAlone_WhenTheyAreTheDefaults()
    {
        var colors = ThemeColorFamily.All.ToDictionary(f => f.Name, f => f.DefaultHex);

        var code = Generate(["TwAlertTheme", "TwCardTheme", "TwChipTheme"], colors);

        Assert.Contains("bg-purple-600", code);
        Assert.Contains("hover:bg-purple-700/20", code);
        Assert.Contains("text-green-600", code);
        Assert.DoesNotContain("oklch", code);
    }

    [Fact]
    public void Generate_ReplacesACustomizedFamilyWithArbitraryValues_KeepingPrefixesAndOpacity()
    {
        var colors = new Dictionary<string, string> { ["Primary"] = "#2563eb" };
        var ramp = ThemeColorRamp.Generate("#2563eb")!;

        var code = Generate(["TwAlertTheme"], colors);

        Assert.Contains($"bg-{ramp[600].ToTailwindValue()}", code);
        Assert.Contains($"hover:bg-{ramp[700].ToTailwindValue()}/20", code);
        Assert.DoesNotContain("purple", code);
    }

    [Fact]
    public void Generate_OnlyTouchesTheCustomizedFamily()
    {
        var colors = new Dictionary<string, string> { ["Primary"] = "#2563eb" };

        var code = Generate(["TwCardTheme", "TwChipTheme", "TwAlertTheme"], colors);

        Assert.Contains("text-green-600", code);
        Assert.Contains("border-red-600", code);
    }

    [Fact]
    public void Generate_IgnoresAnInvalidColor()
    {
        var colors = new Dictionary<string, string> { ["Primary"] = "garbage" };

        var code = Generate(["TwAlertTheme"], colors);

        Assert.Contains("bg-purple-600", code);
    }

    [Fact]
    public void ResolveThemeTypes_UnionsEachComponentsThemes_AndIgnoresUnknownOnes()
    {
        var types = _template.ResolveThemeTypes(["TwCard", "TwAlert", "TwNope"]);

        Assert.Equal(["TwAlertTheme", "TwCardTheme"], types.Order());
    }

    [Fact]
    public void DefaultTemplate_IsBuiltFromTheRealTheme_AndEveryComponentThemeResolvesToABlock()
    {
        var template = ThemeTemplate.Default;

        Assert.NotEmpty(template.Blocks);
        var blockTypes = template.Blocks.Select(b => b.TypeName).ToHashSet();
        Assert.All(template.ComponentThemes.Values.SelectMany(t => t), type => Assert.Contains(type, blockTypes));
    }

    [Fact]
    public void DefaultTemplate_FullSelectionWithDefaultColors_ReproducesEveryRealComponentBlock()
    {
        var template = ThemeTemplate.Default;
        var all = template.Blocks.Select(b => b.TypeName).ToHashSet();

        var code = ThemeCodeGenerator.Generate(template, all, _noColors);

        Assert.All(template.Blocks, block => Assert.Contains(block.Text, code));
        Assert.DoesNotContain("#region", code);
    }

    [Fact]
    public void DefaultTemplate_OneComponent_ProducesAMuchSmallerFileThanEverything()
    {
        var template = ThemeTemplate.Default;
        var everything = ThemeCodeGenerator.Generate(template, template.Blocks.Select(b => b.TypeName).ToHashSet(), _noColors);

        var slider = ThemeCodeGenerator.Generate(template, template.ResolveThemeTypes(["TwSlider"]), _noColors);

        Assert.True(slider.Length < everything.Length / 2);
        Assert.Contains("new TwSliderTheme", slider);
        Assert.DoesNotContain("new TwCalendarTheme", slider);
    }

    private static readonly ThemeTemplate _defaultsTemplate = new(
        ["TwBlazor.Configuration"],
        [new ThemeStatement("rounded", "        var rounded = new TwBlazorRounded { DefaultRounded = Rounded.Md };")],
        "        return new TwBlazorTheme\n        {\n            Rounded = rounded,\n            Shadows = new TwBlazorShadow { DefaultShadow = Shadow.Sm },\n            Components =\n            [",
        [
            new ThemeBlock("TwInputTheme", "                new TwInputTheme\n                {\n                    DefaultInputVariant = InputVariant.Outlined,\n                    Base = \"x\"\n                }"),
            new ThemeBlock("TwButtonTheme", "                new TwButtonTheme\n                {\n                    Base = \"y\"\n                }"),
        ],
        "            ]\n        };",
        new Dictionary<string, IReadOnlyList<string>>());

    private static readonly HashSet<string> _defaultsThemeTypes = ["TwInputTheme", "TwButtonTheme"];

    private static string WithDefaults(ThemeDefaults defaults) =>
        ThemeCodeGenerator.Generate(_defaultsTemplate, _defaultsThemeTypes, _noColors, defaults);

    [Fact]
    public void Generate_WithTheDefaultDefaults_ChangesNothing()
    {
        Assert.Equal(ThemeCodeGenerator.Generate(_defaultsTemplate, _defaultsThemeTypes, _noColors), WithDefaults(ThemeDefaults.Default));
    }

    [Fact]
    public void Generate_SetsTheRoundedAndShadowDefaults()
    {
        var code = WithDefaults(ThemeDefaults.Default with { Rounded = Rounded.Full, Shadow = Shadow.Lg });

        Assert.Contains("DefaultRounded = Rounded.Full", code);
        Assert.Contains("DefaultShadow = Shadow.Lg", code);
        Assert.DoesNotContain("Rounded.Md", code);
    }

    [Fact]
    public void Generate_ReplacesTheInputVariantDefault()
    {
        var code = WithDefaults(ThemeDefaults.Default with { InputVariant = InputVariant.Filled });

        Assert.Contains("DefaultInputVariant = InputVariant.Filled", code);
        Assert.DoesNotContain("InputVariant.Outlined", code);
    }

    [Fact]
    public void Generate_InsertsAButtonVariantDefault_AsTheButtonThemesFirstProperty()
    {
        var code = WithDefaults(ThemeDefaults.Default with { ButtonVariant = ButtonVariant.Text });

        Assert.Contains("new TwButtonTheme\n                {\n                    DefaultVariant = ButtonVariant.Text,\n                    Base = \"y\"", code);
    }

    [Fact]
    public void Generate_LeavesTheButtonThemeAlone_WhenTheDefaultIsTheBuiltInOne()
    {
        Assert.DoesNotContain("DefaultVariant", WithDefaults(ThemeDefaults.Default));
    }

    [Fact]
    public void Generate_OnTheRealTheme_OnlyChangesTheDefaultLines()
    {
        var template = ThemeTemplate.Default;
        var all = template.Blocks.Select(b => b.TypeName).ToHashSet();
        var baseline = ThemeCodeGenerator.Generate(template, all, _noColors).Split('\n');

        var changed = ThemeCodeGenerator.Generate(template, all, _noColors, new ThemeDefaults(Rounded.None, Shadow.None, InputVariant.Filled, ButtonVariant.Text)).Split('\n');

        var added = changed.Except(baseline).Select(l => l.Trim()).Order().ToList();
        Assert.Equal(["DefaultInputVariant = InputVariant.Filled,", "DefaultRounded = Rounded.None,", "DefaultShadow = Shadow.None", "DefaultVariant = ButtonVariant.Text,"], added);
        Assert.Equal(changed.Length - 1, baseline.Length);
    }

    [Fact]
    public void PreviewStyle_IsEmpty_ForDefaultColors()
    {
        var colors = ThemeColorFamily.All.ToDictionary(f => f.Name, f => f.DefaultHex);

        Assert.Equal(string.Empty, ThemePreviewStyle.Build(colors));
    }

    [Fact]
    public void PreviewStyle_RedefinesEveryShadeVariable_OfACustomizedFamily()
    {
        var colors = new Dictionary<string, string> { ["Danger"] = "#2563eb" };

        var style = ThemePreviewStyle.Build(colors);

        Assert.All(ThemeColorRamp.Shades, shade => Assert.Contains($"--color-red-{shade}: oklch(", style));
        Assert.DoesNotContain("--color-purple", style);
    }
}
