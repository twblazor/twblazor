// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using TwBlazor.Configuration.Color;

namespace TwBlazor.Configuration.Components;

/// <summary>
/// Color configuration for alert components (<see cref="TwBlazor.Components.TwAlert"/>), including
/// background, text, and border colors for light and dark modes.
/// </summary>
[ExcludeFromCodeCoverage]
public class TwAlertTheme
{
    /// <summary>
    /// Gets or sets the color palette applied to alerts.
    /// </summary>
    public required TwBlazorPalette Colors { get; set; }

    /// <summary>
    /// Gets or sets the classes for the wrapper around the alert's text content, letting it shrink/wrap
    /// correctly next to the optional start/end icons.
    /// </summary>
    public required string TextWrapper { get; set; }

    /// <summary>
    /// Gets or sets the padding for a standard (non-dense) alert.
    /// </summary>
    public required string Padding { get; set; }

    /// <summary>
    /// Gets or sets the padding for a dense alert.
    /// </summary>
    public required string DensePadding { get; set; }

    /// <summary>
    /// Gets or sets the default padding between the start icon and the alert text.
    /// </summary>
    public required string StartIconPadding { get; set; }

    /// <summary>
    /// Gets or sets the default padding between the alert text and the end icon.
    /// </summary>
    public required string EndIconPadding { get; set; }

    /// <summary>
    /// Gets or sets the transition classes applied while the alert is visible.
    /// </summary>
    public required string Transition { get; set; }

    /// <summary>
    /// Gets or sets the size/shape classes for the dismiss button's clickable hit area.
    /// </summary>
    public required string DismissButtonSize { get; set; }

    /// <summary>
    /// Gets or sets the margin applied to the dismiss button when an <c>EndIcon</c> is also present
    /// (so the two don't collide) - when there's no end icon, the button is pushed to the end via the
    /// shared <c>PushEnd</c> spacing token instead.
    /// </summary>
    public required string DismissButtonSpacingWithEndIcon { get; set; }

    /// <summary>
    /// Gets or sets the color/hover/focus classes for the dismiss button's icon.
    /// </summary>
    public required string DismissButtonColor { get; set; }
}
