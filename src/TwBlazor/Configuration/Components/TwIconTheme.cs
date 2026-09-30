// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using TwBlazor.Configuration.Color;

namespace TwBlazor.Configuration.Components;

/// <summary>
/// Theme configuration for icon components (<see cref="TwBlazor.Components.TwIcon"/>).
/// Override any property to customize icon styles globally.
/// </summary>
[ExcludeFromCodeCoverage]
public class TwIconTheme
{
    /// <summary>
    /// Gets or sets the color palette applied to icons that set <see cref="TwBlazor.Components.TwIcon.Color"/>.
    /// </summary>
    /// <remarks>
    /// Each entry carries both its light and <c>dark:</c> mode classes. Icons are graphics, so these only need
    /// the 3:1 non-text contrast ratio and can be more vivid than the body text colors.
    /// </remarks>
    public required TwBlazorPalette Colors { get; set; }

    /// <summary>
    /// Gets or sets the hover state-layer classes applied to an icon button (a <see cref="TwBlazor.Components.TwIcon"/>
    /// rendered with an <see cref="TwBlazor.Components.TwIcon.OnClick"/> delegate).
    /// </summary>
    /// <remarks>
    /// Tints the button's already-circular <c>rounded-full</c> shape with <c>currentColor</c> on hover, matching
    /// the Material icon button convention, instead of relying on the button's Color/Variant classes
    /// (which are empty for the common colorless, text-variant icon button).
    /// </remarks>
    public required string HoverBackground { get; set; }

    /// <summary>
    /// Gets or sets the classes that enable the icon button's press "pulse".
    /// </summary>
    /// <remarks>
    /// Applies <c>position: relative</c> (the button's own <c>overflow-hidden</c> already comes from
    /// <see cref="TwButtonTheme.Base"/>) plus the <c>tw-icon-pulse</c> marker class defined in <c>input.css</c>,
    /// which blooms a <c>currentColor</c> circle from the button's center on <c>:active</c> and fades it back out
    /// on release.
    /// </remarks>
    public required string Pulse { get; set; }
}
