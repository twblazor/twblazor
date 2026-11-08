using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using TwBlazor.Components;
using TwBlazor.Configuration.Components;
using TwBlazor.Enums;
using TwBlazor.Localization;
using TwBlazor.Models;

namespace TwBlazor.Tests.Components.TagInput;

public class TwTagInputTests : TwBlazorTestBase
{
    private static readonly string[] _pastedTags = ["one", "two", "three", "four", "five"];

    private static readonly string[] _languages = ["C#", "F#", "Go", "Python", "Rust", "TypeScript"];

    private TwTagInputTheme theme => Theme.Components.Require<TwTagInputTheme>();

    private TwInputTheme inputTheme => Theme.Components.Require<TwInputTheme>();

    private sealed class Profile
    {
        public IReadOnlyList<string> Tags { get; set; } = [];
    }

    private IRenderedComponent<TwTagInput> Render(Action<ComponentParameterCollectionBuilder<TwTagInput>>? configure = null) =>
        TestContext.Render<TwTagInput>(p =>
        {
            p.Add(x => x.Id, "tags");
            p.Add(x => x.Label, "Tags");
            configure?.Invoke(p);
        });

    /// <summary>
    /// Renders a field bound to <paramref name="bound"/>, the way <c>@bind-Value</c> does, so each change is reported.
    /// </summary>
    private IRenderedComponent<TwTagInput> RenderBound(List<IReadOnlyList<string>> changes, IReadOnlyList<string>? value = null, Action<ComponentParameterCollectionBuilder<TwTagInput>>? configure = null) =>
        Render(p =>
        {
            p.Add(x => x.Value, value ?? []);
            p.Add(x => x.ValueChanged, (IReadOnlyList<string> v) => changes.Add(v));
            configure?.Invoke(p);
        });

    private static List<string> ChipLabels(IRenderedComponent<TwTagInput> cut) =>
        [.. cut.FindAll("[role='listitem']").Select(c => c.TextContent.Trim())];

    private static Task Commit(IRenderedComponent<TwTagInput> cut, string text) =>
        cut.InvokeAsync(() => cut.Instance.CommitTextAsync(text));

    private static void Type(IRenderedComponent<TwTagInput> cut, string text) =>
        cut.Find("input").Input(text);

    private List<string> FocusedIds() =>
        [.. TestContext.JSInterop.Invocations.Where(i => i.Identifier == "twFocus.focusById").Select(i => (string)i.Arguments[0]!)];

    private int InputFocusCount() =>
        TestContext.JSInterop.Invocations.Count(i => i.Identifier == "Blazor._internal.domWrapper.focus");

    #region Rendering

    [Fact]
    public void Renders_TheLabel_ForTheTextInput()
    {
        var cut = Render();

        var label = cut.Find("label");
        Assert.Equal("Tags", label.TextContent);
        Assert.Equal("tags", label.GetAttribute("for"));
        Assert.Equal("text", cut.Find("input").GetAttribute("type"));
        Assert.Equal("off", cut.Find("input").GetAttribute("autocomplete"));
    }

    [Fact]
    public void Renders_EachTag_AsAChip_InAListNamedAfterTheLabel()
    {
        var cut = Render(p => p.Add(x => x.Value, ["Blazor", "C#"]));

        var list = cut.Find("[role='list']");
        Assert.Equal("Tags", list.GetAttribute("aria-label"));
        Assert.Equal(["Blazor", "C#"], ChipLabels(cut));
        Assert.Equal(2, cut.FindComponents<TwChip>().Count);
        Assert.Equal("tags-tag-0", cut.FindAll("[role='listitem']")[0].Id);
    }

    [Fact]
    public void ListLabel_FallsBackToAriaLabel_ThenToTheLocalizedText()
    {
        var withAriaLabel = TestContext.Render<TwTagInput>(p => p.Add(x => x.Value, ["a"]).Add(x => x.AriaLabel, "Topics"));
        var withNothing = TestContext.Render<TwTagInput>(p => p.Add(x => x.Value, ["a"]));

        Assert.Equal("Topics", withAriaLabel.Find("[role='list']").GetAttribute("aria-label"));
        Assert.Equal(Localizer[TwStrings.TagInput_Tags], withNothing.Find("[role='list']").GetAttribute("aria-label"));
    }

    [Fact]
    public void RendersNoList_WhileThereAreNoTags()
    {
        var cut = Render();

        Assert.Empty(cut.FindAll("[role='list']"));
    }

    [Fact]
    public void Chips_AreRemovable_WithTheirRemoveButton_OutOfTheTabOrder()
    {
        var cut = Render(p => p.Add(x => x.Value, ["Blazor"]));

        var remove = cut.Find("[role='listitem'] button");
        Assert.Equal(Localizer[TwStrings.Chip_Remove, "Blazor"], remove.GetAttribute("aria-label"));
        Assert.Equal("-1", remove.GetAttribute("tabindex"));
        Assert.Equal("-1", cut.Find("[role='listitem']").GetAttribute("tabindex"));
    }

    [Fact]
    public void Shows_ThePlaceholder_OnlyWhileThereAreNoTags()
    {
        var cut = Render(p => p.Add(x => x.Placeholder, "Add a tag"));
        Assert.Equal("Add a tag", cut.Find("input").GetAttribute("placeholder"));

        cut.Render(p => p.Add(x => x.Value, ["a"]));
        Assert.Null(cut.Find("input").GetAttribute("placeholder"));
    }

    [Fact]
    public void Defaults_TheNameAttribute_ToTheId()
    {
        var cut = Render();

        Assert.Equal("tags", cut.Find("input").GetAttribute("name"));
    }

    [Fact]
    public void Field_UsesTheThemeAndTheInputVariant()
    {
        var cut = Render(p => p.Add(x => x.Variant, InputVariant.Outlined));

        var field = cut.Find("[data-tw-tag-field]");
        Assert.Contains("flex-wrap", field.ClassList);
        Assert.Contains("min-h-10", field.ClassList);
        foreach (var cls in inputTheme.OutlinedBorder.Split(' ', StringSplitOptions.RemoveEmptyEntries))
        {
            Assert.Contains(cls, field.ClassList);
        }
    }

    [Fact]
    public void Dense_UsesTheDenseSize_AndMediumChips()
    {
        var cut = Render(p => p.Add(x => x.Dense, true).Add(x => x.Value, ["a"]));

        Assert.Contains("min-h-8", cut.Find("[data-tw-tag-field]").ClassList);
        Assert.Equal(ChipSize.Medium, cut.FindComponent<TwChip>().Instance.Size);
    }

    [Fact]
    public void NotDense_UsesLargeChips()
    {
        var cut = Render(p => p.Add(x => x.Value, ["a"]));

        Assert.Equal(ChipSize.Large, cut.FindComponent<TwChip>().Instance.Size);
    }

    [Fact]
    public void ChipColorAndVariant_ArePassedToEveryChip()
    {
        var cut = Render(p => p.Add(x => x.Value, ["a", "b"]).Add(x => x.ChipColor, Color.Success).Add(x => x.ChipVariant, ButtonVariant.Filled));

        Assert.All(cut.FindComponents<TwChip>(), chip =>
        {
            Assert.Equal(Color.Success, chip.Instance.Color);
            Assert.Equal(ButtonVariant.Filled, chip.Instance.Variant);
        });
    }

    [Fact]
    public void ChipsPadding_IsAppliedOnlyWhileThereAreTags()
    {
        var cut = Render(p => p.Add(x => x.Variant, InputVariant.Outlined));
        Assert.DoesNotContain(theme.ChipsPadding, cut.Find("[data-tw-tag-field]").ClassList);

        cut.Render(p => p.Add(x => x.Value, ["a"]));
        Assert.Contains(theme.ChipsPadding, cut.Find("[data-tw-tag-field]").ClassList);
    }

    [Fact]
    public void Class_IsMergedOntoTheField()
    {
        var cut = Render(p => p.Add(x => x.Class, "min-h-20"));

        var field = cut.Find("[data-tw-tag-field]");
        Assert.Contains("min-h-20", field.ClassList);
        Assert.DoesNotContain("min-h-10", field.ClassList);
    }

    [Fact]
    public void HelperText_IsShown_AndDescribesTheTextInput()
    {
        var cut = Render(p => p.Add(x => x.HelperText, "Press Enter to add a tag."));

        var helper = cut.Find("#tags-helper");
        Assert.Equal("Press Enter to add a tag.", helper.TextContent);
        Assert.Equal("tags-helper", cut.Find("input").GetAttribute("aria-describedby"));
    }

    [Fact]
    public void Required_MarksTheTextInputRequired_ForAssistiveTechnology()
    {
        var cut = Render(p => p.Add(x => x.Required, true));

        Assert.Equal("true", cut.Find("input").GetAttribute("aria-required"));
    }

    [Fact]
    public void WithoutSuggestions_TheTextInputIsNotACombobox()
    {
        var cut = Render();

        var input = cut.Find("input");
        Assert.Null(input.GetAttribute("role"));
        Assert.Null(input.GetAttribute("aria-expanded"));
    }

    #endregion

    #region Adding tags

    [Fact]
    public async Task CommitText_AddsATag_AndEmptiesTheText()
    {
        var changes = new List<IReadOnlyList<string>>();
        var cut = RenderBound(changes);
        Type(cut, "Blazor");

        await Commit(cut, "Blazor");

        Assert.Equal(["Blazor"], changes.Single());
        Assert.Equal(["Blazor"], ChipLabels(cut));
        Assert.Equal(string.Empty, cut.Find("input").GetAttribute("value"));
    }

    [Fact]
    public async Task CommitText_TrimsTheTag_AndIgnoresBlankText()
    {
        var changes = new List<IReadOnlyList<string>>();
        var cut = RenderBound(changes);

        await Commit(cut, "   ");
        await Commit(cut, "  Blazor  ");

        Assert.Equal(["Blazor"], changes.Single());
    }

    [Fact]
    public async Task CommitText_AddsEveryPart_OfTextHoldingSeparators()
    {
        var changes = new List<IReadOnlyList<string>>();
        var cut = RenderBound(changes, ["a"]);

        await Commit(cut, "b, c");

        Assert.Equal(["a", "b", "c"], changes.Single());
    }

    [Fact]
    public async Task AddingATag_InvokesOnTagAdded_AndAnnouncesIt()
    {
        var added = new List<string>();
        var cut = Render(p => p.Add(x => x.OnTagAdded, (string tag) => added.Add(tag)));

        await Commit(cut, "Blazor");

        Assert.Equal(["Blazor"], added);
        Assert.Equal(Localizer[TwStrings.TagInput_Added, "Blazor"], cut.Find("[role='status']").TextContent);
    }

    [Fact]
    public void TypingASeparator_AddsTheTextBeforeIt_AndKeepsTheRest()
    {
        var changes = new List<IReadOnlyList<string>>();
        var cut = RenderBound(changes);

        Type(cut, "red,gre");

        Assert.Equal(["red"], changes.Single());
        Assert.Equal("gre", cut.Find("input").GetAttribute("value"));
    }

    [Fact]
    public void TypingWithoutASeparator_AddsNothing()
    {
        var changes = new List<IReadOnlyList<string>>();
        var cut = RenderBound(changes);

        Type(cut, "red");

        Assert.Empty(changes);
        Assert.Equal("red", cut.Find("input").GetAttribute("value"));
    }

    [Fact]
    public void CustomSeparators_ReplaceTheComma()
    {
        var changes = new List<IReadOnlyList<string>>();
        var cut = RenderBound(changes, configure: p => p.Add(x => x.Separators, [';']));

        Type(cut, "a,b;");

        Assert.Equal(["a,b"], changes.Single());
    }

    [Fact]
    public async Task PasteText_SplitsOnSeparatorsAndLineBreaks()
    {
        var changes = new List<IReadOnlyList<string>>();
        var cut = RenderBound(changes);

        await cut.InvokeAsync(() => cut.Instance.PasteTextAsync("one, two\nthree\r\nfour\tfive"));

        Assert.Equal(_pastedTags, changes.Single());
        var expectedAnnouncement = string.Join(" ", _pastedTags.Select(tag => Localizer[TwStrings.TagInput_Added, tag]));
        Assert.Equal(expectedAnnouncement, cut.Find("[role='status']").TextContent);
    }

    [Fact]
    public async Task PasteText_DropsDuplicatesWithinThePaste()
    {
        var changes = new List<IReadOnlyList<string>>();
        var cut = RenderBound(changes);

        await cut.InvokeAsync(() => cut.Instance.PasteTextAsync("a,A,b"));

        Assert.Equal(["a", "b"], changes.Single());
    }

    [Fact]
    public async Task AddOnBlur_AddsTheTypedText_WhenFocusLeaves()
    {
        var changes = new List<IReadOnlyList<string>>();
        var cut = RenderBound(changes, configure: p => p.Add(x => x.AddOnBlur, true));
        Type(cut, "Blazor");

        await cut.Find("input").BlurAsync(new FocusEventArgs());

        Assert.Equal(["Blazor"], changes.Single());
    }

    [Fact]
    public async Task WithoutAddOnBlur_LeavingTheField_KeepsTheText()
    {
        var changes = new List<IReadOnlyList<string>>();
        var cut = RenderBound(changes);
        Type(cut, "Blazor");

        await cut.Find("input").BlurAsync(new FocusEventArgs());

        Assert.Empty(changes);
        Assert.Equal("Blazor", cut.Find("input").GetAttribute("value"));
    }

    [Fact]
    public void ANewValueFromTheParent_ReplacesTheTags()
    {
        var cut = Render(p => p.Add(x => x.Value, ["a"]));

        cut.Render(p => p.Add(x => x.Value, ["x", "y"]));

        Assert.Equal(["x", "y"], ChipLabels(cut));
    }

    #endregion

    #region Rejecting tags

    [Fact]
    public async Task Duplicate_IsRejected_IgnoringCase_AndStaysInTheField()
    {
        var changes = new List<IReadOnlyList<string>>();
        var rejections = new List<TagRejectedEventArgs>();
        var cut = RenderBound(changes, ["Blazor"], p => p.Add(x => x.OnTagRejected, (TagRejectedEventArgs r) => rejections.Add(r)));

        await Commit(cut, "blazor");

        Assert.Empty(changes);
        var rejection = Assert.Single(rejections);
        Assert.Equal("blazor", rejection.Tag);
        Assert.Equal(TagRejectionReason.Duplicate, rejection.Reason);
        Assert.Equal(Localizer[TwStrings.TagInput_Duplicate, "blazor"], rejection.Message);
        Assert.Equal("blazor", cut.Find("input").GetAttribute("value"));
    }

    [Fact]
    public async Task Rejection_IsShownUnderTheField_AndMarksItInvalid()
    {
        var cut = Render(p => p.Add(x => x.Value, ["a"]));

        await Commit(cut, "a");

        var error = cut.Find("#tags-error");
        Assert.Equal(Localizer[TwStrings.TagInput_Duplicate, "a"], error.TextContent);
        var input = cut.Find("input");
        Assert.Equal("true", input.GetAttribute("aria-invalid"));
        Assert.Equal("tags-error", input.GetAttribute("aria-describedby"));
    }

    [Fact]
    public async Task Rejection_IsCleared_WhenTheTextChanges()
    {
        var cut = Render(p => p.Add(x => x.Value, ["a"]));
        await Commit(cut, "a");

        Type(cut, "ab");

        Assert.Empty(cut.FindAll("#tags-error"));
        Assert.Null(cut.Find("input").GetAttribute("aria-invalid"));
    }

    [Fact]
    public async Task Rejection_AndHelperText_BothDescribeTheTextInput()
    {
        var cut = Render(p => p.Add(x => x.Value, ["a"]).Add(x => x.HelperText, "Hint"));

        await Commit(cut, "a");

        Assert.Equal("tags-helper tags-error", cut.Find("input").GetAttribute("aria-describedby"));
    }

    [Fact]
    public async Task AllowDuplicates_AddsTheSameTagAgain()
    {
        var changes = new List<IReadOnlyList<string>>();
        var cut = RenderBound(changes, ["a"], p => p.Add(x => x.AllowDuplicates, true));

        await Commit(cut, "a");

        Assert.Equal(["a", "a"], changes.Single());
    }

    [Fact]
    public async Task MaxTags_RejectsATagOverTheLimit()
    {
        var changes = new List<IReadOnlyList<string>>();
        var rejections = new List<TagRejectedEventArgs>();
        var cut = RenderBound(changes, ["a", "b"], p => p
            .Add(x => x.MaxTags, 2)
            .Add(x => x.OnTagRejected, (TagRejectedEventArgs r) => rejections.Add(r)));

        await Commit(cut, "c");

        Assert.Empty(changes);
        Assert.Equal(TagRejectionReason.MaxTags, rejections.Single().Reason);
        Assert.Equal(Localizer[TwStrings.TagInput_LimitOther, 2], cut.Find("#tags-error").TextContent);
    }

    [Fact]
    public async Task MaxTags_UsesTheSingularMessage_ForALimitOfOne()
    {
        var cut = Render(p => p.Add(x => x.Value, ["a"]).Add(x => x.MaxTags, 1));

        await Commit(cut, "b");

        Assert.Equal(Localizer[TwStrings.TagInput_LimitOne, 1], cut.Find("#tags-error").TextContent);
    }

    [Fact]
    public async Task MaxTags_StopsAPaste_AtTheLimit()
    {
        var changes = new List<IReadOnlyList<string>>();
        var cut = RenderBound(changes, configure: p => p.Add(x => x.MaxTags, 2));

        await cut.InvokeAsync(() => cut.Instance.PasteTextAsync("a,b,c,d"));

        Assert.Equal(["a", "b"], changes.Single());
        Assert.Equal("c", cut.Find("input").GetAttribute("value"));
    }

    [Fact]
    public async Task MaxTagLength_RejectsALongTag()
    {
        var rejections = new List<TagRejectedEventArgs>();
        var cut = Render(p => p
            .Add(x => x.MaxTagLength, 3)
            .Add(x => x.OnTagRejected, (TagRejectedEventArgs r) => rejections.Add(r)));

        await Commit(cut, "abcd");

        Assert.Equal(TagRejectionReason.MaxLength, rejections.Single().Reason);
        Assert.Equal(Localizer[TwStrings.TagInput_TooLongOther, 3], cut.Find("#tags-error").TextContent);
        Assert.Empty(ChipLabels(cut));
    }

    [Fact]
    public async Task TagValidator_RejectsATag_WithItsMessage()
    {
        var rejections = new List<TagRejectedEventArgs>();
        var cut = Render(p => p
            .Add(x => x.TagValidator, tag => tag.Contains('@') ? null : "Enter an email address")
            .Add(x => x.OnTagRejected, (TagRejectedEventArgs r) => rejections.Add(r)));

        await Commit(cut, "ada");
        await Commit(cut, "ada@example.com");

        Assert.Equal(TagRejectionReason.Invalid, rejections.Single().Reason);
        Assert.Equal("Enter an email address", rejections.Single().Message);
        Assert.Equal(["ada@example.com"], ChipLabels(cut));
    }

    #endregion

    #region Removing tags

    [Fact]
    public async Task RemoveButton_RemovesTheTag_AnnouncesIt_AndFocusesTheTextInput()
    {
        var changes = new List<IReadOnlyList<string>>();
        var removed = new List<string>();
        var cut = RenderBound(changes, ["a", "b"], p => p.Add(x => x.OnTagRemoved, (string tag) => removed.Add(tag)));

        await cut.FindAll("[role='listitem'] button")[0].ClickAsync(new MouseEventArgs());

        Assert.Equal(["b"], changes.Single());
        Assert.Equal(["a"], removed);
        Assert.Equal(Localizer[TwStrings.TagInput_Removed, "a"], cut.Find("[role='status']").TextContent);
        Assert.True(InputFocusCount() > 0);
    }

    [Fact]
    public async Task Backspace_InTheEmptyTextInput_RemovesTheLastTag()
    {
        var changes = new List<IReadOnlyList<string>>();
        var cut = RenderBound(changes, ["a", "b"]);

        await cut.Find("input").KeyDownAsync(new KeyboardEventArgs { Key = "Backspace" });

        Assert.Equal(["a"], changes.Single());
    }

    [Fact]
    public async Task Backspace_WithText_RemovesNoTag()
    {
        var changes = new List<IReadOnlyList<string>>();
        var cut = RenderBound(changes, ["a"]);
        Type(cut, "x");

        await cut.Find("input").KeyDownAsync(new KeyboardEventArgs { Key = "Backspace" });

        Assert.Empty(changes);
    }

    [Fact]
    public async Task HeldBackspace_DoesNotRemoveTags()
    {
        var changes = new List<IReadOnlyList<string>>();
        var cut = RenderBound(changes, ["a"]);

        await cut.Find("input").KeyDownAsync(new KeyboardEventArgs { Key = "Backspace", Repeat = true });

        Assert.Empty(changes);
    }

    [Theory]
    [InlineData("Delete", 1, new[] { "a", "c" }, "tags-tag-1")]
    [InlineData("Backspace", 1, new[] { "a", "c" }, "tags-tag-0")]
    [InlineData("Backspace", 0, new[] { "b", "c" }, "tags-tag-0")]
    public async Task DeleteOrBackspace_OnATag_RemovesIt_AndFocusesTheTagBesideIt(string key, int index, string[] expected, string focused)
    {
        var changes = new List<IReadOnlyList<string>>();
        var cut = RenderBound(changes, ["a", "b", "c"]);

        await cut.Find($"#tags-tag-{index}").KeyDownAsync(new KeyboardEventArgs { Key = key });

        Assert.Equal(expected, changes.Single());
        Assert.Equal(focused, FocusedIds().Last());
    }

    [Fact]
    public async Task Delete_OnTheLastTag_FocusesTheTextInput()
    {
        var cut = Render(p => p.Add(x => x.Value, ["a"]));

        await cut.Find("#tags-tag-0").KeyDownAsync(new KeyboardEventArgs { Key = "Delete" });

        Assert.Empty(ChipLabels(cut));
        Assert.Empty(FocusedIds());
        Assert.True(InputFocusCount() > 0);
    }

    #endregion

    #region Keyboard navigation

    [Fact]
    public async Task ArrowLeft_InTheEmptyTextInput_FocusesTheLastTag()
    {
        var cut = Render(p => p.Add(x => x.Value, ["a", "b"]));

        await cut.Find("input").KeyDownAsync(new KeyboardEventArgs { Key = "ArrowLeft" });

        Assert.Equal(["tags-tag-1"], FocusedIds());
    }

    [Fact]
    public async Task ArrowLeft_WithText_LeavesFocusInTheTextInput()
    {
        var cut = Render(p => p.Add(x => x.Value, ["a"]));
        Type(cut, "x");

        await cut.Find("input").KeyDownAsync(new KeyboardEventArgs { Key = "ArrowLeft" });

        Assert.Empty(FocusedIds());
    }

    [Fact]
    public async Task ArrowKeys_MoveBetweenTheTags_AndOnToTheTextInput()
    {
        var cut = Render(p => p.Add(x => x.Value, ["a", "b"]));

        await cut.Find("#tags-tag-1").KeyDownAsync(new KeyboardEventArgs { Key = "ArrowLeft" });
        await cut.Find("#tags-tag-0").KeyDownAsync(new KeyboardEventArgs { Key = "ArrowRight" });
        var before = InputFocusCount();
        await cut.Find("#tags-tag-1").KeyDownAsync(new KeyboardEventArgs { Key = "ArrowRight" });

        Assert.Equal(["tags-tag-0", "tags-tag-1"], FocusedIds());
        Assert.Equal(before + 1, InputFocusCount());
    }

    [Fact]
    public async Task HomeAndEnd_OnATag_MoveToTheFirstTag_AndToTheTextInput()
    {
        var cut = Render(p => p.Add(x => x.Value, ["a", "b", "c"]));

        await cut.Find("#tags-tag-2").KeyDownAsync(new KeyboardEventArgs { Key = "Home" });
        var before = InputFocusCount();
        await cut.Find("#tags-tag-0").KeyDownAsync(new KeyboardEventArgs { Key = "End" });

        Assert.Equal(["tags-tag-0"], FocusedIds());
        Assert.Equal(before + 1, InputFocusCount());
    }

    [Fact]
    public async Task InARightToLeftLayout_TheArrowKeysAreSwapped()
    {
        var provider = TestContext.Render<TwDirectionProvider>(p => p
            .Add(x => x.Direction, TextDirection.RightToLeft)
            .AddChildContent<TwTagInput>(t => t.Add(x => x.Id, "tags").Add(x => x.Value, ["a", "b"])));

        await provider.Find("input").KeyDownAsync(new KeyboardEventArgs { Key = "ArrowRight" });
        await provider.Find("#tags-tag-1").KeyDownAsync(new KeyboardEventArgs { Key = "ArrowRight" });

        Assert.Equal(["tags-tag-1", "tags-tag-0"], FocusedIds());
    }

    [Fact]
    public async Task ClickingTheField_FocusesTheTextInput()
    {
        var cut = Render();

        await cut.Find("[data-tw-tag-field]").ClickAsync(new MouseEventArgs());

        Assert.True(InputFocusCount() > 0);
    }

    [Fact]
    public async Task FocusAsync_FocusesTheTextInput()
    {
        var cut = Render();

        await cut.InvokeAsync(() => cut.Instance.FocusAsync());

        Assert.True(InputFocusCount() > 0);
    }

    #endregion

    #region Disabled and read-only

    [Fact]
    public void Disabled_DisablesTheTextInput_AndHidesTheRemoveButtons()
    {
        var cut = Render(p => p.Add(x => x.Value, ["a"]).Add(x => x.Disabled, true));

        Assert.True(cut.Find("input").HasAttribute("disabled"));
        Assert.Empty(cut.FindAll("[role='listitem'] button"));
        Assert.Null(cut.Find("[role='listitem']").GetAttribute("tabindex"));
        Assert.Contains(Theme.Interaction.DisabledOpacity, cut.Find("[data-tw-tag-field]").ClassName);
    }

    [Fact]
    public async Task ReadOnly_KeepsTheTags_ButIgnoresEdits()
    {
        var changes = new List<IReadOnlyList<string>>();
        var cut = RenderBound(changes, ["a"], p => p.Add(x => x.ReadOnly, true));

        await Commit(cut, "b");
        await cut.Find("input").KeyDownAsync(new KeyboardEventArgs { Key = "Backspace" });
        await cut.InvokeAsync(() => cut.Instance.PasteTextAsync("c,d"));

        Assert.Empty(changes);
        Assert.True(cut.Find("input").HasAttribute("readonly"));
        Assert.Empty(cut.FindAll("[role='listitem'] button"));
        Assert.Equal(["a"], ChipLabels(cut));
    }

    [Fact]
    public async Task Disabled_IgnoresTagKeys()
    {
        var changes = new List<IReadOnlyList<string>>();
        var cut = RenderBound(changes, ["a"], p => p.Add(x => x.Disabled, true));

        await cut.Find("#tags-tag-0").KeyDownAsync(new KeyboardEventArgs { Key = "Delete" });

        Assert.Empty(changes);
    }

    #endregion

    #region Clearable

    [Fact]
    public void Clearable_ShowsALabelledClearButton_OutOfTheTabOrder()
    {
        var cut = Render(p => p.Add(x => x.Clearable, true).Add(x => x.Value, ["a"]));

        var button = cut.Find("button[data-tw-field-button]");
        Assert.Equal(Localizer[TwStrings.TagInput_ClearAll], button.GetAttribute("aria-label"));
        Assert.Equal("-1", button.GetAttribute("tabindex"));
        Assert.False(button.HasAttribute("disabled"));
        Assert.Contains(inputTheme.ClearablePadding, cut.Find("[data-tw-tag-field]").ClassList);
    }

    [Fact]
    public void Clearable_DisablesTheButton_WhileTheFieldIsEmpty()
    {
        var cut = Render(p => p.Add(x => x.Clearable, true));

        Assert.True(cut.Find("button[data-tw-field-button]").HasAttribute("disabled"));
    }

    [Fact]
    public void Clearable_HidesTheButton_WhenDisabledOrReadOnly()
    {
        var disabled = Render(p => p.Add(x => x.Clearable, true).Add(x => x.Disabled, true).Add(x => x.Value, ["a"]));
        var readOnly = Render(p => p.Add(x => x.Clearable, true).Add(x => x.ReadOnly, true).Add(x => x.Value, ["a"]));

        Assert.Empty(disabled.FindAll("button[data-tw-field-button]"));
        Assert.Empty(readOnly.FindAll("button[data-tw-field-button]"));
    }

    [Fact]
    public async Task ClearButton_RemovesEveryTag_InvokesOnClear_AndAnnouncesIt()
    {
        var changes = new List<IReadOnlyList<string>>();
        var removed = new List<string>();
        var cleared = 0;
        var cut = RenderBound(changes, ["a", "b"], p => p
            .Add(x => x.Clearable, true)
            .Add(x => x.OnClear, () => cleared++)
            .Add(x => x.OnTagRemoved, (string tag) => removed.Add(tag)));

        await cut.Find("button[data-tw-field-button]").ClickAsync(new MouseEventArgs());

        Assert.Empty(changes.Single());
        Assert.Equal(["a", "b"], removed);
        Assert.Equal(1, cleared);
        Assert.Equal(Localizer[TwStrings.TagInput_Cleared], cut.Find("[role='status']").TextContent);
        Assert.True(InputFocusCount() > 0);
    }

    #endregion

    #region Suggestions

    [Fact]
    public void WithSuggestions_TheTextInputIsAClosedCombobox()
    {
        var cut = Render(p => p.Add(x => x.Suggestions, _languages));

        var input = cut.Find("input");
        Assert.Equal("combobox", input.GetAttribute("role"));
        Assert.Equal("list", input.GetAttribute("aria-autocomplete"));
        Assert.Equal("false", input.GetAttribute("aria-expanded"));
        Assert.Empty(cut.FindAll("[role='listbox']"));
    }

    [Fact]
    public void Typing_ListsTheMatchingSuggestions()
    {
        var cut = Render(p => p.Add(x => x.Suggestions, _languages));

        Type(cut, "#");

        var options = cut.FindAll("[role='option']").Select(o => o.TextContent.Trim()).ToList();
        Assert.Equal(["C#", "F#"], options);
        var input = cut.Find("input");
        Assert.Equal("true", input.GetAttribute("aria-expanded"));
        Assert.Equal("tags-listbox", input.GetAttribute("aria-controls"));
    }

    [Fact]
    public void Suggestions_LeaveOutTheTagsAlreadyAdded()
    {
        var cut = Render(p => p.Add(x => x.Suggestions, _languages).Add(x => x.Value, ["C#"]));

        Type(cut, "#");

        Assert.Equal(["F#"], cut.FindAll("[role='option']").Select(o => o.TextContent.Trim()));
    }

    [Fact]
    public void Suggestions_AreNotShown_BeforeMinCharacters()
    {
        var cut = Render(p => p.Add(x => x.Suggestions, _languages).Add(x => x.MinCharacters, 2));

        Type(cut, "t");
        Assert.Empty(cut.FindAll("[role='listbox']"));

        Type(cut, "ty");
        Assert.Single(cut.FindAll("[role='option']"));
    }

    [Fact]
    public void Suggestions_AreClosed_WhenNothingMatches()
    {
        var cut = Render(p => p.Add(x => x.Suggestions, _languages));

        Type(cut, "zzz");

        Assert.Empty(cut.FindAll("[role='listbox']"));
        Assert.Equal("false", cut.Find("input").GetAttribute("aria-expanded"));
    }

    [Fact]
    public void MaxSuggestions_LimitsTheList()
    {
        var cut = Render(p => p.Add(x => x.Suggestions, _languages).Add(x => x.MaxSuggestions, 2));

        Type(cut, "t");

        Assert.Equal(2, cut.FindAll("[role='option']").Count);
    }

    [Fact]
    public async Task ClickingASuggestion_AddsIt_AndClosesTheList()
    {
        var changes = new List<IReadOnlyList<string>>();
        var cut = RenderBound(changes, configure: p => p.Add(x => x.Suggestions, _languages));
        Type(cut, "rus");

        await cut.Find("[role='option']").ClickAsync(new MouseEventArgs());

        Assert.Equal(["Rust"], changes.Single());
        Assert.Empty(cut.FindAll("[role='listbox']"));
        Assert.Equal(string.Empty, cut.Find("input").GetAttribute("value"));
    }

    [Fact]
    public async Task OpenFromKeyboard_ListsEverySuggestion_AndHighlightsTheFirst()
    {
        var cut = Render(p => p.Add(x => x.Suggestions, _languages));

        await cut.InvokeAsync(() => cut.Instance.OpenFromKeyboardAsync("first"));

        Assert.Equal(_languages.Length, cut.FindAll("[role='option']").Count);
        var refresh = TestContext.JSInterop.Invocations.Last(i => i.Identifier == "twAutocomplete.refresh");
        Assert.Equal("first", refresh.Arguments[1]);
    }

    [Fact]
    public async Task OpenFromKeyboard_DoesNothing_WithoutSuggestions()
    {
        var cut = Render();

        await cut.InvokeAsync(() => cut.Instance.OpenFromKeyboardAsync("first"));

        Assert.Empty(cut.FindAll("[role='listbox']"));
    }

    [Fact]
    public async Task Escape_ClosesTheSuggestions()
    {
        var cut = Render(p => p.Add(x => x.Suggestions, _languages));
        Type(cut, "#");

        await cut.Find("input").KeyDownAsync(new KeyboardEventArgs { Key = "Escape" });

        Assert.Empty(cut.FindAll("[role='listbox']"));
    }

    [Fact]
    public async Task SearchFunc_SuppliesTheSuggestions()
    {
        var queries = new List<string>();
        var cut = Render(p => p.Add(x => x.SearchFunc, (query, _) =>
        {
            queries.Add(query);
            return Task.FromResult<IEnumerable<string>>([$"{query} one", $"{query} two"]);
        }));

        Type(cut, "abc");

        cut.WaitForAssertion(() => Assert.Equal(["abc one", "abc two"], cut.FindAll("[role='option']").Select(o => o.TextContent.Trim())));
        Assert.Equal(["abc"], queries);
    }

    [Fact]
    public async Task SelectOption_IgnoresAnUnknownKey()
    {
        var changes = new List<IReadOnlyList<string>>();
        var cut = RenderBound(changes, configure: p => p.Add(x => x.Suggestions, _languages));
        Type(cut, "#");

        await cut.InvokeAsync(() => cut.Instance.SelectOptionAsync(99));

        Assert.Empty(changes);
    }

    #endregion

    #region Forms

    [Fact]
    public async Task InsideAnEditForm_ReportsEachChange()
    {
        var model = new Profile();
        var context = new EditContext(model);
        var changed = new List<string>();
        context.OnFieldChanged += (_, e) => changed.Add(e.FieldIdentifier.FieldName);

        var cut = TestContext.Render<CascadingValue<EditContext>>(p => p
            .Add(x => x.Value, context)
            .Add(x => x.IsFixed, false)
            .AddChildContent<TwTagInput>(t => t
                .Add(x => x.Value, model.Tags)
                .Add(x => x.ValueChanged, (IReadOnlyList<string> v) => model.Tags = v)
                .Add(x => x.ValueExpression, () => model.Tags)));
        var input = cut.FindComponent<TwTagInput>();

        await input.InvokeAsync(() => input.Instance.CommitTextAsync("a"));

        Assert.Equal([nameof(Profile.Tags)], changed);
        Assert.Equal(["a"], model.Tags);
    }

    [Fact]
    public void InsideAnEditForm_ShowsTheFormsMessage()
    {
        var model = new Profile();
        var context = new EditContext(model);
        var messages = new ValidationMessageStore(context);

        var cut = TestContext.Render<CascadingValue<EditContext>>(p => p
            .Add(x => x.Value, context)
            .Add(x => x.IsFixed, false)
            .AddChildContent<TwTagInput>(t => t
                .Add(x => x.Id, "tags")
                .Add(x => x.Value, model.Tags)
                .Add(x => x.ValueExpression, () => model.Tags)));

        messages.Add(context.Field(nameof(Profile.Tags)), "Add at least one tag");
        context.NotifyValidationStateChanged();

        cut.WaitForAssertion(() =>
        {
            Assert.Equal("Add at least one tag", cut.Find("#tags-error").TextContent);
            Assert.Equal("true", cut.Find("input").GetAttribute("aria-invalid"));
        });
    }

    [Fact]
    public void Invalid_ShowsTheErrorMessage()
    {
        var cut = Render(p => p.Add(x => x.Invalid, true).Add(x => x.ErrorMessage, "Wrong"));

        Assert.Equal("Wrong", cut.Find("#tags-error").TextContent);
        Assert.Equal("true", cut.Find("input").GetAttribute("aria-invalid"));
    }

    #endregion

    #region Script

    [Fact]
    public void Attaches_TheScripts_WithTheSeparators()
    {
        var cut = Render(p => p.Add(x => x.Separators, [',', ';']));

        Assert.Single(TestContext.JSInterop.Invocations, i => i.Identifier == "twAutocomplete.attach");
        var attach = Assert.Single(TestContext.JSInterop.Invocations, i => i.Identifier == "twTagInput.attach");
        Assert.Equal(",;", attach.Arguments[2]);
    }

    [Fact]
    public void ChangingTheSeparators_UpdatesTheScript()
    {
        var cut = Render();

        cut.Render(p => p.Add(x => x.Separators, [' ']));

        var update = Assert.Single(TestContext.JSInterop.Invocations, i => i.Identifier == "twTagInput.update");
        Assert.Equal(" ", update.Arguments[1]);
    }

    [Fact]
    public async Task Dispose_DetachesTheScripts()
    {
        var cut = Render();

        await cut.InvokeAsync(() => cut.Instance.DisposeAsync().AsTask());

        Assert.Contains(TestContext.JSInterop.Invocations, i => i.Identifier == "twTagInput.detach");
        Assert.Contains(TestContext.JSInterop.Invocations, i => i.Identifier == "twAutocomplete.detach");
    }

    [Fact]
    public async Task Dispose_ToleratesALostCircuit()
    {
        TestContext.JSInterop.SetupVoid("twTagInput.detach", _ => true).SetException(new JSDisconnectedException("circuit gone"));
        var cut = Render();

        var exception = await Record.ExceptionAsync(() => cut.InvokeAsync(() => cut.Instance.DisposeAsync().AsTask()));

        Assert.Null(exception);
    }

    #endregion

    #region Localization

    [Fact]
    public async Task Announcements_FollowTheConfiguredLanguage()
    {
        await using var context = new BunitContext();
        context.Services.AddTwBlazor(options => options.Localization.Culture = System.Globalization.CultureInfo.GetCultureInfo("de"), TwBlazor.Theme.Theme.CreateDefaultTheme());
        context.JSInterop.Mode = JSRuntimeMode.Loose;
        var cut = context.Render<TwTagInput>(p => p.Add(x => x.Value, ["a"]));

        await cut.Find("input").KeyDownAsync(new KeyboardEventArgs { Key = "Backspace" });

        Assert.Equal("a entfernt", cut.Find("[role='status']").TextContent);
    }

    #endregion
}
