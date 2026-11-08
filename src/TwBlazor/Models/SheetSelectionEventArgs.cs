// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;

namespace TwBlazor.Models;

/// <summary>
/// Describes the selection in a <see cref="Components.TwSpreadsheet"/> workbook after it moved, passed to its
/// <see cref="Components.TwSpreadsheet.OnSelectionChange"/> callback.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed class SheetSelectionEventArgs
{
    /// <summary>
    /// Gets the sheet the selection is on.
    /// </summary>
    public required Sheet Sheet { get; init; }

    /// <summary>
    /// Gets the address of the active cell, the one typing goes into, such as <c>B3</c>.
    /// </summary>
    public required string ActiveCell { get; init; }

    /// <summary>
    /// Gets the selected range, such as <c>B3:D8</c>, or the active cell's address when only one cell is selected.
    /// </summary>
    public required string Range { get; init; }
}
