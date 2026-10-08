// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using TwBlazor.Enums;
using TwBlazor.Models;

namespace TwBlazor.Configuration.Components;

/// <summary>
/// Theme configuration for dialog components shown via <see cref="Services.ITwDialogService"/>.
/// Override any property to customize dialog container styles globally.
/// </summary>
/// <remarks>
/// The styling categories exposed here (backdrop, surface, header, width breakpoints) mirror the container
/// customization points found in MudBlazor's MudDialogProvider/MudDialogContainer (MIT licensed). The
/// dialog surface's background/rounded-corner/shadow defaults sit alongside them. Reached through
/// <see cref="TwOverlayTheme.Dialog"/>.
/// </remarks>
[ExcludeFromCodeCoverage]
public class TwDialogTheme
{
    /// <summary>
    /// Gets or sets the background for the modal dialog surface.
    /// </summary>
    public required string Background { get; set; }

    /// <summary>
    /// Gets or sets the default border radius for dialogs.
    /// </summary>
    /// <remarks>
    /// If not set, falls back to global <see cref="TwBlazorRounded.DefaultRounded"/>.
    /// Individual dialogs can override this via <see cref="TwDialogOptions.Rounded"/>.
    /// </remarks>
    public Rounded? Rounded { get; set; }

    /// <summary>
    /// Gets or sets the default shadow level for dialogs.
    /// </summary>
    /// <remarks>
    /// If not set, falls back to global <see cref="TwBlazorShadow.DefaultShadow"/>.
    /// Individual dialogs can override this via <see cref="TwDialogOptions.Shadow"/>.
    /// </remarks>
    public Shadow? Shadow { get; set; }

    /// <summary>
    /// Gets or sets the classes for the fixed overlay that darkens the page and positions the dialog.
    /// </summary>
    public required string Backdrop { get; set; }

    /// <summary>
    /// Gets or sets the base classes for the visible dialog surface (the card containing the dialog
    /// content) - layout and sizing only; background comes from <see cref="Background"/>.
    /// </summary>
    public required string Surface { get; set; }

    /// <summary>
    /// Gets or sets the classes for the dialog header row containing the title and close button.
    /// </summary>
    public required string Header { get; set; }

    /// <summary>
    /// Gets or sets the classes for the dialog title text.
    /// </summary>
    public required string Title { get; set; }

    /// <summary>
    /// Gets or sets the classes for the close button in the dialog header.
    /// </summary>
    public required string CloseButton { get; set; }

    /// <summary>
    /// Gets or sets the classes for the scrollable dialog body.
    /// </summary>
    public required string Content { get; set; }

    /// <summary>
    /// Gets or sets the classes applied when a dialog is stretched to fill the entire screen.
    /// </summary>
    public required string FullScreen { get; set; }

    /// <summary>
    /// Gets or sets the classes applied when a dialog is stretched to fill its <see cref="TwDialogOptions.MaxWidth"/>.
    /// </summary>
    public required string FullWidth { get; set; }

    /// <summary>
    /// Gets or sets the max-width class for <see cref="Enums.DialogMaxWidth.Small"/> dialogs.
    /// </summary>
    public required string SmallWidth { get; set; }

    /// <summary>
    /// Gets or sets the max-width class for <see cref="Enums.DialogMaxWidth.Medium"/> dialogs.
    /// </summary>
    public required string MediumWidth { get; set; }

    /// <summary>
    /// Gets or sets the max-width class for <see cref="Enums.DialogMaxWidth.Large"/> dialogs.
    /// </summary>
    public required string LargeWidth { get; set; }
}