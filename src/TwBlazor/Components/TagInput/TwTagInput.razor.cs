// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using System.Globalization;
using System.Linq.Expressions;
using TwBlazor.Builders;
using TwBlazor.Configuration.Components;
using TwBlazor.Enums;
using TwBlazor.Localization;
using TwBlazor.Models;
using TwBlazor.Utilities;

namespace TwBlazor.Components;

/// <summary>
/// Represents a field for entering a list of free-form tags or keywords, each shown as a removable
/// <see cref="TwChip"/>.
/// </summary>
/// <remarks>
/// <para>
/// Typed text becomes a tag when the user presses <kbd>Enter</kbd> or types one of the <see cref="Separators"/>
/// (a comma by default). Pasted text is split on the separators and on line breaks, so a whole list can be pasted at
/// once. <kbd>Backspace</kbd> in the empty field removes the last tag, and the arrow keys move between the tags,
/// where <kbd>Delete</kbd> or <kbd>Backspace</kbd> removes the focused one.
/// </para>
/// <para>
/// Bind the list with <c>@bind-Value</c>. Inside an <see cref="EditForm"/> or a <see cref="TwForm"/> the field
/// reports every change, so validation attributes on the bound property, such as <c>[MinLength]</c>, run for it.
/// A tag that cannot be added (see <see cref="TagRejectionReason"/>) stays in the text input, and the reason is shown
/// under the field.
/// </para>
/// </remarks>
public partial class TwTagInput : TwPopoverPickerComponentBase
{
    private static readonly char[] _lineBreaks = ['\r', '\n', '\t'];

    [Inject] private InputVariantBuilder inputVariantBuilder { get; set; } = null!;

    private TwTagInputTheme theme => options.Theme.Components.Require<TwTagInputTheme>();

    private TwInputTheme inputTheme => options.Theme.Components.Require<TwInputTheme>();

    /// <summary>
    /// Gets or sets the tags in the field.
    /// </summary>
    [Parameter] public IReadOnlyList<string> Value { get; set; } = [];

    /// <summary>
    /// Gets or sets the callback that is invoked with the new list whenever a tag is added or removed.
    /// </summary>
    [Parameter] public EventCallback<IReadOnlyList<string>> ValueChanged { get; set; }

    /// <summary>
    /// Gets or sets the expression that identifies the bound value. Set automatically by <c>@bind-Value</c>, it
    /// lets an enclosing <see cref="EditForm"/> or <see cref="TwForm"/> validate the property the field edits.
    /// </summary>
    [Parameter] public Expression<Func<IReadOnlyList<string>>>? ValueExpression { get; set; }

    /// <summary>
    /// Gets or sets the placeholder text shown while the field has no tags.
    /// </summary>
    [Parameter] public string Placeholder { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the text shown under the field, such as a hint on how to add tags. It is read out with the field.
    /// </summary>
    [Parameter] public string HelperText { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets whether at least one tag is required. The field is announced as required by assistive technology.
    /// </summary>
    /// <remarks>
    /// Add a validation attribute such as <c>[MinLength(1)]</c> to the bound property for the form to enforce it.
    /// </remarks>
    [Parameter] public bool Required { get; set; }

    /// <summary>
    /// Gets or sets the characters that turn the typed text into a tag when typed, in addition to <kbd>Enter</kbd>.
    /// Pasted text is split on them as well as on line breaks. Defaults to a comma.
    /// </summary>
    /// <remarks>
    /// Use an empty list to add tags with <kbd>Enter</kbd> only, so a tag can contain commas. Adding a space lets the
    /// user add a tag per word.
    /// </remarks>
    [Parameter] public IReadOnlyCollection<char> Separators { get; set; } = [','];

    /// <summary>
    /// Gets or sets whether the typed text becomes a tag when focus leaves the field.
    /// </summary>
    [Parameter] public bool AddOnBlur { get; set; }

    /// <summary>
    /// Gets or sets the most tags the field holds, or <see langword="null"/> (the default) for no limit.
    /// </summary>
    [Parameter] public int? MaxTags { get; set; }

    /// <summary>
    /// Gets or sets the most characters a tag can have, or <see langword="null"/> (the default) for no limit.
    /// </summary>
    [Parameter] public int? MaxTagLength { get; set; }

    /// <summary>
    /// Gets or sets whether the same tag can be added more than once. Tags are compared ignoring case.
    /// </summary>
    [Parameter] public bool AllowDuplicates { get; set; }

    /// <summary>
    /// Gets or sets a function that checks each tag before it is added. Return <see langword="null"/> or an empty
    /// string to accept the tag, or a message to reject it. The message is shown under the field.
    /// </summary>
    [Parameter] public Func<string, string?>? TagValidator { get; set; }

    /// <summary>
    /// Gets or sets the tags to suggest as the user types. Ignored when <see cref="SearchFunc"/> is set.
    /// </summary>
    /// <remarks>
    /// Suggestions are matched ignoring case and accents, and tags already in the field are left out unless
    /// <see cref="AllowDuplicates"/> is set. The user can still add any other text.
    /// </remarks>
    [Parameter] public IEnumerable<string> Suggestions { get; set; } = [];

    /// <summary>
    /// Gets or sets a function that returns the suggestions for the typed text, for searching on a server or a large
    /// data set. When set, <see cref="Suggestions"/> is not used.
    /// </summary>
    /// <remarks>
    /// The token is cancelled when the user types again before the search completes, so a slow, stale search never
    /// replaces newer results.
    /// </remarks>
    [Parameter] public Func<string, CancellationToken, Task<IEnumerable<string>>>? SearchFunc { get; set; }

    /// <summary>
    /// Gets or sets how long, in milliseconds, to wait after the last keystroke before calling
    /// <see cref="SearchFunc"/>. Defaults to 0, which searches on every keystroke.
    /// </summary>
    [Parameter] public int DebounceInterval { get; set; }

    /// <summary>
    /// Gets or sets how many characters must be typed before suggestions are shown. Defaults to 1. Arrow Down
    /// opens the suggestions whatever has been typed.
    /// </summary>
    [Parameter] public int MinCharacters { get; set; } = 1;

    /// <summary>
    /// Gets or sets the most suggestions to list at once, or <see langword="null"/> to list every match.
    /// </summary>
    [Parameter] public int? MaxSuggestions { get; set; }

    /// <summary>
    /// Gets or sets whether a button that removes every tag is shown at the end of the field. It is disabled while
    /// the field is empty.
    /// </summary>
    /// <remarks>
    /// The button is not a Tab stop, since keyboard users can remove tags with <kbd>Backspace</kbd>, and it is
    /// hidden while the field is disabled or read-only.
    /// </remarks>
    [Parameter] public bool Clearable { get; set; }

    /// <summary>
    /// Gets or sets the callback invoked after the clear button (see <see cref="Clearable"/>) has removed every tag.
    /// </summary>
    [Parameter] public EventCallback OnClear { get; set; }

    /// <summary>
    /// Gets or sets the callback invoked with each tag the user adds.
    /// </summary>
    [Parameter] public EventCallback<string> OnTagAdded { get; set; }

    /// <summary>
    /// Gets or sets the callback invoked with each tag the user removes.
    /// </summary>
    [Parameter] public EventCallback<string> OnTagRemoved { get; set; }

    /// <summary>
    /// Gets or sets the callback invoked when a tag cannot be added: a duplicate, one too many, one too long, or one
    /// <see cref="TagValidator"/> rejected.
    /// </summary>
    [Parameter] public EventCallback<TagRejectedEventArgs> OnTagRejected { get; set; }

    /// <summary>
    /// Gets or sets the color of the tags. Defaults to <see cref="Color.Primary"/>.
    /// </summary>
    [Parameter] public Color ChipColor { get; set; } = Color.Primary;

    /// <summary>
    /// Gets or sets the variant of the tags. Defaults to <see cref="ButtonVariant.Outlined"/>, the same as the
    /// chips of a multiple <see cref="TwSelect{T}"/>.
    /// </summary>
    [Parameter] public ButtonVariant ChipVariant { get; set; } = ButtonVariant.Outlined;

    private ElementReference inputRef;

    private DotNetObjectReference<TwTagInput>? tagInputDotNetRef;

    private List<string> tags = [];

    private IReadOnlyList<string>? lastValue;

    private string text = string.Empty;

    private Dictionary<int, string> shownSuggestions = [];

    private bool isLoading;

    private CancellationTokenSource? searchCancellation;

    /// <summary>
    /// The highlight to apply once the next render has drawn the suggestions: <c>first</c>, <c>last</c>, or
    /// <c>reset</c> to clear it because the suggestions changed.
    /// </summary>
    private string? pendingHighlight;

    /// <summary>
    /// Set while the list is open, so the render that closes it also clears the <c>aria-activedescendant</c> that the
    /// script set on the field.
    /// </summary>
    private bool activeDescendantSet;

    /// <summary>
    /// What to focus once the next render is done: a tag's index, <see cref="focusInput"/> for the text input, or
    /// <see langword="null"/> to leave focus alone.
    /// </summary>
    private int? pendingFocus;

    private const int focusInput = -1;

    /// <summary>
    /// The separators handed to the script, so it is only updated when they change.
    /// </summary>
    private string? attachedSeparators;

    /// <summary>
    /// The reason the last tag could not be added, shown under the field until the text changes.
    /// </summary>
    private string? rejectionMessage;

    private string statusMessage = string.Empty;

    private bool canEdit => !Disabled && !ReadOnly;

    private bool hasSuggestions => SearchFunc is not null || Suggestions.Any();

    private bool isListboxShown => isFocused && !isLoading && shownSuggestions.Count > 0;

    // Only a combobox reports whether its list is open.
    private string? ariaExpanded
    {
        get
        {
            if (!hasSuggestions)
            {
                return null;
            }

            return isListboxShown ? "true" : "false";
        }
    }

    private bool showClearButton => Clearable && canEdit;

    private string listboxId => $"{Id}-listbox";

    private string helperId => $"{Id}-helper";

    private string OptionId(int key) => $"{Id}-option-{key}";

    private string TagId(int index) => $"{Id}-tag-{index}";

    private ChipSize chipSize => Dense ? ChipSize.Medium : ChipSize.Large;

    private string tagListLabel => !string.IsNullOrWhiteSpace(Label) ? Label : AriaLabel ?? localizer[TwStrings.TagInput_Tags];

    private CultureInfo culture => localizer.Formats.Culture;

    private string separatorText => new([.. Separators ?? []]);

    private bool shownInvalid => rejectionMessage is not null || fieldInvalid;

    private string shownErrorMessage => rejectionMessage ?? fieldErrorMessage;

    private string? shownErrorId => shownInvalid && !string.IsNullOrWhiteSpace(shownErrorMessage) ? $"{Id}-error" : null;

    private string? describedBy
    {
        get
        {
            var ids = new List<string>(2);
            if (!string.IsNullOrWhiteSpace(HelperText))
            {
                ids.Add(helperId);
            }

            if (shownErrorId is not null)
            {
                ids.Add(shownErrorId);
            }

            return ids.Count == 0 ? null : string.Join(' ', ids);
        }
    }

    /// <summary>
    /// Gets the classes for the field: the input variant's border, background and focus ring (shown while the text
    /// input or a tag inside it has focus), around a wrapping row of tags.
    /// </summary>
    private string fieldClasses => new ClassBuilder(theme.Field)
        .AddClass(Dense ? theme.DenseSize : theme.Size)
        .AddClass(inputVariantBuilder.GetClasses(effectiveVariant, inputTheme, InputFocusStyle.FocusWithin, effectiveRounded))
        .AddClass(theme.ChipsPadding, tags.Count > 0 && effectiveVariant != InputVariant.Default)
        .AddClass(inputTheme.ClearablePadding, showClearButton)
        .AddClass($"{options.Theme.Interaction.DisabledOpacity} {options.Theme.Interaction.DisabledCursor}", Disabled)
        .AddClass(Class)
        .Build();

    // Sits inside the field, so it is one step tighter than the field's own corners.
    private string clearButtonClasses => new ClassBuilder(inputTheme.ClearButton)
        .AddClass(theme.ClearButton)
        .AddClass(roundedBuilder.GetRounded(effectiveInsetRounded))
        .Build();

    private string panelSurfaceClasses => popoverBuilder.GetSurfaceClasses(Rounded, Shadow, inputTheme.SelectPanelSurface);

    /// <inheritdoc />
    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        if (string.IsNullOrEmpty(RootId))
        {
            RootId = Guid.NewGuid().ToString("N");
        }

        // A field with no "name" gives browsers nothing to key autofill on; default to Id unless the consumer
        // supplied one, the same as TwTextfield and TwAutocomplete.
        if (!Attributes.ContainsKey("name") && !string.IsNullOrEmpty(Id))
        {
            Attributes["name"] = Id;
        }

        // Only a list the parent actually replaced resets the tags, so a parent re-render never undoes a change the
        // parent has not caught up with yet.
        if (!ReferenceEquals(Value, lastValue))
        {
            lastValue = Value;
            tags = [.. Value ?? []];
        }

        if (!canEdit && isFocused)
        {
            shownSuggestions = [];
        }
        else if (isFocused && SearchFunc is null)
        {
            shownSuggestions = FilterSuggestions(text);
        }
    }

    /// <inheritdoc />
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);

        if (firstRender)
        {
            tagInputDotNetRef = DotNetObjectReference.Create(this);

            // The suggestion keys are attached first, so Enter on a highlighted suggestion is handled (and its
            // default prevented) before the tag input's own Enter handler sees it.
            await JSRuntime.InvokeVoidAsync("twAutocomplete.attach", inputRef, tagInputDotNetRef);
            attachedSeparators = separatorText;
            await JSRuntime.InvokeVoidAsync("twTagInput.attach", inputRef, tagInputDotNetRef, attachedSeparators);
        }
        else if (attachedSeparators != separatorText)
        {
            attachedSeparators = separatorText;
            await JSRuntime.InvokeVoidAsync("twTagInput.update", inputRef, attachedSeparators);
        }

        if (isListboxShown && PanelRef.Context != null)
        {
            // matchAnchorWidth: the suggestions span the field's width, as TwAutocomplete's do.
            await RegisterPanelScrollBehaviorAsync(PanelRef, matchAnchorWidth: true);
        }

        if (isListboxShown || activeDescendantSet)
        {
            activeDescendantSet = isListboxShown;
            var highlight = pendingHighlight;
            pendingHighlight = null;
            await JSRuntime.InvokeVoidAsync("twAutocomplete.refresh", inputRef, highlight);
        }

        if (pendingFocus is int target)
        {
            pendingFocus = null;
            await FocusTargetAsync(target);
        }
    }

    /// <summary>
    /// Moves focus to the text input.
    /// </summary>
    /// <returns>A task that completes once focus has moved.</returns>
    public async Task FocusAsync() => await FocusTargetAsync(focusInput);

    /// <summary>
    /// Turns the text in the field into tags, called by <c>twTagInput.attach</c> when the user presses Enter or types
    /// one of the <see cref="Separators"/>. Text that cannot be added stays in the field.
    /// </summary>
    /// <param name="value">The text in the field.</param>
    /// <returns>A task that completes once the tags have been added.</returns>
    [JSInvokable("CommitText")]
    public async Task CommitTextAsync(string? value)
    {
        if (!canEdit)
            return;

        await AddFromTextAsync(value ?? text, commitLast: true);
        await CloseSuggestionsAsync();
        await InvokeAsync(StateHasChanged);
    }

    /// <summary>
    /// Adds a tag for each part of pasted text, split on the <see cref="Separators"/> and line breaks. Called by
    /// <c>twTagInput.attach</c> when the pasted text holds more than one tag.
    /// </summary>
    /// <param name="value">The field's text with the pasted text in place.</param>
    /// <returns>A task that completes once the tags have been added.</returns>
    [JSInvokable("PasteText")]
    public async Task PasteTextAsync(string? value)
    {
        if (!canEdit || value is null)
            return;

        await AddFromTextAsync(value, commitLast: true);
        await CloseSuggestionsAsync();
        await InvokeAsync(StateHasChanged);
    }

    /// <summary>
    /// Opens the suggestions from the keyboard (Arrow Down or Arrow Up in the field, called by
    /// <c>twAutocomplete.attach</c>) and highlights the first or last one.
    /// </summary>
    /// <param name="highlight"><c>first</c> or <c>last</c>, or <see langword="null"/> to open without a highlight.</param>
    /// <returns>A task that completes once the suggestions are shown.</returns>
    [JSInvokable("OpenFromKeyboard")]
    public async Task OpenFromKeyboardAsync(string? highlight)
    {
        if (!canEdit || !hasSuggestions)
            return;

        await RefreshSuggestionsAsync(text, force: true);
        pendingHighlight = highlight;
        await InvokeAsync(StateHasChanged);
    }

    /// <summary>
    /// Adds a suggestion as a tag, by click or by Enter on the highlighted one (see <c>twAutocomplete.attach</c>),
    /// and empties the text. Focus stays in the field.
    /// </summary>
    /// <param name="key">The suggestion's key in the list shown.</param>
    /// <returns>A task that completes once the tag is added.</returns>
    [JSInvokable("SelectOption")]
    public async Task SelectOptionAsync(int key)
    {
        if (!canEdit || !isFocused || !shownSuggestions.TryGetValue(key, out var suggestion))
            return;

        rejectionMessage = null;

        var added = new List<string>();
        var outcome = await TryAddAsync(suggestion, added);
        if (outcome == AddOutcome.Added)
        {
            text = string.Empty;
            await CommitAddedAsync(added);
        }

        await CloseSuggestionsAsync();
        pendingFocus = focusInput;
        await InvokeAsync(StateHasChanged);
    }

    /// <summary>
    /// Removes every tag and the typed text, then moves focus back to the field.
    /// </summary>
    /// <returns>A task that completes once the field has been cleared.</returns>
    public async Task ClearAsync()
    {
        if (!canEdit)
            return;

        var hadTags = tags.Count > 0;
        text = string.Empty;
        rejectionMessage = null;
        await CloseSuggestionsAsync();

        if (hadTags)
        {
            var removed = tags;
            await SetTagsAsync([]);
            statusMessage = localizer[TwStrings.TagInput_Cleared];

            if (OnTagRemoved.HasDelegate)
            {
                foreach (var tag in removed)
                {
                    await OnTagRemoved.InvokeAsync(tag);
                }
            }
        }

        if (OnClear.HasDelegate)
        {
            await OnClear.InvokeAsync();
        }

        pendingFocus = focusInput;
    }

    /// <inheritdoc />
    /// <remarks>
    /// Re-declares <see cref="JSInvokableAttribute"/>: JS interop resolves invokable methods on the runtime type, and
    /// an override does not inherit the attribute from the base method. Focus is never moved, as it never left the
    /// field while the suggestions were shown.
    /// </remarks>
    [JSInvokable("Close")]
    public override async Task Close()
    {
        CancelSearch();
        isLoading = false;
        FocusReturnToken = null;

        await base.Close();
    }

    /// <inheritdoc />
    public override async ValueTask DisposeAsync()
    {
        CancelSearch();

        try
        {
            await JSRuntime.InvokeVoidAsync("twTagInput.detach", inputRef);
            await JSRuntime.InvokeVoidAsync("twAutocomplete.detach", inputRef);
        }
        catch (JSDisconnectedException)
        {
            // The circuit is gone, and the listeners with it.
        }
        catch (InvalidOperationException)
        {
            // JS interop is unavailable during teardown (e.g. prerendering).
        }

        tagInputDotNetRef?.Dispose();
        tagInputDotNetRef = null;

        await base.DisposeAsync();

        GC.SuppressFinalize(this);
    }

    /// <inheritdoc />
    protected override FieldIdentifier? CreateFieldIdentifier() => ValueExpression is null ? null : FieldIdentifier.Create(ValueExpression);

    /// <summary>
    /// Keeps the typed text, turns any complete parts (text before a separator that arrived without its key press,
    /// such as from a phone keyboard or an IME) into tags, and updates the suggestions.
    /// </summary>
    private async Task OnInputAsync(ChangeEventArgs e)
    {
        if (!canEdit)
            return;

        var value = e.Value?.ToString() ?? string.Empty;
        rejectionMessage = null;

        if (value.IndexOfAny([.. Separators ?? []]) >= 0)
        {
            await AddFromTextAsync(value, commitLast: false);
        }
        else
        {
            text = value;
        }

        pendingHighlight = "reset";
        await RefreshSuggestionsAsync(text, force: false);
    }

    /// <summary>
    /// Handles the keys the text input acts on itself: Backspace in the empty input removes the last tag, the
    /// backwards arrow in the empty input moves to the last tag, and Escape closes the suggestions. Enter and the
    /// separators are handled by <c>twTagInput.attach</c>, which also stops them reaching the browser.
    /// </summary>
    private async Task OnInputKeyDownAsync(KeyboardEventArgs e)
    {
        if (!canEdit)
            return;

        switch (ToLogicalKey(e.Key))
        {
            case "Backspace" when text.Length == 0 && tags.Count > 0 && !e.Repeat:
                await RemoveAtAsync(tags.Count - 1);
                break;

            case "ArrowLeft" when text.Length == 0 && tags.Count > 0:
                await CloseSuggestionsAsync();
                pendingFocus = tags.Count - 1;
                break;

            case "Escape" when isFocused:
                await CloseSuggestionsAsync();
                break;
        }
    }

    /// <summary>
    /// Closes the suggestions when focus leaves the field, and adds the typed text when <see cref="AddOnBlur"/> is set.
    /// </summary>
    private async Task OnInputBlurAsync()
    {
        await CloseSuggestionsAsync();

        if (AddOnBlur && canEdit && !string.IsNullOrWhiteSpace(text))
        {
            await AddFromTextAsync(text, commitLast: true);
        }
    }

    /// <summary>
    /// Moves focus to the text input when the empty part of the field is clicked, as a text field would take it.
    /// </summary>
    private void OnFieldClick(MouseEventArgs e)
    {
        if (!Disabled)
        {
            pendingFocus = focusInput;
        }
    }

    private EventCallback<KeyboardEventArgs> TagKeyDownHandler(int index) =>
        EventCallback.Factory.Create<KeyboardEventArgs>(this, e => OnTagKeyDownAsync(e, index));

    /// <summary>
    /// Handles the keys on a focused tag: the arrows move to the tag before or after it (or on to the text input
    /// after the last), Home and End move to the first tag and to the text input, and Delete or Backspace removes it.
    /// </summary>
    private async Task OnTagKeyDownAsync(KeyboardEventArgs e, int index)
    {
        if (!canEdit || index >= tags.Count)
            return;

        switch (ToLogicalKey(e.Key))
        {
            case "ArrowLeft" when index > 0:
                pendingFocus = index - 1;
                break;

            case "ArrowRight":
                pendingFocus = index < tags.Count - 1 ? index + 1 : focusInput;
                break;

            case "Home":
                pendingFocus = 0;
                break;

            case "End":
                pendingFocus = focusInput;
                break;

            case "Backspace":
                await RemoveAtAsync(index);
                // Backspace works backwards, so focus goes to the tag before, or failing that the one now first.
                if (index > 0)
                {
                    pendingFocus = index - 1;
                }
                else
                {
                    pendingFocus = tags.Count > 0 ? 0 : focusInput;
                }
                break;

            case "Delete":
                await RemoveAtAsync(index);
                pendingFocus = index < tags.Count ? index : focusInput;
                break;
        }
    }

    /// <summary>
    /// Removes a tag from its close button, then moves focus to the text input, since the button is gone.
    /// </summary>
    private async Task RemoveFromButtonAsync(int index)
    {
        await RemoveAtAsync(index);
        pendingFocus = focusInput;
    }

    private async Task RemoveAtAsync(int index)
    {
        if (!canEdit || index < 0 || index >= tags.Count)
            return;

        var tag = tags[index];
        var next = new List<string>(tags);
        next.RemoveAt(index);
        rejectionMessage = null;

        await SetTagsAsync(next);
        statusMessage = localizer[TwStrings.TagInput_Removed, tag];

        if (OnTagRemoved.HasDelegate)
        {
            await OnTagRemoved.InvokeAsync(tag);
        }
    }

    /// <summary>
    /// Splits <paramref name="value"/> into tags on the <see cref="Separators"/> and line breaks and adds them.
    /// </summary>
    /// <param name="value">The text to split.</param>
    /// <param name="commitLast">
    /// Whether the part after the last separator is added too. When <see langword="false"/> it is still being typed,
    /// so it stays in the field.
    /// </param>
    private async Task AddFromTextAsync(string value, bool commitLast)
    {
        rejectionMessage = null;

        var parts = value.Split([.. Separators ?? [], .. _lineBreaks]);
        var count = commitLast ? parts.Length : parts.Length - 1;
        var added = new List<string>();
        string? rejected = null;

        for (var i = 0; i < count; i++)
        {
            if (await TryAddAsync(parts[i], added) == AddOutcome.Rejected)
            {
                rejected ??= parts[i].Trim();
            }
        }

        // Text that could not be added stays in the field, so the user can correct it.
        text = commitLast ? rejected ?? string.Empty : parts[^1];
        if (!commitLast && rejected is not null && string.IsNullOrWhiteSpace(text))
        {
            text = rejected;
        }

        if (added.Count > 0)
        {
            await CommitAddedAsync(added);
        }
    }

    private enum AddOutcome
    {
        Added,
        Ignored,
        Rejected
    }

    /// <summary>
    /// Checks one tag and, when it passes, appends it to <paramref name="added"/>. The tags are only committed once
    /// every part has been checked (<see cref="CommitAddedAsync"/>), so a paste reports one change.
    /// </summary>
    private async Task<AddOutcome> TryAddAsync(string raw, List<string> added)
    {
        var tag = raw.Trim();
        if (tag.Length == 0)
            return AddOutcome.Ignored;

        var total = tags.Count + added.Count;
        var comparer = StringComparer.Create(culture, ignoreCase: true);

        (TagRejectionReason Reason, string Message)? rejection = null;
        if (MaxTags is int max && total >= max)
        {
            rejection = (TagRejectionReason.MaxTags, localizer.Plural(max, TwStrings.TagInput_LimitOne, TwStrings.TagInput_LimitOther, max));
        }
        else if (MaxTagLength is int maxLength && tag.Length > maxLength)
        {
            rejection = (TagRejectionReason.MaxLength, localizer.Plural(maxLength, TwStrings.TagInput_TooLongOne, TwStrings.TagInput_TooLongOther, maxLength));
        }
        else if (!AllowDuplicates && (tags.Contains(tag, comparer) || added.Contains(tag, comparer)))
        {
            rejection = (TagRejectionReason.Duplicate, localizer[TwStrings.TagInput_Duplicate, tag]);
        }
        else if (TagValidator?.Invoke(tag) is { Length: > 0 } error)
        {
            rejection = (TagRejectionReason.Invalid, error);
        }

        if (rejection is { } r)
        {
            rejectionMessage = r.Message;
            if (OnTagRejected.HasDelegate)
            {
                await OnTagRejected.InvokeAsync(new TagRejectedEventArgs { Tag = tag, Reason = r.Reason, Message = r.Message });
            }

            return AddOutcome.Rejected;
        }

        added.Add(tag);
        return AddOutcome.Added;
    }

    private async Task CommitAddedAsync(List<string> added)
    {
        await SetTagsAsync([.. tags, .. added]);
        statusMessage = string.Join(" ", added.Select(tag => localizer[TwStrings.TagInput_Added, tag]));

        if (OnTagAdded.HasDelegate)
        {
            foreach (var tag in added)
            {
                await OnTagAdded.InvokeAsync(tag);
            }
        }
    }

    private async Task SetTagsAsync(List<string> next)
    {
        tags = next;
        IReadOnlyList<string> value = next.ToArray();
        Value = value;
        lastValue = value;

        if (ValueChanged.HasDelegate)
        {
            await ValueChanged.InvokeAsync(value);
        }

        NotifyFieldChanged();
    }

    /// <summary>
    /// Shows the suggestions for <paramref name="query"/>, or closes them when there are none.
    /// </summary>
    /// <param name="query">The typed text.</param>
    /// <param name="force">Whether to show suggestions even before <see cref="MinCharacters"/> have been typed.</param>
    private async Task RefreshSuggestionsAsync(string query, bool force)
    {
        query = query.Trim();
        if (!canEdit || !hasSuggestions || (!force && query.Length < Math.Max(MinCharacters, 0)))
        {
            await CloseSuggestionsAsync();
            return;
        }

        if (!isFocused)
        {
            isFocused = true;
            activeDescendantSet = true;
            await RegisterOutsideClickAsync();
        }

        if (SearchFunc is null)
        {
            shownSuggestions = FilterSuggestions(query);
            if (shownSuggestions.Count == 0)
            {
                await CloseSuggestionsAsync();
            }

            return;
        }

        CancelSearch();
        var cancellation = new CancellationTokenSource();
        searchCancellation = cancellation;
        var token = cancellation.Token;
        isLoading = true;

        try
        {
            if (DebounceInterval > 0)
            {
                StateHasChanged();
                await Task.Delay(DebounceInterval, token);
            }

            var results = await SearchFunc(query, token);
            if (token.IsCancellationRequested)
                return;

            shownSuggestions = ToSuggestions(results);
            isLoading = false;
            pendingHighlight = "reset";

            if (shownSuggestions.Count == 0)
            {
                await CloseSuggestionsAsync();
            }
        }
        catch (OperationCanceledException)
        {
            // A newer keystroke, or closing the list, replaced this search.
        }
    }

    private async Task CloseSuggestionsAsync()
    {
        if (isFocused)
        {
            await Close();
        }

        shownSuggestions = [];
    }

    private void CancelSearch()
    {
        searchCancellation?.Cancel();
        searchCancellation?.Dispose();
        searchCancellation = null;
    }

    /// <summary>
    /// Gets the suggestions whose text contains <paramref name="query"/>, ignoring case and accents, leaving out the
    /// tags already added. Each keeps its position in <see cref="Suggestions"/> as its key.
    /// </summary>
    private Dictionary<int, string> FilterSuggestions(string query)
    {
        var matches = new Dictionary<int, string>();
        var compareInfo = culture.CompareInfo;
        const CompareOptions compareOptions = CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace;
        var key = 1;

        foreach (var suggestion in Suggestions)
        {
            if (MaxSuggestions is int max && matches.Count >= max)
                break;

            if (!string.IsNullOrWhiteSpace(suggestion)
                && !IsTaken(suggestion)
                && (query.Length == 0 || compareInfo.IndexOf(suggestion, query, compareOptions) >= 0))
            {
                matches.Add(key, suggestion);
            }

            key++;
        }

        return matches;
    }

    private Dictionary<int, string> ToSuggestions(IEnumerable<string> results)
    {
        var available = results.Where(s => !string.IsNullOrWhiteSpace(s) && !IsTaken(s));
        var limited = MaxSuggestions is int max ? available.Take(max) : available;
        var key = 1;
        return limited.ToDictionary(_ => key++);
    }

    private bool IsTaken(string suggestion) =>
        !AllowDuplicates && tags.Contains(suggestion.Trim(), StringComparer.Create(culture, ignoreCase: true));

    private async Task FocusTargetAsync(int target)
    {
        try
        {
            if (target == focusInput)
            {
                await inputRef.FocusAsync();
            }
            else
            {
                await JSRuntime.InvokeVoidAsync("twFocus.focusById", TagId(target));
            }
        }
        catch (InvalidOperationException)
        {
            // The element is not rendered yet (prerendering); there is nothing to focus.
        }
        catch (JSDisconnectedException)
        {
            // The circuit disconnected; there is no focus left to move.
        }
    }
}
