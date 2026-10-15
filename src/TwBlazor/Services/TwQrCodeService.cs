// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using TwBlazor.Services.Encoding;

namespace TwBlazor.Services;

/// <summary>
/// The default <see cref="ITwQrCodeService"/>, built on the library's own ISO/IEC 18004 encoder.
/// </summary>
public sealed class TwQrCodeService : ITwQrCodeService
{
    /// <inheritdoc />
    public QrMatrix Encode(string value, QrEncodeOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(value);
        return QrEncoder.Encode(QrSegment.FromText(value), options ?? QrEncodeOptions.Default);
    }

    /// <inheritdoc />
    public QrMatrix Encode(byte[] data, QrEncodeOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(data);
        IReadOnlyList<QrSegment> segments = data.Length == 0 ? [] : [QrSegment.MakeBytes(data)];
        return QrEncoder.Encode(segments, options ?? QrEncodeOptions.Default);
    }

    /// <inheritdoc />
    public bool TryEncode(string? value, QrEncodeOptions? options, [NotNullWhen(true)] out QrMatrix? matrix) =>
        SymbolEncodingException.TryEncode(value, text => Encode(text, options), out matrix);
}
