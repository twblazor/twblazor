// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using TwBlazor.Enums;

namespace TwBlazor.Models;

/// <summary>
/// Describes the selection in a <see cref="Components.TwTextEditor"/> after it moved, passed to its
/// <see cref="Components.TwTextEditor.OnSelectionChange"/> callback.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed class TextEditorSelectionEventArgs
{
    /// <summary>
    /// Gets where the selection starts, as a count of characters from the start of the text.
    /// </summary>
    public required int Index { get; init; }

    /// <summary>
    /// Gets how many characters are selected, or 0 when only the caret is placed.
    /// </summary>
    public required int Length { get; init; }

    /// <summary>
    /// Gets the formats applied at the selection, such as <see cref="TextEditorFormat.Bold"/> and
    /// <see cref="TextEditorFormat.Heading2"/>.
    /// </summary>
    public required IReadOnlySet<TextEditorFormat> Formats { get; init; }

    /// <summary>
    /// Gets the address of the link the selection is in, or <see langword="null"/> when it is not in a link.
    /// </summary>
    public string? LinkUrl { get; init; }
}
