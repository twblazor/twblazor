using Bunit;
using Microsoft.Extensions.DependencyInjection;
using TwBlazor.Components;
using TwBlazor.Docs.Pages.Configuration;
using TwBlazor.Docs.Services.ThemeBuilder;
using TwBlazor.Enums;
using ThemePage = TwBlazor.Docs.Pages.Configuration.Theme;

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
    public void Defaults_StartOnTheDefaultThemesOwnValues()
    {
        var cut = Render();

        Assert.Contains("DefaultRounded = Rounded.Lg", Code(cut));
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

        Assert.Contains("DefaultRounded = Rounded.Lg", Code(cut));
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
        Assert.Contains("color: #030712;", cut.Find("[data-testid='theme-preview']").InnerHtml);
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
    public void ExampleThemes_AreOfferedAboveTheSamples_WithPurpleSelected()
    {
        var cut = Render();

        var presets = cut.FindAll("button[id^='theme-preset-']");

        Assert.Equal(ThemePreset._all.Take(8).Select(p => $"theme-preset-{p.Name.ToLowerInvariant()}"), presets.Select(p => p.Id));
        Assert.Equal("true", cut.Find("#theme-preset-purple").GetAttribute("aria-pressed"));
        Assert.Equal("false", cut.Find("#theme-preset-ocean").GetAttribute("aria-pressed"));
        Assert.True(cut.Markup.IndexOf("theme-preset-purple", StringComparison.Ordinal) < cut.Markup.IndexOf("data-testid=\"theme-preview\"", StringComparison.Ordinal));
        Assert.True(cut.Find("#main-content").Contains(cut.Find("#theme-preset-purple")));
    }

    private static Task GoToPresetPageAsync(IRenderedComponent<ThemePage> cut, int page) =>
        cut.InvokeAsync(() => cut.FindComponent<TwPagination>().Instance.ActivePageChanged.InvokeAsync(page));

    [Fact]
    public void ExampleThemes_AreOneRowOfEight_WithPaginationButtonsBesideTheHeading()
    {
        var cut = Render();

        Assert.Equal(8, cut.FindAll("button[id^='theme-preset-']").Count);
        Assert.Equal((int)Math.Ceiling(ThemePreset._all.Count / 8d), cut.FindComponent<TwPagination>().Instance.TotalPages);
        Assert.Contains("@5xl:grid-cols-8", cut.Find("#theme-preset-purple").ParentElement!.ClassList);
        Assert.Same(cut.Find("#theme-presets-heading").ParentElement, cut.Find("nav[aria-label='Example themes pages']").ParentElement);
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

        Assert.Equal(["theme-section-colors-trigger", "theme-section-surfaces-trigger", "theme-section-defaults-trigger", "theme-section-components-trigger"], triggers.Select(t => t.Id));
        Assert.Equal(1, cut.FindAll("h2").Count(h => h.TextContent == "Customize your theme"));
        Assert.DoesNotContain(cut.FindAll("h2"), h => h.TextContent is "Colors" or "Defaults" or "Components");
    }

    [Fact]
    public void ColorsStartOpen_AndTheOtherSectionsClosed_SoThePanelStaysShort()
    {
        var cut = Render();

        Assert.Equal("true", cut.Find("#theme-section-colors-trigger").GetAttribute("aria-expanded"));

        foreach (var id in new[] { "surfaces", "defaults", "components" })
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

        foreach (var section in new[] { "colors", "surfaces", "defaults", "components" })
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

        Assert.Matches(@"7 / 8", cut.Find("fieldset[aria-label='Feedback components']").TextContent);
    }

    [Fact]
    public void Components_ComeBeforeThePreview()
    {
        var cut = Render();

        var markup = cut.Markup;

        Assert.True(markup.IndexOf("theme-group-forms", StringComparison.Ordinal) < markup.IndexOf("data-testid=\"theme-preview\"", StringComparison.Ordinal));
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

    #region Live page

    [Fact]
    public void Page_HasItsOwnFrame_NotTheDocsLayout()
    {
        var layout = Assert.Single(typeof(ThemePage).GetCustomAttributes(typeof(Microsoft.AspNetCore.Components.LayoutAttribute), false));

        Assert.Equal(typeof(TwBlazor.Docs.Layout.EmptyLayout), ((Microsoft.AspNetCore.Components.LayoutAttribute)layout).LayoutType);
        Assert.Equal("Theme builder", Assert.Single(Render().FindAll("h1")).TextContent);
    }

    [Fact]
    public void Page_LeavesTheColorVariablesAlone_ByDefault()
    {
        Assert.True(string.IsNullOrEmpty(Render().Find("#theme-builder").GetAttribute("style")));
    }

    [Fact]
    public async Task PickingAColor_RecolorsThisPageOnly_AndNeverReplacesTheSitesTheme()
    {
        var options = TestContext.Services.GetRequiredService<TwBlazor.Configuration.TwBlazorOptions>();
        var before = options.Theme;
        var cut = Render();

        await PickAsync(cut, 0, "#112233");

        var styled = cut.FindAll("[style*='--color-']");
        Assert.Equal("theme-builder", Assert.Single(styled).Id);
        Assert.Contains("--color-purple-600:", styled[0].GetAttribute("style"));
        Assert.Same(before, options.Theme);
        Assert.Same(Theme, options.Theme);
    }

    [Fact]
    public void TheControlsAndTheSamples_SitInsideTheRecoloredElement_SoTheWholePageFollows()
    {
        var cut = Render();
        var page = cut.Find("#theme-builder");

        Assert.True(page.Contains(cut.Find("#theme-preset-purple")));
        Assert.True(page.Contains(cut.Find("#theme-section-colors")));
        Assert.True(page.Contains(cut.Find("[data-testid='theme-preview']")));
        Assert.True(page.Contains(cut.Find("pre code")));
    }

    [Fact]
    public async Task ChangingADefault_ReachesTheSampleComponents()
    {
        var cut = Render();

        await cut.InvokeAsync(() => cut.FindComponent<TwSelect<Rounded>>().Instance.SelectedValueChanged.InvokeAsync(Rounded.Full));
        await cut.InvokeAsync(() => cut.FindComponent<TwSelect<InputVariant>>().Instance.SelectedValueChanged.InvokeAsync(InputVariant.Filled));
        await cut.InvokeAsync(() => cut.FindComponent<TwSelect<ButtonVariant>>().Instance.SelectedValueChanged.InvokeAsync(ButtonVariant.Outlined));

        var preview = cut.FindComponent<ThemeBuilderPreview>();
        Assert.All(preview.FindComponents<TwAlert>(), alert => Assert.Equal(Rounded.Full, alert.Instance.Rounded));
        Assert.All(preview.FindComponents<TwCard>(), card => Assert.Equal(Rounded.Full, card.Instance.Rounded));
        Assert.All(preview.FindComponents<TwTextfield<string>>().Where(field => field.Instance.Id is "preview-name" or "preview-email"), field => Assert.Equal(InputVariant.Filled, field.Instance.Variant));
        Assert.Equal(InputVariant.Filled, preview.FindComponent<TwDatePicker>().Instance.Variant);
        Assert.All(preview.Find("[data-testid='preview-default-buttons']").QuerySelectorAll("button"), button => Assert.DoesNotContain("text-white", button.ClassList));
    }

    [Fact]
    public void TheSamples_AreGroupedByType_AndEachGroupIsLinkedFromTheBar()
    {
        var cut = Render();

        var links = cut.FindAll("#main-content-root a[data-jump]").Select(link => link.GetAttribute("href")!).ToList();
        var sections = cut.FindAll("[data-testid='theme-preview'] > section");

        Assert.Equal(6, sections.Count);
        Assert.Equal([.. sections.Select(section => $"theme#{section.Id}"), "theme#theme-code"], links);
        Assert.All(sections, section => Assert.Equal("H2", cut.Find($"#{section.GetAttribute("aria-labelledby")}").TagName));
    }

    [Fact]
    public void TheSamples_CoverFarMoreThanTheBasics()
    {
        var preview = Render().FindComponent<ThemeBuilderPreview>();

        Assert.NotEmpty(preview.FindComponents<TwNavbar>());
        Assert.NotEmpty(preview.FindComponents<TwFooter>());
        Assert.NotEmpty(preview.FindComponents<TwDateRangePicker>());
        Assert.NotEmpty(preview.FindComponents<TwDateTimeRangePicker>());
        Assert.NotEmpty(preview.FindComponents<TwTimePicker>());
        Assert.NotEmpty(preview.FindComponents<TwColorPicker>());
        Assert.NotEmpty(preview.FindComponents<TwFileUpload>());
        Assert.NotEmpty(preview.FindComponents<TwSkeleton>());
        Assert.NotEmpty(preview.FindComponents<TwTreeList>());
        Assert.NotEmpty(preview.FindComponents<TwCarousel>());
        Assert.NotEmpty(preview.FindComponents<TwTable>());
        Assert.NotEmpty(preview.FindComponents<TwPickList<string>>());
        Assert.Equal(6, preview.FindComponents<TwChart>().Count);
        Assert.Single(preview.FindAll("input[type=radio]").Select(radio => radio.GetAttribute("name")).Distinct());
    }

    [Fact]
    public void ThePage_SetsItsOwnSurfaceAndTextColor_SoItReadsInEitherMode()
    {
        var classes = Render().Find("#theme-builder").ClassList;

        Assert.All(Theme.Colors.NeutralText.Heading.Split(' '), token => Assert.Contains(token, classes));
        Assert.All(Theme.Colors.NeutralSurface.Background.Split(' '), token => Assert.Contains(token, classes));
    }

    [Fact]
    public void TheThemeFile_ComesLast()
    {
        var markup = Render().Markup;

        Assert.True(markup.IndexOf("id=\"preview-charts\"", StringComparison.Ordinal) < markup.IndexOf("id=\"theme-code\"", StringComparison.Ordinal));
    }

    [Fact]
    public void TheSettings_SitInTheSidebar_AndTheSamplesInTheMainContent_SoBothAreInViewAtOnce()
    {
        var cut = Render();
        var sidebar = cut.FindComponent<TwSidebar>();
        var panel = cut.Find($"#{sidebar.Instance.Id} > nav");
        var main = cut.Find("#main-content");

        Assert.True(panel.Contains(cut.Find("#theme-section-colors")));
        Assert.True(panel.Contains(cut.Find("#theme-section-defaults")));
        Assert.True(panel.Contains(cut.Find("#theme-section-components")));
        Assert.True(main.Contains(cut.Find("[data-testid='theme-preview']")));
        Assert.True(main.Contains(cut.Find("#theme-code")));
        Assert.False(main.Contains(cut.Find("#theme-section-colors")));
    }

    [Fact]
    public void TheSidebar_StartsOpen_AndHasAWayBackToTheDocs()
    {
        var cut = Render();
        var sidebar = cut.FindComponent<TwSidebar>();

        Assert.True(sidebar.Instance.IsSidebarOpen);
        Assert.Contains(cut.FindAll($"#{sidebar.Instance.Id} > nav a"), link => link.TextContent.Contains("Back to the docs"));
    }

    [Fact]
    public void TheCardsOnALine_GrowToShareIt_SoNoLineIsLeftPartlyEmpty()
    {
        var preview = Render().Find("[data-testid='theme-preview']");
        var rows = preview.QuerySelectorAll("section div.flex.flex-wrap.items-stretch");

        Assert.Contains("@container", preview.ClassList);
        Assert.True(rows.Length >= 6);
        Assert.All(rows, row => Assert.All(row.Children, card => Assert.Contains("flex-1", card.ClassList)));
    }

    [Fact]
    public void TheThemeToggle_IsTheDocsOwn_AndSaysWhatItDid()
    {
        TestContext.JSInterop.Setup<bool>("themeToggle.toggle").SetResult(true);
        var cut = Render();
        Assert.Equal("Switch to dark theme", cut.Find("#theme-toggle").GetAttribute("aria-label"));
        Assert.Empty(cut.FindAll("[id^='theme-mode-']"));

        cut.Find("#theme-toggle").Click();

        Assert.Equal("Switch to light theme", cut.Find("#theme-toggle").GetAttribute("aria-label"));
        Assert.Equal("Dark theme on", cut.Find("#theme-toggle ~ [role=status]").TextContent);
        Assert.DoesNotContain("dark", cut.Find("#theme-builder").ClassList);
    }

    #endregion

    #region Showing only the ticked components

    [Fact]
    public void UntickingAComponent_HidesItsSample_AndTickingItBringsItBack()
    {
        var cut = Render();
        Assert.NotEmpty(cut.FindComponent<ThemeBuilderPreview>().FindComponents<TwSlider<int>>());

        cut.Find("#theme-component-twslider").Change(false);

        var preview = cut.FindComponent<ThemeBuilderPreview>();
        Assert.Empty(preview.FindComponents<TwSlider<int>>());
        Assert.DoesNotContain(preview.FindAll("h3"), heading => heading.TextContent == "Sliders");
        Assert.NotEmpty(preview.FindComponents<TwSwitch>());

        cut.Find("#theme-component-twslider").Change(true);

        Assert.NotEmpty(cut.FindComponent<ThemeBuilderPreview>().FindComponents<TwSlider<int>>());
    }

    [Fact]
    public void UntickingAWholeGroup_RemovesItsSection_AndItsLinkInTheBar()
    {
        var cut = Render();

        cut.Find("#theme-group-charts").Change(false);

        Assert.Empty(cut.FindAll("#preview-charts"));
        Assert.DoesNotContain("theme#preview-charts", cut.FindAll("#main-content-root a[data-jump]").Select(link => link.GetAttribute("href")));
        Assert.NotEmpty(cut.FindAll("#preview-forms"));
    }

    [Fact]
    public void WithNothingTicked_TheSamplesAreReplacedByANote()
    {
        var cut = Render();

        cut.Find("button:contains('Clear all')").Click();

        Assert.Empty(cut.FindAll("[data-testid='theme-preview'] > section"));
        Assert.Equal("status", cut.Find("[data-testid='theme-preview-empty']").GetAttribute("role"));
    }

    [Fact]
    public void EverySampledComponent_IsOneTheListOffers()
    {
        var offered = Render().FindAll("input[id^='theme-component-']").Select(box => box.Id!["theme-component-".Length..]).ToHashSet();

        Assert.All(ThemeBuilderPreview.SampledComponents, name => Assert.Contains(name.ToLowerInvariant(), offered));
    }

    #endregion

    #region Surfaces

    private static Task PickSurfaceAsync(IRenderedComponent<ThemePage> cut, bool dark, string hex) =>
        cut.InvokeAsync(() => cut.FindComponents<TwColorPicker>().Single(picker => picker.Instance.Id == (dark ? "theme-surface-dark" : "theme-surface-light")).Instance.ValueChanged.InvokeAsync(hex));

    [Fact]
    public void Surfaces_StartOnTheDefaultThemes_AndAddNothingToThePage()
    {
        var cut = Render();

        Assert.Empty(cut.FindAll("#theme-builder-surfaces"));
        Assert.Contains("bg-[oklch(100%_0_0)] dark:bg-[oklch(25.33%_0.016_252.42)]", Code(cut));
        Assert.Contains("AAA", cut.Find("[data-testid='theme-surface-contrast-light']").TextContent);
        Assert.Contains("AAA", cut.Find("[data-testid='theme-surface-contrast-dark']").TextContent);
    }

    [Fact]
    public async Task PickingALightBackground_RewritesTheLightSurfaces_AndLeavesDarkModeAlone()
    {
        var cut = Render();

        await PickSurfaceAsync(cut, dark: false, "#fdf6e3");

        var code = Code(cut);
        Assert.DoesNotContain("oklch(100%_0_0)", code);
        Assert.DoesNotContain("oklch(92%_0_0)", code);
        Assert.Contains("dark:bg-[oklch(25.33%_0.016_252.42)]", code);
        Assert.Equal("false", cut.Find("#theme-preset-purple").GetAttribute("aria-pressed"));
    }

    [Fact]
    public async Task PickingABackground_RecolorsThisPageOnly()
    {
        var before = TestContext.Services.GetRequiredService<TwBlazor.Configuration.TwBlazorOptions>().Theme;
        var cut = Render();

        await PickSurfaceAsync(cut, dark: true, "#101820");

        var style = cut.Find("#theme-builder > style#theme-builder-surfaces").TextContent;
        var rules = style.Split('\n').Where(line => line.Contains("[class~=")).ToList();
        Assert.StartsWith("@layer utilities {", style);
        Assert.NotEmpty(rules);
        Assert.All(rules, rule => Assert.Contains(":where(#theme-builder) ", rule));
        Assert.Same(before, TestContext.Services.GetRequiredService<TwBlazor.Configuration.TwBlazorOptions>().Theme);
    }

    [Fact]
    public async Task ABackgroundTheTextCannotBeReadOn_IsCalledOut()
    {
        var cut = Render();

        await PickSurfaceAsync(cut, dark: false, "#333333");

        Assert.Contains("fails contrast", cut.Find("[data-testid='theme-surface-contrast-light']").TextContent);
    }

    [Fact]
    public async Task ResetSurfaces_AndChoosingAnExample_BothPutTheDefaultSurfacesBack()
    {
        var cut = Render();
        var original = Code(cut);

        await PickSurfaceAsync(cut, dark: false, "#fdf6e3");
        cut.Find("button:contains('Reset surfaces')").Click();
        Assert.Equal(original, Code(cut));
        Assert.Empty(cut.FindAll("#theme-builder-surfaces"));

        await PickSurfaceAsync(cut, dark: true, "#101820");
        cut.Find("#theme-preset-purple").Click();
        Assert.Equal(original, Code(cut));
    }

    #endregion
}