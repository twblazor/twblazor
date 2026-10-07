// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using Microsoft.AspNetCore.Components;
using TwBlazor.Configuration.Components;
using TwBlazor.Utilities;

namespace TwBlazor.Components.DatePicker;

public partial class TwDatePickerHeader : TwBlazorComponentBase
{
    private TwDatePickerTheme theme => options.Theme.Components.Require<TwDatePickerTheme>();

    /// <summary>
    /// The child content displayed in the datepicker header.
    /// </summary>
    [Parameter] public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Gets or sets the action performed when the title is clicked e.g. navigation from month view to decade view. 
    /// </summary>
    [Parameter] public EventCallback OnTitleClick { get; set; }

    /// <summary>
    /// Gets or sets the action performed when the next button is clicked, such as navigating to the next month.
    /// </summary>
    [Parameter] public EventCallback OnNextClick { get; set; }

    /// <summary>
    /// Gets or sets the action performed when the previous button is clicked, such as navigating to the previous month.
    /// </summary>
    [Parameter] public EventCallback OnPreviousClick { get; set; }

    /// <summary>
    /// Accessible name for the "previous" navigation button (e.g. "Previous month", "Previous year",
    /// "Previous decade" depending on which view this header is rendered for).
    /// </summary>
    [Parameter] public string PreviousLabel { get; set; } = "Previous";

    /// <summary>
    /// Accessible name for the "next" navigation button (e.g. "Next month", "Next year", "Next decade"
    /// depending on which view this header is rendered for).
    /// </summary>
    [Parameter] public string NextLabel { get; set; } = "Next";

    /// <summary>
    /// Disables the "previous" navigation button, e.g. once a picker's <c>MinDate</c> boundary is
    /// reached. Defaults to <see langword="false"/> so existing callers are unaffected.
    /// </summary>
    [Parameter] public bool PreviousDisabled { get; set; }

    /// <summary>
    /// Disables the "next" navigation button, e.g. once a picker's <c>MaxDate</c> boundary is
    /// reached. Defaults to <see langword="false"/> so existing callers are unaffected.
    /// </summary>
    [Parameter] public bool NextDisabled { get; set; }

    private string classes => new ClassBuilder("datepicker-header")
        .AddClass(theme.Header).Build();

    private string headerControlsClasses => new ClassBuilder("datepicker-controls")
        .AddClass(theme.HeaderControls).Build();

    /// <summary>
    /// Gets the classes for the title button that switches between day/month/year views. Uses the
    /// same <see cref="TwBlazor.Configuration.TwBlazorColor.HoverColors"/> primary tint as the day
    /// grid's own cells (see <see cref="TwDatePickerDayView.GetButtonClasses"/>), since this button
    /// otherwise renders with no Color/Variant and so has no hover affordance of its own.
    /// </summary>
    private string titleButtonClasses => new ClassBuilder("view-switch")
        .AddClass(theme.HoverClass)
        .Build();
}
