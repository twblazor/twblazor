// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;

namespace TwBlazor.Configuration.Components;

/// <summary>
/// Theme configuration for range slider components.
/// </summary>
/// <remarks>
/// A range slider is drawn with the <see cref="TwSliderTheme"/>: its colors, wrapper, track, thumbs and value
/// tooltips all come from there, so both sliders always match. This theme only holds what a second handle adds.
/// </remarks>
[ExcludeFromCodeCoverage]
public class TwRangeSliderTheme
{
    /// <summary>
    /// Gets or sets the classes for the layer that holds one handle: its native input, thumb and value tooltip.
    /// Each handle gets its own layer so that hover and focus styles on one handle do not light up the other.
    /// </summary>
    public required string Handle { get; set; }

    /// <summary>
    /// Gets or sets the classes added to <see cref="TwSliderTheme.Base"/> for each native input. The two inputs
    /// overlap, so each one lets pointer events through everywhere except its own thumb. The thumb is sized to
    /// match the visible thumb, and the input is widened by half a thumb on each side so the native thumb lines
    /// up with the visible one at both ends of the track.
    /// </summary>
    public required string Base { get; set; }

    /// <summary>
    /// Gets or sets the classes added to <see cref="TwSliderTheme.Fill"/> so the fill can start at the lower
    /// handle instead of the start of the track.
    /// </summary>
    public required string Fill { get; set; }
}
