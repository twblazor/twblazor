// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using TwBlazor.QrCoding;

namespace TwBlazor.Services;

/// <summary>
/// Encodes text or bytes into QR codes and draws them as SVG, with no third-party dependencies.
/// </summary>
/// <remarks>
/// Encoding produces a <see cref="QrMatrix"/>, a plain grid of dark and light modules that any renderer can draw.
/// <see cref="Components.TwQrCode"/> uses this service to show a code on the page; call it directly to build a
/// code for a download, an email or your own renderer. The service is stateless and safe to share.
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
    /// <exception cref="QrDataTooLongException">The text does not fit at the requested error correction level.</exception>
    QrMatrix Encode(string value, QrEncodeOptions? options = null);

    /// <summary>
    /// Encodes raw bytes into a QR code.
    /// </summary>
    /// <param name="data">The bytes to encode.</param>
    /// <param name="options">The encoding settings, or <see langword="null"/> for <see cref="QrEncodeOptions.Default"/>.</param>
    /// <returns>The encoded matrix.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="data"/> is <see langword="null"/>.</exception>
    /// <exception cref="QrDataTooLongException">The data does not fit at the requested error correction level.</exception>
    QrMatrix Encode(byte[] data, QrEncodeOptions? options = null);

    /// <summary>
    /// Tries to encode text into a QR code, without throwing when it is too long.
    /// </summary>
    /// <param name="value">The text to encode.</param>
    /// <param name="options">The encoding settings, or <see langword="null"/> for <see cref="QrEncodeOptions.Default"/>.</param>
    /// <param name="matrix">The encoded matrix, or <see langword="null"/> when the text does not fit.</param>
    /// <returns><see langword="true"/> when the text was encoded.</returns>
    bool TryEncode(string? value, QrEncodeOptions? options, [NotNullWhen(true)] out QrMatrix? matrix);

    /// <summary>
    /// Builds SVG path data that draws the dark modules, one unit per module, offset by the quiet zone.
    /// The full drawing is <c>matrix.Size + quietZone * 2</c> units square.
    /// </summary>
    /// <param name="matrix">The encoded code.</param>
    /// <param name="quietZone">The light margin to leave around the code, in modules.</param>
    /// <returns>The value for a path's <c>d</c> attribute.</returns>
    string GetSvgPath(QrMatrix matrix, int quietZone = 4);

    /// <summary>
    /// Builds a standalone SVG document for the code, for saving or embedding outside a component.
    /// </summary>
    /// <param name="matrix">The encoded code.</param>
    /// <param name="options">The colors, quiet zone and title, or <see langword="null"/> for <see cref="QrSvgOptions.Default"/>.</param>
    /// <returns>The SVG markup.</returns>
    string ToSvg(QrMatrix matrix, QrSvgOptions? options = null);

    /// <summary>
    /// Builds a <c>data:image/svg+xml</c> URI for the code, usable as an <c>&lt;img&gt;</c> source or a download link.
    /// </summary>
    /// <param name="matrix">The encoded code.</param>
    /// <param name="options">The colors, quiet zone and title, or <see langword="null"/> for <see cref="QrSvgOptions.Default"/>.</param>
    /// <returns>The data URI.</returns>
    string ToSvgDataUri(QrMatrix matrix, QrSvgOptions? options = null);
}
