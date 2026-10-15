// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using TwBlazor.Barcoding;
using TwBlazor.Enums;

namespace TwBlazor.Services;

/// <summary>
/// Encodes values into one-dimensional barcodes and draws them as SVG, with no third-party dependencies.
/// </summary>
/// <remarks>
/// Encoding produces a <see cref="Barcode"/>, a plain row of dark and light modules that any renderer can draw.
/// <see cref="Components.TwBarcode"/> uses this service to show a barcode on the page; call it directly to build
/// one for a download, a label or your own renderer. The service is stateless and safe to share.
/// </remarks>
public interface ITwBarcodeService
{
    /// <summary>
    /// Encodes a value as a barcode.
    /// </summary>
    /// <param name="value">The value to encode.</param>
    /// <param name="format">The barcode format. Defaults to <see cref="BarcodeFormat.Code128"/>.</param>
    /// <returns>The encoded barcode.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/>.</exception>
    /// <exception cref="BarcodeValueException">The value cannot be encoded in <paramref name="format"/>.</exception>
    Barcode Encode(string value, BarcodeFormat format = BarcodeFormat.Code128);

    /// <summary>
    /// Tries to encode a value as a barcode, without throwing when the value does not suit the format.
    /// </summary>
    /// <param name="value">The value to encode.</param>
    /// <param name="format">The barcode format.</param>
    /// <param name="barcode">The encoded barcode, or <see langword="null"/> when the value cannot be encoded.</param>
    /// <returns><see langword="true"/> when the value was encoded.</returns>
    bool TryEncode(string? value, BarcodeFormat format, [NotNullWhen(true)] out Barcode? barcode);

    /// <summary>
    /// Builds SVG path data that draws the bars, one unit per module, offset by the quiet zone.
    /// The full drawing is <c>barcode.Width + quietZone * 2</c> units wide and <paramref name="height"/> units tall.
    /// </summary>
    /// <param name="barcode">The encoded barcode.</param>
    /// <param name="quietZone">The light margin on each side, in modules, or <see langword="null"/> for <see cref="Barcode.QuietZone"/>.</param>
    /// <param name="height">The height of the bars, in modules.</param>
    /// <returns>The value for a path's <c>d</c> attribute.</returns>
    string GetSvgPath(Barcode barcode, int? quietZone = null, int height = 50);

    /// <summary>
    /// Builds a standalone SVG document for the barcode, for saving or embedding outside a component.
    /// </summary>
    /// <param name="barcode">The encoded barcode.</param>
    /// <param name="options">The colors, sizes, text and title, or <see langword="null"/> for <see cref="BarcodeSvgOptions.Default"/>.</param>
    /// <returns>The SVG markup.</returns>
    string ToSvg(Barcode barcode, BarcodeSvgOptions? options = null);

    /// <summary>
    /// Builds a <c>data:image/svg+xml</c> URI for the barcode, usable as a download link or an <c>&lt;img&gt;</c> source.
    /// </summary>
    /// <param name="barcode">The encoded barcode.</param>
    /// <param name="options">The colors, sizes, text and title, or <see langword="null"/> for <see cref="BarcodeSvgOptions.Default"/>.</param>
    /// <returns>The data URI.</returns>
    string ToSvgDataUri(Barcode barcode, BarcodeSvgOptions? options = null);
}
