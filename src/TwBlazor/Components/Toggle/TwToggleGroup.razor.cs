// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using TwBlazor.Builders;
using TwBlazor.Configuration.Components;
using TwBlazor.Enums;
using TwBlazor.Utilities;
using Color = TwBlazor.Enums.Color;

namespace TwBlazor.Components;

/// <summary>
/// A set of related toggle buttons, such as text alignment or view mode options, where one item
/// (<see cref="Multiple"/> off) or any number of items (<see cref="Multiple"/> on) can be pressed.
/// </summary>
/// <typeparam name="T">The type of the value each item stands for.</typeparam>
/// <remarks>
/// <para>
/// Add a <see cref="TwToggleGroupItem{T}"/> for each option; each one renders a <see cref="TwToggle"/>, styled by
/// the same <see cref="TwToggleTheme"/>. Bind the pressed item with <c>@bind-Value</c>, or the pressed items of
/// a <see cref="Multiple"/> group with <c>@bind-Values</c>.
/// </para>
/// <para>
/// The group is a single Tab stop, and the arrow keys, Home and End move between its items (Arrow Left and
/// Arrow Right swap in a right to left layout). A single-select group follows the WAI-ARIA radio group pattern:
/// it is a <c>radiogroup</c> of <c>radio</c> items, and moving to an item selects it. A multiple-select group
/// follows the toolbar pattern: it is a <c>group</c> of toggle buttons with <c>aria-pressed</c>, and Space or
/// Enter presses the focused item. Give the group an accessible name with
/// <see cref="TwBlazorComponentBase.AriaLabel"/> or <see cref="TwBlazorComponentBase.AriaLabelledBy"/>.
/// </para>
/// </remarks>
public partial class TwToggleGroup<T> : TwBlazorComponentBase, IAsyncDisposable
{
    [Inject] private IJSRuntime jsRuntime { get; set; } = null!;

    private TwToggleTheme theme => options.Theme.Components.Require<TwToggleTheme>();

    private readonly List<TwToggleGroupItem<T>> _items = [];

    private ElementReference groupElement;

    private bool keydownGuardRegistered;

    /// <summary>
    /// The item that last had focus, which keeps the group's Tab stop when focus leaves and comes back.
    /// </summary>
    private TwToggleGroupItem<T>? activeItem;

    /// <summary>
    /// Gets or sets the items of the group, each a <see cref="TwToggleGroupItem{T}"/>.
    /// </summary>
    [Parameter] public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Gets or sets whether any number of items can be pressed at once. Bind <see cref="Values"/> when this is
    /// <see langword="true"/>, and <see cref="Value"/> when it is <see langword="false"/> (the default).
    /// </summary>
    [Parameter] public bool Multiple { get; set; }

    /// <summary>
    /// Gets or sets the value of the pressed item of a single-select group.
    /// </summary>
    /// <remarks>Not used when <see cref="Multiple"/> is <see langword="true"/>.</remarks>
    [Parameter] public T? Value { get; set; }

    /// <summary>
    /// Gets or sets the callback invoked when the pressed item of a single-select group changes. Releasing the
    /// pressed item of a <see cref="Deselectable"/> group passes <see langword="default"/>.
    /// </summary>
    [Parameter] public EventCallback<T?> ValueChanged { get; set; }

    /// <summary>
    /// Gets or sets the values of the pressed items of a <see cref="Multiple"/> group.
    /// </summary>
    [Parameter] public IEnumerable<T> Values { get; set; } = [];

    /// <summary>
    /// Gets or sets the callback invoked when the pressed items of a <see cref="Multiple"/> group change. The
    /// values are in the order they were pressed.
    /// </summary>
    [Parameter] public EventCallback<IEnumerable<T>> ValuesChanged { get; set; }

    /// <summary>
    /// Gets or sets whether pressing the pressed item of a single-select group releases it, leaving no item
    /// pressed. Defaults to <see langword="false"/>, so a selection, once made, always stays, as in a radio group.
    /// </summary>
    [Parameter] public bool Deselectable { get; set; }

    /// <summary>
    /// Gets or sets how the group and its items are drawn.
    /// </summary>
    /// <remarks>If not set, uses <see cref="TwToggleTheme.DefaultVariant"/>, which defaults to
    /// <see cref="ToggleVariant.Text"/>: the items sit on a filled bar. <see cref="ToggleVariant.Outlined"/> draws a
    /// border around the group instead.</remarks>
    [Parameter] public ToggleVariant? Variant { get; set; }

    /// <summary>
    /// Gets or sets the size of every item. Defaults to <see cref="Enums.Size.Md"/>.
    /// </summary>
    [Parameter] public Size? Size { get; set; }

    /// <summary>
    /// Gets or sets the color of every pressed item, and of the focus ring. An item can set its own.
    /// </summary>
    /// <remarks>If not set, a pressed item is drawn as a raised surface on the bar, or in a neutral tint.</remarks>
    [Parameter] public Color? Color { get; set; }

    /// <summary>
    /// Gets or sets whether every item in the group is disabled.
    /// </summary>
    [Parameter] public bool Disabled { get; set; }

    /// <summary>
    /// Gets or sets whether the items are stacked top to bottom instead of laid out in a row.
    /// </summary>
    [Parameter] public bool Vertical { get; set; }

    /// <summary>
    /// The rounding of the group's bar: the <see cref="TwBlazorComponentBase.Rounded"/> parameter, then
    /// <see cref="TwToggleTheme.ToggleRounded"/>, then the theme's default.
    /// </summary>
    private Rounded groupRounded => Rounded ?? theme.ToggleRounded ?? options.Theme.Rounded.DefaultRounded;

    /// <summary>
    /// The rounding of each item: one step tighter than the bar around it, so their corners follow the bar's.
    /// </summary>
    internal Rounded itemRounded => RoundedBuilder.Inset(groupRounded);

    internal ToggleVariant itemVariant => Variant ?? theme.DefaultVariant ?? ToggleVariant.Text;

    internal Size? itemSize => Size;

    internal Color? itemColor => Color;

    internal bool itemsDisabled => Disabled;

    internal TwToggleGroupItemContext itemContext => Multiple ? TwToggleGroupItemContext.Multiple : TwToggleGroupItemContext.Single;

    private string classes =>
        new ClassBuilder(theme.GroupBase)
        .AddClass(Vertical ? theme.GroupVertical : theme.GroupHorizontal)
        .AddClass(itemVariant == ToggleVariant.Outlined ? theme.GroupOutlined : theme.GroupText)
        .AddClass(roundedBuilder.GetRounded(groupRounded))
        .AddClass(shadowBuilder.GetShadow(Shadow))
        .AddClass(Class)
        .Build();

    /// <summary>
    /// Adds an item to the group, in the order the items are declared.
    /// </summary>
    internal void Register(TwToggleGroupItem<T> item)
    {
        if (!_items.Contains(item))
        {
            _items.Add(item);
            StateHasChanged();
        }
    }

    /// <summary>
    /// Removes an item from the group when it is disposed.
    /// </summary>
    internal void Unregister(TwToggleGroupItem<T> item)
    {
        if (_items.Remove(item))
        {
            if (ReferenceEquals(activeItem, item))
            {
                activeItem = null;
            }

            _ = InvokeAsync(StateHasChanged);
        }
    }

    /// <summary>
    /// Gets whether an item is pressed: it holds <see cref="Value"/>, or one of <see cref="Values"/> when the group
    /// is <see cref="Multiple"/>.
    /// </summary>
    internal bool IsPressed(TwToggleGroupItem<T> item) => Multiple
        ? Values.Contains(item.Value, EqualityComparer<T>.Default)
        : EqualityComparer<T?>.Default.Equals(Value, item.Value);

    private bool IsEnabled(TwToggleGroupItem<T> item) => !Disabled && !item.Disabled;

    /// <summary>
    /// The item that is the group's single Tab stop (roving tabindex): in a single-select group the pressed item,
    /// otherwise the item that last had focus, otherwise the first enabled item.
    /// </summary>
    private TwToggleGroupItem<T>? tabStop
    {
        get
        {
            if (!Multiple && _items.FirstOrDefault(i => IsEnabled(i) && IsPressed(i)) is { } pressed)
            {
                return pressed;
            }

            if (activeItem is not null && _items.Contains(activeItem) && IsEnabled(activeItem))
            {
                return activeItem;
            }

            return _items.FirstOrDefault(IsEnabled);
        }
    }

    /// <summary>
    /// Gets whether an item is the group's Tab stop, so it gets <c>tabindex="0"</c> and every other item
    /// <c>tabindex="-1"</c>.
    /// </summary>
    internal bool IsTabStop(TwToggleGroupItem<T> item) => ReferenceEquals(tabStop, item);

    /// <summary>
    /// Remembers the item that received focus, so it keeps the Tab stop.
    /// </summary>
    internal void OnItemFocused(TwToggleGroupItem<T> item)
    {
        if (!ReferenceEquals(activeItem, item))
        {
            activeItem = item;
            StateHasChanged();
        }
    }

    /// <summary>
    /// Handles an item being pressed or released by a click, Enter or Space.
    /// </summary>
    internal async Task OnItemPressedChangedAsync(TwToggleGroupItem<T> item, bool pressed)
    {
        if (!IsEnabled(item))
        {
            return;
        }

        activeItem = item;

        if (Multiple)
        {
            var values = Values.Where(v => !EqualityComparer<T>.Default.Equals(v, item.Value)).ToList();
            if (pressed)
            {
                values.Add(item.Value);
            }

            Values = values;
            await ValuesChanged.InvokeAsync(Values);
        }
        else if (pressed)
        {
            await SelectAsync(item);
        }
        else if (Deselectable)
        {
            Value = default;
            await ValueChanged.InvokeAsync(Value);
        }

        // Every item re-renders, so a refused release shows the item pressed again and, in a single-select
        // group, the previously pressed item is released.
        StateHasChanged();
    }

    private async Task SelectAsync(TwToggleGroupItem<T> item)
    {
        Value = item.Value;
        await ValueChanged.InvokeAsync(Value);
    }

    /// <summary>
    /// Moves between the items with the arrow keys, Home and End, wrapping at the ends and skipping disabled items.
    /// In a single-select group moving to an item also selects it. In a right to left layout Arrow Left and Arrow
    /// Right swap.
    /// </summary>
    private async Task OnKeyDownAsync(KeyboardEventArgs e)
    {
        var enabled = _items.Where(IsEnabled).ToList();
        if (enabled.Count == 0)
        {
            return;
        }

        var current = activeItem is not null && enabled.Contains(activeItem) ? activeItem : tabStop;
        var index = current is null ? -1 : enabled.IndexOf(current);

        var target = ToLogicalKey(e.Key) switch
        {
            "ArrowRight" or "ArrowDown" => enabled[(index + 1) % enabled.Count],
            "ArrowLeft" or "ArrowUp" => enabled[(index - 1 + enabled.Count) % enabled.Count],
            "Home" => enabled[0],
            "End" => enabled[^1],
            _ => null
        };

        if (target is null || ReferenceEquals(target, current))
        {
            return;
        }

        activeItem = target;

        if (!Multiple)
        {
            await SelectAsync(target);
        }

        StateHasChanged();
        await target.FocusAsync();
    }

    /// <summary>
    /// Registers a listener that stops the arrow keys, Home and End from also scrolling the page while they move
    /// between the items. Other keys, such as Tab, are left alone.
    /// </summary>
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            try
            {
                await jsRuntime.InvokeVoidAsync("twTabs.registerKeydownGuard", groupElement);
                keydownGuardRegistered = true;
            }
            catch (JSDisconnectedException)
            {
                // The circuit disconnected before the script could run; nothing to register.
            }
        }
    }

    /// <summary>
    /// Releases the keydown listener registered for the group.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        try
        {
            if (keydownGuardRegistered)
            {
                await jsRuntime.InvokeVoidAsync("twTabs.unregisterKeydownGuard", groupElement);
            }
        }
        catch (JSDisconnectedException)
        {
            // The circuit is already gone; nothing left to clean up.
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
