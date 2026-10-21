// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using TwBlazor.Abstraction;
using TwBlazor.Components;
using TwBlazor.Utilities;

namespace TwBlazor;

/// <summary>
/// Base class for every input component: the label, root element, read-only, disabled and validation state they
/// share, and their connection to the form they sit in.
/// </summary>
/// <remarks>
/// Inside an <see cref="EditForm"/> or a <see cref="TwForm"/>, an input whose value is bound with
/// <c>@bind-Value</c> (or its own value parameter's <c>@bind-</c>) reports each change to the form, so validators
/// such as <see cref="DataAnnotationsValidator"/> run for it, and shows the form's validation message for its field.
/// </remarks>
public class TwBlazorInputComponentBase : TwBlazorComponentBase, ITwInputComponent, ITwFormField, IAsyncDisposable
{
    private TwEditContextField? editContextField;
    private TwForm? registeredForm;

    /// <summary>
    /// Gets or sets the edit context of the enclosing <see cref="EditForm"/> or <see cref="TwForm"/>, if any.
    /// </summary>
    [CascadingParameter] private EditContext? cascadedEditContext { get; set; }

    /// <summary>
    /// Gets or sets the enclosing <see cref="TwForm"/>, if any.
    /// </summary>
    [CascadingParameter] private TwForm? cascadedForm { get; set; }

    /// <summary>
    /// Gets or sets the root of the input this one is part of, such as the text field inside a date picker.
    /// </summary>
    [CascadingParameter] private TwInputRoot? parentInputRoot { get; set; }

    /// <summary>
    /// Gets whether the field connects to the enclosing form itself. A field that is part of another input, such as
    /// the text field inside a date picker, leaves that to the outer input, so the form sees each field once.
    /// </summary>
    protected virtual bool participatesInForm => parentInputRoot is null;

    /// <inheritdoc cref="ITwInputComponent.RootId" />
    [Parameter] public string RootId { get; set; } = string.Empty;

    /// <inheritdoc cref="ITwInputComponent.RootClass" />
    [Parameter] public string RootClass { get; set; } = string.Empty;

    /// <inheritdoc cref="ITwInputComponent.RootAttributes" />
    [Parameter] public Dictionary<string, object> RootAttributes { get; set; } = [];

    /// <inheritdoc cref="ITwInputComponent.Label" />
    [Parameter] public string Label { get; set; } = string.Empty;

    /// <inheritdoc cref="ITwInputComponent.LabelId" />
    [Parameter] public string LabelId { get; set; } = string.Empty;

    /// <inheritdoc cref="ITwInputComponent.LabelAttributes" />
    [Parameter] public Dictionary<string, object> LabelAttributes { get; set; } = [];

    /// <inheritdoc cref="ITwInputComponent.LabelClass" />
    [Parameter] public string LabelClass { get; set; } = string.Empty;

    /// <inheritdoc cref="ITwInputComponent.ReadOnly" />
    [Parameter] public bool ReadOnly { get; set; }

    /// <inheritdoc cref="ITwInputComponent.Disabled" />
    [Parameter] public bool Disabled { get; set; }

    /// <inheritdoc cref="ITwInputComponent.Invalid" />
    [Parameter] public bool Invalid { get; set; }

    /// <inheritdoc cref="ITwInputComponent.ErrorMessage" />
    [Parameter] public string ErrorMessage { get; set; } = string.Empty;

    /// <inheritdoc cref="ITwInputComponent.LabelClasses" />
    public string LabelClasses => new ClassBuilder(LabelClass ?? string.Empty).Build();

    /// <summary>
    /// Gets the id of the rendered error message element when the field is invalid, or null otherwise.
    /// Leaf components use this both to render the error text (via TwInputRoot) and to reference it from
    /// the control's aria-describedby, so assistive tech announces the error alongside the field.
    /// </summary>
    protected string? errorId => fieldInvalid && !string.IsNullOrWhiteSpace(fieldErrorMessage) ? $"{Id}-error" : null;

    /// <summary>
    /// Gets whether the field is invalid: <see cref="Invalid"/> is set, or the enclosing form has a validation
    /// message for the field.
    /// </summary>
    protected bool fieldInvalid => Invalid || editContextField?.FirstMessage is not null;

    /// <summary>
    /// Gets the message shown under the field: <see cref="ErrorMessage"/> while <see cref="Invalid"/> is set,
    /// otherwise the enclosing form's first validation message for the field.
    /// </summary>
    protected string fieldErrorMessage => Invalid && !string.IsNullOrWhiteSpace(ErrorMessage)
        ? ErrorMessage
        : editContextField?.FirstMessage ?? ErrorMessage;

    /// <summary>
    /// Gets the model property the field is bound to in the enclosing form, or <see langword="null"/> when it has
    /// no form or its value is not bound to a property.
    /// </summary>
    protected FieldIdentifier? fieldIdentifier => editContextField?.Field;

    /// <summary>
    /// Creates the identifier of the model property the field's value is bound to. Input components override
    /// this to build it from their value expression (supplied by <c>@bind-Value</c>).
    /// </summary>
    /// <returns>The field identifier, or <see langword="null"/> when the value is not bound to a property.</returns>
    protected virtual FieldIdentifier? CreateFieldIdentifier() => null;

    /// <summary>
    /// Tells the enclosing form that the field's value changed, so its validators run for the field. Does nothing
    /// outside a form.
    /// </summary>
    protected void NotifyFieldChanged() => editContextField?.NotifyChanged();

    /// <summary>
    /// Validates the field when its <see cref="TwForm"/> is submitted or validated. Components with a validator of
    /// their own override this to run it first.
    /// </summary>
    /// <returns><see langword="true"/> when the field is valid.</returns>
    public virtual Task<bool> ValidateAsync() => Task.FromResult(!fieldInvalid);

    /// <inheritdoc />
    FieldIdentifier? ITwFormField.BoundField => fieldIdentifier;

    /// <inheritdoc />
    string? ITwFormField.OwnErrorMessage => ownErrorMessage;

    /// <summary>
    /// Gets the field's own error message while it is invalid for a reason the form does not know about, such as a
    /// failed validator or text that could not be read, or <see langword="null"/>. Listed in the form's error summary.
    /// </summary>
    protected virtual string? ownErrorMessage => Invalid && !string.IsNullOrWhiteSpace(ErrorMessage) ? ErrorMessage : null;

    /// <summary>
    /// Releases the field's connection to its form.
    /// </summary>
    /// <returns>A task that completes once the field is released.</returns>
    public virtual ValueTask DisposeAsync()
    {
        editContextField?.Dispose();
        registeredForm?.Unregister(this);
        registeredForm = null;
        GC.SuppressFinalize(this);
        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// Closes the component's picker dialog. Override in picker components to handle close logic.
    /// </summary>
    [Microsoft.JSInterop.JSInvokable("Close")]
    public virtual Task Close() => Task.CompletedTask;

    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        // If the consumer passed Attributes (unmatched values) on the input component itself,
        // prefer forwarding them to the RootAttributes so they end up on the TwInputRoot element.
        if ((RootAttributes == null || RootAttributes.Count == 0) && Attributes != null && Attributes.Count > 0)
        {
            RootAttributes = Attributes;
        }

        var editContext = participatesInForm ? cascadedEditContext : null;
        if (editContext is not null || editContextField is not null)
        {
            editContextField ??= new TwEditContextField(() => _ = InvokeAsync(StateHasChanged));
            editContextField.Attach(editContext, CreateFieldIdentifier);
        }

        var form = participatesInForm ? cascadedForm : null;
        if (!ReferenceEquals(form, registeredForm))
        {
            registeredForm?.Unregister(this);
            registeredForm = form;
            registeredForm?.Register(this);
        }
    }
}
