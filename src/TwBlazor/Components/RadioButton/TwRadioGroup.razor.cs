// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using System.Linq.Expressions;
using TwBlazor.Abstraction;
using TwBlazor.Configuration.Components;
using TwBlazor.Enums;
using TwBlazor.Models;
using TwBlazor.Utilities;

namespace TwBlazor.Components;

/// <summary>
/// Represents a container component for grouping multiple <see cref="TwRadioButton{T}"/> components together.
/// </summary>
/// <typeparam name="T">The type of value in the radio group.</typeparam>
/// <remarks>
/// TwRadioGroup provides two ways to use radio groups:
/// 1. Manual: Add TwRadioButton components as children (ChildContent)
/// 2. Automatic: Provide Items and bind to Value for automatic radio button generation
/// Radio buttons within the group should share the same Name parameter to form a mutually exclusive group.
/// The group can be oriented horizontally or vertically, and supports customizable spacing.
/// The component supports cascading the Name to child radio buttons.
/// </remarks>
/// <remarks>
/// Accessibility: the rendered &lt;fieldset&gt; only has an accessible name when at least one of
/// <see cref="Legend"/>, <see cref="TwBlazorComponentBase.AriaLabel"/>, or
/// <see cref="TwBlazorComponentBase.AriaLabelledBy"/> is supplied. Always set one of these so
/// screen-reader users hear what the options relate to, not just the individual option labels.
/// </remarks>
public partial class TwRadioGroup<T> : TwBlazorComponentBase, ITwFormField, IAsyncDisposable
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

    private TwGroupsTheme groupsTheme => options.Theme.Components.Require<TwGroupsTheme>();
    private TwInputTheme inputTheme => options.Theme.Components.Require<TwInputTheme>();

    /// <summary>
    /// Gets or sets the content to display within the radio group.
    /// </summary>
    [Parameter] public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Gets or sets the legend text for the radio group.
    /// </summary>
    [Parameter] public string? Legend { get; set; }

    /// <summary>
    /// Gets or sets the CSS class for the legend element.
    /// </summary>
    [Parameter] public string? LegendClass { get; set; }

    /// <summary>
    /// Gets or sets the orientation of the radio group.
    /// </summary>
    /// <remarks>
    /// Default is vertical. Set to true for horizontal orientation.
    /// </remarks>
    [Parameter] public bool Horizontal { get; set; } = false;

    /// <summary>
    /// Gets or sets whether the entire radio group is disabled.
    /// </summary>
    [Parameter] public bool Disabled { get; set; } = false;

    /// <summary>
    /// Gets or sets the name for all radio buttons in the group.
    /// </summary>
    /// <remarks>
    /// This value is cascaded to child radio buttons to form a mutually exclusive group.
    /// </remarks>
    [Parameter] public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the collection of items to display as radio buttons (for automatic mode).
    /// </summary>
    /// <remarks>
    /// When provided, radio buttons are automatically generated from the items.
    /// If not provided, use ChildContent to manually add radio buttons.
    /// </remarks>
    [Parameter] public IEnumerable<RadioGroupItem<T>>? Items { get; set; }

    /// <summary>
    /// Gets or sets the selected value.
    /// </summary>
    [Parameter] public T? Value { get; set; }

    /// <summary>
    /// Gets or sets the callback invoked when the selected value changes.
    /// </summary>
    [Parameter] public EventCallback<T> ValueChanged { get; set; }

    /// <summary>
    /// Gets or sets the expression that identifies the bound value. Set automatically by <c>@bind-Value</c>, it
    /// lets an enclosing <see cref="EditForm"/> or <see cref="TwForm"/> validate the property the group edits.
    /// </summary>
    [Parameter] public Expression<Func<T>>? ValueExpression { get; set; }

    /// <summary>
    /// Gets or sets the color for automatically generated radio buttons.
    /// </summary>
    [Parameter] public Color? ItemColor { get; set; }

    /// <summary>
    /// Gets or sets whether the group as a whole is in an error state.
    /// </summary>
    [Parameter] public bool Invalid { get; set; }

    /// <summary>
    /// Gets or sets the error message for the group. It is shown under the options when
    /// <see cref="Invalid"/> is set, announced when it appears, and read as the group's description.
    /// </summary>
    [Parameter] public string ErrorMessage { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets whether one of the options must be chosen. Announced by assistive technology.
    /// </summary>
    [Parameter] public bool Required { get; set; }

    private string? errorId => fieldInvalid && !string.IsNullOrWhiteSpace(fieldErrorMessage) ? $"{Id}-error" : null;

    private bool fieldInvalid => Invalid || editContextField?.FirstMessage is not null;

    private string fieldErrorMessage => Invalid && !string.IsNullOrWhiteSpace(ErrorMessage)
        ? ErrorMessage
        : editContextField?.FirstMessage ?? ErrorMessage;

    /// <inheritdoc />
    FieldIdentifier? ITwFormField.BoundField => editContextField?.Field;

    /// <inheritdoc />
    string? ITwFormField.OwnErrorMessage => Invalid && !string.IsNullOrWhiteSpace(ErrorMessage) ? ErrorMessage : null;

    /// <summary>
    /// Reports whether the group is valid. Called by an enclosing <see cref="TwForm"/> when it is submitted.
    /// </summary>
    /// <returns><see langword="true"/> when the group is valid.</returns>
    public Task<bool> ValidateAsync() => Task.FromResult(!fieldInvalid);

    /// <inheritdoc />
    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        if (cascadedEditContext is not null || editContextField is not null)
        {
            editContextField ??= new TwEditContextField(() => _ = InvokeAsync(StateHasChanged));
            editContextField.Attach(cascadedEditContext, () => ValueExpression is null ? null : FieldIdentifier.Create(ValueExpression));
        }

        if (!ReferenceEquals(cascadedForm, registeredForm))
        {
            registeredForm?.Unregister(this);
            registeredForm = cascadedForm;
            registeredForm?.Register(this);
        }
    }

    private async Task OnItemSelectedAsync(T value)
    {
        Value = value;
        await ValueChanged.InvokeAsync(value);
        editContextField?.NotifyChanged();
    }

    /// <summary>
    /// Releases the group's connection to its form.
    /// </summary>
    /// <returns>A task that completes once the group is released.</returns>
    public ValueTask DisposeAsync()
    {
        editContextField?.Dispose();
        registeredForm?.Unregister(this);
        registeredForm = null;
        GC.SuppressFinalize(this);
        return ValueTask.CompletedTask;
    }


    private string classes =>
        new ClassBuilder(groupsTheme.FieldsetBase)
        .AddClass(Horizontal ? groupsTheme.HorizontalLayout : groupsTheme.VerticalLayout)
        .AddClass(Disabled ? groupsTheme.RadioGroupDisabled : string.Empty)
        .AddClass(groupsTheme.Gap)
        .AddClass(Class)
        .Build();

    private string legendClasses =>
        new ClassBuilder(inputTheme.InputLegendBase)
        .AddClass(LegendClass ?? string.Empty)
        .Build();
}
