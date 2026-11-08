// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using System.Xml;

namespace TwBlazor.Export.Packaging;

/// <summary>
/// A relationship from one part of an Office Open XML package to another part, or to an outside address.
/// </summary>
/// <param name="Id">The id the source part refers to it by, such as <c>rId1</c>.</param>
/// <param name="Type">The relationship type, one of the <see cref="OpenXmlParts"/> constants.</param>
/// <param name="Target">The target part's path relative to the source, or an outside address.</param>
/// <param name="External">Whether the target is an outside address, such as a link.</param>
internal sealed record OpenXmlRelationship(string Id, string Type, string Target, bool External = false);

/// <summary>
/// Writes the parts every Office Open XML package (an Excel workbook or a Word document) needs: the content types
/// and the relationships.
/// </summary>
internal static class OpenXmlParts
{
    /// <summary>The namespace of a package's relationships parts.</summary>
    public const string PackageRelationshipsNamespace = "http://schemas.openxmlformats.org/package/2006/relationships";

    /// <summary>The namespace of relationship ids used inside a document part.</summary>
    public const string RelationshipsNamespace = "http://schemas.openxmlformats.org/officeDocument/2006/relationships";

    /// <summary>The relationship from the package to its main document part.</summary>
    public const string OfficeDocumentRelationship = "http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument";

    /// <summary>The relationship from the package to its core properties, such as the title.</summary>
    public const string CorePropertiesRelationship = "http://schemas.openxmlformats.org/package/2006/relationships/metadata/core-properties";

    /// <summary>The relationship from a document to its styles.</summary>
    public const string StylesRelationship = "http://schemas.openxmlformats.org/officeDocument/2006/relationships/styles";

    /// <summary>The relationship from a Word document to its list numbering definitions.</summary>
    public const string NumberingRelationship = "http://schemas.openxmlformats.org/officeDocument/2006/relationships/numbering";

    /// <summary>The relationship from a Word document to the address of a link.</summary>
    public const string HyperlinkRelationship = "http://schemas.openxmlformats.org/officeDocument/2006/relationships/hyperlink";

    /// <summary>The relationship from a workbook to a worksheet.</summary>
    public const string WorksheetRelationship = "http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet";

    private const string contentTypesNamespace = "http://schemas.openxmlformats.org/package/2006/content-types";

    /// <summary>
    /// Writes <c>[Content_Types].xml</c>: the defaults for <c>.rels</c> and <c>.xml</c> parts, and the content type of
    /// each of <paramref name="parts"/>.
    /// </summary>
    /// <param name="writer">The writer for the part.</param>
    /// <param name="parts">Each part's absolute path, such as <c>/word/document.xml</c>, with its content type.</param>
    public static void WriteContentTypes(XmlWriter writer, params (string Part, string ContentType)[] parts)
    {
        writer.WriteStartElement("Types", contentTypesNamespace);
        WriteDefault("rels", "application/vnd.openxmlformats-package.relationships+xml");
        WriteDefault("xml", "application/xml");
        foreach (var (part, contentType) in parts)
        {
            writer.WriteStartElement("Override", contentTypesNamespace);
            writer.WriteAttributeString("PartName", part);
            writer.WriteAttributeString("ContentType", contentType);
            writer.WriteEndElement();
        }

        writer.WriteEndElement();

        void WriteDefault(string extension, string contentType)
        {
            writer.WriteStartElement("Default", contentTypesNamespace);
            writer.WriteAttributeString("Extension", extension);
            writer.WriteAttributeString("ContentType", contentType);
            writer.WriteEndElement();
        }
    }

    /// <summary>
    /// Writes a relationships part, such as <c>_rels/.rels</c>.
    /// </summary>
    /// <param name="writer">The writer for the part.</param>
    /// <param name="relationships">The relationships.</param>
    public static void WriteRelationships(XmlWriter writer, IEnumerable<OpenXmlRelationship> relationships)
    {
        writer.WriteStartElement("Relationships", PackageRelationshipsNamespace);
        foreach (var relationship in relationships)
        {
            writer.WriteStartElement("Relationship", PackageRelationshipsNamespace);
            writer.WriteAttributeString("Id", relationship.Id);
            writer.WriteAttributeString("Type", relationship.Type);
            writer.WriteAttributeString("Target", relationship.Target);
            if (relationship.External)
            {
                writer.WriteAttributeString("TargetMode", "External");
            }

            writer.WriteEndElement();
        }

        writer.WriteEndElement();
    }

    /// <summary>
    /// Writes <c>docProps/core.xml</c> with the document's title.
    /// </summary>
    /// <param name="writer">The writer for the part.</param>
    /// <param name="title">The title, or <see langword="null"/> for none.</param>
    public static void WriteCoreProperties(XmlWriter writer, string? title)
    {
        const string cp = "http://schemas.openxmlformats.org/package/2006/metadata/core-properties";
        const string dc = "http://purl.org/dc/elements/1.1/";
        writer.WriteStartElement("cp", "coreProperties", cp);
        writer.WriteAttributeString("xmlns", "dc", null, dc);
        if (!string.IsNullOrWhiteSpace(title))
        {
            writer.WriteElementString("dc", "title", dc, XmlCharacters.Clean(title));
        }

        writer.WriteEndElement();
    }
}
