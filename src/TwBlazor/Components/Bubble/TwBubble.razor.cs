// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using Microsoft.AspNetCore.Components;
using TwBlazor.Builders;
using TwBlazor.Configuration.Components;
using TwBlazor.Enums;
using TwBlazor.Models;
using TwBlazor.Utilities;

namespace TwBlazor.Components;

/// <summary>
/// Represents a chat message bubble, with an optional row of reactions under it.
/// </summary>
/// <remarks>
/// <para>
/// Inside a <see cref="TwMessage"/> the bubble follows the message's alignment, and a tip near its bottom points
/// at the sender's avatar. Set <see cref="Align"/> to override the alignment.
/// </para>
/// <para>
/// Accessibility: the reactions are a group named "Reactions". Each reaction is announced by its
/// <see cref="BubbleReaction.Label"/> (or emoji) and count. When <see cref="OnReactionClick"/> is set, each one is
/// a toggle button that reports whether the current user chose it; otherwise the reactions are plain text.
/// </para>
/// </remarks>
public partial class TwBubble : TwBlazorComponentBase
{
    private TwBubbleTheme theme => options.Theme.Components.Require<TwBubbleTheme>();

    [CascadingParameter(Name = TwMessage.alignCascadeName)] private MessageAlign? cascadedAlign { get; set; }

    /// <summary>
    /// Gets or sets the content of the bubble.
    /// </summary>
    [Parameter] public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Gets or sets the bubble's color.
    /// </summary>
    /// <remarks>
    /// If not set, a <see cref="BubbleVariant.Soft"/> or <see cref="BubbleVariant.Outlined"/> bubble is neutral
    /// gray and a <see cref="BubbleVariant.Filled"/> bubble uses the primary color.
    /// </remarks>
    [Parameter] public Color? Color { get; set; }

    /// <summary>
    /// Gets or sets how the bubble is filled.
    /// </summary>
    [Parameter] public BubbleVariant Variant { get; set; } = BubbleVariant.Soft;

    /// <summary>
    /// Gets or sets which side of the conversation the bubble sits on.
    /// </summary>
    /// <remarks>
    /// If not set, the enclosing <see cref="TwMessage"/>'s alignment is used, or <see cref="MessageAlign.Start"/>
    /// outside one.
    /// </remarks>
    [Parameter] public MessageAlign? Align { get; set; }

    /// <summary>
    /// Gets or sets whether a tip near the bottom of the bubble points at the sender.
    /// </summary>
    [Parameter] public bool Tail { get; set; } = true;

    /// <summary>
    /// Gets or sets the reactions shown under the bubble.
    /// </summary>
    [Parameter] public IEnumerable<BubbleReaction>? Reactions { get; set; }

    /// <summary>
    /// Gets or sets the callback invoked when a reaction is clicked. When set, every reaction is a toggle button.
    /// </summary>
    /// <remarks>
    /// The bubble does not change the reaction itself: update its <see cref="BubbleReaction.Reacted"/> and
    /// <see cref="BubbleReaction.Count"/> in the callback.
    /// </remarks>
    [Parameter] public EventCallback<BubbleReaction> OnReactionClick { get; set; }

    private MessageAlign effectiveAlign => Align ?? cascadedAlign ?? MessageAlign.Start;

    private bool hasAccessibleName => !string.IsNullOrWhiteSpace(AriaLabel) || !string.IsNullOrWhiteSpace(AriaLabelledBy);

    private bool hasReactions => Reactions?.Any() == true;

    private Color reactionColor => Color ?? Enums.Color.Primary;

    private string rootClasses => new ClassBuilder(options.Theme.Display.Flex)
        .AddClass(options.Theme.Flexbox.Col)
        .AddClass(options.Theme.Spacing.Gap.Get(2))
        .AddClass(options.Theme.Sizing.MinWidthNone)
        .AddClass(effectiveAlign == MessageAlign.End ? options.Theme.Flexbox.Align.End : options.Theme.Flexbox.Align.Start)
        .AddClass(Class)
        .Build();

    private string surfaceClasses => new ClassBuilder(theme.Surface)
        .AddClass(variantClasses)
        .AddClass(options.Theme.Border.Width.Thin, Variant == BubbleVariant.Outlined)
        .AddClass(roundedBuilder.GetRounded(effectiveRounded))
        .AddClass(theme.Tip, Tail)
        .AddClass(effectiveAlign == MessageAlign.End ? theme.EndTail : theme.StartTail, Tail)
        .Build();

    private string variantClasses => Variant switch
    {
        BubbleVariant.Filled => ColorBuilder.GetPaletteColor(Color ?? Enums.Color.Primary, theme.Filled, theme.Filled.Primary),
        BubbleVariant.Outlined => ColorBuilder.GetPaletteColor(Color, theme.Outlined, theme.NeutralOutlined),
        _ => ColorBuilder.GetPaletteColor(Color, theme.Soft, theme.Neutral)
    };

    private EventCallback ReactionClickCallback(BubbleReaction reaction) => OnReactionClick.HasDelegate
        ? EventCallback.Factory.Create(this, () => OnReactionClick.InvokeAsync(reaction))
        : default;

    private string? AriaPressed(BubbleReaction reaction)
    {
        if (!OnReactionClick.HasDelegate)
        {
            return null;
        }

        return reaction.Reacted ? "true" : "false";
    }

    private string FormatCount(int count) => count.ToString("N0", localizer.Formats.Culture);
}
