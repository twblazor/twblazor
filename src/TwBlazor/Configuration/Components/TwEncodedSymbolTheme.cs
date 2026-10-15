// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;

namespace TwBlazor.Configuration.Components;

/// <summary>
/// The theme settings every scannable symbol shares (<see cref="TwBlazor.Components.TwQrCode"/> and
/// <see cref="TwBlazor.Components.TwBarcode"/>).
/// </summary>
/// <remarks>
/// The modules and any printed text are drawn in <c>currentColor</c>, so <see cref="Foreground"/> takes text color
/// classes and <see cref="Background"/> takes background classes, each with its own <c>dark:</c> variants.
/// Scanners need strong contrast. Dark modules on a light background read everywhere; light modules on a dark
/// background (an inverted symbol) read in most phone camera apps but not in some dedicated scanners.
/// </remarks>
[ExcludeFromCodeCoverage]
public abstract class TwEncodedSymbolTheme
{
    /// <summary>
    /// Gets or sets the structural classes for the symbol's root element. Include <c>forced-color-adjust-none</c> so
    /// high contrast modes do not recolor the symbol into something a scanner cannot read.
    /// </summary>
    public required string Base { get; set; }

    /// <summary>
    /// Gets or sets the background classes behind the modules and quiet zone, for example
    /// <c>bg-white dark:bg-transparent</c>.
    /// </summary>
    public required string Background { get; set; }

    /// <summary>
    /// Gets or sets the text color classes for the dark modules and printed text, for example
    /// <c>text-black dark:text-white</c>.
    /// </summary>
    public required string Foreground { get; set; }
}
