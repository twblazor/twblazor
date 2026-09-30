using TwBlazor.Docs.Compiler;

namespace TwBlazor.Docs.Tests.Build;

public sealed class PageRouteScannerTests : IDisposable
{
    private readonly string _pagesPath = Path.Combine(Path.GetTempPath(), "twblazor-pages-" + Guid.NewGuid().ToString("N"));

    public PageRouteScannerTests() => Directory.CreateDirectory(_pagesPath);

    public void Dispose() => Directory.Delete(_pagesPath, recursive: true);

    private void WritePage(string relativePath, string content)
    {
        var path = Path.Combine(_pagesPath, relativePath);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, content);
    }

    [Fact]
    public void Scan_ReturnsHomeFirstThenRoutesAlphabetically()
    {
        // Arrange
        WritePage("Card/Card.razor", "@page \"/card\"\n<h1>Card</h1>");
        WritePage("Alert/Alert.razor", "@page \"/alert\"\n<h1>Alert</h1>");
        WritePage("Home.razor", "@page \"/\"\n<h1>Home</h1>");

        // Act
        var routes = PageRouteScanner.Scan(_pagesPath).Select(r => r.Route).ToList();

        // Assert
        Assert.Equal(["/", "/alert", "/card"], routes);
    }

    [Fact]
    public void Scan_RecordsTheSourceFileThatDeclaresEachRoute()
    {
        // Arrange
        WritePage("Card/Card.razor", "@page \"/card\"");

        // Act
        var route = Assert.Single(PageRouteScanner.Scan(_pagesPath));

        // Assert
        Assert.Equal(Path.Combine(_pagesPath, "Card", "Card.razor"), route.SourcePath);
    }

    [Fact]
    public void Scan_SkipsPagesMarkedNoIndex()
    {
        // Arrange
        WritePage("Sidebar/SidebarPreview.razor", "@page \"/sidebar/preview\"\n<HeadContent>\n    <meta name=\"robots\" content=\"noindex\" />\n</HeadContent>");
        WritePage("Sidebar/Sidebar.razor", "@page \"/sidebar\"");

        // Act
        var routes = PageRouteScanner.Scan(_pagesPath).Select(r => r.Route).ToList();

        // Assert
        Assert.Equal(["/sidebar"], routes);
    }

    [Fact]
    public void Scan_SkipsParameterisedRoutes()
    {
        // Arrange
        WritePage("Item.razor", "@page \"/items/{Id:int}\"");

        // Act & Assert
        Assert.Empty(PageRouteScanner.Scan(_pagesPath));
    }

    [Fact]
    public void Scan_IgnoresAPageDirectiveThatIsIndentedInsideACodeSample()
    {
        // Arrange - GetStarted.razor shows "@page" inside a code sample, which must not become a route.
        WritePage("GetStarted.razor", "@page \"/get-started\"\n<TwCodeBlock Content=\"@_sample\" />\n@code {\n    private const string _sample = \"\"\"\n        @page \"/component-test\"\n        \"\"\";\n}");

        // Act
        var routes = PageRouteScanner.Scan(_pagesPath).Select(r => r.Route).ToList();

        // Assert
        Assert.Equal(["/get-started"], routes);
    }

    [Fact]
    public void Scan_ReadsTheDirectiveFromAFileThatStartsWithAByteOrderMark()
    {
        // Arrange
        File.WriteAllText(Path.Combine(_pagesPath, "Breadcrumb.razor"), "@page \"/breadcrumb\"\r\n<h1>Breadcrumb</h1>", new System.Text.UTF8Encoding(encoderShouldEmitUTF8Identifier: true));

        // Act
        var routes = PageRouteScanner.Scan(_pagesPath).Select(r => r.Route).ToList();

        // Assert
        Assert.Equal(["/breadcrumb"], routes);
    }

    [Fact]
    public void Scan_ListsARouteDeclaredByTwoFilesOnce()
    {
        // Arrange
        WritePage("A.razor", "@page \"/dup\"");
        WritePage("B.razor", "@page \"/dup\"");

        // Act & Assert
        Assert.Single(PageRouteScanner.Scan(_pagesPath));
    }

    [Fact]
    public void Scan_FindsEveryRealDocsPageExceptTheNoIndexPreviews()
    {
        // Arrange - the real Pages directory, so a newly added page can't silently be left out of the sitemap.
        var pagesPath = Paths.PagesPath;

        // Act
        var routes = PageRouteScanner.Scan(pagesPath).Select(r => r.Route).ToList();

        // Assert
        Assert.Equal("/", routes[0]);
        Assert.Contains("/get-started", routes);
        Assert.Contains("/card", routes);
        Assert.Contains("/date-range-picker", routes);
        Assert.DoesNotContain("/sidebar/preview", routes);
        Assert.DoesNotContain("/sidebar/preview-navigation", routes);
        Assert.DoesNotContain("/component-test", routes);
    }
}
