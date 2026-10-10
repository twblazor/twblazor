// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using TwBlazor.Configuration.Components;
using TwBlazor.Enums;
using TwBlazor.Utilities;

namespace TwBlazor.Components;

/// <summary>
/// Represents a single node in a <see cref="TwTreeList"/>: the actual row (chevron, checkbox, icon,
/// label) plus, when it has children, its own nested <c>&lt;ul role="group"&gt;</c>.
/// </summary>
/// <remarks>
/// Build a tree by nesting <c>&lt;TwTreeListItem&gt;</c> tags directly - inside a <see cref="TwTreeList"/>
/// for a root node, or inside another <c>TwTreeListItem</c> for a child - typically generated with your
/// own <c>@foreach</c> over whatever data you already have. Collapsed/checked state is tracked internally
/// (seeded once from the <see cref="Collapsed"/>/<see cref="Checked"/> parameters at first render, like an
/// uncontrolled input, since a parent cascading a check down to its children can't reach through a
/// one-way parameter) and every node keeps a list of the nested <c>TwTreeListItem</c>s that registered
/// with it via the cascaded parent reference, so checkbox aggregation and cascading see the whole subtree.
/// Every node also cascades a reference to the root <see cref="TwTreeList"/>, so a checkbox change
/// anywhere can trigger <see cref="TwTreeList.NotifyDescendantChangedAsync"/> - re-rendering the whole
/// tree so every ancestor's aggregated checkbox state (computed fresh from its descendants on every
/// render) reflects the change immediately.
/// </remarks>
public partial class TwTreeListItem : TwBlazorComponentBase
{
    private TwTreeListTheme theme => options.Theme.Components.Require<TwTreeListTheme>();

    [CascadingParameter] private TwTreeList? rootList { get; set; }

    [CascadingParameter] private TwTreeListItem? parentItem { get; set; }

    /// <summary>
    /// Gets or sets the display text for the node.
    /// </summary>
    [Parameter] public string Label { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets an arbitrary domain value for the node, for your own use (e.g. read back inside an
    /// <see cref="OnClick"/> closure) - not read or used by the component itself.
    /// </summary>
    [Parameter] public object? Value { get; set; }

    /// <summary>
    /// Gets or sets whether the node is disabled.
    /// </summary>
    [Parameter] public bool Disabled { get; set; }

    /// <summary>
    /// Gets or sets the node's icon override.
    /// </summary>
    [Parameter] public Icon? Icon { get; set; }

    /// <summary>
    /// Gets or sets whether this node's children start collapsed.
    /// </summary>
    [Parameter] public bool Collapsed { get; set; } = true;

    /// <summary>
    /// Gets or sets the callback invoked when <see cref="Collapsed"/> changes.
    /// </summary>
    [Parameter] public EventCallback<bool> CollapsedChanged { get; set; }

    /// <summary>
    /// Gets or sets the callback invoked when the node is clicked or activated via keyboard.
    /// </summary>
    [Parameter] public EventCallback OnClick { get; set; }

    /// <summary>
    /// Gets or sets whether the node is checked, when the owning <see cref="TwTreeList"/> has
    /// <c>ShowCheckboxes</c> enabled. Seeded once at first render, then self-managed - like an
    /// uncontrolled input - since a parent node cascading a check down to this one can't reach through
    /// a one-way parameter.
    /// </summary>
    [Parameter] public bool Checked { get; set; }

    /// <summary>
    /// Gets or sets the callback invoked when this node's checked state changes, whether from a direct
    /// click or cascaded down from an ancestor.
    /// </summary>
    [Parameter] public EventCallback<bool> CheckedChanged { get; set; }

    /// <summary>
    /// Gets or sets nested <c>TwTreeListItem</c> tags, for building this node's children.
    /// </summary>
    [Parameter] public RenderFragment? ChildContent { get; set; }

    private bool collapsed;
    private bool checkedState;
    private bool stateInitialized;
    private readonly List<TwTreeListItem> _children = [];

    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        if (!stateInitialized)
        {
            collapsed = Collapsed;
            checkedState = Checked;
            stateInitialized = true;
        }
    }

    protected override void OnInitialized() => parentItem?.RegisterChild(this);

    /// <summary>
    /// A node only exists as a component once Blazor constructs it as part of rendering its parent's
    /// own render tree - which happens after the parent's own render tree (including the <c>aria-checked</c>
    /// attribute computed from <see cref="GetEffectiveChecked"/>) has already been built, so a node with
    /// registered children needs one extra render pass to reflect their aggregated state on first paint.
    /// Deferred to here (fired once the render batch has committed) rather than triggered re-entrantly
    /// from <see cref="RegisterChild"/> during the child's own <c>OnInitialized</c> - calling
    /// <c>StateHasChanged</c> on an ancestor while still inside the render pass that's constructing that
    /// same child led Blazor to lose track of which child component instances were already registered,
    /// so a checkbox toggle's cascade silently mutated stale, no-longer-rendered instances.
    /// </summary>
    protected override void OnAfterRender(bool firstRender)
    {
        if (firstRender && _children.Count > 0)
        {
            StateHasChanged();
        }
    }

    /// <summary>
    /// Registers a nested child so this node's checkbox aggregation/cascade can see it.
    /// </summary>
    internal void RegisterChild(TwTreeListItem child) => _children.Add(child);

    private bool showCheckboxes => rootList?.ShowCheckboxes ?? false;

    private bool hideIcons => rootList?.HideIcons ?? false;

    private bool hasChildren => ChildContent != null;

    private string? ariaExpandedValue
    {
        get
        {
            if (!hasChildren)
            {
                return null;
            }

            return collapsed ? "false" : "true";
        }
    }

    private async Task OnItemActivatedAsync()
    {
        if (Disabled)
        {
            return;
        }

        if (hasChildren)
        {
            collapsed = !collapsed;
            if (CollapsedChanged.HasDelegate)
            {
                await CollapsedChanged.InvokeAsync(collapsed);
            }
        }

        if (OnClick.HasDelegate)
        {
            await OnClick.InvokeAsync();
        }
    }

    // Enter activates the item. Space checks it when the tree has checkboxes, and otherwise activates it too.
    // Right expands a collapsed item and Left collapses an expanded one; moving focus between items
    // (Up, Down, Home, End, and Left/Right once there is nothing to expand or collapse) is handled in script.
    private Task OnItemKeyDownAsync(KeyboardEventArgs e)
    {
        if (Disabled)
        {
            return Task.CompletedTask;
        }

        return e.Key switch
        {
            "Enter" => OnItemActivatedAsync(),
            " " => showCheckboxes ? ToggleCheckedAsync(GetEffectiveChecked() != true) : OnItemActivatedAsync(),
            "ArrowRight" when hasChildren && collapsed => SetCollapsedAsync(false),
            "ArrowLeft" when hasChildren && !collapsed => SetCollapsedAsync(true),
            _ => Task.CompletedTask
        };
    }

    private async Task SetCollapsedAsync(bool value)
    {
        collapsed = value;
        if (CollapsedChanged.HasDelegate)
        {
            await CollapsedChanged.InvokeAsync(collapsed);
        }
    }

    private async Task ToggleCheckedAsync(bool newValue)
    {
        if (Disabled)
        {
            return;
        }

        await SetCheckedRecursiveAsync(newValue);
        rootList?.NotifyDescendantChangedAsync();
    }

    private async Task SetCheckedRecursiveAsync(bool value)
    {
        checkedState = value;

        if (CheckedChanged.HasDelegate)
        {
            await CheckedChanged.InvokeAsync(value);
        }

        foreach (var child in _children)
        {
            await child.SetCheckedRecursiveAsync(value);
        }
    }

    /// <summary>
    /// Gets this node's effective checkbox state: its own checked value for a leaf, or the combined
    /// state of its children for a parent - <see langword="true"/>/<see langword="false"/> when they
    /// all agree, <see langword="null"/> (indeterminate) when they don't.
    /// </summary>
    private bool? GetEffectiveChecked()
    {
        if (_children.Count == 0)
        {
            return checkedState;
        }

        bool? aggregate = null;
        for (var i = 0; i < _children.Count; i++)
        {
            var childState = _children[i].GetEffectiveChecked();
            if (i == 0)
            {
                aggregate = childState;
                continue;
            }

            if (aggregate != childState)
            {
                return null;
            }
        }

        return aggregate;
    }

    private string GetAriaChecked() =>
        GetEffectiveChecked() switch
        {
            true => "true",
            false => "false",
            null => "mixed"
        };

    private string itemClasses =>
        new ClassBuilder("group")
            .AddClass(theme.ItemOutline)
            .AddClass(Class)
            .Build();

    private string rowClasses =>
        new ClassBuilder(theme.Row)
            .AddClass(theme.RowDisabled, Disabled)
            .Build();

    private string groupClasses =>
        new ClassBuilder(theme.Group)
            .AddClass(roundedBuilder.GetRounded(effectiveRounded))
            .Build();

    private string toggleIconClasses =>
        new ClassBuilder(theme.ToggleIcon)
            .AddClass(theme.ToggleIconOpen, !collapsed)
            .Build();

    /// <summary>
    /// Gets the icon to render for this node: its own override when set, otherwise a folder (open/closed
    /// matching its collapsed state) for a node with children, or a generic file icon for a leaf.
    /// </summary>
    private Icon effectiveIcon => Icon ?? GetDefaultIcon();

    private Icon GetDefaultIcon()
    {
        if (!hasChildren)
        {
            return Enums.Icon.File_Earmark;
        }

        return collapsed ? Enums.Icon.Folder : Enums.Icon.Folder2_Open;
    }

    /// <summary>
    /// Gets a stable identifier for this node, used as the rendered element's id. Falls back to a value
    /// derived from the component instance when <see cref="TwBlazorComponentBase.Id"/> is not supplied.
    /// </summary>
    private string effectiveItemId => Id ?? $"tree-item-{GetHashCode()}";
}
