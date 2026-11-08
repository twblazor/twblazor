// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;

namespace TwBlazor.Models;

/// <summary>
/// Describes the content of a <see cref="Components.TwTextEditor"/> after it changed, passed to its
/// <see cref="Components.TwTextEditor.OnTextChange"/> callback.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed class TextEditorChangeEventArgs
{
    /// <summary>
    /// Gets the content as sanitized HTML, or an empty string when the editor holds no text.
    /// </summary>
    public required string Html { get; init; }

    /// <summary>
    /// Gets the content as plain text, with a line break between paragraphs.
    /// </summary>
    public required string Text { get; init; }

    /// <summary>
    /// Gets the number of characters in <see cref="Text"/>, not counting line breaks.
    /// </summary>
    public required int Length { get; init; }
}
