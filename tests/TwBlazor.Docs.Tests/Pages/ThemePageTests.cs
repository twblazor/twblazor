using Bunit;
using TwBlazor.Components;
using TwBlazor.Docs.Services.ThemeBuilder;
using ThemePage = TwBlazor.Docs.Pages.ThemeBuilder.Theme;
using TwBlazor.Enums;

namespace TwBlazor.Docs.Tests.Pages;

public class ThemePageTests : DocsTestBase
{
    private IRenderedComponent<ThemePage> Render() => TestContext.Render<ThemePage>();

    private static string Code(IRenderedComponent<ThemePage> cut) => cut.Find("pre code").TextContent;

    [Fact]
    public void Page_StartsWithEveryComponentSelected_AndShowsTheGeneratedTheme()
    {
        var cut = Render();

        Assert.All(cut.FindAll("input[id^='theme-component-']"), box => Assert.True(box.HasAttribute("checked")));
        Assert.Contains("public static class Theme", Code(cut));
        Assert.Contains("new TwButtonTheme", Code(cut));
    }

    [Fact]
    public void UncheckingAComponent_RemovesItsTheme_ButKeepsThemesOtherComponentsStillNeed()
    {
        var cut = Render();

        cut.FindAll("input[id^='theme-group-']").ToList().ForEach(g => { if (g.HasAttribute("checked")) g.Change(false); });
        cut.Find("#theme-component-twslider").Change(true);

        var code = Code(cut);
        Assert.Contains("new TwSliderTheme", code);
        Assert.DoesNotContain("new TwCalendarTheme", code);
    }

    [Fact]
    public void CheckingAGroup_SelectsAllItsComponents()
    {
        var cut = Render();
        cut.Find("button:contains('Clear all')").Click();
        Assert.DoesNotContain("new TwSliderTheme", Code(cut));

        cut.Find("#theme-group-forms").Change(true);

        Assert.True(cut.Find("#theme-component-twslider").HasAttribute("checked"));
        Assert.Contains("new TwSliderTheme", Code(cut));
        Assert.DoesNotContain("new TwCalendarTheme", Code(cut));
    }

    [Fact]
    public void UncheckingAFullGroup_ClearsAllItsComponents()
    {
        var cut = Render();

        cut.Find("#theme-group-feedback").Change(false);

        Assert.False(cut.Find("#theme-component-twchip").HasAttribute("checked"));
        Assert.True(cut.Find("#theme-component-twslider").HasAttribute("checked"));
    }

    [Fact]
    public void AGroupWithMixedChildren_ShowsAsIndeterminate()
    {
        var cut = Render();

        cut.Find("#theme-component-twchip").Change(false);

        var feedback = cut.Find("#theme-group-feedback").ParentElement!;
        Assert.NotEmpty(feedback.QuerySelectorAll("svg rect"));
        Assert.Empty(cut.Find("#theme-group-forms").ParentElement!.QuerySelectorAll("svg rect"));
    }

    [Fact]
    public void ClearAll_RemovesEveryComponentTheme()
    {
        var cut = Render();

        cut.Find("button:contains('Clear all')").Click();

        Assert.DoesNotContain("new TwButtonTheme", Code(cut));
        Assert.Contains("0 of", cut.Markup);
    }

    [Fact]
    public void SelectAll_RestoresEveryComponentTheme()
    {
        var cut = Render();
        cut.Find("button:contains('Clear all')").Click();

        cut.Find("button:contains('Select all')").Click();

        Assert.Contains("new TwButtonTheme", Code(cut));
    }

    [Fact]
    public void Preview_LeavesTheColorVariablesAlone_ByDefault()
    {
        var cut = Render();

        var style = cut.Find("[data-testid='theme-preview-light']").GetAttribute("style");

        Assert.True(string.IsNullOrEmpty(style));
    }

    [Fact]
    public void ResetColors_RestoresTheDefaultPreview_AndCode()
    {
        var cut = Render();

        cut.Find("button:contains('Reset colors')").Click();

        Assert.Contains("purple-600", Code(cut));
        Assert.DoesNotContain("oklch(", Code(cut).Split("neutralSurface")[0]);
    }

    [Fact]
    public void Page_IsTheThemePage()
    {
        var route = Assert.Single(typeof(ThemePage).GetCustomAttributes(typeof(Microsoft.AspNetCore.Components.RouteAttribute), false));

        Assert.Equal("/theme", ((Microsoft.AspNetCore.Components.RouteAttribute)route).Template);
    }

    [Fact]
    public void Preview_SitsSideBySide_WheneverTheCardIsWideEnough()
    {
        var cut = Render();

        var grid = cut.Find("[data-testid='theme-preview-light']").ParentElement!;

        Assert.Contains("@lg:grid-cols-2", grid.ClassList);
        Assert.DoesNotContain("@container", grid.ClassList);
        Assert.Contains("@container", grid.ParentElement!.ClassList);
        Assert.Same(grid, cut.Find("[data-testid='theme-preview-dark']").ParentElement);
    }

    [Fact]
    public void Preview_ShowsLightAndDarkModeAtOnce()
    {
        var cut = Render();

        var light = cut.Find("[data-testid='theme-preview-light']");
        var dark = cut.Find("[data-testid='theme-preview-dark']");

        Assert.Contains("theme-light", light.ClassList);
        Assert.Contains("dark", dark.ClassList);
        Assert.DoesNotContain("dark", light.ClassList);
    }

    [Fact]
    public void Preview_GivesEachPaneItsOwnRadioGroup()
    {
        var cut = Render();

        var names = cut.FindAll("input[type=radio]").Select(r => r.GetAttribute("name")).Distinct().ToList();

        Assert.Equal(2, names.Count);
    }

    [Fact]
    public void Preview_ShowsMoreThanTheBasics()
    {
        var cut = Render();
        var light = cut.Find("[data-testid='theme-preview-light']");

        Assert.NotEmpty(light.QuerySelectorAll("[role=tablist]"));
        Assert.NotEmpty(light.QuerySelectorAll("nav"));
        Assert.NotEmpty(light.QuerySelectorAll("label"));
    }

    [Fact]
    public void Defaults_StartOnTheDefaultThemesOwnValues()
    {
        var cut = Render();

        Assert.Contains("DefaultRounded = Rounded.Md", Code(cut));
        Assert.Contains("DefaultShadow = Shadow.Sm", Code(cut));
        Assert.Contains("DefaultInputVariant = InputVariant.Filled", Code(cut));
        Assert.DoesNotContain("DefaultVariant = ButtonVariant", Code(cut));
    }

    [Fact]
    public async Task ChangingTheButtonDefault_IsWrittenIntoTheButtonTheme()
    {
        var cut = Render();

        await cut.InvokeAsync(() => cut.FindComponent<TwSelect<ButtonVariant>>().Instance.SelectedValueChanged.InvokeAsync(ButtonVariant.Outlined));

        Assert.Contains("DefaultVariant = ButtonVariant.Outlined", Code(cut));
    }

    [Fact]
    public async Task ResetDefaults_RestoresTheDefaultTheme()
    {
        var cut = Render();
        await cut.InvokeAsync(() => cut.FindComponent<TwSelect<Rounded>>().Instance.SelectedValueChanged.InvokeAsync(Rounded.Full));
        Assert.Contains("DefaultRounded = Rounded.Full", Code(cut));

        cut.Find("button:contains('Reset defaults')").Click();

        Assert.Contains("DefaultRounded = Rounded.Md", Code(cut));
    }

    private static Task PickAsync(IRenderedComponent<ThemePage> cut, int index, string hex) =>
        cut.InvokeAsync(() => cut.FindComponents<TwColorPicker>()[index].Instance.ValueChanged.InvokeAsync(hex));

    [Fact]
    public async Task PickingAWhiteColor_GivesItDarkTextOnFilledSurfaces_AndExplainsWhy()
    {
        var cut = Render();

        await PickAsync(cut, 1, "#ffffff");

        Assert.Contains("Dark text on filled", cut.Find("[data-testid='theme-contrast-accent']").TextContent);
        Assert.Contains("text-gray-950", Code(cut));
        Assert.Contains("hard to see", cut.Markup);
        Assert.Contains("color: #030712;", cut.Find("[data-testid='theme-preview-light']").InnerHtml);
    }

    [Fact]
    public async Task TheTextOnColorSelect_OverridesTheAutomaticChoice()
    {
        var cut = Render();
        await PickAsync(cut, 1, "#ffffff");

        await cut.InvokeAsync(() => cut.FindComponents<TwSelect<OnColorMode>>()[1].Instance.SelectedValueChanged.InvokeAsync(OnColorMode.Light));

        Assert.Contains("Light text on filled", cut.Find("[data-testid='theme-contrast-accent']").TextContent);
        Assert.Contains("fails contrast", cut.Find("[data-testid='theme-contrast-accent']").TextContent);
    }

    [Fact]
    public void DefaultColors_ShowAPassingContrast_AndNoWarning()
    {
        var cut = Render();

        Assert.All(new[] { "primary", "accent", "success", "danger", "warning", "info" },
            name => Assert.Contains("AA", cut.Find($"[data-testid='theme-contrast-{name}']").TextContent));
        Assert.DoesNotContain("hard to see", cut.Markup);
    }

    [Fact]
    public void Page_OffersAColorPickerForEverySemanticColor_AndNoneForTheNeutrals()
    {
        var cut = Render();

        foreach (var name in new[] { "primary", "accent", "success", "danger", "warning", "info" })
            Assert.NotEmpty(cut.FindAll($"#theme-color-{name}"));

        Assert.Empty(cut.FindAll("#theme-color-light"));
        Assert.Empty(cut.FindAll("#theme-color-dark"));
    }
}
