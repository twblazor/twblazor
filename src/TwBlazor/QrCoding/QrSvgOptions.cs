// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

namespace TwBlazor.QrCoding;

/// <summary>
/// Settings for writing a <see cref="QrMatrix"/> out as a standalone SVG document.
/// </summary>
public sealed record QrSvgOptions
{
    /// <summary>
    /// Gets the shared default options: a 4 module quiet zone, black on white, no title.
    /// </summary>
    public static QrSvgOptions Default { get; } = new();

    /// <summary>
    /// Gets the width of the light margin around the code, in modules. Defaults to 4, the minimum the
    /// standard asks for; scanners struggle to find a code with less.
    /// </summary>
    public int QuietZone { get; init; } = QrSvgWriter.defaultQuietZone;

    /// <summary>
    /// Gets the CSS color of the dark modules. Defaults to <c>#000000</c>. Keep it much darker than
    /// <see cref="LightColor"/>; most scanners cannot read light-on-dark codes.
    /// </summary>
    public string DarkColor { get; init; } = "#000000";

    /// <summary>
    /// Gets the CSS color of the light modules and quiet zone. Defaults to <c>#ffffff</c>.
    /// </summary>
    public string LightColor { get; init; } = "#ffffff";

    /// <summary>
    /// Gets the text alternative written into the SVG's <c>&lt;title&gt;</c>, or <see langword="null"/> for none.
    /// Set it when the SVG is shown on its own (for example as an <c>&lt;img&gt;</c> source) so assistive
    /// technology can describe it.
    /// </summary>
    public string? Title { get; init; }
}
