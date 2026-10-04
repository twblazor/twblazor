using TwBlazor.Docs.Services.ThemeBuilder;

namespace TwBlazor.Docs.Tests.Services.ThemeBuilder;

public class ThemeColorTests
{
    [Theory]
    [InlineData("#000000", 0.0, 0.0)]
    [InlineData("#ffffff", 1.0, 0.0)]
    [InlineData("fff", 1.0, 0.0)]
    public void TryParseHex_ConvertsNeutralColors(string hex, double lightness, double chroma)
    {
        Assert.True(OklchColor.TryParseHex(hex, out var color));

        Assert.Equal(lightness, color.Lightness, 2);
        Assert.Equal(chroma, color.Chroma, 2);
    }

    [Fact]
    public void TryParseHex_MatchesTailwindsOwnValueForPurple600()
    {
        // Tailwind v4 defines purple-600 as oklch(55.8% 0.288 302.321).
        Assert.True(OklchColor.TryParseHex("#9810fa", out var color));

        Assert.Equal(0.558, color.Lightness, 2);
        Assert.Equal(0.288, color.Chroma, 2);
        Assert.Equal(302.3, color.Hue, 0);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("#12")]
    [InlineData("#12345678")]
    [InlineData("#gggggg")]
    [InlineData("rgb(1,2,3)")]
    public void TryParseHex_RejectsInvalidColors(string? hex)
    {
        Assert.False(OklchColor.TryParseHex(hex, out _));
    }

    [Fact]
    public void Formatting_UsesInvariantCultureAndTheRightSeparators()
    {
        var color = new OklchColor(0.558, 0.288, 302.321);

        Assert.Equal("oklch(55.8% 0.288 302.3)", color.ToCss());
        Assert.Equal("[oklch(55.8%_0.288_302.3)]", color.ToTailwindValue());
    }

    [Fact]
    public void Ramp_ReturnsNull_ForAnInvalidColor()
    {
        Assert.Null(ThemeColorRamp.Generate("nope"));
    }

    [Fact]
    public void Ramp_CoversEveryShade_WithThePickedColorAsShade600()
    {
        Assert.True(OklchColor.TryParseHex("#2563eb", out var picked));

        var ramp = ThemeColorRamp.Generate("#2563eb")!;

        Assert.Equal(ThemeColorRamp.Shades, ramp.Keys.Order());
        Assert.Equal(picked, ramp[600]);
    }

    [Theory]
    [InlineData("#9810fa")]
    [InlineData("#2563eb")]
    [InlineData("#000000")]
    [InlineData("#ffffff")]
    [InlineData("#fef08a")]
    public void Ramp_GetsDarkerFromShade50To950_AndKeepsTheHue(string hex)
    {
        Assert.True(OklchColor.TryParseHex(hex, out var picked));

        var ramp = ThemeColorRamp.Generate(hex)!;
        var lightness = ThemeColorRamp.Shades.Select(s => ramp[s].Lightness).ToList();

        Assert.Equal(lightness.OrderDescending(), lightness);
        Assert.All(ramp.Values.Where(c => c.Chroma > 0), c => Assert.Equal(picked.Hue, c.Hue, 3));
    }

    [Fact]
    public void Ramp_KeepsTheLightestShadeNearWhiteAndTheDarkestDark_ForAMidColor()
    {
        var ramp = ThemeColorRamp.Generate("#9810fa")!;

        Assert.True(ramp[50].Lightness > 0.95);
        Assert.True(ramp[950].Lightness < 0.35);
    }

    [Fact]
    public void Ramp_Chroma_PeaksAtTheMiddleAndFadesAtTheEnds()
    {
        var ramp = ThemeColorRamp.Generate("#9810fa")!;

        Assert.True(ramp[50].Chroma < ramp[300].Chroma);
        Assert.True(ramp[300].Chroma < ramp[600].Chroma);
        Assert.True(ramp[950].Chroma < ramp[600].Chroma);
    }

    [Fact]
    public void Family_IsNotCustomized_ForItsDefaultInAnyCase()
    {
        var primary = ThemeColorFamily.All[0];

        Assert.False(primary.IsCustomized(primary.DefaultHex));
        Assert.False(primary.IsCustomized(primary.DefaultHex.ToUpperInvariant()));
    }

    [Fact]
    public void Family_IsCustomized_OnlyForADifferentValidColor()
    {
        var primary = ThemeColorFamily.All[0];

        Assert.True(primary.IsCustomized("#112233"));
        Assert.False(primary.IsCustomized("not a color"));
        Assert.False(primary.IsCustomized(null));
    }

    [Fact]
    public void Families_AreTheSixSemanticColors_WithoutTheNeutrals()
    {
        Assert.Equal(["Primary", "Accent", "Success", "Danger", "Warning", "Info"], ThemeColorFamily.All.Select(f => f.Name));
    }
}
