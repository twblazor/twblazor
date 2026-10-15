// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using System.Globalization;
using Microsoft.AspNetCore.Components;
using TwBlazor.Barcoding;
using TwBlazor.Configuration.Components;
using TwBlazor.Enums;
using TwBlazor.Localization;
using TwBlazor.Services;
using TwBlazor.Utilities;

namespace TwBlazor.Components;

/// <summary>
/// Shows a value, such as a product number or tracking reference, as a scannable one-dimensional barcode.
/// </summary>
/// <remarks>
/// <para>
/// The bars are drawn as a single SVG path, so they stay sharp at any size. The barcode is only re-encoded when
/// <see cref="Value"/>, <see cref="Format"/> or <see cref="QuietZone"/> change. Nothing is rendered when
/// <see cref="Value"/> is empty or cannot be encoded in the chosen <see cref="Format"/>; use
/// <see cref="ITwBarcodeService.TryEncode"/> to check a value beforehand.
/// </para>
/// <para>
/// Accessibility: the barcode is exposed as an image named "Barcode: " followed by its text, so people who cannot
/// scan it still learn what it holds. Set <c>AriaLabel</c> to describe it instead.
/// </para>
/// </remarks>
public partial class TwBarcode : TwBlazorComponentBase
{
    private TwBarcodeTheme theme => options.Theme.Components.Require<TwBarcodeTheme>();

    [Inject] private ITwBarcodeService barcodeService { get; set; } = null!;

    private Barcode? barcode;
    private string path = string.Empty;
    private string viewBox = string.Empty;
    private (string? Value, BarcodeFormat Format, int? QuietZone)? encodedWith;

    /// <summary>
    /// Gets or sets the value to encode. What it may hold depends on <see cref="Format"/>.
    /// </summary>
    [Parameter] public string? Value { get; set; }

    /// <summary>
    /// Gets or sets the barcode format.
    /// </summary>
    /// <remarks>
    /// Defaults to <see cref="BarcodeFormat.Code128"/>, which holds any ASCII text.
    /// </remarks>
    [Parameter] public BarcodeFormat Format { get; set; } = BarcodeFormat.Code128;

    /// <summary>
    /// Gets or sets whether to print the human readable text under the bars.
    /// </summary>
    /// <remarks>
    /// Defaults to <see langword="true"/>, so the value can still be typed in when a scan fails.
    /// </remarks>
    [Parameter] public bool ShowText { get; set; } = true;

    /// <summary>
    /// Gets or sets the light margin on each side of the bars, in modules.
    /// </summary>
    /// <remarks>
    /// Defaults to <see langword="null"/>, which uses the margin the format needs (10 modules for Code 128 and
    /// Code 39, 11 for EAN-13 and UPC-A, 7 for EAN-8). Scanners struggle with less.
    /// </remarks>
    [Parameter] public int? QuietZone { get; set; }

    /// <summary>
    /// Gets or sets the height of the bars. The width follows from it and the length of the value.
    /// </summary>
    [Parameter] public BarcodeSize Size { get; set; } = BarcodeSize.Medium;

    private string? accessibleName => AriaLabel ?? (AriaLabelledBy == null ? localizer[TwStrings.Barcode_Label, barcode?.Text ?? string.Empty] : null);

    private string sizeClasses => Size switch
    {
        BarcodeSize.Small => theme.Small,
        BarcodeSize.Large => theme.Large,
        _ => theme.Medium
    };

    private string classes => new ClassBuilder(theme.Base)
        .AddClass(roundedBuilder.GetRounded(Rounded))
        .AddClass(shadowBuilder.GetShadow(Shadow))
        .AddClass(Class)
        .Build();

    /// <inheritdoc />
    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        var quietZone = QuietZone is { } zone ? Math.Max(0, zone) : (int?)null;
        var key = (Value, Format, quietZone);
        if (encodedWith == key)
        {
            return;
        }

        encodedWith = key;
        if (!string.IsNullOrEmpty(Value) && barcodeService.TryEncode(Value, Format, out var encoded))
        {
            var margin = quietZone ?? encoded.QuietZone;
            barcode = encoded;
            path = barcodeService.GetSvgPath(encoded, margin, BarcodeSvgWriter.defaultHeight);
            viewBox = string.Create(CultureInfo.InvariantCulture, $"0 0 {encoded.Width + (margin * 2)} {BarcodeSvgWriter.defaultHeight}");
        }
        else
        {
            barcode = null;
            path = string.Empty;
            viewBox = string.Empty;
        }
    }
}
