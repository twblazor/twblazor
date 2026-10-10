// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;

namespace TwBlazor.Configuration.Components;

/// <summary>
/// Theme configuration for chip components (<see cref="TwBlazor.Components.TwChip"/>).
/// Override any property to customize chip styles globally.
/// </summary>
[ExcludeFromCodeCoverage]
public class TwChipTheme
{
    /// <summary>
    /// Gets or sets the base classes applied to every chip.
    /// </summary>
    public required string Base { get; set; }

    /// <summary>
    /// Gets or sets the classes for the close button within a chip.
    /// </summary>
    public required string CloseButton { get; set; }

    /// <summary>
    /// Gets or sets the classes for the button that carries a chip's content when the chip is both
    /// clickable and closable. The chip then holds two separate buttons, side by side.
    /// </summary>
    public string ContentButton { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the size classes for <see cref="Enums.ChipSize.Small"/> chips.
    /// </summary>
    public required string Sm { get; set; }

    /// <summary>
    /// Gets or sets the size classes for <see cref="Enums.ChipSize.Medium"/> chips.
    /// </summary>
    public required string Md { get; set; }

    /// <summary>
    /// Gets or sets the size classes for <see cref="Enums.ChipSize.Large"/> chips.
    /// </summary>
    public required string Lg { get; set; }
}