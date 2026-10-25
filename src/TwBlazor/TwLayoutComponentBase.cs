// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using TwBlazor.Configuration;
using TwBlazor.Enums;
using TwBlazor.Utilities;

namespace TwBlazor;

/// <summary>
/// The base for the layout components (<see cref="Components.TwContainer"/>, <see cref="Components.TwFlex"/>,
/// <see cref="Components.TwFlexItem"/>, <see cref="Components.TwGrid"/> and <see cref="Components.TwGridItem"/>):
/// renders a single element of the chosen <see cref="Element"/> type around the child content.
/// </summary>
/// <remarks>
/// A layout component adds no keyboard behaviour and no ARIA role of its own, except <c>role="list"</c> on a
/// <c>ul</c> or <c>ol</c>: Safari stops announcing a list once its bullets are removed, which Tailwind's base
/// styles always do. <see cref="TwBlazorComponentBase.Shadow"/> and <see cref="TwBlazorComponentBase.Rounded"/>
/// apply only when set, so a plain layout never picks up the theme's default card shadow or corners.
/// </remarks>
public abstract class TwLayoutComponentBase : TwBlazorComponentBase
{
    /// <summary>
    /// Gets or sets the HTML element to render. Defaults to <see cref="LayoutElement.Div"/>.
    /// </summary>
    /// <remarks>
    /// Pick the element that says what the content is, such as <see cref="LayoutElement.Ul"/> for a list of
    /// products or <see cref="LayoutElement.Section"/> for a band of the page, so screen readers can describe it.
    /// </remarks>
    [Parameter] public LayoutElement Element { get; set; } = LayoutElement.Div;

    /// <summary>
    /// Gets or sets the content laid out inside the element.
    /// </summary>
    [Parameter] public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Gets the classes for the rendered element, with <see cref="TwBlazorComponentBase.Class"/> merged last.
    /// </summary>
    protected abstract string rootClasses { get; }

    /// <summary>
    /// Gets the inline style for the rendered element. Defaults to <see cref="TwBlazorComponentBase.Style"/>.
    /// </summary>
    protected virtual string? rootStyle => Style;

    /// <summary>
    /// Gets the shadow and corner classes, present only when <see cref="TwBlazorComponentBase.Shadow"/> or
    /// <see cref="TwBlazorComponentBase.Rounded"/> is set.
    /// </summary>
    protected string decorationClasses => JoinClasses(
        Shadow is { } shadow ? shadowBuilder.GetShadow(shadow) : null,
        Rounded is { } rounded ? roundedBuilder.GetRounded(rounded) : null);

    /// <summary>
    /// Gets the shared flexbox utilities from the theme.
    /// </summary>
    protected TwBlazorFlexbox flexbox => options.Theme.Flexbox;

    /// <summary>
    /// Gets the shared grid utilities from the theme.
    /// </summary>
    protected TwBlazorGrid grid => options.Theme.Grid;

    /// <summary>
    /// Gets the shared spacing scales from the theme.
    /// </summary>
    protected TwBlazorSpacing spacing => options.Theme.Spacing;

    /// <inheritdoc />
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, TagName(Element));
        builder.AddAttribute(1, "id", Id);
        builder.AddAttribute(2, "class", rootClasses);
        builder.AddAttribute(3, "style", NullIfEmpty(rootStyle));
        builder.AddAttribute(4, "role", Element is LayoutElement.Ul or LayoutElement.Ol ? "list" : null);
        builder.AddAttribute(5, "aria-label", AriaLabel);
        builder.AddAttribute(6, "aria-labelledby", AriaLabelledBy);
        builder.AddMultipleAttributes(7, Attributes);
        BuildContent(builder);
        builder.CloseElement();
    }

    /// <summary>
    /// Renders the content inside the element. Override it to wrap <see cref="ChildContent"/>, for example in a
    /// cascading value; sequence numbers from 8 upwards are free.
    /// </summary>
    /// <param name="builder">The render tree builder, positioned inside the element.</param>
    protected virtual void BuildContent(RenderTreeBuilder builder) => builder.AddContent(8, ChildContent);

    /// <summary>
    /// Gets the tag name for an element type.
    /// </summary>
    /// <param name="element">The element type.</param>
    /// <returns>The lower case tag name, such as <c>section</c>.</returns>
    internal static string TagName(LayoutElement element) => element.ToString().ToLowerInvariant();

    /// <summary>
    /// Joins classes with single spaces, leaving out empty ones.
    /// </summary>
    /// <param name="classes">The classes to join.</param>
    /// <returns>The joined classes.</returns>
    protected static string JoinClasses(params string?[] classes) =>
        string.Join(' ', classes.Where(value => !string.IsNullOrWhiteSpace(value)));

    /// <summary>
    /// Joins CSS declarations with semicolons, leaving out empty ones.
    /// </summary>
    /// <param name="declarations">The declarations to join, such as <c>grid-area: main</c>.</param>
    /// <returns>The joined declarations, or <see langword="null"/> when there are none.</returns>
    protected static string? JoinStyles(params string?[] declarations)
    {
        var parts = declarations
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value!.Trim().TrimEnd(';').Trim())
            .Where(value => value.Length > 0)
            .ToList();

        return parts.Count == 0 ? null : string.Join("; ", parts);
    }

    /// <summary>
    /// Removes the characters that could end a CSS declaration early, so a value passed by a parameter stays
    /// inside its own declaration.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>The value without semicolons, braces or quotes, trimmed.</returns>
    protected static string CssValue(string value) =>
        new string([.. value.Where(character => character is not (';' or '{' or '}' or '"' or '\''))]).Trim();

    /// <summary>
    /// Gets the classes of a numbered responsive scale for each breakpoint that has a value.
    /// </summary>
    /// <param name="scale">The scale.</param>
    /// <param name="baseValue">The value at every width.</param>
    /// <param name="sm">The value from the <c>sm</c> breakpoint.</param>
    /// <param name="md">The value from the <c>md</c> breakpoint.</param>
    /// <param name="lg">The value from the <c>lg</c> breakpoint.</param>
    /// <param name="xl">The value from the <c>xl</c> breakpoint.</param>
    /// <param name="xl2">The value from the <c>2xl</c> breakpoint.</param>
    /// <returns>The joined classes.</returns>
    protected static string ResponsiveClasses(TwResponsiveScale scale, int? baseValue, int? sm, int? md, int? lg, int? xl, int? xl2) =>
        JoinClasses(
            scale.Get(baseValue, Breakpoint.Base),
            scale.Get(sm, Breakpoint.Sm),
            scale.Get(md, Breakpoint.Md),
            scale.Get(lg, Breakpoint.Lg),
            scale.Get(xl, Breakpoint.Xl),
            scale.Get(xl2, Breakpoint.Xl2));

    /// <summary>
    /// Gets the class for a main-axis distribution.
    /// </summary>
    /// <param name="value">The distribution, or <see langword="null"/> for none.</param>
    /// <returns>The class, or an empty string.</returns>
    protected string JustifyContentClass(JustifyContent? value) => value switch
    {
        JustifyContent.Start => flexbox.Justify.Start,
        JustifyContent.Center => flexbox.Justify.Center,
        JustifyContent.End => flexbox.Justify.End,
        JustifyContent.Between => flexbox.Justify.Between,
        JustifyContent.Around => flexbox.Justify.Around,
        JustifyContent.Evenly => flexbox.Justify.Evenly,
        JustifyContent.Stretch => flexbox.Justify.Stretch,
        _ => string.Empty
    };

    /// <summary>
    /// Gets the class for a cross-axis alignment.
    /// </summary>
    /// <param name="value">The alignment, or <see langword="null"/> for none.</param>
    /// <returns>The class, or an empty string.</returns>
    protected string AlignItemsClass(AlignItems? value) => value switch
    {
        AlignItems.Start => flexbox.Align.Start,
        AlignItems.Center => flexbox.Align.Center,
        AlignItems.End => flexbox.Align.End,
        AlignItems.Stretch => flexbox.Align.Stretch,
        AlignItems.Baseline => flexbox.Align.Baseline,
        _ => string.Empty
    };

    /// <summary>
    /// Gets the class for a distribution of lines or tracks on the cross axis.
    /// </summary>
    /// <param name="value">The distribution, or <see langword="null"/> for none.</param>
    /// <returns>The class, or an empty string.</returns>
    protected string AlignContentClass(AlignContent? value) => value switch
    {
        AlignContent.Start => flexbox.AlignContent.Start,
        AlignContent.Center => flexbox.AlignContent.Center,
        AlignContent.End => flexbox.AlignContent.End,
        AlignContent.Between => flexbox.AlignContent.Between,
        AlignContent.Around => flexbox.AlignContent.Around,
        AlignContent.Evenly => flexbox.AlignContent.Evenly,
        AlignContent.Stretch => flexbox.AlignContent.Stretch,
        _ => string.Empty
    };

    /// <summary>
    /// Gets the class for one item's cross-axis alignment.
    /// </summary>
    /// <param name="value">The alignment, or <see langword="null"/> for none.</param>
    /// <returns>The class, or an empty string.</returns>
    protected string AlignSelfClass(AlignSelf? value) => value switch
    {
        AlignSelf.Start => flexbox.AlignSelf.Start,
        AlignSelf.Center => flexbox.AlignSelf.Center,
        AlignSelf.End => flexbox.AlignSelf.End,
        AlignSelf.Stretch => flexbox.AlignSelf.Stretch,
        AlignSelf.Baseline => flexbox.AlignSelf.Baseline,
        _ => string.Empty
    };

    /// <summary>
    /// Gets the class for an item's visual order.
    /// </summary>
    /// <param name="order">The position, <see cref="ItemOrder.First"/>, <see cref="ItemOrder.Last"/>, or <see langword="null"/> for none.</param>
    /// <returns>The class, or an empty string when the position is not on the theme's scale.</returns>
    protected string OrderClass(int? order) => order switch
    {
        ItemOrder.First => flexbox.OrderFirst,
        ItemOrder.Last => flexbox.OrderLast,
        _ => flexbox.Order.Get(order)
    };

    private static string? NullIfEmpty(string? value) => string.IsNullOrWhiteSpace(value) ? null : value;
}
