// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using TwBlazor.Enums;
using TwBlazor.Services.Encoding;

namespace TwBlazor.Services;

/// <summary>
/// Encodes values into one-dimensional barcodes, with no third-party dependencies.
/// </summary>
/// <remarks>
/// Encoding produces a <see cref="Barcode"/>, a plain row of dark and light modules. Draw it with the
/// <see cref="SymbolSvg"/> methods (<c>ToSvg</c>, <c>ToSvgDataUri</c>, <c>GetSvgPath</c>) or your own renderer.
/// <see cref="Components.TwBarcode"/> uses this service to show a barcode on the page. The service is stateless
/// and safe to share.
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
    /// <exception cref="SymbolEncodingException">The value cannot be encoded in <paramref name="format"/>.</exception>
    Barcode Encode(string value, BarcodeFormat format = BarcodeFormat.Code128);

    /// <summary>
    /// Tries to encode a value as a barcode, without throwing when the value does not suit the format.
    /// </summary>
    /// <param name="value">The value to encode.</param>
    /// <param name="format">The barcode format.</param>
    /// <param name="barcode">The encoded barcode, or <see langword="null"/> when the value cannot be encoded.</param>
    /// <returns><see langword="true"/> when the value was encoded.</returns>
    bool TryEncode(string? value, BarcodeFormat format, [NotNullWhen(true)] out Barcode? barcode);
}
