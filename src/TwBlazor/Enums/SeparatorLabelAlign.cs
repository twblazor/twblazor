// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

namespace TwBlazor.Enums;

/// <summary>
/// Defines where the label of a <see cref="TwBlazor.Components.TwSeparator"/> sits along its line.
/// </summary>
public enum SeparatorLabelAlign
{
    /// <summary>
    /// In the middle of the line. The default.
    /// </summary>
    Center,

    /// <summary>
    /// Near the start of the line: the left in a left to right layout, the right in a right to left one, and the
    /// top of a vertical separator.
    /// </summary>
    Start,

    /// <summary>
    /// Near the end of the line: the right in a left to right layout, the left in a right to left one, and the
    /// bottom of a vertical separator.
    /// </summary>
    End
}
