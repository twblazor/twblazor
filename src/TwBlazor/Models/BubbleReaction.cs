// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;

namespace TwBlazor.Models;

/// <summary>
/// A reaction shown under a chat bubble (<see cref="Components.TwBubble"/>), such as a thumbs up and how many people chose it.
/// </summary>
[ExcludeFromCodeCoverage]
public class BubbleReaction
{
    /// <summary>
    /// Gets or sets the emoji or short text shown on the reaction, for example <c>"👍"</c>.
    /// </summary>
    public required string Emoji { get; set; }

    /// <summary>
    /// Gets or sets how many people chose the reaction.
    /// </summary>
    public int Count { get; set; }

    /// <summary>
    /// Gets or sets whether the current user chose the reaction. A clickable reaction reports it as pressed.
    /// </summary>
    public bool Reacted { get; set; }

    /// <summary>
    /// Gets or sets the name screen readers announce for the reaction, for example <c>"Thumbs up"</c>.
    /// </summary>
    /// <remarks>
    /// If not set, <see cref="Emoji"/> is announced, which screen readers read out by its own name.
    /// </remarks>
    public string? Label { get; set; }
}
