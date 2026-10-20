// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using TwBlazor.Configuration.Components;
using TwBlazor.Enums;
using TwBlazor.Utilities;

namespace TwBlazor.Builders;

/// <summary>
/// Provides utility methods for generating CSS classes for input component variants.
/// </summary>
public class InputVariantBuilder(RoundedBuilder roundedBuilder)
{
    /// <summary>
    /// Gets the Tailwind CSS classes for text input variants.
    /// </summary>
    /// <param name="variant">The input variant.</param>
    /// <param name="theme">Optional theme configuration.</param>
    /// <param name="rounded">The component's rounding. When <see langword="null"/>, the theme's default rounding is used.</param>
    /// <returns>The corresponding Tailwind CSS classes.</returns>
    public string GetClasses(InputVariant variant, TwInputTheme theme, Rounded? rounded = null) => GetClasses(variant, theme, InputFocusStyle.Focus, rounded);

    /// <summary>
    /// Gets the classes for an input variant, with the focus border matched to how the control takes focus.
    /// </summary>
    /// <param name="variant">The input variant.</param>
    /// <param name="theme">The input theme.</param>
    /// <param name="focusStyle">Which of the theme's focus borders to apply.</param>
    /// <param name="rounded">The component's rounding. When <see langword="null"/>, the theme's default rounding is used.</param>
    public string GetClasses(InputVariant variant, TwInputTheme theme, InputFocusStyle focusStyle, Rounded? rounded = null)
    {
        var effectiveRounded = rounded ?? roundedBuilder.DefaultRounded;

        var focusBorder = focusStyle switch
        {
            InputFocusStyle.FocusVisible when !string.IsNullOrEmpty(theme.FocusVisibleBorder) => theme.FocusVisibleBorder,
            InputFocusStyle.FocusWithin when !string.IsNullOrEmpty(theme.FocusWithinBorder) => theme.FocusWithinBorder,
            _ => theme.FocusBorder
        };

        return variant switch
        {
            InputVariant.Default => new ClassBuilder()
                .AddClass(theme.FilledBorder)
                .AddClass(focusBorder)
                .AddClass("bg-transparent px-0")
                .Build(),

            InputVariant.Outlined => new ClassBuilder()
                .AddClass(theme.OutlinedBorder)
                .AddClass(focusBorder)
                .AddClass(roundedBuilder.GetRounded(effectiveRounded))
                .AddClass("bg-transparent px-3")
                .Build(),

            // Only the top corners are rounded here: FilledBorder draws a flat, full-width bottom
            // border (border-b), so rounding all four corners made that border cut across the
            // rounded bottom corners of the filled background instead of following them.
            InputVariant.Filled => new ClassBuilder()
                .AddClass(theme.FilledBorder)
                .AddClass(focusBorder)
                .AddClass(theme.FilledBackgroundColor)
                .AddClass(roundedBuilder.GetRoundedTop(effectiveRounded))
                .AddClass("px-3")
                .Build(),

            _ => new ClassBuilder()
                .AddClass(theme.FilledBorder)
                .AddClass(focusBorder)
                .AddClass("bg-transparent px-0")
                .Build()
        };
    }
}

/// <summary>
/// Which focus border an input control shows.
/// </summary>
public enum InputFocusStyle
{
    /// <summary>Shown whenever the control has focus.</summary>
    Focus,

    /// <summary>Shown only for keyboard focus.</summary>
    FocusVisible,

    /// <summary>Shown on a container while a control inside it has keyboard focus.</summary>
    FocusWithin
}
