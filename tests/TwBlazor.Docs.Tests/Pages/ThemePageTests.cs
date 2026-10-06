using Bunit;
using TwBlazor.Components;
using TwBlazor.Docs.Services.ThemeBuilder;
using ThemePage = TwBlazor.Docs.Pages.ThemeBuilder.Theme;
using TwBlazor.Enums;

namespace TwBlazor.Docs.Tests.Pages;

public class ThemePageTests : DocsTestBase
{
    private static readonly string[] _colorIds = ["primary", "accent", "success", "danger", "warning", "info"];

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
        Assert.Contains("DefaultInputVariant = InputVariant.Outlined", Code(cut));
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

        Assert.All(_colorIds,
            name => Assert.Contains("AA", cut.Find($"[data-testid='theme-contrast-{name}']").TextContent));
        Assert.DoesNotContain("hard to see", cut.Markup);
    }

    [Fact]
    public void ExampleThemes_AreOfferedFirst_WithPurpleSelected()
    {
        var cut = Render();

        var presets = cut.FindAll("button[id^='theme-preset-']");

        Assert.Equal(ThemePreset._all.Take(8).Select(p => $"theme-preset-{p.Name.ToLowerInvariant()}"), presets.Select(p => p.Id));
        Assert.Equal("true", cut.Find("#theme-preset-purple").GetAttribute("aria-pressed"));
        Assert.Equal("false", cut.Find("#theme-preset-ocean").GetAttribute("aria-pressed"));
        Assert.True(cut.Markup.IndexOf("theme-preset-purple", StringComparison.Ordinal) < cut.Markup.IndexOf("theme-color-primary", StringComparison.Ordinal));
    }

    private static Task GoToPresetPageAsync(IRenderedComponent<ThemePage> cut, int page) =>
        cut.InvokeAsync(() => cut.FindComponent<TwPagination>().Instance.ActivePageChanged.InvokeAsync(page));

    [Fact]
    public void ExampleThemes_AreShownEightAtATime_WithPaginationButtonsBelow()
    {
        var cut = Render();

        Assert.Equal(8, cut.FindAll("button[id^='theme-preset-']").Count);
        Assert.Equal((int)Math.Ceiling(ThemePreset._all.Count / 8d), cut.FindComponent<TwPagination>().Instance.TotalPages);
        Assert.True(cut.Markup.IndexOf("theme-preset-purple", StringComparison.Ordinal) < cut.Markup.IndexOf("Example themes pages", StringComparison.Ordinal));
    }

    [Fact]
    public async Task ThePaginationButtons_ShowTheOtherExampleThemes_UntilEveryOneHasBeenSeenOnce()
    {
        var cut = Render();
        var seen = new List<string>();

        for (var page = 1; page <= cut.FindComponent<TwPagination>().Instance.TotalPages; page++)
        {
            await GoToPresetPageAsync(cut, page);
            seen.AddRange(cut.FindAll("button[id^='theme-preset-']").Select(b => b.Id!));
        }

        Assert.Equal(ThemePreset._all.Select(p => $"theme-preset-{p.Name.ToLowerInvariant()}"), seen);
    }

    [Fact]
    public async Task AnExampleThemeOnALaterPage_CanBeChosen_AndStaysSelected()
    {
        var cut = Render();
        var last = ThemePreset._all[^1];
        await GoToPresetPageAsync(cut, cut.FindComponent<TwPagination>().Instance.TotalPages);

        cut.Find($"#theme-preset-{last.Name.ToLowerInvariant()}").Click();

        Assert.Equal("true", cut.Find($"#theme-preset-{last.Name.ToLowerInvariant()}").GetAttribute("aria-pressed"));
        Assert.Equal(last.Colors["Primary"], cut.FindComponents<TwColorPicker>()[0].Instance.Value);
    }

    [Fact]
    public void ChoosingAnExampleTheme_AppliesItsColorsAndDefaults_AndSelectsIt()
    {
        var cut = Render();
        var ocean = ThemePreset._all.Single(p => p.Name == "Ocean");

        cut.Find("#theme-preset-ocean").Click();

        Assert.Equal("true", cut.Find("#theme-preset-ocean").GetAttribute("aria-pressed"));
        Assert.Equal("false", cut.Find("#theme-preset-purple").GetAttribute("aria-pressed"));
        Assert.Contains("DefaultRounded = Rounded.Lg", Code(cut));
        Assert.Contains("DefaultShadow = Shadow.Md", Code(cut));
        Assert.Equal(ocean.Colors["Primary"], cut.FindComponents<TwColorPicker>()[0].Instance.Value);
        Assert.DoesNotContain("purple", Code(cut).Split("neutralSurface")[0]);
    }

    [Fact]
    public async Task ChangingAColorAfterChoosingAnExample_DeselectsIt()
    {
        var cut = Render();
        cut.Find("#theme-preset-ocean").Click();

        await PickAsync(cut, 0, "#112233");

        Assert.Equal("false", cut.Find("#theme-preset-ocean").GetAttribute("aria-pressed"));
    }

    [Fact]
    public void ChoosingPurpleAgain_RestoresTheDefaultTheme()
    {
        var cut = Render();
        var original = Code(cut);
        cut.Find("#theme-preset-ocean").Click();

        cut.Find("#theme-preset-purple").Click();

        Assert.Equal(original, Code(cut));
    }

    [Fact]
    public void ColorsDefaultsAndComponents_ShareOneHeading_AsCollapsesInThatOrder()
    {
        var cut = Render();

        var triggers = cut.FindAll("[id^='theme-section-'][id$='-trigger']");

        Assert.Equal(["theme-section-colors-trigger", "theme-section-defaults-trigger", "theme-section-components-trigger"], triggers.Select(t => t.Id));
        Assert.Equal(1, cut.FindAll("h2").Count(h => h.TextContent == "Customize your theme"));
        Assert.DoesNotContain(cut.FindAll("h2"), h => h.TextContent is "Colors" or "Defaults" or "Components");
    }

    [Fact]
    public void AllSectionsStartClosed()
    {
        var cut = Render();

        foreach (var id in new[] { "colors", "defaults", "components" })
            Assert.Equal("false", cut.Find($"#theme-section-{id}-trigger").GetAttribute("aria-expanded"));
    }

    [Fact]
    public void AnOpenSection_StaysOpen_WhenAnEditRerendersThePage()
    {
        var cut = Render();

        cut.Find("#theme-section-components-trigger").Click();
        cut.Find("button:contains('Clear all')").Click();

        Assert.Equal("true", cut.Find("#theme-section-components-trigger").GetAttribute("aria-expanded"));
    }

    [Fact]
    public void TheComponentsHeader_SummarisesHowManyAreSelected_EvenWhileClosed()
    {
        var cut = Render();
        var header = cut.Find("#theme-section-components-trigger");
        Assert.Matches(@"(\d+) of \1 selected", header.TextContent);

        cut.Find("button:contains('Clear all')").Click();

        Assert.Matches(@"\b0 of \d+ selected", cut.Find("#theme-section-components-trigger").TextContent);
    }

    [Fact]
    public void ThePopupsInsideTheCollapses_AreNotClipped()
    {
        var cut = Render();

        foreach (var section in new[] { "colors", "defaults", "components" })
            Assert.Contains("[&>[role=region]]:overflow-visible", cut.Find($"#theme-section-{section}").ClassList);
    }

    [Fact]
    public void EachComponentGroup_IsABorderedPanel_WithAHeaderAndASelectedCount()
    {
        var cut = Render();

        var panel = cut.Find("fieldset[aria-label='Feedback components']");
        Assert.Contains("border", panel.ClassList);
        Assert.Contains("rounded-lg", panel.ClassList);
        Assert.Matches(@"(\d+) / \1", panel.FirstElementChild!.TextContent);

        cut.Find("#theme-component-twchip").Change(false);

        Assert.Matches(@"5 / 6", cut.Find("fieldset[aria-label='Feedback components']").TextContent);
    }

    [Fact]
    public void Components_ComeBeforeThePreview()
    {
        var cut = Render();

        var markup = cut.Markup;

        Assert.True(markup.IndexOf("theme-group-forms", StringComparison.Ordinal) < markup.IndexOf("theme-preview-light", StringComparison.Ordinal));
    }

    [Fact]
    public void BothPreviewPanes_SetTheirOwnTextColor_SoInheritingTextIsReadableInEither()
    {
        var cut = Render();
        var heading = Theme.Colors.NeutralText.Heading.Split(' ');

        foreach (var mode in new[] { "light", "dark" })
        {
            var classes = cut.Find($"[data-testid='theme-preview-{mode}']").ClassList;
            Assert.All(heading, token => Assert.Contains(token, classes));
        }
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
