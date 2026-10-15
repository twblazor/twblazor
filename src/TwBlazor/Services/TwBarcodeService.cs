// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using TwBlazor.Enums;
using TwBlazor.Services.Encoding;

namespace TwBlazor.Services;

/// <summary>
/// The default <see cref="ITwBarcodeService"/>, built on the library's own barcode encoders.
/// </summary>
public sealed class TwBarcodeService : ITwBarcodeService
{
    /// <inheritdoc />
    public Barcode Encode(string value, BarcodeFormat format = BarcodeFormat.Code128)
    {
        ArgumentNullException.ThrowIfNull(value);
        return BarcodeSymbologies.Get(format).Encode(value);
    }

    /// <inheritdoc />
    public bool TryEncode(string? value, BarcodeFormat format, [NotNullWhen(true)] out Barcode? barcode) =>
        SymbolEncodingException.TryEncode(value, text => Encode(text, format), out barcode);
}
