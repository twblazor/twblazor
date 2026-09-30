namespace TwBlazor.Docs.Compiler;

/// <summary>
/// Paths helper for the docs compiler
/// </summary>
public static class Paths
{
    private static string CombineDir(string baseDirectory, string directoryName)
    {
        if (Path.IsPathRooted(directoryName))
            throw new ArgumentException("directoryName must be a relative directory name, not a rooted path.", nameof(directoryName));
        return Path.Combine(baseDirectory, directoryName);
    }

    public static string GetSolutionDirectory()
    {
        // Start from the executable location
        var currentDir = AppContext.BaseDirectory;

        // Walk up the directory tree to find the repository root (marked by the .slnx file)
        while (!string.IsNullOrEmpty(currentDir))
        {
            if (Directory.GetFiles(currentDir, "*.slnx").Length > 0 ||
                Directory.Exists(CombineDir(currentDir, "docs/TwBlazor.Docs")))
                return currentDir;

            var parent = Directory.GetParent(currentDir);
            if (parent == null)
                break;

            currentDir = parent.FullName;
        }

        // Fallback: try from current directory
        currentDir = Directory.GetCurrentDirectory();
        while (!string.IsNullOrEmpty(currentDir))
        {
            if (Directory.GetFiles(currentDir, "*.slnx").Length > 0 ||
                Directory.Exists(CombineDir(currentDir, "docs/TwBlazor.Docs")))
                return currentDir;

            var parent = Directory.GetParent(currentDir);
            if (parent == null)
                break;

            currentDir = parent.FullName;
        }

        throw new InvalidOperationException($"Solution directory not found. Current dir: {Directory.GetCurrentDirectory()}, Base dir: {AppContext.BaseDirectory}");
    }

    public static string DocsProjectPath => CombineDir(GetSolutionDirectory(), "docs/TwBlazor.Docs");
    public static string ExamplesPath => CombineDir(DocsProjectPath, "Examples");
    public static string GeneratedPath => CombineDir(DocsProjectPath, "Generated");
    public static string PagesPath => CombineDir(DocsProjectPath, "Pages");
    public static string LayoutPath => CombineDir(DocsProjectPath, "Layout");
    public static string SitemapPath => Path.Combine(GetSolutionDirectory(), "docs", "TwBlazor.WASM", "wwwroot", "sitemap.xml");
    public static string ThemeCsFilePath => Path.Combine(GetSolutionDirectory(), "src", "TwBlazor.Theme", "Theme.cs");
    public static string ServerProgramCsFilePath => Path.Combine(GetSolutionDirectory(), "docs", "TwBlazor.Server", "Program.cs");
}
