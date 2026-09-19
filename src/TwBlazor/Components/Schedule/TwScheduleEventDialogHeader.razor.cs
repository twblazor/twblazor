// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using Microsoft.AspNetCore.Components;
using TwBlazor.Configuration.Components;
using TwBlazor.Utilities;

namespace TwBlazor.Components;

/// <summary>
/// The read-only <see cref="TwScheduleEventDialog{T}"/>'s own header - the event's color and name,
/// shown in place of <see cref="TwDialog"/>'s default title bar (suppressed via
/// <see cref="TwBlazor.Models.TwDialogOptions.NoHeader"/> for that dialog instance) so a read-only
/// event reads as "here's the event" rather than a generic "Event" title over a form.
/// </summary>
public partial class TwScheduleEventDialogHeader : TwBlazorComponentBase
{
    private TwScheduleTheme theme => options.Theme.Components.Require<TwScheduleTheme>();

    /// <summary>
    /// The event's name.
    /// </summary>
    [Parameter] public string? Name { get; set; }

    /// <summary>
    /// The event's <see cref="TwBlazor.Components.Schedule{T}.Color"/> (or <see langword="null"/> to
    /// fall back to the theme's default tint) - see <see cref="TwScheduleColors.GetEventCardStyle"/>.
    /// </summary>
    [Parameter] public string? Color { get; set; }

    /// <summary>
    /// Invoked when the close icon is activated.
    /// </summary>
    [Parameter] public EventCallback OnClose { get; set; }

    private string headerClasses => new ClassBuilder(theme.EventDialogHeader).AddClass(Class).Build();

    private string headerStyle => $"{TwScheduleColors.GetEventCardStyle(Color)}{Style}";

    private string titleClasses => new ClassBuilder(theme.EventDialogHeaderTitle).Build();

    private string closeClasses => new ClassBuilder(theme.EventDialogHeaderClose).Build();
}
