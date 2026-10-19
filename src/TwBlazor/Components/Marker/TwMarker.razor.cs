// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using Microsoft.AspNetCore.Components;
using TwBlazor.Builders;
using TwBlazor.Configuration.Components;
using TwBlazor.Enums;
using TwBlazor.Localization;
using TwBlazor.Utilities;

namespace TwBlazor.Components;

/// <summary>
/// Represents an inline marker in a conversation: a status line, such as a tool call an assistant made, or a
/// separator between rows with an optional label.
/// </summary>
/// <remarks>
/// <para>
/// A <see cref="MarkerVariant.Status"/> marker shows an icon, a label and an optional detail. Set
/// <see cref="Status"/> to show a spinner while a step runs and a tick or a cross when it finishes.
/// </para>
/// <para>
/// Accessibility: a status marker's state is written out for screen readers after its label, so it is never
/// told by the icon or color alone. Set <see cref="Live"/> to have changes announced as they happen. A
/// <see cref="MarkerVariant.Separator"/> marker is a separator, named by its <see cref="Label"/>. A separator
/// with custom <see cref="ChildContent"/> needs an <c>AriaLabel</c> to be named.
/// </para>
/// </remarks>
public partial class TwMarker : TwBlazorComponentBase
{
    private TwMarkerTheme theme => options.Theme.Components.Require<TwMarkerTheme>();

    /// <summary>
    /// Gets or sets what the marker draws.
    /// </summary>
    [Parameter] public MarkerVariant Variant { get; set; } = MarkerVariant.Status;

    /// <summary>
    /// Gets or sets the marker's text, for example <c>"Searched the web"</c> or <c>"Today"</c>.
    /// </summary>
    [Parameter] public string? Label { get; set; }

    /// <summary>
    /// Gets or sets custom content shown in place of <see cref="Label"/>.
    /// </summary>
    [Parameter] public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Gets or sets the state a status marker reports.
    /// </summary>
    [Parameter] public MarkerStatus Status { get; set; } = MarkerStatus.None;

    /// <summary>
    /// Gets or sets the icon a status marker shows.
    /// </summary>
    /// <remarks>
    /// If not set, the icon follows <see cref="Status"/>. A running marker always shows a spinner.
    /// </remarks>
    [Parameter] public Icon? Icon { get; set; }

    /// <summary>
    /// Gets or sets the color of a status marker's icon or spinner.
    /// </summary>
    /// <remarks>
    /// If not set, the icon takes the color of its <see cref="Status"/>.
    /// </remarks>
    [Parameter] public Color? Color { get; set; }

    /// <summary>
    /// Gets or sets muted text shown after a status marker's label, for example how long a step took.
    /// </summary>
    [Parameter] public string? Detail { get; set; }

    /// <summary>
    /// Gets or sets whether a status marker is a polite live region, so screen readers announce it when its
    /// label or status changes.
    /// </summary>
    [Parameter] public bool Live { get; set; }

    private bool hasLabel => ChildContent != null || !string.IsNullOrWhiteSpace(Label);

    private string? separatorName => AriaLabelledBy == null ? AriaLabel ?? (string.IsNullOrWhiteSpace(Label) ? null : Label) : null;

    private Icon? effectiveIcon => Icon ?? Status switch
    {
        MarkerStatus.Pending => Enums.Icon.Circle,
        MarkerStatus.Succeeded => Enums.Icon.Check_Circle_Fill,
        MarkerStatus.Failed => Enums.Icon.X_Circle_Fill,
        _ => null
    };

    private string? statusText => Status switch
    {
        MarkerStatus.Pending => localizer[TwStrings.Marker_Pending],
        MarkerStatus.Running => localizer[TwStrings.Marker_Running],
        MarkerStatus.Succeeded => localizer[TwStrings.Marker_Succeeded],
        MarkerStatus.Failed => localizer[TwStrings.Marker_Failed],
        _ => null
    };

    private string statusIconColor => Status switch
    {
        MarkerStatus.Pending => theme.Pending,
        MarkerStatus.Succeeded => theme.Succeeded,
        MarkerStatus.Failed => theme.Failed,
        _ => string.Empty
    };

    private string iconClasses => new ClassBuilder(theme.Icon)
        .AddClass(Color.HasValue ? iconColorBuilder.GetIconColor(Color.Value) : statusIconColor)
        .Build();

    private string statusClasses => new ClassBuilder(theme.Status)
        .AddClass(Class)
        .Build();

    private string separatorClasses => new ClassBuilder(theme.Separator)
        .AddClass(Class)
        .Build();
}
