// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

namespace TwBlazor.Barcoding;

/// <summary>
/// Thrown when a value cannot be encoded in the requested barcode format, for example a letter in an EAN-13,
/// a lower case letter in Code 39 or a wrong check digit.
/// </summary>
public sealed class BarcodeValueException : ArgumentException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BarcodeValueException"/> class.
    /// </summary>
    /// <param name="message">The message that describes the problem with the value.</param>
    public BarcodeValueException(string message) : base(message)
    {
    }
}
