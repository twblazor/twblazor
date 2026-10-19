// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;

namespace TwBlazor.Models;

/// <summary>
/// A single placeholder box measured from a <see cref="TwBlazor.Components.TwSkeleton"/>'s
/// <c>ChildContent</c> by the <c>twSkeleton.observe</c> JS interop call, positioned relative to the
/// measuring wrapper it was measured within.
/// </summary>
[ExcludeFromCodeCoverage]
public class SkeletonRect
{
    /// <summary>
    /// Gets or sets the distance in pixels from the top of the measuring wrapper.
    /// </summary>
    public double Top { get; set; }

    /// <summary>
    /// Gets or sets the distance in pixels from the left of the measuring wrapper.
    /// </summary>
    public double Left { get; set; }

    /// <summary>
    /// Gets or sets the measured width in pixels.
    /// </summary>
    public double Width { get; set; }

    /// <summary>
    /// Gets or sets the measured height in pixels.
    /// </summary>
    public double Height { get; set; }

    /// <summary>
    /// Gets or sets the detected shape: <c>"text"</c> for a single wrapped line of text, <c>"circle"</c>
    /// for an element whose computed border-radius makes it round, or <c>"rect"</c> otherwise.
    /// </summary>
    public string Shape { get; set; } = "rect";
}
