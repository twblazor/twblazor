// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using TwBlazor.Configuration.Color;

namespace TwBlazor.Configuration.Components;

/// <summary>
/// Theme configuration for the avatar component (<see cref="TwBlazor.Components.TwAvatar"/>).
/// Override any property to customize avatar styles globally.
/// </summary>
[ExcludeFromCodeCoverage]
public class TwAvatarTheme
{
    /// <summary>
    /// Gets or sets the background and text classes for each <see cref="Enums.Color"/>, shown behind initials
    /// and icons.
    /// </summary>
    public required TwBlazorPalette Colors { get; set; }

    /// <summary>
    /// Gets or sets the background and text classes used when no color is set.
    /// </summary>
    public required string Neutral { get; set; }

    /// <summary>
    /// Gets or sets the base classes applied to every avatar: its layout, clipping and text weight.
    /// </summary>
    public required string Base { get; set; }

    /// <summary>
    /// Gets or sets the size and text-size classes for <see cref="Enums.AvatarSize.Small"/>.
    /// </summary>
    public required string Small { get; set; }

    /// <summary>
    /// Gets or sets the size and text-size classes for <see cref="Enums.AvatarSize.Medium"/>.
    /// </summary>
    public required string Medium { get; set; }

    /// <summary>
    /// Gets or sets the size and text-size classes for <see cref="Enums.AvatarSize.Large"/>.
    /// </summary>
    public required string Large { get; set; }

    /// <summary>
    /// Gets or sets the size and text-size classes for <see cref="Enums.AvatarSize.ExtraLarge"/>.
    /// </summary>
    public required string ExtraLarge { get; set; }

    /// <summary>
    /// Gets or sets the classes for the image, which fills the avatar and is cropped to its shape.
    /// </summary>
    public required string Image { get; set; }

    /// <summary>
    /// Gets or sets the classes for the fallback icon. Its size follows the avatar's text size.
    /// </summary>
    public required string Icon { get; set; }
}