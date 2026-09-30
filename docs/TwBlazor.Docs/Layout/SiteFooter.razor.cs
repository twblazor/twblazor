namespace TwBlazor.Docs.Layout;

/// <summary>
/// The site-wide footer: links to the GitHub project, the documentation and the MIT license, plus the
/// open source statement.
/// </summary>
public partial class SiteFooter
{
#pragma warning disable S1075 // Fixed external project links, not environment-specific
    /// <summary>Gets the URL of the GitHub project.</summary>
    public const string GitHubUrl = "https://github.com/TwBlazor/twblazor";

    /// <summary>Gets the URL of the API documentation.</summary>
    public const string DocumentationUrl = "https://twblazor.github.io/twblazor/";

    /// <summary>Gets the URL of the MIT license file.</summary>
    public const string LicenseUrl = "https://github.com/TwBlazor/twblazor/blob/develop/LICENSE.txt";
#pragma warning restore S1075

    private const string linkClasses = "inline-flex items-center gap-2 rounded-md px-2.5 py-1 text-purple-900 dark:text-purple-200 transition-colors duration-150 hover:bg-purple-100 hover:text-purple-700 dark:hover:bg-purple-900/40 dark:hover:text-purple-100 focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-purple-600 dark:focus-visible:outline-purple-400";
}
