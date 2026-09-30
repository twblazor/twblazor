// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;

namespace TwBlazor.Configuration.Components;

/// <summary>
/// Theme configuration for the code block component (<see cref="TwBlazor.Components.TwCodeBlock"/>).
/// Override any property to customize code block styles globally.
/// </summary>
[ExcludeFromCodeCoverage]
public class TwCodeBlockTheme
{
    /// <summary>
    /// Gets or sets the background, border and overflow classes applied to the block's root container.
    /// </summary>
    public required string Container { get; set; }

    /// <summary>
    /// Gets or sets the classes for the header bar above the code, which holds the title and the copy button.
    /// </summary>
    public required string Header { get; set; }

    /// <summary>
    /// Gets or sets the classes for the inset panel that holds the highlighted code.
    /// </summary>
    public required string Panel { get; set; }

    /// <summary>
    /// Gets or sets the classes for the copy button's wrapper, at the end of the header.
    /// </summary>
    public required string CopyButtonWrapper { get; set; }
}
