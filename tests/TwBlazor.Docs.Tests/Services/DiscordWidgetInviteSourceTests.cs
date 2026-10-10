using System.Net;
using TwBlazor.Docs.Services;

namespace TwBlazor.Docs.Tests.Services;

public class DiscordWidgetInviteSourceTests
{
    private sealed class StubHandler(Func<HttpRequestMessage, HttpResponseMessage> respond) : HttpMessageHandler
    {
        public List<Uri?> Requests { get; } = [];

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Requests.Add(request.RequestUri);
            return Task.FromResult(respond(request));
        }
    }

    private sealed class ManualTime : TimeProvider
    {
        public DateTimeOffset Now { get; set; } = new(2026, 10, 10, 12, 0, 0, TimeSpan.Zero);

        public override DateTimeOffset GetUtcNow() => Now;
    }

    private static HttpResponseMessage Json(string json) => new(HttpStatusCode.OK) { Content = new StringContent(json) };

    private static string Widget(string invite) => $$"""{"id":"1","name":"twblazor","instant_invite":"{{invite}}","members":[]}""";

    [Fact]
    public async Task GetInviteAsync_ReturnsTheInvite_FromTheServersWidget()
    {
        // Arrange
        var handler = new StubHandler(_ => Json(Widget("https://discord.com/invite/abc123")));
        var source = new DiscordWidgetInviteSource(new HttpClient(handler));

        // Act
        var invite = await source.GetInviteAsync(TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal("https://discord.com/invite/abc123", invite);
        Assert.Equal(new Uri(DiscordWidgetInviteSource.WidgetUrl), Assert.Single(handler.Requests));
    }

    [Fact]
    public async Task GetInviteAsync_RemembersTheInvite_ForAFewMinutes_ThenAsksAgain()
    {
        // Arrange
        var code = 0;
        var handler = new StubHandler(_ => Json(Widget($"https://discord.com/invite/code{++code}")));
        var time = new ManualTime();
        var source = new DiscordWidgetInviteSource(new HttpClient(handler), time);
        var cancellation = TestContext.Current.CancellationToken;

        // Act
        var first = await source.GetInviteAsync(cancellation);
        time.Now += TimeSpan.FromMinutes(4);
        var remembered = await source.GetInviteAsync(cancellation);
        time.Now += TimeSpan.FromMinutes(2);
        var refreshed = await source.GetInviteAsync(cancellation);

        // Assert
        Assert.Equal("https://discord.com/invite/code1", first);
        Assert.Equal("https://discord.com/invite/code1", remembered);
        Assert.Equal("https://discord.com/invite/code2", refreshed);
        Assert.Equal(2, handler.Requests.Count);
    }

    [Theory]
    [InlineData("""{"instant_invite":"javascript:alert(1)"}""")]
    [InlineData("""{"instant_invite":"http://discord.com/invite/abc"}""")]
    [InlineData("""{"instant_invite":"https://discord.com.example.org/invite/abc"}""")]
    [InlineData("""{"instant_invite":"https://example.org/invite/abc"}""")]
    [InlineData("""{"instant_invite":null}""")]
    [InlineData("""{"instant_invite":42}""")]
    [InlineData("""{"name":"twblazor"}""")]
    [InlineData("""[]""")]
    [InlineData("""not json""")]
    public async Task GetInviteAsync_ReturnsNull_WhenTheWidgetHoldsNoHttpsLinkToDiscord(string json)
    {
        // Arrange
        var source = new DiscordWidgetInviteSource(new HttpClient(new StubHandler(_ => Json(json))));

        // Act
        var invite = await source.GetInviteAsync(TestContext.Current.CancellationToken);

        // Assert
        Assert.Null(invite);
    }

    [Fact]
    public async Task GetInviteAsync_AcceptsTheShortDiscordDomain()
    {
        // Arrange
        var source = new DiscordWidgetInviteSource(new HttpClient(new StubHandler(_ => Json(Widget("https://discord.gg/abc123")))));

        // Act & Assert
        Assert.Equal("https://discord.gg/abc123", await source.GetInviteAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task GetInviteAsync_ReturnsNull_WhenDiscordCannotBeReached()
    {
        // Arrange
        var source = new DiscordWidgetInviteSource(new HttpClient(new StubHandler(_ => new HttpResponseMessage(HttpStatusCode.ServiceUnavailable))));

        // Act & Assert
        Assert.Null(await source.GetInviteAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task GetInviteAsync_ReturnsNull_WhenTheRequestTimesOut()
    {
        // Arrange
        var source = new DiscordWidgetInviteSource(new HttpClient(new StubHandler(_ => throw new TaskCanceledException())));

        // Act & Assert
        Assert.Null(await source.GetInviteAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task GetInviteAsync_KeepsTheLastInvite_WhenALaterLookupFails()
    {
        // Arrange
        var fail = false;
        var handler = new StubHandler(_ => fail ? new HttpResponseMessage(HttpStatusCode.InternalServerError) : Json(Widget("https://discord.com/invite/abc123")));
        var time = new ManualTime();
        var source = new DiscordWidgetInviteSource(new HttpClient(handler), time);
        var cancellation = TestContext.Current.CancellationToken;

        // Act
        await source.GetInviteAsync(cancellation);
        fail = true;
        time.Now += TimeSpan.FromMinutes(10);
        var invite = await source.GetInviteAsync(cancellation);

        // Assert
        Assert.Equal("https://discord.com/invite/abc123", invite);
        Assert.Equal(2, handler.Requests.Count);
    }

    [Fact]
    public void Shared_IsAvailable_WithoutAnyRegistration()
    {
        Assert.NotNull(DiscordWidgetInviteSource.Shared);
        Assert.Same(DiscordWidgetInviteSource.Shared, DiscordWidgetInviteSource.Shared);
    }
}
