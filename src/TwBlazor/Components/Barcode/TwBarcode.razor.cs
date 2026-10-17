// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using Microsoft.AspNetCore.Components;
using System.Diagnostics.CodeAnalysis;
using TwBlazor.Configuration.Components;
using TwBlazor.Enums;
using TwBlazor.Localization;
using TwBlazor.Services;
using TwBlazor.Services.Encoding;

namespace TwBlazor.Components;

/// <summary>
/// Shows a value, such as a product number or tracking reference, as a scannable one-dimensional barcode.
/// </summary>
/// <remarks>
/// <para>
/// The bars are drawn as a single SVG path, so they stay sharp at any size. The barcode is only re-encoded when
/// <c>Value</c>, <see cref="Format"/> or <c>QuietZone</c> change. Nothing is rendered when <c>Value</c> is empty or
/// cannot be encoded in the chosen <see cref="Format"/>; use <see cref="ITwBarcodeService.TryEncode"/> to check a
/// value beforehand.
/// </para>
/// <para>
/// Accessibility: the barcode is exposed as an image named "Barcode: " followed by its text, so people who cannot
/// scan it still learn what it holds. Set <c>AriaLabel</c> to describe it instead.
/// </para>
/// </remarks>
public partial class TwBarcode : TwEncodedSymbolBase<Barcode>
{
    private TwBarcodeTheme theme => options.Theme.Components.Require<TwBarcodeTheme>();

    [Inject] private ITwBarcodeService barcodeService { get; set; } = null!;

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
    /// Gets or sets the height of the bars. The width follows from it and the length of the value.
    /// </summary>
    [Parameter] public BarcodeSize Size { get; set; } = BarcodeSize.Medium;

    /// <inheritdoc />
    protected override string labelKey => TwStrings.Barcode_Label;

    /// <inheritdoc />
    protected override string labelText => symbol?.Text ?? string.Empty;

    /// <inheritdoc />
    protected override object? encodingSettings => Format;

    private string sizeClasses => Size switch
    {
        BarcodeSize.Small => theme.Small,
        BarcodeSize.Large => theme.Large,
        _ => theme.Medium
    };

    private string classes => BuildRootClasses(theme);

    /// <inheritdoc />
    protected override bool TryEncode(string value, [NotNullWhen(true)] out Barcode? encoded) =>
        barcodeService.TryEncode(value, Format, out encoded);
}
