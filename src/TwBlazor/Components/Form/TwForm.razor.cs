// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using TwBlazor.Abstraction;
using TwBlazor.Configuration.Components;
using TwBlazor.Utilities;

namespace TwBlazor.Components;

/// <summary>
/// A form that validates the TwBlazor fields inside it: the model's data annotations, plus each field's own
/// <c>Validator</c>, when it is submitted.
/// </summary>
/// <remarks>
/// <para>
/// Bind each field to the model with <c>@bind-Value</c> (or the field's own value parameter, such as
/// <c>@bind-SelectedValue</c>). The field then shows the form's validation message for its property, is marked
/// invalid for assistive technology, and is checked again whenever it changes.
/// </para>
/// <para>
/// <see cref="OnValidSubmit"/> only runs once every field is valid. After a failed submit, focus moves to the
/// error summary when <see cref="ShowSummary"/> is set, otherwise to the first invalid field, so keyboard and screen
/// reader users land on the problem. TwBlazor fields also work in a plain <see cref="EditForm"/>.
/// </para>
/// </remarks>
public partial class TwForm : TwBlazorComponentBase
{
    private readonly List<ITwFormField> _fields = [];
    private EditContext editContext = null!;
    private bool focusPending;

    [Inject] private IJSRuntime jsRuntime { get; set; } = null!;

    private TwFormTheme theme => options.Theme.Components.Require<TwFormTheme>();

    /// <summary>
    /// Gets or sets the object the form edits. Set this or <see cref="EditContext"/>, not both.
    /// </summary>
    [Parameter] public object? Model { get; set; }

    /// <summary>
    /// Gets or sets the edit context the form uses, for full control over validation. Set this or
    /// <see cref="Model"/>, not both.
    /// </summary>
    [Parameter] public EditContext? EditContext { get; set; }

    /// <summary>
    /// Gets or sets the form's fields and buttons. The edit context is passed in as <c>context</c>.
    /// </summary>
    [Parameter] public RenderFragment<EditContext>? ChildContent { get; set; }

    /// <summary>
    /// Raised when the form is submitted, after it has been validated, whether or not it is valid.
    /// </summary>
    [Parameter] public EventCallback<EditContext> OnSubmit { get; set; }

    /// <summary>
    /// Raised when the form is submitted and every field is valid.
    /// </summary>
    [Parameter] public EventCallback<EditContext> OnValidSubmit { get; set; }

    /// <summary>
    /// Raised when the form is submitted and at least one field is invalid.
    /// </summary>
    [Parameter] public EventCallback<EditContext> OnInvalidSubmit { get; set; }

    /// <summary>
    /// Gets or sets whether the model's data annotations, such as <c>[Required]</c>, are checked. Defaults to
    /// <see langword="true"/>.
    /// </summary>
    [Parameter] public bool DataAnnotationsValidation { get; set; } = true;

    /// <summary>
    /// Gets or sets whether a summary of every error is shown above the fields after a failed submit. Each error
    /// links to its field.
    /// </summary>
    [Parameter] public bool ShowSummary { get; set; }

    /// <summary>
    /// Gets or sets whether focus moves to the error summary, or to the first invalid field, after a failed
    /// submit. Defaults to <see langword="true"/>.
    /// </summary>
    [Parameter] public bool FocusOnError { get; set; } = true;

    /// <summary>
    /// Gets or sets whether the browser's own validation runs as well. Off by default, so the browser's error
    /// bubbles do not stop the form showing its own accessible messages.
    /// </summary>
    [Parameter] public bool NativeValidation { get; set; }

    /// <summary>
    /// Gets or sets the form's name, which a statically rendered form needs so its posted values can be found.
    /// </summary>
    [Parameter] public string? FormName { get; set; }

    /// <summary>
    /// Gets or sets whether a statically rendered form is posted with enhanced navigation.
    /// </summary>
    [Parameter] public bool Enhance { get; set; }

    /// <summary>
    /// Gets the edit context the form is using.
    /// </summary>
    public EditContext CurrentEditContext => editContext;

    /// <summary>
    /// Gets whether the last validation found every field valid. <see langword="false"/> until the form has
    /// been validated.
    /// </summary>
    public bool IsValid { get; private set; }

    /// <summary>
    /// Gets whether any field has changed since the form was created.
    /// </summary>
    public bool IsModified => editContext.IsModified();

    /// <summary>
    /// Gets whether the form has been submitted or validated at least once.
    /// </summary>
    public bool HasValidated { get; private set; }

    /// <summary>
    /// Raised when the form validates, so its error summary can show the new state.
    /// </summary>
    internal event Action? Validated;

    /// <summary>
    /// Gets the id of the error summary.
    /// </summary>
    internal string summaryId => $"{Id}-summary";

    /// <summary>
    /// Gets the theme for the form and its summary.
    /// </summary>
    internal TwFormTheme formTheme => theme;

    private string classes => new ClassBuilder(theme.Base).AddClass(Class).Build();

    private RenderFragment renderSummary => builder =>
    {
        builder.OpenComponent<TwFormSummary>(0);
        builder.AddComponentParameter(1, nameof(TwFormSummary.Form), this);
        builder.CloseComponent();
    };

    /// <inheritdoc />
    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        if (EditContext is not null && Model is not null)
        {
            throw new InvalidOperationException($"{nameof(TwForm)} requires a {nameof(Model)} or an {nameof(EditContext)} parameter, but not both.");
        }

        if (EditContext is not null)
        {
            editContext = EditContext;
        }
        else if (Model is not null)
        {
            if (editContext is null || !ReferenceEquals(editContext.Model, Model))
            {
                editContext = new EditContext(Model);
            }
        }
        else
        {
            throw new InvalidOperationException($"{nameof(TwForm)} requires a {nameof(Model)} or an {nameof(EditContext)} parameter.");
        }
    }

    /// <summary>
    /// Validates the model and every field in the form, running each field's own validator too.
    /// </summary>
    /// <returns><see langword="true"/> when every field is valid.</returns>
    public async Task<bool> ValidateAsync()
    {
        var valid = editContext.Validate();

        // Every field runs, even after one fails, so all of their messages show at once.
        foreach (var field in _fields.ToList())
        {
            valid &= await field.ValidateAsync();
        }

        IsValid = valid;
        HasValidated = true;
        Validated?.Invoke();
        return valid;
    }

    /// <summary>
    /// Gets the errors the summary lists, in the order the fields appear, each with the id of the field it
    /// links to when there is one.
    /// </summary>
    /// <returns>The messages and the ids of their fields.</returns>
    internal IReadOnlyList<(string Message, string? FieldId)> GetSummaryEntries()
    {
        List<(string Message, string? FieldId)> entries = [];
        List<string> linked = [];

        foreach (var field in _fields)
        {
            if (field.BoundField is { } bound)
            {
                foreach (var message in editContext.GetValidationMessages(bound))
                {
                    entries.Add((message, field.Id));
                    linked.Add(message);
                }
            }

            if (field.OwnErrorMessage is { } own && !entries.Contains((own, field.Id)))
            {
                entries.Add((own, field.Id));
            }
        }

        // Messages for properties no field is bound to are still listed, without a link.
        foreach (var message in editContext.GetValidationMessages())
        {
            if (!linked.Remove(message))
            {
                entries.Add((message, null));
            }
        }

        return entries;
    }

    /// <summary>
    /// Adds a field to the form, so it is validated on submit.
    /// </summary>
    /// <param name="field">The field.</param>
    internal void Register(ITwFormField field)
    {
        if (!_fields.Contains(field))
        {
            _fields.Add(field);
        }
    }

    /// <summary>
    /// Removes a field from the form.
    /// </summary>
    /// <param name="field">The field.</param>
    internal void Unregister(ITwFormField field) => _fields.Remove(field);

    /// <summary>
    /// Moves focus to a field the error summary links to.
    /// </summary>
    /// <param name="fieldId">The field's id.</param>
    /// <returns>A task that completes once focus has moved.</returns>
    internal Task FocusFieldAsync(string fieldId) => InvokeScriptAsync("twForm.focusField", fieldId);

    private async Task HandleSubmitAsync(EditContext context)
    {
        var valid = await ValidateAsync();

        await OnSubmit.InvokeAsync(context);

        if (valid)
        {
            await OnValidSubmit.InvokeAsync(context);
            return;
        }

        await OnInvalidSubmit.InvokeAsync(context);

        if (FocusOnError)
        {
            // Focus moves once the fields have rendered as invalid.
            focusPending = true;
            StateHasChanged();
        }
    }

    /// <inheritdoc />
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);

        if (!focusPending)
        {
            return;
        }

        focusPending = false;

        if (ShowSummary && GetSummaryEntries().Count > 0)
        {
            await InvokeScriptAsync("twFocus.focusById", summaryId);
        }
        else
        {
            await InvokeScriptAsync("twForm.focusFirstInvalid", Id);
        }
    }

    private async Task InvokeScriptAsync(string identifier, params object?[] args)
    {
        try
        {
            await jsRuntime.InvokeVoidAsync(identifier, args);
        }
        catch (JSDisconnectedException)
        {
            // The circuit disconnected before the script could run.
        }
        catch (InvalidOperationException)
        {
            // JS interop is not available (prerendering).
        }
    }
}
