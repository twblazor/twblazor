// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using TwBlazor.Configuration.Color;

namespace TwBlazor.Configuration.Components;

/// <summary>
/// Theme configuration for the badge component (<see cref="TwBlazor.Components.TwBadge"/>).
/// Override any property to customize badge styles globally.
/// </summary>
[ExcludeFromCodeCoverage]
public class TwBadgeTheme
{
    /// <summary>
    /// Gets or sets the background and text classes for each <see cref="Enums.Color"/>, shown behind the
    /// count, label or dot.
    /// </summary>
    public required TwBlazorPalette Colors { get; set; }

    /// <summary>
    /// Gets or sets the base classes applied to every badge's wrapper: its positioning and layout.
    /// </summary>
    public required string Wrapper { get; set; }

    /// <summary>
    /// Gets or sets the base classes applied to the count or label element.
    /// </summary>
    public required string Badge { get; set; }

    /// <summary>
    /// Gets or sets the base classes applied to the dot element.
    /// </summary>
    public required string Dot { get; set; }

    /// <summary>
    /// Gets or sets the positioning classes for the top-start corner.
    /// </summary>
    public required string TopStart { get; set; }

    /// <summary>
    /// Gets or sets the positioning classes for the top-end corner.
    /// </summary>
    public required string TopEnd { get; set; }

    /// <summary>
    /// Gets or sets the positioning classes for the bottom-start corner.
    /// </summary>
    public required string BottomStart { get; set; }

    /// <summary>
    /// Gets or sets the positioning classes for the bottom-end corner.
    /// </summary>
    public required string BottomEnd { get; set; }
}
