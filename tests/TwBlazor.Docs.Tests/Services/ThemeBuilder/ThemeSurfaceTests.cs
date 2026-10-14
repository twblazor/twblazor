using TwBlazor.Docs.Services.ThemeBuilder;

namespace TwBlazor.Docs.Tests.Services.ThemeBuilder;

/// <summary>
/// Tests <see cref="ThemeSurface"/>: how a picked background recolors the default theme's neutral surfaces, in
/// the generated file and on the theme builder page.
/// </summary>
public class ThemeSurfaceTests
{
    private static readonly ThemeSurfaces _cream = ThemeSurfaces.Default with { Light = "#fdf6e3" };
    private static readonly ThemeSurfaces _navy = ThemeSurfaces.Default with { Dark = "#101820" };

    #region Generated file

    [Fact]
    public void DefaultSurfaces_LeaveTheCodeUntouched()
    {
        const string code = "bg-[oklch(100%_0_0)] dark:bg-[oklch(25.33%_0.016_252.42)]";

        Assert.False(ThemeSurface.IsCustomized(ThemeSurfaces.Default));
        Assert.Equal(code, ThemeSurface.ApplyToCode(code, ThemeSurfaces.Default));
    }

    private const string bodyCode = "bg-[oklch(100%_0_0)] bg-[oklch(98.5%_0_0)] dark:bg-[oklch(25.33%_0.016_252.42)] dark:bg-[oklch(22.2%_0.015_253)]";

    [Fact]
    public void ThePageBody_FollowsThePickedBackground_WhenItHasNoPickOfItsOwn()
    {
        var parts = ThemeSurface.ApplyToCode(bodyCode, _cream).Split(' ');

        Assert.True(OklchColor.TryParseHex("#fdf6e3", out var picked));
        Assert.Equal($"bg-{(picked with { Lightness = picked.Lightness - 0.015 }).ToTailwindValue()}", parts[1]);
        Assert.Equal("dark:bg-[oklch(22.2%_0.015_253)]", parts[3]);
    }

    [Fact]
    public void APickedPageBody_ReplacesOnlyTheBody_InItsOwnMode()
    {
        var surfaces = ThemeSurfaces.Default with { BodyDark = "#0b1020" };

        var parts = ThemeSurface.ApplyToCode(bodyCode, surfaces).Split(' ');

        Assert.True(ThemeSurface.IsCustomized(surfaces));
        Assert.True(OklchColor.TryParseHex("#0b1020", out var body));
        Assert.Equal("bg-[oklch(100%_0_0)]", parts[0]);
        Assert.Equal("bg-[oklch(98.5%_0_0)]", parts[1]);
        Assert.Equal("dark:bg-[oklch(25.33%_0.016_252.42)]", parts[2]);
        Assert.Equal($"dark:bg-{body.ToTailwindValue()}", parts[3]);
    }

    [Fact]
    public void TheEffectiveBody_IsThePickWhenThereIsOne_OrElseTheBodyMovedWithTheBackground()
    {
        Assert.Equal("#0b1020", ThemeSurface.EffectiveBody(ThemeSurfaces.Default with { BodyDark = "#0b1020" }, dark: true));

        Assert.True(OklchColor.TryParseHex(ThemeSurface.EffectiveBody(ThemeSurfaces.Default, dark: false), out var light));
        Assert.InRange(light.Lightness, 0.98, 0.99);

        Assert.True(OklchColor.TryParseHex(ThemeSurface.EffectiveBody(_cream, dark: false), out var followed));
        Assert.True(OklchColor.TryParseHex("#fdf6e3", out var cream));
        Assert.InRange(followed.Lightness, cream.Lightness - 0.02, cream.Lightness - 0.01);
    }

    [Fact]
    public void ATint_LeavesThePageBodyFollowingIt()
    {
        var tinted = ThemeSurface.Tinted(150, 1);

        Assert.Null(tinted.BodyLight);
        Assert.Null(tinted.BodyDark);
    }

    [Fact]
    public void ALightBackground_MovesEveryLightSurface_KeepingEachOnesDistanceFromIt()
    {
        var code = ThemeSurface.ApplyToCode("bg-[oklch(100%_0_0)] border-[oklch(92%_0_0)] dark:bg-[oklch(25.33%_0.016_252.42)]", _cream);
        var parts = code.Split(' ');

        Assert.True(OklchColor.TryParseHex("#fdf6e3", out var picked));
        Assert.Equal($"bg-{picked.ToTailwindValue()}", parts[0]);
        Assert.Equal($"border-{(picked with { Lightness = picked.Lightness - 0.08 }).ToTailwindValue()}", parts[1]);
        Assert.Equal("dark:bg-[oklch(25.33%_0.016_252.42)]", parts[2]);
    }

    [Fact]
    public void ADarkBackground_MovesOnlyTheDarkSurfaces()
    {
        var code = ThemeSurface.ApplyToCode("bg-[oklch(100%_0_0)] dark:bg-[oklch(25.33%_0.016_252.42)] dark:bg-[oklch(40%_0.016_253)]", _navy);

        Assert.StartsWith("bg-[oklch(100%_0_0)] ", code);
        Assert.DoesNotContain("252.42", code);
        Assert.DoesNotContain("oklch(40%_0.016_253)", code);
    }

    [Fact]
    public void TextAndSemanticColors_AreNeverTreatedAsSurfaces()
    {
        const string code = "text-[oklch(21%_0.006_285.885)] dark:text-[oklch(97.807%_0.029_256.847)] text-[oklch(65%_0.15_80)]";

        Assert.Equal(code, ThemeSurface.ApplyToCode(code, new ThemeSurfaces("#fdf6e3", "#101820")));
    }

    [Fact]
    public void ASurfaceCannotLeaveTheLightnessRange()
    {
        var code = ThemeSurface.ApplyToCode("dark:border-[oklch(21.15%_0.012_254.09)]", ThemeSurfaces.Default with { Dark = "#000000" });

        Assert.StartsWith("dark:border-[oklch(0%_", code);
    }

    [Fact]
    public void AnInvalidColor_ChangesNothing()
    {
        const string code = "bg-[oklch(100%_0_0)]";

        Assert.Equal(code, ThemeSurface.ApplyToCode(code, ThemeSurfaces.Default with { Light = "not a color" }));
        Assert.Null(ThemeSurface.TextContrast("not a color", dark: false));
    }

    #endregion

    #region Page rules

    [Fact]
    public void EverySurfaceClassInTheDefaultTheme_HasARule()
    {
        var tokens = ThemeSurface.SurfaceTokens(ThemeTemplate.Default);

        Assert.True(tokens.Count > 40);
        Assert.All(tokens, token => Assert.NotNull(ThemeSurface.Translate(token, _cream)));
    }

    [Fact]
    public void DefaultSurfaces_NeedNoRules()
    {
        Assert.Equal(string.Empty, ThemeSurface.BuildCss(ThemeTemplate.Default, ThemeSurfaces.Default));
    }

    [Fact]
    public void TheRules_JoinTailwindsUtilitiesLayer_AndReachOnlyThePage()
    {
        var css = ThemeSurface.BuildCss(ThemeTemplate.Default, _cream);
        var rules = css.Split('\n')[1..^1];

        Assert.StartsWith("@layer utilities {", css);
        Assert.EndsWith("}", css);
        Assert.All(rules, rule =>
        {
            Assert.Contains(":where(#theme-builder) ", rule);
            Assert.Contains("[class~=\"", rule);
        });
    }

    [Fact]
    public void LightRulesComeBeforeDarkOnes_AndPlainClassesBeforeTheirVariants()
    {
        var tokens = ThemeSurface.SurfaceTokens(ThemeTemplate.Default).ToList();

        Assert.True(tokens.IndexOf("bg-[oklch(100%_0_0)]") < tokens.IndexOf("focus:bg-[oklch(100%_0_0)]"));
        Assert.True(tokens.IndexOf("focus:bg-[oklch(100%_0_0)]") < tokens.IndexOf("dark:bg-[oklch(25.33%_0.016_252.42)]"));
        Assert.True(tokens.IndexOf("dark:bg-[oklch(25.33%_0.016_252.42)]") < tokens.IndexOf("dark:focus:bg-[oklch(40%_0.016_253)]"));
    }

    [Fact]
    public void APlainClass_BecomesARuleForItsOwnMode()
    {
        Assert.True(OklchColor.TryParseHex("#fdf6e3", out var picked));

        Assert.Equal(
            $":where(#theme-builder) [class~=\"bg-[oklch(100%_0_0)]\"]:where(:not(.dark, .dark *)) {{ background-color: {picked.ToCss()}; }}",
            ThemeSurface.Translate("bg-[oklch(100%_0_0)]", _cream));
    }

    [Fact]
    public void AModeLeftOnItsDefault_KeepsItsOwnValueInItsRule()
    {
        Assert.Equal(
            ":where(#theme-builder) [class~=\"dark:bg-[oklch(40%_0.016_253)]\"]:where(.dark, .dark *) { background-color: oklch(40% 0.016 253); }",
            ThemeSurface.Translate("dark:bg-[oklch(40%_0.016_253)]", _cream));
    }

    [Theory]
    [InlineData("hover:bg-[oklch(98%_0_0)]", "@media (hover: hover) { ", "]:where(:not(.dark, .dark *)):hover { background-color:")]
    [InlineData("dark:focus:bg-[oklch(40%_0.016_253)]", ":where(#theme-builder) ", "]:where(.dark, .dark *):focus { background-color:")]
    [InlineData("focus-visible:ring-offset-[oklch(100%_0_0)]", ":where(#theme-builder) ", ":focus-visible { --tw-ring-offset-color:")]
    [InlineData("peer-focus-visible:ring-offset-[oklch(100%_0_0)]", ":where(#theme-builder) ", ":is(:where(.peer):focus-visible ~ *) { --tw-ring-offset-color:")]
    [InlineData("has-[:focus-visible]:ring-offset-[oklch(100%_0_0)]", ":where(#theme-builder) ", ":has(:focus-visible) { --tw-ring-offset-color:")]
    [InlineData("data-[active=true]:bg-[oklch(95%_0_0)]", ":where(#theme-builder) ", "[data-active=\"true\"] { background-color:")]
    [InlineData("[&>tr:hover]:dark:!bg-[oklch(21.15%_0.012_254.09)]", ":where(#theme-builder) ", "]:where(.dark, .dark *)>tr:hover { background-color: oklch(21.15% 0.012 254.09) !important; }")]
    [InlineData("dark:[&_label:hover]:bg-[oklch(34%_0.018_253)]", ":where(#theme-builder) ", "]:where(.dark, .dark *) label:hover { background-color:")]
    [InlineData("[&::picker(select)]:bg-[oklch(100%_0_0)]", ":where(#theme-builder) ", "::picker(select) { background-color:")]
    [InlineData("supports-[appearance:base-select]:hover:bg-[oklch(97%_0_0)]", "@supports (appearance:base-select) { @media (hover: hover) { ", ":hover { background-color:")]
    [InlineData("divide-[oklch(94%_0_0)]", ":where(#theme-builder) :where(", " > :not(:last-child)) { border-color:")]
    [InlineData("stroke-[oklch(78%_0_0)]", ":where(#theme-builder) ", " { stroke:")]
    [InlineData("text-[oklch(91%_0_0)]", ":where(#theme-builder) ", " { color:")]
    [InlineData("ring-[oklch(100%_0_0)]", ":where(#theme-builder) ", " { --tw-ring-color:")]
    public void EachVariantAndUtility_IsTranslatedTheWayTailwindWritesIt(string token, string start, string contains)
    {
        var rule = ThemeSurface.Translate(token, _cream);

        Assert.NotNull(rule);
        Assert.StartsWith(start, rule);
        Assert.Contains(contains, rule);
        Assert.Contains($"[class~=\"{token}\"]", rule);
    }

    [Theory]
    [InlineData("group-hover:bg-[oklch(100%_0_0)]")]
    [InlineData("outline-[oklch(100%_0_0)]")]
    [InlineData("bg-[oklch(100%_0_0)]/50")]
    [InlineData("bg-purple-600")]
    public void AClassItDoesNotUnderstand_GetsNoRule_RatherThanAWrongOne(string token)
    {
        Assert.Null(ThemeSurface.Translate(token, _cream));
    }

    #endregion

    #region Contrast

    [Fact]
    public void TheDefaultBackgrounds_CarryTheThemesTextComfortably()
    {
        Assert.True(ThemeSurface.TextContrast(ThemeSurface.DefaultLightHex, dark: false) > 7);
        Assert.True(ThemeSurface.TextContrast(ThemeSurface.DefaultDarkHex, dark: true) > 7);
    }

    [Fact]
    public void ADarkBackgroundInLightMode_FailsContrast()
    {
        Assert.True(ThemeSurface.TextContrast("#222222", dark: false) < 3);
    }

    #endregion
}