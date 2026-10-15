// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using TwBlazor.Barcoding;
using TwBlazor.Enums;

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
    public bool TryEncode(string? value, BarcodeFormat format, [NotNullWhen(true)] out Barcode? barcode)
    {
        if (value is null)
        {
            barcode = null;
            return false;
        }

        try
        {
            barcode = Encode(value, format);
            return true;
        }
        catch (BarcodeValueException)
        {
            barcode = null;
            return false;
        }
    }

    /// <inheritdoc />
    public string GetSvgPath(Barcode barcode, int? quietZone = null, int height = BarcodeSvgWriter.defaultHeight)
    {
        ArgumentNullException.ThrowIfNull(barcode);
        return BarcodeSvgWriter.GetPath(barcode, quietZone ?? barcode.QuietZone, height);
    }

    /// <inheritdoc />
    public string ToSvg(Barcode barcode, BarcodeSvgOptions? options = null) => BarcodeSvgWriter.ToSvg(barcode, options ?? BarcodeSvgOptions.Default);

    /// <inheritdoc />
    public string ToSvgDataUri(Barcode barcode, BarcodeSvgOptions? options = null) =>
        "data:image/svg+xml;charset=utf-8," + Uri.EscapeDataString(ToSvg(barcode, options));
}
