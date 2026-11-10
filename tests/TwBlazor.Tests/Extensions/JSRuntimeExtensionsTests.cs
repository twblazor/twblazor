using Microsoft.JSInterop;
using TwBlazor.Extensions;

namespace TwBlazor.Tests.Extensions;

public class JSRuntimeExtensionsTests
{
    /// <summary>
    /// A JS runtime that records each call and either returns <see cref="Result"/> or throws <see cref="Failure"/>.
    /// </summary>
    private sealed class FakeJSRuntime : IJSRuntime
    {
        public Exception? Failure { get; init; }

        public object? Result { get; init; }

        public List<(string Identifier, object?[]? Args)> Calls { get; } = [];

        // Forwards to the overload below inside this fake runtime, so there is no real interop call to guard.
#pragma warning disable BL0016
        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args) =>
            InvokeAsync<TValue>(identifier, CancellationToken.None, args);
#pragma warning restore BL0016

        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken, object?[]? args)
        {
            Calls.Add((identifier, args));

            return Failure is null
                ? ValueTask.FromResult((TValue)Result!)
                : ValueTask.FromException<TValue>(Failure);
        }
    }

    [Fact]
    public async Task TryInvokeVoidAsync_CallsTheFunction_WithItsArguments()
    {
        var js = new FakeJSRuntime();

        var ran = await js.TryInvokeVoidAsync("twDialog.focusPanel", "panel", 3);

        Assert.True(ran);
        var call = Assert.Single(js.Calls);
        Assert.Equal("twDialog.focusPanel", call.Identifier);
        Assert.Equal(["panel", 3], call.Args!);
    }

    [Fact]
    public async Task TryInvokeVoidAsync_SkipsQuietly_WhenTheCircuitHasDisconnected()
    {
        var js = new FakeJSRuntime { Failure = new JSDisconnectedException("gone") };

        Assert.False(await js.TryInvokeVoidAsync("twDialog.focusPanel"));
    }

    [Fact]
    public async Task TryInvokeVoidAsync_SkipsQuietly_WhilePrerendering()
    {
        var js = new FakeJSRuntime { Failure = new InvalidOperationException("JavaScript interop calls cannot be issued at this time.") };

        Assert.False(await js.TryInvokeVoidAsync("twDialog.focusPanel"));
    }

    [Fact]
    public async Task TryInvokeVoidAsync_SkipsQuietly_WhenTheCircuitIsTornDown()
    {
        var js = new FakeJSRuntime { Failure = new TaskCanceledException() };

        Assert.False(await js.TryInvokeVoidAsync("twPicker.unregisterOutsideClick"));
    }

    [Fact]
    public async Task TryInvokeVoidAsync_LetsScriptErrorsThrough()
    {
        var js = new FakeJSRuntime { Failure = new JSException("twDialog is not defined") };

        await Assert.ThrowsAsync<JSException>(async () => await js.TryInvokeVoidAsync("twDialog.focusPanel"));
    }

    [Fact]
    public async Task TryInvokeAsync_ReturnsTheResult_WhenTheCallRuns()
    {
        var js = new FakeJSRuntime { Result = 42 };

        Assert.Equal(42, await js.TryInvokeAsync("twSpreadsheet.fitColumn", 0, "grid", 1));
        Assert.Equal(["grid", 1], Assert.Single(js.Calls).Args!);
    }

    [Fact]
    public async Task TryInvokeAsync_ReturnsTheFallback_WhenTheCircuitHasDisconnected()
    {
        var js = new FakeJSRuntime { Failure = new JSDisconnectedException("gone") };

        Assert.Null(await js.TryInvokeAsync<string?>("twDialog.captureFocus", null));
    }

    [Fact]
    public async Task TryInvokeAsync_ReturnsTheFallback_WhilePrerendering()
    {
        var js = new FakeJSRuntime { Failure = new InvalidOperationException("JavaScript interop calls cannot be issued at this time.") };

        double[] fallback = [1.0, 2.0];

        var size = await js.TryInvokeAsync("twColorPicker.getSize", fallback);

        Assert.Same(fallback, size);
    }

    [Fact]
    public async Task TryInvokeAsync_ReturnsTheFallback_WhenTheCircuitIsTornDown()
    {
        var js = new FakeJSRuntime { Failure = new TaskCanceledException() };

        Assert.False(await js.TryInvokeAsync("twSidebar.isMobileViewport", false));
    }

    [Fact]
    public async Task TryInvokeAsync_LetsScriptErrorsThrough()
    {
        var js = new FakeJSRuntime { Failure = new JSException("twColorPicker is not defined") };

        await Assert.ThrowsAsync<JSException>(async () => await js.TryInvokeAsync("twColorPicker.supportsEyeDropper", false));
    }
}
