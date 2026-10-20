// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

namespace TwBlazor.Enums;

/// <summary>
/// Defines what a marker draws.
/// </summary>
public enum MarkerVariant
{
    /// <summary>
    /// A single line of status text with an icon, such as a tool call an assistant made. The default.
    /// </summary>
    Status,

    /// <summary>
    /// A horizontal rule between rows, with an optional label in the middle such as "Today".
    /// </summary>
    Separator
}
