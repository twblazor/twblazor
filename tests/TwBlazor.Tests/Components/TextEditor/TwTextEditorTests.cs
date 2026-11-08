using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Text;
using TwBlazor.Components;
using TwBlazor.Configuration.Components;
using TwBlazor.Enums;
using TwBlazor.Localization;
using TwBlazor.Models;
using ThemeBase = TwBlazor.Theme.Theme;

namespace TwBlazor.Tests.Components.TextEditor;

public class TwTextEditorTests : TwBlazorTestBase
{
    private TwTextEditorTheme theme => Theme.Components.Require<TwTextEditorTheme>();

    private sealed class Article
    {
        [Required(ErrorMessage = "Write the article")]
        public string? Body { get; set; }
    }

    private IRenderedComponent<TwTextEditor> Render(Action<ComponentParameterCollectionBuilder<TwTextEditor>>? configure = null) =>
        TestContext.Render<TwTextEditor>(p =>
        {
            p.Add(x => x.Id, "ed");
            p.Add(x => x.Label, "Body");
            configure?.Invoke(p);
        });

    private static AngleSharp.Dom.IElement Content(IRenderedComponent<TwTextEditor> cut) => cut.Find("#ed");

    private static AngleSharp.Dom.IElement ToggleNamed(IRenderedComponent<TwTextEditor> cut, string name) =>
        cut.FindAll("button").Single(b => b.GetAttribute("aria-label") == name);

    private static bool HasButton(IRenderedComponent<TwTextEditor> cut, string name) =>
        cut.FindAll("button").Any(b => b.GetAttribute("aria-label") == name);

    private List<string> FormatCommands() =>
        [.. TestContext.JSInterop.Invocations.Where(i => i.Identifier == "twTextEditor.format").Select(i => (string)i.Arguments[1]!)];

    private List<string?> SetHtmlCalls() =>
        [.. TestContext.JSInterop.Invocations.Where(i => i.Identifier == "twTextEditor.setHtml").Select(i => (string?)i.Arguments[1])];

    private static Task SendState(IRenderedComponent<TwTextEditor> cut, string? link = null, int index = 0, int length = 0, params string[] formats) =>
        cut.InvokeAsync(() => cut.Instance.NotifyStateAsync(new TextEditorSelectionState { Index = index, Length = length, Formats = formats, Link = link }));

    #region Rendering

    [Fact]
    public void Renders_AMultilineTextbox_NamedByTheLabel()
    {
        var cut = Render();

        var content = Content(cut);
        Assert.Equal("textbox", content.GetAttribute("role"));
        Assert.Equal("true", content.GetAttribute("aria-multiline"));
        Assert.Equal("true", content.GetAttribute("contenteditable"));
        Assert.Equal("ed-label", content.GetAttribute("aria-labelledby"));
        Assert.Null(content.GetAttribute("aria-label"));
        Assert.Equal("Body", cut.Find("#ed-label").TextContent.Trim());
        Assert.Equal("true", content.GetAttribute("spellcheck"));
    }

    [Fact]
    public void Content_IsNamedByAriaLabel_OrAGenericName_WithoutALabel()
    {
        var named = TestContext.Render<TwTextEditor>(p => p.Add(x => x.Id, "ed").Add(x => x.AriaLabel, "Reply"));
        Assert.Equal("Reply", Content(named).GetAttribute("aria-label"));
        Assert.Null(Content(named).GetAttribute("aria-labelledby"));

        var unnamed = TestContext.Render<TwTextEditor>(p => p.Add(x => x.Id, "ed"));
        Assert.Equal(Localizer[TwStrings.TextEditor_Label], Content(unnamed).GetAttribute("aria-label"));
    }

    [Fact]
    public void Content_UsesAriaLabelledBy_WhenSet()
    {
        var cut = Render(p => p.Add(x => x.AriaLabelledBy, "heading"));

        Assert.Equal("heading", Content(cut).GetAttribute("aria-labelledby"));
    }

    [Fact]
    public void Placeholder_IsShownThroughDataAndAriaAttributes()
    {
        var cut = Render(p => p.Add(x => x.Placeholder, "Write here"));

        Assert.Equal("Write here", Content(cut).GetAttribute("data-placeholder"));
        Assert.Equal("Write here", Content(cut).GetAttribute("aria-placeholder"));
    }

    [Fact]
    public void Content_HasTheThemeClasses()
    {
        var cut = Render();

        var classes = Content(cut).ClassList;
        Assert.Contains(theme.Content.Split(' ')[0], classes);
        Assert.Contains(theme.Typography.Split(' ')[0], classes);
    }

    [Fact]
    public void DefaultVariant_RemovesTheContentSidePadding()
    {
        var cut = Render(p => p.Add(x => x.Variant, InputVariant.Default));

        Assert.Contains(theme.ContentDefaultVariant, Content(cut).ClassList);
    }

    [Theory]
    [InlineData(InputVariant.Outlined)]
    [InlineData(InputVariant.Filled)]
    [InlineData(InputVariant.Default)]
    public void Box_DrawsTheInputVariantBorder(InputVariant variant)
    {
        var cut = Render(p => p.Add(x => x.Variant, variant));
        var inputTheme = Theme.Components.Require<TwInputTheme>();
        var expected = variant == InputVariant.Outlined ? inputTheme.OutlinedBorder : inputTheme.FilledBorder;

        var box = cut.Find("[data-tw-text-editor]");
        foreach (var cls in expected.Split(' ', StringSplitOptions.RemoveEmptyEntries))
        {
            Assert.Contains(cls, box.ClassList);
        }
    }

    [Fact]
    public void Height_DefaultsToAMinimumOf150px()
    {
        var cut = Render();

        Assert.Equal("min-height:150px", Content(cut).GetAttribute("style"));
    }

    [Fact]
    public void Height_SetsAFixedHeight_AndMaxHeightCapsIt()
    {
        var fixedHeight = Render(p => p.Add(x => x.Height, "120px"));
        Assert.Equal("height:120px", Content(fixedHeight).GetAttribute("style"));

        var capped = Render(p => p.Add(x => x.MinHeight, "80px").Add(x => x.MaxHeight, "200px"));
        Assert.Equal("min-height:80px;max-height:200px", Content(capped).GetAttribute("style"));

        var none = Render(p => p.Add(x => x.MinHeight, null));
        Assert.Null(Content(none).GetAttribute("style"));
    }

    [Fact]
    public void HelperText_AndToolbarHint_DescribeTheContent()
    {
        var cut = Render(p => p.Add(x => x.HelperText, "Markdown is not supported"));

        Assert.Equal("Markdown is not supported", cut.Find("#ed-helper").TextContent);
        Assert.Equal(Localizer[TwStrings.TextEditor_ToolbarHint], cut.Find("#ed-hint").TextContent);
        Assert.Equal("ed-helper ed-hint", Content(cut).GetAttribute("aria-describedby"));
    }

    [Fact]
    public void Invalid_ShowsTheErrorMessage_AndMarksTheContent()
    {
        var cut = Render(p => p.Add(x => x.Invalid, true).Add(x => x.ErrorMessage, "Too long"));

        Assert.Equal("true", Content(cut).GetAttribute("aria-invalid"));
        Assert.Contains("Too long", cut.Markup);
        Assert.Contains("ed-error", Content(cut).GetAttribute("aria-describedby"));
    }

    [Fact]
    public void Required_IsAnnounced()
    {
        var cut = Render(p => p.Add(x => x.Required, true));

        Assert.Equal("true", Content(cut).GetAttribute("aria-required"));
    }

    [Fact]
    public void Disabled_MakesTheContentUneditable_AndUnfocusable_AndDisablesTheToolbar()
    {
        var cut = Render(p => p.Add(x => x.Disabled, true));

        var content = Content(cut);
        Assert.Equal("false", content.GetAttribute("contenteditable"));
        Assert.Equal("true", content.GetAttribute("aria-disabled"));
        Assert.Null(content.GetAttribute("tabindex"));
        Assert.True(ToggleNamed(cut, Localizer[TwStrings.TextEditor_Bold]).HasAttribute("disabled"));
        Assert.True(ToggleNamed(cut, Localizer[TwStrings.TextEditor_Link]).HasAttribute("disabled"));
        foreach (var cls in theme.Disabled.Split(' '))
        {
            Assert.Contains(cls, cut.Find("[data-tw-text-editor]").ClassList);
        }

        Assert.DoesNotContain("ed-hint", content.GetAttribute("aria-describedby") ?? string.Empty);
    }

    [Fact]
    public void ReadOnly_KeepsTheContentInTheTabOrder_ButUneditable()
    {
        var cut = Render(p => p.Add(x => x.ReadOnly, true));

        var content = Content(cut);
        Assert.Equal("false", content.GetAttribute("contenteditable"));
        Assert.Equal("true", content.GetAttribute("aria-readonly"));
        Assert.Equal("0", content.GetAttribute("tabindex"));
        Assert.True(ToggleNamed(cut, Localizer[TwStrings.TextEditor_Bold]).HasAttribute("disabled"));
    }

    #endregion

    #region Toolbar presets

    [Fact]
    public void Toolbar_IsANamedToolbar_ThatControlsTheContent()
    {
        var cut = Render();

        var toolbar = cut.Find("[role='toolbar']");
        Assert.Equal("ed-toolbar", toolbar.Id);
        Assert.Equal(Localizer[TwStrings.TextEditor_Toolbar], toolbar.GetAttribute("aria-label"));
        Assert.Equal("ed", toolbar.GetAttribute("aria-controls"));
    }

    [Fact]
    public void None_RendersNoToolbar_AndNoHint()
    {
        var cut = Render(p => p.Add(x => x.Toolbar, TextEditorToolbar.None));

        Assert.Empty(cut.FindAll("[role='toolbar']"));
        Assert.Empty(cut.FindAll("#ed-hint"));
        Assert.Null(Content(cut).GetAttribute("aria-describedby"));
    }

    [Fact]
    public void Simple_HasMarksAndLists_Only()
    {
        var cut = Render(p => p.Add(x => x.Toolbar, TextEditorToolbar.Simple));

        Assert.True(HasButton(cut, Localizer[TwStrings.TextEditor_Bold]));
        Assert.True(HasButton(cut, Localizer[TwStrings.TextEditor_Italic]));
        Assert.True(HasButton(cut, Localizer[TwStrings.TextEditor_Underline]));
        Assert.True(HasButton(cut, Localizer[TwStrings.TextEditor_BulletList]));
        Assert.True(HasButton(cut, Localizer[TwStrings.TextEditor_NumberedList]));
        Assert.False(HasButton(cut, Localizer[TwStrings.TextEditor_Strikethrough]));
        Assert.False(HasButton(cut, Localizer[TwStrings.TextEditor_Link]));
        Assert.False(HasButton(cut, Localizer[TwStrings.TextEditor_Undo]));
        Assert.DoesNotContain(Localizer[TwStrings.TextEditor_Paragraph], cut.Markup);
    }

    [Fact]
    public void Standard_AddsTheTextStyleList_StrikethroughAndLinks()
    {
        var cut = Render();

        Assert.Contains(Localizer[TwStrings.TextEditor_Paragraph], cut.Markup);
        Assert.True(HasButton(cut, Localizer[TwStrings.TextEditor_Strikethrough]));
        Assert.True(HasButton(cut, Localizer[TwStrings.TextEditor_Link]));
        Assert.False(HasButton(cut, Localizer[TwStrings.TextEditor_Blockquote]));
        Assert.False(HasButton(cut, Localizer[TwStrings.TextEditor_Source]));
    }

    [Fact]
    public void Full_AddsBlocksHistoryAndSource()
    {
        var cut = Render(p => p.Add(x => x.Toolbar, TextEditorToolbar.Full));

        foreach (var key in new[] { TwStrings.TextEditor_Blockquote, TwStrings.TextEditor_CodeBlock, TwStrings.TextEditor_ClearFormatting, TwStrings.TextEditor_Undo, TwStrings.TextEditor_Redo, TwStrings.TextEditor_Source })
        {
            Assert.True(HasButton(cut, Localizer[key]), key);
        }
    }

    [Fact]
    public void Groups_AreNamedForAssistiveTechnology()
    {
        var cut = Render(p => p.Add(x => x.Toolbar, TextEditorToolbar.Full));

        var names = cut.FindAll("[role='group']").Select(g => g.GetAttribute("aria-label")).ToList();
        Assert.Contains(Localizer[TwStrings.TextEditor_TextFormatting], names);
        Assert.Contains(Localizer[TwStrings.TextEditor_Lists], names);
        Assert.Contains(Localizer[TwStrings.TextEditor_Blocks], names);
        Assert.Contains(Localizer[TwStrings.TextEditor_History], names);
    }

    [Fact]
    public void Buttons_ListTheirShortcuts()
    {
        var cut = Render(p => p.Add(x => x.Toolbar, TextEditorToolbar.Full));

        Assert.Equal("Control+B", ToggleNamed(cut, Localizer[TwStrings.TextEditor_Bold]).GetAttribute("aria-keyshortcuts"));
        Assert.Equal("Control+Shift+8", ToggleNamed(cut, Localizer[TwStrings.TextEditor_BulletList]).GetAttribute("aria-keyshortcuts"));
        Assert.Equal("Control+K", ToggleNamed(cut, Localizer[TwStrings.TextEditor_Link]).GetAttribute("aria-keyshortcuts"));
        Assert.Equal("Control+Z", ToggleNamed(cut, Localizer[TwStrings.TextEditor_Undo]).GetAttribute("aria-keyshortcuts"));
    }

    [Fact]
    public void Custom_RendersOnlyTheToolbarContent_WithTheEditorAsContext()
    {
        TwTextEditor? context = null;
        var cut = Render(p => p
            .Add(x => x.Toolbar, TextEditorToolbar.Custom)
            .Add(x => x.ToolbarContent, editor => builder =>
            {
                context = editor;
                builder.AddMarkupContent(0, "<button id=\"mine\" aria-label=\"Mine\">M</button>");
            }));

        Assert.NotNull(cut.Find("#mine"));
        Assert.Same(cut.Instance, context);
        Assert.False(HasButton(cut, Localizer[TwStrings.TextEditor_Bold]));
    }

    [Fact]
    public void ToolbarContent_IsAddedAfterAPreset()
    {
        var cut = Render(p => p
            .Add(x => x.Toolbar, TextEditorToolbar.Simple)
            .Add(x => x.ToolbarContent, _ => builder => builder.AddMarkupContent(0, "<button id=\"mine\" aria-label=\"Mine\">M</button>")));

        Assert.NotNull(cut.Find("#mine"));
        Assert.True(HasButton(cut, Localizer[TwStrings.TextEditor_Bold]));
    }

    #endregion

    #region Value and browser callbacks

    [Fact]
    public void FirstRender_WritesTheValue_AndAttachesTheEditor()
    {
        Render(p => p.Add(x => x.Value, "<p>Hello</p>"));

        Assert.Equal(["<p>Hello</p>"], SetHtmlCalls());
        var attach = TestContext.JSInterop.Invocations.Single(i => i.Identifier == "twTextEditor.attach");
        Assert.NotNull(attach.Arguments[1]);
    }

    [Fact]
    public void FirstRender_WithoutAValue_WritesNothing()
    {
        Render();

        Assert.Empty(SetHtmlCalls());
    }

    [Fact]
    public void AValueChangedByTheParent_IsWrittenIntoTheEditor()
    {
        var cut = Render(p => p.Add(x => x.Value, "<p>one</p>"));

        cut.Render(p => p.Add(x => x.Value, "<p>two</p>"));

        Assert.Equal(["<p>one</p>", "<p>two</p>"], SetHtmlCalls());
    }

    [Fact]
    public async Task TypedContent_IsReported_AndNotWrittenBack()
    {
        string? bound = null;
        TextEditorChangeEventArgs? change = null;
        var cut = Render(p => p
            .Add(x => x.ValueChanged, (string? v) => bound = v)
            .Add(x => x.OnTextChange, (TextEditorChangeEventArgs e) => change = e));

        await cut.InvokeAsync(() => cut.Instance.NotifyInputAsync("<p>typed</p>", "typed", 5));
        cut.Render(p => p.Add(x => x.Value, "<p>typed</p>"));

        Assert.Equal("<p>typed</p>", bound);
        Assert.Equal("<p>typed</p>", change!.Html);
        Assert.Equal("typed", change.Text);
        Assert.Equal(5, change.Length);
        Assert.Equal("<p>typed</p>", cut.Instance.Html);
        Assert.Empty(SetHtmlCalls());
    }

    [Fact]
    public async Task State_PressesTheToolbarButtons_AndPicksTheTextStyle()
    {
        var cut = Render();

        await SendState(cut, null, 0, 0, "Bold", "Heading2", "BulletList");

        Assert.Equal("true", ToggleNamed(cut, Localizer[TwStrings.TextEditor_Bold]).GetAttribute("aria-pressed"));
        Assert.Equal("false", ToggleNamed(cut, Localizer[TwStrings.TextEditor_Italic]).GetAttribute("aria-pressed"));
        Assert.Equal("true", ToggleNamed(cut, Localizer[TwStrings.TextEditor_BulletList]).GetAttribute("aria-pressed"));
        Assert.Contains(Localizer[TwStrings.TextEditor_Heading2], cut.Find("[role='toolbar']").TextContent);
        Assert.True(cut.Instance.IsActive(TextEditorFormat.Heading2));
        Assert.False(cut.Instance.IsActive(TextEditorFormat.Paragraph));
    }

    [Fact]
    public async Task State_RaisesOnSelectionChange()
    {
        TextEditorSelectionEventArgs? selection = null;
        var cut = Render(p => p.Add(x => x.OnSelectionChange, (TextEditorSelectionEventArgs e) => selection = e));

        await SendState(cut, "https://example.com", 4, 3, "Italic", "Link", "Nonsense");

        Assert.Equal(4, selection!.Index);
        Assert.Equal(3, selection.Length);
        Assert.Equal("https://example.com", selection.LinkUrl);
        Assert.Contains(TextEditorFormat.Italic, selection.Formats);
        Assert.Contains(TextEditorFormat.Link, selection.Formats);
        // No paragraph style was sent, so the text is a plain paragraph.
        Assert.Contains(TextEditorFormat.Paragraph, selection.Formats);
        Assert.Equal(3, selection.Formats.Count);
    }

    [Fact]
    public async Task State_IgnoresNull()
    {
        var cut = Render();

        await cut.InvokeAsync(() => cut.Instance.NotifyStateAsync(null));

        Assert.Equal([TextEditorFormat.Paragraph], cut.Instance.ActiveFormats);
    }

    [Fact]
    public async Task Focus_AndBlur_RaiseTheirCallbacks()
    {
        var focused = 0;
        var blurred = 0;
        var cut = Render(p => p.Add(x => x.OnFocus, () => focused++).Add(x => x.OnBlur, () => blurred++));

        await cut.InvokeAsync(cut.Instance.NotifyFocusAsync);
        await cut.InvokeAsync(cut.Instance.NotifyBlurAsync);

        Assert.Equal(1, focused);
        Assert.Equal(1, blurred);
    }

    #endregion

    #region Toolbar actions

    [Fact]
    public void PressingAToggle_RunsItsFormat()
    {
        var cut = Render(p => p.Add(x => x.Toolbar, TextEditorToolbar.Full));

        ToggleNamed(cut, Localizer[TwStrings.TextEditor_Bold]).Click();
        ToggleNamed(cut, Localizer[TwStrings.TextEditor_NumberedList]).Click();
        ToggleNamed(cut, Localizer[TwStrings.TextEditor_Blockquote]).Click();

        Assert.Equal(["Bold", "NumberedList", "Blockquote"], FormatCommands());
    }

    [Fact]
    public async Task ReleasingAPressedToggle_RunsItsFormatAgain_ToRemoveIt()
    {
        var cut = Render();
        await SendState(cut, null, 0, 0, "Bold");

        ToggleNamed(cut, Localizer[TwStrings.TextEditor_Bold]).Click();

        Assert.Equal(["Bold"], FormatCommands());
    }

    [Fact]
    public void HistoryButtons_RunTheirCommands()
    {
        var cut = Render(p => p.Add(x => x.Toolbar, TextEditorToolbar.Full));

        ToggleNamed(cut, Localizer[TwStrings.TextEditor_ClearFormatting]).Click();
        ToggleNamed(cut, Localizer[TwStrings.TextEditor_Undo]).Click();
        ToggleNamed(cut, Localizer[TwStrings.TextEditor_Redo]).Click();

        Assert.Equal(["ClearFormatting", "Undo", "Redo"], FormatCommands());
    }

    [Fact]
    public async Task PublicMethods_RunTheirCommands()
    {
        var cut = Render();

        await cut.InvokeAsync(() => cut.Instance.FormatAsync(TextEditorFormat.Heading3));
        await cut.InvokeAsync(cut.Instance.UndoAsync);
        await cut.InvokeAsync(cut.Instance.RedoAsync);
        await cut.InvokeAsync(cut.Instance.ClearFormattingAsync);

        Assert.Equal(["Heading3", "Undo", "Redo", "ClearFormatting"], FormatCommands());
    }

    [Fact]
    public async Task Commands_DoNothing_WhileReadOnly()
    {
        var cut = Render(p => p.Add(x => x.ReadOnly, true));

        await cut.InvokeAsync(() => cut.Instance.FormatAsync(TextEditorFormat.Bold));
        var linked = await cut.InvokeAsync(() => cut.Instance.InsertLinkAsync("https://example.com"));

        Assert.Empty(FormatCommands());
        Assert.False(linked);
    }

    [Fact]
    public async Task SetHtmlAsync_WritesAndReportsTheContent()
    {
        TestContext.JSInterop.Setup<string?>("twTextEditor.setHtml", _ => true).SetResult("<p>clean</p>");
        TestContext.JSInterop.Setup<string?>("twTextEditor.getText", _ => true).SetResult("clean");
        TestContext.JSInterop.Setup<int>("twTextEditor.getLength", _ => true).SetResult(5);
        string? bound = null;
        TextEditorChangeEventArgs? change = null;
        var cut = Render(p => p
            .Add(x => x.ValueChanged, (string? v) => bound = v)
            .Add(x => x.OnTextChange, (TextEditorChangeEventArgs e) => change = e));

        await cut.InvokeAsync(() => cut.Instance.SetHtmlAsync("<p>clean<script></script></p>"));

        Assert.Equal("<p>clean</p>", bound);
        Assert.Equal(5, change!.Length);
        Assert.Equal("clean", change.Text);
    }

    [Fact]
    public async Task GetMethods_ReadFromTheBrowser()
    {
        TestContext.JSInterop.Setup<string?>("twTextEditor.getHtml", _ => true).SetResult("<p>a</p>");
        TestContext.JSInterop.Setup<string?>("twTextEditor.getText", _ => true).SetResult("a");
        TestContext.JSInterop.Setup<int>("twTextEditor.getLength", _ => true).SetResult(1);
        var cut = Render();

        Assert.Equal("<p>a</p>", await cut.InvokeAsync(cut.Instance.GetHtmlAsync));
        Assert.Equal("a", await cut.InvokeAsync(cut.Instance.GetTextAsync));
        Assert.Equal(1, await cut.InvokeAsync(cut.Instance.GetLengthAsync));
    }

    [Fact]
    public void ClickingTheLabel_FocusesTheContent()
    {
        var cut = Render();

        cut.Find("#ed-label").Click();

        Assert.Contains(TestContext.JSInterop.Invocations, i => i.Identifier == "twTextEditor.focus");
    }

    [Fact]
    public async Task Dispose_DetachesTheEditor()
    {
        var cut = Render();

        await cut.Instance.DisposeAsync();

        Assert.Contains(TestContext.JSInterop.Invocations, i => i.Identifier == "twTextEditor.detach");
    }

    #endregion

    #region Links

    [Fact]
    public void LinkButton_OpensTheLinkBar_AndFocusesItsField()
    {
        var cut = Render();

        ToggleNamed(cut, Localizer[TwStrings.TextEditor_Link]).Click();

        var panel = cut.Find("#ed-link");
        Assert.Equal("group", panel.GetAttribute("role"));
        Assert.Equal(Localizer[TwStrings.TextEditor_LinkUrl], cut.Find("#ed-link-url").GetAttribute("aria-label"));
        Assert.Equal("url", cut.Find("#ed-link-url").GetAttribute("type"));
        // The link icon is drawn over the start of the field, so the field is padded to keep text clear of it.
        Assert.Contains(theme.LinkInputField, cut.Find("#ed-link-url").ClassList);
        Assert.Equal("true", ToggleNamed(cut, Localizer[TwStrings.TextEditor_Link]).GetAttribute("aria-expanded"));
        Assert.Contains(TestContext.JSInterop.Invocations, i => i.Identifier == "twTextEditor.bindLinkInput" && (string?)i.Arguments[0] == "ed-link-url");
    }

    [Fact]
    public async Task LinkBar_StartsWithTheAddressOfTheLinkAtTheCaret_AndOffersToRemoveIt()
    {
        var cut = Render();
        await SendState(cut, "https://example.com", 0, 0, "Link");

        await cut.InvokeAsync(cut.Instance.OpenLinkPanelAsync);

        Assert.Equal("https://example.com", cut.Find("#ed-link-url").GetAttribute("value"));
        Assert.Contains(Localizer[TwStrings.TextEditor_LinkRemove], cut.Find("#ed-link").TextContent);
    }

    [Fact]
    public async Task FormatLink_OpensTheLinkBar()
    {
        var cut = Render();

        await cut.InvokeAsync(() => cut.Instance.FormatAsync(TextEditorFormat.Link));

        Assert.NotNull(cut.Find("#ed-link"));
    }

    [Fact]
    public async Task ApplyingAValidAddress_LinksIt_AndClosesTheBar()
    {
        var cut = Render();
        await cut.InvokeAsync(cut.Instance.OpenLinkPanelAsync);

        await cut.InvokeAsync(() => cut.Instance.ApplyLinkAsync("example.com"));

        var link = TestContext.JSInterop.Invocations.Single(i => i.Identifier == "twTextEditor.link");
        Assert.Equal("https://example.com", link.Arguments[1]);
        Assert.Empty(cut.FindAll("#ed-link"));
    }

    [Fact]
    public async Task ApplyingAnUnsafeAddress_KeepsTheBarOpen_AndShowsWhy()
    {
        var cut = Render();
        await cut.InvokeAsync(cut.Instance.OpenLinkPanelAsync);

        await cut.InvokeAsync(() => cut.Instance.ApplyLinkAsync("javascript:alert(1)"));

        Assert.DoesNotContain(TestContext.JSInterop.Invocations, i => i.Identifier == "twTextEditor.link");
        Assert.Equal("true", cut.Find("#ed-link-url").GetAttribute("aria-invalid"));
        Assert.Contains(Localizer[TwStrings.TextEditor_LinkInvalid], cut.Markup);
    }

    [Fact]
    public async Task CancellingTheLinkBar_ClosesIt_AndReturnsFocusToTheText()
    {
        var cut = Render();
        await cut.InvokeAsync(cut.Instance.OpenLinkPanelAsync);

        await cut.InvokeAsync(cut.Instance.CloseLinkPanelAsync);

        Assert.Empty(cut.FindAll("#ed-link"));
        Assert.Contains(TestContext.JSInterop.Invocations, i => i.Identifier == "twTextEditor.focus");
    }

    [Fact]
    public async Task EscapeInTheLinkBar_ClosesIt()
    {
        var cut = Render();
        await cut.InvokeAsync(cut.Instance.OpenLinkPanelAsync);

        cut.Find("#ed-link").KeyDown("Escape");

        Assert.Empty(cut.FindAll("#ed-link"));
    }

    [Fact]
    public async Task RemoveLink_UnlinksAndClosesTheBar()
    {
        var cut = Render();
        await cut.InvokeAsync(cut.Instance.OpenLinkPanelAsync);

        await cut.InvokeAsync(cut.Instance.RemoveLinkAsync);

        Assert.Contains(TestContext.JSInterop.Invocations, i => i.Identifier == "twTextEditor.unlink");
        Assert.Empty(cut.FindAll("#ed-link"));
    }

    [Fact]
    public async Task LinkBar_DoesNotOpen_WhileDisabled()
    {
        var cut = Render(p => p.Add(x => x.Disabled, true));

        await cut.InvokeAsync(cut.Instance.OpenLinkPanelAsync);

        Assert.Empty(cut.FindAll("#ed-link"));
    }

    [Theory]
    [InlineData("https://example.com", "https://example.com")]
    [InlineData("  http://example.com/a?b=c  ", "http://example.com/a?b=c")]
    [InlineData("example.com", "https://example.com")]
    [InlineData("example.com:8080/path", "https://example.com:8080/path")]
    [InlineData("//cdn.example.com", "https://cdn.example.com")]
    [InlineData("ada@example.com", "mailto:ada@example.com")]
    [InlineData("mailto:ada@example.com", "mailto:ada@example.com")]
    [InlineData("tel:+44123", "tel:+44123")]
    [InlineData("/docs", "/docs")]
    [InlineData("#section", "#section")]
    [InlineData("../up", "../up")]
    [InlineData("javascript:alert(1)", null)]
    [InlineData("JAVASCRIPT:alert(1)", null)]
    [InlineData("data:text/html,x", null)]
    [InlineData("java\tscript:alert(1)", null)]
    [InlineData("not a url", null)]
    [InlineData("localhost", null)]
    [InlineData("", null)]
    [InlineData(null, null)]
    public void NormalizeUrl_KeepsSafeAddresses_AndRefusesTheRest(string? input, string? expected)
    {
        Assert.Equal(expected, TwTextEditor.NormalizeUrl(input));
    }

    #endregion

    #region Source view

    [Fact]
    public async Task SourceToggle_ShowsTheHtmlInAMultilineField_AndHidesTheText()
    {
        TestContext.JSInterop.Setup<string?>("twTextEditor.getHtml", _ => true).SetResult("<p>a</p><ul><li>b</li></ul>");
        var cut = Render(p => p.Add(x => x.Toolbar, TextEditorToolbar.Full));

        ToggleNamed(cut, Localizer[TwStrings.TextEditor_Source]).Click();

        var source = cut.Find("textarea#ed-source");
        Assert.Equal("<p>a</p>\n<ul>\n<li>b</li>\n</ul>", source.GetAttribute("value") ?? source.TextContent);
        Assert.True(Content(cut).HasAttribute("hidden"));
        Assert.True(ToggleNamed(cut, Localizer[TwStrings.TextEditor_Bold]).HasAttribute("disabled"));
        Assert.Equal("true", ToggleNamed(cut, Localizer[TwStrings.TextEditor_Source]).GetAttribute("aria-pressed"));
        await Task.CompletedTask;
    }

    [Fact]
    public void EditingTheSource_WritesItBackIntoTheEditor()
    {
        TestContext.JSInterop.Setup<string?>("twTextEditor.getHtml", _ => true).SetResult("<p>a</p>");
        var cut = Render(p => p.Add(x => x.Toolbar, TextEditorToolbar.Full));
        ToggleNamed(cut, Localizer[TwStrings.TextEditor_Source]).Click();

        cut.Find("textarea#ed-source").Change("<h1>new</h1>");

        Assert.Contains("<h1>new</h1>", SetHtmlCalls());
    }

    [Fact]
    public void LeavingTheSourceView_WritesTheSourceIntoTheEditor()
    {
        TestContext.JSInterop.Setup<string?>("twTextEditor.getHtml", _ => true).SetResult("<p>a</p>");
        var cut = Render(p => p.Add(x => x.Toolbar, TextEditorToolbar.Full));
        var toggle = () => ToggleNamed(cut, Localizer[TwStrings.TextEditor_Source]);

        toggle().Click();
        toggle().Click();

        Assert.Empty(cut.FindAll("textarea#ed-source"));
        Assert.False(Content(cut).HasAttribute("hidden"));
        Assert.Equal(["<p>a</p>"], SetHtmlCalls());
    }

    [Fact]
    public void FormatSource_PutsEachBlockOnItsOwnLine()
    {
        Assert.Equal("<h1>a</h1>\n<p>b</p>\n<blockquote>\n<p>c</p>\n</blockquote>", TwTextEditor.FormatSource("<h1>a</h1><p>b</p><blockquote><p>c</p></blockquote>"));
        Assert.Equal(string.Empty, TwTextEditor.FormatSource(string.Empty));
    }

    #endregion

    #region Export

    [Fact]
    public void ExportMenu_IsHidden_WithoutFormats()
    {
        var cut = Render();

        Assert.False(HasButton(cut, Localizer[TwStrings.TextEditor_Export]));
    }

    [Fact]
    public void ExportMenu_ListsTheFormats_InOrder_WithTheirExtensions()
    {
        var cut = Render(p => p.Add(x => x.ExportFormats, [TextEditorExportFormat.Docx, TextEditorExportFormat.Markdown, TextEditorExportFormat.Docx]));

        ToggleNamed(cut, Localizer[TwStrings.TextEditor_Export]).Click();

        var items = cut.FindAll("[role='menuitem']").Select(i => i.TextContent.Trim()).ToList();
        Assert.Equal([$"{Localizer[TwStrings.TextEditor_ExportDocx]} (.docx)", $"{Localizer[TwStrings.TextEditor_ExportMarkdown]} (.md)"], items);
    }

    [Fact]
    public void ExportMenu_StaysAvailable_WhileReadOnly_ButNotWhileDisabled()
    {
        var readOnly = Render(p => p.Add(x => x.ReadOnly, true).Add(x => x.ExportFormats, [TextEditorExportFormat.Markdown]));
        Assert.False(ToggleNamed(readOnly, Localizer[TwStrings.TextEditor_Export]).HasAttribute("disabled"));

        var disabled = Render(p => p.Add(x => x.Disabled, true).Add(x => x.ExportFormats, [TextEditorExportFormat.Markdown]));
        Assert.True(ToggleNamed(disabled, Localizer[TwStrings.TextEditor_Export]).HasAttribute("disabled"));
    }

    [Fact]
    public async Task ExportAsync_SavesTheFile_AndAnnouncesIt()
    {
        TestContext.JSInterop.Setup<string?>("twTextEditor.getHtml", _ => true).SetResult("<h1>Notes</h1>");
        var cut = Render(p => p.Add(x => x.ExportFileName, "Minutes"));

        var file = await cut.InvokeAsync(() => cut.Instance.ExportAsync(TextEditorExportFormat.Markdown));

        Assert.Equal("Minutes.md", file.FileName);
        Assert.Equal("# Notes\n", Encoding.UTF8.GetString(file.Content));
        var save = TestContext.JSInterop.Invocations.Single(i => i.Identifier == "twDownload.save");
        Assert.Equal("Minutes.md", save.Arguments[0]);
        Assert.Equal(TwTextEditorExport.MarkdownContentType, save.Arguments[1]);
        Assert.Contains(Localizer[TwStrings.TextEditor_Exported, "Minutes.md"], cut.Find("[role='status']").TextContent);
    }

    [Fact]
    public async Task ExportAsync_UsesTheFileNameGiven()
    {
        var cut = Render();

        var file = await cut.InvokeAsync(() => cut.Instance.ExportAsync(TextEditorExportFormat.Docx, "Report"));

        Assert.Equal("Report.docx", file.FileName);
    }

    [Fact]
    public async Task ChoosingAFormat_ExportsIt()
    {
        var cut = Render(p => p.Add(x => x.ExportFormats, [TextEditorExportFormat.Rtf]));

        ToggleNamed(cut, Localizer[TwStrings.TextEditor_Export]).Click();
        await cut.InvokeAsync(() => cut.Find("[role='menuitem']").Click());

        var save = TestContext.JSInterop.Invocations.Single(i => i.Identifier == "twDownload.save");
        Assert.Equal("document.rtf", save.Arguments[0]);
    }

    #endregion

    #region Context menu

    [Fact]
    public void ContextMenu_IsOff_ByDefault()
    {
        var cut = Render();

        Assert.Equal("disabled", cut.Find("#ed-context").GetAttribute("data-tw-context-menu"));
    }

    [Fact]
    public void ContextMenu_IsOn_WithShowContextMenu_ButNotWhileReadOnly()
    {
        var on = Render(p => p.Add(x => x.ShowContextMenu, true));
        Assert.Equal("enabled", on.Find("#ed-context").GetAttribute("data-tw-context-menu"));

        var readOnly = Render(p => p.Add(x => x.ShowContextMenu, true).Add(x => x.ReadOnly, true));
        Assert.Equal("disabled", readOnly.Find("#ed-context").GetAttribute("data-tw-context-menu"));
    }

    [Fact]
    public async Task ContextMenu_ItemsRunTheirFormats()
    {
        var cut = Render(p => p.Add(x => x.ShowContextMenu, true));
        var menu = cut.FindComponent<TwContextMenu>();
        await cut.InvokeAsync(() => menu.Instance.OpenAtAsync(10, 10));

        cut.FindAll("[role='menuitemcheckbox']")[0].Click();

        Assert.Equal(["Bold"], FormatCommands());
    }

    #endregion

    #region Forms and localization

    [Fact]
    public async Task InsideAnEditForm_AnEmptyEditor_FailsRequiredValidation()
    {
        var article = new Article { Body = "<p>x</p>" };
        var editContext = new EditContext(article);
        var cut = TestContext.Render<CascadingValue<EditContext>>(p => p
            .Add(x => x.Value, editContext)
            .AddChildContent<DataAnnotationsValidator>()
            .AddChildContent<TwTextEditor>(e => e
                .Add(x => x.Id, "ed")
                .Add(x => x.Label, "Body")
                .Add(x => x.Value, article.Body)
                .Add(x => x.ValueChanged, (string? v) => article.Body = v)
                .Add(x => x.ValueExpression, () => article.Body)));
        var editor = cut.FindComponent<TwTextEditor>();

        await cut.InvokeAsync(() => editor.Instance.NotifyInputAsync(string.Empty, string.Empty, 0));

        Assert.Equal(string.Empty, article.Body);
        Assert.Contains("Write the article", editContext.GetValidationMessages());
        cut.WaitForAssertion(() => Assert.Equal("true", editor.Find("#ed").GetAttribute("aria-invalid")));
    }

    [Fact]
    public void Toolbar_IsTranslated()
    {
        using var context = new BunitContext();
        context.Services.AddTwBlazor(options => options.Localization.Culture = CultureInfo.GetCultureInfo("de"), ThemeBase.CreateDefaultTheme());
        context.JSInterop.Mode = JSRuntimeMode.Loose;

        var cut = context.Render<TwTextEditor>(p => p.Add(x => x.Id, "ed"));

        Assert.Equal("Formatierung", cut.Find("[role='toolbar']").GetAttribute("aria-label"));
        Assert.Contains(cut.FindAll("button"), b => b.GetAttribute("aria-label") == "Fett");
        Assert.Equal("Texteditor", cut.Find("#ed").GetAttribute("aria-label"));
    }

    #endregion
}
