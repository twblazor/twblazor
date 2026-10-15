// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

namespace TwBlazor.Barcoding;

/// <summary>
/// Settings for writing a <see cref="Barcode"/> out as a standalone SVG document.
/// </summary>
public sealed record BarcodeSvgOptions
{
    /// <summary>
    /// Gets the shared default options: the format's own quiet zone, bars 50 modules tall, black on white,
    /// no text and no title.
    /// </summary>
    public static BarcodeSvgOptions Default { get; } = new();

    /// <summary>
    /// Gets the light margin on each side, in modules, or <see langword="null"/> (the default) for the
    /// <see cref="Barcode.QuietZone"/> the format needs.
    /// </summary>
    public int? QuietZone { get; init; }

    /// <summary>
    /// Gets the height of the bars, in modules. Defaults to 50.
    /// </summary>
    public int Height { get; init; } = BarcodeSvgWriter.defaultHeight;

    /// <summary>
    /// Gets whether to print <see cref="Barcode.Text"/> under the bars. Defaults to <see langword="false"/>.
    /// </summary>
    public bool ShowText { get; init; }

    /// <summary>
    /// Gets the CSS color of the bars and text. Defaults to <c>#000000</c>. Keep it much darker than
    /// <see cref="LightColor"/>; scanners need strong contrast.
    /// </summary>
    public string DarkColor { get; init; } = "#000000";

    /// <summary>
    /// Gets the CSS color of the spaces and quiet zone. Defaults to <c>#ffffff</c>.
    /// </summary>
    public string LightColor { get; init; } = "#ffffff";

    /// <summary>
    /// Gets the text alternative written into the SVG's <c>&lt;title&gt;</c>, or <see langword="null"/> for none.
    /// Set it when the SVG is shown on its own so assistive technology can describe it.
    /// </summary>
    public string? Title { get; init; }
}
