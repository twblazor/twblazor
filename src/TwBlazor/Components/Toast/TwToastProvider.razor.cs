// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using Microsoft.AspNetCore.Components;
using TwBlazor.Builders;
using TwBlazor.Configuration.Components;
using TwBlazor.Models;
using TwBlazor.Services;
using TwBlazor.Utilities;

namespace TwBlazor.Components;

/// <summary>
/// A component that displays toast notifications managed by <see cref="ITwToastService"/>.
/// </summary>
public sealed partial class TwToastProvider : IDisposable
{
    [Inject] private ITwToastService toastService { get; set; } = null!;
    [Inject] private ToastBuilder toastBuilder { get; set; } = null!;

    private TwToastTheme theme => options.Theme.Components.Require<TwToastTheme>();

    /// <summary>
    /// Gets the CSS classes applied to the toast container element.
    /// </summary>
    private string containerClasses =>
        new ClassBuilder(theme.Container)
        .AddClass(Class)
        .Build();

    /// <summary>
    /// Gets the CSS classes applied to the toast header section.
    /// </summary>
    private string headerClasses => theme.HeaderClasses;

    /// <summary>
    /// Gets the CSS classes applied to the toast title.
    /// </summary>
    private string titleClasses => theme.Title;

    /// <summary>
    /// Gets the CSS classes applied to the toast message.
    /// </summary>
    private string messageClasses => theme.Message;

    /// <summary>
    /// Gets the CSS classes applied to the toast timestamp.
    /// </summary>
    private string timestampClasses => theme.Timestamp;

    /// <summary>
    /// Gets the CSS classes applied to the icon container.
    /// </summary>
    private string iconContainerClasses => theme.IconContainer;

    /// <summary>
    /// Gets the CSS classes applied to the close button.
    /// </summary>
    private string closeButtonClasses => theme.CloseButton;

    /// <summary>
    /// Gets the CSS classes for a specific toast based on its color.
    /// </summary>
    private string GetToastClasses(ToastModel toast)
    {
        return new ClassBuilder()
            .AddClass(toastBuilder.GetToastClasses(base.Rounded))
            .AddClass(GetToastColor(toast.Color))
            .Build();
    }

    /// <summary>
    /// Initializes the component and subscribes to toast service events.
    /// </summary>
    protected override void OnInitialized()
    {
        if (toastService is null)
            return;

        toastService.ToasterChanged += OnToastChanged;
        toastService.ToasterTimerElapsed += OnToastChanged;
    }

    private string GetToastColor(Enums.Color? color) => ColorBuilder.GetPaletteColor(color, theme.Colors, string.Empty);

    /// <summary>
    /// Clears a specific toast.
    /// </summary>
    // An error or a warning interrupts; anything else waits for the screen reader to finish speaking.
    private static string GetToastRole(ToastModel toast) =>
        toast.Color is Enums.Color.Danger or Enums.Color.Warning ? "alert" : "status";

    private void ClearToast(ToastModel toast) => toastService?.ClearToast(toast);

    /// <summary>
    /// Pauses a toast's auto-dismiss timer while the pointer is hovering it, so users don't lose a
    /// toast to auto-dismiss while still reading it.
    /// </summary>
    private void PauseToastForHover(ToastModel toast) => toastService?.PauseToastForHover(toast.Id);

    /// <summary>
    /// Resumes a toast's auto-dismiss timer once the pointer has left it, unless it still has keyboard
    /// focus (e.g. a user tabbed onto its close button before moving the pointer away).
    /// </summary>
    private void ResumeToastForHover(ToastModel toast) => toastService?.ResumeToastForHover(toast.Id);

    /// <summary>
    /// Pauses a toast's auto-dismiss timer while it (or a descendant, e.g. its close button) has
    /// keyboard focus, so users don't lose a toast to auto-dismiss while still interacting with it.
    /// </summary>
    private void PauseToastForFocus(ToastModel toast) => toastService?.PauseToastForFocus(toast.Id);

    /// <summary>
    /// Resumes a toast's auto-dismiss timer once keyboard focus has left it, unless the pointer is
    /// still hovering it.
    /// </summary>
    private void ResumeToastForFocus(ToastModel toast) => toastService?.ResumeToastForFocus(toast.Id);

    /// <summary>
    /// Handles toast collection changes and triggers a state update.
    /// </summary>
    private void OnToastChanged(object? sender, EventArgs e) => base.InvokeAsync(StateHasChanged);

    /// <summary>
    /// Unsubscribes from toast service events.
    /// </summary>
    public void Dispose()
    {
        if (toastService is null)
            return;

        toastService.ToasterChanged -= OnToastChanged;
        toastService.ToasterTimerElapsed -= OnToastChanged;

        GC.SuppressFinalize(this);
    }
}
