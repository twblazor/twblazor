// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

namespace TwBlazor.Enums;

/// <summary>
/// Defines the barcode height variants. The width follows from the height and the length of the value.
/// </summary>
public enum BarcodeSize
{
    /// <summary>
    /// Small barcode (height 10).
    /// </summary>
    Small,

    /// <summary>
    /// Medium barcode (height 16) - Default.
    /// </summary>
    Medium,

    /// <summary>
    /// Large barcode (height 24).
    /// </summary>
    Large
}
