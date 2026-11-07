// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using Microsoft.AspNetCore.Components;
using TwBlazor.Components;
using TwBlazor.Configuration.Components;
using TwBlazor.Enums;

namespace TwBlazor;

/// <summary>
/// Base class for input components styled via <see cref="Enums.InputVariant"/> (Default, Outlined,
/// Filled) - text-editable "textfield-shaped" controls (<see cref="Components.TwTextfield{T}"/>,
/// <see cref="Components.TwSelect{T}"/>, <see cref="Components.TwColorPicker"/>) and the popover pickers
/// built on top of a textfield trigger (<see cref="Components.TwDatePicker"/>,
/// <see cref="Components.TwTimePicker"/>, <see cref="Components.TwDateTimePicker"/>).
/// </summary>
/// <remarks>
/// Deliberately narrower than <see cref="TwBlazorInputComponentBase"/>: other inputs derived from that
/// base (checkboxes, radio buttons, switches, sliders, progress bars, file upload) have no
/// Default/Outlined/Filled textfield concept, and one of them (<see cref="Components.TwFileUpload"/>)
/// already exposes its own unrelated <c>Variant</c> (a <see cref="ButtonVariant"/>) - Blazor's parameter
/// binding rejects two same-named parameters on one component even when one hides the other via
/// <see langword="new"/>, so <c>Variant</c> can't live on the shared input base without colliding there.
/// </remarks>
public abstract class TwBlazorTextInputComponentBase : TwBlazorInputComponentBase
{
    /// <summary>
    /// Gets or sets the visual variant of the input control (Default, Outlined, Filled).
    /// </summary>
    /// <remarks>
    /// If not set, uses the global default from <see cref="TwInputTheme.DefaultInputVariant"/> - see
    /// <see cref="effectiveVariant"/>.
    /// </remarks>
    [Parameter] public InputVariant? Variant { get; set; }

    /// <summary>
    /// Gets or sets whether the input uses a more compact height and text size, useful when screen space
    /// is limited.
    /// </summary>
    /// <remarks>
    /// The height and text size come from <see cref="TwInputTheme.Size"/> and <see cref="TwInputTheme.DenseSize"/>,
    /// shared by every single-line input so they all line up. Not applied to a multi-select
    /// (<see cref="Components.TwSelect{T}.Multiple"/>), whose height is driven by its selected chips.
    /// </remarks>
    [Parameter] public bool Dense { get; set; }

    /// <summary>
    /// Gets the height and text size classes for a single-line input: <see cref="TwInputTheme.DenseSize"/>
    /// when <see cref="Dense"/> is set, otherwise <see cref="TwInputTheme.Size"/>.
    /// </summary>
    protected string inputSizeClasses
    {
        get
        {
            var theme = options.Theme.Components.Require<TwInputTheme>();
            return effectiveDense ? theme.DenseSize : theme.Size;
        }
    }

    /// <summary>
    /// Gets or sets the <see cref="TwInputGroup"/> the input sits in, if any.
    /// </summary>
    [CascadingParameter] private TwInputGroupContext? cascadedInputGroup { get; set; }

    /// <summary>
    /// Gets whether the input sits in a <see cref="TwInputGroup"/>, which then draws its border, background, rounding
    /// and focus ring.
    /// </summary>
    protected bool isInInputGroup => cascadedInputGroup is not null;

    /// <summary>
    /// Gets whether the input uses the compact size: <see cref="Dense"/> is set, or the
    /// <see cref="TwInputGroup"/> it sits in is <see cref="TwInputGroup.Dense"/>.
    /// </summary>
    protected bool effectiveDense => Dense || (cascadedInputGroup?.Dense ?? false);

    /// <summary>
    /// Gets the classes added to the input's control after its variant classes while it sits in a
    /// <see cref="TwInputGroup"/>, removing the border, background, rounding, padding and focus ring the group draws
    /// instead, or an empty string outside a group.
    /// </summary>
    protected string inputGroupControlClasses => cascadedInputGroup?.ControlClasses ?? string.Empty;

    /// <summary>
    /// Joins the control's own <c>aria-describedby</c> ids with those of the <see cref="TwInputGroupText"/>s in the
    /// <see cref="TwInputGroup"/> it sits in.
    /// </summary>
    /// <param name="ownIds">The control's own ids, such as its error message's, or <see langword="null"/>.</param>
    /// <returns>The ids separated by spaces, or <see langword="null"/> when there are none.</returns>
    protected string? GetDescribedBy(string? ownIds) => TwInputGroupContext.JoinDescribedBy(cascadedInputGroup, ownIds);

    /// <summary>
    /// Gets the effective input variant to use: <see cref="Variant"/> when explicitly set by the
    /// consumer, otherwise the global default configured via <see cref="TwInputTheme.DefaultInputVariant"/>.
    /// </summary>
    protected InputVariant effectiveVariant => Variant ?? options.Theme.Components.Require<TwInputTheme>().DefaultInputVariant;

    /// <summary>
    /// Gets the input variant whose surface the control's field draws. Inside a <see cref="TwInputGroup"/>
    /// this is the group's variant (the group draws the field's border and background), otherwise the
    /// control's own <see cref="effectiveVariant"/>.
    /// </summary>
    protected InputVariant surfaceVariant => cascadedInputGroup?.Variant ?? effectiveVariant;
}
