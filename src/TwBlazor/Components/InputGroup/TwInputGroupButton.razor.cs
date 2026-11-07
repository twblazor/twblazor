// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using Microsoft.AspNetCore.Components;
using TwBlazor.Builders;
using TwBlazor.Configuration.Components;
using TwBlazor.Enums;
using TwBlazor.Utilities;
using Color = TwBlazor.Enums.Color;

namespace TwBlazor.Components;

/// <summary>
/// A <see cref="TwButton"/> sized to sit inside a <see cref="TwInputGroupAddon"/>, such as a copy, search or show
/// password button.
/// </summary>
/// <remarks>
/// It is a compact <see cref="ButtonVariant.Text"/> button by default, rounded one step tighter than its group so its
/// corners follow the field's, and smaller still in a <see cref="TwInputGroup.Dense"/> group. Give an icon-only button
/// an <see cref="TwBlazorComponentBase.AriaLabel"/>.
/// </remarks>
public partial class TwInputGroupButton : TwBlazorComponentBase
{
    private TwInputGroupTheme theme => options.Theme.Components.Require<TwInputGroupTheme>();

    [CascadingParameter] private TwInputGroupContext? inputGroup { get; set; }

    /// <summary>
    /// Gets or sets the callback invoked when the button is clicked.
    /// </summary>
    [Parameter] public EventCallback OnClick { get; set; }

    /// <summary>
    /// Gets or sets the text of the button. <see cref="ChildContent"/> is shown instead when it is set.
    /// </summary>
    [Parameter] public string Label { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets content to show instead of <see cref="Label"/>.
    /// </summary>
    [Parameter] public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Gets or sets the button's <c>type</c>. Defaults to <c>button</c>, so it does not submit the form it sits in.
    /// </summary>
    [Parameter] public string Type { get; set; } = "button";

    /// <summary>
    /// Gets or sets the button's color.
    /// </summary>
    [Parameter] public Color? Color { get; set; }

    /// <summary>
    /// Gets or sets the button's variant. Defaults to <see cref="ButtonVariant.Text"/>.
    /// </summary>
    [Parameter] public ButtonVariant? Variant { get; set; }

    /// <summary>
    /// Gets or sets the icon shown before the label.
    /// </summary>
    [Parameter] public Icon? StartIcon { get; set; }

    /// <summary>
    /// Gets or sets the icon shown after the label.
    /// </summary>
    [Parameter] public Icon? EndIcon { get; set; }

    /// <summary>
    /// Gets or sets whether the button is a square holding only an icon (its <see cref="ChildContent"/> or
    /// <see cref="StartIcon"/>). Give it an <see cref="TwBlazorComponentBase.AriaLabel"/>.
    /// </summary>
    [Parameter] public bool IconButton { get; set; }

    /// <summary>
    /// Gets or sets whether the button is disabled.
    /// </summary>
    [Parameter] public bool Disabled { get; set; }

    /// <summary>
    /// Gets or sets the text of a tooltip shown while the button is hovered or focused.
    /// </summary>
    [Parameter] public string? Tooltip { get; set; }

    private bool dense => inputGroup?.Dense ?? false;

    // Icon buttons stay circles unless asked for a rounding; a labelled button follows the group's corners, one step in.
    private Rounded? buttonRounded => Rounded ?? (IconButton || inputGroup is null ? null : RoundedBuilder.Inset(inputGroup.Rounded));

    private string classes => new ClassBuilder(theme.Button)
        .AddClass(theme.ButtonDense, dense && !IconButton)
        .AddClass(theme.IconButtonDense, dense && IconButton)
        .AddClass(Class)
        .Build();
}
