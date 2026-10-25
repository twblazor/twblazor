// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using System.Globalization;
using Microsoft.AspNetCore.Components;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using TwBlazor.Builders;
using TwBlazor.Configuration.Components;
using TwBlazor.Enums;
using TwBlazor.Localization;
using TwBlazor.Utilities;

namespace TwBlazor.Components;

/// <summary>
/// Represents a text field that suggests matching options as the user types, and binds the option they pick.
/// </summary>
/// <typeparam name="T">The type of the options.</typeparam>
/// <remarks>
/// The field follows the WAI-ARIA combobox pattern with list autocomplete: focus stays in the text field while
/// the arrow keys move through the suggestions, which are exposed through <c>aria-activedescendant</c>. It shares
/// its look with <see cref="TwSelect{T}"/>, so its listbox is styled by the same <see cref="TwInputTheme"/> values.
/// Options come from <see cref="Values"/>, filtered on the client by <see cref="FilterMode"/>, or from
/// <see cref="SearchFunc"/> for server side search.
/// </remarks>
public partial class TwAutocomplete<T> : TwPopoverPickerComponentBase
{
    [Inject] private InputVariantBuilder inputVariantBuilder { get; set; } = null!;

    private TwInputTheme theme => options.Theme.Components.Require<TwInputTheme>();

    /// <summary>
    /// Gets or sets the options to suggest. Ignored when <see cref="SearchFunc"/> is set.
    /// </summary>
    [Parameter] public IEnumerable<T> Values { get; set; } = [];

    /// <summary>
    /// Gets or sets the selected option.
    /// </summary>
    [Parameter] public T SelectedValue { get; set; } = default!;

    /// <summary>
    /// Gets or sets the callback that is invoked when the selected option changes. Clearing the field passes
    /// <see langword="default"/>, such as <see langword="null"/> for a reference type.
    /// </summary>
    [Parameter] public EventCallback<T> SelectedValueChanged { get; set; }

    /// <summary>
    /// Gets or sets the expression that identifies the bound value. Set automatically by <c>@bind-SelectedValue</c>, it
    /// lets an enclosing <see cref="EditForm"/> or <see cref="TwForm"/> validate the property the field edits.
    /// </summary>
    [Parameter] public Expression<Func<T>>? SelectedValueExpression { get; set; }

    /// <summary>
    /// Gets or sets the name of the property to display for complex objects.
    /// </summary>
    /// <remarks>
    /// If null or empty, the option's <see cref="object.ToString"/> is used. The displayed text is also the text
    /// that the typed text is matched against.
    /// </remarks>
    [Parameter] public string PropertyName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the placeholder text shown while the field is empty.
    /// </summary>
    [Parameter] public string Placeholder { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets whether a value is required. A required field cannot be cleared by emptying its text.
    /// </summary>
    [Parameter] public bool Required { get; set; }

    /// <summary>
    /// Gets or sets whether a clear button is shown at the end of the field.
    /// </summary>
    [Parameter] public bool Clearable { get; set; }

    /// <summary>
    /// Gets or sets how the typed text is matched against each of <see cref="Values"/>. Defaults to
    /// <see cref="AutocompleteFilterMode.Contains"/>. Matching ignores case and accents.
    /// </summary>
    [Parameter] public AutocompleteFilterMode FilterMode { get; set; } = AutocompleteFilterMode.Contains;

    /// <summary>
    /// Gets or sets a function that returns the options for the typed text, for searching on a server or a large
    /// data set. When set, <see cref="Values"/> and <see cref="FilterMode"/> are not used.
    /// </summary>
    /// <remarks>
    /// The token is cancelled when the user types again before the search completes, so a slow, stale search never
    /// replaces newer results.
    /// </remarks>
    [Parameter] public Func<string, CancellationToken, Task<IEnumerable<T>>>? SearchFunc { get; set; }

    /// <summary>
    /// Gets or sets how long, in milliseconds, to wait after the last keystroke before calling
    /// <see cref="SearchFunc"/>. Defaults to 0, which searches on every keystroke.
    /// </summary>
    [Parameter] public int DebounceInterval { get; set; }

    /// <summary>
    /// Gets or sets how many characters must be typed before suggestions are shown. Defaults to 0, so every option
    /// is listed as soon as the list opens.
    /// </summary>
    [Parameter] public int MinCharacters { get; set; }

    /// <summary>
    /// Gets or sets the most suggestions to list at once, or <see langword="null"/> to list every match.
    /// </summary>
    [Parameter] public int? MaxItems { get; set; }

    /// <summary>
    /// Gets or sets the content rendered for each suggestion. Defaults to the option's display text.
    /// </summary>
    /// <remarks>
    /// Unlike <see cref="TwSelect{T}"/>, the autocomplete never switches to the browser's native picker, on mobile or
    /// anywhere else (<see cref="TwPopoverPickerComponentBase.PreferNativePicker"/> has no effect), so the template is
    /// always used in the list. The text field itself can only hold text, so it always shows the selected option's
    /// display text from <see cref="PropertyName"/> or <see cref="object.ToString"/>, and the typed text is matched
    /// against that display text, not against what the template renders.
    /// </remarks>
    [Parameter] public RenderFragment<T>? ItemTemplate { get; set; }

    /// <summary>
    /// Gets or sets the text shown when nothing matches the typed text. Defaults to the localized
    /// <see cref="TwStrings.Common_NoResults"/> text ("No results").
    /// </summary>
    [Parameter] public string? NoResultsText { get; set; }

    private ElementReference inputRef;

    private DotNetObjectReference<TwAutocomplete<T>>? autocompleteDotNetRef;

    private string text = string.Empty;

    private Dictionary<int, T> shownOptions = [];

    private bool hasSelection;

    private bool isLoading;

    private bool parametersInitialized;

    private T lastSelectedValue = default!;

    private IEnumerable<T>? lastValues;

    private CancellationTokenSource? searchCancellation;

    /// <summary>
    /// The highlight to apply once the next render has drawn the options: <c>first</c>, <c>last</c>, or <c>reset</c>
    /// to clear it because the options changed.
    /// </summary>
    private string? pendingHighlight;

    /// <summary>
    /// Set while the panel is open, so the render that closes it also clears the <c>aria-activedescendant</c> that the
    /// script set on the field.
    /// </summary>
    private bool activeDescendantSet;

    private string listboxId => $"{Id}-listbox";

    private string OptionId(int key) => $"{Id}-option-{key}";

    private bool isListboxShown => isFocused && !isLoading && shownOptions.Count > 0;

    private bool showClearButton => Clearable && !Disabled && !ReadOnly;

    private string effectiveNoResultsText => NoResultsText ?? localizer[TwStrings.Common_NoResults];

    private string listboxLabel => string.IsNullOrWhiteSpace(Label) ? AriaLabel ?? localizer[TwStrings.Select_Options] : Label;

    private string selectedDisplayText => hasSelection ? GetDisplayText(SelectedValue) : string.Empty;

    private CultureInfo culture => localizer.Formats.Culture;

    /// <summary>
    /// Gets the message announced to screen readers while the list is open: how many suggestions there are, or that
    /// there are none.
    /// </summary>
    private string statusMessage
    {
        get
        {
            if (!isFocused || isLoading)
            {
                return string.Empty;
            }

            return shownOptions.Count == 0
                ? effectiveNoResultsText
                : localizer.Plural(shownOptions.Count, TwStrings.Autocomplete_ResultsOne, TwStrings.Autocomplete_ResultsOther, shownOptions.Count);
        }
    }

    private string inputClasses => new ClassBuilder(theme.TextfieldBase)
        .AddClass(inputVariantBuilder.GetClasses(effectiveVariant, theme, effectiveRounded))
        .AddClass(Disabled ? $"{options.Theme.Interaction.DisabledOpacity} {options.Theme.Interaction.DisabledCursor}" : string.Empty)
        .AddClass(inputSizeClasses)
        .AddClass(Class)
        .Build();

    /// <summary>
    /// Gets the end padding that keeps the typed text clear of the buttons drawn over the end of the field.
    /// </summary>
    /// <remarks>
    /// Each button is 2rem wide with a 0.25rem gap (see <see cref="TwInputTheme.ClearButton"/> and
    /// <see cref="TwInputTheme.FieldEndContent"/>). One button gives 2.5rem, the same as
    /// <see cref="TwInputTheme.ClearablePadding"/>.
    /// </remarks>
    private string inputStyle => string.Create(CultureInfo.InvariantCulture, $"padding-inline-end: calc({(showClearButton ? 2 : 1)} * 2.25rem + 0.25rem);");

    private string endContentClasses => new ClassBuilder(theme.FieldEndContent).AddClass(inputSizeClasses).Build();

    private string chevronClasses => new ClassBuilder(theme.SelectCustomChevron)
        .AddClass(theme.SelectCustomChevronOpen, isFocused)
        .Build();

    private string panelSurfaceClasses => popoverBuilder.GetSurfaceClasses(Rounded, Shadow, theme.SelectPanelSurface);

    private string messageClasses => new ClassBuilder(theme.SelectCustomPlaceholder)
        .AddClass(options.Theme.Typography.Size.Sm)
        .AddClass(options.Theme.Spacing.Padding.Get(2))
        .Build();

    /// <inheritdoc />
    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        if (string.IsNullOrEmpty(RootId))
        {
            RootId = Guid.NewGuid().ToString("N");
        }

        // A field with no "name" is omitted from form submissions; default to Id unless the consumer supplied one,
        // the same as TwSelect and TwTextfield.
        if (!Attributes.ContainsKey("name") && !string.IsNullOrEmpty(Id))
        {
            Attributes["name"] = Id;
        }

        // Only a SelectedValue the parent actually changed resets the text, so a parent re-render never wipes out what
        // the user is part way through typing.
        if (!parametersInitialized || !EqualityComparer<T>.Default.Equals(SelectedValue, lastSelectedValue))
        {
            parametersInitialized = true;
            lastSelectedValue = SelectedValue;
            hasSelection = IsKnownValue(SelectedValue);
            text = selectedDisplayText;
        }
        else if (!ReferenceEquals(Values, lastValues) && !hasSelection && !isFocused && IsKnownValue(SelectedValue))
        {
            // Values often arrive after SelectedValue (loaded asynchronously), which only then makes it a real option.
            hasSelection = true;
            text = selectedDisplayText;
        }

        lastValues = Values;

        if (isFocused && SearchFunc is null)
        {
            shownOptions = FilterValues(GetQuery());
        }
    }

    /// <inheritdoc />
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);

        if (firstRender)
        {
            autocompleteDotNetRef = DotNetObjectReference.Create(this);
            await JSRuntime.InvokeVoidAsync("twAutocomplete.attach", inputRef, autocompleteDotNetRef);
        }

        if (isFocused && PanelRef.Context != null)
        {
            // matchAnchorWidth: the suggestions span the field's width, as TwSelect's options do.
            await RegisterPanelScrollBehaviorAsync(PanelRef, matchAnchorWidth: true);
        }

        if (isFocused || activeDescendantSet)
        {
            activeDescendantSet = isFocused;
            var highlight = pendingHighlight;
            pendingHighlight = null;
            await JSRuntime.InvokeVoidAsync("twAutocomplete.refresh", inputRef, highlight);
        }
    }

    /// <summary>
    /// Opens the list from the keyboard (Arrow Down or Arrow Up on the field, called by <c>twAutocomplete.attach</c>)
    /// and highlights its first or last suggestion.
    /// </summary>
    /// <param name="highlight"><c>first</c> or <c>last</c>, or <see langword="null"/> to open without a highlight.</param>
    /// <returns>A task that completes once the list is open.</returns>
    [JSInvokable("OpenFromKeyboard")]
    public async Task OpenFromKeyboardAsync(string? highlight)
    {
        if (Disabled || ReadOnly)
            return;

        if (!isFocused)
        {
            await OpenAsync();
            await RefreshOptionsAsync(GetQuery());
        }

        pendingHighlight = highlight;
        await InvokeAsync(StateHasChanged);
    }

    /// <summary>
    /// Selects a suggestion, by click or by Enter on the highlighted one (see <c>twAutocomplete.attach</c>), puts its
    /// text in the field and closes the list. Focus stays in the field.
    /// </summary>
    /// <param name="key">The suggestion's key in the list shown.</param>
    /// <returns>A task that completes once the selection is committed.</returns>
    [JSInvokable("SelectOption")]
    public async Task SelectOptionAsync(int key)
    {
        if (Disabled || ReadOnly || !isFocused || !shownOptions.TryGetValue(key, out var value))
            return;

        await CommitSelectionAsync(value);
        await Close();
    }

    /// <summary>
    /// Empties the field and clears the selection, then moves focus back to the field.
    /// </summary>
    /// <returns>A task that completes once the field has been cleared.</returns>
    public async Task ClearAsync()
    {
        if (Disabled || ReadOnly)
            return;

        text = string.Empty;

        if (hasSelection)
        {
            await CommitClearedAsync();
        }

        if (isFocused)
        {
            await RefreshOptionsAsync(text);
        }

        await FocusInputAsync();
    }

    /// <inheritdoc />
    /// <remarks>
    /// Re-declares <see cref="JSInvokableAttribute"/>: JS interop resolves invokable methods on the runtime type, and
    /// an override does not inherit the attribute from the base method. Focus is never moved, as it never left the
    /// field while the list was open.
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

        autocompleteDotNetRef?.Dispose();
        autocompleteDotNetRef = null;

        await base.DisposeAsync();

        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Updates the suggestions as the user types, opening the list once <see cref="MinCharacters"/> have been typed.
    /// </summary>
    private async Task OnInputAsync(ChangeEventArgs e)
    {
        if (Disabled || ReadOnly)
            return;

        text = e.Value?.ToString() ?? string.Empty;

        if (text.Length < MinCharacters)
        {
            if (isFocused)
            {
                await Close();
            }

            return;
        }

        await OpenAsync();
        pendingHighlight = "reset";
        await RefreshOptionsAsync(text);
    }

    /// <summary>
    /// Opens the list when the field is clicked. Focus stays in the field so the user can type.
    /// </summary>
    private async Task OnInputClickAsync()
    {
        if (Disabled || ReadOnly || isFocused || text.Length < MinCharacters)
            return;

        await OpenAsync();
        await RefreshOptionsAsync(GetQuery());
    }

    /// <summary>
    /// Opens or closes the list from the chevron button, keeping focus in the field.
    /// </summary>
    private async Task TogglePanelAsync()
    {
        if (Disabled || ReadOnly)
            return;

        if (isFocused)
        {
            await Close();
            return;
        }

        // The button keeps focus from moving on mousedown, so a field that was not focused yet is focused here.
        await FocusInputAsync();
        await OpenAsync();
        await RefreshOptionsAsync(GetQuery());
    }

    /// <summary>
    /// Closes the list when focus leaves the field, and settles the text: it becomes the selected option's text again,
    /// unless it exactly matches another option (which is then selected) or was emptied (which clears the selection).
    /// </summary>
    private async Task OnInputBlurAsync()
    {
        if (isFocused)
        {
            await Close();
        }

        if (Disabled || ReadOnly || (hasSelection && text == selectedDisplayText))
            return;

        if (text.Length == 0)
        {
            if (hasSelection && !Required)
            {
                await CommitClearedAsync();
            }

            text = selectedDisplayText;
            return;
        }

        var candidates = SearchFunc is null ? Values : shownOptions.Values;
        // A list rather than FirstOrDefault, as default(T) can itself be one of the options.
        var match = candidates.Where(c => string.Compare(GetDisplayText(c), text, culture, CompareOptions.IgnoreCase) == 0).Take(1).ToList();
        if (match.Count == 1)
        {
            await CommitSelectionAsync(match[0]);
            return;
        }

        text = selectedDisplayText;
    }

    private async Task OpenAsync()
    {
        if (isFocused)
            return;

        isFocused = true;
        activeDescendantSet = true;
        await RegisterOutsideClickAsync();
    }

    /// <summary>
    /// Gets the text to filter by. When the field still shows the selected option, every option is listed, so opening
    /// the list shows the alternatives rather than only the current choice.
    /// </summary>
    private string GetQuery() => hasSelection && text == selectedDisplayText ? string.Empty : text;

    /// <summary>
    /// Fills the list for <paramref name="query"/>, from <see cref="Values"/> or from <see cref="SearchFunc"/>.
    /// </summary>
    private async Task RefreshOptionsAsync(string query)
    {
        if (SearchFunc is null)
        {
            shownOptions = FilterValues(query);
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

            shownOptions = ToOptions(results);
            isLoading = false;
            pendingHighlight = "reset";
        }
        catch (OperationCanceledException)
        {
            // A newer keystroke, or closing the list, replaced this search.
        }
    }

    private void CancelSearch()
    {
        searchCancellation?.Cancel();
        searchCancellation?.Dispose();
        searchCancellation = null;
    }

    /// <summary>
    /// Gets the options from <see cref="Values"/> whose text matches <paramref name="query"/>, keyed by their position
    /// in <see cref="Values"/> so an option keeps its id however the list is filtered.
    /// </summary>
    private Dictionary<int, T> FilterValues(string query)
    {
        var matches = new Dictionary<int, T>();
        var compareInfo = culture.CompareInfo;
        const CompareOptions compareOptions = CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace;
        var key = 1;

        foreach (var value in Values)
        {
            if (MaxItems is int max && matches.Count >= max)
                break;

            var display = GetDisplayText(value);
            var isMatch = query.Length == 0 || FilterMode switch
            {
                AutocompleteFilterMode.StartsWith => compareInfo.IsPrefix(display, query, compareOptions),
                _ => compareInfo.IndexOf(display, query, compareOptions) >= 0
            };

            if (isMatch)
            {
                matches.Add(key, value);
            }

            key++;
        }

        return matches;
    }

    private Dictionary<int, T> ToOptions(IEnumerable<T> results)
    {
        var limited = MaxItems is int max ? results.Take(max) : results;
        var key = 1;
        return limited.ToDictionary(_ => key++);
    }

    private bool IsSelected(T value) => hasSelection && EqualityComparer<T>.Default.Equals(value, SelectedValue);

    /// <summary>
    /// Whether <paramref name="value"/> is a real selection: one of <see cref="Values"/>, or with
    /// <see cref="SearchFunc"/> any value other than <see langword="default"/>.
    /// </summary>
    private bool IsKnownValue(T value)
    {
        if (value is null)
            return false;

        return SearchFunc is null
            ? Values.Contains(value, EqualityComparer<T>.Default)
            : !EqualityComparer<T>.Default.Equals(value, default!);
    }

    private async Task CommitSelectionAsync(T value)
    {
        var changed = !hasSelection || !EqualityComparer<T>.Default.Equals(value, SelectedValue);

        SelectedValue = value;
        lastSelectedValue = value;
        hasSelection = true;
        text = GetDisplayText(value);

        if (changed && SelectedValueChanged.HasDelegate)
        {
            await SelectedValueChanged.InvokeAsync(value);
            NotifyFieldChanged();
        }
    }

    private async Task CommitClearedAsync()
    {
        SelectedValue = default!;
        lastSelectedValue = default!;
        hasSelection = false;

        if (SelectedValueChanged.HasDelegate)
        {
            await SelectedValueChanged.InvokeAsync(SelectedValue);
            NotifyFieldChanged();
        }
    }

    private async Task FocusInputAsync()
    {
        try
        {
            await inputRef.FocusAsync();
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

    private string GetDisplayText(T value)
    {
        if (value is null)
            return string.Empty;

        if (!string.IsNullOrWhiteSpace(PropertyName))
        {
            var property = value.GetType().GetProperty(PropertyName);
            if (property != null)
            {
                return property.GetValue(value)?.ToString() ?? string.Empty;
            }
        }

        return value.ToString() ?? string.Empty;
    }

    /// <inheritdoc />
    protected override FieldIdentifier? CreateFieldIdentifier() => SelectedValueExpression is null ? null : FieldIdentifier.Create(SelectedValueExpression);
}
