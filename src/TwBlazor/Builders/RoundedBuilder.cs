// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using TwBlazor.Configuration;
using TwBlazor.Enums;

namespace TwBlazor.Builders;

/// <summary>
/// Provides utility methods for generating CSS border-radius classes.
/// </summary>
public class RoundedBuilder(TwBlazorOptions options)
{
    /// <summary>
    /// Gets the theme's default rounded level.
    /// </summary>
    public Rounded DefaultRounded => options.Theme.Rounded.DefaultRounded;

    /// <summary>
    /// Gets the Tailwind CSS border-radius class for the default rounded level defined in the theme.
    /// </summary>
    /// <returns>The corresponding Tailwind CSS class.</returns>
    public string GetRounded() => GetRounded(options.Theme.Rounded.DefaultRounded);

    /// <summary>
    /// Gets the Tailwind CSS border-radius class for the specified rounded level.
    /// </summary>
    /// <param name="rounded">The rounded level.</param>
    /// <returns>The corresponding Tailwind CSS class.</returns>
    public string GetRounded(Rounded? rounded) => rounded switch
    {
        Rounded.None => options.Theme.Rounded.None,
        Rounded.Sm => options.Theme.Rounded.Sm,
        Rounded.Md => options.Theme.Rounded.Md,
        Rounded.Lg => options.Theme.Rounded.Lg,
        Rounded.Full => options.Theme.Rounded.Full,
        null => string.Empty,
        _ => GetRounded(options.Theme.Rounded.DefaultRounded)
    };

    /// <summary>
    /// Gets the Tailwind CSS border-radius class for the top corners at the default rounded level defined in the theme.
    /// </summary>
    /// <returns>The corresponding Tailwind CSS class for top border-radius.</returns>
    public string GetRoundedTop() => GetRoundedTop(options.Theme.Rounded.DefaultRounded);

    /// <summary>
    /// Gets the Tailwind CSS border-radius class for the top corners at the specified rounded level.
    /// </summary>
    /// <param name="rounded">The rounded level.</param>
    /// <returns>The corresponding Tailwind CSS class for top border-radius.</returns>
    public string GetRoundedTop(Rounded? rounded) => rounded switch
    {
        Rounded.None => options.Theme.Rounded.RoundedTop.None,
        Rounded.Sm => options.Theme.Rounded.RoundedTop.Sm,
        Rounded.Md => options.Theme.Rounded.RoundedTop.Md,
        Rounded.Lg => options.Theme.Rounded.RoundedTop.Lg,
        Rounded.Full => options.Theme.Rounded.RoundedTop.Full,
        null => string.Empty,
        _ => options.Theme.Rounded.RoundedTop.Lg
    };

    /// <summary>
    /// Gets the Tailwind CSS border-radius class for the bottom corners at the specified rounded level.
    /// </summary>
    /// <param name="rounded">The rounded level.</param>
    /// <returns>The corresponding Tailwind CSS class for bottom border-radius.</returns>
    public string GetRoundedBottom(Rounded? rounded) => rounded switch
    {
        Rounded.None => options.Theme.Rounded.RoundedBottom.None,
        Rounded.Sm => options.Theme.Rounded.RoundedBottom.Sm,
        Rounded.Md => options.Theme.Rounded.RoundedBottom.Md,
        Rounded.Lg => options.Theme.Rounded.RoundedBottom.Lg,
        Rounded.Full => options.Theme.Rounded.RoundedBottom.Full,
        null => string.Empty,
        _ => options.Theme.Rounded.RoundedBottom.Lg
    };

    /// <summary>
    /// Gets the Tailwind CSS border-radius class for the start corners (logical, RTL-aware) at the default rounded level defined in the theme.
    /// </summary>
    /// <returns>The corresponding Tailwind CSS class for start border-radius.</returns>
    public string GetRoundedStart() => GetRoundedStart(options.Theme.Rounded.DefaultRounded);

    /// <summary>
    /// Gets the Tailwind CSS border-radius class for the start corners (logical, RTL-aware) at the specified rounded level.
    /// </summary>
    /// <param name="rounded">The rounded level.</param>
    /// <returns>The corresponding Tailwind CSS class for start border-radius.</returns>
    public string GetRoundedStart(Rounded? rounded) => rounded switch
    {
        Rounded.None => options.Theme.Rounded.RoundedStart.None,
        Rounded.Sm => options.Theme.Rounded.RoundedStart.Sm,
        Rounded.Md => options.Theme.Rounded.RoundedStart.Md,
        Rounded.Lg => options.Theme.Rounded.RoundedStart.Lg,
        Rounded.Full => options.Theme.Rounded.RoundedStart.Full,
        null => string.Empty,
        _ => options.Theme.Rounded.RoundedStart.Lg
    };

    /// <summary>
    /// Gets the Tailwind CSS border-radius class for the end corners (logical, RTL-aware) at the default rounded level defined in the theme.
    /// </summary>
    /// <returns>The corresponding Tailwind CSS class for end border-radius.</returns>
    public string GetRoundedEnd() => GetRoundedEnd(options.Theme.Rounded.DefaultRounded);

    /// <summary>
    /// Gets the Tailwind CSS border-radius class for the end corners (logical, RTL-aware) at the specified rounded level.
    /// </summary>
    /// <param name="rounded">The rounded level.</param>
    /// <returns>The corresponding Tailwind CSS class for end border-radius.</returns>
    public string GetRoundedEnd(Rounded? rounded) => rounded switch
    {
        Rounded.None => options.Theme.Rounded.RoundedEnd.None,
        Rounded.Sm => options.Theme.Rounded.RoundedEnd.Sm,
        Rounded.Md => options.Theme.Rounded.RoundedEnd.Md,
        Rounded.Lg => options.Theme.Rounded.RoundedEnd.Lg,
        Rounded.Full => options.Theme.Rounded.RoundedEnd.Full,
        null => string.Empty,
        _ => options.Theme.Rounded.RoundedEnd.Lg
    };
}
