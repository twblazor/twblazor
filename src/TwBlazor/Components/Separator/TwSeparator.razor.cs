// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using Microsoft.AspNetCore.Components;
using TwBlazor.Builders;
using TwBlazor.Configuration.Components;
using TwBlazor.Enums;
using TwBlazor.Utilities;

namespace TwBlazor.Components;

/// <summary>
/// Represents a horizontal or vertical line that divides content, optionally with a label such as "or" or a date
/// in the line.
/// </summary>
/// <remarks>
/// By default the separator is exposed to assistive technology as a separator with its orientation, named by its
/// <see cref="Label"/>. Set <see cref="Decorative"/> when the line is purely visual, so it is hidden from
/// assistive technology.
/// </remarks>
public partial class TwSeparator : TwBlazorComponentBase
{
    private TwSeparatorTheme theme => options.Theme.Components.Require<TwSeparatorTheme>();

    /// <summary>
    /// Gets or sets whether the separator runs across (a row) or down (a column). Defaults to
    /// <see cref="SeparatorOrientation.Horizontal"/>.
    /// </summary>
    /// <remarks>
    /// A vertical separator stretches to the height of its parent row, for example between the items of a flex
    /// toolbar.
    /// </remarks>
    [Parameter] public SeparatorOrientation Orientation { get; set; } = SeparatorOrientation.Horizontal;

    /// <summary>
    /// Gets or sets whether the separator is purely visual. Defaults to <see langword="false"/>.
    /// </summary>
    /// <remarks>
    /// A decorative separator has <c>role="none"</c> and, with no label, is hidden with <c>aria-hidden</c>; a label
    /// is then read as plain text. Otherwise the separator has <c>role="separator"</c> and <c>aria-orientation</c>,
    /// so screen readers announce the break between the content on either side.
    /// </remarks>
    [Parameter] public bool Decorative { get; set; }

    /// <summary>
    /// Gets or sets the text shown in the line, such as "or continue with". It also names the separator for
    /// assistive technology.
    /// </summary>
    [Parameter] public string? Label { get; set; }

    /// <summary>
    /// Gets or sets custom content shown in the line instead of <see cref="Label"/>, such as an icon or a button.
    /// </summary>
    /// <remarks>
    /// Assistive technology does not read the content of a separator, so set <see cref="TwBlazorComponentBase.AriaLabel"/>
    /// to name a separator with custom content, unless it is <see cref="Decorative"/>.
    /// </remarks>
    [Parameter] public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Gets or sets where the label sits along the line. Defaults to <see cref="SeparatorLabelAlign.Center"/>.
    /// </summary>
    /// <remarks>
    /// Start and end follow the reading direction, so a start-aligned label sits on the right in a right to left
    /// layout.
    /// </remarks>
    [Parameter] public SeparatorLabelAlign LabelAlign { get; set; } = SeparatorLabelAlign.Center;

    /// <summary>
    /// Gets or sets the line style. Defaults to <see cref="SeparatorVariant.Solid"/>.
    /// </summary>
    [Parameter] public SeparatorVariant Variant { get; set; } = SeparatorVariant.Solid;

    /// <summary>
    /// Gets or sets how thick the line is. Defaults to <see cref="SeparatorThickness.Thin"/>.
    /// </summary>
    [Parameter] public SeparatorThickness Thickness { get; set; } = SeparatorThickness.Thin;

    /// <summary>
    /// Gets or sets the color of the line and label. If not set, the separator uses the theme's neutral border
    /// color.
    /// </summary>
    [Parameter] public Color? Color { get; set; }

    private bool hasLabel => ChildContent != null || !string.IsNullOrWhiteSpace(Label);

    private bool isVertical => Orientation == SeparatorOrientation.Vertical;

    private string role => Decorative ? "none" : "separator";

    private string? ariaOrientation
    {
        get
        {
            if (Decorative)
            {
                return null;
            }

            return isVertical ? "vertical" : "horizontal";
        }
    }

    private string? ariaHidden => Decorative && !hasLabel ? "true" : null;

    private string? accessibleLabelledBy => Decorative ? null : AriaLabelledBy;

    private string? accessibleName
    {
        get
        {
            if (Decorative || AriaLabelledBy != null)
            {
                return null;
            }

            if (AriaLabel != null)
            {
                return AriaLabel;
            }

            return string.IsNullOrWhiteSpace(Label) ? null : Label;
        }
    }

    private string orientationLine => isVertical ? theme.VerticalLine : theme.HorizontalLine;

    private string thicknessClasses => Thickness switch
    {
        SeparatorThickness.Medium => theme.Medium,
        SeparatorThickness.Thick => theme.Thick,
        _ => theme.Thin,
    };

    private string variantClasses => Variant switch
    {
        SeparatorVariant.Dashed => theme.Dashed,
        SeparatorVariant.Dotted => theme.Dotted,
        _ => theme.Solid,
    };

    private string colorClasses => ColorBuilder.GetPaletteColor(Color, theme.Colors, theme.Neutral);

    private string lineClasses => new ClassBuilder(theme.Base)
        .AddClass(isVertical ? theme.Vertical : theme.Horizontal)
        .AddClass(orientationLine)
        .AddClass(thicknessClasses)
        .AddClass(variantClasses)
        .AddClass(colorClasses)
        .AddClass(Class)
        .Build();

    private string labeledClasses => new ClassBuilder(theme.Base)
        .AddClass(isVertical ? theme.LabeledVertical : theme.LabeledHorizontal)
        .AddClass(thicknessClasses)
        .AddClass(colorClasses)
        .AddClass(Class)
        .Build();

    private string startLineClasses => BuildLabelLine(LabelAlign == SeparatorLabelAlign.Start);

    private string endLineClasses => BuildLabelLine(LabelAlign == SeparatorLabelAlign.End);

    private string BuildLabelLine(bool inset) => new ClassBuilder(theme.LabelLine)
        .AddClass(orientationLine)
        .AddClass(variantClasses)
        .AddClass(inset ? theme.LineInset : theme.LineStretch)
        .Build();
}
