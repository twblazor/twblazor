using TwBlazor.Docs.Services.ThemeBuilder;

namespace TwBlazor.Docs.Tests.Services.ThemeBuilder;

public class ThemePresetTests
{
    [Fact]
    public void PurpleIsFirst_AndIsExactlyTheDefaultTheme()
    {
        var purple = ThemePreset._all[0];

        Assert.Equal("Purple", purple.Name);
        Assert.Equal(ThemeDefaults.Default, purple.Defaults);
        Assert.All(ThemeColorFamily.All, f => Assert.Equal(f.DefaultHex, purple.Colors[f.Name]));
    }

    [Fact]
    public void ThereAreManyNamedVariantsBesidesPurple_WithUniqueNames()
    {
        Assert.True(ThemePreset._all.Count - 1 >= 25);
        Assert.Equal(ThemePreset._all.Count, ThemePreset._all.Select(p => p.Name).Distinct(StringComparer.OrdinalIgnoreCase).Count());
        Assert.All(ThemePreset._all, p => Assert.Matches("^[A-Za-z]+$", p.Name));
    }

    [Fact]
    public void NoTwoPresets_AreTheSameTheme()
    {
        var keys = ThemePreset._all.Select(p => string.Join(",", p.Colors.Values) + p.Defaults).ToList();

        Assert.Equal(keys.Count, keys.Distinct().Count());
    }

    [Fact]
    public void ThePresetsVaryTheDefaultsToo_NotJustTheColors()
    {
        Assert.True(ThemePreset._all.Select(p => p.Defaults.Rounded).Distinct().Count() >= 4);
        Assert.True(ThemePreset._all.Select(p => p.Defaults.Shadow).Distinct().Count() >= 3);
        Assert.Equal(3, ThemePreset._all.Select(p => p.Defaults.InputVariant).Distinct().Count());
        Assert.True(ThemePreset._all.Select(p => p.Defaults.ButtonVariant).Distinct().Count() >= 3);
    }

    [Fact]
    public void EveryPreset_DefinesAValidColorForEverySemanticColor()
    {
        foreach (var preset in ThemePreset._all)
        {
            Assert.All(ThemeColorFamily.All, f =>
            {
                Assert.True(preset.Colors.TryGetValue(f.Name, out var hex), $"{preset.Name} {f.Name}");
                Assert.True(OklchColor.TryParseHex(hex, out _), $"{preset.Name} {f.Name} {hex}");
            });
        }
    }

    [Fact]
    public void EveryPreset_HasReadableTextOnEveryFilledColor()
    {
        foreach (var preset in ThemePreset._all)
        {
            foreach (var family in ThemeColorFamily.All)
            {
                var background = family.FilledBackground(preset.Colors[family.Name])!.Value;

                Assert.True(ThemeOnColor.Contrast(OnColorMode.Auto, background) >= 4.5, $"{preset.Name} {family.Name}");
            }
        }
    }

    [Fact]
    public void EveryPreset_HasNoFaintBorderOnWhite_AtItsForegroundShade()
    {
        foreach (var preset in ThemePreset._all.Skip(1))
        {
            foreach (var family in ThemeColorFamily.All)
            {
                var ramp = ThemeColorRamp.Generate(preset.Colors[family.Name])!;

                Assert.True(ramp[family.ForegroundShade].ContrastWith((255, 255, 255)) >= 3, $"{preset.Name} {family.Name}");
            }
        }
    }

    [Fact]
    public void Matches_IsTrueForItself_FalseAfterAChange_AndIgnoresHexCase()
    {
        var ocean = ThemePreset._all.Single(p => p.Name == "Ocean");
        var colors = ocean.Colors.ToDictionary(c => c.Key, c => c.Value.ToUpperInvariant());

        Assert.True(ocean.Matches(colors, ocean.Defaults, ocean.Surfaces with { Light = ocean.Surfaces.Light.ToUpperInvariant() }));

        Assert.False(ocean.Matches(ocean.Colors, ocean.Defaults, ThemeSurfaces.Default));
        Assert.False(ocean.Matches(ocean.Colors, ThemeDefaults.Default, ocean.Surfaces));

        colors["Primary"] = "#112233";
        Assert.False(ocean.Matches(colors, ocean.Defaults, ocean.Surfaces));
    }

    [Fact]
    public void PurpleKeepsTheDefaultSurfaces_AndEveryOtherPresetTintsItsOwn()
    {
        Assert.Equal(ThemeSurfaces.Default, ThemePreset._all[0].Surfaces);

        var others = ThemePreset._all.Skip(1).ToList();
        Assert.All(others, preset => Assert.True(ThemeSurface.IsCustomized(preset.Surfaces), preset.Name));
        var clashes = others.GroupBy(preset => preset.Surfaces).Where(group => group.Count() > 1).Select(group => string.Join(" = ", group.Select(preset => preset.Name)));
        Assert.Empty(clashes);
    }

    [Fact]
    public void EveryPresetsSurfaces_AreOnlySlightlyTinted_AndKeepTheTextAAA()
    {
        foreach (var preset in ThemePreset._all.Where(p => !ThemePreset.HasClearlyTintedSurfaces(p.Tone)))
        {
            Assert.True(OklchColor.TryParseHex(preset.Surfaces.Light, out var light));
            Assert.True(light.Lightness >= 0.98, $"{preset.Name} light {light.Lightness}");
            Assert.True(light.Chroma <= 0.015, $"{preset.Name} light chroma {light.Chroma}");
            Assert.True(ThemeSurface.TextContrast(preset.Surfaces.Light, dark: false) >= 7, $"{preset.Name} light");
            Assert.True(ThemeSurface.TextContrast(preset.Surfaces.Dark, dark: true) >= 7, $"{preset.Name} dark");
        }
    }

    [Fact]
    public void ThereAreTwentyPastelThemes()
    {
        Assert.Equal(20, ThemePreset._all.Count(p => p.Tone == ThemeTone.Pastel));
    }

    [Theory]
    [InlineData("Vivid", 10)]
    [InlineData("Fun", 10)]
    public void ThereAreVividAndFunThemes(string tone, int count)
    {
        Assert.Equal(count, ThemePreset._all.Count(p => p.Tone == Enum.Parse<ThemeTone>(tone)));
    }

    [Theory]
    [InlineData("Terminal")]
    [InlineData("Synthwave")]
    [InlineData("Handheld")]
    public void TheFunThemes_IncludeTheRetroOnes(string name)
    {
        Assert.Equal(ThemeTone.Fun, ThemePreset._all.Single(p => p.Name == name).Tone);
    }

    [Fact]
    public void VividThemes_AreMoreSaturatedThanPastelOnes()
    {
        static double Chroma(ThemePreset preset) =>
            OklchColor.TryParseHex(preset.Colors["Primary"], out var color) ? color.Chroma : 0;

        var weakestVivid = ThemePreset._all.Where(p => p.Tone == ThemeTone.Vivid).Min(Chroma);
        var strongestPastel = ThemePreset._all.Where(p => p.Tone == ThemeTone.Pastel).Max(Chroma);

        Assert.True(weakestVivid > strongestPastel, $"{weakestVivid} vs {strongestPastel}");
    }

    [Fact]
    public void ThePresets_AreGroupedByCategory_InTheOrderTheFiltersOffer()
    {
        var tones = ThemePreset._all.Skip(1).Select(p => p.Tone).ToList();

        Assert.Equal(Enum.GetValues<ThemeTone>(), tones.Distinct());
        Assert.Equal(tones.Distinct().Count(), tones.Zip(tones.Skip(1)).Count(pair => pair.First != pair.Second) + 1);
    }

    [Fact]
    public void ClearlyTintedSurfaces_StillKeepTheTextAAA()
    {
        foreach (var preset in ThemePreset._all.Where(p => ThemePreset.HasClearlyTintedSurfaces(p.Tone)))
        {
            Assert.True(OklchColor.TryParseHex(preset.Surfaces.Light, out var light));
            Assert.True(light.Lightness >= 0.96, $"{preset.Name} light {light.Lightness}");
            Assert.True(light.Chroma >= 0.015, $"{preset.Name} light chroma {light.Chroma}");
            Assert.True(ThemeSurface.TextContrast(preset.Surfaces.Light, dark: false) >= 7, $"{preset.Name} light");
            Assert.True(ThemeSurface.TextContrast(ThemeSurface.EffectiveBody(preset.Surfaces, dark: false), dark: false) >= 7, $"{preset.Name} light body");
            Assert.True(ThemeSurface.TextContrast(preset.Surfaces.Dark, dark: true) >= 7, $"{preset.Name} dark");
        }
    }

    [Fact]
    public void ATint_LeansTowardThePrimarysHue()
    {
        var forest = ThemePreset._all.Single(p => p.Name == "Forest");
        Assert.True(OklchColor.TryParseHex(forest.Colors["Primary"], out var primary));
        Assert.True(OklchColor.TryParseHex(forest.Surfaces.Dark, out var dark));

        var gap = Math.Abs(primary.Hue - dark.Hue) % 360;
        Assert.True(Math.Min(gap, 360 - gap) < 30, $"{primary.Hue} vs {dark.Hue}");
    }

    [Fact]
    public void EveryColorFamily_HasSeveralThemes()
    {
        Assert.All(Enum.GetValues<ThemeTone>(), tone => Assert.True(ThemePreset._all.Count(p => p.Tone == tone) >= 3, tone.ToString()));
    }

    [Theory]
    [InlineData("Ocean", "Blue")]
    [InlineData("Forest", "Green")]
    [InlineData("Bubblegum", "Pink")]
    [InlineData("Cherry", "Red")]
    [InlineData("Espresso", "Orange")]
    [InlineData("Slate", "Neutral")]
    [InlineData("Flamingo", "Pink")]
    public void APreset_IsFiledUnderItsColor(string name, string tone)
    {
        Assert.Equal(Enum.Parse<ThemeTone>(tone), ThemePreset._all.Single(p => p.Name == name).Tone);
    }

    [Theory]
    [InlineData("Flamingo")]
    [InlineData("Tangerine")]
    public void APreset_UsesLargeCorners_RatherThanPills(string name)
    {
        Assert.Equal(TwBlazor.Enums.Rounded.Lg, ThemePreset._all.Single(p => p.Name == name).Defaults.Rounded);
    }

    [Fact]
    public void ThereAreSomeNewOnesForFun()
    {
        var names = ThemePreset._all.Select(p => p.Name).ToHashSet();

        Assert.Superset(new HashSet<string> { "Blueberry", "Lagoon", "Matcha", "Peacock", "Neon", "Flamingo", "Sakura", "Raspberry", "Tangerine", "Lemon", "Cocoa", "Royal", "Ink" }, names);
        Assert.True(ThemePreset._all.Count >= 46);
    }

    [Fact]
    public void EveryPreset_ProducesAThemeDifferentFromTheDefault_ExceptPurple()
    {
        var template = ThemeTemplate.Default;
        var all = template.Blocks.Select(b => b.TypeName).ToHashSet();
        var baseline = ThemeCodeGenerator.Generate(template, all, new Dictionary<string, string>());

        foreach (var preset in ThemePreset._all)
        {
            var code = ThemeCodeGenerator.Generate(template, all, preset.Colors, preset.Defaults);

            Assert.Equal(preset.Name == "Purple", code == baseline);
        }
    }
}
