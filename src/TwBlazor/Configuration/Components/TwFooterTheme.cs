// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;

namespace TwBlazor.Configuration.Components;

/// <summary>
/// Theme configuration for <see cref="TwBlazor.Components.TwFooter"/>.
/// Override any property to customize footer styles globally.
/// </summary>
[ExcludeFromCodeCoverage]
public class TwFooterTheme
{
    /// <summary>
    /// Gets or sets the classes for the footer itself: its surface, text color and top edge.
    /// </summary>
    public required string Footer { get; set; }

    /// <summary>
    /// Gets or sets the classes for the element that centers the footer's content and gives it its padding.
    /// </summary>
    public required string Container { get; set; }

    /// <summary>
    /// Gets or sets the classes for the brand slot at the top of the footer.
    /// </summary>
    public required string Brand { get; set; }

    /// <summary>
    /// Gets or sets the classes for the navigation that lays the sections out as columns.
    /// </summary>
    public required string Sections { get; set; }

    /// <summary>
    /// Gets or sets the classes for one section: a heading and its list of links.
    /// </summary>
    public required string Section { get; set; }

    /// <summary>
    /// Gets or sets the classes for a section's heading.
    /// </summary>
    public required string SectionTitle { get; set; }

    /// <summary>
    /// Gets or sets the classes for a section's list of links.
    /// </summary>
    public required string LinkList { get; set; }

    /// <summary>
    /// Gets or sets the classes for a link, in a section or along the bottom row. They include its focus ring.
    /// </summary>
    public required string Link { get; set; }

    /// <summary>
    /// Gets or sets the classes for the element wrapping the footer's free-form content.
    /// </summary>
    public required string Content { get; set; }

    /// <summary>
    /// Gets or sets the classes for the bottom row, which holds the copyright line and the bottom links.
    /// </summary>
    public required string Bottom { get; set; }

    /// <summary>
    /// Gets or sets the classes for the copyright line.
    /// </summary>
    public required string Copyright { get; set; }

    /// <summary>
    /// Gets or sets the classes for the list of links along the bottom row.
    /// </summary>
    public required string BottomLinks { get; set; }
}