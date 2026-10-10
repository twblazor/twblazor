// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Microsoft.AspNetCore.Components.Web;
using TwBlazor.Enums;
using TwBlazor.Utilities;

namespace TwBlazor.Components;

/// <summary>
/// Wraps any content and shows a short description in a popover-styled bubble when that content is
/// hovered or keyboard-focused. On a touch screen a tap shows it and a tap anywhere else hides it.
/// </summary>
/// <remarks>
/// The bubble is a <c>role="tooltip"</c> element linked to the wrapper via <c>aria-describedby</c>. It stays
/// visible while the pointer moves onto it (hoverable), can be dismissed with Escape without moving focus
/// (dismissible), and remains until the user leaves or dismisses it (persistent), matching WCAG 1.4.13.
/// Because a tooltip only supplements the accessible name, it must never be the sole source of essential
/// information, and it must not contain interactive content. To describe a button or icon button, prefer the
/// <c>Tooltip</c> parameter on <see cref="TwButton"/> or <see cref="TwIcon"/>, which links the tooltip to the
/// control itself.
/// </remarks>
public partial class TwTooltip : TwBlazorComponentBase
{
    /// <summary>
    /// Gets or sets the content the tooltip describes.
    /// </summary>
    [Parameter] public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Gets or sets the plain text shown in the tooltip bubble.
    /// </summary>
    /// <remarks>
    /// Ignored when <see cref="TooltipContent"/> is set. When both are empty no bubble is rendered.
    /// </remarks>
    [Parameter] public string? Text { get; set; }

    /// <summary>
    /// Gets or sets rich (non-interactive) content shown in the tooltip bubble instead of <see cref="Text"/>.
    /// </summary>
    [Parameter] public RenderFragment? TooltipContent { get; set; }

    /// <summary>
    /// Gets or sets which side of the content the bubble appears on.
    /// </summary>
    /// <remarks>
    /// Default is <see cref="TooltipPlacement.Top"/>.
    /// </remarks>
    [Parameter] public TooltipPlacement Placement { get; set; } = TooltipPlacement.Top;

    /// <summary>
    /// Gets or sets whether the tooltip is turned off. The content still renders, but without a bubble.
    /// </summary>
    [Parameter] public bool Disabled { get; set; }

    /// <summary>
    /// Gets or sets whether the bubble shows a small arrow pointing at the content.
    /// </summary>
    /// <remarks>
    /// Default is <see langword="false"/>. The arrow is decorative and hidden from assistive technology.
    /// </remarks>
    [Parameter] public bool Arrow { get; set; }

    /// <summary>
    /// Gets or sets whether the wrapper itself is added to the tab order so keyboard users can reveal the
    /// tooltip.
    /// </summary>
    /// <remarks>
    /// Default is <see langword="true"/>, which is needed when the content is not already focusable (for
    /// example a paragraph or an abbreviation). Set to <see langword="false"/> when the content contains its own
    /// focusable control, to avoid a duplicate tab stop; the tooltip still appears when that control gains
    /// focus.
    /// </remarks>
    [Parameter] public bool Focusable { get; set; } = true;

    /// <summary>
    /// Gets or sets additional classes applied to the tooltip bubble (as opposed to <see cref="TwBlazorComponentBase.Class"/>,
    /// which is applied to the wrapper).
    /// </summary>
    [Parameter] public string? TooltipClass { get; set; }

    /// <summary>
    /// Gets or sets inline styles applied to the tooltip bubble (as opposed to <see cref="TwBlazorComponentBase.Style"/>,
    /// which is applied to the wrapper), for example to place the bubble somewhere other than the middle of the content.
    /// </summary>
    [Parameter] public string? TooltipStyle { get; set; }

    /// <summary>
    /// Set when the user presses Escape while the tooltip is showing, hiding it until the pointer
    /// leaves or focus moves away (WCAG 1.4.13 "dismissible").
    /// </summary>
    private bool dismissed;

    private ElementReference wrapperRef;

    // Whether the wrapped content is itself focusable (a button, a link). The tooltip then describes that
    // control and the wrapper stays out of the tab order, so the control is not reached twice.
    private bool describesControl;

    [Inject] private IJSRuntime jsRuntime { get; set; } = null!;

    /// <inheritdoc />
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);

        // A tooltip that never takes focus (Focusable off, as on a chart's data points) has nothing to hand over.
        if (!firstRender || !isFocusable)
        {
            return;
        }

        try
        {
            describesControl = await jsRuntime.InvokeAsync<bool>("twTooltip.describeControl", wrapperRef, tooltipId);
        }
        catch (JSDisconnectedException)
        {
            // The circuit disconnected before the script could run; the wrapper keeps its own tab stop.
        }
        catch (InvalidOperationException)
        {
            // JS interop is not available (prerendering); the wrapper keeps its own tab stop.
        }

        if (describesControl)
        {
            StateHasChanged();
        }
    }

    private bool hasTooltip => !Disabled && (TooltipContent != null || !string.IsNullOrWhiteSpace(Text));

    private bool isFocusable => hasTooltip && Focusable;

    private string tooltipId => $"{Id}-tooltip";

    private string wrapperClasses => new ClassBuilder(popoverBuilder.GetTooltipWrapperClasses())
        .AddClass(colorBuilder.GetFocusRing(null), isFocusable)
        .AddClass(Class)
        .Build();

    private string tooltipClasses => popoverBuilder.GetTooltipClasses(dismissed, Placement, Rounded, Shadow, TooltipClass);

    private string arrowClasses => popoverBuilder.GetTooltipArrowClasses(Placement);

    private void OnKeyDown(KeyboardEventArgs e)
    {
        if (e.Key == "Escape")
        {
            dismissed = true;
        }
    }

    private void ResetDismissal() => dismissed = false;
}
