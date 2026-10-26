// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using Microsoft.AspNetCore.Components;
using TwBlazor.Configuration.Components;
using TwBlazor.Enums;
using TwBlazor.Utilities;

namespace TwBlazor.Components;

public partial class TwLink : TwBlazorComponentBase
{
    private TwLinkTheme theme => options.Theme.Components.Require<TwLinkTheme>();

    /// <summary>
    /// Gets or sets the URL that the hyperlink points to.
    /// </summary>
    [Parameter] public string Href { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the name of the target frame or window for the navigation request.
    /// </summary>
    /// <remarks>Set this property to specify where the linked content will be displayed. Common values
    /// include "_blank" to open in a new window or tab, "_self" to open in the same frame, and "_parent" or "_top" for
    /// parent or top-level frames. If not set, the default behaviour depends on the browser and context.</remarks>
    [Parameter] public string? Target { get; set; }

    /// <summary>
    /// Gets or sets the semantic colour of the link.
    /// </summary>
    /// <remarks>
    /// When neither <see cref="Color"/> nor <see cref="Tone"/> is set, a link without a custom
    /// <see cref="TwBlazorComponentBase.Class"/> uses the theme's primary text colour. A custom class replaces that
    /// default look, so links used inside a button keep the button's own colour; an explicit colour or tone is still
    /// applied.
    /// </remarks>
    [Parameter] public Color? Color { get; set; }

    /// <summary>
    /// Gets or sets the neutral colour of the link.
    /// </summary>
    /// <remarks>
    /// Ignored when <see cref="Color"/> is set. Use <see cref="TextTone.Inherit"/> to keep the surrounding colour.
    /// </remarks>
    [Parameter] public TextTone? Tone { get; set; }

    /// <summary>
    /// Gets or sets the size on the theme's font-size scale.
    /// </summary>
    /// <remarks>
    /// Leave it unset to inherit the surrounding font size.
    /// </remarks>
    [Parameter] public TextSize? Size { get; set; }

    /// <summary>
    /// Gets or sets the weight on the theme's font-weight scale.
    /// </summary>
    /// <remarks>
    /// Leave it unset to inherit the surrounding font weight.
    /// </remarks>
    [Parameter] public TextWeight? Weight { get; set; }

    /// <summary>
    /// Gets or sets whether overflowing link text is truncated with an ellipsis.
    /// </summary>
    [Parameter] public bool Truncate { get; set; }

    /// <summary>
    /// The child content of the link element.
    /// </summary>
    [Parameter] public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Gets or sets the relationship between the linked resource and the current document.
    /// </summary>
    /// <remarks>
    /// When not set and <see cref="Target"/> is "_blank", defaults to "noopener" so a new-tab link
    /// can't access <c>window.opener</c> on the page it opened. This property overrides that default.
    /// </remarks>
    [Parameter] public string? Rel { get; set; }

    private string? effectiveRel => Rel ?? (Target == "_blank" ? "noopener" : null);

    private string classes
    {
        get
        {
            var builder = new ClassBuilder();
            var hasCustomClass = !string.IsNullOrEmpty(Class);
            var hasExplicitTone = Color is not null || Tone is not null;

            if (hasExplicitTone)
            {
                builder.AddClass(TextClasses.Tone(options, colorBuilder, Color, Tone));
            }
            else if (!hasCustomClass)
            {
                builder.AddClass(colorBuilder.GetTextColor(Enums.Color.Primary));
            }

            if (hasCustomClass)
            {
                // A custom class replaces the theme's default link treatment, but it must not remove the
                // visible focus ring that keeps keyboard focus usable.
                builder.AddClass(colorBuilder.GetFocusRing(Color));
                builder.AddClass(options.Theme.Interaction.FocusOutlineNone);
            }
            else
            {
                builder.AddClass(theme.Default);

                // The default link treatment carries the primary focus ring, so give an explicit
                // semantic colour its matching ring as well.
                if (Color is not null)
                {
                    builder.AddClass(colorBuilder.GetFocusRing(Color));
                }
            }

            builder.AddClass(TextClasses.Size(options.Theme.Typography, Size));
            builder.AddClass(TextClasses.Weight(options.Theme.Typography, Weight));
            builder.AddClass(options.Theme.Typography.Truncate, Truncate);
            builder.AddClass(Class);

            return builder.Build();
        }
    }
}
