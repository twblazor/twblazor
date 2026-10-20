// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using TwBlazor.Configuration.Color;

namespace TwBlazor.Configuration.Components;

/// <summary>
/// Theme configuration for the chat bubble (<see cref="TwBlazor.Components.TwBubble"/>).
/// Override any property to customize bubble styles globally.
/// </summary>
[ExcludeFromCodeCoverage]
public class TwBubbleTheme
{
    /// <summary>
    /// Gets or sets the classes for the bubble itself: its padding, text size and wrapping.
    /// </summary>
    public required string Surface { get; set; }

    /// <summary>
    /// Gets or sets the fill and text classes of a <see cref="Enums.BubbleVariant.Soft"/> bubble for each <see cref="Enums.Color"/>.
    /// </summary>
    public required TwBlazorPalette Soft { get; set; }

    /// <summary>
    /// Gets or sets the fill and text classes of a <see cref="Enums.BubbleVariant.Filled"/> bubble for each <see cref="Enums.Color"/>.
    /// </summary>
    public required TwBlazorPalette Filled { get; set; }

    /// <summary>
    /// Gets or sets the border and text classes of a <see cref="Enums.BubbleVariant.Outlined"/> bubble for each <see cref="Enums.Color"/>.
    /// </summary>
    public required TwBlazorPalette Outlined { get; set; }

    /// <summary>
    /// Gets or sets the fill and text classes of a <see cref="Enums.BubbleVariant.Soft"/> bubble with no color.
    /// </summary>
    public required string Neutral { get; set; }

    /// <summary>
    /// Gets or sets the border and text classes of a <see cref="Enums.BubbleVariant.Outlined"/> bubble with no color.
    /// </summary>
    public required string NeutralOutlined { get; set; }

    /// <summary>
    /// Gets or sets the classes that draw the tail at the bottom corner of the bubble on the side facing the sender's
    /// avatar, angled down and outwards. It takes the bubble's fill and border, so it matches every variant.
    /// </summary>
    public required string Tip { get; set; }

    /// <summary>
    /// Gets or sets the classes that square off the bottom start corner and place the <see cref="Tip"/> there, for
    /// a bubble at the start edge.
    /// </summary>
    public required string StartTail { get; set; }

    /// <summary>
    /// Gets or sets the classes that square off the bottom end corner and place the <see cref="Tip"/> there, for a
    /// bubble at the end edge.
    /// </summary>
    public required string EndTail { get; set; }

    /// <summary>
    /// Gets or sets the classes for the row of reactions under the bubble.
    /// </summary>
    public required string Reactions { get; set; }

    /// <summary>
    /// Gets or sets the classes for the emoji on a reaction.
    /// </summary>
    public required string ReactionEmoji { get; set; }

    /// <summary>
    /// Gets or sets the classes for the count on a reaction.
    /// </summary>
    public required string ReactionCount { get; set; }
}
