// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using Microsoft.AspNetCore.Components.Forms;

namespace TwBlazor.Utilities;

/// <summary>
/// Connects one input to the <see cref="EditContext"/> of the form it sits in: it reports value changes to the
/// form, reads back the form's validation messages for the field, and asks the input to render again whenever the
/// form's validation state changes.
/// </summary>
/// <remarks>
/// An input with no enclosing form, or one whose value is not bound to a model property (so it has no
/// <see cref="FieldIdentifier"/>), is left alone: it reports nothing and has no messages.
/// </remarks>
internal sealed class TwEditContextField : IDisposable
{
    private readonly Action _onValidationStateChanged;

    /// <summary>
    /// Initializes a new instance of the <see cref="TwEditContextField"/> class.
    /// </summary>
    /// <param name="onValidationStateChanged">Called when the form's validation state changes, so the input can render again.</param>
    public TwEditContextField(Action onValidationStateChanged)
    {
        _onValidationStateChanged = onValidationStateChanged;
    }

    /// <summary>
    /// Gets the form the input is attached to, if any.
    /// </summary>
    public EditContext? EditContext { get; private set; }

    /// <summary>
    /// Gets the model property the input's value is bound to, if any.
    /// </summary>
    public FieldIdentifier? Field { get; private set; }

    /// <summary>
    /// Gets whether the input is attached to a form and bound to one of its model's properties.
    /// </summary>
    public bool IsBound => EditContext is not null && Field is not null;

    /// <summary>
    /// Gets the form's validation messages for the field. Empty when the input is not bound.
    /// </summary>
    public IEnumerable<string> Messages
    {
        get
        {
            if (EditContext is { } context && Field is { } boundField)
            {
                return context.GetValidationMessages(boundField);
            }

            return Enumerable.Empty<string>();
        }
    }

    /// <summary>
    /// Gets the first of the form's validation messages for the field, or <see langword="null"/> when there are none.
    /// </summary>
    public string? FirstMessage => Messages.FirstOrDefault();

    /// <summary>
    /// Attaches the input to a form, or detaches it when <paramref name="editContext"/> is <see langword="null"/>.
    /// </summary>
    /// <param name="editContext">The enclosing form's edit context.</param>
    /// <param name="createField">Creates the field identifier from the input's value expression. Only called when the form changes.</param>
    public void Attach(EditContext? editContext, Func<FieldIdentifier?> createField)
    {
        if (ReferenceEquals(editContext, EditContext))
        {
            // A value expression can be supplied after the first render, so a missing field is tried again.
            if (EditContext is not null && Field is null)
            {
                Field = createField();
            }

            return;
        }

        Detach();

        if (editContext is null)
        {
            return;
        }

        EditContext = editContext;
        Field = createField();
        editContext.OnValidationStateChanged += HandleValidationStateChanged;
    }

    /// <summary>
    /// Tells the form that the field's value changed, which runs its validators for that field.
    /// </summary>
    public void NotifyChanged()
    {
        if (EditContext is { } context && Field is { } boundField)
        {
            context.NotifyFieldChanged(boundField);
        }
    }

    /// <summary>
    /// Stops listening to the form.
    /// </summary>
    public void Dispose() => Detach();

    private void Detach()
    {
        if (EditContext is { } context)
        {
            context.OnValidationStateChanged -= HandleValidationStateChanged;
        }

        EditContext = null;
        Field = null;
    }

    private void HandleValidationStateChanged(object? sender, ValidationStateChangedEventArgs e) => _onValidationStateChanged();
}
