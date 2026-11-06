// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using TwBlazor.Enums;
using Color = TwBlazor.Enums.Color;

namespace TwBlazor.Components;

/// <summary>
/// One option of a <see cref="TwToggleGroup{T}"/>, rendered as a <see cref="TwToggle"/>.
/// </summary>
/// <typeparam name="T">The type of the value the item stands for; inferred from the enclosing group.</typeparam>
/// <remarks>
/// The item takes its variant, size, rounding and pressed state from the group. An item that shows only an
/// icon needs an <see cref="TwBlazorComponentBase.AriaLabel"/>.
/// </remarks>
public partial class TwToggleGroupItem<T> : TwBlazorComponentBase, IDisposable
{
    [CascadingParameter] private TwToggleGroup<T>? cascadedGroup { get; set; }

    private TwToggleGroup<T> group => cascadedGroup
        ?? throw new InvalidOperationException($"{nameof(TwToggleGroupItem<T>)} must be placed inside a {nameof(TwToggleGroup<T>)}.");

    private TwToggle? toggle;

    /// <summary>
    /// Gets or sets the value the item stands for. The item is pressed while the group's value (or one of its
    /// values) equals it.
    /// </summary>
    [Parameter, EditorRequired] public T Value { get; set; } = default!;

    /// <summary>
    /// Gets or sets the text shown in the item. Ignored when <see cref="ChildContent"/> is set.
    /// </summary>
    [Parameter] public string Label { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the icon shown before the label, or on its own for an icon-only item.
    /// </summary>
    [Parameter] public Icon? Icon { get; set; }

    /// <summary>
    /// Gets or sets custom content shown in the item in place of <see cref="Label"/>.
    /// </summary>
    [Parameter] public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Gets or sets whether this item is disabled. Disabled items are skipped by the arrow keys.
    /// </summary>
    [Parameter] public bool Disabled { get; set; }

    /// <summary>
    /// Gets or sets the color of this item while it is pressed, in place of the group's <c>Color</c>.
    /// </summary>
    [Parameter] public Color? Color { get; set; }

    /// <summary>
    /// Gets or sets the text of a tooltip shown when the item is hovered or keyboard-focused.
    /// </summary>
    [Parameter] public string? Tooltip { get; set; }

    /// <summary>
    /// The attributes handed to the toggle: the consumer's own, plus the roving tabindex that makes the group a
    /// single Tab stop and a focus listener that tells the group which item has focus.
    /// </summary>
    private Dictionary<string, object> GetToggleAttributes() => new(Attributes)
    {
        ["tabindex"] = group.IsTabStop(this) ? "0" : "-1",
        ["onfocus"] = EventCallback.Factory.Create<FocusEventArgs>(this, () => group.OnItemFocused(this)),
    };

    /// <inheritdoc />
    protected override void OnInitialized()
    {
        base.OnInitialized();
        group.Register(this);
    }

    /// <summary>
    /// Moves keyboard focus to the item.
    /// </summary>
    internal Task FocusAsync() => toggle?.FocusAsync() ?? Task.CompletedTask;

    private Task OnPressedChangedAsync(bool pressed) => group.OnItemPressedChangedAsync(this, pressed);

    /// <summary>
    /// Removes the item from its group.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Unregisters the item from its group.
    /// </summary>
    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            cascadedGroup?.Unregister(this);
        }
    }
}
