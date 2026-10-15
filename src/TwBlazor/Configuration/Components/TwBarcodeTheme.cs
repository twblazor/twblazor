// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;

namespace TwBlazor.Configuration.Components;

/// <summary>
/// Theme configuration for the barcode component (<see cref="TwBlazor.Components.TwBarcode"/>).
/// Override any property to customize barcode styles globally.
/// </summary>
/// <remarks>
/// Keep <see cref="Dark"/> much darker than <see cref="Light"/>, in dark mode too: scanners need dark bars on a
/// light background.
/// </remarks>
[ExcludeFromCodeCoverage]
public class TwBarcodeTheme
{
    /// <summary>
    /// Gets or sets the base classes for the wrapper holding the bars and text. Include a light background and
    /// <c>forced-color-adjust-none</c> so high contrast modes do not recolor the barcode into something a
    /// scanner cannot read.
    /// </summary>
    public required string Base { get; set; }

    /// <summary>
    /// Gets or sets the fill classes for the spaces and quiet zone.
    /// </summary>
    public required string Light { get; set; }

    /// <summary>
    /// Gets or sets the fill classes for the bars.
    /// </summary>
    public required string Dark { get; set; }

    /// <summary>
    /// Gets or sets the classes for the human readable text under the bars.
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
