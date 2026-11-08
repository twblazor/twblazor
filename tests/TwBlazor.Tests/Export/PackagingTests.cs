using System.IO.Compression;
using System.Text;
using System.Xml.Linq;
using TwBlazor.Export.Packaging;

namespace TwBlazor.Tests.Export;

public class PackagingTests
{
    private static ZipArchive Open(byte[] bytes) => new(new MemoryStream(bytes), ZipArchiveMode.Read);

    private static string Read(ZipArchive zip, string path)
    {
        using var reader = new StreamReader(zip.GetEntry(path)!.Open(), Encoding.UTF8);
        return reader.ReadToEnd();
    }

    [Fact]
    public void ZipPackage_WritesPartsInOrder_AsXmlAndText()
    {
        using var package = new ZipPackage();
        package.AddText("mimetype", "application/test", compress: false);
        package.AddXml("doc.xml", writer => writer.WriteElementString("root", "hello & <world>"));

        using var zip = Open(package.ToArray());

        Assert.Equal(["mimetype", "doc.xml"], zip.Entries.Select(e => e.FullName));
        Assert.Equal("application/test", Read(zip, "mimetype"));
        Assert.Equal("hello & <world>", XDocument.Parse(Read(zip, "doc.xml")).Root!.Value);
    }

    [Fact]
    public void ZipPackage_StoresAnUncompressedPart_Uncompressed()
    {
        using var package = new ZipPackage();
        package.AddText("mimetype", new string('a', 200), compress: false);

        using var zip = Open(package.ToArray());

        var entry = zip.GetEntry("mimetype")!;
        Assert.Equal(entry.Length, entry.CompressedLength);
    }

    [Fact]
    public void ZipPackage_RefusesPartsAfterItIsFinished()
    {
        using var package = new ZipPackage();
        package.ToArray();

        Assert.Throws<InvalidOperationException>(() => package.AddText("late.txt", "x"));
    }

    [Fact]
    public void OpenXmlParts_WritesContentTypes_WithDefaultsAndOverrides()
    {
        using var package = new ZipPackage();
        package.AddXml("[Content_Types].xml", writer => OpenXmlParts.WriteContentTypes(writer, ("/word/document.xml", "application/main+xml")));

        using var zip = Open(package.ToArray());
        var types = XDocument.Parse(Read(zip, "[Content_Types].xml")).Root!;
        XNamespace ns = "http://schemas.openxmlformats.org/package/2006/content-types";

        Assert.Equal(["rels", "xml"], types.Elements(ns + "Default").Select(d => (string)d.Attribute("Extension")!));
        var over = Assert.Single(types.Elements(ns + "Override"));
        Assert.Equal("/word/document.xml", (string)over.Attribute("PartName")!);
        Assert.Equal("application/main+xml", (string)over.Attribute("ContentType")!);
    }

    [Fact]
    public void OpenXmlParts_WritesRelationships_MarkingExternalTargets()
    {
        using var package = new ZipPackage();
        package.AddXml("_rels/.rels", writer => OpenXmlParts.WriteRelationships(writer,
        [
            new("rId1", OpenXmlParts.StylesRelationship, "styles.xml"),
            new("rId2", OpenXmlParts.HyperlinkRelationship, "https://example.com", External: true),
        ]));

        using var zip = Open(package.ToArray());
        XNamespace ns = OpenXmlParts.PackageRelationshipsNamespace;
        var relationships = XDocument.Parse(Read(zip, "_rels/.rels")).Root!.Elements(ns + "Relationship").ToList();

        Assert.Equal(2, relationships.Count);
        Assert.Null(relationships[0].Attribute("TargetMode"));
        Assert.Equal("External", (string)relationships[1].Attribute("TargetMode")!);
        Assert.Equal("https://example.com", (string)relationships[1].Attribute("Target")!);
    }

    [Fact]
    public void OpenXmlParts_WritesTheTitle_InTheCoreProperties()
    {
        using var package = new ZipPackage();
        package.AddXml("core.xml", writer => OpenXmlParts.WriteCoreProperties(writer, "Notes"));
        package.AddXml("empty.xml", writer => OpenXmlParts.WriteCoreProperties(writer, null));

        using var zip = Open(package.ToArray());
        XNamespace dc = "http://purl.org/dc/elements/1.1/";

        Assert.Equal("Notes", XDocument.Parse(Read(zip, "core.xml")).Root!.Element(dc + "title")!.Value);
        Assert.Null(XDocument.Parse(Read(zip, "empty.xml")).Root!.Element(dc + "title"));
    }

    [Theory]
    [InlineData("plain", "plain")]
    [InlineData("a\u0001b\u0008c", "abc")]
    [InlineData("tab\tand\nline", "tab\tand\nline")]
    [InlineData("emoji \U0001F600", "emoji \U0001F600")]
    public void XmlCharacters_RemovesWhatXmlCannotHold(string input, string expected)
    {
        Assert.Equal(expected, XmlCharacters.Clean(input));
    }

    [Fact]
    public void XmlCharacters_RemovesALoneSurrogate()
    {
        // Built here rather than passed as theory data, which cannot carry a lone surrogate.
        var input = "lone " + (char)0xD800 + " surrogate";

        Assert.Equal("lone  surrogate", XmlCharacters.Clean(input));
    }
}
