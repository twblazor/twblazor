// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

namespace TwBlazor.Services.Encoding;

/// <summary>
/// Settings for writing an <see cref="IEncodedSymbol"/> out as a standalone SVG document.
/// </summary>
public sealed record SymbolSvgOptions
{
    /// <summary>
    /// Gets the shared default options: the symbol's own quiet zone, black on white, no caption and no title.
    /// </summary>
    public static SymbolSvgOptions Default { get; } = new();

    /// <summary>
    /// Gets the light margin, in modules, or <see langword="null"/> (the default) for the symbol's own
    /// <see cref="IEncodedSymbol.QuietZone"/>. Scanners struggle with less.
    /// </summary>
    public int? QuietZone { get; init; }

    /// <summary>
    /// Gets the height of the bars of a one-dimensional barcode, in modules. Defaults to 50. Two-dimensional
    /// symbols ignore it.
    /// </summary>
    public int BarHeight { get; init; } = SymbolSvg.defaultBarHeight;

    /// <summary>
    /// Gets the CSS color of the dark modules and caption. Defaults to <c>#000000</c>.
    /// </summary>
    public string DarkColor { get; init; } = "#000000";

    /// <summary>
    /// Gets the CSS color of the background, or <see langword="null"/> for a transparent one. Defaults to
    /// <c>#ffffff</c>. Keep strong contrast with <see cref="DarkColor"/>: most scanners need dark modules on a
    /// light background.
    /// </summary>
    public string? LightColor { get; init; } = "#ffffff";

    /// <summary>
    /// Gets the text printed under the symbol, such as a barcode's <see cref="Barcode.Text"/>, or
    /// <see langword="null"/> (the default) for none.
    /// </summary>
    public string? Caption { get; init; }

    /// <summary>
    /// Gets the text alternative written into the SVG's <c>&lt;title&gt;</c>, or <see langword="null"/> for none.
    /// Set it when the SVG is shown on its own so assistive technology can describe it.
    /// </summary>
    public string? Title { get; init; }
}
