namespace TwBlazor.Docs.Layout;

/// <summary>
/// A link to the Discord server that opens in a new tab, starting on the permanent invite and swapping to the
/// server's own invite the first time a visitor reaches for it.
/// </summary>
public partial class DiscordInviteLink
{
#pragma warning disable S1075 // Fixed external link to the project's own Discord server, not environment-specific
    /// <summary>
    /// The permanent invite link of the Discord server, used until, and unless, the server's own invite has been
    /// looked up.
    /// </summary>
    public const string PermanentUrl = "https://discord.gg/f7CBdf38Nw";
#pragma warning restore S1075
}
