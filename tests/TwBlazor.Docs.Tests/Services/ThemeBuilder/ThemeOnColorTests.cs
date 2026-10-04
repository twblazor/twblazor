using System.Text.RegularExpressions;
using TwBlazor.Configuration;
using TwBlazor.Docs.Services.ThemeBuilder;
using ThemeBase = TwBlazor.Theme.Theme;

namespace TwBlazor.Docs.Tests.Services.ThemeBuilder;

public class ThemeOnColorTests
{
    private static readonly IReadOnlySet<string> AllThemes = ThemeTemplate.Default.Blocks.Select(b => b.TypeName).ToHashSet();

    private static OklchColor Parse(string hex)
    {
        OklchColor.TryParseHex(hex, out var color);
        return color;
    }

    private static string Generate(Dictionary<string, string> colors, Dictionary<string, OnColorMode>? onColors = null) =>
        ThemeCodeGenerator.Generate(ThemeTemplate.Default, AllThemes, colors, onColors: onColors);

    private static string FilledLine(string code, string name)
    {
        var filled = Regex.Match(code, @"Filled = new\(\)\s*\{.*?\n\s*\},", RegexOptions.Singleline).Value;
        return filled.Split('\n').Single(l => l.TrimStart().StartsWith($"{name} = ", StringComparison.Ordinal));
    }

    [Theory]
    [InlineData("#ffffff", OnColorMode.Dark)]
    [InlineData("#fef08a", OnColorMode.Dark)]
    [InlineData("#f5f5f5", OnColorMode.Dark)]
    [InlineData("#000000", OnColorMode.Light)]
    [InlineData("#1e3a8a", OnColorMode.Light)]
    [InlineData("#9810fa", OnColorMode.Light)]
    public void Auto_PicksTheTextWithTheBetterContrast(string hex, OnColorMode expected)
    {
        Assert.Equal(expected, ThemeOnColor.Resolve(OnColorMode.Auto, Parse(hex)));
    }

    [Theory]
    [InlineData(OnColorMode.Light)]
    [InlineData(OnColorMode.Dark)]
    public void AnExplicitMode_IsNeverOverridden(OnColorMode mode)
    {
        Assert.Equal(mode, ThemeOnColor.Resolve(mode, Parse("#ffffff")));
    }

    [Fact]
    public void AutoText_OnAnyColor_ReachesAtLeastThreeToOne()
    {
        // Light or dark text always manages 4.58:1 against some mid-tone at worst, so Auto never picks a failing pair.
        for (var step = 0; step <= 255; step += 15)
        {
            var color = Parse($"#{step:x2}{step:x2}{step:x2}");
            Assert.True(ThemeOnColor.Contrast(OnColorMode.Auto, color) >= 4.5, $"grey {step}");
        }
    }

    [Fact]
    public void DefaultColors_KeepTheDefaultThemesOwnText()
    {
        var colors = ThemeColorFamily.All.ToDictionary(f => f.Name, f => f.DefaultHex);

        var code = Generate(colors);

        Assert.Equal(Generate([]), code);
        Assert.Contains("hover:bg-purple-700 active:bg-purple-800 {text.Medium.Light}", code);
    }

    [Fact]
    public void ALightColor_GetsDarkTextAndLighterHoverShades_OnItsFilledSurface()
    {
        var code = Generate(new() { ["Accent"] = "#ffffff" });

        var line = FilledLine(code, "Accent");
        Assert.EndsWith(" text-gray-950\",", line);
        Assert.DoesNotContain("text-gray-100", line);
        var ramp = ThemeColorRamp.Generate("#ffffff")!;
        Assert.Contains($"hover:bg-{ramp[600].ToTailwindValue()}", line);
        Assert.Contains($"active:bg-{ramp[500].ToTailwindValue()}", line);
    }

    [Fact]
    public void ADarkColor_KeepsLightText_AndDarkerHoverShades()
    {
        var code = Generate(new() { ["Primary"] = "#1e3a8a" });

        var line = FilledLine(code, "Primary");
        Assert.Contains("text-gray-100", line);
        var ramp = ThemeColorRamp.Generate("#1e3a8a")!;
        Assert.Contains($"hover:bg-{ramp[700].ToTailwindValue()}", line);
        Assert.Contains($"active:bg-{ramp[800].ToTailwindValue()}", line);
    }

    [Fact]
    public void OnlyTheCustomizedColorsFilledLine_IsRewritten()
    {
        var code = Generate(new() { ["Accent"] = "#ffffff" });

        Assert.Equal(Regex.Replace(FilledLine(Generate([]), "Danger"), @"\s+", " "), Regex.Replace(FilledLine(code, "Danger"), @"\s+", " "));
    }

    [Fact]
    public void AnExplicitMode_AppliesEvenToAnUntouchedColor()
    {
        var colors = ThemeColorFamily.All.ToDictionary(f => f.Name, f => f.DefaultHex);

        var code = Generate(colors, new() { ["Warning"] = OnColorMode.Light });

        Assert.Contains("text-gray-100", FilledLine(code, "Warning"));
    }

    [Fact]
    public void TheTextModeOfOneColor_DoesNotLeakIntoAnother()
    {
        var code = Generate(new() { ["Primary"] = "#ffffff" }, new() { ["Primary"] = OnColorMode.Light });

        Assert.Contains("text-gray-100", FilledLine(code, "Primary"));
        Assert.DoesNotContain("text-gray-950", FilledLine(code, "Info"));
    }

    [Fact]
    public void TheFilledShadeConstants_MatchTheRealDefaultTheme()
    {
        TwBlazorTheme theme = ThemeBase.CreateDefaultTheme();

        foreach (var family in ThemeColorFamily.All)
        {
            var filled = family.Name switch
            {
                "Primary" => theme.Colors.SurfaceColors.Filled.Primary,
                "Accent" => theme.Colors.SurfaceColors.Filled.Accent,
                "Success" => theme.Colors.SurfaceColors.Filled.Success,
                "Danger" => theme.Colors.SurfaceColors.Filled.Danger,
                "Warning" => theme.Colors.SurfaceColors.Filled.Warning,
                _ => theme.Colors.SurfaceColors.Filled.Info,
            };

            Assert.Matches($@"(?<![:\w-])bg-{family.TailwindName}-{family.FilledShade}\b", filled);
        }
    }

    [Fact]
    public void TheForegroundShadeConstants_MatchTheRealDefaultThemesLightBorders()
    {
        TwBlazorTheme theme = ThemeBase.CreateDefaultTheme();

        foreach (var family in ThemeColorFamily.All)
        {
            var border = family.Name switch
            {
                "Primary" => theme.Border.Colors.Primary,
                "Accent" => theme.Border.Colors.Accent,
                "Success" => theme.Border.Colors.Success,
                "Danger" => theme.Border.Colors.Danger,
                "Warning" => theme.Border.Colors.Warning,
                _ => theme.Border.Colors.Info,
            };

            Assert.Matches($@"(?<![:\w-])border-{family.TailwindName}-{family.ForegroundShade}\b", border);
        }
    }

    [Fact]
    public void EveryDefaultColor_ReadsAsAVisibleBorderOnWhite_AtItsForegroundShade()
    {
        foreach (var family in ThemeColorFamily.All)
        {
            var ramp = ThemeColorRamp.Generate(family.DefaultHex)!;

            Assert.True(ramp[family.ForegroundShade].ContrastWith((255, 255, 255)) >= 3, family.Name);
        }
    }

    [Fact]
    public void PreviewText_FollowsTheSameDecision_AsTheGeneratedTheme()
    {
        var family = ThemeColorFamily.All.Single(f => f.Name == "Accent");
        var colors = new Dictionary<string, string> { ["Accent"] = "#ffffff" };

        Assert.Equal("color: #030712;", ThemePreviewStyle.FilledText(family, colors, new Dictionary<string, OnColorMode>()));
        Assert.Equal("color: #f3f4f6;", ThemePreviewStyle.FilledText(family, colors, new Dictionary<string, OnColorMode> { ["Accent"] = OnColorMode.Light }));
        Assert.Equal(string.Empty, ThemePreviewStyle.FilledText(family, new Dictionary<string, string> { ["Accent"] = family.DefaultHex }, new Dictionary<string, OnColorMode>()));
    }
}
