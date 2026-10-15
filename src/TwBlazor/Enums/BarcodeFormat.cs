// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

namespace TwBlazor.Enums;

/// <summary>
/// Defines the barcode symbologies (formats) that can be generated.
/// </summary>
public enum BarcodeFormat
{
    /// <summary>
    /// Code 128 - Default. Holds any ASCII text and packs runs of digits densely. The usual choice for
    /// shipping labels, tickets and internal tracking.
    /// </summary>
    Code128,

    /// <summary>
    /// Code 39. Holds upper case A-Z, digits, space and <c>-.$/+%</c>. Wider than Code 128, but read by
    /// almost every scanner, so it suits older equipment and ID badges.
    /// </summary>
    Code39,

    /// <summary>
    /// EAN-13, the retail product barcode used worldwide. Takes 12 digits (the check digit is added) or 13
    /// digits (the check digit is verified).
    /// </summary>
    Ean13,

    /// <summary>
    /// EAN-8, the short retail barcode for small packages. Takes 7 digits (the check digit is added) or 8
    /// digits (the check digit is verified).
    /// </summary>
    Ean8,

    /// <summary>
    /// UPC-A, the retail product barcode used in North America. Takes 11 digits (the check digit is added)
    /// or 12 digits (the check digit is verified).
    /// </summary>
    UpcA
}
