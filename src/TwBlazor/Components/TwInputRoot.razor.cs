// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using Microsoft.AspNetCore.Components;
using TwBlazor.Configuration;
using TwBlazor.Configuration.Components;
using TwBlazor.Utilities;

namespace TwBlazor.Components;

/// <summary>
/// Used for all input components to wrap the input element and provide a
/// consistent root element for styling and attributes.
/// </summary>
public partial class TwInputRoot : IDisposable
{
    private TwInputGroup? reportedGroup;

    [Inject] private TwBlazorOptions options { get; set; } = null!;

    private TwInputTheme theme => options.Theme.Components.Require<TwInputTheme>();

    /// <summary>
    /// Gets or sets the <see cref="TwInputGroup"/> this input sits in, if any.
    /// </summary>
    [CascadingParameter] private TwInputGroupContext? inputGroup { get; set; }

    /// <summary>
    /// Gets or sets the root of the input this one is part of, such as the text field inside a date picker.
    /// </summary>
    [CascadingParameter] private TwInputRoot? parentRoot { get; set; }

    // Only the outermost input in a group is laid out by it; one nested inside another input sits in that input's root.
    private string? rootClasses => inputGroup is not null && parentRoot is null
        ? new ClassBuilder(inputGroup.ControlRootClasses).AddClass(RootClass ?? string.Empty).Build()
        : RootClass;

    /// <summary>
    /// The input element is rendered as a child of this component, allowing it to apply consistent 
    /// styling and attributes to the root element. The ChildContent parameter is used to render the
    /// input element within the TwInputRoot component.
    /// </summary>
    [Parameter] public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// The id of the root element.
    /// </summary>
    [Parameter] public required string RootId { get; set; }

    /// <summary>
    /// The classes applied to the root element.
    /// </summary>
    [Parameter] public required string? RootClass { get; set; }

    /// <summary>
    /// The additional attributes applied to the root element.
    /// </summary>
    [Parameter] public required Dictionary<string, object>? RootAttributes { get; set; }

    /// <summary>
    /// Optional inline style for the root element.
    /// </summary>
    [Parameter] public string? RootStyle { get; set; }

    /// <summary>
    /// Optional ARIA label for the root element.
    /// </summary>
    [Parameter] public string? RootAriaLabel { get; set; }

    /// <summary>
    /// Optional ARIA labelledby attribute for the root element.
    /// </summary>
    [Parameter] public string? RootAriaLabelledBy { get; set; }

    /// <summary>
    /// Optional validation error message. Rendered below the input, with the id <see cref="RootErrorId"/> so the
    /// input's aria-describedby can reference it, only while <see cref="RootErrorId"/> is also set.
    /// </summary>
    /// <remarks>
    /// Input components only set <see cref="RootErrorId"/> while they are invalid, so a message passed alongside
    /// <c>Invalid="false"</c> stays hidden rather than showing without being linked to the input.
    /// </remarks>
    [Parameter] public string? RootErrorMessage { get; set; }

    /// <summary>
    /// The id applied to the rendered error message element. Must match the id the input component
    /// passes to its own aria-describedby for the association to work.
    /// </summary>
    [Parameter] public string? RootErrorId { get; set; }

    /// <summary>
    /// The reference to the root DOM element. Set internally via @ref on the rendered div.
    /// Consumers capture this by holding a @ref to the TwInputRoot component instance.
    /// </summary>
    public ElementReference RootRef { get; private set; }

    /// <inheritdoc />
    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        // Inside a group the message is shown below the group rather than inside it, beside the addons.
        var group = inputGroup?.Group;
        if (!ReferenceEquals(group, reportedGroup))
        {
            reportedGroup?.SetError(this, null, null);
            reportedGroup = group;
        }

        reportedGroup?.SetError(this, RootErrorId, RootErrorMessage);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Unreports the input's validation message from its group.
    /// </summary>
    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            reportedGroup?.SetError(this, null, null);
            reportedGroup = null;
        }
    }
}
