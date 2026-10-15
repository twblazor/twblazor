// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using System.Globalization;
using System.Security;
using System.Text;

namespace TwBlazor.QrCoding;

/// <summary>
/// Writes a <see cref="QrMatrix"/> as SVG. Each horizontal run of dark modules becomes one rectangle in a
/// single path, which keeps the markup small and the DOM to one element however large the code is.
/// </summary>
internal static class QrSvgWriter
{
    /// <summary>
    /// The quiet zone the standard asks for, in modules.
    /// </summary>
    internal const int defaultQuietZone = 4;

    /// <summary>
    /// Builds the path data for the dark modules, offset by the quiet zone.
    /// </summary>
    /// <param name="matrix">The encoded code.</param>
    /// <param name="quietZone">The margin, in modules, to leave around the code.</param>
    /// <returns>The value for a path's <c>d</c> attribute.</returns>
    internal static string GetPath(QrMatrix matrix, int quietZone)
    {
        ArgumentNullException.ThrowIfNull(matrix);
        ArgumentOutOfRangeException.ThrowIfNegative(quietZone);

        var builder = new StringBuilder(matrix.Size * matrix.Size);
        for (var y = 0; y < matrix.Size; y++)
        {
            var x = 0;
            while (x < matrix.Size)
            {
                if (!matrix[x, y])
                {
                    x++;
                    continue;
                }

                var start = x;
                while (x < matrix.Size && matrix[x, y])
                {
                    x++;
                }

                var length = x - start;
                builder.Append(CultureInfo.InvariantCulture, $"M{start + quietZone} {y + quietZone}h{length}v1h-{length}z");
            }
        }

        return builder.ToString();
    }

    /// <summary>
    /// Builds a standalone SVG document for the code.
    /// </summary>
    /// <param name="matrix">The encoded code.</param>
    /// <param name="options">The colors, quiet zone and title.</param>
    /// <returns>The SVG markup.</returns>
    internal static string ToSvg(QrMatrix matrix, QrSvgOptions options)
    {
        ArgumentNullException.ThrowIfNull(matrix);
        ArgumentNullException.ThrowIfNull(options);

        var dimension = matrix.Size + (options.QuietZone * 2);
        var path = GetPath(matrix, options.QuietZone);
        var builder = new StringBuilder(path.Length + 400);
        builder.Append(CultureInfo.InvariantCulture, $"<svg xmlns=\"http://www.w3.org/2000/svg\" viewBox=\"0 0 {dimension} {dimension}\" shape-rendering=\"crispEdges\"");
        if (string.IsNullOrWhiteSpace(options.Title))
        {
            builder.Append(" aria-hidden=\"true\">");
        }
        else
        {
            builder.Append(" role=\"img\"><title>").Append(SecurityElement.Escape(options.Title)).Append("</title>");
        }

        builder.Append("<rect width=\"100%\" height=\"100%\" fill=\"").Append(SecurityElement.Escape(options.LightColor)).Append("\"/>");
        builder.Append("<path fill=\"").Append(SecurityElement.Escape(options.DarkColor)).Append("\" d=\"").Append(path).Append("\"/>");
        builder.Append("</svg>");
        return builder.ToString();
    }
}
