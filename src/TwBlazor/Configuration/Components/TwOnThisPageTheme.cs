// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;

namespace TwBlazor.Configuration.Components;

/// <summary>
/// Theme configuration for the "On this page" navigation component (<see cref="TwBlazor.Components.TwOnThisPage"/>).
/// Override any property to customize its styles globally.
/// </summary>
[ExcludeFromCodeCoverage]
public class TwOnThisPageTheme
{
    /// <summary>
    /// Gets or sets the classes for the heading shown above the list of links.
    /// </summary>
    public required string Title { get; set; }

    /// <summary>
    /// Gets or sets the classes for the &lt;ul&gt; list, including the rail drawn down its start edge.
    /// </summary>
    public required string List { get; set; }

    /// <summary>
    /// Gets or sets the classes for each link: its text color, hover state, keyboard focus ring and the
    /// transparent marker that sits over the list's rail.
    /// </summary>
    public required string Link { get; set; }

    /// <summary>
    /// Gets or sets the classes that restyle the link to the section currently being read. They are applied
    /// to every link and must be scoped to <c>aria-current="location"</c>, which is set in the browser as
    /// the page scrolls.
    /// </summary>
    public required string LinkCurrent { get; set; }
}
