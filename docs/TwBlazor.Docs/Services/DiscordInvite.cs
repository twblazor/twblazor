using System.Text.Json;

namespace TwBlazor.Docs.Services;

/// <summary>
/// Supplies an invite link to the twblazor Discord server.
/// </summary>
public interface IDiscordInviteSource
{
    /// <summary>
    /// Gets a current invite link.
    /// </summary>
    /// <param name="cancellationToken">Cancels the lookup.</param>
    /// <returns>The invite link, or <see langword="null"/> when none could be found.</returns>
    Task<string?> GetInviteAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Reads the invite link from the Discord server's public widget.
/// </summary>
/// <remarks>
/// An invite a member creates shows that member's profile to whoever opens it. The widget's invite belongs to the
/// server, so it shows nobody, but Discord replaces it about once a day and it cannot be written into the page.
/// The link is remembered for a few minutes so that a busy site asks Discord only now and then.
/// </remarks>
public sealed class DiscordWidgetInviteSource : IDiscordInviteSource
{
#pragma warning disable S1075 // Fixed external link to the project's own Discord server, not environment-specific
    /// <summary>The address of the server's widget, which holds the current invite.</summary>
    public const string WidgetUrl = "https://discord.com/api/guilds/1558478203499323534/widget.json";
#pragma warning restore S1075

    private static readonly TimeSpan _lifetime = TimeSpan.FromMinutes(5);

    private static readonly string[] _inviteHosts = ["discord.com", "discord.gg"];

    private readonly HttpClient _http;
    private readonly TimeProvider _time;
    private string? _invite;
    private DateTimeOffset _fetchedAt;

    /// <summary>
    /// Creates a source that asks Discord through <paramref name="http"/>.
    /// </summary>
    /// <param name="http">The client used to read the widget.</param>
    /// <param name="time">The clock used to age the remembered link. Defaults to the system clock.</param>
    public DiscordWidgetInviteSource(HttpClient http, TimeProvider? time = null)
    {
        _http = http;
        _time = time ?? TimeProvider.System;
    }

    /// <summary>
    /// Gets the source used when the application has not registered one of its own.
    /// </summary>
    public static DiscordWidgetInviteSource Shared { get; } = new(new HttpClient { Timeout = TimeSpan.FromSeconds(4) });

    /// <inheritdoc />
    public async Task<string?> GetInviteAsync(CancellationToken cancellationToken = default)
    {
        if (_invite is not null && _time.GetUtcNow() - _fetchedAt < _lifetime)
            return _invite;

        try
        {
            var json = await _http.GetStringAsync(WidgetUrl, cancellationToken);

            if (ReadInvite(json) is { } invite)
            {
                _invite = invite;
                _fetchedAt = _time.GetUtcNow();
            }
        }
        catch (Exception exception) when (exception is HttpRequestException or TaskCanceledException or JsonException)
        {
            // Discord could not be reached or answered with something else. The last link read is still returned,
            // since it stays valid for about a day.
        }

        return _invite;
    }

    /// <summary>
    /// Reads the invite link out of the widget's json, accepting only an https link to Discord itself.
    /// </summary>
    private static string? ReadInvite(string json)
    {
        using var document = JsonDocument.Parse(json);

        if (document.RootElement.ValueKind != JsonValueKind.Object
            || !document.RootElement.TryGetProperty("instant_invite", out var property)
            || property.ValueKind != JsonValueKind.String)
        {
            return null;
        }

        var invite = property.GetString();

        return Uri.TryCreate(invite, UriKind.Absolute, out var uri) && uri.Scheme == Uri.UriSchemeHttps && _inviteHosts.Contains(uri.Host)
            ? invite
            : null;
    }
}
