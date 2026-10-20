using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using TwBlazor.Components;
using TwBlazor.Configuration.Components;
using TwBlazor.Enums;
using TwBlazor.Localization;

namespace TwBlazor.Tests.Components.Autocomplete;

public class TwAutocompleteTests : TwBlazorTestBase
{
    private static readonly string[] _countries = ["Canada", "France", "Germany", "United Kingdom", "United States"];

    private static readonly string[] _cafes = ["Café Nero", "Costa", "Pret"];

    private TwInputTheme inputTheme => Theme.Components.Require<TwInputTheme>();

    private sealed class Person
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;
    }

    private static readonly Person[] _people =
    [
        new() { Id = 1, Name = "Ada Lovelace" },
        new() { Id = 2, Name = "Alan Turing" },
        new() { Id = 3, Name = "Grace Hopper" }
    ];

    private IRenderedComponent<TwAutocomplete<string>> Render(Action<ComponentParameterCollectionBuilder<TwAutocomplete<string>>>? configure = null) =>
        TestContext.Render<TwAutocomplete<string>>(p =>
        {
            p.Add(x => x.Id, "country");
            p.Add(x => x.Label, "Country");
            p.Add(x => x.Values, _countries);
            configure?.Invoke(p);
        });

    private static List<string> OptionTexts(IRenderedComponent<TwAutocomplete<string>> cut) =>
        [.. cut.FindAll("[role='option']").Select(o => o.TextContent.Trim())];

    #region Rendering

    [Fact]
    public async Task Renders_AClosedCombobox()
    {
        var cut = Render();

        var input = cut.Find("input");
        Assert.Equal("combobox", input.GetAttribute("role"));
        Assert.Equal("list", input.GetAttribute("aria-autocomplete"));
        Assert.Equal("false", input.GetAttribute("aria-expanded"));
        Assert.Null(input.GetAttribute("aria-controls"));
        Assert.Equal("off", input.GetAttribute("autocomplete"));
        Assert.Equal("text", input.GetAttribute("type"));
        Assert.Empty(cut.FindAll("[role='listbox']"));
        await Task.CompletedTask;
    }

    [Fact]
    public async Task Renders_TheLabel_ForTheInput()
    {
        var cut = Render();

        var label = cut.Find("label");
        Assert.Equal("Country", label.TextContent);
        Assert.Equal("country", label.GetAttribute("for"));
        await Task.CompletedTask;
    }

    [Fact]
    public async Task Defaults_TheNameAttribute_ToTheId()
    {
        var cut = Render();

        Assert.Equal("country", cut.Find("input").GetAttribute("name"));
        await Task.CompletedTask;
    }

    [Fact]
    public async Task Shows_TheSelectedValue_AsTheText()
    {
        var cut = Render(p => p.Add(x => x.SelectedValue, "France"));

        Assert.Equal("France", cut.Find("input").GetAttribute("value"));
        await Task.CompletedTask;
    }

    [Fact]
    public async Task Shows_ThePropertyName_ForComplexObjects()
    {
        var cut = TestContext.Render<TwAutocomplete<Person>>(p => p
            .Add(x => x.Values, _people)
            .Add(x => x.PropertyName, nameof(Person.Name))
            .Add(x => x.SelectedValue, _people[1]));

        Assert.Equal("Alan Turing", cut.Find("input").GetAttribute("value"));
        await Task.CompletedTask;
    }

    [Theory]
    [InlineData(false, "Size")]
    [InlineData(true, "DenseSize")]
    public async Task Uses_TheInputSize_ForDense(bool dense, string sizeProperty)
    {
        var cut = Render(p => p.Add(x => x.Dense, dense));

        var expected = sizeProperty == "Size" ? inputTheme.Size : inputTheme.DenseSize;
        Assert.Contains(expected, cut.Find("input").GetAttribute("class"));
        await Task.CompletedTask;
    }

    [Fact]
    public async Task Marks_AnInvalidField_AndLinksItsError()
    {
        var cut = Render(p => p
            .Add(x => x.Invalid, true)
            .Add(x => x.ErrorMessage, "Pick a country."));

        var input = cut.Find("input");
        Assert.Equal("true", input.GetAttribute("aria-invalid"));
        Assert.Equal("country-error", input.GetAttribute("aria-describedby"));
        Assert.Equal("Pick a country.", cut.Find("#country-error").TextContent);
        await Task.CompletedTask;
    }

    [Fact]
    public async Task Marks_ARequiredField()
    {
        var cut = Render(p => p.Add(x => x.Required, true));

        var input = cut.Find("input");
        Assert.True(input.HasAttribute("required"));
        Assert.Equal("true", input.GetAttribute("aria-required"));
        await Task.CompletedTask;
    }

    [Fact]
    public async Task Attaches_TheKeyboardScript_OnFirstRender()
    {
        Render();

        Assert.Single(TestContext.JSInterop.Invocations, i => i.Identifier == "twAutocomplete.attach");
        await Task.CompletedTask;
    }

    #endregion

    #region Filtering

    [Fact]
    public async Task Typing_OpensTheList_WithTheMatchingOptions()
    {
        var cut = Render();

        await cut.Find("input").InputAsync(new ChangeEventArgs { Value = "united" });

        var input = cut.Find("input");
        Assert.Equal("true", input.GetAttribute("aria-expanded"));
        Assert.Equal("country-listbox", input.GetAttribute("aria-controls"));
        Assert.Equal(["United Kingdom", "United States"], OptionTexts(cut));
        Assert.Equal("Country", cut.Find("[role='listbox']").GetAttribute("aria-label"));
    }

    [Fact]
    public async Task Contains_MatchesTextAnywhere()
    {
        var cut = Render();

        await cut.Find("input").InputAsync(new ChangeEventArgs { Value = "an" });

        Assert.Equal(["Canada", "France", "Germany"], OptionTexts(cut));
    }

    [Fact]
    public async Task StartsWith_MatchesOnlyTheStartOfTheText()
    {
        var cut = Render(p => p.Add(x => x.FilterMode, AutocompleteFilterMode.StartsWith));

        await cut.Find("input").InputAsync(new ChangeEventArgs { Value = "an" });

        Assert.Empty(cut.FindAll("[role='option']"));

        await cut.Find("input").InputAsync(new ChangeEventArgs { Value = "ger" });

        Assert.Equal(["Germany"], OptionTexts(cut));
    }

    [Fact]
    public async Task Filtering_IgnoresCaseAndAccents()
    {
        var cut = TestContext.Render<TwAutocomplete<string>>(p => p.Add(x => x.Values, _cafes));

        await cut.Find("input").InputAsync(new ChangeEventArgs { Value = "CAFE" });

        Assert.Equal(["Café Nero"], cut.FindAll("[role='option']").Select(o => o.TextContent.Trim()));
    }

    [Fact]
    public async Task Options_KeepTheirIds_WhenFiltered()
    {
        var cut = Render();

        await cut.Find("input").InputAsync(new ChangeEventArgs { Value = "germ" });

        var option = cut.Find("[role='option']");
        Assert.Equal("country-option-3", option.GetAttribute("id"));
        Assert.Equal("3", option.GetAttribute("data-value"));
    }

    [Fact]
    public async Task MinCharacters_KeepsTheListClosed_UntilEnoughAreTyped()
    {
        var cut = Render(p => p.Add(x => x.MinCharacters, 2));

        await cut.Find("input").InputAsync(new ChangeEventArgs { Value = "u" });
        Assert.Empty(cut.FindAll("[data-tw-popover]"));

        await cut.Find("input").InputAsync(new ChangeEventArgs { Value = "un" });
        Assert.Equal(2, cut.FindAll("[role='option']").Count);

        await cut.Find("input").InputAsync(new ChangeEventArgs { Value = "u" });
        Assert.Empty(cut.FindAll("[data-tw-popover]"));
    }

    [Fact]
    public async Task MaxItems_LimitsTheSuggestions()
    {
        var cut = Render(p => p.Add(x => x.MaxItems, 2));

        await cut.Find("input").InputAsync(new ChangeEventArgs { Value = "a" });

        Assert.Equal(["Canada", "France"], OptionTexts(cut));
    }

    [Fact]
    public async Task NoMatches_ShowsTheNoResultsText_WithoutAListbox()
    {
        var cut = Render();

        await cut.Find("input").InputAsync(new ChangeEventArgs { Value = "zzz" });

        Assert.Empty(cut.FindAll("[role='listbox']"));
        Assert.Contains(Localizer[TwStrings.Common_NoResults], cut.Find("[data-tw-popover]").TextContent);
        Assert.Equal("false", cut.Find("input").GetAttribute("aria-expanded"));
        Assert.Null(cut.Find("input").GetAttribute("aria-controls"));
    }

    [Fact]
    public async Task NoResultsText_ReplacesTheDefault()
    {
        var cut = Render(p => p.Add(x => x.NoResultsText, "No such country"));

        await cut.Find("input").InputAsync(new ChangeEventArgs { Value = "zzz" });

        Assert.Equal("No such country", cut.Find("[data-tw-popover]").TextContent.Trim());
    }

    [Theory]
    [InlineData("germ", "1 result available")]
    [InlineData("united", "2 results available")]
    [InlineData("zzz", "No results")]
    public async Task Announces_HowManySuggestionsThereAre(string typed, string expected)
    {
        var cut = Render();

        await cut.Find("input").InputAsync(new ChangeEventArgs { Value = typed });

        Assert.Equal(expected, cut.Find("[role='status']").TextContent);
    }

    [Fact]
    public async Task Announces_Nothing_WhileClosed()
    {
        var cut = Render();

        Assert.Equal(string.Empty, cut.Find("[role='status']").TextContent);
        await Task.CompletedTask;
    }

    [Fact]
    public async Task Typing_ResetsTheHighlight()
    {
        var cut = Render();

        await cut.Find("input").InputAsync(new ChangeEventArgs { Value = "a" });

        var refresh = TestContext.JSInterop.Invocations.Last(i => i.Identifier == "twAutocomplete.refresh");
        Assert.Equal("reset", refresh.Arguments[1]);
    }

    #endregion

    #region Opening and closing

    [Fact]
    public async Task Clicking_TheField_ListsEveryOption()
    {
        var cut = Render();

        await cut.Find("input").ClickAsync(new());

        Assert.Equal(_countries, OptionTexts(cut));
    }

    [Fact]
    public async Task Opening_WithASelection_ListsEveryOption_AndMarksTheSelectedOne()
    {
        var cut = Render(p => p.Add(x => x.SelectedValue, "France"));

        await cut.Find("input").ClickAsync(new());

        Assert.Equal(_countries.Length, cut.FindAll("[role='option']").Count);
        var selected = Assert.Single(cut.FindAll("[aria-selected='true']"));
        Assert.Equal("France", selected.TextContent.Trim());
        Assert.Equal(_countries.Length - 1, cut.FindAll("[aria-selected='false']").Count);
    }

    [Fact]
    public async Task TheChevronButton_OpensAndClosesTheList()
    {
        var cut = Render();
        var toggle = cut.Find("button[aria-label='Country']");
        Assert.Equal("-1", toggle.GetAttribute("tabindex"));
        Assert.Equal("false", toggle.GetAttribute("aria-expanded"));

        await toggle.ClickAsync(new());
        Assert.Equal("true", cut.Find("button[aria-label='Country']").GetAttribute("aria-expanded"));
        Assert.NotEmpty(cut.FindAll("[role='option']"));

        await cut.Find("button[aria-label='Country']").ClickAsync(new());
        Assert.Empty(cut.FindAll("[data-tw-popover]"));
    }

    [Fact]
    public async Task TheChevronButton_UsesTheOptionsLabel_WithoutALabel()
    {
        var cut = TestContext.Render<TwAutocomplete<string>>(p => p.Add(x => x.Values, _countries));

        Assert.NotEmpty(cut.FindAll($"button[aria-label='{Localizer[TwStrings.Select_Options]}']"));
        await Task.CompletedTask;
    }

    [Fact]
    public async Task OpenFromKeyboard_OpensTheList_AndHighlightsTheRequestedEnd()
    {
        var cut = Render();

        await cut.InvokeAsync(() => cut.Instance.OpenFromKeyboardAsync("last"));

        Assert.Equal(_countries.Length, cut.FindAll("[role='option']").Count);
        var refresh = TestContext.JSInterop.Invocations.Last(i => i.Identifier == "twAutocomplete.refresh");
        Assert.Equal("last", refresh.Arguments[1]);
    }

    [Fact]
    public async Task Close_ClosesTheList()
    {
        var cut = Render();
        await cut.Find("input").InputAsync(new ChangeEventArgs { Value = "a" });

        await cut.InvokeAsync(() => cut.Instance.Close());

        Assert.Empty(cut.FindAll("[data-tw-popover]"));
        Assert.Equal("false", cut.Find("input").GetAttribute("aria-expanded"));
    }

    [Fact]
    public async Task Opening_RegistersTheOutsideClickAndEscapeHandling()
    {
        var cut = Render();

        await cut.Find("input").InputAsync(new ChangeEventArgs { Value = "a" });

        Assert.Contains(TestContext.JSInterop.Invocations, i => i.Identifier == "twPicker.registerOutsideClick");
    }

    [Fact]
    public async Task Opening_DoesNotTrapFocusOrMakeThePageInert()
    {
        var cut = Render();

        await cut.Find("input").InputAsync(new ChangeEventArgs { Value = "a" });

        Assert.DoesNotContain(TestContext.JSInterop.Invocations, i => i.Identifier is "twDialog.trapFocus" or "twDialog.setBackgroundInert");
    }

    #endregion

    #region Selecting

    [Fact]
    public async Task ClickingAnOption_SelectsIt_AndClosesTheList()
    {
        var received = new List<string>();
        var cut = Render(p => p.Add(x => x.SelectedValueChanged, EventCallback.Factory.Create<string>(this, received.Add)));
        await cut.Find("input").InputAsync(new ChangeEventArgs { Value = "fr" });

        await cut.Find("[role='option']").ClickAsync(new());

        Assert.Equal(["France"], received);
        Assert.Equal("France", cut.Find("input").GetAttribute("value"));
        Assert.Empty(cut.FindAll("[data-tw-popover]"));
    }

    [Fact]
    public async Task SelectOption_IgnoresAnUnknownKey()
    {
        var calls = 0;
        var cut = Render(p => p.Add(x => x.SelectedValueChanged, EventCallback.Factory.Create<string>(this, _ => calls++)));
        await cut.Find("input").InputAsync(new ChangeEventArgs { Value = "fr" });

        await cut.InvokeAsync(() => cut.Instance.SelectOptionAsync(99));

        Assert.Equal(0, calls);
    }

    [Fact]
    public async Task SelectOption_DoesNotNotify_WhenTheSameOptionIsPickedAgain()
    {
        var calls = 0;
        var cut = Render(p => p
            .Add(x => x.SelectedValue, "France")
            .Add(x => x.SelectedValueChanged, EventCallback.Factory.Create<string>(this, _ => calls++)));
        await cut.Find("input").ClickAsync(new());

        await cut.InvokeAsync(() => cut.Instance.SelectOptionAsync(2));

        Assert.Equal(0, calls);
    }

    [Fact]
    public async Task SelectOption_WorksWithComplexObjects()
    {
        Person? received = null;
        var cut = TestContext.Render<TwAutocomplete<Person>>(p => p
            .Add(x => x.Values, _people)
            .Add(x => x.PropertyName, nameof(Person.Name))
            .Add(x => x.SelectedValueChanged, EventCallback.Factory.Create<Person>(this, v => received = v)));
        await cut.Find("input").InputAsync(new ChangeEventArgs { Value = "grace" });

        await cut.Find("[role='option']").ClickAsync(new());

        Assert.Same(_people[2], received);
        Assert.Equal("Grace Hopper", cut.Find("input").GetAttribute("value"));
    }

    [Fact]
    public async Task ItemTemplate_RendersEachSuggestion()
    {
        var cut = TestContext.Render<TwAutocomplete<Person>>(p => p
            .Add(x => x.Values, _people)
            .Add(x => x.PropertyName, nameof(Person.Name))
            .Add(x => x.ItemTemplate, person => builder => builder.AddMarkupContent(0, $"<b>{person.Name}</b> #{person.Id}")));

        await cut.Find("input").InputAsync(new ChangeEventArgs { Value = "alan" });

        Assert.Equal("Alan Turing", cut.Find("[role='option'] b").TextContent);
        Assert.Contains("#2", cut.Find("[role='option']").TextContent);
    }

    #endregion

    #region Leaving the field

    [Fact]
    public async Task Blur_PutsBackTheSelectedText_WhenTheTypedTextMatchesNothing()
    {
        var calls = 0;
        var cut = Render(p => p
            .Add(x => x.SelectedValue, "France")
            .Add(x => x.SelectedValueChanged, EventCallback.Factory.Create<string>(this, _ => calls++)));
        await cut.Find("input").InputAsync(new ChangeEventArgs { Value = "Fra" });

        await cut.Find("input").BlurAsync(new());

        Assert.Equal("France", cut.Find("input").GetAttribute("value"));
        Assert.Equal(0, calls);
        Assert.Empty(cut.FindAll("[data-tw-popover]"));
    }

    [Fact]
    public async Task Blur_SelectsAnOption_WhoseTextWasTypedInFull()
    {
        var received = new List<string>();
        var cut = Render(p => p.Add(x => x.SelectedValueChanged, EventCallback.Factory.Create<string>(this, received.Add)));
        await cut.Find("input").InputAsync(new ChangeEventArgs { Value = "germany" });

        await cut.Find("input").BlurAsync(new());

        Assert.Equal(["Germany"], received);
        Assert.Equal("Germany", cut.Find("input").GetAttribute("value"));
    }

    [Fact]
    public async Task Blur_WithEmptyText_ClearsTheSelection()
    {
        var received = new List<string>();
        var cut = Render(p => p
            .Add(x => x.SelectedValue, "France")
            .Add(x => x.SelectedValueChanged, EventCallback.Factory.Create<string>(this, received.Add)));
        await cut.Find("input").InputAsync(new ChangeEventArgs { Value = string.Empty });

        await cut.Find("input").BlurAsync(new());

        Assert.Equal([null!], received);
        Assert.Equal(string.Empty, cut.Find("input").GetAttribute("value"));
    }

    [Fact]
    public async Task Blur_WithEmptyText_KeepsARequiredSelection()
    {
        var calls = 0;
        var cut = Render(p => p
            .Add(x => x.Required, true)
            .Add(x => x.SelectedValue, "France")
            .Add(x => x.SelectedValueChanged, EventCallback.Factory.Create<string>(this, _ => calls++)));
        await cut.Find("input").InputAsync(new ChangeEventArgs { Value = string.Empty });

        await cut.Find("input").BlurAsync(new());

        Assert.Equal(0, calls);
        Assert.Equal("France", cut.Find("input").GetAttribute("value"));
    }

    [Fact]
    public async Task Blur_WithoutASelection_EmptiesUnmatchedText()
    {
        var cut = Render();
        await cut.Find("input").InputAsync(new ChangeEventArgs { Value = "Atlantis" });

        await cut.Find("input").BlurAsync(new());

        Assert.Equal(string.Empty, cut.Find("input").GetAttribute("value"));
    }

    #endregion

    #region Clearing

    [Fact]
    public async Task Clearable_ShowsAClearButton_DisabledWhileEmpty()
    {
        var cut = Render(p => p.Add(x => x.Clearable, true));

        var clear = cut.Find($"button[aria-label='{Localizer[TwStrings.Textfield_Clear]}']");
        Assert.True(clear.HasAttribute("disabled"));
        Assert.Equal("-1", clear.GetAttribute("tabindex"));
        await Task.CompletedTask;
    }

    [Fact]
    public async Task Clearable_ClearsTheSelection()
    {
        var received = new List<string>();
        var cut = Render(p => p
            .Add(x => x.Clearable, true)
            .Add(x => x.SelectedValue, "France")
            .Add(x => x.SelectedValueChanged, EventCallback.Factory.Create<string>(this, received.Add)));

        await cut.Find($"button[aria-label='{Localizer[TwStrings.Textfield_Clear]}']").ClickAsync(new());

        Assert.Equal([null!], received);
        Assert.Equal(string.Empty, cut.Find("input").GetAttribute("value"));
    }

    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public async Task Clearable_HidesTheClearButton_WhenDisabledOrReadOnly(bool disabled, bool readOnly)
    {
        var cut = Render(p => p
            .Add(x => x.Clearable, true)
            .Add(x => x.Disabled, disabled)
            .Add(x => x.ReadOnly, readOnly));

        Assert.Empty(cut.FindAll($"button[aria-label='{Localizer[TwStrings.Textfield_Clear]}']"));
        await Task.CompletedTask;
    }

    [Fact]
    public async Task Clearable_WidensTheEndPadding_ForBothButtons()
    {
        var plain = Render();
        var clearable = Render(p => p.Add(x => x.Clearable, true));

        Assert.Equal("padding-inline-end: calc(1 * 2.25rem + 0.25rem);", plain.Find("input").GetAttribute("style"));
        Assert.Equal("padding-inline-end: calc(2 * 2.25rem + 0.25rem);", clearable.Find("input").GetAttribute("style"));
        await Task.CompletedTask;
    }

    #endregion

    #region Disabled and read only

    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public async Task DisabledOrReadOnly_NeverOpensTheList(bool disabled, bool readOnly)
    {
        var cut = Render(p => p
            .Add(x => x.Disabled, disabled)
            .Add(x => x.ReadOnly, readOnly));

        await cut.Find("input").InputAsync(new ChangeEventArgs { Value = "fr" });
        await cut.Find("input").ClickAsync(new());
        await cut.InvokeAsync(() => cut.Instance.OpenFromKeyboardAsync("first"));

        Assert.Empty(cut.FindAll("[data-tw-popover]"));
        Assert.True(cut.Find("button[aria-label='Country']").HasAttribute("disabled"));
        Assert.Equal(disabled, cut.Find("input").HasAttribute("disabled"));
        Assert.Equal(readOnly, cut.Find("input").HasAttribute("readonly"));
    }

    #endregion

    #region Parameters

    [Fact]
    public async Task ChangingSelectedValue_FromTheParent_UpdatesTheText()
    {
        var cut = Render(p => p.Add(x => x.SelectedValue, "France"));

        cut.Render(p => p.Add(x => x.SelectedValue, "Canada"));

        Assert.Equal("Canada", cut.Find("input").GetAttribute("value"));
        await Task.CompletedTask;
    }

    [Fact]
    public async Task AParentRender_KeepsTheTypedText()
    {
        var cut = Render(p => p.Add(x => x.SelectedValue, "France"));
        await cut.Find("input").InputAsync(new ChangeEventArgs { Value = "Ger" });

        cut.Render(p => p.Add(x => x.SelectedValue, "France"));

        Assert.Equal("Ger", cut.Find("input").GetAttribute("value"));
    }

    [Fact]
    public async Task ValuesArrivingAfterTheSelection_ShowTheSelectedText()
    {
        var cut = TestContext.Render<TwAutocomplete<string>>(p => p.Add(x => x.SelectedValue, "France"));
        Assert.Equal(string.Empty, cut.Find("input").GetAttribute("value"));

        cut.Render(p => p.Add(x => x.Values, _countries));

        Assert.Equal("France", cut.Find("input").GetAttribute("value"));
        await Task.CompletedTask;
    }

    [Fact]
    public async Task ClearingAValueType_StaysCleared_WhenItsDefaultIsAnOption()
    {
        int[] quantities = [0, 1, 2];
        var cut = TestContext.Render<TwAutocomplete<int>>(p => p
            .Add(x => x.Values, quantities)
            .Add(x => x.SelectedValue, 2));
        await cut.Find("input").InputAsync(new ChangeEventArgs { Value = string.Empty });
        await cut.Find("input").BlurAsync(new());

        cut.Render(p => p.Add(x => x.SelectedValue, 0));

        Assert.Equal(string.Empty, cut.Find("input").GetAttribute("value"));
    }

    #endregion

    #region Search

    [Fact]
    public async Task SearchFunc_SuppliesTheSuggestions_ForTheTypedText()
    {
        var queries = new List<string>();
        var cut = TestContext.Render<TwAutocomplete<string>>(p => p
            .Add(x => x.Values, _countries)
            .Add(x => x.SearchFunc, (query, _) =>
            {
                queries.Add(query);
                return Task.FromResult<IEnumerable<string>>([$"{query} one", $"{query} two"]);
            }));

        await cut.Find("input").InputAsync(new ChangeEventArgs { Value = "zz" });

        Assert.Equal(["zz"], queries);
        Assert.Equal(["zz one", "zz two"], cut.FindAll("[role='option']").Select(o => o.TextContent.Trim()));
    }

    [Fact]
    public async Task SearchFunc_ShowsLoading_UntilTheResultsArrive()
    {
        var pending = new TaskCompletionSource<IEnumerable<string>>();
        var cut = TestContext.Render<TwAutocomplete<string>>(p => p.Add(x => x.SearchFunc, (_, _) => pending.Task));

        cut.Find("input").Input("a");

        await cut.WaitForAssertionAsync(() => Assert.Contains(Localizer[TwStrings.Common_LoadingEllipsis], cut.Find("[data-tw-popover]").TextContent));
        Assert.Empty(cut.FindAll("[role='listbox']"));

        await cut.InvokeAsync(() => pending.SetResult(["Apple"]));

        await cut.WaitForAssertionAsync(() => Assert.Equal("Apple", cut.Find("[role='option']").TextContent.Trim()));
    }

    [Fact]
    public async Task SearchFunc_CancelsAStaleSearch_WhenTheUserTypesAgain()
    {
        var searches = new List<(string Query, CancellationToken Token, TaskCompletionSource<IEnumerable<string>> Result)>();
        var cut = TestContext.Render<TwAutocomplete<string>>(p => p.Add(x => x.SearchFunc, (query, token) =>
        {
            var result = new TaskCompletionSource<IEnumerable<string>>();
            searches.Add((query, token, result));
            return result.Task;
        }));

        cut.Find("input").Input("a");
        cut.Find("input").Input("ab");
        await cut.WaitForAssertionAsync(() => Assert.Equal(2, searches.Count));

        Assert.True(searches[0].Token.IsCancellationRequested);
        Assert.False(searches[1].Token.IsCancellationRequested);

        await cut.InvokeAsync(() => searches[1].Result.SetResult(["Fresh"]));
        await cut.InvokeAsync(() => searches[0].Result.SetResult(["Stale"]));

        await cut.WaitForAssertionAsync(() => Assert.Equal("Fresh", cut.Find("[role='option']").TextContent.Trim()));
        Assert.Single(cut.FindAll("[role='option']"));
    }

    [Fact]
    public async Task DebounceInterval_SearchesOnce_ForQuickKeystrokes()
    {
        var queries = new List<string>();
        var cut = TestContext.Render<TwAutocomplete<string>>(p => p
            .Add(x => x.DebounceInterval, 100)
            .Add(x => x.SearchFunc, (query, _) =>
            {
                queries.Add(query);
                return Task.FromResult<IEnumerable<string>>([query]);
            }));

        cut.Find("input").Input("a");
        cut.Find("input").Input("ab");
        cut.Find("input").Input("abc");

        await cut.WaitForAssertionAsync(() => Assert.Equal("abc", cut.Find("[role='option']").TextContent.Trim()));
        Assert.Equal(["abc"], queries);
    }

    [Fact]
    public async Task SearchFunc_LimitsTheResults_ToMaxItems()
    {
        var cut = TestContext.Render<TwAutocomplete<string>>(p => p
            .Add(x => x.MaxItems, 1)
            .Add(x => x.SearchFunc, (_, _) => Task.FromResult<IEnumerable<string>>(["One", "Two"])));

        await cut.Find("input").InputAsync(new ChangeEventArgs { Value = "o" });

        Assert.Single(cut.FindAll("[role='option']"));
    }

    [Fact]
    public async Task Close_CancelsAPendingSearch()
    {
        CancellationToken captured = default;
        var cut = TestContext.Render<TwAutocomplete<string>>(p => p.Add(x => x.SearchFunc, (_, token) =>
        {
            captured = token;
            return new TaskCompletionSource<IEnumerable<string>>().Task;
        }));
        cut.Find("input").Input("a");
        await cut.WaitForAssertionAsync(() => Assert.True(captured.CanBeCanceled));

        await cut.InvokeAsync(() => cut.Instance.Close());

        Assert.True(captured.IsCancellationRequested);
    }

    #endregion

    #region Interop

    [Theory]
    [InlineData(nameof(TwAutocomplete<string>.SelectOptionAsync), "SelectOption")]
    [InlineData(nameof(TwAutocomplete<string>.OpenFromKeyboardAsync), "OpenFromKeyboard")]
    [InlineData(nameof(TwAutocomplete<string>.Close), "Close")]
    public async Task JsInvokableMethods_UseTheNamesTheScriptCalls(string methodName, string identifier)
    {
        var method = typeof(TwAutocomplete<string>).GetMethod(methodName);

        var attribute = method!.GetCustomAttributes(typeof(JSInvokableAttribute), inherit: false).Cast<JSInvokableAttribute>().Single();
        Assert.Equal(identifier, attribute.Identifier);
        await Task.CompletedTask;
    }

    [Fact]
    public async Task Dispose_DetachesTheKeyboardScript()
    {
        var cut = Render();

        await cut.Instance.DisposeAsync();

        Assert.Contains(TestContext.JSInterop.Invocations, i => i.Identifier == "twAutocomplete.detach");
    }

    [Fact]
    public async Task Dispose_DoesNotThrow_WhenTheCircuitHasDisconnected()
    {
        TestContext.JSInterop.SetupVoid("twAutocomplete.detach", _ => true).SetException(new JSDisconnectedException("circuit gone"));
        var cut = Render();

        var exception = await Record.ExceptionAsync(async () => await cut.Instance.DisposeAsync());

        Assert.Null(exception);
    }

    #endregion

    [Fact]
    public void Outlined_UsesTheRoundedParameter()
    {
        var cut = Render(p =>
        {
            p.Add(x => x.Variant, InputVariant.Outlined);
            p.Add(x => x.Rounded, Rounded.Full);
        });

        var input = cut.Find("input[role='combobox']");
        Assert.Contains("rounded-full", input.ClassList);
        Assert.DoesNotContain("rounded-lg", input.ClassList);
    }
}
