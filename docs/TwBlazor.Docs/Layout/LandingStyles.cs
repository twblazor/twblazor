namespace TwBlazor.Docs.Layout;

/// <summary>
/// The surfaces, borders, shadows and gradients the landing page and the site footer share, so every section draws
/// from one small set instead of its own one-off mix.
/// </summary>
/// <remarks>
/// <para>
/// Light mode tints edges and washes with purple. Dark mode keeps the site's blue-slate surfaces (the same values the
/// theme gives <c>TwCard</c> and the page background), with neutral white hairlines, so the landing page matches the
/// docs pages around it. There is one brand gradient, purple to fuchsia, used for accents only: text highlights, icon
/// tiles and the closing call to action.
/// </para>
/// <para>
/// Each value is a whole class literal, so Tailwind finds every class when it scans this file.
/// </para>
/// </remarks>
internal static class LandingStyles
{
    /// <summary>The hairline around cards, figures and bands.</summary>
    public const string Border = "border-purple-200/70 dark:border-white/10";

    /// <summary>A raised surface: the same white and dark slate as <c>TwCard</c>.</summary>
    public const string Surface = "bg-white dark:bg-[oklch(28%_0.016_252.42)]";

    /// <summary>A sunken well inside a card or figure, such as a "Rendered" preview.</summary>
    public const string Well = "rounded-xl border border-purple-200/70 bg-purple-50/50 dark:border-white/10 dark:bg-white/5";

    /// <summary>The resting shadow of a card.</summary>
    public const string CardShadow = "shadow-xs";

    /// <summary>The shadow of a figure, code window or the showcase: one step up from a card.</summary>
    public const string FigureShadow = "shadow-xl shadow-purple-900/10 dark:shadow-black/30";

    /// <summary>A card: content that sits on the page.</summary>
    public const string Card = "rounded-2xl border border-purple-200/70 bg-white shadow-xs dark:border-white/10 dark:bg-[oklch(28%_0.016_252.42)]";

    /// <summary>A figure: a live example or code window, raised above the cards.</summary>
    public const string Figure = "rounded-2xl border border-purple-200/70 bg-white shadow-xl shadow-purple-900/10 dark:border-white/10 dark:bg-[oklch(28%_0.016_252.42)] dark:shadow-black/30";

    /// <summary>A card that is a link: lifts a little and firms up its edge on hover.</summary>
    public const string HoverLift = "transition duration-300 hover:-translate-y-0.5 hover:border-purple-300 hover:shadow-lg hover:shadow-purple-900/10 motion-reduce:transition-none motion-reduce:hover:translate-y-0 dark:hover:border-purple-400/40 dark:hover:shadow-black/30";

    /// <summary>
    /// A tinted band behind a whole section, edged top and bottom. The one wash the landing page and footer use.
    /// </summary>
    public const string Band = "border-y border-purple-200/70 bg-purple-50/60 dark:border-white/10 dark:bg-purple-400/[0.04]";

    /// <summary>The footer's version of <see cref="Band"/>: it has a top edge only.</summary>
    public const string FooterBand = "border-t border-purple-200/70 bg-purple-50/60 dark:border-white/10 dark:bg-purple-400/[0.04]";

    /// <summary>A small rounded label or toggle that sits on the page, such as a section badge.</summary>
    public const string Pill = "border border-purple-200 bg-white/80 text-purple-800 shadow-xs dark:border-purple-400/30 dark:bg-purple-400/10 dark:text-purple-200";

    /// <summary>A solid icon tile in the brand gradient.</summary>
    public const string IconTile = "inline-flex size-11 shrink-0 items-center justify-center rounded-xl bg-linear-to-br from-purple-600 to-fuchsia-500 text-xl text-white shadow-md shadow-purple-500/25 dark:shadow-black/30";

    /// <summary>A soft icon tile, for links and secondary items.</summary>
    public const string IconTileSoft = "inline-flex size-11 shrink-0 items-center justify-center rounded-xl bg-purple-100 text-xl text-purple-700 dark:bg-purple-400/15 dark:text-purple-200";

    /// <summary>The round check beside a list item.</summary>
    public const string Bullet = "mt-0.5 inline-flex size-6 shrink-0 items-center justify-center rounded-full bg-linear-to-br from-purple-600 to-fuchsia-500 text-sm text-white";

    /// <summary>Highlighted words in a heading.</summary>
    public const string TextGradient = "bg-linear-to-r from-purple-700 to-fuchsia-600 bg-clip-text text-transparent dark:from-purple-300 dark:to-fuchsia-300";

    /// <summary>A card in the showcase's Forms tab. The cards flow down CSS columns, so each sets its own gap between fields.</summary>
    public const string FormCard = "flex flex-col gap-4 rounded-2xl border border-purple-200/70 bg-white p-5 shadow-xs dark:border-white/10 dark:bg-white/[0.03]";

    /// <summary>The icon tile at the top of a Forms tab card.</summary>
    public const string FormIcon = "inline-flex size-10 shrink-0 items-center justify-center rounded-xl bg-purple-100 text-lg text-purple-700 dark:bg-purple-400/15 dark:text-purple-200";
}
