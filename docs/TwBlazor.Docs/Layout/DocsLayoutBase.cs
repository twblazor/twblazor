using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using TwBlazor.Enums;

namespace TwBlazor.Docs.Layout;

/// <summary>
/// The base of the docs layouts. Holds the reader's direction choice, wraps the page in a
/// <c>TwDirectionProvider</c> set from it. The choice is kept in page memory, so it carries across pages (which
/// swap the layout) but a fresh load always follows the language.
/// </summary>
public abstract class DocsLayoutBase : LayoutComponentBase, IDisposable
{
    private readonly CancellationTokenSource _cts = new();

    [Inject] private IJSRuntime js { get; set; } = null!;

    protected DocsDirectionState directionState { get; }

    protected DocsLayoutBase() => directionState = new DocsDirectionState(SetDirectionAsync);

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender)
            return;

        try
        {
            var saved = Parse(await js.InvokeAsync<string?>("docsLocale.getDirection", _cts.Token));
            if (saved != directionState.Direction)
            {
                directionState.Direction = saved;
                StateHasChanged();
            }
        }
        catch (Exception ex) when (ex is JSException or JSDisconnectedException or OperationCanceledException or InvalidOperationException)
        {
            // No script (tests, prerendering) or the circuit went away: keep following the language.
        }
    }

    private async Task SetDirectionAsync(TextDirection? direction)
    {
        directionState.Direction = direction;
        StateHasChanged();

        try
        {
            await js.InvokeVoidAsync("docsLocale.setDirection", _cts.Token, direction switch
            {
                TextDirection.RightToLeft => "rtl",
                TextDirection.LeftToRight => "ltr",
                _ => null,
            });
        }
        catch (Exception ex) when (ex is JSException or JSDisconnectedException or OperationCanceledException)
        {
            // Saving is a convenience; the switch has already been applied.
        }
    }

    private static TextDirection? Parse(string? value) => value switch
    {
        "rtl" => TextDirection.RightToLeft,
        "ltr" => TextDirection.LeftToRight,
        _ => null,
    };

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            _cts.Cancel();
            _cts.Dispose();
        }
    }
}
