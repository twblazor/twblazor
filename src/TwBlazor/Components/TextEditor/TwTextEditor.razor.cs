// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using System.Linq.Expressions;
using TwBlazor.Configuration.Components;
using TwBlazor.Enums;
using TwBlazor.Export;
using TwBlazor.Localization;
using TwBlazor.Models;
using TwBlazor.Utilities;
using TwBlazor.Extensions;

namespace TwBlazor.Components;

/// <summary>
/// Represents a rich text editor that edits formatted text, such as headings, bold text, lists and links, and binds it
/// as HTML.
/// </summary>
/// <remarks>
/// <para>
/// Bind the content with <c>@bind-Value</c>. The value is HTML made only of the tags the editor produces
/// (<c>p</c>, <c>h1</c> to <c>h3</c>, <c>strong</c>, <c>em</c>, <c>u</c>, <c>s</c>, <c>ul</c>, <c>ol</c>, <c>li</c>,
/// <c>blockquote</c>, <c>pre</c>, <c>code</c>, <c>br</c> and <c>a</c> with an http, https, mailto, tel or relative
/// <c>href</c>), or an empty string when the editor holds no text. Everything that enters the editor, including a value
/// set from code and pasted content, is cleaned the same way. The cleaning runs in the browser, so treat HTML posted to a
/// server as untrusted and sanitize it again there before storing or showing it.
/// </para>
/// <para>
/// <see cref="Toolbar"/> chooses the controls above the text. The toolbar is built from <see cref="TwSelect{T}"/>,
/// <see cref="TwToggleGroup{T}"/>, <see cref="TwToggle"/>, <see cref="TwButton"/> and <see cref="TwSeparator"/>, the
/// link bar from <see cref="TwTextfield{T}"/>, and the HTML source view from a multiline <see cref="TwTextfield{T}"/>.
/// The editing area is a <c>textbox</c> named after the <see cref="TwBlazorInputComponentBase.Label"/>, and the toolbar
/// follows the ARIA toolbar pattern: <kbd>Alt</kbd> + <kbd>F10</kbd> moves focus from the text to the toolbar.
/// </para>
/// </remarks>
public partial class TwTextEditor : TwBlazorTextInputComponentBase
{
    private static readonly TextEditorFormat[] _markFormats = [TextEditorFormat.Bold, TextEditorFormat.Italic, TextEditorFormat.Underline, TextEditorFormat.Strikethrough];

    private static readonly TextEditorFormat[] _listFormats = [TextEditorFormat.BulletList, TextEditorFormat.NumberedList];

    private static readonly TextEditorFormat[] _blockFormats = [TextEditorFormat.Blockquote, TextEditorFormat.CodeBlock];

    private static readonly TextEditorFormat[] _styleFormats = [TextEditorFormat.Paragraph, TextEditorFormat.Heading1, TextEditorFormat.Heading2, TextEditorFormat.Heading3];

    private static readonly string[] _linkSchemes = ["http", "https", "mailto", "tel"];

    [Inject] private IJSRuntime jsRuntime { get; set; } = null!;

    private TwTextEditorTheme theme => options.Theme.Components.Require<TwTextEditorTheme>();

    private TwInputTheme inputTheme => options.Theme.Components.Require<TwInputTheme>();

    private TwToggleTheme toggleTheme => options.Theme.Components.Require<TwToggleTheme>();

    /// <summary>
    /// Gets or sets the content as HTML.
    /// </summary>
    [Parameter] public string? Value { get; set; }

    /// <summary>
    /// Gets or sets the callback invoked with the new HTML when the content changes.
    /// </summary>
    /// <remarks>While the user types, the change is reported once typing pauses for <see cref="DebounceInterval"/>, and
    /// always when focus leaves the editor.</remarks>
    [Parameter] public EventCallback<string?> ValueChanged { get; set; }

    /// <summary>
    /// Gets or sets the expression that identifies the bound value. Set automatically by <c>@bind-Value</c>, it lets an
    /// enclosing <see cref="EditForm"/> or <see cref="TwForm"/> validate the property the editor edits.
    /// </summary>
    [Parameter] public Expression<Func<string?>>? ValueExpression { get; set; }

    /// <summary>
    /// Gets or sets the text shown in the editor while it is empty.
    /// </summary>
    [Parameter] public string Placeholder { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the text shown under the editor. It is read out with the editor.
    /// </summary>
    [Parameter] public string HelperText { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets whether the editor must hold some text. The editor is announced as required by assistive technology.
    /// </summary>
    /// <remarks>Add a validation attribute such as <c>[Required]</c> to the bound property for the form to enforce it;
    /// an empty editor binds as an empty string.</remarks>
    [Parameter] public bool Required { get; set; }

    /// <summary>
    /// Gets or sets which controls the toolbar shows. Defaults to <see cref="TextEditorToolbar.Standard"/>.
    /// </summary>
    [Parameter] public TextEditorToolbar Toolbar { get; set; } = TextEditorToolbar.Standard;

    /// <summary>
    /// Gets or sets controls added to the end of the toolbar, or that make up the whole toolbar when
    /// <see cref="Toolbar"/> is <see cref="TextEditorToolbar.Custom"/>. The context is the editor, so a control can call
    /// <see cref="FormatAsync(TextEditorFormat)"/> and read <see cref="IsActive(TextEditorFormat)"/>.
    /// </summary>
    /// <remarks>Not shown when <see cref="Toolbar"/> is <see cref="TextEditorToolbar.None"/>. An icon-only control
    /// needs an <c>AriaLabel</c>.</remarks>
    [Parameter] public RenderFragment<TwTextEditor>? ToolbarContent { get; set; }

    /// <summary>
    /// Gets or sets the smallest height of the editing area, as a CSS length. Defaults to <c>150px</c>.
    /// </summary>
    [Parameter] public string? MinHeight { get; set; } = "150px";

    /// <summary>
    /// Gets or sets the largest height of the editing area, as a CSS length. The area scrolls once its text is taller.
    /// </summary>
    [Parameter] public string? MaxHeight { get; set; }

    /// <summary>
    /// Gets or sets a fixed height for the editing area, as a CSS length. The area scrolls once its text is taller.
    /// </summary>
    [Parameter] public string? Height { get; set; }

    /// <summary>
    /// Gets or sets how long, in milliseconds, to wait after the last keystroke before reporting the new content.
    /// Defaults to 250. Use 0 to report every keystroke.
    /// </summary>
    /// <remarks>Formatting from the toolbar, a paste and focus leaving the editor are reported at once.</remarks>
    [Parameter] public int DebounceInterval { get; set; } = 250;

    /// <summary>
    /// Gets or sets whether a right-click on the text opens a menu of formatting actions instead of the browser's own
    /// context menu. Keyboard users open it with <kbd>Shift</kbd> + <kbd>F10</kbd> or the Menu key.
    /// </summary>
    /// <remarks>Off by default, since the browser's menu holds spelling suggestions and clipboard actions.</remarks>
    [Parameter] public bool ShowContextMenu { get; set; }

    /// <summary>
    /// Gets or sets the formats offered by an export menu at the end of the toolbar, in the order listed. Empty (the
    /// default) shows no export menu.
    /// </summary>
    /// <remarks>The files are written by <see cref="TwTextEditorExport"/>, using only the .NET libraries, and saved by
    /// the browser. The menu is not shown when <see cref="Toolbar"/> is <see cref="TextEditorToolbar.None"/>; call
    /// <see cref="ExportAsync(TextEditorExportFormat, string?)"/> from your own control instead.</remarks>
    [Parameter] public IReadOnlyList<TextEditorExportFormat> ExportFormats { get; set; } = [];

    /// <summary>
    /// Gets or sets the name exported files are saved with, without the extension. It is also stored as the
    /// document's title. Defaults to <c>document</c>.
    /// </summary>
    [Parameter] public string ExportFileName { get; set; } = "document";

    /// <summary>
    /// Gets or sets the callback invoked with the new content whenever it changes.
    /// </summary>
    [Parameter] public EventCallback<TextEditorChangeEventArgs> OnTextChange { get; set; }

    /// <summary>
    /// Gets or sets the callback invoked when the selection moves or the formats at it change.
    /// </summary>
    [Parameter] public EventCallback<TextEditorSelectionEventArgs> OnSelectionChange { get; set; }

    /// <summary>
    /// Gets or sets the callback invoked when the editing area receives focus.
    /// </summary>
    [Parameter] public EventCallback OnFocus { get; set; }

    /// <summary>
    /// Gets or sets the callback invoked when the editing area loses focus.
    /// </summary>
    [Parameter] public EventCallback OnBlur { get; set; }

    private ElementReference contentRef;

    private DotNetObjectReference<TwTextEditor>? dotNetRef;

    private bool attached;

    private string? lastValue;

    private string? pendingHtml;

    private string currentHtml = string.Empty;

    private HashSet<TextEditorFormat> activeFormats = [TextEditorFormat.Paragraph];

    private string? linkAtSelection;

    private bool linkPanelOpen;

    private bool focusLinkInput;

    private string linkUrl = string.Empty;

    private bool linkInvalid;

    private bool sourceMode;

    private string sourceHtml = string.Empty;

    private string statusMessage = string.Empty;

    private string? attachedToolbarId;

    private int attachedDebounce;

    private List<BlockOption> blockOptions = [];

    /// <summary>
    /// A text style in the toolbar's list, shown by its localized name.
    /// </summary>
    private sealed record BlockOption(TextEditorFormat Format, string Name)
    {
        /// <inheritdoc />
        public override string ToString() => Name;
    }

    /// <summary>
    /// Gets the formats applied at the selection, such as <see cref="TextEditorFormat.Bold"/> and
    /// <see cref="TextEditorFormat.Heading2"/>.
    /// </summary>
    public IReadOnlySet<TextEditorFormat> ActiveFormats => activeFormats;

    /// <summary>
    /// Gets the content as HTML, as last reported by the editor.
    /// </summary>
    public string Html => currentHtml;

    private bool canEdit => !Disabled && !ReadOnly;

    private bool toolbarDisabled => !canEdit || sourceMode;

    private bool showToolbar => Toolbar != TextEditorToolbar.None;

    private bool hasStandard => Toolbar is TextEditorToolbar.Standard or TextEditorToolbar.Full;

    private bool hasFull => Toolbar == TextEditorToolbar.Full;

    private string labelId => string.IsNullOrEmpty(LabelId) ? $"{Id}-label" : LabelId;

    /// <summary>
    /// The editable area is the control itself, the same as a text field's input, so it carries the component's Id.
    /// </summary>
    private string contentId => Id ?? string.Empty;

    private string toolbarId => $"{Id}-toolbar";

    private string helperId => $"{Id}-helper";

    private string hintId => $"{Id}-hint";

    private string linkPanelId => $"{Id}-link";

    private string linkInputId => $"{Id}-link-url";

    private string sourceId => $"{Id}-source";

    private string contextMenuId => $"{Id}-context";

    private string exportMenuId => $"{Id}-export";

    private BlockOption? selectedBlock => blockOptions.FirstOrDefault(o => activeFormats.Contains(o.Format)) ?? blockOptions.FirstOrDefault();

    /// <summary>
    /// The editing area is named by <see cref="TwBlazorComponentBase.AriaLabelledBy"/>, then the visible label, then
    /// <see cref="TwBlazorComponentBase.AriaLabel"/>, then a generic "Text editor".
    /// </summary>
    private string? contentLabelledBy
    {
        get
        {
            if (!string.IsNullOrEmpty(AriaLabelledBy))
            {
                return AriaLabelledBy;
            }

            return string.IsNullOrWhiteSpace(Label) ? null : labelId;
        }
    }

    private string? contentAriaLabel => contentLabelledBy is not null
        ? null
        : AriaLabel ?? localizer[TwStrings.TextEditor_Label];

    private string? describedBy
    {
        get
        {
            List<string> ids = [];
            if (!string.IsNullOrWhiteSpace(HelperText))
            {
                ids.Add(helperId);
            }

            if (errorId is not null)
            {
                ids.Add(errorId);
            }

            if (showToolbar && canEdit)
            {
                ids.Add(hintId);
            }

            return GetDescribedBy(ids.Count == 0 ? null : string.Join(' ', ids));
        }
    }

    /// <summary>
    /// Builds the label's attributes: the consumer's own, plus a click that moves focus into the text, since a
    /// label's <c>for</c> only works with form controls.
    /// </summary>
    /// <returns>A new dictionary of attributes.</returns>
    private Dictionary<string, object> GetLabelAttributes() => new(LabelAttributes)
    {
        ["onclick"] = EventCallback.Factory.Create<MouseEventArgs>(this, FocusAsync),
    };

    private Dictionary<string, object> linkToggleAttributes => new()
    {
        ["aria-keyshortcuts"] = "Control+K",
        ["aria-expanded"] = linkPanelOpen ? "true" : "false",
        ["aria-controls"] = linkPanelId,
    };

    private Dictionary<string, object> sourceToggleAttributes => new()
    {
        ["aria-controls"] = $"{contentId} {sourceId}",
    };

    private string rootClasses => new ClassBuilder(theme.Root)
        .AddClass(surfaceClasses)
        .AddClass(theme.Disabled, Disabled)
        .AddClass(Class)
        .Build();

    /// <summary>
    /// The border, background, rounding and focus ring of the box, from the same <see cref="TwInputTheme"/> classes as a
    /// text field. The side padding the input variants add is left out, since the toolbar spans the full width.
    /// </summary>
    private string surfaceClasses
    {
        get
        {
            var focus = string.IsNullOrEmpty(inputTheme.FocusWithinBorder) ? inputTheme.FocusBorder : inputTheme.FocusWithinBorder;
            return surfaceVariant switch
            {
                InputVariant.Outlined => new ClassBuilder(inputTheme.OutlinedBorder).AddClass(focus).AddClass(roundedBuilder.GetRounded(effectiveRounded)).Build(),
                InputVariant.Filled => new ClassBuilder(inputTheme.FilledBorder).AddClass(focus).AddClass(inputTheme.FilledBackgroundColor).AddClass(roundedBuilder.GetRoundedTop(effectiveRounded)).Build(),
                _ => new ClassBuilder(inputTheme.FilledBorder).AddClass(focus).Build(),
            };
        }
    }

    private string contentClasses => new ClassBuilder(theme.Content)
        .AddClass(theme.Typography)
        .AddClass(theme.ContentDefaultVariant, surfaceVariant == InputVariant.Default)
        .Build();

    private string? contentStyle
    {
        get
        {
            var parts = new List<string>();
            if (!string.IsNullOrWhiteSpace(Height))
            {
                parts.Add($"height:{Height}");
            }
            else if (!string.IsNullOrWhiteSpace(MinHeight))
            {
                parts.Add($"min-height:{MinHeight}");
            }

            if (!string.IsNullOrWhiteSpace(MaxHeight))
            {
                parts.Add($"max-height:{MaxHeight}");
            }

            return parts.Count == 0 ? null : string.Join(';', parts);
        }
    }

    /// <summary>
    /// A toolbar button drawn the same way as the toolbar's toggles while they are not pressed, from
    /// <see cref="TwToggleTheme"/>, with its focus ring and rounding.
    /// </summary>
    private string toolbarButtonClasses => new ClassBuilder(toggleTheme.Base)
        .AddClass(toggleTheme.Sm)
        .AddClass(roundedBuilder.GetRounded(Rounded ?? toggleTheme.ToggleRounded ?? options.Theme.Rounded.DefaultRounded))
        .AddClass(toggleTheme.Unpressed)
        .AddClass(toggleTheme.Hover, !toolbarDisabled)
        .AddClass(toggleTheme.Disabled, toolbarDisabled)
        .AddClass(colorBuilder.GetFocusRing(null))
        .Build();

    /// <summary>
    /// The export menu's button: a toolbar button that stays enabled while the editor is read-only, since reading
    /// the content is allowed then.
    /// </summary>
    private string exportButtonClasses => new ClassBuilder(toggleTheme.Base)
        .AddClass(toggleTheme.Sm)
        .AddClass(roundedBuilder.GetRounded(Rounded ?? toggleTheme.ToggleRounded ?? options.Theme.Rounded.DefaultRounded))
        .AddClass(toggleTheme.Unpressed)
        .AddClass(toggleTheme.Hover, !Disabled)
        .AddClass(toggleTheme.Disabled, Disabled)
        .AddClass(colorBuilder.GetFocusRing(null))
        .Build();

    /// <summary>
    /// Gets whether a format is applied at the selection.
    /// </summary>
    /// <param name="format">The format to check.</param>
    /// <returns><see langword="true"/> when the format is applied at the selection.</returns>
    public bool IsActive(TextEditorFormat format) => activeFormats.Contains(format);

    private TextEditorFormat[] ActiveIn(TextEditorFormat[] group) => [.. group.Where(activeFormats.Contains)];

    private string WithShortcut(string labelKey, string shortcut) => $"{localizer[labelKey]} ({shortcut})";

    private static Dictionary<string, object> KeyShortcuts(string shortcut) => new() { ["aria-keyshortcuts"] = shortcut };

    /// <inheritdoc />
    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        if (string.IsNullOrEmpty(RootId))
        {
            RootId = $"{Id}-root";
        }

        blockOptions =
        [
            new(TextEditorFormat.Paragraph, localizer[TwStrings.TextEditor_Paragraph]),
            new(TextEditorFormat.Heading1, localizer[TwStrings.TextEditor_Heading1]),
            new(TextEditorFormat.Heading2, localizer[TwStrings.TextEditor_Heading2]),
            new(TextEditorFormat.Heading3, localizer[TwStrings.TextEditor_Heading3]),
        ];

        // Only a value the parent changed is written into the editor, so a re-render never overwrites text the user
        // typed that the parent has not caught up with yet.
        if (!string.Equals(Value, lastValue, StringComparison.Ordinal))
        {
            lastValue = Value;
            if (!string.Equals(Value ?? string.Empty, currentHtml, StringComparison.Ordinal))
            {
                pendingHtml = Value ?? string.Empty;
                currentHtml = pendingHtml;
            }
        }

        if (!canEdit)
        {
            linkPanelOpen = false;
        }
    }

    /// <inheritdoc />
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);

        var currentToolbarId = showToolbar ? toolbarId : null;

        try
        {
            if (firstRender)
            {
                dotNetRef = DotNetObjectReference.Create(this);
                if (!string.IsNullOrEmpty(Value))
                {
                    pendingHtml = Value;
                }

                if (pendingHtml is not null)
                {
                    await SetContentAsync(pendingHtml);
                }

                await jsRuntime.InvokeVoidAsync("twTextEditor.attach", contentRef, dotNetRef, new { toolbarId = currentToolbarId, debounce = DebounceInterval });
                attached = true;
                attachedToolbarId = currentToolbarId;
                attachedDebounce = DebounceInterval;
            }
            else
            {
                if (pendingHtml is not null)
                {
                    await SetContentAsync(pendingHtml);
                }

                if (attachedToolbarId != currentToolbarId || attachedDebounce != DebounceInterval)
                {
                    attachedToolbarId = currentToolbarId;
                    attachedDebounce = DebounceInterval;
                    await jsRuntime.InvokeVoidAsync("twTextEditor.update", contentRef, new { toolbarId = currentToolbarId, debounce = DebounceInterval });
                }
            }

            if (focusLinkInput)
            {
                focusLinkInput = false;
                await jsRuntime.InvokeVoidAsync("twTextEditor.bindLinkInput", linkInputId, dotNetRef);
            }
        }
        catch (JSDisconnectedException)
        {
            // The circuit disconnected before the script could run; nothing to set up.
        }
    }

    // The content calls below are left unguarded on purpose: an empty fallback would be reported through
    // ValueChanged, or handed to a caller saving the content, as if the user had cleared the editor.
#pragma warning disable BL0016
    private async Task SetContentAsync(string html)
    {
        pendingHtml = null;
        var sanitized = await jsRuntime.InvokeAsync<string?>("twTextEditor.setHtml", contentRef, html);
        currentHtml = sanitized ?? string.Empty;
    }

    #region Public methods

    /// <summary>
    /// Moves focus to the editing area, putting the caret back where it was.
    /// </summary>
    /// <returns>A task that completes once focus has moved.</returns>
    public async Task FocusAsync()
    {
        if (Disabled)
        {
            return;
        }

        await jsRuntime.TryInvokeVoidAsync("twTextEditor.focus", contentRef);
    }

    /// <summary>
    /// Gets the content as sanitized HTML, including any change still waiting for <see cref="DebounceInterval"/>.
    /// </summary>
    /// <returns>The HTML, or an empty string when the editor holds no text.</returns>
    public async Task<string> GetHtmlAsync() => await jsRuntime.InvokeAsync<string?>("twTextEditor.getHtml", contentRef) ?? string.Empty;

    /// <summary>
    /// Replaces the content, cleaning the HTML first, and reports the change through <see cref="ValueChanged"/>.
    /// </summary>
    /// <param name="html">The new content as HTML, or <see langword="null"/> to empty the editor.</param>
    /// <returns>A task that completes once the content has been replaced.</returns>
    public async Task SetHtmlAsync(string? html)
    {
        await SetContentAsync(html ?? string.Empty);
        var text = await GetTextAsync();
        var length = await jsRuntime.InvokeAsync<int>("twTextEditor.getLength", contentRef);
        await ReportChangeAsync(currentHtml, text, length);
    }

    /// <summary>
    /// Gets the content as plain text, with a line break between paragraphs.
    /// </summary>
    /// <returns>The text.</returns>
    public async Task<string> GetTextAsync() => await jsRuntime.InvokeAsync<string?>("twTextEditor.getText", contentRef) ?? string.Empty;

    /// <summary>
    /// Gets the number of characters in the content, not counting line breaks.
    /// </summary>
    /// <returns>The number of characters.</returns>
    public async Task<int> GetLengthAsync() => await jsRuntime.InvokeAsync<int>("twTextEditor.getLength", contentRef);
#pragma warning restore BL0016

    /// <summary>
    /// Applies a format to the selection, or removes it when it is already applied. A text style such as
    /// <see cref="TextEditorFormat.Heading1"/> replaces the style of the paragraphs the selection touches.
    /// </summary>
    /// <param name="format">The format to apply. <see cref="TextEditorFormat.Link"/> opens the link bar, or use
    /// <see cref="InsertLinkAsync(string)"/>.</param>
    /// <returns>A task that completes once the format has been applied.</returns>
    public async Task FormatAsync(TextEditorFormat format)
    {
        if (format == TextEditorFormat.Link)
        {
            await OpenLinkPanelAsync();
            return;
        }

        await RunCommandAsync(format.ToString());
    }

    /// <summary>
    /// Links the selected text to an address, changes the link the caret is in, or inserts the address as a link when
    /// nothing is selected.
    /// </summary>
    /// <param name="url">The address. One without a scheme, such as <c>example.com</c>, gets <c>https://</c>, and an
    /// email address gets <c>mailto:</c>.</param>
    /// <returns><see langword="true"/> when the link was added; <see langword="false"/> when the address is not a web,
    /// email, phone or relative address.</returns>
    public async Task<bool> InsertLinkAsync(string url)
    {
        var href = NormalizeUrl(url);
        if (href is null || !canEdit)
        {
            return false;
        }

        await jsRuntime.TryInvokeVoidAsync("twTextEditor.link", contentRef, href);
        return true;
    }

    /// <summary>
    /// Removes the link the caret is in, or the links in the selection, keeping their text.
    /// </summary>
    /// <returns>A task that completes once the link has been removed.</returns>
    public async Task RemoveLinkAsync()
    {
        if (!canEdit)
        {
            return;
        }

        await jsRuntime.TryInvokeVoidAsync("twTextEditor.unlink", contentRef);
        await CloseLinkPanelAsync();
    }

    /// <summary>
    /// Undoes the last change.
    /// </summary>
    /// <returns>A task that completes once the change has been undone.</returns>
    public Task UndoAsync() => RunCommandAsync("Undo");

    /// <summary>
    /// Redoes the last change that was undone.
    /// </summary>
    /// <returns>A task that completes once the change has been redone.</returns>
    public Task RedoAsync() => RunCommandAsync("Redo");

    /// <summary>
    /// Removes bold, italic, underline, strikethrough and links from the selection.
    /// </summary>
    /// <returns>A task that completes once the formatting has been removed.</returns>
    public Task ClearFormattingAsync() => RunCommandAsync("ClearFormatting");

    /// <summary>
    /// Exports the content as a file and has the browser save it. Any change still waiting for
    /// <see cref="DebounceInterval"/> is included, and the export is announced to screen readers.
    /// </summary>
    /// <param name="format">The file format.</param>
    /// <param name="fileName">The file name without its extension, or <see langword="null"/> to use
    /// <see cref="ExportFileName"/>.</param>
    /// <returns>The exported file, for example to send it somewhere else as well.</returns>
    public async Task<ExportFile> ExportAsync(TextEditorExportFormat format, string? fileName = null)
    {
        var html = sourceMode ? sourceHtml : await GetHtmlAsync();
        var file = TwTextEditorExport.Export(html, format, fileName ?? ExportFileName);

        if (await ExportDownload.SaveAsync(jsRuntime, file))
        {
            statusMessage = localizer[TwStrings.TextEditor_Exported, file.FileName];
            StateHasChanged();
        }

        return file;
    }

    #endregion

    private string ExportLabel(TextEditorExportFormat format)
    {
        var name = format switch
        {
            TextEditorExportFormat.Markdown => localizer[TwStrings.TextEditor_ExportMarkdown],
            TextEditorExportFormat.Docx => localizer[TwStrings.TextEditor_ExportDocx],
            TextEditorExportFormat.Odt => localizer[TwStrings.TextEditor_ExportOdt],
            TextEditorExportFormat.Html => localizer[TwStrings.TextEditor_ExportHtml],
            TextEditorExportFormat.PlainText => localizer[TwStrings.TextEditor_ExportPlainText],
            _ => localizer[TwStrings.TextEditor_ExportRtf],
        };

        return $"{name} ({TwTextEditorExport.ExtensionOf(format)})";
    }

    private static Icon ExportIcon(TextEditorExportFormat format) => format switch
    {
        TextEditorExportFormat.Markdown => Icon.Markdown,
        TextEditorExportFormat.Docx => Icon.File_Earmark_Word,
        TextEditorExportFormat.Odt => Icon.File_Earmark_Text,
        TextEditorExportFormat.Html => Icon.File_Earmark_Code,
        TextEditorExportFormat.PlainText => Icon.Filetype_Txt,
        _ => Icon.File_Earmark_Richtext,
    };

    /// <summary>
    /// Turns what the user typed into a link address: a web, email, phone or relative address is kept, an address with
    /// no scheme gets <c>https://</c> (or <c>mailto:</c> for an email address), and anything else, such as a
    /// <c>javascript:</c> address, is refused.
    /// </summary>
    /// <param name="url">The typed address.</param>
    /// <returns>The address to link to, or <see langword="null"/> when it is not allowed.</returns>
    internal static string? NormalizeUrl(string? url)
    {
        var value = url?.Trim() ?? string.Empty;
        if (value.Length == 0 || value.Any(char.IsControl))
        {
            return null;
        }

        if (IsRelativeAddress(value))
        {
            return value;
        }

        if (SchemeOf(value) is { } scheme)
        {
            return _linkSchemes.Contains(scheme, StringComparer.OrdinalIgnoreCase) && Uri.TryCreate(value, UriKind.Absolute, out _) ? value : null;
        }

        return WithAssumedScheme(value);
    }

    /// <summary>
    /// Gets whether an address points within the current site: a fragment, or a path that is not protocol-relative.
    /// </summary>
    private static bool IsRelativeAddress(string value) =>
        value.StartsWith('#')
        || (value.StartsWith('/') && !value.StartsWith("//", StringComparison.Ordinal))
        || value.StartsWith("./", StringComparison.Ordinal)
        || value.StartsWith("../", StringComparison.Ordinal);

    /// <summary>
    /// Gets the scheme an address starts with, such as <c>https</c>, or <see langword="null"/> when it has none. A host
    /// with a port, such as <c>example.com:8080</c>, has no scheme.
    /// </summary>
    private static string? SchemeOf(string value)
    {
        var colon = value.IndexOf(':');
        if (colon <= 0 || !char.IsAsciiLetter(value[0]) || !value[..colon].All(c => char.IsAsciiLetterOrDigit(c) || c is '+' or '-' or '.'))
        {
            return null;
        }

        var scheme = value[..colon];
        var isHostWithPort = colon + 1 < value.Length && char.IsAsciiDigit(value[colon + 1]) && scheme.Contains('.');
        return isHostWithPort ? null : scheme;
    }

    /// <summary>
    /// Adds the scheme an address with none most likely means: <c>mailto:</c> for an email address, otherwise
    /// <c>https://</c>. Returns <see langword="null"/> when the result is not an address.
    /// </summary>
    private static string? WithAssumedScheme(string value)
    {
        if (value.Any(char.IsWhiteSpace))
        {
            return null;
        }

        if (value.Contains('@') && !value.Contains('/'))
        {
            return $"mailto:{value}";
        }

        var withScheme = value.StartsWith("//", StringComparison.Ordinal) ? $"https:{value}" : $"https://{value}";
        return value.Contains('.') && Uri.TryCreate(withScheme, UriKind.Absolute, out _) ? withScheme : null;
    }

    private async Task RunCommandAsync(string command)
    {
        if (!canEdit || sourceMode)
        {
            return;
        }

        await jsRuntime.TryInvokeVoidAsync("twTextEditor.format", contentRef, command);
    }

    private async Task OnGroupChangedAsync(IEnumerable<TextEditorFormat> values, TextEditorFormat[] group)
    {
        var pressed = values.ToHashSet();
        var changed = group.Where(format => pressed.Contains(format) != activeFormats.Contains(format)).Take(1);
        foreach (var format in changed)
        {
            await FormatAsync(format);
        }
    }

    private async Task OnBlockChangedAsync(BlockOption? option)
    {
        if (option is null || activeFormats.Contains(option.Format))
        {
            return;
        }

        await FormatAsync(option.Format);
    }

    #region Link bar

    private async Task OnLinkToggleAsync(bool pressed)
    {
        if (linkPanelOpen)
        {
            await CloseLinkPanelAsync();
            return;
        }

        await OpenLinkPanelAsync();
    }

    /// <summary>
    /// Opens the bar for adding or changing a link, with the address of the link the caret is in. Called by
    /// <c>twTextEditor.attach</c> for <kbd>Ctrl</kbd> + <kbd>K</kbd>.
    /// </summary>
    /// <returns>A task that completes once the bar is open.</returns>
    [JSInvokable("OpenLinkPanel")]
    public Task OpenLinkPanelAsync()
    {
        if (!canEdit || sourceMode)
        {
            return Task.CompletedTask;
        }

        linkPanelOpen = true;
        linkInvalid = false;
        linkUrl = linkAtSelection ?? string.Empty;
        focusLinkInput = true;
        // Also called from script for Ctrl + K, where nothing renders for us.
#pragma warning disable BL0012
        StateHasChanged();
#pragma warning restore BL0012
        return Task.CompletedTask;
    }

    /// <summary>
    /// Adds or changes the link, called by <c>twTextEditor.bindLinkInput</c> when <kbd>Enter</kbd> is pressed in the
    /// address field and by the bar's Apply button. An address that is not allowed keeps the bar open and shows why.
    /// </summary>
    /// <param name="url">The typed address.</param>
    /// <returns>A task that completes once the link has been added.</returns>
    [JSInvokable("ApplyLink")]
    public async Task ApplyLinkAsync(string? url)
    {
        linkUrl = url ?? string.Empty;
        if (!await InsertLinkAsync(linkUrl))
        {
            linkInvalid = true;
            StateHasChanged();
            return;
        }

        linkPanelOpen = false;
        linkInvalid = false;
        StateHasChanged();
    }

    /// <summary>
    /// Closes the link bar without changing anything and returns focus to the text. Called by
    /// <c>twTextEditor.bindLinkInput</c> for <kbd>Escape</kbd>.
    /// </summary>
    /// <returns>A task that completes once focus is back in the text.</returns>
    [JSInvokable("CloseLinkPanel")]
    public async Task CloseLinkPanelAsync()
    {
        if (!linkPanelOpen)
        {
            return;
        }

        linkPanelOpen = false;
        linkInvalid = false;
        // Also called from script for Escape in the address field, where nothing renders for us.
#pragma warning disable BL0012
        StateHasChanged();
#pragma warning restore BL0012
        await FocusAsync();
    }

    private void OnLinkUrlChanged(string? value)
    {
        linkUrl = value ?? string.Empty;
        linkInvalid = false;
    }

    private async Task OnLinkPanelKeyDownAsync(KeyboardEventArgs e)
    {
        if (e.Key == "Escape")
        {
            await CloseLinkPanelAsync();
        }
    }

    #endregion

    #region Source view

    private async Task OnSourceToggleAsync(bool pressed)
    {
        if (pressed)
        {
            linkPanelOpen = false;
            sourceHtml = FormatSource(await GetHtmlAsync());
            sourceMode = true;
            return;
        }

        sourceMode = false;
        await SetHtmlAsync(sourceHtml);
    }

    private async Task OnSourceChangedAsync(string? value)
    {
        sourceHtml = value ?? string.Empty;
        await SetHtmlAsync(sourceHtml);
    }

    /// <summary>
    /// Puts each paragraph-level element of the HTML on its own line, so the source view is readable.
    /// </summary>
    /// <param name="html">HTML written out by the editor.</param>
    /// <returns>The same HTML with line breaks between its blocks.</returns>
    internal static string FormatSource(string html)
    {
        foreach (var tag in new[] { "p", "h1", "h2", "h3", "ul", "ol", "li", "blockquote", "pre" })
        {
            html = html.Replace($"</{tag}><", $"</{tag}>\n<", StringComparison.Ordinal);
        }

        foreach (var tag in new[] { "ul", "ol", "blockquote" })
        {
            html = html.Replace($"<{tag}><", $"<{tag}>\n<", StringComparison.Ordinal);
        }

        return html;
    }

    #endregion

    #region Callbacks from the browser

    /// <summary>
    /// Takes the content after the user changed it, called by <c>twTextEditor.attach</c>.
    /// </summary>
    /// <param name="html">The content as sanitized HTML.</param>
    /// <param name="text">The content as plain text.</param>
    /// <param name="length">The number of characters.</param>
    /// <returns>A task that completes once the change has been reported.</returns>
    [JSInvokable("NotifyInput")]
    public Task NotifyInputAsync(string? html, string? text, int length) => ReportChangeAsync(html ?? string.Empty, text ?? string.Empty, length);

    /// <summary>
    /// Takes the formats at the selection after it moved, called by <c>twTextEditor.attach</c>.
    /// </summary>
    /// <param name="state">The selection and its formats.</param>
    /// <returns>A task that completes once the toolbar shows the new formats.</returns>
    [JSInvokable("NotifyState")]
    public async Task NotifyStateAsync(TextEditorSelectionState? state)
    {
        if (state is null)
        {
            return;
        }

        var formats = new HashSet<TextEditorFormat>();
        foreach (var name in state.Formats ?? [])
        {
            if (Enum.TryParse<TextEditorFormat>(name, ignoreCase: false, out var format))
            {
                formats.Add(format);
            }
        }

        if (!formats.Overlaps(_styleFormats) && !formats.Overlaps(_blockFormats))
        {
            formats.Add(TextEditorFormat.Paragraph);
        }

        activeFormats = formats;
        linkAtSelection = state.Link;
        StateHasChanged();

        await OnSelectionChange.InvokeAsync(new TextEditorSelectionEventArgs
        {
            Index = state.Index,
            Length = state.Length,
            Formats = formats,
            LinkUrl = state.Link,
        });
    }

    /// <summary>
    /// Raises <see cref="OnFocus"/>, called by <c>twTextEditor.attach</c>.
    /// </summary>
    /// <returns>A task that completes once the callback has run.</returns>
    [JSInvokable("NotifyFocus")]
    public Task NotifyFocusAsync() => OnFocus.InvokeAsync();

    /// <summary>
    /// Raises <see cref="OnBlur"/>, called by <c>twTextEditor.attach</c>.
    /// </summary>
    /// <returns>A task that completes once the callback has run.</returns>
    [JSInvokable("NotifyBlur")]
    public Task NotifyBlurAsync() => OnBlur.InvokeAsync();

    #endregion

    private async Task ReportChangeAsync(string html, string text, int length)
    {
        var changed = !string.Equals(html, currentHtml, StringComparison.Ordinal) || !string.Equals(html, Value ?? string.Empty, StringComparison.Ordinal);
        currentHtml = html;

        if (changed)
        {
            Value = html;
            lastValue = html;
            await ValueChanged.InvokeAsync(html);
            NotifyFieldChanged();
        }

        await OnTextChange.InvokeAsync(new TextEditorChangeEventArgs { Html = html, Text = text, Length = length });
    }

    /// <inheritdoc />
    protected override FieldIdentifier? CreateFieldIdentifier() => ValueExpression is null ? null : FieldIdentifier.Create(ValueExpression);

    /// <inheritdoc />
    public override async ValueTask DisposeAsync()
    {
        try
        {
            if (attached)
            {
                await jsRuntime.InvokeVoidAsync("twTextEditor.detach", contentRef);
            }
        }
        catch (JSDisconnectedException)
        {
            // The circuit is already gone; nothing left to clean up.
        }
        catch (InvalidOperationException)
        {
            // JS interop unavailable during teardown (e.g. prerendering); safe to ignore.
        }

        dotNetRef?.Dispose();
        await base.DisposeAsync();
        GC.SuppressFinalize(this);
    }
}
