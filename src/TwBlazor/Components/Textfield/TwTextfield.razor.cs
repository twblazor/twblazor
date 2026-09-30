// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using TwBlazor.Builders;
using TwBlazor.Configuration.Components;
using TwBlazor.Utilities;

namespace TwBlazor.Components;

/// <summary>
/// Represents a generic text input field component that supports two-way data binding.
/// </summary>
/// <typeparam name="T">The type of value bound to the text field. Common types include string, int, decimal, and DateTime.</typeparam>
/// <remarks>
/// The TwTextfield component is a flexible input control that supports various input types (text, password, email, number, etc.)
/// and provides customizable styling for both the input field and its label. It implements two-way binding through the
/// <see cref="Value"/> and <see cref="ValueChanged"/> parameters, allowing seamless integration with Blazor's @bind directive.
/// The component can be configured to trigger data binding on different DOM events using the <see cref="BindEvent"/> parameter.
/// </remarks>
public partial class TwTextfield<T> : TwBlazorTextInputComponentBase
{
    private const string emailInputType = "email";

    private ElementReference inputRef;

    /// <summary>
    /// Reference to the rendered &lt;input&gt; element itself. Lets consumers (e.g. picker components
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
    /// Gets or sets the callback that is invoked when the textfield is focused.
    /// </summary>
    [Parameter] public EventCallback<FocusEventArgs> OnFocus { get; set; } = default!;

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
    /// Gets the CSS classes applied to the input element.
    /// </summary>
    /// <remarks>
    /// Includes default styling for colors, borders, sizing, padding, shadows, and placeholder text.
    /// Custom classes specified in <see cref="TwBlazorComponentBase.Class"/> are also included.
    /// </remarks>
    private string classes => new ClassBuilder(theme.TextfieldBase)
        .AddClass(inputVariantBuilder.GetClasses(effectiveVariant, theme))
        .AddClass(Disabled ? "opacity-40 cursor-not-allowed" : string.Empty)
        .AddClass(inputSizeClasses)
        .AddClass(Class)
        .Build();

    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        if (string.IsNullOrEmpty(RootId))
        {
            RootId = Guid.NewGuid().ToString("N");
        }

        if (Disabled && !Attributes.ContainsKey("disabled"))
        {
            Attributes["disabled"] = true;
        }

        if (ReadOnly && !Attributes.ContainsKey("readonly"))
        {
            Attributes["readonly"] = true;
        }

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

        var autoComplete = GetAutoCompleteForInputType(InputType);
        if (autoComplete is not null && !Attributes.ContainsKey("autocomplete"))
        {
            Attributes["autocomplete"] = autoComplete;
        }

        var inputMode = GetInputModeForInputType(InputType);
        if (inputMode is not null && !Attributes.ContainsKey("inputmode"))
        {
            Attributes["inputmode"] = inputMode;
        }
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

        if (Validator is not null)
        {
            var error = await Validator(value);
            Invalid = !string.IsNullOrEmpty(error);
            ErrorMessage = error ?? string.Empty;
        }
    }
}
