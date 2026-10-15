// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;

namespace TwBlazor.Configuration.Components;

/// <summary>
/// Theme configuration for the QR code component (<see cref="TwBlazor.Components.TwQrCode"/>).
/// Override any property to customize QR code styles globally.
/// </summary>
/// <remarks>
/// Keep <see cref="Dark"/> much darker than <see cref="Light"/>, in dark mode too: most scanners cannot read a
/// code whose colors are inverted or low in contrast.
/// </remarks>
[ExcludeFromCodeCoverage]
public class TwQrCodeTheme
{
    /// <summary>
    /// Gets or sets the base classes for the <c>&lt;svg&gt;</c>. Include <c>forced-color-adjust-none</c> so
    /// high contrast modes do not recolor the code into something a scanner cannot read.
    /// </summary>
    public required string Base { get; set; }

    /// <summary>
    /// Gets or sets the fill classes for the light modules and quiet zone.
    /// </summary>
    public required string Light { get; set; }

    /// <summary>
    /// Gets or sets the fill classes for the dark modules.
    /// </summary>
    public required string Dark { get; set; }

    /// <summary>
    /// Gets or sets the size classes for <see cref="Enums.QrCodeSize.Small"/>.
    /// </summary>
    public required string Small { get; set; }

    /// <summary>
    /// Gets or sets the size classes for <see cref="Enums.QrCodeSize.Medium"/>.
    /// </summary>
    public required string Medium { get; set; }

    /// <summary>
    /// Gets or sets the size classes for <see cref="Enums.QrCodeSize.Large"/>.
    /// </summary>
    public required string Large { get; set; }

    /// <summary>
    /// Gets or sets the size classes for <see cref="Enums.QrCodeSize.ExtraLarge"/>.
    /// </summary>
    public required string ExtraLarge { get; set; }
}
