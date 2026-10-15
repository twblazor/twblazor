// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using TwBlazor.Enums;

namespace TwBlazor.Barcoding;

/// <summary>
/// The registry of supported barcode formats.
/// </summary>
internal static class BarcodeSymbologies
{
    private static readonly Dictionary<BarcodeFormat, IBarcodeSymbology> _all = new IBarcodeSymbology[]
    {
        new Code128Symbology(),
        new Code39Symbology(),
        new EanUpcSymbology(BarcodeFormat.Ean13),
        new EanUpcSymbology(BarcodeFormat.Ean8),
        new EanUpcSymbology(BarcodeFormat.UpcA),
    }.ToDictionary(symbology => symbology.Format);

    /// <summary>
    /// Gets the symbology for a format.
    /// </summary>
    /// <param name="format">The barcode format.</param>
    /// <returns>The symbology that encodes it.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="format"/> is not a supported format.</exception>
    internal static IBarcodeSymbology Get(BarcodeFormat format) =>
        _all.TryGetValue(format, out var symbology)
            ? symbology
            : throw new ArgumentOutOfRangeException(nameof(format), format, "The barcode format is not supported.");
}
