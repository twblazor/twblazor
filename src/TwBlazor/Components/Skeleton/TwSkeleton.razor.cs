// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Globalization;
using TwBlazor.Configuration.Components;
using TwBlazor.Enums;
using TwBlazor.Localization;
using TwBlazor.Models;
using TwBlazor.Utilities;

namespace TwBlazor.Components;

/// <summary>
/// Renders a loading placeholder. With no <see cref="ChildContent"/> it renders a single static shape
/// (<see cref="SkeletonType"/>/<see cref="Width"/>/<see cref="Height"/>). 
/// With <see cref="ChildContent"/> supplied, it instead measures the real rendered
/// layout of that content (via the <c>twSkeleton.observe</c> JS interop call) and generates matching
/// placeholder blocks automatically, so wrapping any markup in <c>&lt;TwSkeleton Loading="..."&gt;</c>
/// produces a skeleton shaped like the content it hides.
/// </summary>
public partial class TwSkeleton : TwBlazorComponentBase, IAsyncDisposable
{
    [Inject] private IJSRuntime jSRuntime { get; set; } = null!;

    private TwSkeletonTheme theme => options.Theme.Components.Require<TwSkeletonTheme>();

    private ElementReference measureRef;
    private DotNetObjectReference<TwSkeleton>? selfReference;
    private bool isObserving;
    private List<SkeletonRect> measuredRects = [];

    /// <summary>
    /// Gets or sets whether the placeholder (or, with <see cref="ChildContent"/> supplied, a generated
    /// skeleton shaped like it) is shown instead of the real content. Default is <c>true</c>.
    /// </summary>
    [Parameter] public bool Loading { get; set; } = true;

    /// <summary>
    /// Gets or sets the placeholder shape used when no <see cref="ChildContent"/> is supplied.
    /// Default is <see cref="SkeletonType.Text"/>.
    /// </summary>
    [Parameter] public SkeletonType SkeletonType { get; set; } = SkeletonType.Text;

    /// <summary>
    /// Gets or sets the loading animation played by every placeholder block this component renders.
    /// Default is <see cref="SkeletonAnimation.Pulse"/>.
    /// </summary>
    [Parameter] public SkeletonAnimation Animation { get; set; } = SkeletonAnimation.Pulse;

    /// <summary>
    /// Gets or sets a CSS width (e.g. <c>"50px"</c>, <c>"100%"</c>) for the standalone placeholder shape
    /// shown when no <see cref="ChildContent"/> is supplied. Ignored once <see cref="ChildContent"/> is set,
    /// since generated placeholders are sized from the content's own measured layout instead.
    /// </summary>
    [Parameter] public string? Width { get; set; }

    /// <summary>
    /// Gets or sets a CSS height for the standalone placeholder shape. See <see cref="Width"/>.
    /// </summary>
    [Parameter] public string? Height { get; set; }

    /// <summary>
    /// Gets or sets the real content to show once <see cref="Loading"/> is <c>false</c>. When supplied,
    /// this component ignores <see cref="SkeletonType"/>/<see cref="Width"/>/<see cref="Height"/> and
    /// instead generates placeholder blocks that mirror this content's own measured layout.
    /// </summary>
    [Parameter] public RenderFragment? ChildContent { get; set; }

    private bool hasChildContent => ChildContent is not null;

    private bool isStandalonePlaceholder => Loading && !hasChildContent;

    private bool shouldMeasure => Loading && hasChildContent;

    private string animationClass => Animation switch
    {
        SkeletonAnimation.Pulse => theme.Pulse,
        SkeletonAnimation.Wave => theme.Wave,
        _ => string.Empty
    };

    private string standaloneVisualClasses => new ClassBuilder(theme.Base)
        .AddClass(animationClass)
        .AddClass(theme.Text, SkeletonType == SkeletonType.Text)
        .AddClass(theme.Rectangle, SkeletonType == SkeletonType.Rectangle)
        .AddClass(theme.Circle, SkeletonType == SkeletonType.Circle)
        .AddClass(options.Theme.Rounded.Full, SkeletonType == SkeletonType.Circle)
        .AddClass(roundedBuilder.GetRounded(effectiveRounded), SkeletonType != SkeletonType.Circle)
        .Build();

    private string rootClasses => new ClassBuilder(isStandalonePlaceholder ? standaloneVisualClasses : options.Theme.Position.Relative)
        .AddClass(Class)
        .Build();

    private string rootStyle
    {
        get
        {
            if (!isStandalonePlaceholder)
            {
                return Style ?? string.Empty;
            }

            var parts = new List<string>();
            if (!string.IsNullOrWhiteSpace(Width))
            {
                parts.Add($"width:{Width}");
            }

            if (!string.IsNullOrWhiteSpace(Height))
            {
                parts.Add($"height:{Height}");
            }

            if (!string.IsNullOrWhiteSpace(Style))
            {
                parts.Add(Style);
            }

            return string.Join(";", parts);
        }
    }

    /// <summary>
    /// Falls back to an accessible "Loading" label while <see cref="Loading"/> is true and the consumer
    /// hasn't supplied their own <see cref="TwBlazorComponentBase.AriaLabel"/> or
    /// <see cref="TwBlazorComponentBase.AriaLabelledBy"/>, so the busy region never announces with no name.
    /// </summary>
    private string? effectiveAriaLabel => Loading && string.IsNullOrWhiteSpace(AriaLabel) && string.IsNullOrWhiteSpace(AriaLabelledBy)
        ? localizer[TwStrings.Common_Loading]
        : AriaLabel;

    private string RectClasses(SkeletonRect rect) => new ClassBuilder(theme.Base)
        .AddClass(animationClass)
        .AddClass(options.Theme.Rounded.Full, rect.Shape is "circle" or "text")
        .AddClass(roundedBuilder.GetRounded(effectiveRounded), rect.Shape == "rect")
        .Build();

    private static string RectStyle(SkeletonRect rect) => string.Create(CultureInfo.InvariantCulture,
        $"position:absolute;top:{rect.Top}px;left:{rect.Left}px;width:{rect.Width}px;height:{rect.Height}px;");

    /// <inheritdoc />
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (shouldMeasure)
        {
            await StartObservingAsync();
        }
        else if (isObserving)
        {
            await StopObservingAsync();
        }
    }

    private async Task StartObservingAsync()
    {
        if (isObserving)
        {
            return;
        }

        try
        {
            selfReference ??= DotNetObjectReference.Create(this);
            await jSRuntime.InvokeVoidAsync("twSkeleton.observe", measureRef, selfReference);
            isObserving = true;
        }
        catch (JSDisconnectedException)
        {
            // The circuit disconnected before the script could run; nothing to observe.
        }
    }

    private async Task StopObservingAsync()
    {
        isObserving = false;
        measuredRects = [];

        try
        {
            await jSRuntime.InvokeVoidAsync("twSkeleton.unobserve", measureRef);
        }
        catch (JSDisconnectedException)
        {
            // The circuit is already gone; nothing left to clean up.
        }
        catch (InvalidOperationException)
        {
            // JS interop unavailable during teardown (e.g. prerendering); safe to ignore.
        }
    }

    /// <summary>
    /// Invoked from JS (<c>twSkeleton.observe</c>'s <c>ResizeObserver</c> callback) whenever the measured
    /// <see cref="ChildContent"/> layout is (re)computed - once for the initial measurement, and again
    /// after every subsequent resize while this component keeps observing it.
    /// </summary>
    [JSInvokable]
    public void OnRectsMeasured(List<SkeletonRect> rects)
    {
        measuredRects = rects;
        StateHasChanged();
    }

    /// <summary>
    /// Stops observing the measured content's layout and releases the JS interop reference.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        if (isObserving)
        {
            try
            {
                await jSRuntime.InvokeVoidAsync("twSkeleton.unobserve", measureRef);
            }
            catch (JSDisconnectedException)
            {
                // The circuit is already gone; nothing left to clean up.
            }
            catch (InvalidOperationException)
            {
                // JS interop unavailable during teardown (e.g. prerendering); safe to ignore.
            }
        }

        selfReference?.Dispose();
        GC.SuppressFinalize(this);
    }
}
