// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;

namespace TwBlazor.Configuration;

/// <summary>
/// Shared design tokens: the full Tailwind classes that many component themes reuse, such as the focus ring, the
/// neutral fills and inks, and the disabled look. Each is declared once in the theme file and referenced everywhere
/// else, so changing one changes every component that uses it.
/// </summary>
/// <remarks>
/// Every property holds complete class names. Tailwind only compiles classes it can find written out in full, so
/// never build a class from parts at runtime.
/// </remarks>
[ExcludeFromCodeCoverage]
public class TwBlazorTokens
{
    /// <summary>
    /// Gets or sets the height of every single-line input, and of controls laid out beside inputs such as
    /// pagination, so they line up whichever component renders them.
    /// </summary>
    public string InputHeight { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the dense form of <see cref="InputHeight"/>.
    /// </summary>
    public string InputDenseHeight { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the label text used above and beside inputs.
    /// </summary>
    public string InputLabelText { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the focus ring of every focusable surface: a 2px ring in the primary color.
    /// </summary>
    public string FocusRing { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the inset form of <see cref="FocusRing"/>, for rows that sit flush against their container's
    /// edge and would otherwise have the ring clipped.
    /// </summary>
    public string FocusRingInset { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a focus ring in the current text color, for controls that sit on a colored surface such as an
    /// alert's dismiss button.
    /// </summary>
    public string FocusRingCurrent { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the gap between a focus ring and its control, matched to the surface behind it in each mode.
    /// </summary>
    public string FocusRingOffset { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the transparent background.
    /// </summary>
    public string Transparent { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the neutral fill for a recessed track, a selected row or a pressed state.
    /// </summary>
    public string Track { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a recessed track that stays visible on a white card. Progress and slider tracks carry the
    /// value, so they need more contrast than <see cref="Track"/>.
    /// </summary>
    public string TrackStrong { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the surface of a selected segment raised above a <see cref="Track"/>, such as the active tab of a
    /// segmented control.
    /// </summary>
    public string RaisedSurface { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a ring in the page surface color, which separates touching marks such as chart bars and dots.
    /// </summary>
    public string SurfaceRing { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the neutral text color on hover.
    /// </summary>
    public string HoverText { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the neutral fill on hover for buttons and icon buttons.
    /// </summary>
    public string HoverFill { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the lighter neutral fill on hover for rows in lists, menus and tables.
    /// </summary>
    public string RowHover { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the neutral border color on hover for checkboxes and radio buttons.
    /// </summary>
    public string HoverBorder { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the stronger neutral border color on hover for outlined links and steps.
    /// </summary>
    public string HoverBorderStrong { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a hairline ring that outlines swatches and raised tabs.
    /// </summary>
    public string HairlineRing { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the faded text of a disabled control.
    /// </summary>
    public string DisabledInk { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets <see cref="DisabledInk"/> applied through <c>aria-disabled</c>, for controls that stay focusable
    /// while unavailable.
    /// </summary>
    public string AriaDisabledInk { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the disabled look of a small native button, including its colors in forced colors mode.
    /// </summary>
    public string DisabledControl { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the dimmed backdrop behind dialogs, drawers and the mobile sidebar.
    /// </summary>
    public string Backdrop { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the classes that remove a list's bullets, margin and padding.
    /// </summary>
    public string ListReset { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the class that turns transitions off for visitors who ask their system for reduced motion.
    /// </summary>
    public string ReduceMotion { get; set; } = string.Empty;
}
