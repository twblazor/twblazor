// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

namespace TwBlazor.Enums;

/// <summary>
/// Defines where a <see cref="TwBlazor.Components.TwInputGroupAddon"/> sits in its
/// <see cref="TwBlazor.Components.TwInputGroup"/>, whatever its position in the markup.
/// </summary>
public enum InputGroupAlign
{
    /// <summary>
    /// Before the input on the same line: on the left in a left to right layout, on the right in a right to left one.
    /// The default.
    /// </summary>
    InlineStart,

    /// <summary>
    /// After the input on the same line: on the right in a left to right layout, on the left in a right to left one.
    /// </summary>
    InlineEnd,

    /// <summary>
    /// On its own row above the input, such as a heading or a toolbar for a multiline text field.
    /// </summary>
    BlockStart,

    /// <summary>
    /// On its own row below the input, such as a character counter or a send button for a multiline text field.
    /// </summary>
    BlockEnd
}
