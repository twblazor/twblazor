// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using Microsoft.AspNetCore.Components;
using TwBlazor.Configuration;
using TwBlazor.Enums;
using TwBlazor.Utilities;
using Color = TwBlazor.Enums.Color;

namespace TwBlazor.Components;

/// <summary>
/// Renders text with the theme's neutral text colours and shared typography scale.
/// </summary>
/// <remarks>
/// <para>
/// The element defaults to a <c>span</c>, so the component can sit inside a line of text. Set
/// <see cref="TwLayoutComponentBase.Element"/> to a paragraph, heading or other semantic element when the text is a
/// block of its own.
/// </para>
/// <para>
/// The colour comes from <see cref="TwBlazorColor.NeutralText"/> through <see cref="Tone"/>, so it follows light and
/// dark mode without retyping gray classes. Set <see cref="Color"/> to use a semantic colour from the theme instead,
/// or <see cref="TextTone.Inherit"/> to keep the colour of the surrounding element.
/// </para>
/// </remarks>
public class TwText : TwLayoutComponentBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TwText"/> class.
    /// </summary>
    public TwText()
    {
        Element = LayoutElement.Span;
    }

    /// <summary>
    /// Gets or sets the neutral colour of the text. Defaults to <see cref="TextTone.Default"/>.
    /// </summary>
    /// <remarks>
    /// Ignored when <see cref="Color"/> is set. Use <see cref="TextTone.Inherit"/> inside a link, button or other
    /// element that already supplies its own colour.
    /// </remarks>
    [Parameter] public TextTone Tone { get; set; } = TextTone.Default;

    /// <summary>
    /// Gets or sets a semantic colour from the theme, such as <see cref="Color.Primary"/>.
    /// </summary>
    /// <remarks>
    /// Takes precedence over <see cref="Tone"/> so text can share the colour of a button, link or status.
    /// </remarks>
    [Parameter] public Color? Color { get; set; }

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
    /// Gets or sets whether overflowing text is truncated with an ellipsis.
    /// </summary>
    [Parameter] public bool Truncate { get; set; }

    private TwBlazorTypography typography => options.Theme.Typography;

    /// <inheritdoc />
    protected override string rootClasses => new ClassBuilder()
        .AddClass(TextClasses.Tone(options, colorBuilder, Color, Tone))
        .AddClass(TextClasses.Size(typography, Size))
        .AddClass(TextClasses.Weight(typography, Weight))
        .AddClass(typography.Truncate, Truncate)
        .AddClass(Class)
        .Build();
}
