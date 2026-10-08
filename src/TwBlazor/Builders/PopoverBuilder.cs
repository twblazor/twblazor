// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using TwBlazor.Configuration;
using TwBlazor.Configuration.Components;
using TwBlazor.Enums;
using TwBlazor.Utilities;

namespace TwBlazor.Builders;

/// <summary>
/// Builds the shared "box" classes (background, border, rounded corners, shadow) for every popover
/// panel (date/time/color picker panels) from <see cref="TwOverlayTheme"/>. Mirrors
/// <see cref="DialogBuilder.GetSurfaceClasses"/>'s instance-override/theme-default/global-default
/// resolution, so popovers and the modal dialog behave the same way instead of the popover family
/// having no such override tier at all. Also builds tooltip classes, since a tooltip is a small popover.
/// </summary>
public class PopoverBuilder(TwBlazorOptions options, RoundedBuilder roundedBuilder, ShadowBuilder shadowBuilder)
{
    private TwPopoverTheme theme => options.Theme.Components.Require<TwOverlayTheme>().Popover;

    private TwTooltipTheme tooltipTheme => options.Theme.Components.Require<TwOverlayTheme>().Tooltip;

    /// <summary>
    /// Gets the classes for a popover panel's surface (background, border, rounded corners, shadow).
    /// </summary>
    /// <param name="rounded">Instance-level rounded override.</param>
    /// <param name="shadow">Instance-level shadow override.</param>
    /// <param name="customClass">Additional custom classes to append.</param>
    public string GetSurfaceClasses(Rounded? rounded, Shadow? shadow, string? customClass = null)
    {
        var effectiveRounded = rounded ?? theme.Rounded ?? options.Theme.Rounded.DefaultRounded;
        var effectiveShadow = shadow ?? theme.Shadow ?? options.Theme.Shadows.DefaultShadow;

        return new ClassBuilder(theme.Background)
            .AddClass(theme.Border)
            .AddClass(roundedBuilder.GetRounded(effectiveRounded))
            .AddClass(shadowBuilder.GetShadow(effectiveShadow))
            .AddClass(customClass ?? string.Empty, !string.IsNullOrWhiteSpace(customClass))
            .Build();
    }

    /// <summary>
    /// Gets the classes for the element wrapping a control and its tooltip bubble.
    /// </summary>
    public string GetTooltipWrapperClasses() => tooltipTheme.Wrapper;

    /// <summary>
    /// Gets the classes for a tooltip bubble: the shared tooltip layout/visibility classes on top of the
    /// popover surface from <see cref="GetSurfaceClasses"/>, so tooltips match popover panels.
    /// </summary>
    /// <param name="dismissed">Whether the user has dismissed the tooltip (Escape), which hides it
    /// regardless of hover or focus until they leave the control.</param>
    /// <param name="placement">Which side of the control the bubble appears on.</param>
    /// <param name="rounded">Instance-level rounded override.</param>
    /// <param name="shadow">Instance-level shadow override.</param>
    /// <param name="customClass">Additional custom classes to append to the bubble.</param>
    public string GetTooltipClasses(
        bool dismissed,
        TooltipPlacement placement = TooltipPlacement.Top,
        Rounded? rounded = null,
        Shadow? shadow = null,
        string? customClass = null) =>
        new ClassBuilder(tooltipTheme.Bubble)
            .AddClass(GetPlacementClasses(placement))
            .AddClass(GetSurfaceClasses(rounded, shadow, customClass))
            .AddClass(options.Theme.Display.Hidden, dismissed)
            .AddClass(tooltipTheme.Dismissed, dismissed)
            .Build();

    /// <summary>
    /// Gets the classes for a tooltip's arrow: the shared arrow shape, the popover fill and border so it
    /// continues the bubble's outline, and the offset for the bubble's <paramref name="placement"/>.
    /// </summary>
    /// <param name="placement">Which side of the control the bubble appears on.</param>
    public string GetTooltipArrowClasses(TooltipPlacement placement) =>
        new ClassBuilder(tooltipTheme.Arrow)
            .AddClass(theme.Background)
            .AddClass(theme.Border)
            .AddClass(placement switch
            {
                TooltipPlacement.Bottom => tooltipTheme.ArrowBottom,
                TooltipPlacement.Left => tooltipTheme.ArrowLeft,
                TooltipPlacement.Right => tooltipTheme.ArrowRight,
                _ => tooltipTheme.ArrowTop
            })
            .Build();

    private string GetPlacementClasses(TooltipPlacement placement) => placement switch
    {
        TooltipPlacement.Bottom => tooltipTheme.Bottom,
        TooltipPlacement.Left => tooltipTheme.Left,
        TooltipPlacement.Right => tooltipTheme.Right,
        _ => tooltipTheme.Top
    };
}
