// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using System.Linq.Expressions;
using System.Text;

namespace TwBlazor.Components;

/// <summary>
/// Represents a text field that enforces a structured format as the user types, such as a phone number, a social
/// security number or a custom code.
/// </summary>
/// <remarks>
/// <para>
/// The field is a <see cref="TwTextfield{T}"/>, so it shares its variants, sizes, label, validation and error display.
/// The format is described by <see cref="Mask"/>, a string of tokens and literals. The raw, unformatted characters are
/// held in <see cref="Value"/>; the literals and <see cref="PlaceholderChar"/>s are only shown in the field.
/// </para>
/// <para>
/// Masking itself runs in the browser, so pasting, IME input, Backspace and Delete all re-format to the same shape and
/// the field never holds a value that breaks the mask. While <see cref="ShowMask"/> is set, an empty field shows the
/// full mask so its structure is visible before any input.
/// </para>
/// </remarks>
public partial class TwMaskedInput : TwBlazorTextInputComponentBase
{
    // The mask tokens: '9' a digit, 'A' a letter and '*' a letter or digit. Every other character is a literal separator.
    private const char digitToken = '9';
    private const char letterToken = 'A';
    private const char alphanumericToken = '*';

    // The inner field gets root attributes of its own, so the input's attributes are not copied onto its wrapper.
    private readonly Dictionary<string, object> _textfieldRootAttributes = new() { ["data-tw-masked-input-field"] = true };

    [Inject] private IJSRuntime jsRuntime { get; set; } = null!;

    private TwTextfield<string>? textfield;

    /// <summary>
    /// The masked text shown in the field, which differs from <see cref="Value"/> by its literals and placeholder
    /// characters.
    /// </summary>
    private string text = string.Empty;

    private bool textSynced;
    private string lastValue = string.Empty;
    private string? lastMask;
    private char lastPlaceholderChar;
    private bool lastShowMask;

    /// <summary>
    /// The last message returned by <see cref="Validator"/>.
    /// </summary>
    private string? validationError;

    private bool jsAttached;
    private string? attachedMask;
    private char attachedPlaceholderChar;
    private bool attachedShowMask;

    /// <summary>
    /// Gets or sets the mask the field enforces.
    /// </summary>
    /// <remarks>
    /// Each character describes one position: <c>9</c> accepts a digit 0-9, <c>A</c> a letter A-Z or a-z and <c>*</c>
    /// either. Any other character is a literal shown between the typed characters, such as <c>(</c>, <c>)</c>,
    /// <c>-</c> or a space. For example <c>(999) 999-9999</c> for a phone number.
    /// </remarks>
    [Parameter, EditorRequired] public string Mask { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the current value as the raw, unformatted characters, without the mask's literals or placeholders.
    /// </summary>
    /// <remarks>
    /// Works with <see cref="ValueChanged"/> for <c>@bind-Value</c>. For the mask <c>(999) 999-9999</c>, the value
    /// <c>1234567890</c> is shown as <c>(123) 456-7890</c>.
    /// </remarks>
    [Parameter] public string Value { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the callback that is invoked when the raw value changes.
    /// </summary>
    /// <remarks>
    /// Invoked when typed text is committed (see <see cref="BindEvent"/>) and when the field is cleared.
    /// </remarks>
    [Parameter] public EventCallback<string> ValueChanged { get; set; }

    /// <summary>
    /// Gets or sets the expression that identifies the bound value. Set automatically by <c>@bind-Value</c>, it
    /// lets an enclosing <see cref="EditForm"/> or <see cref="TwForm"/> validate the property the field edits.
    /// </summary>
    [Parameter] public Expression<Func<string>>? ValueExpression { get; set; }

    /// <summary>
    /// Gets or sets the character shown for each unfilled mask position. Defaults to <c>_</c>.
    /// </summary>
    [Parameter] public char PlaceholderChar { get; set; } = '_';

    /// <summary>
    /// Gets or sets whether an empty field shows the full mask as a placeholder. Defaults to <see langword="true"/>.
    /// </summary>
    /// <remarks>
    /// When <see langword="false"/>, an empty field is left empty, and a partially filled field shows only the typed
    /// characters and the literals that lead up to them.
    /// </remarks>
    [Parameter] public bool ShowMask { get; set; } = true;

    /// <summary>
    /// Gets or sets the name of the DOM event that commits typed text. Defaults to <c>onchange</c>, which commits when
    /// the user leaves the field. Set it to <c>oninput</c> to update the value on every keystroke.
    /// </summary>
    [Parameter] public string BindEvent { get; set; } = "onchange";

    /// <summary>
    /// Gets or sets the placeholder text shown while the field is empty and <see cref="ShowMask"/> is
    /// <see langword="false"/>.
    /// </summary>
    [Parameter] public string Placeholder { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets whether a value is required. The field is marked required for the browser's own validation and
    /// announced as required by assistive technology.
    /// </summary>
    [Parameter] public bool Required { get; set; }

    /// <summary>
    /// Gets or sets an optional asynchronous validator invoked with the raw value whenever it changes.
    /// </summary>
    /// <remarks>
    /// Return <see langword="null"/> or an empty string to mark the value valid, or a message to mark the field invalid
    /// and show the message below it. The validator runs after <see cref="ValueChanged"/> is invoked.
    /// </remarks>
    [Parameter] public Func<string, Task<string?>>? Validator { get; set; }

    /// <summary>
    /// Gets or sets whether the text field's clear button is shown at the end of the field.
    /// </summary>
    [Parameter] public bool Clearable { get; set; }

    /// <summary>
    /// Gets or sets the callback invoked after the clear button (see <see cref="Clearable"/>) has emptied the field.
    /// </summary>
    [Parameter] public EventCallback OnClear { get; set; }

    /// <summary>
    /// Gets or sets the callback invoked when the field is focused.
    /// </summary>
    [Parameter] public EventCallback<FocusEventArgs> OnFocus { get; set; }

    /// <summary>
    /// Gets or sets the callback invoked when the field loses focus.
    /// </summary>
    [Parameter] public EventCallback<FocusEventArgs> OnBlur { get; set; }

    /// <summary>
    /// Gets or sets the callback invoked when the field is clicked.
    /// </summary>
    [Parameter] public EventCallback<MouseEventArgs> OnClick { get; set; }

    /// <summary>
    /// Gets or sets the callback invoked when a key is pressed while the field has focus.
    /// </summary>
    [Parameter] public EventCallback<KeyboardEventArgs> OnKeyDown { get; set; }

    /// <summary>
    /// Gets the reference to the rendered &lt;input&gt; element.
    /// </summary>
    public ElementReference InputRef => textfield?.InputRef ?? default;

    private bool allDigitsMask
    {
        get
        {
            var hasToken = false;
            foreach (var character in Mask)
            {
                if (!IsToken(character))
                {
                    continue;
                }

                hasToken = true;
                if (character != digitToken)
                {
                    return false;
                }
            }

            return hasToken;
        }
    }

    /// <summary>
    /// Gets the attributes for the &lt;input&gt;: the autocomplete and mobile-keyboard defaults, followed by anything
    /// the consumer passed, so those can override them.
    /// </summary>
    private Dictionary<string, object> inputAttributes
    {
        get
        {
            var attributes = new Dictionary<string, object>
            {
                ["autocomplete"] = "off",
                ["inputmode"] = allDigitsMask ? "numeric" : "text"
            };

            foreach (var attribute in Attributes)
            {
                attributes[attribute.Key] = attribute.Value;
            }

            return attributes;
        }
    }

    private bool effectiveInvalid => fieldInvalid || !string.IsNullOrEmpty(validationError);

    private string effectiveErrorMessage => (Invalid && !string.IsNullOrEmpty(ErrorMessage)
        ? ErrorMessage
        : validatorMessage) ?? fieldErrorMessage;

    /// <summary>
    /// Gets the message <see cref="Validator"/> last returned, or <see langword="null"/> when it found no problem.
    /// </summary>
    private string? validatorMessage => string.IsNullOrEmpty(validationError) ? null : validationError;

    /// <inheritdoc />
    protected override string? ownErrorMessage => base.ownErrorMessage ?? validatorMessage;

    /// <inheritdoc />
    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        if (string.IsNullOrEmpty(RootId))
        {
            RootId = Guid.NewGuid().ToString("N");
        }

        var maskChanged = Mask != lastMask || PlaceholderChar != lastPlaceholderChar || ShowMask != lastShowMask;
        var valueChanged = Value != lastValue;

        // A value the mask no longer holds (it shrank, or the value gained a character the mask rejects) is truncated
        // to the prefix it can hold and the change is emitted once, so the model and the field never disagree. The
        // echoed value round-trips, so this fires at most once per change.
        if (maskChanged || valueChanged)
        {
            var valid = TruncateToMask(Mask, Value);
            if (valid != Value)
            {
                Value = valid;
                lastValue = valid;
                valueChanged = true;
                if (ValueChanged.HasDelegate)
                {
                    _ = ValueChanged.InvokeAsync(valid);
                }
            }
        }

        if (!textSynced || valueChanged || maskChanged)
        {
            textSynced = true;
            lastValue = Value;
            lastMask = Mask;
            lastPlaceholderChar = PlaceholderChar;
            lastShowMask = ShowMask;
            text = FormatMask(Mask, Value, PlaceholderChar, ShowMask);
        }
    }

    /// <inheritdoc />
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (textfield is null)
        {
            return;
        }

        try
        {
            if (!jsAttached)
            {
                await jsRuntime.InvokeVoidAsync("twMaskedInput.attach", textfield.InputRef, Mask, PlaceholderChar.ToString(), ShowMask);
                jsAttached = true;
            }
            else if (Mask != attachedMask || PlaceholderChar != attachedPlaceholderChar || ShowMask != attachedShowMask)
            {
                await jsRuntime.InvokeVoidAsync("twMaskedInput.update", textfield.InputRef, Mask, PlaceholderChar.ToString(), ShowMask);
            }

            attachedMask = Mask;
            attachedPlaceholderChar = PlaceholderChar;
            attachedShowMask = ShowMask;
        }
        catch (JSDisconnectedException)
        {
            // The circuit disconnected before the script could run; nothing to attach.
        }
        catch (InvalidOperationException)
        {
            // JS interop is unavailable while prerendering; it is attached on the interactive render instead.
        }
    }

    /// <summary>
    /// Moves focus to the field.
    /// </summary>
    /// <returns>A task that completes once focus has been requested.</returns>
    public async Task FocusAsync()
    {
        if (textfield is not null)
        {
            await textfield.InputRef.FocusAsync();
        }
    }

    /// <summary>
    /// Empties the field: clears the raw value, restores the mask placeholder and notifies the consumer.
    /// </summary>
    /// <remarks>
    /// Does nothing while the field is disabled or read-only. The mask is redrawn even when <see cref="Value"/> was
    /// already empty, so the field never stays blank.
    /// </remarks>
    /// <returns>A task that completes once the value has been cleared and the consumer notified.</returns>
    public async Task ClearAsync()
    {
        if (Disabled || ReadOnly)
        {
            return;
        }

        text = FormatMask(Mask, string.Empty, PlaceholderChar, ShowMask);
        await SetValueAsync(string.Empty);

        if (OnClear.HasDelegate)
        {
            await OnClear.InvokeAsync();
        }

        await InvokeAsync(StateHasChanged);
    }

    /// <summary>
    /// Strips the literals and placeholders from text committed by the field and applies the raw characters as the value.
    /// </summary>
    /// <param name="newText">The text in the field.</param>
    /// <returns>A task that completes once the value has been applied.</returns>
    private async Task OnTextChangedAsync(string newText)
    {
        newText ??= string.Empty;

        var raw = StripMask(Mask, newText, PlaceholderChar);
        if (raw == Value)
        {
            return;
        }

        // Keep the display in sync with the raw value, so the field still shows the mask when the JavaScript
        // reformatter has not run (for example while prerendering).
        text = FormatMask(Mask, raw, PlaceholderChar, ShowMask);

        await SetValueAsync(raw);
    }

    /// <summary>
    /// Applies a new raw value, notifies the consumer and runs <see cref="Validator"/>.
    /// </summary>
    /// <param name="value">The new raw value.</param>
    /// <returns>A task that completes once the consumer has been notified and the value validated.</returns>
    private async Task SetValueAsync(string value)
    {
        if (value == Value)
        {
            return;
        }

        // Value and lastValue are updated before any await, so a keystroke queued behind this change reads the new
        // value, and the parent echoing it back does not overwrite the text the user is typing.
        Value = value;
        lastValue = value;

        if (ValueChanged.HasDelegate)
        {
            await ValueChanged.InvokeAsync(value);
        }

        NotifyFieldChanged();

        if (Validator is not null)
        {
            validationError = await Validator(value);
        }
    }

    /// <inheritdoc />
    protected override FieldIdentifier? CreateFieldIdentifier() => ValueExpression is null ? null : FieldIdentifier.Create(ValueExpression);

    /// <summary>
    /// Runs <see cref="Validator"/> on the current value, then reports whether the field is valid. Called by an
    /// enclosing <see cref="TwForm"/> when it is submitted.
    /// </summary>
    /// <returns><see langword="true"/> when the field is valid.</returns>
    public override async Task<bool> ValidateAsync()
    {
        if (Validator is not null)
        {
            validationError = await Validator(Value);
            await InvokeAsync(StateHasChanged);
        }

        return !effectiveInvalid;
    }

    /// <summary>
    /// Removes the input listener from the field.
    /// </summary>
    /// <returns>A task that completes once the listener is removed.</returns>
    public override async ValueTask DisposeAsync()
    {
        await base.DisposeAsync();

        if (!jsAttached || textfield is null)
        {
            return;
        }

        try
        {
            await jsRuntime.InvokeVoidAsync("twMaskedInput.detach", textfield.InputRef);
        }
        catch (JSDisconnectedException)
        {
            // The circuit is already gone; nothing left to clean up.
        }
        catch (InvalidOperationException)
        {
            // JS interop unavailable during teardown; safe to ignore.
        }
        finally
        {
            GC.SuppressFinalize(this);
        }
    }

    /// <summary>
    /// Reports whether a mask character is a fillable token rather than a literal separator.
    /// </summary>
    /// <param name="character">The mask character.</param>
    /// <returns><see langword="true"/> when the character is a fillable token.</returns>
    private static bool IsToken(char character) => character is digitToken or letterToken or alphanumericToken;

    /// <summary>
    /// Reports whether a typed character matches a fillable token.
    /// </summary>
    /// <param name="token">The mask token.</param>
    /// <param name="character">The typed character.</param>
    /// <returns><see langword="true"/> when the character is accepted by the token.</returns>
    private static bool TokenMatches(char token, char character) => token switch
    {
        digitToken => character is >= '0' and <= '9',
        letterToken => character is (>= 'A' and <= 'Z') or (>= 'a' and <= 'z'),
        alphanumericToken => character is (>= '0' and <= '9') or (>= 'A' and <= 'Z') or (>= 'a' and <= 'z'),
        _ => false
    };

    /// <summary>
    /// Builds the masked display string from a raw value.
    /// </summary>
    /// <param name="mask">The mask.</param>
    /// <param name="raw">The raw, unformatted characters.</param>
    /// <param name="placeholderChar">The character shown for unfilled positions.</param>
    /// <param name="showMask">Whether an empty field shows the full mask.</param>
    /// <returns>The masked string.</returns>
    private static string FormatMask(string mask, string raw, char placeholderChar, bool showMask)
    {
        var full = new StringBuilder(mask.Length);
        var rawIndex = 0;
        var lastFilled = -1;

        foreach (var token in mask)
        {
            if (IsToken(token))
            {
                // Fill from raw only while the next character matches this token. The first character that does not
                // (wrong type, non-ASCII, or past the end of the mask) stops filling, and every later token stays a
                // placeholder. This keeps StripMask(FormatMask(v)) == v for every token-valid v.
                if (rawIndex < raw.Length && TokenMatches(token, raw[rawIndex]))
                {
                    full.Append(raw[rawIndex]);
                    rawIndex++;
                    lastFilled = full.Length - 1;
                }
                else
                {
                    full.Append(placeholderChar);
                }
            }
            else
            {
                full.Append(token);
            }
        }

        if (showMask)
        {
            return full.ToString();
        }

        // Without the mask shown, keep everything up to and including the last typed character, dropping the
        // placeholder and literal characters that follow it.
        return lastFilled < 0 ? string.Empty : full.ToString(0, lastFilled + 1);
    }

    /// <summary>
    /// Extracts the raw characters from a masked string.
    /// </summary>
    /// <param name="mask">The mask.</param>
    /// <param name="value">The masked string.</param>
    /// <param name="placeholderChar">The character used for unfilled positions.</param>
    /// <returns>The raw, unformatted characters.</returns>
    private static string StripMask(string mask, string value, char placeholderChar)
    {
        var raw = new StringBuilder(value.Length);
        var tokenIndex = 0;

        foreach (var character in value)
        {
            // Skip over the mask's literal separators to the next fillable token.
            while (tokenIndex < mask.Length && !IsToken(mask[tokenIndex]))
            {
                tokenIndex++;
            }

            if (tokenIndex >= mask.Length)
            {
                break;
            }

            if (character != placeholderChar && TokenMatches(mask[tokenIndex], character))
            {
                raw.Append(character);
                tokenIndex++;
            }
        }

        return raw.ToString();
    }

    /// <summary>
    /// Returns the longest prefix of <paramref name="value"/> the mask can hold: each character matches the next
    /// fillable token, and nothing is kept past the mask's last token.
    /// </summary>
    /// <param name="mask">The mask.</param>
    /// <param name="value">The raw value to truncate.</param>
    /// <returns>The token-valid prefix of <paramref name="value"/>.</returns>
    private static string TruncateToMask(string mask, string value)
    {
        var result = new StringBuilder(value.Length);
        var tokenIndex = 0;

        foreach (var character in value)
        {
            // Skip over the mask's literal separators to the next fillable token.
            while (tokenIndex < mask.Length && !IsToken(mask[tokenIndex]))
            {
                tokenIndex++;
            }

            if (tokenIndex >= mask.Length || !TokenMatches(mask[tokenIndex], character))
            {
                break;
            }

            result.Append(character);
            tokenIndex++;
        }

        return result.ToString();
    }
}
