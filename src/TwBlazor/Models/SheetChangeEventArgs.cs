// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;

namespace TwBlazor.Models;

/// <summary>
/// Describes cells of a <see cref="Components.TwSpreadsheet"/> workbook that changed, passed to its
/// <see cref="Components.TwSpreadsheet.OnCellsChange"/> callback.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed class SheetChangeEventArgs
{
    /// <summary>
    /// Gets the sheet the cells are on.
    /// </summary>
    public required Sheet Sheet { get; init; }

    /// <summary>
    /// Gets the addresses of the cells whose value or formatting changed, such as <c>A1</c>.
    /// </summary>
    public required IReadOnlyList<string> Addresses { get; init; }
}
