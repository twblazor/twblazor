// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;

namespace TwBlazor.Models;

/// <summary>
/// The selection in a <see cref="Components.TwTextEditor"/> as the browser reports it, passed to
/// <see cref="Components.TwTextEditor.NotifyStateAsync(TextEditorSelectionState?)"/>.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed class TextEditorSelectionState
{
    /// <summary>
    /// Gets or sets where the selection starts, as a count of characters from the start of the text.
    /// </summary>
    public int Index { get; set; }

    /// <summary>
    /// Gets or sets how many characters are selected.
    /// </summary>
    public int Length { get; set; }

    /// <summary>
    /// Gets or sets the names of the <see cref="Enums.TextEditorFormat"/> values applied at the selection.
    /// </summary>
    public string[]? Formats { get; set; }

    /// <summary>
    /// Gets or sets the address of the link the selection is in, or <see langword="null"/>.
    /// </summary>
    public string? Link { get; set; }
}
