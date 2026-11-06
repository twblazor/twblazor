// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using TwBlazor.Configuration.Color;

namespace TwBlazor.Configuration.Components;

/// <summary>
/// Theme configuration for the empty state component (<see cref="TwBlazor.Components.TwEmpty"/>).
/// Override any property to customize empty state styles globally.
/// </summary>
[ExcludeFromCodeCoverage]
public class TwEmptyTheme
{
    /// <summary>
    /// Gets or sets the base classes applied to every empty state's root: its layout and alignment.
    /// </summary>
    public required string Base { get; set; }

    /// <summary>
    /// Gets or sets the classes added when the empty state is outlined: a dashed border around it.
    /// </summary>
    public required string Outlined { get; set; }

    /// <summary>
    /// Gets or sets the classes added when the empty state is filled: a subtle background behind it.
    /// </summary>
    public required string Filled { get; set; }

    /// <summary>
    /// Gets or sets the padding and spacing of a small empty state.
    /// </summary>
    public required string Small { get; set; }

    /// <summary>
    /// Gets or sets the padding and spacing of a medium empty state, the default size.
    /// </summary>
    public required string Medium { get; set; }

    /// <summary>
    /// Gets or sets the padding and spacing of a large empty state.
    /// </summary>
    public required string Large { get; set; }

    /// <summary>
    /// Gets or sets the classes applied to the element that holds the icon, avatar or custom media.
    /// </summary>
    public required string Media { get; set; }

    /// <summary>
    /// Gets or sets the base classes of the tile behind the icon when the media variant is a tile.
    /// </summary>
    public required string Tile { get; set; }

    /// <summary>
    /// Gets or sets the tile's background and icon colour when no <see cref="Enums.Color"/> is set.
    /// </summary>
    public required string TileNeutral { get; set; }

    /// <summary>
    /// Gets or sets the tile's background and icon colour for each <see cref="Enums.Color"/>.
    /// </summary>
    public required TwBlazorPalette TileColors { get; set; }

    /// <summary>
    /// Gets or sets the size of the tile in a small empty state.
    /// </summary>
    public required string TileSmall { get; set; }

    /// <summary>
    /// Gets or sets the size of the tile in a medium empty state.
    /// </summary>
    public required string TileMedium { get; set; }

    /// <summary>
    /// Gets or sets the size of the tile in a large empty state.
    /// </summary>
    public required string TileLarge { get; set; }

    /// <summary>
    /// Gets or sets the icon colour of a plain icon (shown without a tile) when no <see cref="Enums.Color"/> is set.
    /// </summary>
    public required string PlainIcon { get; set; }

    /// <summary>
    /// Gets or sets the size of a plain icon in a small empty state.
    /// </summary>
    public required string PlainIconSmall { get; set; }

    /// <summary>
    /// Gets or sets the size of a plain icon in a medium empty state.
    /// </summary>
    public required string PlainIconMedium { get; set; }

    /// <summary>
    /// Gets or sets the size of a plain icon in a large empty state.
    /// </summary>
    public required string PlainIconLarge { get; set; }

    /// <summary>
    /// Gets or sets the classes applied to the element that groups the title and description.
    /// </summary>
    public required string Header { get; set; }

    /// <summary>
    /// Gets or sets the base classes applied to the title.
    /// </summary>
    public required string Title { get; set; }

    /// <summary>
    /// Gets or sets the text size of the title in a small empty state.
    /// </summary>
    public required string TitleSmall { get; set; }

    /// <summary>
    /// Gets or sets the text size of the title in a medium empty state.
    /// </summary>
    public required string TitleMedium { get; set; }

    /// <summary>
    /// Gets or sets the text size of the title in a large empty state.
    /// </summary>
    public required string TitleLarge { get; set; }

    /// <summary>
    /// Gets or sets the base classes applied to the description.
    /// </summary>
    public required string Description { get; set; }

    /// <summary>
    /// Gets or sets the text size of the description in a small empty state.
    /// </summary>
    public required string DescriptionSmall { get; set; }

    /// <summary>
    /// Gets or sets the text size of the description in a medium empty state.
    /// </summary>
    public required string DescriptionMedium { get; set; }

    /// <summary>
    /// Gets or sets the text size of the description in a large empty state.
    /// </summary>
    public required string DescriptionLarge { get; set; }

    /// <summary>
    /// Gets or sets the classes applied to the element that holds the child content, below the description.
    /// </summary>
    public required string Content { get; set; }

    /// <summary>
    /// Gets or sets the classes applied to the row of actions.
    /// </summary>
    public required string Actions { get; set; }
}
