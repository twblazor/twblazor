// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using System.Globalization;
using Microsoft.AspNetCore.Components;
using TwBlazor.Configuration.Components;
using TwBlazor.Enums;
using TwBlazor.Localization;
using TwBlazor.QrCoding;
using TwBlazor.Services;
using TwBlazor.Utilities;

namespace TwBlazor.Components;

/// <summary>
/// Shows text, such as a URL, as a scannable QR code.
/// </summary>
/// <remarks>
/// <para>
/// The code is drawn as a single SVG path, so it stays sharp at any size and adds only two elements to the page.
/// It is only re-encoded when <see cref="Value"/>, <see cref="ErrorCorrection"/>, <see cref="BoostErrorCorrection"/>
/// or <see cref="QuietZone"/> change. Nothing is rendered when <see cref="Value"/> is empty or too long to fit; use
/// <see cref="ITwQrCodeService.TryEncode"/> to check a value beforehand.
/// </para>
/// <para>
/// Accessibility: the code is exposed as an image named "QR code: " followed by its <see cref="Value"/>, so people
/// who cannot scan it still learn what it holds. Set <c>AriaLabel</c> to describe it instead (for example
/// "Scan to open the menu"), and show the link as text nearby so everyone can use it without a camera.
/// </para>
/// </remarks>
public partial class TwQrCode : TwBlazorComponentBase
{
    private TwQrCodeTheme theme => options.Theme.Components.Require<TwQrCodeTheme>();

    [Inject] private ITwQrCodeService qrCodeService { get; set; } = null!;

    private QrMatrix? matrix;
    private string path = string.Empty;
    private (string? Value, QrErrorCorrection ErrorCorrection, bool Boost, int QuietZone)? encodedWith;

    /// <summary>
    /// Gets or sets the text to encode, for example a URL, an email address or a Wi-Fi configuration string.
    /// </summary>
    [Parameter] public string? Value { get; set; }

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
    /// Gets or sets the width of the light margin around the code, in modules.
    /// </summary>
    /// <remarks>
    /// Defaults to 4, the minimum the QR code standard asks for. Scanners struggle with less unless the
    /// code already sits on a plain light background.
    /// </remarks>
    [Parameter] public int QuietZone { get; set; } = 4;

    /// <summary>
    /// Gets or sets the size of the QR code.
    /// </summary>
    [Parameter] public QrCodeSize Size { get; set; } = QrCodeSize.Medium;

    private string viewBox
    {
        get
        {
            var dimension = (matrix?.Size ?? 0) + (Math.Max(0, QuietZone) * 2);
            return string.Create(CultureInfo.InvariantCulture, $"0 0 {dimension} {dimension}");
        }
    }

    private string? accessibleName => AriaLabel ?? (AriaLabelledBy == null ? localizer[TwStrings.QrCode_Label, Value ?? string.Empty] : null);

    private string sizeClasses => Size switch
    {
        QrCodeSize.Small => theme.Small,
        QrCodeSize.Large => theme.Large,
        QrCodeSize.ExtraLarge => theme.ExtraLarge,
        _ => theme.Medium
    };

    private string classes => new ClassBuilder(sizeClasses)
        .AddClass(theme.Base)
        .AddClass(roundedBuilder.GetRounded(Rounded))
        .AddClass(shadowBuilder.GetShadow(Shadow))
        .AddClass(Class)
        .Build();

    /// <inheritdoc />
    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        var quietZone = Math.Max(0, QuietZone);
        var key = (Value, ErrorCorrection, BoostErrorCorrection, quietZone);
        if (encodedWith == key)
        {
            return;
        }

        encodedWith = key;
        var encodeOptions = new QrEncodeOptions { ErrorCorrection = ErrorCorrection, BoostErrorCorrection = BoostErrorCorrection };
        if (!string.IsNullOrEmpty(Value) && qrCodeService.TryEncode(Value, encodeOptions, out var encoded))
        {
            matrix = encoded;
            path = qrCodeService.GetSvgPath(encoded, quietZone);
        }
        else
        {
            matrix = null;
            path = string.Empty;
        }
    }
}
