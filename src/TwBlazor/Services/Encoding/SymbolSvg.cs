// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using System.Globalization;
using System.Security;
using System.Text;

namespace TwBlazor.Services.Encoding;

/// <summary>
/// Draws any <see cref="IEncodedSymbol"/> (QR codes and barcodes alike) as SVG.
/// </summary>
/// <remarks>
/// Each horizontal run of dark modules becomes one rectangle in a single path, so the DOM holds one element
/// however large the symbol is. One unit of the drawing is one module wide; the rows of a one-dimensional
/// barcode are stretched to the bar height.
/// </remarks>
public static class SymbolSvg
{
    /// <summary>
    /// The default bar height of a one-dimensional barcode, in modules.
    /// </summary>
    internal const int defaultBarHeight = 50;

    private const int captionHeight = 14;

    /// <summary>
    /// Builds the SVG path data for the dark modules, offset by the quiet zone.
    /// </summary>
    /// <param name="symbol">The encoded symbol.</param>
    /// <param name="quietZone">The light margin, in modules, or <see langword="null"/> for the symbol's own.</param>
    /// <param name="barHeight">The bar height of a one-dimensional barcode, in modules.</param>
    /// <returns>The value for a path's <c>d</c> attribute.</returns>
    public static string GetSvgPath(this IEncodedSymbol symbol, int? quietZone = null, int barHeight = defaultBarHeight)
    {
        var layout = SymbolLayout.Create(symbol, quietZone, barHeight);
        var builder = new StringBuilder(symbol.Width * symbol.Height);
        for (var y = 0; y < symbol.Height; y++)
        {
            var top = layout.Top + (y * layout.RowHeight);
            var x = 0;
            while (x < symbol.Width)
            {
                var start = x;
                while (symbol[x, y])
                {
                    x++;
                }

                if (x > start)
                {
                    var length = x - start;
                    builder.Append(CultureInfo.InvariantCulture, $"M{start + layout.QuietZone} {top}h{length}v{layout.RowHeight}h-{length}z");
                }
                else
                {
                    x++;
                }
            }
        }

        return builder.ToString();
    }

    /// <summary>
    /// Gets the SVG <c>viewBox</c> that fits the symbol and its quiet zone.
    /// </summary>
    /// <param name="symbol">The encoded symbol.</param>
    /// <param name="quietZone">The light margin, in modules, or <see langword="null"/> for the symbol's own.</param>
    /// <param name="barHeight">The bar height of a one-dimensional barcode, in modules.</param>
    /// <returns>The value for an SVG's <c>viewBox</c> attribute.</returns>
    public static string GetSvgViewBox(this IEncodedSymbol symbol, int? quietZone = null, int barHeight = defaultBarHeight)
    {
        var layout = SymbolLayout.Create(symbol, quietZone, barHeight);
        return string.Create(CultureInfo.InvariantCulture, $"0 0 {layout.Width} {layout.Height}");
    }

    /// <summary>
    /// Builds a standalone SVG document for the symbol, for saving, emailing or embedding outside a component.
    /// </summary>
    /// <param name="symbol">The encoded symbol.</param>
    /// <param name="options">The colors, sizes, caption and title, or <see langword="null"/> for <see cref="SymbolSvgOptions.Default"/>.</param>
    /// <returns>The SVG markup.</returns>
    public static string ToSvg(this IEncodedSymbol symbol, SymbolSvgOptions? options = null)
    {
        options ??= SymbolSvgOptions.Default;
        var layout = SymbolLayout.Create(symbol, options.QuietZone, options.BarHeight);
        var path = symbol.GetSvgPath(options.QuietZone, options.BarHeight);
        var hasCaption = !string.IsNullOrEmpty(options.Caption);
        var height = layout.Height + (hasCaption ? captionHeight : 0);
        var dark = SecurityElement.Escape(options.DarkColor);

        var builder = new StringBuilder(path.Length + 500);
        builder.Append(CultureInfo.InvariantCulture, $"<svg xmlns=\"http://www.w3.org/2000/svg\" viewBox=\"0 0 {layout.Width} {height}\" shape-rendering=\"crispEdges\"");
        if (string.IsNullOrWhiteSpace(options.Title))
        {
            builder.Append(" aria-hidden=\"true\">");
        }
        else
        {
            builder.Append(" role=\"img\"><title>").Append(SecurityElement.Escape(options.Title)).Append("</title>");
        }

        if (options.LightColor is not null)
        {
            builder.Append("<rect width=\"100%\" height=\"100%\" fill=\"").Append(SecurityElement.Escape(options.LightColor)).Append("\"/>");
        }

        builder.Append("<path fill=\"").Append(dark).Append("\" d=\"").Append(path).Append("\"/>");
        if (hasCaption)
        {
            builder.Append(CultureInfo.InvariantCulture, $"<text x=\"{layout.Width / 2.0}\" y=\"{layout.Height + 11}\" font-family=\"monospace\" font-size=\"11\" text-anchor=\"middle\" fill=\"");
            builder.Append(dark).Append("\">").Append(SecurityElement.Escape(options.Caption)).Append("</text>");
        }

        builder.Append("</svg>");
        return builder.ToString();
    }

    /// <summary>
    /// Builds a <c>data:image/svg+xml</c> URI for the symbol, usable as a download link or an <c>&lt;img&gt;</c> source.
    /// </summary>
    /// <param name="symbol">The encoded symbol.</param>
    /// <param name="options">The colors, sizes, caption and title, or <see langword="null"/> for <see cref="SymbolSvgOptions.Default"/>.</param>
    /// <returns>The data URI.</returns>
    public static string ToSvgDataUri(this IEncodedSymbol symbol, SymbolSvgOptions? options = null) =>
        "data:image/svg+xml;charset=utf-8," + Uri.EscapeDataString(symbol.ToSvg(options));

    /// <summary>
    /// Where the modules sit in the drawing. A one-dimensional barcode has its quiet zone on the left and right
    /// only, and its single row stretched to the bar height.
    /// </summary>
    private readonly record struct SymbolLayout(int QuietZone, int Top, int RowHeight, int Width, int Height)
    {
        internal static SymbolLayout Create(IEncodedSymbol symbol, int? quietZone, int barHeight)
        {
            ArgumentNullException.ThrowIfNull(symbol);
            ArgumentOutOfRangeException.ThrowIfLessThan(barHeight, 1);
            if (quietZone < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(quietZone), quietZone, "The quiet zone cannot be negative.");
            }

            var margin = quietZone ?? symbol.QuietZone;

            var oneDimensional = symbol.Height == 1;
            var rowHeight = oneDimensional ? barHeight : 1;
            var top = oneDimensional ? 0 : margin;
            return new SymbolLayout(margin, top, rowHeight, symbol.Width + (margin * 2), (symbol.Height * rowHeight) + (top * 2));
        }
    }
}
