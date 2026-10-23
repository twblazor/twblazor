using TwBlazor.Models;
using IconEnum = TwBlazor.Enums.Icon;

namespace TwBlazor.Docs.Layout;

/// <summary>
/// The site-wide footer, built on <c>TwFooter</c>: the twblazor name and tagline with links to the Discord server and
/// the GitHub project, columns of guide, resource and community links, and the open source statement.
/// </summary>
public partial class SiteFooter
{
#pragma warning disable S1075 // Fixed external project links, not environment-specific
    /// <summary>Gets the permanent invite link of the Discord server, see <see cref="DiscordInviteLink"/>.</summary>
    public const string DiscordUrl = DiscordInviteLink.PermanentUrl;

    /// <summary>Gets the URL of the GitHub project.</summary>
    public const string GitHubUrl = "https://github.com/TwBlazor/twblazor";

    /// <summary>Gets the URL of the API documentation.</summary>
    public const string DocumentationUrl = "https://twblazor.github.io/twblazor/";

    /// <summary>Gets the URL of the MIT license file.</summary>
    public const string LicenseUrl = "https://github.com/TwBlazor/twblazor/blob/develop/LICENSE.txt";

    /// <summary>Gets the URL of the <c>llms.txt</c> file that points AI assistants at the setup guide, Theme builder and component pages.</summary>
    public const string LlmsTxtUrl = "https://twblazor.com/llms.txt";

    /// <summary>Gets the URL of the contributing guidelines.</summary>
    public const string ContributingUrl = "https://github.com/TwBlazor/twblazor/blob/develop/CONTRIBUTING.md";

    /// <summary>Gets the URL of the code of conduct.</summary>
    public const string CodeOfConductUrl = "https://github.com/TwBlazor/twblazor/blob/develop/CODE_OF_CONDUCT.md";

    /// <summary>Gets the URL for reporting an issue on GitHub.</summary>
    public const string IssuesUrl = "https://github.com/TwBlazor/twblazor/issues";
#pragma warning restore S1075

    private static readonly IReadOnlyList<FooterSection> _sections =
    [
        new("Get started", [
            new FooterLink("Install guide", "/get-started"),
            new FooterLink("Theming", "/theme"),
            new FooterLink("Class merging", "/class-merge"),
            new FooterLink("Localization", "/localization"),
        ]),
        new("Guides", [
            new FooterLink("Tailwind CSS with Blazor", "/tailwind-blazor"),
            new FooterLink("Why twblazor?", "/why-twblazor"),
            new FooterLink("Right to left", "/direction-provider"),
            new FooterLink("AI agents and skills", "/ai-agents"),
        ]),
        new("Resources", [
            new FooterLink("API documentation", DocumentationUrl, IconEnum.Book, OpensInNewTab: true),
            new FooterLink("llms.txt", LlmsTxtUrl, IconEnum.Robot, OpensInNewTab: true),
            new FooterLink("MIT License", LicenseUrl, IconEnum.File_Earmark_Text, OpensInNewTab: true),
        ]),
        new("Community", [
            new FooterLink("Contributing", ContributingUrl, OpensInNewTab: true),
            new FooterLink("Code of conduct", CodeOfConductUrl, OpensInNewTab: true),
            new FooterLink("Report an issue", IssuesUrl, OpensInNewTab: true),
        ]),
    ];

    private const string socialLinkClasses =
        "inline-flex min-h-10 items-center gap-2 rounded-lg border border-purple-200 bg-white px-3.5 text-sm font-medium text-purple-900 shadow-xs transition-colors duration-150 hover:border-fuchsia-400 hover:text-fuchsia-700 focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-purple-600 dark:border-purple-500/30 dark:bg-purple-500/10 dark:text-purple-100 dark:hover:border-fuchsia-400 dark:hover:text-fuchsia-200 dark:focus-visible:outline-purple-300";
}
