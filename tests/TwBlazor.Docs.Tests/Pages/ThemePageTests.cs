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

    private static readonly string[] _stickyPanelClasses = ["lg:sticky", "lg:top-0", "lg:h-dvh", "lg:overflow-y-auto"];

    // The charts and data table render just after the first paint, so a test starts once they are in and that
    // render has finished. bUnit renders off the test thread, so an event fired mid-render could otherwise be lost.
    private async Task<IRenderedComponent<ThemePage>> RenderAsync()
    {
        var cut = TestContext.Render<ThemePage>();
        await cut.WaitForElementAsync("#preview-charts");
        await cut.InvokeAsync(() => { });
        return cut;
    }

    // The theme is only generated on request, so reading it means asking for it with the current settings.
    private static string Code(IRenderedComponent<ThemePage> cut)
    {
        cut.Find("#theme-generate-panel").Click();
        return cut.Find("pre code").TextContent;
    }

    private static void OpenTab(IRenderedComponent<ThemePage> cut, string label) =>
        cut.Find($"#theme-settings [role=tab]:contains('{label}')").Click();

    private async Task<IRenderedComponent<ThemePage>> RenderOnTabAsync(string label)
    {
        var cut = await RenderAsync();
        OpenTab(cut, label);
        return cut;
    }

    [Fact]
    public async Task Page_StartsWithEveryComponentSelected_AndShowsTheGeneratedTheme()
    {
        var cut = await RenderOnTabAsync("Components");

        var boxes = cut.FindAll("input[id^='theme-component-']");
        Assert.NotEmpty(boxes);
        Assert.All(boxes, box => Assert.True(box.HasAttribute("checked")));
        Assert.Contains("public static class Theme", Code(cut));
        Assert.Contains("new TwButtonTheme", Code(cut));
    }

    [Fact]
    public async Task UncheckingAComponent_RemovesItsTheme_ButKeepsThemesOtherComponentsStillNeed()
    {
        var cut = await RenderOnTabAsync("Components");

        cut.FindAll("input[id^='theme-group-']").ToList().ForEach(g => { if (g.HasAttribute("checked")) g.Change(false); });
        cut.Find("#theme-component-twslider").Change(true);

        var code = Code(cut);
        Assert.Contains("new TwSliderTheme", code);
        Assert.DoesNotContain("new TwCalendarTheme", code);
    }

    [Fact]
    public async Task CheckingAGroup_SelectsAllItsComponents()
    {
        var cut = await RenderOnTabAsync("Components");
        cut.Find("button:contains('Clear all')").Click();
        Assert.DoesNotContain("new TwSliderTheme", Code(cut));

        cut.Find("#theme-group-forms").Change(true);

        Assert.True(cut.Find("#theme-component-twslider").HasAttribute("checked"));
        Assert.Contains("new TwSliderTheme", Code(cut));
        Assert.DoesNotContain("new TwCalendarTheme", Code(cut));
    }

    [Fact]
    public async Task UncheckingAFullGroup_ClearsAllItsComponents()
    {
        var cut = await RenderOnTabAsync("Components");

        cut.Find("#theme-group-feedback").Change(false);

        Assert.False(cut.Find("#theme-component-twchip").HasAttribute("checked"));
        Assert.True(cut.Find("#theme-component-twslider").HasAttribute("checked"));
    }

    [Fact]
    public async Task AGroupWithMixedChildren_ShowsAsIndeterminate()
    {
        var cut = await RenderOnTabAsync("Components");

        cut.Find("#theme-component-twchip").Change(false);

        var feedback = cut.Find("#theme-group-feedback").ParentElement!;
        Assert.NotEmpty(feedback.QuerySelectorAll("svg rect"));
        Assert.Empty(cut.Find("#theme-group-forms").ParentElement!.QuerySelectorAll("svg rect"));
    }

    [Fact]
    public async Task ClearAll_RemovesEveryComponentTheme()
    {
        var cut = await RenderOnTabAsync("Components");

        cut.Find("button:contains('Clear all')").Click();

        Assert.DoesNotContain("new TwButtonTheme", Code(cut));
        Assert.Contains("0 of", cut.Markup);
    }

    [Fact]
    public async Task SelectAll_RestoresEveryComponentTheme()
    {
        var cut = await RenderOnTabAsync("Components");
        cut.Find("button:contains('Clear all')").Click();

        cut.Find("button:contains('Select all')").Click();

        Assert.Contains("new TwButtonTheme", Code(cut));
    }

    [Fact]
    public async Task ResetColors_RestoresTheDefaultPreview_AndCode()
    {
        var cut = await RenderAsync();

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
    public async Task Defaults_StartOnTheDefaultThemesOwnValues()
    {
        var cut = await RenderAsync();

        Assert.Contains("DefaultRounded = Rounded.Lg", Code(cut));
        Assert.Contains("DefaultShadow = Shadow.Sm", Code(cut));
        Assert.Contains("DefaultInputVariant = InputVariant.Outlined", Code(cut));
        Assert.DoesNotContain("DefaultVariant = ButtonVariant", Code(cut));
    }

    [Fact]
    public async Task ChangingTheButtonDefault_IsWrittenIntoTheButtonTheme()
    {
        var cut = await RenderOnTabAsync("Style");

        await cut.InvokeAsync(() => cut.FindComponent<TwSelect<ButtonVariant>>().Instance.SelectedValueChanged.InvokeAsync(ButtonVariant.Outlined));

        Assert.Contains("DefaultVariant = ButtonVariant.Outlined", Code(cut));
    }

    [Fact]
    public async Task ResetDefaults_RestoresTheDefaultTheme()
    {
        var cut = await RenderOnTabAsync("Style");
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
        var cut = await RenderAsync();

        await PickAsync(cut, 1, "#ffffff");

        Assert.Contains("Dark text on filled", cut.Find("[data-testid='theme-contrast-accent']").TextContent);
        Assert.Contains("text-gray-950", Code(cut));
        Assert.Contains("hard to see", cut.Markup);
        Assert.Contains("color: #030712;", cut.Find("[data-testid='theme-preview']").InnerHtml);
    }

    [Fact]
    public async Task TheTextOnColorSelect_OverridesTheAutomaticChoice()
    {
        var cut = await RenderAsync();
        await PickAsync(cut, 1, "#ffffff");

        await cut.InvokeAsync(() => cut.FindComponents<TwSelect<OnColorMode>>()[1].Instance.SelectedValueChanged.InvokeAsync(OnColorMode.Light));

        Assert.Contains("Light text on filled", cut.Find("[data-testid='theme-contrast-accent']").TextContent);
        Assert.Contains("fails contrast", cut.Find("[data-testid='theme-contrast-accent']").TextContent);
    }

    [Fact]
    public async Task DefaultColors_ShowAPassingContrast_AndNoWarning()
    {
        var cut = await RenderAsync();

        Assert.All(_colorIds,
            name => Assert.Contains("AA", cut.Find($"[data-testid='theme-contrast-{name}']").TextContent));
        Assert.DoesNotContain("hard to see", cut.Markup);
    }

    [Fact]
    public async Task ExampleThemes_AreOfferedAboveTheSamples_WithPurpleSelected()
    {
        var cut = await RenderAsync();

        var presets = cut.FindAll("button[id^='theme-preset-']");

        Assert.Equal(ThemePreset._all.Take(8).Select(p => $"theme-preset-{p.Name.ToLowerInvariant()}"), presets.Select(p => p.Id));
        Assert.Equal("true", cut.Find("#theme-preset-purple").GetAttribute("aria-pressed"));
        Assert.Equal("false", cut.Find("#theme-preset-ocean").GetAttribute("aria-pressed"));
        Assert.True(cut.Markup.IndexOf("theme-preset-purple", StringComparison.Ordinal) < cut.Markup.IndexOf("data-testid=\"theme-preview\"", StringComparison.Ordinal));
        Assert.True(cut.Find("#theme-controls").Contains(cut.Find("#theme-preset-purple")));
    }

    private static Task GoToPresetPageAsync(IRenderedComponent<ThemePage> cut, int page) =>
        cut.InvokeAsync(() => cut.FindComponent<TwPagination>().Instance.ActivePageChanged.InvokeAsync(page));

    [Fact]
    public async Task ExampleThemes_AreTwoRowsOfFour_WithDensePaginationBesideTheHeading()
    {
        var cut = await RenderAsync();
        var pagination = cut.FindComponent<TwPagination>();

        Assert.Equal(8, cut.FindAll("button[id^='theme-preset-']").Count);
        Assert.Equal((int)Math.Ceiling(ThemePreset._all.Count / 8d), pagination.Instance.TotalPages);
        Assert.True(pagination.Instance.Dense);
        Assert.Contains("grid-cols-4", cut.Find("#theme-preset-purple").ParentElement!.ClassList);
        Assert.True(cut.Find("#theme-presets-heading").ParentElement!.Contains(cut.Find("nav[aria-label='Example themes pages']")));
    }

    [Fact]
    public async Task TheChosenExampleTheme_StandsOutByMoreThanColor()
    {
        var cut = await RenderAsync();

        Assert.Contains("ring-1", cut.Find("#theme-preset-purple").ClassList);
        Assert.DoesNotContain("ring-1", cut.Find("#theme-preset-ocean").ClassList);
    }

    [Fact]
    public async Task ThePaginationButtons_ShowTheOtherExampleThemes_UntilEveryOneHasBeenSeenOnce()
    {
        var cut = await RenderAsync();
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
        var cut = await RenderAsync();
        var last = ThemePreset._all[^1];
        await GoToPresetPageAsync(cut, cut.FindComponent<TwPagination>().Instance.TotalPages);

        cut.Find($"#theme-preset-{last.Name.ToLowerInvariant()}").Click();

        Assert.Equal("true", cut.Find($"#theme-preset-{last.Name.ToLowerInvariant()}").GetAttribute("aria-pressed"));
        Assert.Equal(last.Colors["Primary"], cut.FindComponents<TwColorPicker>()[0].Instance.Value);
    }

    [Fact]
    public async Task ChoosingAnExampleTheme_AppliesItsColorsAndDefaults_AndSelectsIt()
    {
        var cut = await RenderAsync();
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
    public async Task ChoosingAnExampleTheme_AlsoTintsTheBackgrounds()
    {
        var cut = await RenderAsync();
        var ocean = ThemePreset._all.Single(p => p.Name == "Ocean");

        cut.Find("#theme-preset-ocean").Click();

        Assert.Equal(ocean.Surfaces.Light, cut.FindComponents<TwColorPicker>().Single(p => p.Instance.Id == "theme-surface-light").Instance.Value);
        Assert.Equal(ocean.Surfaces.Dark, cut.FindComponents<TwColorPicker>().Single(p => p.Instance.Id == "theme-surface-dark").Instance.Value);
        Assert.NotEmpty(cut.FindAll("#theme-builder-surfaces"));
        Assert.DoesNotContain("oklch(100%_0_0)", Code(cut));
    }

    [Fact]
    public async Task ChangingABackgroundAfterChoosingAnExample_DeselectsIt()
    {
        var cut = await RenderAsync();
        cut.Find("#theme-preset-ocean").Click();

        await cut.InvokeAsync(() => cut.FindComponents<TwColorPicker>().Single(p => p.Instance.Id == "theme-surface-light").Instance.ValueChanged.InvokeAsync("#ffffff"));

        Assert.Equal("false", cut.Find("#theme-preset-ocean").GetAttribute("aria-pressed"));
    }

    [Fact]
    public async Task TheColorFilters_StartOnAll_AndOfferEveryColorFamily()
    {
        var cut = await RenderAsync();
        var chips = cut.FindAll("#theme-tone-filters button");

        Assert.Equal(["All", "Purple", "Blue", "Green", "Pink", "Red", "Orange", "Neutral"], chips.Select(chip => chip.TextContent.Trim()));
        Assert.Equal("true", chips[0].GetAttribute("aria-pressed"));
        Assert.All(chips.Skip(1), chip => Assert.Equal("false", chip.GetAttribute("aria-pressed")));
        Assert.All(chips.Skip(1), chip => Assert.Equal("true", chip.QuerySelector("span")!.GetAttribute("aria-hidden")));
    }

    [Fact]
    public async Task AColorFilter_ShowsOnlyThatColorsThemes_FromTheFirstPage()
    {
        var cut = await RenderAsync();
        await GoToPresetPageAsync(cut, 2);

        cut.Find("#theme-tone-green").Click();

        var greens = ThemePreset._all.Where(p => p.Tone == ThemeTone.Green).ToList();
        Assert.Equal(greens.Take(8).Select(p => $"theme-preset-{p.Name.ToLowerInvariant()}"), cut.FindAll("button[id^='theme-preset-']").Select(b => b.Id));
        Assert.Equal(1, cut.FindComponent<TwPagination>().Instance.ActivePage);
        Assert.Equal((int)Math.Ceiling(greens.Count / 8d), cut.FindComponent<TwPagination>().Instance.TotalPages);
        Assert.Equal("true", cut.Find("#theme-tone-green").GetAttribute("aria-pressed"));
        Assert.Equal($"Showing {greens.Count} green example themes", cut.Find("#theme-controls p[role=status]").TextContent);
    }

    [Fact]
    public async Task All_BringsEveryThemeBack()
    {
        var cut = await RenderAsync();
        cut.Find("#theme-tone-neutral").Click();

        cut.Find("#theme-tone-all").Click();

        Assert.Equal(8, cut.FindAll("button[id^='theme-preset-']").Count);
        Assert.Equal($"Showing all {ThemePreset._all.Count} example themes", cut.Find("#theme-controls p[role=status]").TextContent);
    }

    [Fact]
    public async Task ChangingAColorAfterChoosingAnExample_DeselectsIt()
    {
        var cut = await RenderAsync();
        cut.Find("#theme-preset-ocean").Click();

        await PickAsync(cut, 0, "#112233");

        Assert.Equal("false", cut.Find("#theme-preset-ocean").GetAttribute("aria-pressed"));
    }

    [Fact]
    public async Task ChoosingPurpleAgain_RestoresTheDefaultTheme()
    {
        var cut = await RenderAsync();
        var original = Code(cut);
        cut.Find("#theme-preset-ocean").Click();

        cut.Find("#theme-preset-purple").Click();

        Assert.Equal(original, Code(cut));
    }

    [Fact]
    public async Task TheSettings_AreDenseTabs_ColorsStyleAndComponents_UnderOneHeading()
    {
        var cut = await RenderAsync();

        var tabs = cut.FindAll("#theme-settings [role=tab]");

        Assert.Equal(["Colors", "Style", "Components"], tabs.Select(tab => tab.TextContent.Trim()));
        Assert.True(cut.FindComponent<TwTabContainer>().Instance.Dense);
        Assert.Equal("Customize", Assert.Single(cut.FindAll("#theme-controls h2")).TextContent);
    }

    [Fact]
    public async Task TheColorsTab_StartsSelected()
    {
        var cut = await RenderAsync();

        Assert.Equal("true", cut.Find("#theme-settings [role=tab]:contains('Colors')").GetAttribute("aria-selected"));
        Assert.NotEmpty(cut.FindAll("#theme-color-primary"));
    }

    [Fact]
    public async Task TheChosenTab_StaysChosen_WhenAnEditRerendersThePage()
    {
        var cut = await RenderOnTabAsync("Components");

        cut.Find("button:contains('Clear all')").Click();

        Assert.Equal("true", cut.Find("#theme-settings [role=tab]:contains('Components')").GetAttribute("aria-selected"));
    }

    [Fact]
    public async Task TheComponentsTab_SummarisesHowManyAreSelected()
    {
        var cut = await RenderOnTabAsync("Components");
        Assert.Matches(@"(\d+) of \1 selected", cut.Find("#theme-components-summary").TextContent);

        cut.Find("button:contains('Clear all')").Click();

        Assert.Matches(@"^0 of \d+ selected$", cut.Find("#theme-components-summary").TextContent);
        Assert.Equal("polite", cut.Find("#theme-components-summary").GetAttribute("aria-live"));
    }

    [Fact]
    public async Task TheControls_UseTheirDenseVariants()
    {
        var cut = await RenderAsync();

        Assert.All(cut.FindComponents<TwColorPicker>().Where(picker => picker.Instance.Id!.StartsWith("theme-", StringComparison.Ordinal)), picker => Assert.True(picker.Instance.Dense));
        Assert.All(cut.FindComponents<TwSelect<OnColorMode>>(), select => Assert.True(select.Instance.Dense));

        OpenTab(cut, "Style");

        Assert.True(cut.FindComponent<TwSelect<Rounded>>().Instance.Dense);
        Assert.True(cut.FindComponent<TwSelect<Shadow>>().Instance.Dense);
    }

    [Fact]
    public async Task EachTextOnFillSelect_IsNamedForItsColor()
    {
        var cut = await RenderAsync();

        Assert.Equal(_colorIds.Select(id => $"Text on fill for {char.ToUpperInvariant(id[0])}{id[1..]}"), cut.FindComponents<TwSelect<OnColorMode>>().Select(select => select.Instance.AriaLabel));
    }

    [Fact]
    public async Task EachComponentGroup_IsABorderedPanel_WithAHeaderAndASelectedCount()
    {
        var cut = await RenderOnTabAsync("Components");

        var panel = cut.Find("fieldset[aria-label='Feedback components']");
        Assert.Contains("border", panel.ClassList);
        Assert.Contains("rounded-lg", panel.ClassList);
        Assert.Matches(@"(\d+) / \1", panel.FirstElementChild!.TextContent);

        cut.Find("#theme-component-twchip").Change(false);

        Assert.Matches(@"7 / 8", cut.Find("fieldset[aria-label='Feedback components']").TextContent);
    }

    [Fact]
    public async Task Components_ComeBeforeThePreview()
    {
        var cut = await RenderOnTabAsync("Components");

        var markup = cut.Markup;

        Assert.True(markup.IndexOf("theme-group-forms", StringComparison.Ordinal) < markup.IndexOf("data-testid=\"theme-preview\"", StringComparison.Ordinal));
    }

    [Fact]
    public async Task Page_OffersAColorPickerForEverySemanticColor_AndNoneForTheNeutrals()
    {
        var cut = await RenderAsync();

        foreach (var name in new[] { "primary", "accent", "success", "danger", "warning", "info" })
            Assert.NotEmpty(cut.FindAll($"#theme-color-{name}"));

        Assert.Empty(cut.FindAll("#theme-color-light"));
        Assert.Empty(cut.FindAll("#theme-color-dark"));
    }

    #region Live page

    [Fact]
    public async Task Page_HasItsOwnFrame_NotTheDocsLayout()
    {
        var layout = Assert.Single(typeof(ThemePage).GetCustomAttributes(typeof(Microsoft.AspNetCore.Components.LayoutAttribute), false));

        Assert.Equal(typeof(TwBlazor.Docs.Layout.EmptyLayout), ((Microsoft.AspNetCore.Components.LayoutAttribute)layout).LayoutType);
        var cut = await RenderAsync();
        Assert.Equal("Theme builder", Assert.Single(cut.FindAll("h1")).TextContent);
    }

    [Fact]
    public async Task Page_LeavesTheColorVariablesAlone_ByDefault()
    {
        var cut = await RenderAsync();
        Assert.True(string.IsNullOrEmpty(cut.Find("#theme-builder").GetAttribute("style")));
    }

    [Fact]
    public async Task PickingAColor_RecolorsThisPageOnly_AndNeverReplacesTheSitesTheme()
    {
        var options = TestContext.Services.GetRequiredService<TwBlazor.Configuration.TwBlazorOptions>();
        var before = options.Theme;
        var cut = await RenderAsync();

        await PickAsync(cut, 0, "#112233");

        var styled = cut.FindAll("[style*='--color-']");
        Assert.Equal("theme-builder", Assert.Single(styled).Id);
        Assert.Contains("--color-purple-600:", styled[0].GetAttribute("style"));
        Assert.Same(before, options.Theme);
        Assert.Same(Theme, options.Theme);
    }

    [Fact]
    public async Task TheControlsAndTheSamples_SitInsideTheRecoloredElement_SoTheWholePageFollows()
    {
        var cut = await RenderAsync();
        var page = cut.Find("#theme-builder");

        Assert.True(page.Contains(cut.Find("#theme-preset-purple")));
        Assert.True(page.Contains(cut.Find("#theme-controls")));
        Assert.True(page.Contains(cut.Find("[data-testid='theme-preview']")));
        Assert.True(page.Contains(cut.Find("#theme-code")));
    }

    [Fact]
    public async Task ChangingADefault_ReachesTheSampleComponents()
    {
        var cut = await RenderOnTabAsync("Style");

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
    public async Task TheSamples_AreOneFlowOfCards_InColumns_WithNoSectionHeadings()
    {
        var cut = await RenderAsync();
        var preview = cut.Find("[data-testid='theme-preview']");
        var cards = preview.QuerySelector("[data-testid='theme-preview-cards']")!;

        Assert.Empty(preview.QuerySelectorAll("section"));
        Assert.Contains("@container", preview.ClassList);
        Assert.Contains("columns-1", cards.ClassList);
        Assert.Contains("@4xl:columns-3", cards.ClassList);
        Assert.True(cards.Children.Length >= 15);
        Assert.All(cards.Children, card => Assert.Contains("break-inside-avoid", card.ClassList));
    }

    [Fact]
    public async Task ThePreview_HasOneHeadingForScreenReaders_AboveTheCardTitles()
    {
        var cut = await RenderAsync();
        var heading = cut.Find("[data-testid='theme-preview'] > h2");

        Assert.Equal("Component preview", heading.TextContent);
        Assert.Contains("sr-only", heading.ClassList);
    }

    [Fact]
    public async Task TheCharts_ShareAGrid_SoEveryRowIsFull()
    {
        var cut = await RenderAsync();
        var charts = cut.Find("#preview-charts");

        Assert.Contains("grid", charts.ClassList);
        Assert.Equal(6, charts.Children.Length);
    }

    [Fact]
    public async Task TheSamples_CoverFarMoreThanTheBasics()
    {
        var cut = await RenderAsync();
        var preview = cut.FindComponent<ThemeBuilderPreview>();

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
        Assert.Single(preview.FindAll("input[type=radio]:not([name^='rating-'])").Select(radio => radio.GetAttribute("name")).Distinct());
        Assert.Equal(2, preview.FindComponents<TwRating<double>>().Count);
    }

    [Fact]
    public async Task TheBarcodeAndQrCode_ShareOneEncodingCard_AtTheEndOfTheCards()
    {
        var cut = await RenderAsync();
        var cards = cut.Find("[data-testid='theme-preview-cards']").Children;

        var encoding = cards[^1];
        Assert.Contains("Encoding", encoding.TextContent);
        Assert.NotNull(encoding.QuerySelector("#preview-barcode"));
        Assert.NotNull(encoding.QuerySelector("#preview-qr-code"));
    }

    [Fact]
    public async Task TheNavbar_CollapsesBehindAMenuButton_OnSmallScreens()
    {
        var cut = await RenderAsync();
        var navbar = cut.FindComponent<ThemeBuilderPreview>().FindComponent<TwNavbar>();

        Assert.False(navbar.Instance.DisableResponsiveCollapse);
        Assert.NotNull(navbar.Find("[aria-controls$='-menu']"));
    }

    [Fact]
    public async Task ThePage_SetsItsOwnSurfaceAndTextColor_SoItReadsInEitherMode()
    {
        var cut = await RenderAsync();
        var classes = cut.Find("#theme-builder").ClassList;

        Assert.All(Theme.Colors.NeutralText.Heading.Split(' '), token => Assert.Contains(token, classes));
        Assert.All(Theme.Colors.NeutralSurface.Background.Split(' '), token => Assert.Contains(token, classes));
    }

    [Fact]
    public async Task TheThemeFile_ComesLast()
    {
        var cut = await RenderAsync();
        var markup = cut.Markup;

        Assert.True(markup.IndexOf("id=\"preview-charts\"", StringComparison.Ordinal) < markup.IndexOf("id=\"theme-code\"", StringComparison.Ordinal));
    }

    [Fact]
    public async Task TheSettings_SitInTheirOwnPanel_AndTheSamplesInTheMainContent()
    {
        var cut = await RenderAsync();
        var panel = cut.Find("aside#theme-controls");
        var main = cut.Find("main#main-content");

        Assert.Empty(cut.FindComponents<TwSidebar>());
        Assert.Equal("theme-controls-heading", panel.GetAttribute("aria-labelledby"));
        Assert.True(panel.Contains(cut.Find("#theme-settings")));
        Assert.True(main.Contains(cut.Find("[data-testid='theme-preview']")));
        Assert.True(main.Contains(cut.Find("#theme-code")));
        Assert.False(main.Contains(cut.Find("#theme-settings")));
    }

    [Fact]
    public async Task ThePanel_StaysInViewBesideTheSamplesOnAWideScreen_AndComesFirstOnANarrowOne()
    {
        var cut = await RenderAsync();
        var panel = cut.Find("#theme-controls");

        Assert.All(_stickyPanelClasses, token => Assert.Contains(token, panel.ClassList));
        var column = panel.ParentElement!;
        Assert.Equal("theme-controls", column.FirstElementChild!.Id);
        Assert.Equal("theme-controls-column", column.ParentElement!.FirstElementChild!.Id);
        Assert.Contains("lg:grid", column.ParentElement!.ClassList);
    }

    [Fact]
    public async Task ThePanelsColumn_CarriesItsBackground_TheFullHeightOfThePage()
    {
        var cut = await RenderAsync();
        var column = cut.Find("#theme-controls").ParentElement!;

        Assert.All(Theme.Colors.NeutralSurface.BackgroundSubtle.Split(' '), token => Assert.Contains(token, column.ClassList));
        Assert.Contains("lg:border-e", column.ClassList);
        Assert.DoesNotContain("lg:h-dvh", column.ClassList);
    }

    [Fact]
    public async Task ASkipLink_JumpsPastTheSettings_ToThePreview()
    {
        var cut = await RenderAsync();
        var skip = cut.Find("#theme-builder > a.sr-only");

        Assert.Equal("theme#main-content", skip.GetAttribute("href"));
        Assert.Contains("focus:not-sr-only", skip.ClassList);
        Assert.Equal("-1", cut.Find("#main-content").GetAttribute("tabindex"));
    }

    [Fact]
    public async Task ThePage_SharesTheExampleThemesBar_WithCreateYourOwnAsTheCurrentPage()
    {
        var cut = await RenderAsync();
        var links = cut.FindAll("nav[aria-label='Example themes'] a");

        Assert.Equal([.. TwBlazor.Docs.Pages.Showcase.Shared.ShowcaseBrand.All.Select(brand => brand.Path), "/theme"], links.Select(link => link.GetAttribute("href")));
        Assert.Equal("Create your own", Assert.Single(links, link => link.GetAttribute("aria-current") == "page").TextContent.Trim());
        Assert.Equal("/", cut.Find("a:contains('Back to twblazor')").GetAttribute("href"));
    }

    [Fact]
    public async Task TheThemeFile_CanBeGenerated_FromTheTopOfThePreview_AndFromThePanel()
    {
        var cut = await RenderAsync();

        Assert.Contains("Generate Theme", cut.Find("main header button#theme-generate-header").TextContent);
        Assert.True(cut.Find("aside#theme-controls").Contains(cut.Find("#theme-generate-panel")));
    }

    [Fact]
    public async Task TheThemeFile_IsNotGenerated_UntilItIsAskedFor()
    {
        var cut = await RenderAsync();

        Assert.Empty(cut.FindAll("pre code"));
        Assert.NotNull(cut.Find("#theme-code-empty #theme-generate-code"));

        cut.Find("#theme-generate-code").Click();

        Assert.Contains("public static class Theme", cut.Find("pre code").TextContent);
        Assert.Empty(cut.FindAll("#theme-code-empty"));
        Assert.Equal("Theme.cs generated", cut.Find("#theme-code [role=status]").TextContent);
    }

    [Theory]
    [InlineData("#theme-generate-header")]
    [InlineData("#theme-generate-panel")]
    public async Task GeneratingTheTheme_MovesFocusToIt(string button)
    {
        var cut = await RenderAsync();

        cut.Find(button).Click();

        var heading = cut.Find("#theme-code-heading");
        Assert.Equal("-1", heading.GetAttribute("tabindex"));
        Assert.Contains(TestContext.JSInterop.Invocations, invocation => invocation.Identifier == "Blazor._internal.domWrapper.focus");
    }

    [Fact]
    public async Task ChangingASetting_AfterGenerating_MarksTheThemeOutOfDate_UntilItIsGeneratedAgain()
    {
        var cut = await RenderAsync();
        cut.Find("#theme-generate-panel").Click();
        Assert.Empty(cut.FindAll("#theme-code-stale"));
        var before = cut.Find("pre code").TextContent;

        cut.Find("#theme-preset-ocean").Click();

        Assert.Contains("changed since this was generated", cut.Find("#theme-code-stale").TextContent);
        Assert.Equal(before, cut.Find("pre code").TextContent);

        cut.Find("#theme-generate-code").Click();

        Assert.Empty(cut.FindAll("#theme-code-stale"));
        Assert.NotEqual(before, cut.Find("pre code").TextContent);
    }

    [Fact]
    public void ThePreview_OnlyRerenders_WhenItsVersionChanges()
    {
        var selected = new HashSet<string> { "TwButton" };
        var preview = TestContext.Render<ThemeBuilderPreview>(parameters => parameters
            .Add(p => p.Version, 1)
            .Add(p => p.Colors, ThemeColorFamily.All.ToDictionary(f => f.Name, f => f.DefaultHex))
            .Add(p => p.OnColors, ThemeColorFamily.All.ToDictionary(f => f.Name, _ => OnColorMode.Auto))
            .Add(p => p.Defaults, ThemeDefaults.Default)
            .Add(p => p.Selected, selected));
        Assert.Empty(preview.FindAll("h3:contains('Alerts')"));

        selected.Add("TwAlert");
        preview.Render(parameters => parameters.Add(p => p.Version, 1));

        Assert.Empty(preview.FindAll("h3:contains('Alerts')"));

        preview.Render(parameters => parameters.Add(p => p.Version, 2));

        Assert.NotEmpty(preview.FindAll("h3:contains('Alerts')"));
    }

    [Fact]
    public async Task TheChartsAndDataTable_RenderJustAfterTheRestOfThePreview()
    {
        var preview = TestContext.Render<ThemeBuilderPreview>(parameters => parameters
            .Add(p => p.Version, 1)
            .Add(p => p.Colors, ThemeColorFamily.All.ToDictionary(f => f.Name, f => f.DefaultHex))
            .Add(p => p.OnColors, ThemeColorFamily.All.ToDictionary(f => f.Name, _ => OnColorMode.Auto))
            .Add(p => p.Defaults, ThemeDefaults.Default)
            .Add(p => p.Selected, ThemeBuilderPreview.SampledComponents.ToHashSet()));

        await preview.WaitForAssertionAsync(() =>
        {
            Assert.Equal(6, preview.FindComponents<TwChart>().Count);
            Assert.Contains(preview.FindAll("h3"), heading => heading.TextContent == "Data table");
        });
    }

    [Fact]
    public async Task TheThemeToggle_IsTheDocsOwn_AndSaysWhatItDid()
    {
        TestContext.JSInterop.Setup<bool>("themeToggle.toggle").SetResult(true);
        var cut = await RenderAsync();
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
    public async Task UntickingAComponent_HidesItsSample_AndTickingItBringsItBack()
    {
        var cut = await RenderOnTabAsync("Components");
        Assert.NotEmpty(cut.FindComponent<ThemeBuilderPreview>().FindComponents<TwSlider<int>>());

        cut.Find("#theme-component-twslider").Change(false);

        var preview = cut.FindComponent<ThemeBuilderPreview>();
        Assert.Empty(preview.FindComponents<TwSlider<int>>());
        Assert.DoesNotContain(preview.FindAll("h3"), heading => heading.TextContent == "Sliders");
        Assert.NotEmpty(preview.FindComponents<TwSwitch<bool>>());

        cut.Find("#theme-component-twslider").Change(true);

        Assert.NotEmpty(cut.FindComponent<ThemeBuilderPreview>().FindComponents<TwSlider<int>>());
    }

    [Fact]
    public async Task UntickingAWholeGroup_RemovesItsSamples_AndLeavesTheRest()
    {
        var cut = await RenderOnTabAsync("Components");

        cut.Find("#theme-group-charts").Change(false);

        var preview = cut.FindComponent<ThemeBuilderPreview>();
        Assert.Empty(cut.FindAll("#preview-charts"));
        Assert.Empty(preview.FindComponents<TwChart>());
        Assert.NotEmpty(preview.FindComponents<TwSlider<int>>());
    }

    [Fact]
    public async Task WithNothingTicked_TheSamplesAreReplacedByANote()
    {
        var cut = await RenderOnTabAsync("Components");

        cut.Find("button:contains('Clear all')").Click();

        Assert.Empty(cut.FindComponent<ThemeBuilderPreview>().FindComponents<TwCard>());
        Assert.Equal("status", cut.Find("[data-testid='theme-preview-empty']").GetAttribute("role"));
    }

    [Fact]
    public async Task EverySampledComponent_IsOneTheListOffers()
    {
        var cut = await RenderOnTabAsync("Components");
        var offered = cut.FindAll("input[id^='theme-component-']").Select(box => box.Id!["theme-component-".Length..]).ToHashSet();

        Assert.All(ThemeBuilderPreview.SampledComponents, name => Assert.Contains(name.ToLowerInvariant(), offered));
    }

    #endregion

    #region Surfaces

    private static Task PickSurfaceAsync(IRenderedComponent<ThemePage> cut, bool dark, string hex) =>
        cut.InvokeAsync(() => cut.FindComponents<TwColorPicker>().Single(picker => picker.Instance.Id == (dark ? "theme-surface-dark" : "theme-surface-light")).Instance.ValueChanged.InvokeAsync(hex));

    [Fact]
    public async Task Surfaces_StartOnTheDefaultThemes_AndAddNothingToThePage()
    {
        var cut = await RenderAsync();

        Assert.Empty(cut.FindAll("#theme-builder-surfaces"));
        Assert.Contains("bg-[oklch(100%_0_0)] dark:bg-[oklch(25.33%_0.016_252.42)]", Code(cut));
        Assert.Contains("AAA", cut.Find("[data-testid='theme-surface-contrast-light']").TextContent);
        Assert.Contains("AAA", cut.Find("[data-testid='theme-surface-contrast-dark']").TextContent);
    }

    [Fact]
    public async Task PickingALightBackground_RewritesTheLightSurfaces_AndLeavesDarkModeAlone()
    {
        var cut = await RenderAsync();

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
        var cut = await RenderAsync();

        await PickSurfaceAsync(cut, dark: true, "#101820");

        var style = cut.Find("#theme-builder > style#theme-builder-surfaces").TextContent;
        var rules = style.Split('\n').Where(line => line.Contains("[class~=")).ToList();
        Assert.StartsWith("@layer utilities {", style);
        Assert.NotEmpty(rules);
        Assert.All(rules, rule => Assert.Contains(":where(#theme-builder) ", rule));
        Assert.Same(before, TestContext.Services.GetRequiredService<TwBlazor.Configuration.TwBlazorOptions>().Theme);
    }

    [Fact]
    public async Task EachMode_HasACardsPicker_AndAPagePicker()
    {
        var cut = await RenderAsync();

        var labels = cut.FindComponents<TwColorPicker>().Where(p => p.Instance.Id!.StartsWith("theme-surface-", StringComparison.Ordinal) || p.Instance.Id!.StartsWith("theme-body-", StringComparison.Ordinal)).Select(p => p.Instance.Label);

        Assert.Equal(["Light cards", "Light page", "Dark cards", "Dark page"], labels);
        Assert.Contains("AAA", cut.Find("[data-testid='theme-body-contrast-light']").TextContent);
        Assert.Contains("AAA", cut.Find("[data-testid='theme-body-contrast-dark']").TextContent);
    }

    [Fact]
    public async Task PickingAPageColor_WritesItIntoTheBodyToken_AndDeselectsTheExample()
    {
        var cut = await RenderAsync();
        var before = Code(cut);

        await cut.InvokeAsync(() => cut.FindComponents<TwColorPicker>().Single(p => p.Instance.Id == "theme-body-dark").Instance.ValueChanged.InvokeAsync("#0b1020"));

        Assert.True(OklchColor.TryParseHex("#0b1020", out var body));
        Assert.Contains($"dark:bg-{body.ToTailwindValue()}", Code(cut));
        Assert.DoesNotContain("oklch(22.2%_0.015_253)", Code(cut));
        Assert.Contains("oklch(25.33%_0.016_252.42)", Code(cut));
        Assert.NotEqual(before, Code(cut));
        Assert.Equal("false", cut.Find("#theme-preset-purple").GetAttribute("aria-pressed"));
        Assert.NotEmpty(cut.FindAll("#theme-builder-surfaces"));
    }

    [Fact]
    public async Task ThePagePicker_ShowsWhereTheBodyEndsUp_WhenOnlyTheCardsChange()
    {
        var cut = await RenderAsync();

        await PickSurfaceAsync(cut, dark: false, "#fdf6e3");

        var page = cut.FindComponents<TwColorPicker>().Single(p => p.Instance.Id == "theme-body-light").Instance.Value;
        Assert.Equal(ThemeSurface.EffectiveBody(ThemeSurfaces.Default with { Light = "#fdf6e3" }, dark: false), page);
        Assert.NotEqual("#fdf6e3", page);
    }

    [Fact]
    public async Task ResetSurfaces_AlsoPutsThePageBack()
    {
        var cut = await RenderAsync();
        var original = Code(cut);
        await cut.InvokeAsync(() => cut.FindComponents<TwColorPicker>().Single(p => p.Instance.Id == "theme-body-light").Instance.ValueChanged.InvokeAsync("#eeeeee"));

        cut.Find("button:contains('Reset surfaces')").Click();

        Assert.Equal(original, Code(cut));
    }

    [Fact]
    public async Task TheSamples_SitOnThePageBody_SoTheCardsStandOut()
    {
        var cut = await RenderAsync();

        Assert.All(Theme.Colors.NeutralSurface.Body.Split(' '), token => Assert.Contains(token, cut.Find("#main-content").ClassList));
    }

    [Fact]
    public async Task ABackgroundTheTextCannotBeReadOn_IsCalledOut()
    {
        var cut = await RenderAsync();

        await PickSurfaceAsync(cut, dark: false, "#333333");

        Assert.Contains("fails contrast", cut.Find("[data-testid='theme-surface-contrast-light']").TextContent);
    }

    [Fact]
    public async Task ResetSurfaces_AndChoosingAnExample_BothPutTheDefaultSurfacesBack()
    {
        var cut = await RenderAsync();
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