// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;

namespace TwBlazor.Configuration.Components;

/// <summary>
/// Theme configuration for the drawer component (<see cref="TwBlazor.Components.TwDrawer"/>).
/// Override any property to customize drawer styles globally.
/// </summary>
/// <remarks>
/// The backdrop and the panel carry a <c>data-state</c> attribute of <c>open</c> or <c>closed</c>. Style the closed
/// state with the <c>data-[state=closed]:</c> variant, so the drawer slides and fades in both directions with CSS
/// alone. The panel stays on the page while closed, hidden with <c>invisible</c>, so it can animate out.
/// </remarks>
[ExcludeFromCodeCoverage]
public class TwDrawerTheme
{
    /// <summary>
    /// Gets or sets the classes for the element wrapping the backdrop and the panel. It takes no space in the
    /// page's layout.
    /// </summary>
    public required string Root { get; set; }

    /// <summary>
    /// Gets or sets the classes for the backdrop that dims the page behind a modal drawer.
    /// </summary>
    public required string Backdrop { get; set; }

    /// <summary>
    /// Gets or sets the base classes for the panel: position, background, text color and transition.
    /// </summary>
    public required string Panel { get; set; }

    /// <summary>
    /// Gets or sets the classes for a panel that slides in from the start edge, including its closed position.
    /// </summary>
    public required string Start { get; set; }

    /// <summary>
    /// Gets or sets the classes for a panel that slides in from the end edge, including its closed position.
    /// </summary>
    public required string End { get; set; }

    /// <summary>
    /// Gets or sets the classes for a panel that slides down from the top edge, including its closed position.
    /// </summary>
    public required string Top { get; set; }

    /// <summary>
    /// Gets or sets the classes for a panel that slides up from the bottom edge, including its closed position.
    /// </summary>
    public required string Bottom { get; set; }

    /// <summary>
    /// Gets or sets the width of a <see cref="Enums.DrawerSize.Small"/> drawer on the start or end edge.
    /// </summary>
    public required string SmallWidth { get; set; }

    /// <summary>
    /// Gets or sets the width of a <see cref="Enums.DrawerSize.Medium"/> drawer on the start or end edge.
    /// </summary>
    public required string MediumWidth { get; set; }

    /// <summary>
    /// Gets or sets the width of a <see cref="Enums.DrawerSize.Large"/> drawer on the start or end edge.
    /// </summary>
    public required string LargeWidth { get; set; }

    /// <summary>
    /// Gets or sets the width of a <see cref="Enums.DrawerSize.Full"/> drawer on the start or end edge.
    /// </summary>
    public required string FullWidth { get; set; }

    /// <summary>
    /// Gets or sets the height of a <see cref="Enums.DrawerSize.Small"/> drawer on the top or bottom edge.
    /// </summary>
    public required string SmallHeight { get; set; }

    /// <summary>
    /// Gets or sets the height of a <see cref="Enums.DrawerSize.Medium"/> drawer on the top or bottom edge.
    /// </summary>
    public required string MediumHeight { get; set; }

    /// <summary>
    /// Gets or sets the height of a <see cref="Enums.DrawerSize.Large"/> drawer on the top or bottom edge.
    /// </summary>
    public required string LargeHeight { get; set; }

    /// <summary>
    /// Gets or sets the height of a <see cref="Enums.DrawerSize.Full"/> drawer on the top or bottom edge.
    /// </summary>
    public required string FullHeight { get; set; }

    /// <summary>
    /// Gets or sets the classes for the header row holding the title, description and close button.
    /// </summary>
    public required string Header { get; set; }

    /// <summary>
    /// Gets or sets the classes for the block holding the title and description inside the header.
    /// </summary>
    public required string Heading { get; set; }

    /// <summary>
    /// Gets or sets the classes for the title, which also names the drawer for assistive technology.
    /// </summary>
    public required string Title { get; set; }

    /// <summary>
    /// Gets or sets the classes for the description under the title.
    /// </summary>
    public required string Description { get; set; }

    /// <summary>
    /// Gets or sets the classes for the close button in the header.
    /// </summary>
    public required string CloseButton { get; set; }

    /// <summary>
    /// Gets or sets the classes for the body, which scrolls when the content is taller than the panel.
    /// </summary>
    public required string Content { get; set; }

    /// <summary>
    /// Gets or sets the classes for the footer, which stays in view below the body.
    /// </summary>
    public required string Footer { get; set; }
}
