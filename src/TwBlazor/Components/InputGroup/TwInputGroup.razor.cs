// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using Microsoft.AspNetCore.Components;
using TwBlazor.Configuration.Components;
using TwBlazor.Enums;
using TwBlazor.Utilities;

namespace TwBlazor.Components;

/// <summary>
/// Wraps an input with addons - text, icons or buttons - before, after, above or below it, drawn as one field.
/// </summary>
/// <remarks>
/// <para>
/// Place one input (<see cref="TwTextfield{T}"/>, <see cref="TwNumeric{T}"/>, <see cref="TwMaskedInput"/>,
/// <see cref="TwSelect{T}"/>, <see cref="TwAutocomplete{T}"/> or a picker built on a text field) and any number of
/// <see cref="TwInputGroupAddon"/>s inside. The group draws the border, background, rounding, focus ring and invalid
/// state for the input's <see cref="Variant"/>, and the input inside it drops its own, so the addons sit inside the field.
/// Addons are placed by their <see cref="TwInputGroupAddon.Align"/>, whatever their order in the markup, and follow the
/// layout direction, so a start addon moves to the right in a right to left layout.
/// </para>
/// <para>
/// The group is rendered with <c>role="group"</c>, named by <see cref="Label"/>, <see cref="TwBlazorComponentBase.AriaLabel"/>
/// or <see cref="TwBlazorComponentBase.AriaLabelledBy"/>. A <see cref="TwInputGroupText"/> is added to the input's
/// <c>aria-describedby</c>, so a screen reader reads a prefix or unit such as "https://" or "kg" with the field.
/// The input's validation message is shown below the group instead of inside it.
/// </para>
/// </remarks>
public partial class TwInputGroup : TwBlazorComponentBase
{
    private readonly List<string> _describedByIds = [];
    private readonly List<(object Source, string Id, string Message)> _errorEntries = [];
    private TwInputGroupContext? context;
    private bool disposed;

    private TwInputGroupTheme theme => options.Theme.Components.Require<TwInputGroupTheme>();

    private TwInputTheme inputTheme => options.Theme.Components.Require<TwInputTheme>();

    /// <summary>
    /// Gets or sets the input and addons inside the group.
    /// </summary>
    [Parameter] public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Gets or sets the visual variant of the group (Default, Outlined, Filled).
    /// </summary>
    /// <remarks>
    /// If not set, uses the global default from <see cref="TwInputTheme.DefaultInputVariant"/>, the same as the inputs.
    /// The input inside the group takes its look from the group, so its own <c>Variant</c> has no effect on its border.
    /// </remarks>
    [Parameter] public InputVariant? Variant { get; set; }

    /// <summary>
    /// Gets or sets whether the group, and the input and buttons inside it, use the compact height and text size.
    /// </summary>
    [Parameter] public bool Dense { get; set; }

    /// <summary>
    /// Gets or sets the label shown above the group, which also names the group for screen readers.
    /// </summary>
    /// <remarks>
    /// Set <see cref="For"/> to the input's <c>Id</c> so clicking the label focuses the input and the input takes its
    /// name from the label. Otherwise, give the input its own <c>AriaLabel</c>.
    /// </remarks>
    [Parameter] public string? Label { get; set; }

    /// <summary>
    /// Gets or sets the id of the input that <see cref="Label"/> labels.
    /// </summary>
    [Parameter] public string? For { get; set; }

    /// <summary>
    /// Gets or sets additional classes for the label.
    /// </summary>
    [Parameter] public string LabelClass { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets additional classes for the outer element that holds the label, the group and its validation messages.
    /// <see cref="TwBlazorComponentBase.Class"/> styles the group itself.
    /// </summary>
    [Parameter] public string RootClass { get; set; } = string.Empty;

    private InputVariant effectiveVariant => Variant ?? inputTheme.DefaultInputVariant;

    private string labelId => $"{Id}-label";

    private bool hasLabel => !string.IsNullOrWhiteSpace(Label);

    private string? groupLabelledBy => !string.IsNullOrWhiteSpace(AriaLabelledBy)
        ? AriaLabelledBy
        : hasLabel && string.IsNullOrWhiteSpace(AriaLabel) ? labelId : null;

    private IEnumerable<(string Id, string Message)> errors => _errorEntries.Select(e => (e.Id, e.Message));

    private string rootClasses => new ClassBuilder(theme.Root).AddClass(RootClass).Build();

    private string groupClasses => new ClassBuilder(theme.Group)
        .AddClass(variantClasses)
        .AddClass(effectiveVariant == InputVariant.Default ? theme.DefaultPadding : theme.Padding)
        .AddClass(theme.FocusBorder)
        .AddClass(theme.InvalidBorder)
        .AddClass(theme.Disabled)
        .AddClass(Class)
        .Build();

    // The same border, background and rounding InputVariantBuilder gives an input of this variant, without its focus
    // border (the group shows focus with FocusBorder, while its input has focus) or padding.
    private string variantClasses => effectiveVariant switch
    {
        InputVariant.Outlined => new ClassBuilder(inputTheme.OutlinedBorder)
            .AddClass(options.Theme.Tokens.Transparent)
            .AddClass(roundedBuilder.GetRounded(effectiveRounded))
            .Build(),
        InputVariant.Filled => new ClassBuilder(inputTheme.FilledBorder)
            .AddClass(inputTheme.FilledBackgroundColor)
            .AddClass(roundedBuilder.GetRoundedTop(effectiveRounded))
            .Build(),
        _ => new ClassBuilder(inputTheme.FilledBorder).AddClass(options.Theme.Tokens.Transparent).Build()
    };

    private string controlRootClasses => new ClassBuilder(theme.ControlRoot)
        .AddClass(effectiveVariant == InputVariant.Outlined ? theme.ControlRootOutlinedOffset : theme.ControlRootUnderlinedOffset)
        .Build();

    /// <inheritdoc />
    protected override void OnParametersSet()
    {
        base.OnParametersSet();
        UpdateContext();
    }

    /// <summary>
    /// Creates a new <see cref="TwInputGroupContext"/> when anything in it has changed, so everything inside the group
    /// re-renders with it.
    /// </summary>
    private void UpdateContext()
    {
        var next = new TwInputGroupContext(
            this,
            effectiveVariant,
            Dense,
            effectiveRounded,
            theme.Control,
            controlRootClasses,
            _describedByIds.Count == 0 ? null : string.Join(' ', _describedByIds));

        if (context is null || context != next)
        {
            context = next;
        }
    }

    /// <summary>
    /// Adds a text's id to the input's <c>aria-describedby</c>.
    /// </summary>
    /// <param name="id">The id of the text element.</param>
    internal void AddDescription(string id)
    {
        if (string.IsNullOrEmpty(id) || _describedByIds.Contains(id))
        {
            return;
        }

        _describedByIds.Add(id);
        Refresh();
    }

    /// <summary>
    /// Removes a text's id from the input's <c>aria-describedby</c>.
    /// </summary>
    /// <param name="id">The id of the text element.</param>
    internal void RemoveDescription(string id)
    {
        if (_describedByIds.Remove(id))
        {
            Refresh();
        }
    }

    /// <summary>
    /// Shows, updates or removes the validation message of an input inside the group, below the group.
    /// </summary>
    /// <param name="source">The input root reporting the message.</param>
    /// <param name="id">The id the message must be rendered with, so the input's <c>aria-describedby</c> finds it,
    /// or <see langword="null"/> while the input is valid.</param>
    /// <param name="message">The message, or <see langword="null"/> while the input is valid.</param>
    internal void SetError(object source, string? id, string? message)
    {
        var index = _errorEntries.FindIndex(e => ReferenceEquals(e.Source, source));
        var show = !string.IsNullOrEmpty(id) && !string.IsNullOrWhiteSpace(message);

        if (!show)
        {
            if (index >= 0)
            {
                _errorEntries.RemoveAt(index);
                Refresh();
            }

            return;
        }

        var entry = (source, id!, message!);
        if (index < 0)
        {
            _errorEntries.Add(entry);
            Refresh();
        }
        else if (_errorEntries[index].Id != id || _errorEntries[index].Message != message)
        {
            _errorEntries[index] = entry;
            Refresh();
        }
    }

    private void Refresh()
    {
        if (disposed)
        {
            return;
        }

        UpdateContext();
        _ = InvokeAsync(StateHasChanged);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        disposed = true;
        GC.SuppressFinalize(this);
    }
}
