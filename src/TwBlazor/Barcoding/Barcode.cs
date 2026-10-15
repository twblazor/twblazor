// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using TwBlazor.Enums;

namespace TwBlazor.Barcoding;

/// <summary>
/// An encoded one-dimensional barcode: a row of dark and light modules (the narrowest bar width), ready to be
/// drawn by any renderer.
/// </summary>
/// <remarks>
/// The barcode is immutable, so it can be cached and shared freely. It does not include the quiet zone; a
/// renderer adds that margin itself, using <see cref="QuietZone"/> as the recommended width.
/// </remarks>
public sealed class Barcode
{
    private readonly bool[] _modules;

    internal Barcode(BarcodeFormat format, string text, bool[] modules, int quietZone)
    {
        Format = format;
        Text = text;
        QuietZone = quietZone;
        _modules = modules;
    }

    /// <summary>
    /// Gets the symbology the barcode was encoded with.
    /// </summary>
    public BarcodeFormat Format { get; }

    /// <summary>
    /// Gets the human readable text printed under the bars, including any check digit that was added.
    /// </summary>
    public string Text { get; }

    /// <summary>
    /// Gets the light margin, in modules, the symbology needs on each side so scanners can find the bars.
    /// </summary>
    public int QuietZone { get; }

    /// <summary>
    /// Gets the width of the bars, in modules, without the quiet zone.
    /// </summary>
    public int Width => _modules.Length;

    /// <summary>
    /// Gets whether the module at a position is dark (part of a bar).
    /// </summary>
    /// <param name="x">The module, from 0 at the left.</param>
    /// <returns><see langword="true"/> for a dark module; <see langword="false"/> for a light one or a position outside the bars.</returns>
    public bool this[int x] => x >= 0 && x < _modules.Length && _modules[x];
}
