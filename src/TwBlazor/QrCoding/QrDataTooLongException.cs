// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

namespace TwBlazor.QrCoding;

/// <summary>
/// Thrown when data does not fit in a QR code at the requested error correction level and version range.
/// </summary>
/// <remarks>
/// Lower the error correction level, raise <see cref="QrEncodeOptions.MaxVersion"/>, or shorten the data
/// (for example, use a short link instead of a long URL).
/// </remarks>
public sealed class QrDataTooLongException : ArgumentException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="QrDataTooLongException"/> class.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public QrDataTooLongException(string message) : base(message)
    {
    }
}
