// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;

namespace TwBlazor.Models;

/// <summary>
/// A file a component exported, ready to save or send, such as the result of
/// <see cref="Components.TwTextEditorExport.Export(string?, Enums.TextEditorExportFormat, string)"/>.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed class ExportFile
{
    /// <summary>
    /// Gets the file name, with its extension, such as <c>notes.docx</c>.
    /// </summary>
    public required string FileName { get; init; }

    /// <summary>
    /// Gets the file's media type, such as <c>text/markdown</c>.
    /// </summary>
    public required string ContentType { get; init; }

    /// <summary>
    /// Gets the file's contents.
    /// </summary>
    [SuppressMessage("Performance", "CA1819:Properties should not return arrays", Justification = "The file's bytes, handed straight to a stream or a download.")]
    public required byte[] Content { get; init; }
}
