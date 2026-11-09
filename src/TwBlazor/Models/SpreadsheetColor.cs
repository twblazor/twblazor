// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;

namespace TwBlazor.Models;

/// <summary>
/// A colour offered by the text color and fill color menus of a <see cref="Components.TwSpreadsheet"/>.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed class SpreadsheetColor
{
    /// <summary>
    /// Gets the name the menu shows and screen readers read out, such as <c>Brand blue</c>.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Gets the colour as hex, such as <c>#2563EB</c>.
    /// </summary>
    public required string Value { get; init; }
}
