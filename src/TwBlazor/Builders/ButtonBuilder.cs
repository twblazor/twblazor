// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using TwBlazor.Configuration;
using TwBlazor.Configuration.Components;
using TwBlazor.Enums;
using TwBlazor.Utilities;

namespace TwBlazor.Builders;

/// <summary>
/// Provides utility methods for building button CSS classes.
/// </summary>
public class ButtonBuilder(TwBlazorOptions options, RoundedBuilder roundedBuilder, ColorBuilder colorBuilder)
{
    private TwButtonTheme theme => options.Theme.Components.Require<TwButtonTheme>();

    /// <summary>
    /// Gets the base structure classes.
    /// </summary>
    /// <param name="iconButton">Whether this is an icon button.</param>
    /// <param name="dense">Whether the button has dense padding.</param>
    /// <param name="componentRounded">Component-level Rounded parameter value.</param>
    public string GetBaseClasses(bool iconButton, bool dense, Rounded? componentRounded = null)
    {
        // Precedence: Component Rounded > Theme ButtonRounded > Global DefaultRounded
        var effectiveRounded = componentRounded ?? theme.ButtonRounded ?? options.Theme.Rounded.DefaultRounded;

        return new ClassBuilder(theme.Base)
            .AddClass(theme.Padding, !iconButton && !dense)
            .AddClass(theme.DensePadding, !iconButton && dense)
            .AddClass(theme.IconButton, iconButton)
            .AddClass(roundedBuilder.GetRounded(effectiveRounded), !iconButton)
            // Icon buttons are circles unless the component asks for a rounding of its own.
            .AddClass(roundedBuilder.GetRounded(componentRounded), iconButton && componentRounded is not null)
            .Build();
    }

    /// <summary>
    /// Gets the typography classes (Label Large: 14px, medium weight).
    /// </summary>
    /// <param name="uppercase">Whether to apply uppercase transformation.</param>
    public string GetTypographyClasses(bool uppercase)
    {
        var uppercaseClass = uppercase ? theme.Uppercase : string.Empty;

        return !string.IsNullOrWhiteSpace(uppercaseClass) ?
            $"{theme.Typography} {uppercaseClass}".Trim() : theme.Typography;
    }

    /// <summary>
    /// Gets the cursor state classes.
    /// </summary>
    /// <param name="disabled">Whether the button is disabled.</param>
    /// <param name="readonly">Whether the button is readonly.</param>
    public string GetCursorClasses(bool disabled, bool @readonly)
    {
        if (disabled) return theme.DisabledCursor;
        if (@readonly) return theme.ReadonlyCursor;
        return theme.DefaultCursor;
    }

    /// <summary>
    /// Gets the CSS classes for a button based on its variant and color.
    /// </summary>
    /// <param name="variant">The button variant.</param>
    /// <param name="color">The button color theme.</param>
    /// <param name="disabled">Whether the button is disabled.</param>
    /// <param name="shadowOverride">
    /// When set, an explicit <see cref="Shadow"/> parameter was passed to the component instance and
    /// should take precedence over the <see cref="ButtonVariant.Elevated"/> variant's fixed Lg shadow.
    /// </param>
    /// <returns>A string containing the appropriate CSS classes.</returns>
    public string GetVariantClasses(ButtonVariant? variant, Color? color, bool disabled, Shadow? shadowOverride = null)
    {
        if (disabled)
        {
            return GetDisabledClasses(variant);
        }

        return variant switch
        {
            // Shadow size is intentionally fixed to Lg for the elevated variant, unless the caller
            // explicitly overrode the shadow via the Shadow parameter, which should always win.
            ButtonVariant.Elevated => shadowOverride.HasValue
                ? colorBuilder.GetFilledVariantColor(color)
                : $"{colorBuilder.GetFilledVariantColor(color)} {options.Theme.Shadows.Lg}",
            ButtonVariant.Filled => colorBuilder.GetFilledVariantColor(color),
            ButtonVariant.Outlined => colorBuilder.GetOutlinedVariantColor(color),
            ButtonVariant.Text => colorBuilder.GetTextVariantColor(color),
            _ => string.Empty
        };
    }

    /// <summary>
    /// Gets the disabled state classes for any button variant.
    /// </summary>
    public string GetDisabledClasses(ButtonVariant? variant)
    {
        return variant switch
        {
            ButtonVariant.Outlined => theme.DisabledOutlined,
            ButtonVariant.Text => theme.DisabledText,
            ButtonVariant.Filled or ButtonVariant.Elevated => theme.DisabledFilled,
            _ => string.Empty
        };
    }
}
