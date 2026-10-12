// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using TwBlazor.Configuration.Color;

namespace TwBlazor.Configuration.Components;

/// <summary>
/// Theme configuration for the progress bar component (<see cref="TwBlazor.Components.TwProgress{T}"/>).
/// Override any property to customize progress bar styles globally.
/// </summary>
/// <remarks>
/// Styles a native &lt;progress&gt; element via Tailwind's arbitrary variant support for the
/// "::-webkit-progress-bar"/"::-webkit-progress-value" (Chromium/Safari) and "::-moz-progress-bar" (Firefox)
/// pseudo-elements, rather than replacing it with custom div markup, so the element keeps its built-in
/// "progressbar" role and automatic aria-valuenow/min/max semantics.
/// </remarks>
[ExcludeFromCodeCoverage]
public class TwProgressTheme
{
    /// <summary>
    /// Gets or sets the color palette applied to progress bars.
    /// </summary>
    public required TwBlazorPalette Colors { get; set; }

    /// <summary>
    /// Gets or sets the base classes applied to the &lt;progress&gt; element, including the track background,
    /// rounding, and the cross-browser pseudo-element resets.
    /// </summary>
    public required string Base { get; set; }

    /// <summary>
    /// Gets or sets the height classes for <see cref="Enums.ProgressSize.Small"/>.
    /// </summary>
    public required string Small { get; set; }

    /// <summary>
    /// Gets or sets the height classes for <see cref="Enums.ProgressSize.Medium"/>.
    /// </summary>
    public required string Medium { get; set; }

    /// <summary>
    /// Gets or sets the height classes for <see cref="Enums.ProgressSize.Large"/>.
    /// </summary>
    public required string Large { get; set; }

    /// <summary>
    /// Gets or sets the classes applied to the element that wraps the &lt;progress&gt; element, which positions
    /// the <see cref="Animated"/> overlay.
    /// </summary>
    public required string Wrapper { get; set; }

    /// <summary>
    /// Gets or sets the classes applied to the decorative overlay rendered over the filled portion of the bar
    /// when <see cref="TwBlazor.Components.TwProgress{T}.Animated"/> is <c>true</c>.
    /// </summary>
    /// <remarks>
    /// The default theme reuses the <c>tw-wave</c> shimmer defined in <c>input.css</c>, which sweeps a
    /// gradient across the overlay via "::after" and so needs it to be positioned and "overflow-hidden". The
    /// overlay is sized to the current percentage so the shimmer stays inside the filled portion, and its
    /// colour is set through the <c>--tw-wave-color</c> custom property.
    /// </remarks>
    public required string Animated { get; set; }
}
