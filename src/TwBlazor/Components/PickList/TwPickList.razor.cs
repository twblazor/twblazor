// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using TwBlazor.Configuration.Components;
using TwBlazor.Utilities;

namespace TwBlazor.Components;

/// <summary>
/// Represents a dual list-box ("pick list") that moves items between a source and a target
/// collection, with two-way binding on both lists plus optional up/down reordering within either one.
/// </summary>
/// <typeparam name="TItem">The type of items in both lists.</typeparam>
/// <remarks>
/// Selection is tracked internally by value equality (<see cref="EqualityComparer{TItem}"/>), the same
/// approach used by <see cref="TwCheckboxGroup{TValue}"/> - so items that compare equal are
/// selected/deselected together. <see cref="SelectedSourceItems"/> and <see cref="SelectedTargetItems"/>
/// only seed the initial selection (like <see cref="TwTreeListItem.Checked"/>) rather than staying
/// fully controlled, since re-syncing from the parameter on every render would immediately overwrite a
/// click with a caller that isn't also echoing the corresponding <c>Changed</c> callback back in.
/// </remarks>
public partial class TwPickList<TItem> : TwBlazorComponentBase
{
    private TwPickListTheme theme => options.Theme.Components.Require<TwPickListTheme>();

    /// <summary>
    /// Gets or sets the items available in the source (left) list.
    /// </summary>
    [Parameter] public IEnumerable<TItem> SourceItems { get; set; } = [];

    /// <summary>
    /// Gets or sets the callback invoked when items move into or out of the source list, or are
    /// reordered within it.
    /// </summary>
    [Parameter] public EventCallback<IEnumerable<TItem>> SourceItemsChanged { get; set; }

    /// <summary>
    /// Gets or sets the items in the target (right) list.
    /// </summary>
    [Parameter] public IEnumerable<TItem> TargetItems { get; set; } = [];

    /// <summary>
    /// Gets or sets the callback invoked when items move into or out of the target list, or are
    /// reordered within it.
    /// </summary>
    [Parameter] public EventCallback<IEnumerable<TItem>> TargetItemsChanged { get; set; }

    /// <summary>
    /// Gets or sets the source list's initially selected items. Only read once, at first render - see
    /// the remarks on <see cref="TwPickList{TItem}"/>.
    /// </summary>
    [Parameter] public IEnumerable<TItem> SelectedSourceItems { get; set; } = [];

    /// <summary>
    /// Gets or sets the callback invoked whenever the source list's selection changes.
    /// </summary>
    [Parameter] public EventCallback<IEnumerable<TItem>> SelectedSourceItemsChanged { get; set; }

    /// <summary>
    /// Gets or sets the target list's initially selected items. Only read once, at first render - see
    /// the remarks on <see cref="TwPickList{TItem}"/>.
    /// </summary>
    [Parameter] public IEnumerable<TItem> SelectedTargetItems { get; set; } = [];

    /// <summary>
    /// Gets or sets the callback invoked whenever the target list's selection changes.
    /// </summary>
    [Parameter] public EventCallback<IEnumerable<TItem>> SelectedTargetItemsChanged { get; set; }

    /// <summary>
    /// Gets or sets the name of the property to display for each item.
    /// </summary>
    /// <remarks>
    /// If null, empty, or not found on <typeparamref name="TItem"/>, the item's ToString() is used.
    /// </remarks>
    [Parameter] public string TextField { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the heading shown above the source list.
    /// </summary>
    [Parameter] public string SourceLabel { get; set; } = "Available";

    /// <summary>
    /// Gets or sets the heading shown above the target list.
    /// </summary>
    [Parameter] public string TargetLabel { get; set; } = "Selected";

    /// <summary>
    /// Gets or sets the text shown in place of a list that has no items.
    /// </summary>
    [Parameter] public string EmptyText { get; set; } = "No items";

    /// <summary>
    /// Gets or sets whether the whole component is disabled.
    /// </summary>
    [Parameter] public bool Disabled { get; set; }

    /// <summary>
    /// Gets or sets whether the "move all" (double chevron) buttons are shown alongside the
    /// single-transfer chevron buttons.
    /// </summary>
    [Parameter] public bool ShowMoveAllButtons { get; set; } = true;

    /// <summary>
    /// Gets or sets whether each column shows up/down buttons for reordering its current selection.
    /// </summary>
    [Parameter] public bool AllowReorder { get; set; } = true;

    private HashSet<TItem> selectedSource = [];
    private HashSet<TItem> selectedTarget = [];
    private bool selectionInitialized;

    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        if (!selectionInitialized)
        {
            selectedSource = [.. SelectedSourceItems];
            selectedTarget = [.. SelectedTargetItems];
            selectionInitialized = true;
        }
    }

    private string GetDisplayText(TItem item)
    {
        // Only a genuinely absent (null) item should render as empty - default(TItem) is a
        // legitimate, displayable item for non-nullable value types.
        if (item is null)
        {
            return string.Empty;
        }

        if (!string.IsNullOrWhiteSpace(TextField))
        {
            var property = item.GetType().GetProperty(TextField);
            if (property != null)
            {
                return property.GetValue(item)?.ToString() ?? string.Empty;
            }
        }

        return item.ToString() ?? string.Empty;
    }

    private bool IsSelected(TItem item, bool isSource) =>
        (isSource ? selectedSource : selectedTarget).Contains(item);

    private async Task ToggleSelectionAsync(TItem item, bool isSource)
    {
        if (Disabled)
        {
            return;
        }

        var selection = isSource ? selectedSource : selectedTarget;

        if (!selection.Remove(item))
        {
            selection.Add(item);
        }

        await RaiseSelectionChangedAsync(isSource);
    }

    private Task OnItemKeyDownAsync(KeyboardEventArgs e, TItem item, bool isSource) =>
        e.Key is "Enter" or " " ? ToggleSelectionAsync(item, isSource) : Task.CompletedTask;

    private Task RaiseSelectionChangedAsync(bool isSource) => isSource
        ? SelectedSourceItemsChanged.InvokeAsync([.. selectedSource])
        : SelectedTargetItemsChanged.InvokeAsync([.. selectedTarget]);

    /// <summary>
    /// Moves the current selection from one list to the other, appending the moved items to the end
    /// of the destination list.
    /// </summary>
    private async Task TransferSelectedAsync(bool fromSource)
    {
        if (Disabled)
        {
            return;
        }

        var selection = fromSource ? selectedSource : selectedTarget;
        if (selection.Count == 0)
        {
            return;
        }

        var origin = (fromSource ? SourceItems : TargetItems).ToList();
        var moving = origin.Where(selection.Contains).ToList();
        if (moving.Count == 0)
        {
            return;
        }

        origin.RemoveAll(selection.Contains);
        var destination = (fromSource ? TargetItems : SourceItems).ToList();
        destination.AddRange(moving);
        selection.Clear();

        await ApplyTransferAsync(fromSource, origin, destination);
    }

    /// <summary>
    /// Moves every item from one list to the other, regardless of selection.
    /// </summary>
    private async Task TransferAllAsync(bool fromSource)
    {
        if (Disabled)
        {
            return;
        }

        var origin = (fromSource ? SourceItems : TargetItems).ToList();
        if (origin.Count == 0)
        {
            return;
        }

        var destination = (fromSource ? TargetItems : SourceItems).ToList();
        destination.AddRange(origin);
        origin.Clear();
        (fromSource ? selectedSource : selectedTarget).Clear();

        await ApplyTransferAsync(fromSource, origin, destination);
    }

    private async Task ApplyTransferAsync(bool fromSource, List<TItem> newOrigin, List<TItem> newDestination)
    {
        if (fromSource)
        {
            SourceItems = newOrigin;
            TargetItems = newDestination;
        }
        else
        {
            TargetItems = newOrigin;
            SourceItems = newDestination;
        }

        await SourceItemsChanged.InvokeAsync(SourceItems);
        await TargetItemsChanged.InvokeAsync(TargetItems);
        await RaiseSelectionChangedAsync(fromSource);
    }

    /// <summary>
    /// Shifts every selected item in a list one position up or down, preserving the relative order of
    /// a multi-item selection (each selected item swaps with its nearest unselected neighbor in the
    /// move direction, so a contiguous block of selected items moves together as a unit).
    /// </summary>
    private async Task MoveSelectionAsync(bool isSource, bool up)
    {
        if (Disabled || !AllowReorder)
        {
            return;
        }

        var selection = isSource ? selectedSource : selectedTarget;
        if (selection.Count == 0)
        {
            return;
        }

        var list = (isSource ? SourceItems : TargetItems).ToList();
        ShiftSelected(list, selection, up);

        if (isSource)
        {
            SourceItems = list;
            await SourceItemsChanged.InvokeAsync(SourceItems);
        }
        else
        {
            TargetItems = list;
            await TargetItemsChanged.InvokeAsync(TargetItems);
        }
    }

    /// <summary>
    /// Swaps each selected item in place with its nearest unselected neighbor in the move direction.
    /// Walking away from the direction of travel lets a contiguous block of selected items move together.
    /// </summary>
    private static void ShiftSelected(List<TItem> list, HashSet<TItem> selection, bool up)
    {
        var step = up ? 1 : -1;
        var start = up ? 1 : list.Count - 2;

        for (var i = start; i >= 0 && i < list.Count; i += step)
        {
            var neighbor = i - step;
            if (selection.Contains(list[i]) && !selection.Contains(list[neighbor]))
            {
                (list[neighbor], list[i]) = (list[i], list[neighbor]);
            }
        }
    }

    private string containerClasses => new ClassBuilder(theme.Container).AddClass(Class).Build();

    private string GetListBoxClasses() => new ClassBuilder(theme.ListBox)
        .AddClass(theme.ListBoxDisabled, Disabled)
        .AddClass(roundedBuilder.GetRounded(effectiveRounded))
        .Build();

    private string GetItemClasses(TItem item, bool isSource) => new ClassBuilder(theme.Item)
        .AddClass(theme.ItemSelected, IsSelected(item, isSource))
        .Build();
}
