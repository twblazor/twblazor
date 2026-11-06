// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

namespace TwBlazor.Enums;

/// <summary>
/// Defines whether a component runs along a row or down a column, for example a
/// <see cref="TwBlazor.Components.TwSeparator"/>.
/// </summary>
public enum SeparatorOrientation
{
    /// <summary>
    /// Runs from the start edge to the end edge, along a row. The default.
    /// </summary>
    Horizontal,

    /// <summary>
    /// Runs from the top to the bottom, down a column.
    /// </summary>
    Vertical
}
