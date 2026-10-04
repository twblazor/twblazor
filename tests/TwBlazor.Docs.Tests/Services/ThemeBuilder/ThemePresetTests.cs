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

        Assert.True(ocean.Matches(colors, ocean.Defaults));

        colors["Primary"] = "#112233";
        Assert.False(ocean.Matches(colors, ocean.Defaults));
        Assert.False(ocean.Matches(ocean.Colors, ThemeDefaults.Default));
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
