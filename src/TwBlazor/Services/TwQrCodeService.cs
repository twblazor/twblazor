// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using TwBlazor.QrCoding;

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
    public bool TryEncode(string? value, QrEncodeOptions? options, [NotNullWhen(true)] out QrMatrix? matrix)
    {
        if (value is null)
        {
            matrix = null;
            return false;
        }

        try
        {
            matrix = Encode(value, options);
            return true;
        }
        catch (QrDataTooLongException)
        {
            matrix = null;
            return false;
        }
    }

    /// <inheritdoc />
    public string GetSvgPath(QrMatrix matrix, int quietZone = QrSvgWriter.defaultQuietZone) => QrSvgWriter.GetPath(matrix, quietZone);

    /// <inheritdoc />
    public string ToSvg(QrMatrix matrix, QrSvgOptions? options = null) => QrSvgWriter.ToSvg(matrix, options ?? QrSvgOptions.Default);

    /// <inheritdoc />
    public string ToSvgDataUri(QrMatrix matrix, QrSvgOptions? options = null) =>
        "data:image/svg+xml;charset=utf-8," + Uri.EscapeDataString(ToSvg(matrix, options));
}
