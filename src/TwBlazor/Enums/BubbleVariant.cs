// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

namespace TwBlazor.Enums;

/// <summary>
/// Defines how a chat bubble is filled.
/// </summary>
public enum BubbleVariant
{
    /// <summary>
    /// A light tint of the bubble's color, or a neutral gray when it has none. The default.
    /// </summary>
    Soft,

    /// <summary>
    /// A solid fill of the bubble's color, or the primary color when it has none.
    /// </summary>
    Filled,

    /// <summary>
    /// A border in the bubble's color with no fill.
    /// </summary>
    Outlined
}
