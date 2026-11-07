// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;

namespace TwBlazor.Configuration.Components;

/// <summary>
/// Theme configuration for the input group (<see cref="TwBlazor.Components.TwInputGroup"/>) and its parts
/// (<see cref="TwBlazor.Components.TwInputGroupAddon"/>, <see cref="TwBlazor.Components.TwInputGroupText"/> and
/// <see cref="TwBlazor.Components.TwInputGroupButton"/>). Override any property to customize input group styles globally.
/// </summary>
/// <remarks>
/// The group draws the field's border, background and rounding from <see cref="TwInputTheme"/> (its
/// <see cref="TwInputTheme.OutlinedBorder"/>, <see cref="TwInputTheme.FilledBorder"/> and
/// <see cref="TwInputTheme.FilledBackgroundColor"/>), so it looks like the inputs beside it. The input inside it is
/// marked with a <c>data-tw-input-group-control</c> attribute, which the focus, invalid and disabled classes below
/// select on.
/// </remarks>
[ExcludeFromCodeCoverage]
public class TwInputGroupTheme
{
    /// <summary>
    /// Gets or sets the classes for the outer element that holds the label, the group and any validation messages.
    /// </summary>
    public required string Root { get; set; }

    /// <summary>
    /// Gets or sets the base classes for the group itself: the box that holds the input and its addons in a row, or in
    /// a column once it has a block addon (<see cref="Enums.InputGroupAlign.BlockStart"/> or
    /// <see cref="Enums.InputGroupAlign.BlockEnd"/>).
    /// </summary>
    public required string Group { get; set; }

    /// <summary>
    /// Gets or sets the padding of a group using the <see cref="Enums.InputVariant.Outlined"/> or
    /// <see cref="Enums.InputVariant.Filled"/> variant.
    /// </summary>
    public required string Padding { get; set; }

    /// <summary>
    /// Gets or sets the padding of a group using the <see cref="Enums.InputVariant.Default"/> variant, whose input has
    /// no padding of its own.
    /// </summary>
    public required string DefaultPadding { get; set; }

    /// <summary>
    /// Gets or sets the border and ring shown on the group while its input has focus.
    /// </summary>
    public required string FocusBorder { get; set; }

    /// <summary>
    /// Gets or sets the border shown on the group while its input is invalid (<c>aria-invalid="true"</c>).
    /// </summary>
    public required string InvalidBorder { get; set; }

    /// <summary>
    /// Gets or sets the classes applied to the group while its input is disabled.
    /// </summary>
    public required string Disabled { get; set; }

    /// <summary>
    /// Gets or sets the classes added to the input inside a group, after its own variant classes, to remove the border,
    /// background, rounding, padding and focus ring the group now draws instead.
    /// </summary>
    public required string Control { get; set; }

    /// <summary>
    /// Gets or sets the classes for the root element of an input inside a group, so it fills the room the addons leave.
    /// </summary>
    public required string ControlRoot { get; set; }

    /// <summary>
    /// Gets or sets the margin that takes the border of an <see cref="Enums.InputVariant.Outlined"/> group off the
    /// height of the input's root, so the group is as tall as an input on its own.
    /// </summary>
    public required string ControlRootOutlinedOffset { get; set; }

    /// <summary>
    /// Gets or sets the margin that takes the bottom border of a <see cref="Enums.InputVariant.Filled"/> or
    /// <see cref="Enums.InputVariant.Default"/> group off the height of the input's root, so the group is as tall as an
    /// input on its own.
    /// </summary>
    public required string ControlRootUnderlinedOffset { get; set; }

    /// <summary>
    /// Gets or sets the base classes for every addon.
    /// </summary>
    public required string Addon { get; set; }

    /// <summary>
    /// Gets or sets the classes for an addon placed with <see cref="Enums.InputGroupAlign.InlineStart"/>.
    /// </summary>
    public required string AddonInlineStart { get; set; }

    /// <summary>
    /// Gets or sets the classes for an addon placed with <see cref="Enums.InputGroupAlign.InlineEnd"/>.
    /// </summary>
    public required string AddonInlineEnd { get; set; }

    /// <summary>
    /// Gets or sets the classes for an addon placed with <see cref="Enums.InputGroupAlign.BlockStart"/>.
    /// </summary>
    public required string AddonBlockStart { get; set; }

    /// <summary>
    /// Gets or sets the classes for an addon placed with <see cref="Enums.InputGroupAlign.BlockEnd"/>.
    /// </summary>
    public required string AddonBlockEnd { get; set; }

    /// <summary>
    /// Gets or sets the classes for the static text of a <see cref="TwBlazor.Components.TwInputGroupText"/>.
    /// </summary>
    public required string Text { get; set; }

    /// <summary>
    /// Gets or sets the classes added to every <see cref="TwBlazor.Components.TwInputGroupButton"/>.
    /// </summary>
    public required string Button { get; set; }

    /// <summary>
    /// Gets or sets the classes added to a <see cref="TwBlazor.Components.TwInputGroupButton"/> with a label in a dense
    /// group, so it fits the shorter field.
    /// </summary>
    public required string ButtonDense { get; set; }

    /// <summary>
    /// Gets or sets the classes added to an icon-only <see cref="TwBlazor.Components.TwInputGroupButton"/> in a dense
    /// group, so it fits the shorter field.
    /// </summary>
    public required string IconButtonDense { get; set; }
}
