// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using System.IO.Compression;
using System.Text;
using System.Xml;

namespace TwBlazor.Export.Packaging;

/// <summary>
/// Builds a zip based document package in memory, such as an Excel workbook, a Word document or an OpenDocument
/// file, from XML and text parts.
/// </summary>
/// <remarks>Parts are written in the order they are added, which matters for OpenDocument, whose <c>mimetype</c> part
/// must come first and be stored uncompressed.</remarks>
internal sealed class ZipPackage : IDisposable
{
    private static readonly XmlWriterSettings _xmlSettings = new() { Encoding = new UTF8Encoding(false) };

    private readonly MemoryStream _output = new();
    private ZipArchive? archive;

    /// <summary>
    /// Starts an empty package.
    /// </summary>
    public ZipPackage()
    {
        archive = new ZipArchive(_output, ZipArchiveMode.Create, leaveOpen: true);
    }

    /// <summary>
    /// Adds an XML part, written by <paramref name="write"/> between the XML declaration and the end of the document.
    /// </summary>
    /// <param name="path">The part's path in the package, such as <c>word/document.xml</c>.</param>
    /// <param name="write">Writes the part's root element.</param>
    public void AddXml(string path, Action<XmlWriter> write)
    {
        var entry = openArchive.CreateEntry(path, CompressionLevel.Optimal);
        using var stream = entry.Open();
        using var writer = XmlWriter.Create(stream, _xmlSettings);
        writer.WriteStartDocument(true);
        write(writer);
        writer.WriteEndDocument();
    }

    /// <summary>
    /// Adds a text part in UTF-8, without a byte order mark.
    /// </summary>
    /// <param name="path">The part's path in the package.</param>
    /// <param name="text">The text.</param>
    /// <param name="compress">Whether to compress the part. OpenDocument's <c>mimetype</c> part must not be.</param>
    public void AddText(string path, string text, bool compress = true)
    {
        var entry = openArchive.CreateEntry(path, compress ? CompressionLevel.Optimal : CompressionLevel.NoCompression);
        using var stream = entry.Open();
        var bytes = new UTF8Encoding(false).GetBytes(text);
        stream.Write(bytes, 0, bytes.Length);
    }

    /// <summary>
    /// Finishes the package and returns its bytes. No parts can be added afterwards.
    /// </summary>
    /// <returns>The contents of the package file.</returns>
    public byte[] ToArray()
    {
        archive?.Dispose();
        archive = null;
        return _output.ToArray();
    }

    private ZipArchive openArchive => archive ?? throw new InvalidOperationException("The package has already been finished.");

    /// <inheritdoc />
    public void Dispose()
    {
        archive?.Dispose();
        _output.Dispose();
    }
}
