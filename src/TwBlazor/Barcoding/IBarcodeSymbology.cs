// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using TwBlazor.Enums;

namespace TwBlazor.Barcoding;

/// <summary>
/// Encodes values in one barcode format. Add a format by implementing this, adding a
/// <see cref="BarcodeFormat"/> value and registering it in <see cref="BarcodeSymbologies"/>.
/// </summary>
internal interface IBarcodeSymbology
{
    /// <summary>
    /// Gets the format this symbology produces.
    /// </summary>
    BarcodeFormat Format { get; }

    /// <summary>
    /// Encodes a value.
    /// </summary>
    /// <param name="value">The value to encode.</param>
    /// <returns>The encoded barcode.</returns>
    /// <exception cref="BarcodeValueException">The value cannot be encoded in this format.</exception>
    Barcode Encode(string value);
}
