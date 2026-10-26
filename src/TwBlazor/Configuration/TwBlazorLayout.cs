// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using TwBlazor.Configuration.Color;

namespace TwBlazor.Configuration;

/// <summary>
/// Global configuration for CSS display utility classes (e.g. <c>block</c>, <c>flex</c>) shared by
/// all components. Each property holds a single, complete Tailwind class so it can be referenced
/// from component themes or from a component's own markup without concatenation.
/// </summary>
[ExcludeFromCodeCoverage]
public class TwBlazorDisplay
{
    /// <summary>
    /// Gets or sets the "block" display class.
    /// </summary>
    public string Block { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the "inline-block" display class.
    /// </summary>
    public string InlineBlock { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the "flex" display class.
    /// </summary>
    public string Flex { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the "inline-flex" display class.
    /// </summary>
    public string InlineFlex { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the "grid" display class.
    /// </summary>
    public string Grid { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the "inline-grid" display class.
    /// </summary>
    public string InlineGrid { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the "hidden" display class.
    /// </summary>
    public string Hidden { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the "contents" display class.
    /// </summary>
    public string Contents { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the visually-hidden-but-accessible ("sr-only") class.
    /// </summary>
    public string ScreenReaderOnly { get; set; } = string.Empty;
}

/// <summary>
/// A reusable alignment scale for a single flexbox/grid alignment axis (e.g. <c>justify-content</c>,
/// <c>align-items</c>). Not every axis supports every value - for example <c>justify-content</c> has
/// no "baseline" - so leave the classes that don't apply to a given axis unset.
/// </summary>
[ExcludeFromCodeCoverage]
public class TwFlexAlignmentScale
{
    /// <summary>
    /// Gets or sets the class that aligns items to the start of the axis.
    /// </summary>
    public string Start { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the class that aligns items to the center of the axis.
    /// </summary>
    public string Center { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the class that aligns items to the end of the axis.
    /// </summary>
    public string End { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the class that distributes items with space between them.
    /// </summary>
    public string Between { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the class that distributes items with space around them.
    /// </summary>
    public string Around { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the class that distributes items with even space around them.
    /// </summary>
    public string Evenly { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the class that stretches items to fill the axis.
    /// </summary>
    public string Stretch { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the class that aligns items to their baseline.
    /// </summary>
    public string Baseline { get; set; } = string.Empty;
}

/// <summary>
/// Global configuration for the flexbox utility classes shared by all components: direction,
/// wrapping, grow/shrink, and the justify/align/align-content/align-self alignment scales. See
/// <see cref="TwFlexAlignmentScale"/> for the shape of each alignment axis.
/// </summary>
[ExcludeFromCodeCoverage]
public class TwBlazorFlexbox
{
    /// <summary>
    /// Gets or sets the "flex-row" direction class.
    /// </summary>
    public string Row { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the "flex-row-reverse" direction class.
    /// </summary>
    public string RowReverse { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the "flex-col" direction class.
    /// </summary>
    public string Col { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the "flex-col-reverse" direction class.
    /// </summary>
    public string ColReverse { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the "flex-wrap" wrapping class.
    /// </summary>
    public string Wrap { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the "flex-nowrap" wrapping class.
    /// </summary>
    public string NoWrap { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the "flex-wrap-reverse" wrapping class.
    /// </summary>
    public string WrapReverse { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the class that allows a flex item to grow.
    /// </summary>
    public string Grow { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the class that prevents a flex item from growing.
    /// </summary>
    public string GrowNone { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the class that allows a flex item to shrink.
    /// </summary>
    public string Shrink { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the class that prevents a flex item from shrinking.
    /// </summary>
    public string ShrinkNone { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the "flex-1" shorthand class (grow, shrink, and a zero flex-basis).
    /// </summary>
    public string Flex1 { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the justify-content alignment scale (main-axis alignment).
    /// </summary>
    public TwFlexAlignmentScale Justify { get; set; } = new();

    /// <summary>
    /// Gets or sets the align-items alignment scale (cross-axis alignment).
    /// </summary>
    public TwFlexAlignmentScale Align { get; set; } = new();

    /// <summary>
    /// Gets or sets the align-content alignment scale (multi-line cross-axis alignment).
    /// </summary>
    public TwFlexAlignmentScale AlignContent { get; set; } = new();

    /// <summary>
    /// Gets or sets the align-self alignment scale (per-item cross-axis override).
    /// </summary>
    public TwFlexAlignmentScale AlignSelf { get; set; } = new();

    /// <summary>
    /// Gets or sets the direction classes written out at every breakpoint, for layouts that change direction
    /// as the screen widens. See <see cref="TwResponsiveFlexDirection"/>.
    /// </summary>
    public TwResponsiveFlexDirection ResponsiveDirection { get; set; } = new();

    /// <summary>
    /// Gets or sets the flex basis classes (<c>basis-*</c>). See <see cref="TwFlexBasisScale"/>.
    /// </summary>
    public TwFlexBasisScale Basis { get; set; } = new();

    /// <summary>
    /// Gets or sets the "flex-auto" class: grow and shrink from the item's content size.
    /// </summary>
    public string FlexAuto { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the "flex-initial" class: shrink but never grow.
    /// </summary>
    public string FlexInitial { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the "flex-none" class: neither grow nor shrink.
    /// </summary>
    public string FlexNone { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the visual order classes (<c>order-*</c>), keyed by position. Shared by flex and grid items.
    /// </summary>
    public TwClassScale Order { get; set; } = [];

    /// <summary>
    /// Gets or sets the "order-first" class.
    /// </summary>
    public string OrderFirst { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the "order-last" class.
    /// </summary>
    public string OrderLast { get; set; } = string.Empty;
}

/// <summary>
/// Global configuration for spacing utility classes shared by all components. Each scale is keyed by its step on
/// Tailwind's spacing scale, so <c>Gap[4]</c> holds <c>gap-4</c> and <c>Padding[0.5]</c> holds <c>p-0.5</c>, and
/// every class is written out once.
/// </summary>
[ExcludeFromCodeCoverage]
public class TwBlazorSpacing
{
    /// <summary>
    /// Gets or sets the gap between rows and columns (<c>gap-*</c>), keyed by step. See <see cref="TwSpacingStepScale"/>.
    /// </summary>
    public TwSpacingStepScale Gap { get; set; } = [];

    /// <summary>
    /// Gets or sets the gap between columns (<c>gap-x-*</c>), keyed by step. See <see cref="TwSpacingStepScale"/>.
    /// </summary>
    public TwSpacingStepScale GapX { get; set; } = [];

    /// <summary>
    /// Gets or sets the gap between rows (<c>gap-y-*</c>), keyed by step. See <see cref="TwSpacingStepScale"/>.
    /// </summary>
    public TwSpacingStepScale GapY { get; set; } = [];

    /// <summary>
    /// Gets or sets the padding on every side (<c>p-*</c>), keyed by step. See <see cref="TwSpacingStepScale"/>.
    /// </summary>
    public TwSpacingStepScale Padding { get; set; } = [];

    /// <summary>
    /// Gets or sets the padding on the start and end (<c>px-*</c>), keyed by step. See <see cref="TwSpacingStepScale"/>.
    /// </summary>
    public TwSpacingStepScale PaddingX { get; set; } = [];

    /// <summary>
    /// Gets or sets the padding on the top and bottom (<c>py-*</c>), keyed by step. See <see cref="TwSpacingStepScale"/>.
    /// </summary>
    public TwSpacingStepScale PaddingY { get; set; } = [];

    /// <summary>
    /// Gets or sets the padding on the top (<c>pt-*</c>), keyed by step. See <see cref="TwSpacingStepScale"/>.
    /// </summary>
    public TwSpacingStepScale PaddingTop { get; set; } = [];

    /// <summary>
    /// Gets or sets the padding on the bottom (<c>pb-*</c>), keyed by step. See <see cref="TwSpacingStepScale"/>.
    /// </summary>
    public TwSpacingStepScale PaddingBottom { get; set; } = [];

    /// <summary>
    /// Gets or sets the padding on the start, for example to clear a leading icon (<c>ps-*</c>), keyed by step. See <see cref="TwSpacingStepScale"/>.
    /// </summary>
    public TwSpacingStepScale PaddingStart { get; set; } = [];

    /// <summary>
    /// Gets or sets the margin on every side (<c>m-*</c>), keyed by step. See <see cref="TwSpacingStepScale"/>.
    /// </summary>
    public TwSpacingStepScale Margin { get; set; } = [];

    /// <summary>
    /// Gets or sets the margin on the top (<c>mt-*</c>), keyed by step. See <see cref="TwSpacingStepScale"/>.
    /// </summary>
    public TwSpacingStepScale MarginTop { get; set; } = [];

    /// <summary>
    /// Gets or sets the margin on the bottom (<c>mb-*</c>), keyed by step. See <see cref="TwSpacingStepScale"/>.
    /// </summary>
    public TwSpacingStepScale MarginBottom { get; set; } = [];

    /// <summary>
    /// Gets or sets the margin on the start (<c>ms-*</c>), keyed by step. See <see cref="TwSpacingStepScale"/>.
    /// </summary>
    public TwSpacingStepScale MarginStart { get; set; } = [];

    /// <summary>
    /// Gets or sets the margin on the end (<c>me-*</c>), keyed by step. See <see cref="TwSpacingStepScale"/>.
    /// </summary>
    public TwSpacingStepScale MarginEnd { get; set; } = [];

    /// <summary>
    /// Gets or sets the padding preset used for interactive list/menu rows.
    /// </summary>
    public string InteractiveRowPadding { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the "ms-auto" class used to push an element to the end of its flex row.
    /// </summary>
    public string PushEnd { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the "mt-auto" class used to push an element to the bottom of its flex column.
    /// </summary>
    public string PushBottom { get; set; } = string.Empty;
}

/// <summary>
/// Global configuration for interaction-state classes (cursors and disabled opacity) shared by all components.
/// </summary>
[ExcludeFromCodeCoverage]
public class TwBlazorInteraction
{
    /// <summary>
    /// Gets or sets the opacity applied to disabled elements.
    /// </summary>
    public string DisabledOpacity { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the cursor for interactive/clickable elements.
    /// </summary>
    public string PointerCursor { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the cursor for disabled elements.
    /// </summary>
    public string DisabledCursor { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the cursor for readonly elements.
    /// </summary>
    public string ReadonlyCursor { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the "pointer-events-none" class used to make an element ignore pointer input.
    /// </summary>
    public string PointerEventsNone { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the "focus:outline-none" class used to suppress the native focus ring in favour of a custom one.
    /// </summary>
    public string FocusOutlineNone { get; set; } = string.Empty;
}

/// <summary>
/// Global configuration for border-width utility classes, used by <see cref="TwBlazorBorder.Width"/>.
/// Kept separate from border color so a width and a color can be mixed and matched.
/// </summary>
[ExcludeFromCodeCoverage]
public class TwBlazorBorderWidth
{
    /// <summary>
    /// Gets or sets the "border-0" (no border) width class.
    /// </summary>
    public string None { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the default 1px "border" width class.
    /// </summary>
    public string Thin { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the default 1.5px "border" width class.
    /// </summary>
    public string Medium { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the 2px "border-2" width class.
    /// </summary>
    public string Thick { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the thick leading-edge border class used for accent bars (e.g. alerts, toasts).
    /// </summary>
    public string AccentEdge { get; set; } = string.Empty;
}

/// <summary>
/// A reusable set of neutral (gray-scale) border tones, mirroring the border-related members of
/// <see cref="Color.TwSurfacePalette"/> so they can also be reached from <see cref="TwBlazorBorder.Neutral"/>
/// alongside the semantic <see cref="TwBlazorBorder.Colors"/> and the shared <see cref="TwBlazorBorder.Width"/>.
/// </summary>
[ExcludeFromCodeCoverage]
public class TwBorderNeutralTones
{
    /// <summary>
    /// Gets or sets the standard neutral border tone (mirrors <see cref="Color.TwSurfacePalette.Border"/>).
    /// </summary>
    public string Base { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the subtle neutral border tone (mirrors <see cref="Color.TwSurfacePalette.BorderSubtle"/>).
    /// </summary>
    public string Subtle { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the strong neutral border tone (mirrors <see cref="Color.TwSurfacePalette.BorderStrong"/>).
    /// </summary>
    public string Strong { get; set; } = string.Empty;
}

/// <summary>
/// The single combined home for everything border-related: width, semantic color, and neutral
/// (gray-scale) color. Compose a class by picking one width and one color, e.g.
/// <c>$"{Border.Width.AccentEdge} {Border.Colors.Primary}"</c> or <c>$"{Border.Width.Thin} {Border.Neutral.Base}"</c>.
/// </summary>
[ExcludeFromCodeCoverage]
public class TwBlazorBorder
{
    /// <summary>
    /// Gets or sets the border-width scale. See <see cref="TwBlazorBorderWidth"/>.
    /// </summary>
    public TwBlazorBorderWidth Width { get; set; } = new();

    /// <summary>
    /// Gets or sets the semantic (Primary/Accent/.../Dark) border color palette.
    /// </summary>
    public TwBlazorPalette Colors { get; set; } = new();

    /// <summary>
    /// Gets or sets the neutral (gray-scale) border tones. See <see cref="TwBorderNeutralTones"/>.
    /// </summary>
    public TwBorderNeutralTones Neutral { get; set; } = new();
}

/// <summary>
/// Global configuration for the CSS "position" utility classes (e.g. <c>absolute</c>, <c>relative</c>)
/// shared by all components. For anchoring a floating element to a spot on screen (e.g. a dialog or
/// toast corner), see <see cref="TwAnchorPosition"/> instead.
/// </summary>
[ExcludeFromCodeCoverage]
public class TwBlazorPositioning
{
    /// <summary>
    /// Gets or sets the "static" position class.
    /// </summary>
    public string Static { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the "relative" position class.
    /// </summary>
    public string Relative { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the "absolute" position class.
    /// </summary>
    public string Absolute { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the "fixed" position class.
    /// </summary>
    public string Fixed { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the "sticky" position class.
    /// </summary>
    public string Sticky { get; set; } = string.Empty;
}

/// <summary>
/// Global configuration for CSS inset utility classes (Tailwind's <c>top-*</c>/<c>bottom-*</c>/<c>start-*</c>/<c>end-*</c>),
/// used to pin a <c>relative</c>/<c>absolute</c>-positioned element to an edge of its container. Kept
/// separate from <see cref="TwBlazorPositioning"/>, which is only the CSS "position" mode itself
/// (<c>relative</c>, <c>absolute</c>, etc.) - insets are a distinct CSS concern usually combined with it.
/// </summary>
[ExcludeFromCodeCoverage]
public class TwBlazorInset
{
    /// <summary>
    /// Gets or sets the "top-0" inset class.
    /// </summary>
    public string Top { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the "bottom-0" inset class.
    /// </summary>
    public string Bottom { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the "start-0" inset class.
    /// </summary>
    public string Start { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the "end-0" inset class.
    /// </summary>
    public string End { get; set; } = string.Empty;
}

/// <summary>
/// Global configuration for CSS transition utility classes shared by all components: the property
/// being transitioned, the duration scale, and the common property+duration combinations most
/// components reach for. Compose a one-off combination from <see cref="Colors"/>/<see cref="Transform"/>,
/// <see cref="DurationFast"/>/<see cref="DurationSlow"/>, and <see cref="EaseInOut"/> if none of the
/// pre-combined properties fit.
/// </summary>
[ExcludeFromCodeCoverage]
public class TwBlazorTransition
{
    /// <summary>
    /// Gets or sets the "transition-colors" property class, with no duration applied.
    /// </summary>
    public string Colors { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the "transition-transform" property class, with no duration applied.
    /// </summary>
    public string Transform { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the "ease-in-out" timing-function class.
    /// </summary>
    public string EaseInOut { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the fast "duration-200" duration class, used for most interactive state changes.
    /// </summary>
    public string DurationFast { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the slow "duration-300" duration class, used for larger movements (panels, indicators).
    /// </summary>
    public string DurationSlow { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the combined "transition-colors" + <see cref="DurationFast"/> class - the most common color transition across interactive components.
    /// </summary>
    public string ColorsFast { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the combined "transition-colors" + <see cref="DurationSlow"/> class.
    /// </summary>
    public string ColorsSlow { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the combined "transition-transform" + <see cref="DurationFast"/> class.
    /// </summary>
    public string TransformFast { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the combined "transition-transform" + <see cref="DurationSlow"/> class.
    /// </summary>
    public string TransformSlow { get; set; } = string.Empty;
}

/// <summary>
/// A reusable font-size scale (Tailwind's "text-*" size classes), used by <see cref="TwBlazorTypography.Size"/>.
/// </summary>
[ExcludeFromCodeCoverage]
public class TwBlazorFontSizeScale
{
    /// <summary>
    /// Gets or sets the smallest text size class, for dense captions such as calendar labels and small chips.
    /// </summary>
    public string Xxs { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the extra-small text size class.
    /// </summary>
    public string Xs { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the small text size class.
    /// </summary>
    public string Sm { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the base (default) text size class.
    /// </summary>
    public string Base { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the large text size class.
    /// </summary>
    public string Lg { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the extra-large text size class.
    /// </summary>
    public string Xl { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the 2xl text size class.
    /// </summary>
    public string Xl2 { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the 3xl text size class.
    /// </summary>
    public string Xl3 { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the 4xl text size class.
    /// </summary>
    public string Xl4 { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the 5xl text size class.
    /// </summary>
    public string Xl5 { get; set; } = string.Empty;
}

/// <summary>
/// A reusable font-weight scale (Tailwind's "font-*" weight classes), used by <see cref="TwBlazorTypography.Weight"/>.
/// </summary>
[ExcludeFromCodeCoverage]
public class TwBlazorFontWeightScale
{
    /// <summary>
    /// Gets or sets the normal font-weight class.
    /// </summary>
    public string Normal { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the medium font-weight class.
    /// </summary>
    public string Medium { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the semibold font-weight class.
    /// </summary>
    public string Semibold { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the bold font-weight class.
    /// </summary>
    public string Bold { get; set; } = string.Empty;
}

/// <summary>
/// Global configuration for shared typography utility classes (font-size scale, font-weight scale,
/// text alignment and truncation) reused across component themes.
/// </summary>
[ExcludeFromCodeCoverage]
public class TwBlazorTypography
{
    /// <summary>
    /// Gets or sets the font-size scale. See <see cref="TwBlazorFontSizeScale"/>.
    /// </summary>
    public TwBlazorFontSizeScale Size { get; set; } = new();

    /// <summary>
    /// Gets or sets the font-weight scale. See <see cref="TwBlazorFontWeightScale"/>.
    /// </summary>
    public TwBlazorFontWeightScale Weight { get; set; } = new();

    /// <summary>
    /// Gets or sets the "text-center" text-alignment class.
    /// </summary>
    public string AlignCenter { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the "wrap-break-word" text-alignment class.
    /// </summary>
    public string WrapBreakWord { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the class that truncates text with an ellipsis when it overflows.
    /// </summary>
    public string Truncate { get; set; } = string.Empty;
}

/// <summary>
/// Global configuration for CSS text-transform utility classes shared by all components.
/// </summary>
[ExcludeFromCodeCoverage]
public class TwBlazorTextTransform
{
    /// <summary>
    /// Gets or sets the "uppercase" text-transform class.
    /// </summary>
    public string Uppercase { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the "lowercase" text-transform class.
    /// </summary>
    public string Lowercase { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the "capitalize" text-transform class.
    /// </summary>
    public string Capitalize { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the "normal-case" (revert transform) text-transform class.
    /// </summary>
    public string NormalCase { get; set; } = string.Empty;
}

/// <summary>
/// A reusable scale of square icon sizes (equal width and height, e.g. Tailwind's <c>size-*</c>
/// shorthand), used by <see cref="TwBlazorSizing.Icon"/>.
/// </summary>
[ExcludeFromCodeCoverage]
public class TwBlazorIconSize
{
    /// <summary>
    /// Gets or sets the extra-small icon size class.
    /// </summary>
    public string Xs { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the small icon size class.
    /// </summary>
    public string Sm { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the medium icon size class.
    /// </summary>
    public string Md { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the large icon size class.
    /// </summary>
    public string Lg { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the extra-large icon size class.
    /// </summary>
    public string Xl { get; set; } = string.Empty;
}

/// <summary>
/// Global configuration for shared width/height utility classes reused by all components, so
/// "w-full", "h-full", and common square icon sizes aren't retyped in every component theme.
/// </summary>
[ExcludeFromCodeCoverage]
public class TwBlazorSizing
{


    /// <summary>
    /// Gets or sets the "w-full" (100% width) class.
    /// </summary>
    public string FullWidth { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the "h-full" (100% height) class.
    /// </summary>
    public string FullHeight { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the combined 100% width and height classes.
    /// </summary>
    public string Full { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the "min-w-0" class used to let a flex/grid child shrink below its content size.
    /// </summary>
    public string MinWidthNone { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the reusable square icon size scale. See <see cref="TwBlazorIconSize"/>.
    /// </summary>
    public TwBlazorIconSize Icon { get; set; } = new();

    /// <summary>
    /// Gets or sets the maximum width classes (<c>max-w-*</c>), one per <see cref="Enums.ContainerSize"/>.
    /// See <see cref="TwMaxWidthScale"/>.
    /// </summary>
    public TwMaxWidthScale MaxWidth { get; set; } = new();
}

/// <summary>
/// A reusable three-step height scale, used by component themes that need a small/medium/large
/// fixed-height preset (e.g. a size variant that isn't just icon-driven).
/// </summary>
[ExcludeFromCodeCoverage]
public class TwBlazorHeight
{
    /// <summary>
    /// Gets or sets the "sm" height class.
    /// </summary>
    public string Sm { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the "md" height class.
    /// </summary>
    public string Md { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the "lg" height class.
    /// </summary>
    public string Lg { get; set; } = string.Empty;
}

/// <summary>
/// A reusable three-step width scale, used by component themes that need a small/medium/large
/// fixed-width preset.
/// </summary>
[ExcludeFromCodeCoverage]
public class TwBlazorWidth
{
    /// <summary>
    /// Gets or sets the "sm" width class.
    /// </summary>
    public string Sm { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the "md" width class.
    /// </summary>
    public string Md { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the "lg" width class.
    /// </summary>
    public string Lg { get; set; } = string.Empty;
}

/// <summary>
/// Global configuration for CSS overflow utility classes shared by all components.
/// </summary>
[ExcludeFromCodeCoverage]
public class TwBlazorOverflow
{
    /// <summary>
    /// Gets or sets the "overflow-hidden" class.
    /// </summary>
    public string Hidden { get; set; } = string.Empty;
}
