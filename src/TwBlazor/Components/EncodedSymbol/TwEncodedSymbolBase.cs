// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using Microsoft.AspNetCore.Components;
using System.Diagnostics.CodeAnalysis;
using TwBlazor.Configuration.Components;
using TwBlazor.Services.Encoding;
using TwBlazor.Utilities;

namespace TwBlazor.Components;

/// <summary>
/// The shared behaviour of the scannable symbol components, <see cref="TwQrCode"/> and <see cref="TwBarcode"/>:
/// encoding the <see cref="Value"/> only when it would change, drawing it as one SVG path, naming it for
/// assistive technology and applying the shared theme colors.
/// </summary>
/// <typeparam name="TSymbol">The kind of symbol the component draws.</typeparam>
public abstract class TwEncodedSymbolBase<TSymbol> : TwBlazorComponentBase where TSymbol : class, IEncodedSymbol
{
    private object? encodedWith;

    /// <summary>
    /// Gets or sets the value to encode. Nothing is rendered while it is empty or cannot be encoded.
    /// </summary>
    [Parameter] public string? Value { get; set; }

    /// <summary>
    /// Gets or sets the light margin around the symbol, in modules.
    /// </summary>
    /// <remarks>
    /// Defaults to <see langword="null"/>, which uses the margin the symbology needs. Scanners struggle with less
    /// unless the symbol already sits on a plain background.
    /// </remarks>
    [Parameter] public int? QuietZone { get; set; }

    /// <summary>
    /// Gets the encoded symbol, or <see langword="null"/> when there is nothing to draw.
    /// </summary>
    protected TSymbol? symbol { get; private set; }

    /// <summary>
    /// Gets the SVG path data for the dark modules.
    /// </summary>
    protected string path { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the SVG <c>viewBox</c> that fits the symbol and its quiet zone.
    /// </summary>
    protected string viewBox { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the resource key of the default accessible name, a format string taking <see cref="labelText"/>.
    /// </summary>
    protected abstract string labelKey { get; }

    /// <summary>
    /// Gets the text the default accessible name describes. Defaults to <see cref="Value"/>.
    /// </summary>
    protected virtual string labelText => Value ?? string.Empty;

    /// <summary>
    /// Gets the component's other settings that change the encoding, so a change to any of them re-encodes.
    /// </summary>
    protected virtual object? encodingSettings => null;

    /// <summary>
    /// Gets the accessible name: <c>AriaLabel</c>, or the default name unless <c>AriaLabelledBy</c> names it instead.
    /// </summary>
    protected string? accessibleName => AriaLabel ?? (AriaLabelledBy == null ? localizer[labelKey, labelText] : null);

    /// <summary>
    /// Encodes a value.
    /// </summary>
    /// <param name="value">The non-empty value to encode.</param>
    /// <param name="encoded">The encoded symbol, or <see langword="null"/> when the value cannot be encoded.</param>
    /// <returns><see langword="true"/> when the value was encoded.</returns>
    protected abstract bool TryEncode(string value, [NotNullWhen(true)] out TSymbol? encoded);

    /// <summary>
    /// Builds the classes for the root element: the theme's structure and colors, then rounding, shadow and
    /// <c>Class</c>.
    /// </summary>
    /// <param name="theme">The component's theme.</param>
    /// <param name="leading">Extra classes to start with, such as the size.</param>
    /// <returns>The merged classes.</returns>
    protected string BuildRootClasses(TwEncodedSymbolTheme theme, string leading = "") => new ClassBuilder(leading)
        .AddClass(theme.Base)
        .AddClass(theme.Background)
        .AddClass(theme.Foreground)
        .AddClass(roundedBuilder.GetRounded(Rounded))
        .AddClass(shadowBuilder.GetShadow(Shadow))
        .AddClass(Class)
        .Build();

    /// <inheritdoc />
    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        var quietZone = QuietZone is { } zone ? Math.Max(0, zone) : (int?)null;
        object key = (Value, quietZone, encodingSettings);
        if (Equals(encodedWith, key))
        {
            return;
        }

        encodedWith = key;
        if (!string.IsNullOrEmpty(Value) && TryEncode(Value, out var encoded))
        {
            symbol = encoded;
            path = encoded.GetSvgPath(quietZone);
            viewBox = encoded.GetSvgViewBox(quietZone);
        }
        else
        {
            symbol = null;
            path = string.Empty;
            viewBox = string.Empty;
        }
    }
}
