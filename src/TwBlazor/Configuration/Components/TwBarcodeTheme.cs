// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;

namespace TwBlazor.Configuration.Components;

/// <summary>
/// Theme configuration for the barcode component (<see cref="TwBlazor.Components.TwBarcode"/>).
/// Override any property to customize barcode styles globally. The colors are shared with the QR code; see
/// <see cref="TwEncodedSymbolTheme"/>.
/// </summary>
[ExcludeFromCodeCoverage]
public class TwBarcodeTheme : TwEncodedSymbolTheme
{
    /// <summary>
    /// Gets or sets the classes for the human readable text under the bars. Its color comes from
    /// <see cref="TwEncodedSymbolTheme.Foreground"/>.
    /// </summary>
    public required string Text { get; set; }

    /// <summary>
    /// Gets or sets the size classes of the bars for <see cref="Enums.BarcodeSize.Small"/>.
    /// </summary>
    public required string Small { get; set; }

    /// <summary>
    /// Gets or sets the size classes of the bars for <see cref="Enums.BarcodeSize.Medium"/>.
    /// </summary>
    public required string Medium { get; set; }

    /// <summary>
    /// Gets or sets the size classes of the bars for <see cref="Enums.BarcodeSize.Large"/>.
    /// </summary>
    public required string Large { get; set; }
}
