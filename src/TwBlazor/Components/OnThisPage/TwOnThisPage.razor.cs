// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using TwBlazor.Configuration.Components;
using TwBlazor.Localization;
using TwBlazor.Models;
using TwBlazor.Utilities;

namespace TwBlazor.Components;

/// <summary>
/// An "On this page" navigation list: a link to each section of the page, with the section currently being
/// read highlighted as the page scrolls.
/// </summary>
/// <remarks>
/// The component renders a <c>nav</c> landmark named by its heading. The link to the section being read is
/// marked with <c>aria-current="location"</c>, so the highlight is announced by screen readers as well as
/// shown. While <see cref="Items"/> is empty the landmark is hidden, so an empty navigation is never announced.
/// </remarks>
public partial class TwOnThisPage : TwBlazorComponentBase, IAsyncDisposable
{
    [Inject] private IJSRuntime jSRuntime { get; set; } = null!;

    private TwOnThisPageTheme theme => options.Theme.Components.Require<TwOnThisPageTheme>();

    /// <summary>
    /// Gets or sets the sections to list, in the order they appear on the page. Each
    /// <see cref="OnThisPageItem.Id"/> must be the DOM id of an element on the page.
    /// </summary>
    [Parameter] public IReadOnlyList<OnThisPageItem> Items { get; set; } = [];

    /// <summary>
    /// Gets or sets the heading shown above the links. Defaults to the localized "On this page".
    /// </summary>
    [Parameter] public string? Title { get; set; }

    /// <summary>
    /// Gets or sets whether the visible heading is left out. The navigation landmark is then named by
    /// <see cref="Title"/> through <c>aria-label</c> instead, so it is still announced.
    /// </summary>
    [Parameter] public bool HideTitle { get; set; }

    /// <summary>
    /// Gets or sets the heading level (1 to 6) of the title. Defaults to 2. Pick the level that fits the
    /// page's heading outline.
    /// </summary>
    [Parameter] public int HeadingLevel { get; set; } = 2;

    /// <summary>
    /// Gets or sets the path each link is relative to, e.g. <c>/docs/card</c>, giving links such as
    /// <c>/docs/card#usage</c>.
    /// </summary>
    /// <remarks>
    /// Set this to the current page's path in an app with a <c>&lt;base href&gt;</c> (every Blazor app has
    /// one), where a bare <c>#usage</c> link would otherwise resolve against the site root instead of the
    /// current page.
    /// </remarks>
    [Parameter] public string BasePath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the distance in pixels from the top of the viewport at which a section counts as the one
    /// being read. Raise it when a fixed header covers the top of the page. Defaults to 140.
    /// </summary>
    [Parameter] public int Offset { get; set; } = 140;

    private ElementReference navElement;

    private string observedKey = string.Empty;

    private bool observing;

    private string headingId => $"{Id}-heading";

    private string effectiveTitle => Title ?? localizer[TwStrings.OnThisPage_Title];

    private bool hasHeading => !HideTitle && Items.Count > 0;

    private string? navLabel => AriaLabel ?? (!hasHeading && AriaLabelledBy is null ? effectiveTitle : null);

    private string? navLabelledBy => AriaLabelledBy ?? (AriaLabel is null && hasHeading ? headingId : null);

    private string classes => new ClassBuilder(options.Theme.Typography.Size.Sm)
        .AddClass(Class)
        .Build();

    private string linkClasses => new ClassBuilder(theme.Link)
        .AddClass(theme.LinkCurrent)
        .Build();

    private RenderFragment heading => builder =>
    {
        builder.OpenElement(0, $"h{Math.Clamp(HeadingLevel, 1, 6)}");
        builder.AddAttribute(1, "id", headingId);
        builder.AddAttribute(2, "class", theme.Title);
        builder.AddContent(3, effectiveTitle);
        builder.CloseElement();
    };

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        var ids = Items.Select(item => item.Id).ToArray();
        var key = $"{Offset}|{string.Join('|', ids)}";

        if (key == observedKey)
        {
            return;
        }

        observedKey = key;

        try
        {
            if (ids.Length == 0)
            {
                await StopObservingAsync();
                return;
            }

            await jSRuntime.InvokeVoidAsync("twOnThisPage.observe", Id, navElement, ids, Offset);
            observing = true;
        }
        catch (JSDisconnectedException)
        {
            // The circuit closed while the page was still rendering, so there is nothing left to highlight.
        }
        catch (TaskCanceledException)
        {
            // Navigation cancelled the call before it ran.
        }
    }

    private async Task StopObservingAsync()
    {
        if (!observing)
        {
            return;
        }

        observing = false;
        await jSRuntime.InvokeVoidAsync("twOnThisPage.dispose", Id);
    }

    /// <summary>
    /// Removes the scroll listener registered for this component.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        try
        {
            await StopObservingAsync();
        }
        catch (JSDisconnectedException)
        {
            // The circuit is already gone, so the listener went with the page.
        }
        catch (InvalidOperationException)
        {
            // JS interop unavailable during teardown (e.g. prerendering); safe to ignore.
        }
        finally
        {
            GC.SuppressFinalize(this);
        }
    }
}
