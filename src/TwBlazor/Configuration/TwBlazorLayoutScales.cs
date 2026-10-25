// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using TwBlazor.Enums;

namespace TwBlazor.Configuration;

/// <summary>
/// One Tailwind utility written out at every breakpoint, e.g. <c>grid-cols-2</c>, <c>sm:grid-cols-2</c> and so on.
/// </summary>
/// <remarks>
/// Tailwind only compiles class names it can find written in full, so a breakpoint prefix can never be added at
/// runtime. Each breakpoint's class is spelled out here instead, once, in the theme file.
/// </remarks>
public class TwResponsiveClass
{
    /// <summary>
    /// Initializes an empty set of classes.
    /// </summary>
    public TwResponsiveClass()
    {
    }

    /// <summary>
    /// Initializes the classes for every breakpoint at once.
    /// </summary>
    /// <param name="baseClass">The class for every screen width, with no prefix.</param>
    /// <param name="sm">The class with the <c>sm:</c> prefix.</param>
    /// <param name="md">The class with the <c>md:</c> prefix.</param>
    /// <param name="lg">The class with the <c>lg:</c> prefix.</param>
    /// <param name="xl">The class with the <c>xl:</c> prefix.</param>
    /// <param name="xl2">The class with the <c>2xl:</c> prefix.</param>
    public TwResponsiveClass(string baseClass, string sm, string md, string lg, string xl, string xl2)
    {
        Base = baseClass;
        Sm = sm;
        Md = md;
        Lg = lg;
        Xl = xl;
        Xl2 = xl2;
    }

    /// <summary>
    /// Gets or sets the class for every screen width, with no prefix.
    /// </summary>
    public string Base { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the class with the <c>sm:</c> prefix.
    /// </summary>
    public string Sm { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the class with the <c>md:</c> prefix.
    /// </summary>
    public string Md { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the class with the <c>lg:</c> prefix.
    /// </summary>
    public string Lg { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the class with the <c>xl:</c> prefix.
    /// </summary>
    public string Xl { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the class with the <c>2xl:</c> prefix.
    /// </summary>
    public string Xl2 { get; set; } = string.Empty;

    /// <summary>
    /// Gets the class for a breakpoint.
    /// </summary>
    /// <param name="breakpoint">The breakpoint.</param>
    /// <returns>The class, or an empty string when none is set.</returns>
    public string Get(Breakpoint breakpoint) => breakpoint switch
    {
        Breakpoint.Sm => Sm,
        Breakpoint.Md => Md,
        Breakpoint.Lg => Lg,
        Breakpoint.Xl => Xl,
        Breakpoint.Xl2 => Xl2,
        _ => Base
    };
}

/// <summary>
/// A numbered utility written out at every breakpoint, keyed by its number: for example grid columns, where key
/// <c>3</c> holds <c>grid-cols-3</c>, <c>sm:grid-cols-3</c> and so on.
/// </summary>
public class TwResponsiveScale : Dictionary<int, TwResponsiveClass>
{
    /// <summary>
    /// Gets or sets the classes used for <see cref="GridSpan.Full"/>, such as <c>col-span-full</c>. Leave it empty
    /// for scales that have no "full" step.
    /// </summary>
    public TwResponsiveClass Full { get; set; } = new();

    /// <summary>
    /// Gets the class for a step at a breakpoint.
    /// </summary>
    /// <param name="step">The step, or <see cref="GridSpan.Full"/>. Nothing is returned when it is <see langword="null"/>.</param>
    /// <param name="breakpoint">The breakpoint.</param>
    /// <returns>The class, or an empty string when the step is not on the scale.</returns>
    public string Get(int? step, Breakpoint breakpoint) => step switch
    {
        null => string.Empty,
        GridSpan.Full => Full.Get(breakpoint),
        _ => TryGetValue(step.Value, out var classes) ? classes.Get(breakpoint) : string.Empty
    };
}

/// <summary>
/// A numbered utility keyed by its number, such as <c>order-3</c> under key <c>3</c>.
/// </summary>
public class TwClassScale : Dictionary<int, string>
{
    /// <summary>
    /// Gets the class for a step.
    /// </summary>
    /// <param name="step">The step. Nothing is returned when it is <see langword="null"/>.</param>
    /// <returns>The class, or an empty string when the step is not on the scale.</returns>
    public string Get(int? step) => step is { } value && TryGetValue(value, out var classes) ? classes : string.Empty;
}

/// <summary>
/// A spacing utility keyed by its step on Tailwind's spacing scale, such as <c>gap-4</c> under key <c>4</c> and
/// <c>gap-0.5</c> under key <c>0.5</c>.
/// </summary>
public class TwSpacingStepScale : Dictionary<double, string>
{
    /// <summary>
    /// Gets the class for a step.
    /// </summary>
    /// <param name="step">The step. Nothing is returned when it is <see langword="null"/>.</param>
    /// <returns>The class, or an empty string when the step is not on the scale.</returns>
    public string Get(double? step) => step is { } value && TryGetValue(value, out var classes) ? classes : string.Empty;
}

/// <summary>
/// The maximum width classes, one for each <see cref="ContainerSize"/>.
/// </summary>
public class TwMaxWidthScale
{
    /// <summary>Gets or sets the <c>max-w-xs</c> class.</summary>
    public string Xs { get; set; } = string.Empty;

    /// <summary>Gets or sets the <c>max-w-sm</c> class.</summary>
    public string Sm { get; set; } = string.Empty;

    /// <summary>Gets or sets the <c>max-w-md</c> class.</summary>
    public string Md { get; set; } = string.Empty;

    /// <summary>Gets or sets the <c>max-w-lg</c> class.</summary>
    public string Lg { get; set; } = string.Empty;

    /// <summary>Gets or sets the <c>max-w-xl</c> class.</summary>
    public string Xl { get; set; } = string.Empty;

    /// <summary>Gets or sets the <c>max-w-2xl</c> class.</summary>
    public string Xl2 { get; set; } = string.Empty;

    /// <summary>Gets or sets the <c>max-w-3xl</c> class.</summary>
    public string Xl3 { get; set; } = string.Empty;

    /// <summary>Gets or sets the <c>max-w-4xl</c> class.</summary>
    public string Xl4 { get; set; } = string.Empty;

    /// <summary>Gets or sets the <c>max-w-5xl</c> class.</summary>
    public string Xl5 { get; set; } = string.Empty;

    /// <summary>Gets or sets the <c>max-w-6xl</c> class.</summary>
    public string Xl6 { get; set; } = string.Empty;

    /// <summary>Gets or sets the <c>max-w-7xl</c> class.</summary>
    public string Xl7 { get; set; } = string.Empty;

    /// <summary>Gets or sets the <c>max-w-prose</c> class.</summary>
    public string Prose { get; set; } = string.Empty;

    /// <summary>Gets or sets the <c>max-w-full</c> class.</summary>
    public string Full { get; set; } = string.Empty;

    /// <summary>Gets or sets the <c>max-w-none</c> class.</summary>
    public string None { get; set; } = string.Empty;

    /// <summary>
    /// Gets the class for a size.
    /// </summary>
    /// <param name="size">The size.</param>
    /// <returns>The class, or an empty string when none is set.</returns>
    public string Get(ContainerSize size) => size switch
    {
        ContainerSize.Xs => Xs,
        ContainerSize.Sm => Sm,
        ContainerSize.Md => Md,
        ContainerSize.Lg => Lg,
        ContainerSize.Xl => Xl,
        ContainerSize.Xl2 => Xl2,
        ContainerSize.Xl3 => Xl3,
        ContainerSize.Xl4 => Xl4,
        ContainerSize.Xl5 => Xl5,
        ContainerSize.Xl6 => Xl6,
        ContainerSize.Xl7 => Xl7,
        ContainerSize.Prose => Prose,
        ContainerSize.Full => Full,
        _ => None
    };
}

/// <summary>
/// The flex direction classes written out at every breakpoint, so a layout can stack on a phone and sit side by
/// side on a wider screen.
/// </summary>
public class TwResponsiveFlexDirection
{
    /// <summary>Gets or sets the <c>flex-row</c> classes.</summary>
    public TwResponsiveClass Row { get; set; } = new();

    /// <summary>Gets or sets the <c>flex-row-reverse</c> classes.</summary>
    public TwResponsiveClass RowReverse { get; set; } = new();

    /// <summary>Gets or sets the <c>flex-col</c> classes.</summary>
    public TwResponsiveClass Col { get; set; } = new();

    /// <summary>Gets or sets the <c>flex-col-reverse</c> classes.</summary>
    public TwResponsiveClass ColReverse { get; set; } = new();

    /// <summary>
    /// Gets the class for a direction at a breakpoint.
    /// </summary>
    /// <param name="direction">The direction. Nothing is returned when it is <see langword="null"/>.</param>
    /// <param name="breakpoint">The breakpoint.</param>
    /// <returns>The class, or an empty string when none is set.</returns>
    public string Get(FlexDirection? direction, Breakpoint breakpoint) => direction switch
    {
        FlexDirection.Row => Row.Get(breakpoint),
        FlexDirection.RowReverse => RowReverse.Get(breakpoint),
        FlexDirection.Col => Col.Get(breakpoint),
        FlexDirection.ColReverse => ColReverse.Get(breakpoint),
        _ => string.Empty
    };
}

/// <summary>
/// The flex basis classes, one for each <see cref="FlexBasis"/>.
/// </summary>
public class TwFlexBasisScale
{
    /// <summary>Gets or sets the <c>basis-auto</c> class.</summary>
    public string Auto { get; set; } = string.Empty;

    /// <summary>Gets or sets the <c>basis-full</c> class.</summary>
    public string Full { get; set; } = string.Empty;

    /// <summary>Gets or sets the <c>basis-1/2</c> class.</summary>
    public string Half { get; set; } = string.Empty;

    /// <summary>Gets or sets the <c>basis-1/3</c> class.</summary>
    public string Third { get; set; } = string.Empty;

    /// <summary>Gets or sets the <c>basis-2/3</c> class.</summary>
    public string TwoThirds { get; set; } = string.Empty;

    /// <summary>Gets or sets the <c>basis-1/4</c> class.</summary>
    public string Quarter { get; set; } = string.Empty;

    /// <summary>Gets or sets the <c>basis-3/4</c> class.</summary>
    public string ThreeQuarters { get; set; } = string.Empty;

    /// <summary>
    /// Gets the class for a basis.
    /// </summary>
    /// <param name="basis">The basis. Nothing is returned when it is <see langword="null"/>.</param>
    /// <returns>The class, or an empty string when none is set.</returns>
    public string Get(FlexBasis? basis) => basis switch
    {
        FlexBasis.Auto => Auto,
        FlexBasis.Full => Full,
        FlexBasis.Half => Half,
        FlexBasis.Third => Third,
        FlexBasis.TwoThirds => TwoThirds,
        FlexBasis.Quarter => Quarter,
        FlexBasis.ThreeQuarters => ThreeQuarters,
        _ => string.Empty
    };
}

/// <summary>
/// The grid auto flow classes, one for each <see cref="GridAutoFlow"/>.
/// </summary>
public class TwGridAutoFlowScale
{
    /// <summary>Gets or sets the <c>grid-flow-row</c> class.</summary>
    public string Row { get; set; } = string.Empty;

    /// <summary>Gets or sets the <c>grid-flow-col</c> class.</summary>
    public string Col { get; set; } = string.Empty;

    /// <summary>Gets or sets the <c>grid-flow-dense</c> class.</summary>
    public string Dense { get; set; } = string.Empty;

    /// <summary>Gets or sets the <c>grid-flow-row-dense</c> class.</summary>
    public string RowDense { get; set; } = string.Empty;

    /// <summary>Gets or sets the <c>grid-flow-col-dense</c> class.</summary>
    public string ColDense { get; set; } = string.Empty;

    /// <summary>
    /// Gets the class for a flow.
    /// </summary>
    /// <param name="flow">The flow. Nothing is returned when it is <see langword="null"/>.</param>
    /// <returns>The class, or an empty string when none is set.</returns>
    public string Get(GridAutoFlow? flow) => flow switch
    {
        GridAutoFlow.Row => Row,
        GridAutoFlow.Col => Col,
        GridAutoFlow.Dense => Dense,
        GridAutoFlow.RowDense => RowDense,
        GridAutoFlow.ColDense => ColDense,
        _ => string.Empty
    };
}

/// <summary>
/// The implicit track size classes for either axis, one for each <see cref="GridAutoSize"/>.
/// </summary>
public class TwGridAutoSizeScale
{
    /// <summary>Gets or sets the class for <see cref="GridAutoSize.Auto"/>, such as <c>auto-rows-auto</c>.</summary>
    public string Auto { get; set; } = string.Empty;

    /// <summary>Gets or sets the class for <see cref="GridAutoSize.Min"/>, such as <c>auto-rows-min</c>.</summary>
    public string Min { get; set; } = string.Empty;

    /// <summary>Gets or sets the class for <see cref="GridAutoSize.Max"/>, such as <c>auto-rows-max</c>.</summary>
    public string Max { get; set; } = string.Empty;

    /// <summary>Gets or sets the class for <see cref="GridAutoSize.Fr"/>, such as <c>auto-rows-fr</c>.</summary>
    public string Fr { get; set; } = string.Empty;

    /// <summary>
    /// Gets the class for a size.
    /// </summary>
    /// <param name="size">The size. Nothing is returned when it is <see langword="null"/>.</param>
    /// <returns>The class, or an empty string when none is set.</returns>
    public string Get(GridAutoSize? size) => size switch
    {
        GridAutoSize.Auto => Auto,
        GridAutoSize.Min => Min,
        GridAutoSize.Max => Max,
        GridAutoSize.Fr => Fr,
        _ => string.Empty
    };
}

/// <summary>
/// Global configuration for the CSS grid utility classes shared by all components: column and row counts, spans
/// and start lines at every breakpoint, subgrid, auto flow, implicit track sizes and cell alignment.
/// </summary>
public class TwBlazorGrid
{
    /// <summary>
    /// Gets or sets the column counts at every breakpoint (<c>grid-cols-*</c>), keyed by the number of columns.
    /// </summary>
    public TwResponsiveScale Cols { get; set; } = [];

    /// <summary>
    /// Gets or sets the row counts at every breakpoint (<c>grid-rows-*</c>), keyed by the number of rows.
    /// </summary>
    public TwResponsiveScale Rows { get; set; } = [];

    /// <summary>
    /// Gets or sets the column spans at every breakpoint (<c>col-span-*</c>), keyed by the number of columns, with
    /// <see cref="TwResponsiveScale.Full"/> for <c>col-span-full</c>.
    /// </summary>
    public TwResponsiveScale ColSpan { get; set; } = [];

    /// <summary>
    /// Gets or sets the row spans at every breakpoint (<c>row-span-*</c>), keyed by the number of rows, with
    /// <see cref="TwResponsiveScale.Full"/> for <c>row-span-full</c>.
    /// </summary>
    public TwResponsiveScale RowSpan { get; set; } = [];

    /// <summary>
    /// Gets or sets the column start lines at every breakpoint (<c>col-start-*</c>), keyed by the line number.
    /// </summary>
    public TwResponsiveScale ColStart { get; set; } = [];

    /// <summary>
    /// Gets or sets the row start lines at every breakpoint (<c>row-start-*</c>), keyed by the line number.
    /// </summary>
    public TwResponsiveScale RowStart { get; set; } = [];

    /// <summary>
    /// Gets or sets the class that makes a nested grid share its parent's columns (<c>grid-cols-subgrid</c>).
    /// </summary>
    public string ColsSubgrid { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the class that makes a nested grid share its parent's rows (<c>grid-rows-subgrid</c>).
    /// </summary>
    public string RowsSubgrid { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the auto flow classes (<c>grid-flow-*</c>).
    /// </summary>
    public TwGridAutoFlowScale AutoFlow { get; set; } = new();

    /// <summary>
    /// Gets or sets the implicit column size classes (<c>auto-cols-*</c>).
    /// </summary>
    public TwGridAutoSizeScale AutoCols { get; set; } = new();

    /// <summary>
    /// Gets or sets the implicit row size classes (<c>auto-rows-*</c>).
    /// </summary>
    public TwGridAutoSizeScale AutoRows { get; set; } = new();

    /// <summary>
    /// Gets or sets the classes that align every item inside its cell on the row axis (<c>justify-items-*</c>).
    /// Uses <see cref="TwFlexAlignmentScale.Start"/>, <see cref="TwFlexAlignmentScale.Center"/>,
    /// <see cref="TwFlexAlignmentScale.End"/> and <see cref="TwFlexAlignmentScale.Stretch"/>.
    /// </summary>
    public TwFlexAlignmentScale JustifyItems { get; set; } = new();

    /// <summary>
    /// Gets or sets the classes that align one item inside its cell on the row axis (<c>justify-self-*</c>).
    /// Uses <see cref="TwFlexAlignmentScale.Start"/>, <see cref="TwFlexAlignmentScale.Center"/>,
    /// <see cref="TwFlexAlignmentScale.End"/> and <see cref="TwFlexAlignmentScale.Stretch"/>.
    /// </summary>
    public TwFlexAlignmentScale JustifySelf { get; set; } = new();
}
