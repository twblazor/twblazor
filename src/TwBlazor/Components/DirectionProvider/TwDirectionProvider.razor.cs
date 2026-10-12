// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using Microsoft.AspNetCore.Components;
using TwBlazor.Enums;

namespace TwBlazor.Components;

/// <summary>
/// Sets the direction, left to right or right to left, of everything inside it. It writes the <c>dir</c>
/// attribute the styles react to and tells the components inside which way their arrow keys should move.
/// </summary>
/// <remarks>
/// Wrap the whole application in one to have it follow the current language, or wrap part of a page to give
/// that part a different direction from the rest. An application that already sets <c>dir</c> on its
/// <c>html</c> element from the same culture does not need one.
/// </remarks>
public partial class TwDirectionProvider : TwBlazorComponentBase
{
    /// <summary>
    /// Gets or sets the direction of the content. Leave unset to follow an enclosing provider, and failing
    /// that the current language.
    /// </summary>
    [Parameter] public TextDirection? Direction { get; set; }

    /// <summary>
    /// Gets or sets the content the direction applies to.
    /// </summary>
    [Parameter] public RenderFragment? ChildContent { get; set; }

    private TextDirection? resolvedDirection => Direction ?? effectiveDirection;

    private string directionAttribute => resolvedDirection == TextDirection.RightToLeft ? "rtl" : "ltr";
}
