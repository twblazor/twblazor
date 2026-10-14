// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;

namespace TwBlazor.Configuration.Color;

/// <summary>
/// Neutral (gray-scale, non-semantic) surface tokens shared across components for card/dialog/popover
/// backgrounds, dividing borders, and hover tints. Unlike <see cref="TwBlazorPalette"/> (one distinct
/// color per semantic role - primary, danger, etc.), every property here is a single reusable class
/// string that already covers both light and dark mode, so components that just need "the" neutral
/// surface/border/hover look reference one of these instead of retyping the same literal classes.
/// </summary>
/// <remarks>
/// Add more weights here as new components need them (e.g. a stronger border, a deeper background) -
/// each stays a single string property so the full class names remain visible to Tailwind's content
/// scanner, which can't see classes assembled only at runtime from a compiled DLL.
/// </remarks>
[ExcludeFromCodeCoverage]
public class TwSurfacePalette
{
    /// <summary>
    /// Gets or sets the standard opaque background for cards, dialogs, and popovers
    /// (e.g. <c>bg-white dark:bg-gray-800</c>).
    /// </summary>
    public string Background { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the page background that layouts such as <c>TwSidebar</c>'s main content sit on. It is set
    /// a step away from <see cref="Background"/> so cards, panels and bars stand out from the page behind them
    /// (e.g. <c>bg-gray-50 dark:bg-gray-950</c>).
    /// </summary>
    public string Body { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a dimmer background for muted panels, striped rows, or nested sections
    /// (e.g. <c>bg-gray-50 dark:bg-gray-900</c>).
    /// </summary>
    public string BackgroundSubtle { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the standard outer border for cards, dialogs, and popover surfaces
    /// (e.g. <c>border-gray-200 dark:border-gray-700</c>).
    /// </summary>
    public string Border { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a lighter border for internal dividers within an already-bordered surface
    /// (e.g. <c>border-gray-100 dark:border-gray-700</c>).
    /// </summary>
    public string BorderSubtle { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the neutral hover background tint for list items, menu entries, and similar
    /// interactive rows (e.g. <c>hover:bg-gray-100 dark:hover:bg-gray-700</c>).
    /// </summary>
    public string Hover { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a background for emphasized (filled) inputs that must read clearly against the
    /// page even with no backdrop behind them. Unlike <see cref="BackgroundSubtle"/>, this is
    /// deliberately lighter than <see cref="Background"/> in dark mode, since a same-or-darker fill
    /// has nothing to visually pop against without a dimming backdrop (e.g. <c>bg-gray-50 dark:bg-gray-700</c>).
    /// </summary>
    public string Elevated { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the background for floating popover/dialog panels (date/time/color picker
    /// bodies). Deliberately a different tone from <see cref="Elevated"/> - not just from
    /// <see cref="Background"/> - because filled inputs (which use <see cref="Elevated"/>) are
    /// routinely rendered inside these panels; if the panel and the filled input shared a tone, the
    /// input would visually disappear into the panel behind it (e.g. <c>bg-white dark:bg-gray-700</c>).
    /// </summary>
    public string Overlay { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a more visible border than <see cref="Border"/>, for the same no-backdrop
    /// surfaces as <see cref="Elevated"/>/<see cref="Overlay"/> plus input outlines, where the
    /// default border is too subtle to read against the page (e.g. <c>border-gray-400 dark:border-gray-500</c>).
    /// </summary>
    public string BorderStrong { get; set; } = string.Empty;
}
