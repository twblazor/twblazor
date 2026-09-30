// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;

namespace TwBlazor.Configuration.Components;

/// <summary>
/// Theme configuration for the pagination component (<see cref="TwBlazor.Components.TwPagination"/>).
/// Override any property to customize pagination styles globally.
/// </summary>
[ExcludeFromCodeCoverage]
public class TwPaginationTheme
{
    /// <summary>
    /// Gets or sets the classes for the root <c>&lt;ul&gt;</c> element listing the pagination buttons.
    /// </summary>
    public required string List { get; set; }

    /// <summary>
    /// Gets or sets the base classes applied to all pagination buttons.
    /// </summary>
    public required string Base { get; set; }

    /// <summary>
    /// Gets or sets the sizing classes (height, minimum width, horizontal padding and text size) applied
    /// to every pagination button by default.
    /// </summary>
    public required string Size { get; set; }

    /// <summary>
    /// Gets or sets the sizing classes applied to every pagination button instead of <see cref="Size"/>
    /// when <see cref="TwBlazor.Components.TwPagination.Dense"/> is enabled.
    /// </summary>
    public required string DenseSize { get; set; }

    /// <summary>
    /// Gets or sets the classes applied to non-active (unselected) page number buttons.
    /// </summary>
    public required string Buttons { get; set; }

    /// <summary>
    /// Gets or sets the classes applied to the active (currently selected) page button.
    /// </summary>
    public required string ActiveButton { get; set; }
}