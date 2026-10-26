// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

namespace TwBlazor.Enums;

/// <summary>
/// The screen widths a responsive layout value can change at, matching Tailwind's default breakpoints.
/// </summary>
public enum Breakpoint
{
    /// <summary>
    /// Every screen width, with no breakpoint prefix.
    /// </summary>
    Base,

    /// <summary>
    /// 40rem (640px) and wider, Tailwind's <c>sm:</c> prefix.
    /// </summary>
    Sm,

    /// <summary>
    /// 48rem (768px) and wider, Tailwind's <c>md:</c> prefix.
    /// </summary>
    Md,

    /// <summary>
    /// 64rem (1024px) and wider, Tailwind's <c>lg:</c> prefix.
    /// </summary>
    Lg,

    /// <summary>
    /// 80rem (1280px) and wider, Tailwind's <c>xl:</c> prefix.
    /// </summary>
    Xl,

    /// <summary>
    /// 96rem (1536px) and wider, Tailwind's <c>2xl:</c> prefix.
    /// </summary>
    Xl2
}

/// <summary>
/// The HTML element a layout component renders, so the layout can carry the right meaning for assistive technology.
/// </summary>
public enum LayoutElement
{
    /// <summary>
    /// A <c>div</c>, which has no meaning of its own.
    /// </summary>
    Div,

    /// <summary>
    /// A <c>section</c>. It becomes a region landmark once it has an accessible name.
    /// </summary>
    Section,

    /// <summary>
    /// An <c>article</c>, for self-contained content such as a product or a post.
    /// </summary>
    Article,

    /// <summary>
    /// A <c>main</c> landmark. A page should have only one.
    /// </summary>
    Main,

    /// <summary>
    /// An <c>aside</c> (complementary landmark), for content related to the main content.
    /// </summary>
    Aside,

    /// <summary>
    /// A <c>header</c>.
    /// </summary>
    Header,

    /// <summary>
    /// A <c>footer</c>.
    /// </summary>
    Footer,

    /// <summary>
    /// A <c>nav</c> landmark. Give it an accessible name when a page has more than one.
    /// </summary>
    Nav,

    /// <summary>
    /// A <c>ul</c> (unordered list). Its children should be list items.
    /// </summary>
    Ul,

    /// <summary>
    /// An <c>ol</c> (ordered list). Its children should be list items.
    /// </summary>
    Ol,

    /// <summary>
    /// An <c>li</c> (list item), for an item inside a list layout.
    /// </summary>
    Li,

    /// <summary>
    /// A <c>dl</c> (description list).
    /// </summary>
    Dl,

    /// <summary>
    /// A <c>form</c>.
    /// </summary>
    Form,

    /// <summary>
    /// A <c>fieldset</c>, for a group of related form controls.
    /// </summary>
    Fieldset,

    /// <summary>
    /// A <c>span</c>, for an inline layout inside text.
    /// </summary>
    Span,

    /// <summary>
    /// A <c>p</c> (paragraph).
    /// </summary>
    P,

    /// <summary>
    /// An <c>h1</c>, the page's main heading.
    /// </summary>
    H1,

    /// <summary>
    /// An <c>h2</c>, a section heading.
    /// </summary>
    H2,

    /// <summary>
    /// An <c>h3</c>, a subsection heading.
    /// </summary>
    H3,

    /// <summary>
    /// An <c>h4</c>.
    /// </summary>
    H4,

    /// <summary>
    /// An <c>h5</c>.
    /// </summary>
    H5,

    /// <summary>
    /// An <c>h6</c>.
    /// </summary>
    H6,

    /// <summary>
    /// A <c>label</c>, for the caption of a form control.
    /// </summary>
    Label,

    /// <summary>
    /// A <c>strong</c>, for text with strong importance.
    /// </summary>
    Strong,

    /// <summary>
    /// An <c>em</c>, for text with stress emphasis.
    /// </summary>
    Em,

    /// <summary>
    /// A <c>small</c>, for side comments and fine print.
    /// </summary>
    Small
}

/// <summary>
/// The maximum width of a <see cref="Components.TwContainer"/>, matching Tailwind's <c>max-w-*</c> sizes.
/// </summary>
public enum ContainerSize
{
    /// <summary>
    /// 20rem (320px).
    /// </summary>
    Xs,

    /// <summary>
    /// 24rem (384px).
    /// </summary>
    Sm,

    /// <summary>
    /// 28rem (448px).
    /// </summary>
    Md,

    /// <summary>
    /// 32rem (512px).
    /// </summary>
    Lg,

    /// <summary>
    /// 36rem (576px).
    /// </summary>
    Xl,

    /// <summary>
    /// 42rem (672px).
    /// </summary>
    Xl2,

    /// <summary>
    /// 48rem (768px).
    /// </summary>
    Xl3,

    /// <summary>
    /// 56rem (896px).
    /// </summary>
    Xl4,

    /// <summary>
    /// 64rem (1024px).
    /// </summary>
    Xl5,

    /// <summary>
    /// 72rem (1152px).
    /// </summary>
    Xl6,

    /// <summary>
    /// 80rem (1280px).
    /// </summary>
    Xl7,

    /// <summary>
    /// About 65 characters, a comfortable line length for reading.
    /// </summary>
    Prose,

    /// <summary>
    /// The full width of the parent.
    /// </summary>
    Full,

    /// <summary>
    /// No maximum width.
    /// </summary>
    None
}

/// <summary>
/// The background a <see cref="Components.TwContainer"/> paints behind its content.
/// </summary>
public enum ContainerSurface
{
    /// <summary>
    /// No background: the page shows through.
    /// </summary>
    None,

    /// <summary>
    /// The theme's standard surface, the same as a card.
    /// </summary>
    Default,

    /// <summary>
    /// The theme's subtle surface, a quiet tint for a band of the page.
    /// </summary>
    Subtle,

    /// <summary>
    /// The theme's elevated surface, a step up from <see cref="Default"/>.
    /// </summary>
    Elevated
}

/// <summary>
/// The direction a <see cref="Components.TwFlex"/> lays out its children in.
/// </summary>
public enum FlexDirection
{
    /// <summary>
    /// Side by side in reading order (<c>flex-row</c>).
    /// </summary>
    Row,

    /// <summary>
    /// Side by side, last child first (<c>flex-row-reverse</c>). Only the visual order changes.
    /// </summary>
    RowReverse,

    /// <summary>
    /// Stacked top to bottom (<c>flex-col</c>).
    /// </summary>
    Col,

    /// <summary>
    /// Stacked bottom to top (<c>flex-col-reverse</c>). Only the visual order changes.
    /// </summary>
    ColReverse
}

/// <summary>
/// Whether the children of a <see cref="Components.TwFlex"/> wrap onto new lines.
/// </summary>
public enum FlexWrap
{
    /// <summary>
    /// Keeps every child on one line (<c>flex-nowrap</c>).
    /// </summary>
    NoWrap,

    /// <summary>
    /// Wraps children onto new lines (<c>flex-wrap</c>).
    /// </summary>
    Wrap,

    /// <summary>
    /// Wraps children onto new lines above the first (<c>flex-wrap-reverse</c>).
    /// </summary>
    WrapReverse
}

/// <summary>
/// How a <see cref="Components.TwFlexItem"/> grows and shrinks, matching Tailwind's <c>flex-*</c> shorthands.
/// </summary>
public enum FlexSize
{
    /// <summary>
    /// Grows and shrinks from its content size (<c>flex-auto</c>).
    /// </summary>
    Auto,

    /// <summary>
    /// Shrinks but does not grow (<c>flex-initial</c>).
    /// </summary>
    Initial,

    /// <summary>
    /// Neither grows nor shrinks (<c>flex-none</c>).
    /// </summary>
    None,

    /// <summary>
    /// Takes an equal share of the free space, ignoring its content size (<c>flex-1</c>).
    /// </summary>
    Fill
}

/// <summary>
/// The starting size of a <see cref="Components.TwFlexItem"/> along the main axis, before free space is shared out.
/// </summary>
public enum FlexBasis
{
    /// <summary>
    /// The item's own size (<c>basis-auto</c>).
    /// </summary>
    Auto,

    /// <summary>
    /// The full main axis (<c>basis-full</c>).
    /// </summary>
    Full,

    /// <summary>
    /// Half the main axis (<c>basis-1/2</c>).
    /// </summary>
    Half,

    /// <summary>
    /// A third of the main axis (<c>basis-1/3</c>).
    /// </summary>
    Third,

    /// <summary>
    /// Two thirds of the main axis (<c>basis-2/3</c>).
    /// </summary>
    TwoThirds,

    /// <summary>
    /// A quarter of the main axis (<c>basis-1/4</c>).
    /// </summary>
    Quarter,

    /// <summary>
    /// Three quarters of the main axis (<c>basis-3/4</c>).
    /// </summary>
    ThreeQuarters
}

/// <summary>
/// How children are spaced along the main axis (<c>justify-content</c>).
/// </summary>
public enum JustifyContent
{
    /// <summary>
    /// Packed at the start (<c>justify-start</c>).
    /// </summary>
    Start,

    /// <summary>
    /// Packed in the center (<c>justify-center</c>).
    /// </summary>
    Center,

    /// <summary>
    /// Packed at the end (<c>justify-end</c>).
    /// </summary>
    End,

    /// <summary>
    /// The first and last at the edges, with equal space between the rest (<c>justify-between</c>).
    /// </summary>
    Between,

    /// <summary>
    /// Equal space around each child (<c>justify-around</c>).
    /// </summary>
    Around,

    /// <summary>
    /// Equal space between every child and the edges (<c>justify-evenly</c>).
    /// </summary>
    Evenly,

    /// <summary>
    /// Stretched to fill the axis where the layout allows it (<c>justify-stretch</c>).
    /// </summary>
    Stretch
}

/// <summary>
/// How children are aligned on the cross axis (<c>align-items</c>).
/// </summary>
public enum AlignItems
{
    /// <summary>
    /// Aligned to the start (<c>items-start</c>).
    /// </summary>
    Start,

    /// <summary>
    /// Centered (<c>items-center</c>).
    /// </summary>
    Center,

    /// <summary>
    /// Aligned to the end (<c>items-end</c>).
    /// </summary>
    End,

    /// <summary>
    /// Stretched to fill (<c>items-stretch</c>), the browser default.
    /// </summary>
    Stretch,

    /// <summary>
    /// Aligned so their text baselines line up (<c>items-baseline</c>).
    /// </summary>
    Baseline
}

/// <summary>
/// How wrapped lines or grid tracks are spaced on the cross axis (<c>align-content</c>).
/// </summary>
public enum AlignContent
{
    /// <summary>
    /// Packed at the start (<c>content-start</c>).
    /// </summary>
    Start,

    /// <summary>
    /// Packed in the center (<c>content-center</c>).
    /// </summary>
    Center,

    /// <summary>
    /// Packed at the end (<c>content-end</c>).
    /// </summary>
    End,

    /// <summary>
    /// The first and last at the edges, with equal space between (<c>content-between</c>).
    /// </summary>
    Between,

    /// <summary>
    /// Equal space around each line (<c>content-around</c>).
    /// </summary>
    Around,

    /// <summary>
    /// Equal space between every line and the edges (<c>content-evenly</c>).
    /// </summary>
    Evenly,

    /// <summary>
    /// Stretched to fill (<c>content-stretch</c>).
    /// </summary>
    Stretch
}

/// <summary>
/// How a single item is aligned on the cross axis, overriding its container (<c>align-self</c>).
/// </summary>
public enum AlignSelf
{
    /// <summary>
    /// Aligned to the start (<c>self-start</c>).
    /// </summary>
    Start,

    /// <summary>
    /// Centered (<c>self-center</c>).
    /// </summary>
    Center,

    /// <summary>
    /// Aligned to the end (<c>self-end</c>).
    /// </summary>
    End,

    /// <summary>
    /// Stretched to fill (<c>self-stretch</c>).
    /// </summary>
    Stretch,

    /// <summary>
    /// Aligned by its text baseline (<c>self-baseline</c>).
    /// </summary>
    Baseline
}

/// <summary>
/// How grid items are aligned inside their cells on the row axis (<c>justify-items</c>).
/// </summary>
public enum JustifyItems
{
    /// <summary>
    /// Aligned to the start of the cell (<c>justify-items-start</c>).
    /// </summary>
    Start,

    /// <summary>
    /// Centered in the cell (<c>justify-items-center</c>).
    /// </summary>
    Center,

    /// <summary>
    /// Aligned to the end of the cell (<c>justify-items-end</c>).
    /// </summary>
    End,

    /// <summary>
    /// Stretched across the cell (<c>justify-items-stretch</c>), the browser default.
    /// </summary>
    Stretch
}

/// <summary>
/// How a single grid item is aligned inside its cell on the row axis, overriding its grid (<c>justify-self</c>).
/// </summary>
public enum JustifySelf
{
    /// <summary>
    /// Aligned to the start of the cell (<c>justify-self-start</c>).
    /// </summary>
    Start,

    /// <summary>
    /// Centered in the cell (<c>justify-self-center</c>).
    /// </summary>
    Center,

    /// <summary>
    /// Aligned to the end of the cell (<c>justify-self-end</c>).
    /// </summary>
    End,

    /// <summary>
    /// Stretched across the cell (<c>justify-self-stretch</c>).
    /// </summary>
    Stretch
}

/// <summary>
/// How a <see cref="Components.TwGrid"/> places children that are not given a position (<c>grid-auto-flow</c>).
/// </summary>
public enum GridAutoFlow
{
    /// <summary>
    /// Fills each row before starting the next (<c>grid-flow-row</c>), the browser default.
    /// </summary>
    Row,

    /// <summary>
    /// Fills each column before starting the next (<c>grid-flow-col</c>).
    /// </summary>
    Col,

    /// <summary>
    /// Fills earlier gaps with later, smaller items (<c>grid-flow-dense</c>). Only the visual order changes.
    /// </summary>
    Dense,

    /// <summary>
    /// Fills rows and backfills gaps (<c>grid-flow-row-dense</c>). Only the visual order changes.
    /// </summary>
    RowDense,

    /// <summary>
    /// Fills columns and backfills gaps (<c>grid-flow-col-dense</c>). Only the visual order changes.
    /// </summary>
    ColDense
}

/// <summary>
/// The size of grid tracks the browser creates on its own (<c>grid-auto-columns</c> and <c>grid-auto-rows</c>).
/// </summary>
public enum GridAutoSize
{
    /// <summary>
    /// Sized to their content (<c>auto</c>).
    /// </summary>
    Auto,

    /// <summary>
    /// As small as their content allows (<c>min-content</c>).
    /// </summary>
    Min,

    /// <summary>
    /// As large as their content wants (<c>max-content</c>).
    /// </summary>
    Max,

    /// <summary>
    /// An equal share of the free space (<c>minmax(0, 1fr)</c>).
    /// </summary>
    Fr
}
