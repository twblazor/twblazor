// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using TwBlazor.Services.Encoding;

namespace TwBlazor.Services;

/// <summary>
/// Encodes text or bytes into QR codes, with no third-party dependencies.
/// </summary>
/// <remarks>
/// Encoding produces a <see cref="QrMatrix"/>, a plain grid of dark and light modules. Draw it with the
/// <see cref="SymbolSvg"/> methods (<c>ToSvg</c>, <c>ToSvgDataUri</c>, <c>GetSvgPath</c>) or your own renderer.
/// <see cref="Components.TwQrCode"/> uses this service to show a code on the page. The service is stateless and
/// safe to share.
/// </remarks>
public interface ITwQrCodeService
{
    /// <summary>
    /// Encodes text into a QR code, using the densest mode the text allows (numeric, alphanumeric or UTF-8 bytes).
    /// </summary>
    /// <param name="value">The text to encode, for example a URL.</param>
    /// <param name="options">The encoding settings, or <see langword="null"/> for <see cref="QrEncodeOptions.Default"/>.</param>
    /// <returns>The encoded matrix.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/>.</exception>
    /// <exception cref="SymbolEncodingException">The text does not fit at the requested error correction level.</exception>
    QrMatrix Encode(string value, QrEncodeOptions? options = null);

    /// <summary>
    /// Encodes raw bytes into a QR code.
    /// </summary>
    /// <param name="data">The bytes to encode.</param>
    /// <param name="options">The encoding settings, or <see langword="null"/> for <see cref="QrEncodeOptions.Default"/>.</param>
    /// <returns>The encoded matrix.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="data"/> is <see langword="null"/>.</exception>
    /// <exception cref="SymbolEncodingException">The data does not fit at the requested error correction level.</exception>
    QrMatrix Encode(byte[] data, QrEncodeOptions? options = null);

    /// <summary>
    /// Tries to encode text into a QR code, without throwing when it is too long.
    /// </summary>
    /// <param name="value">The text to encode.</param>
    /// <param name="options">The encoding settings, or <see langword="null"/> for <see cref="QrEncodeOptions.Default"/>.</param>
    /// <param name="matrix">The encoded matrix, or <see langword="null"/> when the text does not fit.</param>
    /// <returns><see langword="true"/> when the text was encoded.</returns>
    bool TryEncode(string? value, QrEncodeOptions? options, [NotNullWhen(true)] out QrMatrix? matrix);
}
