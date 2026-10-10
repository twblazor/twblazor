// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using TwBlazor.Configuration.Components;
using TwBlazor.Utilities;

namespace TwBlazor.Components;

/// <summary>
/// Root of a tree of expandable/selectable items: renders the outer <c>&lt;ul role="tree"&gt;</c> and
/// hosts nested <see cref="TwTreeListItem"/> nodes, given as <see cref="ChildContent"/> - typically
/// generated with your own <c>@foreach</c> over whatever data you already have.
/// </summary>
/// <remarks>
/// Every <see cref="TwTreeListItem"/> in the tree - regardless of nesting depth - cascades a reference
/// to this root instance, so a checkbox toggle anywhere can call back into
/// <see cref="NotifyDescendantChangedAsync"/> to re-render the whole tree (needed for ancestors to
/// reflect an indeterminate state once their descendants disagree).
/// </remarks>
public partial class TwTreeList : TwBlazorComponentBase
{
    private TwTreeListTheme theme => options.Theme.Components.Require<TwTreeListTheme>();

    /// <summary>
    /// Gets or sets the root-level <see cref="TwTreeListItem"/> nodes.
    /// </summary>
    [Parameter] public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Gets or sets whether a checkbox is rendered next to each node. Checking or unchecking a node
    /// cascades to all of its descendants; a parent whose descendants disagree renders indeterminate.
    /// </summary>
    [Parameter] public bool ShowCheckboxes { get; set; }

    /// <summary>
    /// Gets or sets whether each node's leading icon is hidden. Icons are shown by default - a folder
    /// for a node with children, a generic file icon for a leaf - unless a node's own <c>Icon</c>
    /// overrides it.
    /// </summary>
    [Parameter] public bool HideIcons { get; set; }

    /// <summary>
    /// Re-renders the whole tree. Called by a <see cref="TwTreeListItem"/> anywhere in the tree after
    /// its checked state changes, so every ancestor's indeterminate/checked display - computed fresh
    /// from its descendants on every render - reflects the change immediately.
    /// </summary>
    internal void NotifyDescendantChangedAsync() => StateHasChanged();

    private ElementReference rootRef;

    private const string itemSelector = "[role=\"treeitem\"]";

    [Inject] private IJSRuntime jsRuntime { get; set; } = null!;

    /// <summary>
    /// Makes the tree a single Tab stop whose items are reached with the arrow keys, Home and End. Moving
    /// focus is done in script, since a round trip per key press would lag.
    /// </summary>
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);

        try
        {
            await jsRuntime.InvokeVoidAsync("twRoving.attach", rootRef, itemSelector, "tree");
        }
        catch (JSDisconnectedException)
        {
            // The circuit disconnected before the script could run; nothing to wire up.
        }
        catch (InvalidOperationException)
        {
            // JS interop is not available (prerendering); the next interactive render wires it up.
        }
    }

    private string rootClasses =>
        new ClassBuilder(theme.Container)
            .AddClass(Class)
            .Build();
}
