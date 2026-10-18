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
/// Shows text, such as a URL, as a scannable QR code.
/// </summary>
/// <remarks>
/// <para>
/// The code is drawn as a single SVG path, so it stays sharp at any size and adds only two elements to the page.
/// It is only re-encoded when <c>Value</c>, <see cref="ErrorCorrection"/>, <see cref="BoostErrorCorrection"/>
/// or <c>QuietZone</c> change. Nothing is rendered when <c>Value</c> is empty or too long to fit; use
/// <see cref="ITwQrCodeService.TryEncode"/> to check a value beforehand.
/// </para>
/// <para>
/// Accessibility: the code is exposed as an image named "QR code: " followed by its <c>Value</c>, so people
/// who cannot scan it still learn what it holds. Set <c>AriaLabel</c> to describe it instead (for example
/// "Scan to open the menu"), and show the link as text nearby so everyone can use it without a camera.
/// </para>
/// </remarks>
public partial class TwQrCode : TwEncodedSymbolBase<QrMatrix>
{
    private TwQrCodeTheme theme => options.Theme.Components.Require<TwQrCodeTheme>();

    [Inject] private ITwQrCodeService qrCodeService { get; set; } = null!;

    /// <summary>
    /// Gets or sets the minimum share of the code that can be damaged or covered and still scan.
    /// </summary>
    /// <remarks>
    /// Defaults to <see cref="QrErrorCorrection.Medium"/>. Higher levels make a denser code for the same text.
    /// </remarks>
    [Parameter] public QrErrorCorrection ErrorCorrection { get; set; } = QrErrorCorrection.Medium;

    /// <summary>
    /// Gets or sets whether to raise <see cref="ErrorCorrection"/> when that does not make the code any denser.
    /// </summary>
    /// <remarks>
    /// Defaults to <see langword="true"/>.
    /// </remarks>
    [Parameter] public bool BoostErrorCorrection { get; set; } = true;

    /// <summary>
    /// Gets or sets the size of the QR code.
    /// </summary>
    [Parameter] public QrCodeSize Size { get; set; } = QrCodeSize.Medium;

    /// <inheritdoc />
    protected override string labelKey => TwStrings.QrCode_Label;

    /// <inheritdoc />
    protected override object? encodingSettings => (ErrorCorrection, BoostErrorCorrection);

    private string sizeClasses => Size switch
    {
        QrCodeSize.Small => theme.Small,
        QrCodeSize.Large => theme.Large,
        QrCodeSize.ExtraLarge => theme.ExtraLarge,
        _ => theme.Medium
    };

    private string classes => BuildRootClasses(theme, sizeClasses);

    /// <inheritdoc />
    protected override bool TryEncode(string value, [NotNullWhen(true)] out QrMatrix? encoded) =>
        qrCodeService.TryEncode(value, new QrEncodeOptions { ErrorCorrection = ErrorCorrection, BoostErrorCorrection = BoostErrorCorrection }, out encoded);
}
