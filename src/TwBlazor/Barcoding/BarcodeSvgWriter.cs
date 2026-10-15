// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using System.Globalization;
using System.Security;
using System.Text;

namespace TwBlazor.Barcoding;

/// <summary>
/// Writes a <see cref="Barcode"/> as SVG. Each bar becomes one rectangle in a single path, so the DOM holds
/// one element however long the barcode is.
/// </summary>
internal static class BarcodeSvgWriter
{
    /// <summary>
    /// The default bar height, in modules.
    /// </summary>
    internal const int defaultHeight = 50;

    private const int textHeight = 14;

    /// <summary>
    /// Builds the path data for the bars, offset by the quiet zone.
    /// </summary>
    /// <param name="barcode">The encoded barcode.</param>
    /// <param name="quietZone">The margin, in modules, to leave on each side.</param>
    /// <param name="height">The height of the bars, in modules.</param>
    /// <returns>The value for a path's <c>d</c> attribute.</returns>
    internal static string GetPath(Barcode barcode, int quietZone, int height)
    {
        ArgumentNullException.ThrowIfNull(barcode);
        ArgumentOutOfRangeException.ThrowIfNegative(quietZone);
        ArgumentOutOfRangeException.ThrowIfLessThan(height, 1);

        var builder = new StringBuilder(barcode.Width * 4);
        var x = 0;
        while (x < barcode.Width)
        {
            if (!barcode[x])
            {
                x++;
                continue;
            }

            var start = x;
            while (barcode[x])
            {
                x++;
            }

            var width = x - start;
            builder.Append(CultureInfo.InvariantCulture, $"M{start + quietZone} 0h{width}v{height}h-{width}z");
        }

        return builder.ToString();
    }

    /// <summary>
    /// Builds a standalone SVG document for the barcode.
    /// </summary>
    /// <param name="barcode">The encoded barcode.</param>
    /// <param name="options">The colors, sizes, text and title.</param>
    /// <returns>The SVG markup.</returns>
    internal static string ToSvg(Barcode barcode, BarcodeSvgOptions options)
    {
        ArgumentNullException.ThrowIfNull(barcode);
        ArgumentNullException.ThrowIfNull(options);

        var quietZone = options.QuietZone ?? barcode.QuietZone;
        var path = GetPath(barcode, quietZone, options.Height);
        var width = barcode.Width + (quietZone * 2);
        var height = options.Height + (options.ShowText ? textHeight : 0);
        var dark = SecurityElement.Escape(options.DarkColor);

        var builder = new StringBuilder(path.Length + 500);
        builder.Append(CultureInfo.InvariantCulture, $"<svg xmlns=\"http://www.w3.org/2000/svg\" viewBox=\"0 0 {width} {height}\" shape-rendering=\"crispEdges\"");
        if (string.IsNullOrWhiteSpace(options.Title))
        {
            builder.Append(" aria-hidden=\"true\">");
        }
        else
        {
            builder.Append(" role=\"img\"><title>").Append(SecurityElement.Escape(options.Title)).Append("</title>");
        }

        builder.Append("<rect width=\"100%\" height=\"100%\" fill=\"").Append(SecurityElement.Escape(options.LightColor)).Append("\"/>");
        builder.Append("<path fill=\"").Append(dark).Append("\" d=\"").Append(path).Append("\"/>");
        if (options.ShowText)
        {
            builder.Append(CultureInfo.InvariantCulture, $"<text x=\"{width / 2.0}\" y=\"{options.Height + 11}\" font-family=\"monospace\" font-size=\"11\" text-anchor=\"middle\" fill=\"");
            builder.Append(dark).Append("\">").Append(SecurityElement.Escape(barcode.Text)).Append("</text>");
        }

        builder.Append("</svg>");
        return builder.ToString();
    }
}
