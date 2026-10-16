// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using TwBlazor.Configuration.Color;

namespace TwBlazor.Configuration.Components;

/// <summary>
/// Theme configuration for rating components (<see cref="TwBlazor.Components.TwRating{T}"/>).
/// Override any property to customize rating styles globally.
/// </summary>
[ExcludeFromCodeCoverage]
public class TwRatingTheme
{
    /// <summary>
    /// Gets or sets the text color palette applied to the filled part of each icon.
    /// </summary>
    /// <remarks>
    /// Each entry carries both its light and <c>dark:</c> mode classes. The filled icons carry the value, so they
    /// need the 3:1 non-text contrast ratio against the surface. <see cref="TwBlazorPalette.Warning"/> is used
    /// when no <see cref="TwBlazor.Components.TwRating{T}.Color"/> is set.
    /// </remarks>
    public required TwBlazorPalette Colors { get; set; }

    /// <summary>
    /// Gets or sets the classes for the element that holds the icons. It is the radio group when the rating can be
    /// changed and an image when it is read-only.
    /// </summary>
    public required string Base { get; set; }

    /// <summary>
    /// Gets or sets the classes added to <see cref="Base"/> to draw a focus ring around every icon while the
    /// "no rating" option has keyboard focus, since that option has no icon of its own.
    /// </summary>
    public required string EmptyFocusRing { get; set; }

    /// <summary>
    /// Gets or sets the classes for each icon's cell, which stacks the visible icon over its radio inputs and
    /// their pointer targets.
    /// </summary>
    public required string Item { get; set; }

    /// <summary>
    /// Gets or sets the classes added to <see cref="Item"/> to draw a focus ring while one of the cell's radio
    /// inputs has keyboard focus. The inputs themselves are visually hidden, so the ring is drawn on the cell.
    /// </summary>
    public required string ItemFocusRing { get; set; }

    /// <summary>
    /// Gets or sets the classes added to <see cref="Item"/> when the rating can be changed and animation is on,
    /// growing the hovered icon slightly.
    /// </summary>
    public required string ItemHover { get; set; }

    /// <summary>
    /// Gets or sets the classes for the visible, decorative part of a cell: the unfilled icon with the clipped
    /// filled icon on top.
    /// </summary>
    public required string Visual { get; set; }

    /// <summary>
    /// Gets or sets the classes for the unfilled icon.
    /// </summary>
    /// <remarks>
    /// A translucent neutral "overlay" tint, so unfilled icons stay visible on light and dark surfaces alike
    /// without competing with the filled ones.
    /// </remarks>
    public required string Empty { get; set; }

    /// <summary>
    /// Gets or sets the classes for the element that clips the filled icon to the fraction of the cell it fills.
    /// Its width is set inline to 0%, 50% or 100% and it is anchored to the inline start, so in a right to left
    /// layout a half-filled icon fills its right half.
    /// </summary>
    public required string Clip { get; set; }

    /// <summary>
    /// Gets or sets the extra classes for the filled icon, on top of the active <see cref="Colors"/> entry.
    /// </summary>
    public required string Filled { get; set; }

    /// <summary>
    /// Gets or sets the marker class that plays the "pop" animation when an icon is filled by a click or key press.
    /// </summary>
    /// <remarks>
    /// The keyframes behind <c>tw-rating-pop</c> live in <c>input.css</c>, since Tailwind has no utility for them,
    /// and are switched off under <c>prefers-reduced-motion</c>.
    /// </remarks>
    public required string Pop { get; set; }

    /// <summary>
    /// Gets or sets the classes for the native radio inputs, which are visually hidden but stay in the
    /// accessibility tree and the tab order.
    /// </summary>
    public required string Input { get; set; }

    /// <summary>
    /// Gets or sets the classes for the pointer target that selects the first half of an icon.
    /// </summary>
    public required string HalfTarget { get; set; }

    /// <summary>
    /// Gets or sets the classes for the pointer target that selects a whole icon.
    /// </summary>
    public required string FullTarget { get; set; }

    /// <summary>
    /// Gets or sets the classes added to <see cref="Base"/> when the rating is disabled.
    /// </summary>
    public required string Disabled { get; set; }

    /// <summary>
    /// Gets or sets the icon size classes for <see cref="TwBlazor.Enums.Size.Sm"/>.
    /// </summary>
    public required string Small { get; set; }

    /// <summary>
    /// Gets or sets the icon size classes for <see cref="TwBlazor.Enums.Size.Md"/>.
    /// </summary>
    public required string Medium { get; set; }

    /// <summary>
    /// Gets or sets the icon size classes for <see cref="TwBlazor.Enums.Size.Lg"/>.
    /// </summary>
    public required string Large { get; set; }
}
