// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using TwBlazor.Configuration.Color;
using TwBlazor.Enums;

namespace TwBlazor.Configuration.Components;

/// <summary>
/// Theme configuration shared by toggle buttons (<see cref="TwBlazor.Components.TwToggle"/>) and toggle groups
/// (<see cref="TwBlazor.Components.TwToggleGroup{T}"/>), whose items are toggles.
/// Override any property to customize toggle styles globally.
/// </summary>
[ExcludeFromCodeCoverage]
public class TwToggleTheme
{
    /// <summary>
    /// Gets or sets the default border radius for toggles and toggle groups.
    /// </summary>
    /// <remarks>
    /// If not set, falls back to the global <see cref="TwBlazorRounded.DefaultRounded"/>. The items of a toggle
    /// group use one step tighter than the group, so their corners follow the group's.
    /// Default is <c>null</c> (uses global default).
    /// </remarks>
    public Rounded? ToggleRounded { get; set; }

    /// <summary>
    /// Gets or sets the variant used when a toggle or toggle group does not set its own.
    /// </summary>
    /// <remarks>Default is <see cref="ToggleVariant.Text"/>.</remarks>
    public ToggleVariant? DefaultVariant { get; set; } = ToggleVariant.Text;

    /// <summary>
    /// Gets or sets the background and text classes of a pressed toggle for each <see cref="Enums.Color"/>.
    /// Used when the toggle or group sets a <c>Color</c>; otherwise <see cref="Pressed"/> or
    /// <see cref="GroupPressed"/> is used.
    /// </summary>
    public required TwBlazorPalette Colors { get; set; }

    /// <summary>
    /// Gets or sets the base classes applied to every toggle: layout, typography and transition.
    /// </summary>
    public required string Base { get; set; }

    /// <summary>
    /// Gets or sets the height, minimum width, padding and text size of a small toggle.
    /// </summary>
    public required string Sm { get; set; }

    /// <summary>
    /// Gets or sets the height, minimum width, padding and text size of a medium toggle (the default).
    /// </summary>
    public required string Md { get; set; }

    /// <summary>
    /// Gets or sets the height, minimum width, padding and text size of a large toggle.
    /// </summary>
    public required string Lg { get; set; }

    /// <summary>
    /// Gets or sets the size classes of the icon in a small toggle.
    /// </summary>
    public required string IconSm { get; set; }

    /// <summary>
    /// Gets or sets the size classes of the icon in a medium toggle.
    /// </summary>
    public required string IconMd { get; set; }

    /// <summary>
    /// Gets or sets the size classes of the icon in a large toggle.
    /// </summary>
    public required string IconLg { get; set; }

    /// <summary>
    /// Gets or sets the text classes of a toggle that is not pressed.
    /// </summary>
    public required string Unpressed { get; set; }

    /// <summary>
    /// Gets or sets the hover classes of a toggle that is not pressed. Left off while the toggle is disabled.
    /// </summary>
    public required string Hover { get; set; }

    /// <summary>
    /// Gets or sets the background and text classes of a pressed toggle with no <c>Color</c>.
    /// </summary>
    public required string Pressed { get; set; }

    /// <summary>
    /// Gets or sets the background and text classes of a pressed item with no <c>Color</c> in a
    /// <see cref="ToggleVariant.Text"/> toggle group, where the items sit on a filled bar.
    /// </summary>
    public required string GroupPressed { get; set; }

    /// <summary>
    /// Gets or sets the classes added to every pressed toggle so the pressed state stays visible in forced
    /// colors (Windows high contrast) mode, where backgrounds are replaced by system colors.
    /// </summary>
    public required string PressedForcedColors { get; set; }

    /// <summary>
    /// Gets or sets the border classes of a standalone <see cref="ToggleVariant.Outlined"/> toggle.
    /// </summary>
    public required string Outlined { get; set; }

    /// <summary>
    /// Gets or sets the classes applied to a disabled toggle.
    /// </summary>
    public required string Disabled { get; set; }

    /// <summary>
    /// Gets or sets the base classes of the toggle group container: layout, gap and padding.
    /// </summary>
    public required string GroupBase { get; set; }

    /// <summary>
    /// Gets or sets the classes of a horizontal toggle group (the default).
    /// </summary>
    public required string GroupHorizontal { get; set; }

    /// <summary>
    /// Gets or sets the classes of a vertical toggle group.
    /// </summary>
    public required string GroupVertical { get; set; }

    /// <summary>
    /// Gets or sets the classes of a <see cref="ToggleVariant.Text"/> toggle group: the filled bar its items sit on.
    /// </summary>
    public required string GroupText { get; set; }

    /// <summary>
    /// Gets or sets the classes of a <see cref="ToggleVariant.Outlined"/> toggle group: the border around its items.
    /// </summary>
    public required string GroupOutlined { get; set; }
}
