// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

namespace TwBlazor.Enums;

/// <summary>
/// Defines how much of a QR code can be damaged, dirty or covered and still be read.
/// </summary>
/// <remarks>
/// A higher level adds more recovery data, so the same text needs a larger, denser code. Use
/// <see cref="Medium"/> for most screens, and <see cref="Quartile"/> or <see cref="High"/> for printed
/// codes that may get scuffed or for codes with a picture placed over them.
/// </remarks>
public enum QrErrorCorrection
{
    /// <summary>
    /// Recovers about 7% of the code. Gives the smallest code, for clean screens only.
    /// </summary>
    Low,

    /// <summary>
    /// Recovers about 15% of the code - Default. A good balance for most uses.
    /// </summary>
    Medium,

    /// <summary>
    /// Recovers about 25% of the code. Suits printed codes that may get worn.
    /// </summary>
    Quartile,

    /// <summary>
    /// Recovers about 30% of the code. Suits harsh conditions or a picture placed over the centre.
    /// </summary>
    High
}
