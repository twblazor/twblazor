// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;
using System.Globalization;
using System.Linq.Expressions;
using TwBlazor.Builders;
using TwBlazor.Configuration.Components;
using TwBlazor.Enums;
using TwBlazor.Utilities;

namespace TwBlazor.Components;

/// <summary>
/// Represents a generic text input field component that supports two-way data binding.
/// </summary>
/// <typeparam name="T">The type of value bound to the text field. Common types include string, int, decimal, and DateTime.</typeparam>
/// <remarks>
/// The TwTextfield component is a flexible input control that supports various input types (text, password, email, number, etc.)
/// and provides customizable styling for both the input field and its label. Set <see cref="Multiline"/> to render a
/// &lt;textarea&gt; for longer, multi-line text instead. It implements two-way binding through the
/// <see cref="Value"/> and <see cref="ValueChanged"/> parameters, allowing seamless integration with Blazor's @bind directive.
/// The component can be configured to trigger data binding on different DOM events using the <see cref="BindEvent"/> parameter.
/// </remarks>
public partial class TwTextfield<T> : TwBlazorTextInputComponentBase
{
    private const string emailInputType = "email";

    private ElementReference inputRef;

    /// <summary>
    /// Reference to the rendered &lt;input&gt; (or &lt;textarea&gt; when <see cref="Multiline"/> is set) element itself. Lets consumers (e.g. picker components
    /// with a decorative trigger icon that sits ahead of the input in DOM order) focus this specific
    /// element directly, rather than searching a wider container where the first focusable descendant
    /// found might not be this input.
    /// </summary>
    public ElementReference InputRef => inputRef;

    [Inject] private InputVariantBuilder inputVariantBuilder { get; set; } = null!;

    private TwInputTheme theme => options.Theme.Components.Require<TwInputTheme>();

    /// <summary>
    /// Gets or sets the current value of the text field.
    /// </summary>
    /// <remarks>
    /// This property works in conjunction with <see cref="ValueChanged"/> to support two-way binding.
    /// The type parameter T determines what type of value the text field accepts and emits.
    /// </remarks>
    [Parameter] public T Value { get; set; } = default!;

    /// <summary>
    /// Gets or sets the callback that is invoked when the value of the text field changes.
    /// </summary>
    /// <remarks>
    /// This callback is invoked based on the event specified by <see cref="BindEvent"/> (default is "onchange").
    /// It enables two-way data binding when used with the @bind-Value directive in Blazor.
    /// </remarks>
    [Parameter] public EventCallback<T> ValueChanged { get; set; } = default!;

    /// <summary>
    /// Gets or sets the expression that identifies the bound value. Set automatically by <c>@bind-Value</c>, it
    /// lets an enclosing <see cref="EditForm"/> or <see cref="TwForm"/> validate the property the field edits.
    /// </summary>
    [Parameter] public Expression<Func<T>>? ValueExpression { get; set; }

    /// <summary>
    /// Gets or sets the callback that is invoked when the textfield is focused.
    /// </summary>
    [Parameter] public EventCallback<FocusEventArgs> OnFocus { get; set; } = default!;

    /// <summary>
    /// Gets or sets the callback that is invoked when the textfield loses focus.
    /// </summary>
    [Parameter] public EventCallback<FocusEventArgs> OnBlur { get; set; }

    /// <summary>
    /// Gets or sets the callback invoked when the input is clicked.
    /// </summary>
    [Parameter] public EventCallback<MouseEventArgs> OnClick { get; set; }

    /// <summary>
    /// Gets or sets the callback invoked when a key is pressed while the input has focus.
    /// </summary>
    [Parameter] public EventCallback<KeyboardEventArgs> OnKeyDown { get; set; }

    /// <summary>
    /// Gets or sets an optional asynchronous validator invoked with the new value whenever it changes.
    /// </summary>
    /// <remarks>
    /// Return <see langword="null"/> or an empty string to mark the value valid. Return a non-empty
    /// string to mark the field invalid (see <see cref="TwBlazorInputComponentBase.Invalid"/>) and
    /// display it as the error message below the input (see <see cref="TwBlazorInputComponentBase.ErrorMessage"/>).
    /// The validator runs after <see cref="ValueChanged"/> is invoked, so consumers of this callback see
    /// the raw value before validation has been applied. Because the validator returns a
    /// <see cref="Task{TResult}"/>, it can perform asynchronous work such as an API call (e.g. checking
    /// whether a username is already taken) without blocking the UI thread.
    /// </remarks>
    [Parameter] public Func<T, Task<string?>>? Validator { get; set; }

    /// <summary>
    /// Gets or sets the name of the DOM event that triggers data binding for the associated element.
    /// </summary>
    /// <remarks>The default value is "onchange", which binds on input changes. Set this property to a
    /// different event name (such as "oninput") to change when data binding occurs. The value should correspond to a
    /// valid DOM event for the element.</remarks>
    [Parameter] public string BindEvent { get; set; } = "onchange";

    /// <summary>
    /// Gets or sets the type of input expected by the control.
    /// </summary>
    /// <remarks>Common values include "text", "password", "email", and "number". The value determines how the
    /// input is rendered and what kind of data is accepted.</remarks>
    [Parameter] public string InputType { get; set; } = "text";

    /// <summary>
    /// Gets or sets the placeholder text displayed when the input field is empty.
    /// </summary>
    [Parameter] public string Placeholder { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets whether a value is required. The field is marked required for the browser's own
    /// validation and announced as required by assistive technology.
    /// </summary>
    [Parameter] public bool Required { get; set; }

    /// <summary>
    /// Gets or sets whether the field accepts multiple lines of text, rendering a &lt;textarea&gt; instead of an &lt;input&gt;.
    /// </summary>
    /// <remarks>
    /// <see cref="InputType"/> is ignored while this is set. <kbd>Enter</kbd> inserts a new line rather than
    /// submitting the form, and <kbd>Tab</kbd> still moves focus out of the field.
    /// </remarks>
    [Parameter] public bool Multiline { get; set; }

    /// <summary>
    /// Gets or sets the number of visible lines of text in a <see cref="Multiline"/> field. Defaults to 3.
    /// </summary>
    /// <remarks>
    /// With <see cref="AutoGrow"/> set, this is the minimum height the field starts at before growing.
    /// </remarks>
    [Parameter] public int Rows { get; set; } = 3;

    /// <summary>
    /// Gets or sets whether a <see cref="Multiline"/> field grows taller as the user types instead of scrolling.
    /// </summary>
    /// <remarks>
    /// Uses the CSS <c>field-sizing</c> property, so in browsers without support the field keeps its
    /// <see cref="Rows"/> height and scrolls. Manual resizing is turned off while this is set.
    /// </remarks>
    [Parameter] public bool AutoGrow { get; set; }

    /// <summary>
    /// Gets or sets the most lines an <see cref="AutoGrow"/> field grows to before it scrolls.
    /// <see langword="null"/> (the default) lets it grow without limit.
    /// </summary>
    [Parameter] public int? MaxRows { get; set; }

    /// <summary>
    /// Gets or sets whether a button that empties the field is shown at its end. It is disabled while the field is empty.
    /// </summary>
    /// <remarks>
    /// Clearing sets <see cref="Value"/> to its default (an empty string becomes <see langword="null"/>), invokes
    /// <see cref="ValueChanged"/> and <see cref="Validator"/>, then <see cref="OnClear"/>, and moves focus back to the
    /// field. The button is not a Tab stop, since keyboard users can clear the text themselves, and it is hidden while
    /// the field is disabled or read-only. Not shown on a <see cref="Multiline"/> field.
    /// </remarks>
    [Parameter] public bool Clearable { get; set; }

    /// <summary>
    /// Gets or sets the callback invoked after the clear button (see <see cref="Clearable"/>) has emptied the field.
    /// </summary>
    [Parameter] public EventCallback OnClear { get; set; }

    /// <summary>
    /// Gets or sets content drawn over the start of a single-line field, such as an icon or a currency symbol.
    /// </summary>
    /// <remarks>
    /// The content is not part of the value, and the field's padding is not widened for it: add start padding through
    /// <see cref="TwBlazorComponentBase.Class"/> or a <c>style</c> attribute so the text does not run underneath it.
    /// Hide decorative content from screen readers with <c>aria-hidden="true"</c>.
    /// </remarks>
    [Parameter] public RenderFragment? StartContent { get; set; }

    /// <summary>
    /// Gets or sets content drawn over the end of a single-line field, before the clear button, such as a unit or
    /// extra buttons.
    /// </summary>
    /// <remarks>
    /// Its items are laid out in a row 0.25rem apart. The field's padding is not widened for it: add end padding through
    /// <see cref="TwBlazorComponentBase.Class"/> or a <c>style</c> attribute so the text does not run underneath it.
    /// </remarks>
    [Parameter] public RenderFragment? EndContent { get; set; }

    private bool hasFieldContent => Clearable || StartContent is not null || EndContent is not null;

    // Always shown, and disabled while empty, so the buttons beside it do not shift under the pointer when it appears.
    private bool showClearButton => Clearable && !Disabled && !ReadOnly;

    private bool hasValue => Value is string text ? text.Length > 0 : !EqualityComparer<T>.Default.Equals(Value, default!);

    private string startContentClasses => new ClassBuilder(theme.FieldStartContent).AddClass(inputSizeClasses).Build();

    private string endContentClasses => new ClassBuilder(theme.FieldEndContent).AddClass(inputSizeClasses).Build();

    // The start content sits where the variant's own padding would start the text: flush for Default, which has none.
    private string startContentStyle => effectiveVariant == InputVariant.Default ? "padding-inline-start: 0px;" : "padding-inline-start: 0.75rem;";

    /// <summary>
    /// Gets the CSS classes applied to the input element.
    /// </summary>
    /// <remarks>
    /// Includes default styling for colors, borders, sizing, padding, shadows, and placeholder text.
    /// Custom classes specified in <see cref="TwBlazorComponentBase.Class"/> are also included.
    /// </remarks>
    private string classes => new ClassBuilder(theme.TextfieldBase)
        .AddClass(inputVariantBuilder.GetClasses(effectiveVariant, theme, effectiveRounded))
        .AddClass(Disabled ? "opacity-40 cursor-not-allowed" : string.Empty)
        .AddClass(Multiline ? multilineClasses : inputSizeClasses)
        .AddClass(theme.ClearablePadding, Clearable && !Multiline && !Disabled && !ReadOnly)
        .AddClass(Class)
        .Build();

    private string multilineClasses => new ClassBuilder(Dense ? theme.MultilineDenseSize : theme.MultilineSize)
        .AddClass(AutoGrow ? theme.MultilineAutoGrow : theme.MultilineResize)
        .Build();

    /// <summary>
    /// Gets the inline sizing for an <see cref="AutoGrow"/> field, or <see langword="null"/> when none applies.
    /// </summary>
    /// <remarks>
    /// <c>field-sizing: content</c> ignores the <c>rows</c> attribute, so the minimum and maximum heights are
    /// given in lines (<c>lh</c>) plus the theme's vertical padding instead.
    /// </remarks>
    private string? multilineStyle
    {
        get
        {
            if (!AutoGrow)
            {
                return null;
            }

            var style = $"min-block-size: {GetRowsHeight(Rows)};";
            if (MaxRows is int maxRows)
            {
                style += $" max-block-size: {GetRowsHeight(Math.Max(maxRows, Rows))};";
            }

            return style;
        }
    }

    private static string GetRowsHeight(int rows) => string.Create(CultureInfo.InvariantCulture, $"calc({Math.Max(rows, 1)}lh + 2 * var(--textarea-py, 0px))");

    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        if (string.IsNullOrEmpty(RootId))
        {
            RootId = Guid.NewGuid().ToString("N");
        }

        // The attributes dictionary can outlive one parameter set, so anything this component adds to it is
        // taken out again once the parameter is switched off. An attribute the consumer supplied is left alone.
        disabledAdded = SyncAttribute("disabled", Disabled, disabledAdded);
        readOnlyAdded = SyncAttribute("readonly", ReadOnly, readOnlyAdded);

        // A field with no "name" is omitted from form submissions and gives browsers nothing to key
        // autofill/password-manager suggestions on, so it defaults to Id (already unique per instance)
        // unless the consumer supplied their own. Likewise, "autocomplete"/"inputmode" default to the
        // token that matches InputType (e.g. type="email" implies autocomplete="email" and a matching
        // mobile keyboard) only when there's an unambiguous standard mapping - both stay fully
        // overridable via Attributes.
        if (!Attributes.ContainsKey("name") && !string.IsNullOrEmpty(Id))
        {
            Attributes["name"] = Id;
        }

        autoCompleteAdded = SyncAttribute("autocomplete", Multiline ? null : GetAutoCompleteForInputType(InputType), autoCompleteAdded);
        inputModeAdded = SyncAttribute("inputmode", Multiline ? null : GetInputModeForInputType(InputType), inputModeAdded);
    }

    private bool disabledAdded;
    private bool readOnlyAdded;
    private bool autoCompleteAdded;
    private bool inputModeAdded;

    /// <summary>
    /// Adds or removes a boolean attribute the component manages, leaving one the consumer supplied untouched.
    /// </summary>
    /// <param name="name">The attribute name.</param>
    /// <param name="enabled">Whether the attribute should be present.</param>
    /// <param name="addedByComponent">Whether the component added the attribute on a previous parameter set.</param>
    /// <returns>Whether the component now owns the attribute.</returns>
    private bool SyncAttribute(string name, bool enabled, bool addedByComponent) => SyncAttribute(name, enabled ? true : null, addedByComponent);

    /// <summary>
    /// Sets or removes an attribute the component manages, leaving one the consumer supplied untouched.
    /// </summary>
    /// <param name="name">The attribute name.</param>
    /// <param name="value">The value to set, or <see langword="null"/> to remove the attribute.</param>
    /// <param name="addedByComponent">Whether the component added the attribute on a previous parameter set.</param>
    /// <returns>Whether the component now owns the attribute.</returns>
    private bool SyncAttribute(string name, object? value, bool addedByComponent)
    {
        if (value is not null)
        {
            if (Attributes.ContainsKey(name) && !addedByComponent)
            {
                return false;
            }

            Attributes[name] = value;
            return true;
        }

        if (addedByComponent)
        {
            Attributes.Remove(name);
        }

        return false;
    }

    /// <summary>
    /// Maps an HTML input <c>type</c> to the "autocomplete" token browsers use to key
    /// autofill/password-manager suggestions, for the subset of types with an unambiguous standard
    /// mapping. Returns <see langword="null"/> for types (like "text" or "password", whose correct
    /// autocomplete token depends on context this component can't know) where no safe default applies.
    /// </summary>
    private static string? GetAutoCompleteForInputType(string inputType) => inputType switch
    {
        emailInputType => emailInputType,
        "tel" => "tel",
        "url" => "url",
        _ => null
    };

    /// <summary>
    /// Maps an HTML input <c>type</c> to the "inputmode" token that requests the matching virtual
    /// keyboard on mobile devices.
    /// </summary>
    private static string? GetInputModeForInputType(string inputType) => inputType switch
    {
        emailInputType => emailInputType,
        "tel" => "tel",
        "url" => "url",
        "search" => "search",
        "number" => "decimal",
        _ => null
    };

    /// <summary>
    /// Empties the field, notifies the consumer, and moves focus back to the field.
    /// </summary>
    /// <returns>A task that completes once the field has been cleared.</returns>
    public async Task ClearAsync()
    {
        if (Disabled || ReadOnly)
        {
            return;
        }

        await OnValueChanged(default!);

        if (OnClear.HasDelegate)
        {
            await OnClear.InvokeAsync();
        }

        try
        {
            await inputRef.FocusAsync();
        }
        catch (InvalidOperationException)
        {
            // The element is not rendered yet (prerendering); there is nothing to focus.
        }
    }

    /// <summary>
    /// Handles value changes from the input element and invokes the <see cref="ValueChanged"/> callback.
    /// </summary>
    /// <param name="value">The new value from the input element.</param>
    /// <remarks>
    /// This method updates the <see cref="Value"/> property and invokes the <see cref="ValueChanged"/> callback
    /// if it has a delegate. This enables the component to participate in two-way binding scenarios.
    /// </remarks>
    /// <returns>A task that represents the asynchronous operation.</returns>
    private async Task OnValueChanged(T value)
    {
        Value = value;
        if (ValueChanged.HasDelegate)
        {
            await ValueChanged.InvokeAsync(value);
        }

        NotifyFieldChanged();
        await RunValidatorAsync(value);
    }

    private async Task RunValidatorAsync(T value)
    {
        if (Validator is not null)
        {
            var error = await Validator(value);
            Invalid = !string.IsNullOrEmpty(error);
            ErrorMessage = error ?? string.Empty;
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
        await RunValidatorAsync(Value);
        await InvokeAsync(StateHasChanged);
        return await base.ValidateAsync();
    }
}
