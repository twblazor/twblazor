// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using TwBlazor.Builders;
using TwBlazor.Configuration;
using TwBlazor.Enums;

namespace TwBlazor.Utilities;

/// <summary>
/// Resolves the colour, size and weight classes shared by the text and link components.
/// </summary>
internal static class TextClasses
{
    /// <summary>
    /// Resolves an explicit semantic <see cref="Color"/> first, then a neutral <see cref="TextTone"/>.
    /// </summary>
    public static string Tone(TwBlazorOptions options, ColorBuilder colorBuilder, Color? color, TextTone? tone)
    {
        if (color is not null)
        {
            return colorBuilder.GetTextColor(color);
        }

        return tone switch
        {
            TextTone.Default => options.Theme.Colors.NeutralText.Heading,
            TextTone.Secondary => options.Theme.Colors.NeutralText.Secondary,
            TextTone.Muted => options.Theme.Colors.NeutralText.Muted,
            TextTone.Subtle => options.Theme.Colors.NeutralText.Subtle,
            _ => string.Empty
        };
    }

    /// <summary>
    /// Resolves a size on the theme's font-size scale.
    /// </summary>
    public static string Size(TwBlazorTypography typography, TextSize? size) => size switch
    {
        TextSize.Xxs => typography.Size.Xxs,
        TextSize.Xs => typography.Size.Xs,
        TextSize.Sm => typography.Size.Sm,
        TextSize.Base => typography.Size.Base,
        TextSize.Lg => typography.Size.Lg,
        TextSize.Xl => typography.Size.Xl,
        TextSize.Xl2 => typography.Size.Xl2,
        TextSize.Xl3 => typography.Size.Xl3,
        TextSize.Xl4 => typography.Size.Xl4,
        TextSize.Xl5 => typography.Size.Xl5,
        _ => string.Empty
    };

    /// <summary>
    /// Resolves a weight on the theme's font-weight scale.
    /// </summary>
    public static string Weight(TwBlazorTypography typography, TextWeight? weight) => weight switch
    {
        TextWeight.Normal => typography.Weight.Normal,
        TextWeight.Medium => typography.Weight.Medium,
        TextWeight.Semibold => typography.Weight.Semibold,
        TextWeight.Bold => typography.Weight.Bold,
        _ => string.Empty
    };
}
