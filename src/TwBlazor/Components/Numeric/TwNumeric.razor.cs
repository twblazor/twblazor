// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using System.Globalization;
using System.Text;
using TwBlazor.Configuration.Components;
using TwBlazor.Enums;
using TwBlazor.Localization;
using TwBlazor.Utilities;

namespace TwBlazor.Components;

/// <summary>
/// Represents a field for entering a number, with decrease and increase buttons, keyboard and mouse wheel stepping, and
/// optional limits, formatting, prefix and suffix.
/// </summary>
/// <remarks>
/// <para>
/// The field is a <see cref="TwTextfield{T}"/>, so it shares its variants, sizes, label, validation and error display.
/// The value is parsed and formatted in <see cref="Culture"/>, kept within <see cref="Min"/> and <see cref="Max"/>, and
/// stepped by <see cref="Step"/>.
/// </para>
/// <para>
/// The input has the <c>spinbutton</c> role, with <c>aria-valuenow</c>, <c>aria-valuemin</c>, <c>aria-valuemax</c> and
/// <c>aria-valuetext</c> kept up to date, so screen readers announce the value and its limits. Arrow Up and Arrow Down
/// step the value, and Page Up and Page Down step it ten times as far. The buttons are left out of the Tab order, as the
/// keys do the same job, and clicking them keeps focus in the field.
/// </para>
/// </remarks>
/// <typeparam name="T">
/// The type of the value: <see cref="int"/>, <see cref="long"/>, <see cref="short"/>, <see cref="sbyte"/>,
/// <see cref="uint"/>, <see cref="ulong"/>, <see cref="ushort"/>, <see cref="byte"/>, <see cref="decimal"/>,
/// <see cref="double"/> or <see cref="float"/>, or the nullable form of one to let the field be left empty.
/// </typeparam>
public partial class TwNumeric<T> : TwBlazorTextInputComponentBase, IAsyncDisposable
{
    // Page Up and Page Down take this many steps at once, the larger step the spinbutton pattern asks for.
    private const int pageSteps = 10;

    // The width of each button and the gap beside a prefix or suffix. TwNumericTheme documents both, since the field's
    // padding is widened by them to keep the text clear of what is drawn over it.
    private const string buttonWidth = "2rem";
    private const string adornmentGap = "0.25rem";
    private const string variantPadding = "0.75rem";

    private static readonly NumericValue<T> _number = NumericValue<T>.Instance;

    // The inner field gets root attributes of its own, so it does not copy the input's attributes (role, aria-value*,
    // the wheel handler) onto its wrapper as well.
    private readonly Dictionary<string, object> _textfieldRootAttributes = new() { ["data-tw-numeric-field"] = true };

    [Inject] private IJSRuntime jsRuntime { get; set; } = null!;

    private TwNumericTheme theme => options.Theme.Components.Require<TwNumericTheme>();

    private TwTextfield<string>? textfield;

    /// <summary>
    /// The text shown in the field, which can differ from <see cref="Value"/> while the user is typing.
    /// </summary>
    private string text = string.Empty;

    private bool textSynced;
    private T lastValue = default!;
    private string? lastFormat;
    private CultureInfo? lastCulture;

    /// <summary>
    /// Set when the text in the field is not a number, and cleared once it is.
    /// </summary>
    private string? inputError;

    /// <summary>
    /// The last message returned by <see cref="Validator"/>.
    /// </summary>
    private string? validationError;

    // Text that failed to parse while BindEvent is "oninput" is only flagged once the user leaves the field, so the
    // error does not flash up while they are halfway through typing "-" or "1.".
    private bool pendingInputError;

    private bool isFocused;
    private bool jsAttached;
    private string? attachedPattern;
    private bool attachedWheelDisabled;

    /// <summary>
    /// Initializes a new instance, with <see cref="Min"/>, <see cref="Max"/> and <see cref="Step"/> set for <typeparamref name="T"/>.
    /// </summary>
    public TwNumeric()
    {
        Min = _number.MinValue;
        Max = _number.MaxValue;
        Step = _number.One;
    }

    /// <summary>
    /// Gets or sets the current value.
    /// </summary>
    /// <remarks>
    /// Works with <see cref="ValueChanged"/> for <c>@bind-Value</c>. A nullable <typeparamref name="T"/> lets the user
    /// empty the field, which sets the value to <see langword="null"/>.
    /// </remarks>
    [Parameter] public T Value { get; set; } = default!;

    /// <summary>
    /// Gets or sets the callback that is invoked when the value changes.
    /// </summary>
    /// <remarks>
    /// Invoked when typed text is committed (see <see cref="BindEvent"/>), and on every step from a key, the mouse wheel
    /// or a button.
    /// </remarks>
    [Parameter] public EventCallback<T> ValueChanged { get; set; }

    /// <summary>
    /// Gets or sets the lowest value allowed. Defaults to the smallest value <typeparamref name="T"/> can hold.
    /// </summary>
    /// <remarks>
    /// A lower typed value is raised to it, and stepping stops at it. For a nullable <typeparamref name="T"/>,
    /// <see langword="null"/> means no limit.
    /// </remarks>
    [Parameter] public T Min { get; set; }

    /// <summary>
    /// Gets or sets the highest value allowed. Defaults to the largest value <typeparamref name="T"/> can hold.
    /// </summary>
    /// <remarks>
    /// A higher typed value is lowered to it, and stepping stops at it. For a nullable <typeparamref name="T"/>,
    /// <see langword="null"/> means no limit.
    /// </remarks>
    [Parameter] public T Max { get; set; }

    /// <summary>
    /// Gets or sets how much one step changes the value. Defaults to 1.
    /// </summary>
    /// <remarks>
    /// Used by Arrow Up and Arrow Down, the mouse wheel and the buttons. Page Up and Page Down take ten steps. A negative
    /// step is treated as positive.
    /// </remarks>
    [Parameter] public T Step { get; set; }

    /// <summary>
    /// Gets or sets the .NET numeric format string the value is shown in, such as <c>N2</c> for two decimal places with
    /// thousands separators, or <c>C</c> for currency. Defaults to the general format.
    /// </summary>
    /// <remarks>
    /// The text is reformatted when the user leaves the field, or as soon as it is committed while
    /// <see cref="BindEvent"/> is <c>onchange</c>. Typed text is parsed in <see cref="Culture"/>, so thousands
    /// separators and the culture's currency symbol are accepted.
    /// </remarks>
    [Parameter] public string? Format { get; set; }

    /// <summary>
    /// Gets or sets the culture the value is parsed and formatted in. Defaults to the culture set through
    /// <c>options.Localization</c>, or the current culture.
    /// </summary>
    [Parameter] public CultureInfo? Culture { get; set; }

    /// <summary>
    /// Gets or sets a regular expression that each typed character must match, such as <c>[0-9]</c>. Keys that do not
    /// match are ignored.
    /// </summary>
    /// <remarks>
    /// Defaults to the digits plus the separators and signs <typeparamref name="T"/> can use in <see cref="Culture"/>:
    /// the decimal separator only for types that hold fractions, and the minus sign only when <see cref="Min"/> is below
    /// zero. Pasted text is not filtered, it is parsed and flagged when it is not a number. The pattern is tested in the
    /// browser as a JavaScript regular expression.
    /// </remarks>
    [Parameter] public string? Pattern { get; set; }

    /// <summary>
    /// Gets or sets the name of the DOM event that commits typed text. Defaults to <c>onchange</c>, which commits when
    /// the user leaves the field. Set it to <c>oninput</c> to update the value on every keystroke.
    /// </summary>
    [Parameter] public string BindEvent { get; set; } = "onchange";

    /// <summary>
    /// Gets or sets the placeholder text shown while the field is empty.
    /// </summary>
    [Parameter] public string Placeholder { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets whether a value is required. The field is marked required for the browser's own validation and
    /// announced as required by assistive technology.
    /// </summary>
    [Parameter] public bool Required { get; set; }

    /// <summary>
    /// Gets or sets whether the decrease and increase buttons are hidden. The keys and mouse wheel still step the value.
    /// </summary>
    [Parameter] public bool HideSpinButtons { get; set; }

    /// <summary>
    /// Gets or sets whether the text field's clear button (see <see cref="TwTextfield{T}.Clearable"/>) is shown at the
    /// end of the field. It is disabled while the field is empty.
    /// </summary>
    /// <remarks>
    /// Clearing sets a nullable <typeparamref name="T"/> to <see langword="null"/>, and any other type to zero, kept
    /// within <see cref="Min"/> and <see cref="Max"/>. The button is hidden while the field is disabled or read-only.
    /// </remarks>
    [Parameter] public bool Clearable { get; set; }

    /// <summary>
    /// Gets or sets whether scrolling the mouse wheel up decreases the value instead of increasing it.
    /// </summary>
    [Parameter] public bool InvertMouseWheel { get; set; }

    /// <summary>
    /// Gets or sets whether the mouse wheel is ignored, so scrolling over the focused field scrolls the page instead of
    /// stepping the value.
    /// </summary>
    [Parameter] public bool DisableMouseWheel { get; set; }

    /// <summary>
    /// Gets or sets text shown at the start of the field, such as a currency symbol.
    /// </summary>
    /// <remarks>
    /// It is not part of the value. Screen readers hear it with the value through <c>aria-valuetext</c>.
    /// </remarks>
    [Parameter] public string? Prefix { get; set; }

    /// <summary>
    /// Gets or sets text shown at the end of the field, such as a unit.
    /// </summary>
    /// <remarks>
    /// It is not part of the value. Screen readers hear it with the value through <c>aria-valuetext</c>.
    /// </remarks>
    [Parameter] public string? Suffix { get; set; }

    /// <summary>
    /// Gets or sets an optional asynchronous validator invoked with the new value whenever it changes.
    /// </summary>
    /// <remarks>
    /// Return <see langword="null"/> or an empty string to mark the value valid, or a message to mark the field invalid
    /// and show the message below it. The validator runs after <see cref="ValueChanged"/> is invoked.
    /// </remarks>
    [Parameter] public Func<T, Task<string?>>? Validator { get; set; }

    /// <summary>
    /// Gets or sets the callback that is invoked when the field is focused.
    /// </summary>
    [Parameter] public EventCallback<FocusEventArgs> OnFocus { get; set; }

    /// <summary>
    /// Gets or sets the callback that is invoked when the field loses focus.
    /// </summary>
    [Parameter] public EventCallback<FocusEventArgs> OnBlur { get; set; }

    /// <summary>
    /// Gets or sets the callback invoked when the field is clicked.
    /// </summary>
    [Parameter] public EventCallback<MouseEventArgs> OnClick { get; set; }

    /// <summary>
    /// Gets or sets the callback invoked when a key is pressed while the field has focus. It runs before the key steps
    /// the value.
    /// </summary>
    [Parameter] public EventCallback<KeyboardEventArgs> OnKeyDown { get; set; }

    /// <summary>
    /// Gets the reference to the rendered &lt;input&gt; element.
    /// </summary>
    public ElementReference InputRef => textfield?.InputRef ?? default;

    private CultureInfo effectiveCulture => Culture ?? localizer.Formats.Culture;

    private bool canStep => !Disabled && !ReadOnly;

    private bool canIncrease => canStep && (!_number.HasValue(Value) || !_number.HasValue(Max) || _number.Compare(Value, Max) < 0);

    private bool canDecrease => canStep && (!_number.HasValue(Value) || !_number.HasValue(Min) || _number.Compare(Value, Min) > 0);

    private bool reservesClearButton => Clearable && canStep;

    private bool hasPrefix => !string.IsNullOrEmpty(Prefix);

    private bool hasSuffix => !string.IsNullOrEmpty(Suffix);

    private int buttonCount => (reservesClearButton ? 1 : 0) + (HideSpinButtons ? 0 : 2);

    private bool hasEndContent => hasSuffix || buttonCount > 0;

    // With no button after it, the suffix keeps the variant's padding from the field's end edge.
    private string? suffixStyle => HideSpinButtons && !reservesClearButton ? $"padding-inline-end: {padding};" : null;

    private bool effectiveInvalid => Invalid || inputError is not null || !string.IsNullOrEmpty(validationError);

    private string effectiveErrorMessage => (Invalid && !string.IsNullOrEmpty(ErrorMessage)
        ? ErrorMessage
        : inputError ?? validationError) ?? string.Empty;

    private string padding => effectiveVariant == InputVariant.Default ? "0px" : variantPadding;

    private static string buttonStyle => $"inline-size: {buttonWidth};";

    /// <summary>
    /// Gets the padding that keeps the typed text clear of the prefix, the suffix and the buttons.
    /// </summary>
    private string? inputStyle
    {
        get
        {
            var style = new StringBuilder();
            if (hasPrefix)
            {
                style.Append(CultureInfo.InvariantCulture, $"padding-inline-start: calc({padding} + {Prefix!.Length}ch + {adornmentGap});");
            }

            if (hasEndContent)
            {
                // One gap before each item in the end row: the first keeps it clear of the text, the rest separate the items.
                var items = buttonCount + (hasSuffix ? 1 : 0);
                var suffixWidth = hasSuffix ? $"{Suffix!.Length}ch + " : string.Empty;
                var edge = buttonCount > 0 ? "1px" : padding;
                style.Append(CultureInfo.InvariantCulture, $" padding-inline-end: calc({suffixWidth}{buttonCount} * {buttonWidth} + {items} * {adornmentGap} + {edge});");
            }

            return style.Length == 0 ? null : style.ToString().Trim();
        }
    }

    /// <summary>
    /// Gets the attributes for the &lt;input&gt;: the spinbutton role and state, the mobile keyboard to show, the padding
    /// and the mouse wheel handler, followed by anything the consumer passed.
    /// </summary>
    private Dictionary<string, object> inputAttributes
    {
        get
        {
            var attributes = new Dictionary<string, object>
            {
                ["role"] = "spinbutton",
                ["inputmode"] = _number.IsFractional ? "decimal" : "numeric",
                ["autocomplete"] = "off",
                ["onwheel"] = EventCallback.Factory.Create<WheelEventArgs>(this, OnWheelAsync)
            };

            if (_number.FormatInvariant(Value) is { } valueNow)
            {
                attributes["aria-valuenow"] = valueNow;
            }

            if (_number.IsBound(Min) && _number.FormatInvariant(Min) is { } valueMin)
            {
                attributes["aria-valuemin"] = valueMin;
            }

            if (_number.IsBound(Max) && _number.FormatInvariant(Max) is { } valueMax)
            {
                attributes["aria-valuemax"] = valueMax;
            }

            if (valueText is { } textValue)
            {
                attributes["aria-valuetext"] = textValue;
            }

            if (inputStyle is { } style)
            {
                attributes["style"] = style;
            }

            foreach (var attribute in Attributes)
            {
                attributes[attribute.Key] = attribute.Value;
            }

            return attributes;
        }
    }

    /// <summary>
    /// Gets the value as it should be read out when that differs from the bare number, such as "£1,250.00" or "20 kg".
    /// </summary>
    private string? valueText
    {
        get
        {
            if (!_number.HasValue(Value))
            {
                return null;
            }

            var formatted = _number.Format(Value, Format, effectiveCulture);
            var spoken = string.Join(" ", new[] { Prefix, formatted, Suffix }.Where(part => !string.IsNullOrEmpty(part)));
            return spoken == _number.FormatInvariant(Value) ? null : spoken;
        }
    }

    /// <summary>
    /// Gets the pattern each typed character must match: <see cref="Pattern"/>, or one built for
    /// <typeparamref name="T"/> and <see cref="Culture"/>.
    /// </summary>
    private string allowedPattern
    {
        get
        {
            if (!string.IsNullOrEmpty(Pattern))
            {
                return Pattern;
            }

            var numberFormat = effectiveCulture.NumberFormat;
            var characters = new StringBuilder();
            AppendCharacters(characters, numberFormat.NumberGroupSeparator);

            if (_number.IsFractional)
            {
                AppendCharacters(characters, numberFormat.NumberDecimalSeparator);
            }

            if (allowsNegative)
            {
                AppendCharacters(characters, numberFormat.NegativeSign);
                AppendCharacters(characters, "-");
            }

            if (Format is { Length: > 0 } format && char.ToUpperInvariant(format[0]) == 'C')
            {
                AppendCharacters(characters, numberFormat.CurrencySymbol);
                AppendCharacters(characters, numberFormat.CurrencyGroupSeparator);
                if (_number.IsFractional)
                {
                    AppendCharacters(characters, numberFormat.CurrencyDecimalSeparator);
                }
            }

            return $"[0-9{characters}]";
        }
    }

    private bool allowsNegative => _number.IsSigned && (!_number.HasValue(Min) || _number.Compare(Min, _number.Zero) < 0);

    /// <summary>
    /// Appends each character of <paramref name="value"/> to a regular expression character class, escaped so that
    /// separators such as "." or "-" are taken literally. Characters already in the class are skipped.
    /// </summary>
    /// <param name="builder">The character class being built.</param>
    /// <param name="value">The characters to allow.</param>
    private static void AppendCharacters(StringBuilder builder, string value)
    {
        foreach (var character in value)
        {
            var escaped = string.Create(CultureInfo.InvariantCulture, $"\\u{{{(int)character:X4}}}");
            if (!builder.ToString().Contains(escaped, StringComparison.Ordinal))
            {
                builder.Append(escaped);
            }
        }
    }

    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        if (string.IsNullOrEmpty(RootId))
        {
            RootId = Guid.NewGuid().ToString("N");
        }

        var culture = effectiveCulture;
        if (!textSynced || !EqualityComparer<T>.Default.Equals(Value, lastValue) || Format != lastFormat || !Equals(culture, lastCulture))
        {
            textSynced = true;
            lastValue = Value;
            lastFormat = Format;
            lastCulture = culture;
            text = _number.Format(Value, Format, culture);
            inputError = null;
            pendingInputError = false;
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (textfield is null)
        {
            return;
        }

        var pattern = allowedPattern;
        try
        {
            if (!jsAttached)
            {
                await jsRuntime.InvokeVoidAsync("twNumeric.attach", textfield.InputRef, pattern, DisableMouseWheel);
                jsAttached = true;
            }
            else if (pattern != attachedPattern || DisableMouseWheel != attachedWheelDisabled)
            {
                await jsRuntime.InvokeVoidAsync("twNumeric.update", textfield.InputRef, pattern, DisableMouseWheel);
            }

            attachedPattern = pattern;
            attachedWheelDisabled = DisableMouseWheel;
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
    /// Increases the value by one <see cref="Step"/>, stopping at <see cref="Max"/>.
    /// </summary>
    /// <returns>A task that completes once the new value has been applied.</returns>
    public Task IncrementAsync() => StepAsync(1);

    /// <summary>
    /// Decreases the value by one <see cref="Step"/>, stopping at <see cref="Min"/>.
    /// </summary>
    /// <returns>A task that completes once the new value has been applied.</returns>
    public Task DecrementAsync() => StepAsync(-1);

    /// <summary>
    /// Empties the field. A nullable <typeparamref name="T"/> becomes <see langword="null"/>, and any other type zero,
    /// kept within <see cref="Min"/> and <see cref="Max"/>.
    /// </summary>
    /// <returns>A task that completes once the new value has been applied.</returns>
    public async Task ClearAsync()
    {
        if (!canStep)
        {
            return;
        }

        var cleared = _number.IsNullable ? default! : _number.Clamp(_number.Zero, Min, Max);
        ClearInputError();
        text = _number.Format(cleared, Format, effectiveCulture);
        await SetValueAsync(cleared);
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
    /// Takes a number of steps from the current value and shows the result.
    /// </summary>
    /// <param name="steps">How many steps to take. Negative steps go down.</param>
    /// <returns>A task that completes once the new value has been applied.</returns>
    private async Task StepAsync(int steps)
    {
        if (!canStep)
        {
            return;
        }

        var stepped = _number.Add(Value, Step, steps, Min, Max);
        ClearInputError();
        text = _number.Format(stepped, Format, effectiveCulture);
        await SetValueAsync(stepped);
    }

    /// <summary>
    /// Parses text committed from the field.
    /// </summary>
    /// <param name="newText">The text in the field.</param>
    /// <returns>A task that completes once the value has been applied.</returns>
    private async Task OnTextChangedAsync(string newText)
    {
        newText ??= string.Empty;
        if (newText == text)
        {
            return;
        }

        text = newText;
        var committed = BindEvent != "oninput";

        if (string.IsNullOrWhiteSpace(newText))
        {
            ClearInputError();
            if (_number.IsNullable)
            {
                await SetValueAsync(default!);
            }
            else if (committed)
            {
                text = _number.Format(Value, Format, effectiveCulture);
            }

            return;
        }

        if (!_number.TryParse(newText, effectiveCulture, out var parsed))
        {
            if (committed)
            {
                inputError = localizer[TwStrings.Numeric_Invalid];
            }
            else
            {
                pendingInputError = true;
            }

            return;
        }

        ClearInputError();
        var clamped = _number.Clamp(parsed, Min, Max);
        if (committed)
        {
            text = _number.Format(clamped, Format, effectiveCulture);
        }

        await SetValueAsync(clamped);
    }

    /// <summary>
    /// Applies a new value, notifies the consumer and runs <see cref="Validator"/>.
    /// </summary>
    /// <param name="value">The new value, already within <see cref="Min"/> and <see cref="Max"/>.</param>
    /// <returns>A task that completes once the consumer has been notified and the value validated.</returns>
    private async Task SetValueAsync(T value)
    {
        if (EqualityComparer<T>.Default.Equals(value, Value))
        {
            return;
        }

        // Value and lastValue are updated before any await, so a key press queued behind this change steps from the
        // new value, and the parent echoing it back does not overwrite the text the user is typing.
        Value = value;
        lastValue = value;

        if (ValueChanged.HasDelegate)
        {
            await ValueChanged.InvokeAsync(value);
        }

        if (Validator is not null)
        {
            validationError = await Validator(value);
        }
    }

    private void ClearInputError()
    {
        inputError = null;
        pendingInputError = false;
    }

    private async Task OnInputFocusAsync(FocusEventArgs args)
    {
        isFocused = true;
        if (OnFocus.HasDelegate)
        {
            await OnFocus.InvokeAsync(args);
        }
    }

    /// <summary>
    /// Flags text that is not a number, or tidies the text into <see cref="Format"/>, once the user leaves the field.
    /// </summary>
    private async Task OnInputBlurAsync(FocusEventArgs args)
    {
        isFocused = false;
        if (pendingInputError)
        {
            pendingInputError = false;
            inputError = localizer[TwStrings.Numeric_Invalid];
        }
        else if (inputError is null)
        {
            text = _number.Format(Value, Format, effectiveCulture);
        }

        if (OnBlur.HasDelegate)
        {
            await OnBlur.InvokeAsync(args);
        }
    }

    private async Task OnInputKeyDownAsync(KeyboardEventArgs args)
    {
        if (OnKeyDown.HasDelegate)
        {
            await OnKeyDown.InvokeAsync(args);
        }

        if (args.CtrlKey || args.MetaKey || args.AltKey)
        {
            return;
        }

        var steps = args.Key switch
        {
            "ArrowUp" => 1,
            "ArrowDown" => -1,
            "PageUp" => pageSteps,
            "PageDown" => -pageSteps,
            _ => 0
        };

        if (steps != 0)
        {
            await StepAsync(steps);
        }
    }

    /// <summary>
    /// Steps the value while the field has focus: scrolling up increases it, unless <see cref="InvertMouseWheel"/> is set.
    /// </summary>
    private async Task OnWheelAsync(WheelEventArgs args)
    {
        if (!isFocused || DisableMouseWheel || args.DeltaY == 0)
        {
            return;
        }

        var steps = args.DeltaY < 0 ? 1 : -1;
        await StepAsync(InvertMouseWheel ? -steps : steps);
    }

    /// <summary>
    /// Removes the keyboard and mouse wheel listeners from the field.
    /// </summary>
    /// <returns>A task that completes once the listeners are removed.</returns>
    public async ValueTask DisposeAsync()
    {
        if (!jsAttached || textfield is null)
        {
            GC.SuppressFinalize(this);
            return;
        }

        try
        {
            await jsRuntime.InvokeVoidAsync("twNumeric.detach", textfield.InputRef);
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
}
