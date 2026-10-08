// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

// Design and API shape inspired by MudBlazor's DialogOptions
// (https://github.com/MudBlazor/MudBlazor/tree/dev/src/MudBlazor/Services/Dialog), MIT License.

using TwBlazor.Configuration.Components;
using TwBlazor.Enums;

namespace TwBlazor.Models;

/// <summary>
/// Customization options for a dialog shown via <see cref="Services.ITwDialogService"/>.
/// </summary>
public record TwDialogOptions
{
    /// <summary>
    /// The default dialog options, used when no options are supplied to a Show call.
    /// </summary>
    internal static readonly TwDialogOptions _default = new();

    /// <summary>
    /// Gets or sets the screen position of the dialog.
    /// </summary>
    /// <remarks>Defaults to <see cref="DialogPosition.Center"/> when not set.</remarks>
    public DialogPosition? Position { get; init; }

    /// <summary>
    /// Gets or sets the maximum width of the dialog.
    /// </summary>
    /// <remarks>Defaults to <see cref="DialogMaxWidth.Small"/> when not set.</remarks>
    public DialogMaxWidth? MaxWidth { get; init; }

    /// <summary>
    /// Gets or sets whether the dialog can be closed by clicking the backdrop.
    /// </summary>
    /// <remarks>Defaults to <c>true</c> when not set.</remarks>
    public bool? BackdropClick { get; init; }

    /// <summary>
    /// Gets or sets whether the dialog can be closed by pressing the Escape key.
    /// </summary>
    /// <remarks>Defaults to <c>true</c> when not set.</remarks>
    public bool? CloseOnEscapeKey { get; init; }

    /// <summary>
    /// Gets or sets whether the dialog header (title and close button) is hidden.
    /// </summary>
    /// <remarks>Defaults to <c>false</c> when not set.</remarks>
    public bool? NoHeader { get; init; }

    /// <summary>
    /// Gets or sets whether a close button is shown in the top-right corner of the dialog.
    /// </summary>
    /// <remarks>Defaults to <c>true</c> when not set. Ignored when <see cref="NoHeader"/> is <c>true</c>.</remarks>
    public bool? CloseButton { get; init; }

    /// <summary>
    /// Gets or sets whether the dialog stretches to fill the entire screen.
    /// </summary>
    /// <remarks>Defaults to <c>false</c>. When <c>true</c>, <see cref="MaxWidth"/> and <see cref="FullWidth"/> are ignored.</remarks>
    public bool? FullScreen { get; init; }

    /// <summary>
    /// Gets or sets whether the dialog stretches to fill its <see cref="MaxWidth"/> boundary.
    /// </summary>
    /// <remarks>Defaults to <c>false</c>.</remarks>
    public bool? FullWidth { get; init; }

    /// <summary>
    /// Gets or sets the border radius override for this dialog instance.
    /// </summary>
    /// <remarks>If not set, falls back to <see cref="TwDialogTheme.Rounded"/>, then the global default.</remarks>
    public Rounded? Rounded { get; init; }

    /// <summary>
    /// Gets or sets the shadow override for this dialog instance.
    /// </summary>
    /// <remarks>If not set, falls back to <see cref="TwDialogTheme.Shadow"/>, then the global default.</remarks>
    public Shadow? Shadow { get; init; }

    /// <summary>
    /// Gets or sets additional custom CSS classes applied to the dialog surface.
    /// </summary>
    public string? Class { get; init; }

    /// <summary>
    /// Gets or sets additional custom CSS classes applied to the backdrop/overlay element.
    /// </summary>
    public string? BackdropClass { get; init; }
}
