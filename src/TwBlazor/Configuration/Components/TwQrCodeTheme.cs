// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;

namespace TwBlazor.Configuration.Components;

/// <summary>
/// Theme configuration for the QR code component (<see cref="TwBlazor.Components.TwQrCode"/>).
/// Override any property to customize QR code styles globally. The colors are shared with the barcode; see
/// <see cref="TwEncodedSymbolTheme"/>.
/// </summary>
[ExcludeFromCodeCoverage]
public class TwQrCodeTheme : TwEncodedSymbolTheme
{
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
