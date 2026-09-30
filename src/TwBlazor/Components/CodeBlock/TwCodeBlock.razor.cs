// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Diagnostics;
using TwBlazor.Configuration.Components;
using TwBlazor.Utilities;

namespace TwBlazor.Components;

public partial class TwCodeBlock : TwBlazorComponentBase, IAsyncDisposable
{
    private TwCodeBlockTheme theme => options.Theme.Components.Require<TwCodeBlockTheme>();

    private CancellationTokenSource? cancellationTokenSource;

    [Inject] private IJSRuntime jSRuntime { get; set; } = null!;

    private bool copied { get; set; }

    private string? statusMessage { get; set; }

    /// <summary>
    /// Flips on every <see cref="SetStatusMessage"/> call and is appended to <see cref="statusMessage"/>
    /// as an invisible marker, so the rendered text always genuinely differs from what it replaces.
    /// </summary>
    private bool statusMessageMarker;

    [Parameter] public bool Inline { get; set; }
    [Parameter] public string? Content { get; set; }
    [Parameter] public string Language { get; set; } = "html";

    /// <summary>
    /// Gets or sets the label shown in the header bar. Defaults to a display name for <see cref="Language"/>
    /// (e.g. <c>csharp</c> shows "C#", <c>bash</c> shows "Terminal").
    /// </summary>
    [Parameter] public string? Title { get; set; }

    private static readonly Dictionary<string, string> _languageTitles = new(StringComparer.OrdinalIgnoreCase)
    {
        ["html"] = "Razor",
        ["csharp"] = "C#",
        ["cs"] = "C#",
        ["css"] = "CSS",
        ["bash"] = "Terminal",
        ["shell"] = "Terminal",
        ["json"] = "JSON",
        ["xml"] = "XML",
        ["javascript"] = "JavaScript",
        ["js"] = "JavaScript",
        ["text"] = "Text"
    };

    private string headerTitle => Title
        ?? (_languageTitles.TryGetValue(Language, out var title) ? title : Language.ToUpperInvariant());

    private string headerClasses =>
        new ClassBuilder(theme.Header)
        .AddClass(options.Theme.Display.Flex)
        .AddClass(options.Theme.Flexbox.Align.Center)
        .AddClass(options.Theme.Flexbox.Justify.Between)
        .Build();

    private string classes =>
        new ClassBuilder(Class)
        .AddClass(shadowBuilder.GetShadow(effectiveShadow))
        .AddClass(roundedBuilder.GetRounded(effectiveRounded))
        .AddClass(options.Theme.Position.Relative)
        .AddClass(options.Theme.Display.Flex)
        .AddClass(options.Theme.Flexbox.Col)
        .AddClass(theme.Container).Build();

    private ElementReference codeBlock { get; set; }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender)
            return;

        cancellationTokenSource ??= new CancellationTokenSource();

        try
        {
            await jSRuntime.InvokeVoidAsync("twCodeBlock.highlightElement", cancellationTokenSource.Token, codeBlock);
        }
        catch (TaskCanceledException)
        {
            // Highlight operation was cancelled due to component disposal or navigation.
        }
        catch (JSException)
        {
            // Suppress JS interop errors during unit tests (bUnit) or when JS runtime is unavailable.
        }
        catch (InvalidOperationException)
        {
            // Suppress JS interop errors during unit tests (bUnit) or when JS runtime is unavailable.
        }
    }

    private async Task Copy()
    {
        cancellationTokenSource ??= new CancellationTokenSource();

        try
        {
            // Write to clipboard immediately — document is still focused from the user's click
            await jSRuntime.InvokeVoidAsync("navigator.clipboard.writeText", cancellationTokenSource.Token, Content);

            copied = true;
            SetStatusMessage("Copied to clipboard");

            await Task.Delay(1000, cancellationTokenSource.Token);

            copied = false;
            StateHasChanged();
        }
        catch (TaskCanceledException)
        {
            // Cancellation is expected when the component is disposed or the operation is otherwise aborted.
            Debug.WriteLine("Copy operation was canceled.", nameof(TwCodeBlock));
        }
        catch (JSException ex)
        {
            // Clipboard write failed (e.g. permission denied, unavailable in unit tests/JS runtime) - surface it via the live region instead of swallowing it.
            Debug.WriteLine(ex, nameof(TwCodeBlock));
            copied = false;
            SetStatusMessage("Copy failed");
        }
        catch (InvalidOperationException ex)
        {
            // JS interop unavailable (e.g. during prerendering or unit tests) - surface it via the live region instead of swallowing it.
            Debug.WriteLine(ex, nameof(TwCodeBlock));
            copied = false;
            SetStatusMessage("Copy failed");
        }
    }

    /// <summary>
    /// Sets the <c>aria-live</c> status message announced to assistive technology.
    /// </summary>
    /// <remarks>
    /// Blazor only mutates the live region's DOM text node when the rendered string actually changes,
    /// so setting the same message twice in a row (e.g. two consecutive failed copy attempts) would
    /// otherwise produce no DOM mutation and go unannounced. A trailing zero-width space is toggled on
    /// and off on every call so the rendered text always genuinely changes - and therefore is always
    /// re-announced - even when the visible message text is identical to the previous one.
    /// </remarks>
    private void SetStatusMessage(string message)
    {
        // Zero-width space (U+200B), expressed as a char literal rather than embedded directly in a
        // string literal so its codepoint is unambiguous in source.
        const char zeroWidthSpace = (char)0x200B;

        statusMessageMarker = !statusMessageMarker;
        statusMessage = statusMessageMarker ? message + zeroWidthSpace : message;
        StateHasChanged();
    }

    public async ValueTask DisposeAsync()
    {
        if (cancellationTokenSource is not null)
        {
            await cancellationTokenSource.CancelAsync();
            cancellationTokenSource.Dispose();
        }

        GC.SuppressFinalize(this);
    }
}
