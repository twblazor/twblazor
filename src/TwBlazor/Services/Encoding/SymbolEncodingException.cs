// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;

namespace TwBlazor.Services.Encoding;

/// <summary>
/// Thrown when a value cannot be encoded as the requested symbol: for example text too long for a QR code at the
/// chosen error correction, a letter in an EAN-13, or a wrong check digit.
/// </summary>
/// <remarks>
/// Use the services' <c>TryEncode</c> methods to check a value without catching this.
/// </remarks>
public sealed class SymbolEncodingException : ArgumentException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SymbolEncodingException"/> class.
    /// </summary>
    /// <param name="message">The message that describes why the value cannot be encoded.</param>
    public SymbolEncodingException(string message) : base(message)
    {
    }

    /// <summary>
    /// Runs an encoder, turning a <see cref="SymbolEncodingException"/> into a <see langword="false"/> result.
    /// </summary>
    /// <typeparam name="TSymbol">The kind of symbol encoded.</typeparam>
    /// <param name="value">The value to encode, or <see langword="null"/>.</param>
    /// <param name="encode">The encoder to run.</param>
    /// <param name="symbol">The encoded symbol, or <see langword="null"/> when the value cannot be encoded.</param>
    /// <returns><see langword="true"/> when the value was encoded.</returns>
    internal static bool TryEncode<TSymbol>(string? value, Func<string, TSymbol> encode, [NotNullWhen(true)] out TSymbol? symbol)
        where TSymbol : class
    {
        symbol = null;
        if (value is null)
        {
            return false;
        }

        try
        {
            symbol = encode(value);
            return true;
        }
        catch (SymbolEncodingException)
        {
            return false;
        }
    }
}
