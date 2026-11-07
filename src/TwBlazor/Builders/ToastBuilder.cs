// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using TwBlazor.Configuration;
using TwBlazor.Configuration.Components;
using TwBlazor.Enums;
using TwBlazor.Utilities;

namespace TwBlazor.Builders;

/// <summary>
/// Provides utility methods for building toast CSS classes.
/// </summary>
public class ToastBuilder(TwBlazorOptions options, RoundedBuilder roundedBuilder)
{
    private TwToastTheme theme => options.Theme.Components.Require<TwToastTheme>();

    /// <summary>
    /// Gets the base classes for a toast.
    /// </summary>
    /// <param name="componentRounded">Component-level Rounded parameter value.</param>
    public string GetToastClasses(Rounded? componentRounded = null)
    {
        var effectiveRounded = RoundedBuilder.ForSurface(componentRounded ?? theme.ToastRounded ?? options.Theme.Rounded.DefaultRounded);

        return new ClassBuilder(theme.Toast)
            .AddClass(roundedBuilder.GetRounded(effectiveRounded))
            .AddClass(theme.ToastWidth ?? string.Empty, !string.IsNullOrWhiteSpace(theme.ToastWidth))
            .Build();
    }
}
