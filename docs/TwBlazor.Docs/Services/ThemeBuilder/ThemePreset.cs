using TwBlazor.Enums;

namespace TwBlazor.Docs.Services.ThemeBuilder;

/// <summary>
/// The color family an example theme is filed under, so the theme builder can filter them.
/// </summary>
internal enum ThemeTone
{
    /// <summary>Purples and violets.</summary>
    Purple,

    /// <summary>Blues and cyans.</summary>
    Blue,

    /// <summary>Greens and teals.</summary>
    Green,

    /// <summary>Pinks and roses.</summary>
    Pink,

    /// <summary>Reds.</summary>
    Red,

    /// <summary>Oranges, yellows and browns.</summary>
    Orange,

    /// <summary>Grays.</summary>
    Neutral,

    /// <summary>Soft, muted colors on clearly tinted light backgrounds.</summary>
    Pastel,

    /// <summary>Fully saturated colors on crisp backgrounds.</summary>
    Vivid,

    /// <summary>Themed looks, from retro terminals to synthwave, on clearly tinted backgrounds.</summary>
    Fun,
}

/// <summary>
/// A ready-made starting point for the theme builder: the six semantic colors, the enum defaults and the page
/// backgrounds.
/// </summary>
/// <param name="Name">The preset's name.</param>
/// <param name="Description">A few words on its character.</param>
/// <param name="Tone">The color family it is filed under.</param>
/// <param name="Colors">Hex color per <see cref="ThemeColorFamily.Name"/>.</param>
/// <param name="Defaults">The enum defaults.</param>
/// <param name="Surfaces">The light and dark page backgrounds.</param>
internal sealed record ThemePreset(string Name, string Description, ThemeTone Tone, IReadOnlyDictionary<string, string> Colors, ThemeDefaults Defaults, ThemeSurfaces Surfaces)
{
    /// <summary>
    /// The example themes, with the default (purple) theme first.
    /// </summary>
    public static readonly IReadOnlyList<ThemePreset> All =
    [
        new("Purple", "The default theme", ThemeTone.Purple, ThemeColorFamily.All.ToDictionary(f => f.Name, f => f.DefaultHex), ThemeDefaults.Default, ThemeSurfaces.Default),

        // Purples and violets
        Create("Lavender", "Soft violet, gentle corners", ThemeTone.Purple, (Palette.Violet600, Palette.Fuchsia600, Palette.Green700, Palette.Red600, Palette.Yellow600, Palette.Blue600), new(Rounded.Lg, Shadow.Sm, InputVariant.Filled, ButtonVariant.Filled)),
        Create("Grape", "Rich violet, crisp edges", ThemeTone.Purple, (Palette.Violet700, Palette.Fuchsia700, Palette.Green700, Palette.Red700, Palette.Amber700, Palette.Blue700), new(Rounded.Sm, Shadow.Md, InputVariant.Outlined, ButtonVariant.Filled)),
        Create("Orchid", "Magenta with a violet partner", ThemeTone.Purple, (Palette.Fuchsia700, Palette.Violet600, Palette.Green800, Palette.Rose700, Palette.Amber600, Palette.Sky700), new(Rounded.Md, Shadow.Sm, InputVariant.Filled, ButtonVariant.Filled)),
        Create("Plum", "Deep wine and berry", ThemeTone.Purple, (Palette.Fuchsia800, Palette.Rose700, Palette.Green800, Palette.Rose800, Palette.Amber700, Palette.Blue700), new(Rounded.Md, Shadow.Md, InputVariant.Filled, ButtonVariant.Elevated)),
        Create("Dusk", "Twilight purple, deep shadows", ThemeTone.Purple, (Palette.Violet800, Palette.Pink700, Palette.Green700, Palette.Red700, Palette.Amber600, Palette.Sky700), new(Rounded.Lg, Shadow.Lg, InputVariant.Filled, ButtonVariant.Elevated)),
        Create("Royal", "Regal purple and gold", ThemeTone.Purple, (Palette.Purple800, Palette.Amber700, Palette.Green700, Palette.Red700, Palette.Yellow600, Palette.Blue700), new(Rounded.Md, Shadow.Lg, InputVariant.Outlined, ButtonVariant.Filled)),

        // Blues
        Create("Ocean", "Cool blues, rounder corners", ThemeTone.Blue, (Palette.Sky700, Palette.Teal600, Palette.Green700, Palette.Red600, Palette.Amber600, Palette.Indigo600), new(Rounded.Lg, Shadow.Md, InputVariant.Outlined, ButtonVariant.Filled)),
        Create("Midnight", "Indigo night, outlined inputs", ThemeTone.Blue, (Palette.Indigo700, Palette.Indigo500, Palette.Green700, Palette.Red700, Palette.Amber600, Palette.Sky700), new(Rounded.Md, Shadow.Lg, InputVariant.Outlined, ButtonVariant.Filled)),
        Create("Indigo", "Classic indigo, balanced", ThemeTone.Blue, (Palette.Indigo700, Palette.Violet600, Palette.Green700, Palette.Rose700, Palette.Yellow600, Palette.Sky700), new(Rounded.Md, Shadow.Sm, InputVariant.Filled, ButtonVariant.Filled)),
        Create("Cobalt", "Bold blue, straight down the middle", ThemeTone.Blue, (Palette.Blue700, Palette.Sky700, Palette.Green700, Palette.Red700, Palette.Amber600, Palette.Indigo700), new(Rounded.Md, Shadow.Md, InputVariant.Filled, ButtonVariant.Filled)),
        Create("Navy", "Dark blue, sharp and quiet", ThemeTone.Blue, (Palette.Blue900, Palette.Teal700, Palette.Green800, Palette.Red800, Palette.Amber700, Palette.Blue800), new(Rounded.Sm, Shadow.None, InputVariant.Outlined, ButtonVariant.Filled)),
        Create("Sky", "Open blue, light touch", ThemeTone.Blue, (Palette.Sky700, Palette.Violet600, Palette.Green700, Palette.Red600, Palette.Yellow600, Palette.Cyan700), new(Rounded.Lg, Shadow.Sm, InputVariant.Default, ButtonVariant.Filled)),
        Create("Arctic", "Icy blue, text buttons", ThemeTone.Blue, (Palette.Sky600, Palette.Indigo500, Palette.Green700, Palette.Red600, Palette.Amber600, Palette.Sky700), new(Rounded.Lg, Shadow.Sm, InputVariant.Outlined, ButtonVariant.Text)),
        Create("Cyan", "Electric teal-blue", ThemeTone.Blue, (Palette.Cyan700, Palette.Indigo600, Palette.Green700, Palette.Red700, Palette.Amber600, Palette.Sky700), new(Rounded.Md, Shadow.Md, InputVariant.Filled, ButtonVariant.Filled)),
        Create("Blueberry", "Juicy blue, pill shaped", ThemeTone.Blue, (Palette.Blue800, Palette.Violet600, Palette.Green700, Palette.Red700, Palette.Amber600, Palette.Sky700), new(Rounded.Full, Shadow.Md, InputVariant.Filled, ButtonVariant.Filled)),
        Create("Lagoon", "Deep cyan water", ThemeTone.Blue, (Palette.Cyan800, Palette.Teal600, Palette.Green700, Palette.Red600, Palette.Amber600, Palette.Blue700), new(Rounded.Lg, Shadow.Sm, InputVariant.Outlined, ButtonVariant.Elevated)),

        // Greens and teals
        Create("Forest", "Earthy greens, flat and square", ThemeTone.Green, (Palette.Green700, Palette.Amber700, Palette.Lime700, Palette.Red700, Palette.Yellow600, Palette.Cyan700), new(Rounded.Sm, Shadow.None, InputVariant.Default, ButtonVariant.Filled)),
        Create("Emerald", "Jewel green, outlined inputs", ThemeTone.Green, (Palette.Emerald700, Palette.Teal700, Palette.Green700, Palette.Red700, Palette.Amber700, Palette.Sky700), new(Rounded.Lg, Shadow.Md, InputVariant.Outlined, ButtonVariant.Filled)),
        Create("Mint", "Fresh green-teal", ThemeTone.Green, (Palette.Teal700, Palette.Sky700, Palette.Green700, Palette.Rose700, Palette.Amber700, Palette.Indigo700), new(Rounded.Lg, Shadow.Sm, InputVariant.Filled, ButtonVariant.Filled)),
        Create("Teal", "Calm teal, friendly corners", ThemeTone.Green, (Palette.Teal600, Palette.Sky600, Palette.Green700, Palette.Red600, Palette.Amber600, Palette.Indigo600), new(Rounded.Lg, Shadow.Sm, InputVariant.Filled, ButtonVariant.Filled)),
        Create("Lime", "Zesty olive-lime", ThemeTone.Green, (Palette.Lime700, Palette.Teal700, Palette.Green800, Palette.Red700, Palette.Yellow600, Palette.Sky700), new(Rounded.Md, Shadow.Sm, InputVariant.Filled, ButtonVariant.Filled)),
        Create("Moss", "Muted woodland green", ThemeTone.Green, (Palette.Lime800, Palette.Yellow800, Palette.Green800, Palette.Red800, Palette.Yellow600, Palette.Teal700), new(Rounded.Sm, Shadow.None, InputVariant.Default, ButtonVariant.Filled)),
        Create("Aurora", "Green-teal with violet glow", ThemeTone.Green, (Palette.Teal700, Palette.Violet600, Palette.Green700, Palette.Rose700, Palette.Yellow600, Palette.Sky600), new(Rounded.Lg, Shadow.Lg, InputVariant.Filled, ButtonVariant.Elevated)),
        Create("Matcha", "Whisked green tea, outlined", ThemeTone.Green, (Palette.Green800, Palette.Lime700, Palette.Emerald700, Palette.Red700, Palette.Amber700, Palette.Cyan700), new(Rounded.Md, Shadow.None, InputVariant.Default, ButtonVariant.Outlined)),
        Create("Peacock", "Teal feathers with a violet flash", ThemeTone.Green, (Palette.Teal800, Palette.Violet700, Palette.Green700, Palette.Rose700, Palette.Amber600, Palette.Sky700), new(Rounded.Lg, Shadow.Md, InputVariant.Filled, ButtonVariant.Filled)),
        Create("Neon", "Bright green with a fuchsia spark", ThemeTone.Green, (Palette.Emerald600, Palette.Fuchsia600, Palette.Green700, Palette.Red600, Palette.Amber600, Palette.Sky600), new(Rounded.Full, Shadow.Lg, InputVariant.Outlined, ButtonVariant.Elevated)),

        // Pinks
        Create("Rose", "Romantic rose and violet", ThemeTone.Pink, (Palette.Rose700, Palette.Purple600, Palette.Green700, Palette.Red700, Palette.Amber600, Palette.Sky700), new(Rounded.Lg, Shadow.Md, InputVariant.Filled, ButtonVariant.Filled)),
        Create("Bubblegum", "Playful pink", ThemeTone.Pink, (Palette.Pink700, Palette.Violet600, Palette.Green700, Palette.Red600, Palette.Yellow600, Palette.Sky600), new(Rounded.Lg, Shadow.Md, InputVariant.Filled, ButtonVariant.Filled)),
        Create("Flamingo", "Hot pink, round and raised", ThemeTone.Pink, (Palette.Pink700, Palette.Orange600, Palette.Green700, Palette.Red600, Palette.Amber600, Palette.Sky600), new(Rounded.Lg, Shadow.Lg, InputVariant.Filled, ButtonVariant.Elevated)),
        Create("Sakura", "Cherry blossom pink, soft", ThemeTone.Pink, (Palette.Rose700, Palette.Fuchsia600, Palette.Green700, Palette.Red700, Palette.Amber600, Palette.Sky700), new(Rounded.Lg, Shadow.Sm, InputVariant.Filled, ButtonVariant.Text)),

        // Reds
        Create("Cherry", "Deep red, clean lines", ThemeTone.Red, (Palette.Red700, Palette.Pink700, Palette.Green800, Palette.Red800, Palette.Amber700, Palette.Blue700), new(Rounded.Sm, Shadow.Sm, InputVariant.Outlined, ButtonVariant.Filled)),
        Create("Volcano", "Molten red and orange", ThemeTone.Red, (Palette.Red700, Palette.Orange700, Palette.Green700, Palette.Red900, Palette.Amber700, Palette.Blue700), new(Rounded.Sm, Shadow.Lg, InputVariant.Filled, ButtonVariant.Elevated)),
        Create("Raspberry", "Tart red-pink, crisp", ThemeTone.Red, (Palette.Rose800, Palette.Pink700, Palette.Green800, Palette.Red800, Palette.Amber700, Palette.Blue700), new(Rounded.Sm, Shadow.Md, InputVariant.Outlined, ButtonVariant.Filled)),

        // Oranges, yellows and browns
        Create("Sunset", "Warm oranges and pinks, deep shadows", ThemeTone.Orange, (Palette.Orange600, Palette.Pink600, Palette.Green700, Palette.Rose700, Palette.Yellow600, Palette.Violet600), new(Rounded.Lg, Shadow.Lg, InputVariant.Filled, ButtonVariant.Elevated)),
        Create("Coral", "Burnt orange, elevated buttons", ThemeTone.Orange, (Palette.Orange700, Palette.Pink800, Palette.Green700, Palette.Red700, Palette.Yellow600, Palette.Cyan700), new(Rounded.Lg, Shadow.Md, InputVariant.Filled, ButtonVariant.Elevated)),
        Create("Amber", "Golden brown, flat", ThemeTone.Orange, (Palette.Amber700, Palette.Fuchsia700, Palette.Green700, Palette.Red700, Palette.Yellow600, Palette.Blue700), new(Rounded.Sm, Shadow.None, InputVariant.Default, ButtonVariant.Filled)),
        Create("Terracotta", "Baked clay and sage", ThemeTone.Orange, (Palette.Orange800, Palette.Yellow700, Palette.Lime800, Palette.Red800, Palette.Amber700, Palette.Cyan800), new(Rounded.Sm, Shadow.Md, InputVariant.Filled, ButtonVariant.Filled)),
        Create("Espresso", "Dark roast, no shadows", ThemeTone.Orange, (Palette.Amber900, Palette.Yellow700, Palette.Lime800, Palette.Red800, Palette.Amber700, Palette.Cyan800), new(Rounded.Sm, Shadow.None, InputVariant.Default, ButtonVariant.Filled)),
        Create("Tangerine", "Zingy orange, soft and bold", ThemeTone.Orange, (Palette.Orange600, Palette.Sky700, Palette.Green700, Palette.Red700, Palette.Yellow600, Palette.Blue700), new(Rounded.Lg, Shadow.Md, InputVariant.Filled, ButtonVariant.Filled)),
        Create("Lemon", "Sharp yellow, flat", ThemeTone.Orange, (Palette.Yellow800, Palette.Lime700, Palette.Green800, Palette.Red700, Palette.Amber700, Palette.Sky700), new(Rounded.Md, Shadow.None, InputVariant.Default, ButtonVariant.Filled)),
        Create("Cocoa", "Milk chocolate and berry", ThemeTone.Orange, (Palette.Amber800, Palette.Rose700, Palette.Lime800, Palette.Red800, Palette.Amber700, Palette.Cyan800), new(Rounded.Lg, Shadow.Md, InputVariant.Filled, ButtonVariant.Elevated)),

        // Neutrals
        Create("Slate", "Monochrome, sharp and outlined", ThemeTone.Neutral, (Palette.Slate700, Palette.Slate500, Palette.Green700, Palette.Red700, Palette.Amber700, Palette.Sky700), new(Rounded.None, Shadow.None, InputVariant.Outlined, ButtonVariant.Outlined)),
        Create("Graphite", "Neutral grey, square and outlined", ThemeTone.Neutral, (Palette.Gray700, Palette.Gray500, Palette.Green700, Palette.Red700, Palette.Amber700, Palette.Sky700), new(Rounded.None, Shadow.None, InputVariant.Outlined, ButtonVariant.Outlined)),
        Create("Stone", "Warm grey, text buttons", ThemeTone.Neutral, (Palette.Stone600, Palette.Yellow700, Palette.Lime800, Palette.Orange800, Palette.Amber700, Palette.Cyan700), new(Rounded.Sm, Shadow.Sm, InputVariant.Default, ButtonVariant.Text)),
        Create("Ink", "Near black, square and stark", ThemeTone.Neutral, (Palette.Zinc800, Palette.Indigo600, Palette.Green700, Palette.Red700, Palette.Amber700, Palette.Sky700), new(Rounded.None, Shadow.None, InputVariant.Outlined, ButtonVariant.Filled)),

        // Pastels
        Create("Lilac", "Soft lilac with a rosy accent", ThemeTone.Pastel, ("#a284c5", "#b16d94", Soft.Success, Soft.Danger, Soft.Warning, Soft.Info), new(Rounded.Lg, Shadow.Sm, InputVariant.Filled, ButtonVariant.Filled)),
        Create("Periwinkle", "Blue-violet, gentle and calm", ThemeTone.Pastel, ("#868bce", "#a571a7", Soft.Success, Soft.Danger, Soft.Warning, Soft.Info), new(Rounded.Lg, Shadow.None, InputVariant.Default, ButtonVariant.Filled)),
        Create("Wisteria", "Pale violet blossom", ThemeTone.Pastel, ("#a782c2", "#5186bb", Soft.Success, Soft.Danger, Soft.Warning, Soft.Info), new(Rounded.Md, Shadow.Sm, InputVariant.Outlined, ButtonVariant.Outlined)),
        Create("Hydrangea", "Dusky blue-violet petals", ThemeTone.Pastel, ("#9187cb", "#0c8d9e", Soft.Success, Soft.Danger, Soft.Warning, Soft.Info), new(Rounded.Lg, Shadow.Md, InputVariant.Filled, ButtonVariant.Elevated)),
        Create("Powder", "Powder blue and lilac", ThemeTone.Pastel, ("#5795c8", "#8e76b8", Soft.Success, Soft.Danger, Soft.Warning, Soft.Info), new(Rounded.Lg, Shadow.Sm, InputVariant.Filled, ButtonVariant.Text)),
        Create("Glacier", "Cool icy blue", ThemeTone.Pastel, ("#3c9abb", "#737fc1", Soft.Success, Soft.Danger, Soft.Warning, Soft.Info), new(Rounded.Md, Shadow.None, InputVariant.Outlined, ButtonVariant.Filled)),
        Create("Seafoam", "Washed sea green", ThemeTone.Pastel, ("#3ca18a", "#328bb0", Soft.Success, Soft.Danger, Soft.Warning, Soft.Info), new(Rounded.Lg, Shadow.Sm, InputVariant.Default, ButtonVariant.Filled)),
        Create("Pistachio", "Nutty pale green", ThemeTone.Pastel, ("#739b5c", "#9f7e34", Soft.Success, Soft.Danger, Soft.Warning, Soft.Info), new(Rounded.Md, Shadow.Sm, InputVariant.Filled, ButtonVariant.Outlined)),
        Create("Sage", "Herby grey-green", ThemeTone.Pastel, ("#5e9f6d", "#b07543", Soft.Success, Soft.Danger, Soft.Warning, Soft.Info), new(Rounded.Sm, Shadow.None, InputVariant.Default, ButtonVariant.Filled)),
        Create("Honeydew", "Melon green with a hint of teal", ThemeTone.Pastel, ("#809852", "#1e907f", Soft.Success, Soft.Danger, Soft.Warning, Soft.Info), new(Rounded.Lg, Shadow.Md, InputVariant.Filled, ButtonVariant.Elevated)),
        Create("Blush", "Soft pink with a lilac accent", ThemeTone.Pastel, ("#c7798b", "#8e76b8", Soft.Success, Soft.Danger, Soft.Warning, Soft.Info), new(Rounded.Lg, Shadow.Sm, InputVariant.Filled, ButtonVariant.Filled)),
        Create("Candyfloss", "Spun sugar pink and blue", ThemeTone.Pastel, ("#c07aa2", "#4188b6", Soft.Success, Soft.Danger, Soft.Warning, Soft.Info), new(Rounded.Lg, Shadow.Md, InputVariant.Filled, ButtonVariant.Elevated)),
        Create("Mauve", "Dusty pink-purple", ThemeTone.Pastel, ("#b77db1", "#7e7cbe", Soft.Success, Soft.Danger, Soft.Warning, Soft.Info), new(Rounded.Md, Shadow.Sm, InputVariant.Outlined, ButtonVariant.Filled)),
        Create("Rosewater", "Faded rose and peach", ThemeTone.Pastel, ("#c97980", "#b07543", Soft.Success, Soft.Danger, Soft.Warning, Soft.Info), new(Rounded.Lg, Shadow.None, InputVariant.Default, ButtonVariant.Text)),
        Create("Sorbet", "Raspberry ripple and lemon", ThemeTone.Pastel, ("#c67991", "#9a8034", Soft.Success, Soft.Danger, Soft.Warning, Soft.Info), new(Rounded.Lg, Shadow.Md, InputVariant.Filled, ButtonVariant.Filled)),
        Create("Peach", "Ripe peach with a pink accent", ThemeTone.Pastel, ("#c77f5f", "#b36c8f", Soft.Success, Soft.Danger, Soft.Warning, Soft.Info), new(Rounded.Lg, Shadow.Sm, InputVariant.Filled, ButtonVariant.Filled)),
        Create("Apricot", "Warm apricot and coral", ThemeTone.Pastel, ("#bf8350", "#bb6d67", Soft.Success, Soft.Danger, Soft.Warning, Soft.Info), new(Rounded.Md, Shadow.Md, InputVariant.Filled, ButtonVariant.Elevated)),
        Create("Buttercup", "Soft yellow with a coral accent", ThemeTone.Pastel, ("#a88e42", "#bb6e62", Soft.Success, Soft.Danger, Soft.Warning, Soft.Info), new(Rounded.Lg, Shadow.Sm, InputVariant.Default, ButtonVariant.Filled)),
        Create("Oatmeal", "Warm oat with a lavender accent", ThemeTone.Pastel, ("#9f8d7a", "#8e76b8", Soft.Success, Soft.Danger, Soft.Warning, Soft.Info), new(Rounded.Sm, Shadow.None, InputVariant.Outlined, ButtonVariant.Outlined)),
        Create("Dove", "Grey with a lilac whisper", ThemeTone.Pastel, ("#8e8ea6", "#a86fa3", Soft.Success, Soft.Danger, Soft.Warning, Soft.Info), new(Rounded.Md, Shadow.Sm, InputVariant.Outlined, ButtonVariant.Filled)),

        // Vivid
        Create("Electric", "Pure electric blue", ThemeTone.Vivid, ("#5c8bfe", "#bb4efc", Vivid.Success, Vivid.Danger, Vivid.Warning, Vivid.Info), new(Rounded.Md, Shadow.Md, InputVariant.Filled, ButtonVariant.Filled)),
        Create("Ultraviolet", "Blacklight violet", ThemeTone.Vivid, ("#a278fe", "#ed2caf", Vivid.Success, Vivid.Danger, Vivid.Warning, Vivid.Info), new(Rounded.Lg, Shadow.Lg, InputVariant.Filled, ButtonVariant.Elevated)),
        Create("Fuchsia", "Full strength pink-purple", ThemeTone.Vivid, ("#e94be1", "#487bff", Vivid.Success, Vivid.Danger, Vivid.Warning, Vivid.Info), new(Rounded.Lg, Shadow.Md, InputVariant.Outlined, ButtonVariant.Filled)),
        Create("Magenta", "Loud hot pink", ThemeTone.Vivid, ("#ff3eaf", "#e55105", Vivid.Success, Vivid.Danger, Vivid.Warning, Vivid.Info), new(Rounded.Lg, Shadow.Lg, InputVariant.Filled, ButtonVariant.Elevated)),
        Create("Scarlet", "Fire engine red", ThemeTone.Vivid, ("#ff544c", "#af7603", Vivid.Success, Vivid.Danger, Vivid.Warning, Vivid.Info), new(Rounded.Sm, Shadow.Md, InputVariant.Outlined, ButtonVariant.Filled)),
        Create("Marigold", "Saturated orange-gold", ThemeTone.Vivid, ("#d07d05", "#f90755", Vivid.Success, Vivid.Danger, Vivid.Warning, Vivid.Info), new(Rounded.Md, Shadow.Sm, InputVariant.Filled, ButtonVariant.Filled)),
        Create("Chartreuse", "Acid yellow-green", ThemeTone.Vivid, ("#6f9f03", "#0c908a", Vivid.Success, Vivid.Danger, Vivid.Warning, Vivid.Info), new(Rounded.Sm, Shadow.None, InputVariant.Default, ButtonVariant.Outlined)),
        Create("Jade", "Deep saturated green", ThemeTone.Vivid, ("#05a55f", "#038bb7", Vivid.Success, Vivid.Danger, Vivid.Warning, Vivid.Info), new(Rounded.Md, Shadow.Md, InputVariant.Filled, ButtonVariant.Filled)),
        Create("Turquoise", "Bright tropical teal", ThemeTone.Vivid, ("#04a19b", "#dc3dd5", Vivid.Success, Vivid.Danger, Vivid.Warning, Vivid.Info), new(Rounded.Lg, Shadow.Sm, InputVariant.Filled, ButtonVariant.Filled)),
        Create("Azure", "Clear saturated sky", ThemeTone.Vivid, ("#0599df", "#fd1a06", Vivid.Success, Vivid.Danger, Vivid.Warning, Vivid.Info), new(Rounded.Md, Shadow.Sm, InputVariant.Outlined, ButtonVariant.Text)),

        // Fun
        Create("Terminal", "Retro phosphor green on a CRT", ThemeTone.Fun, ("#17a805", "#af7603", Vivid.Success, Vivid.Danger, Vivid.Warning, Vivid.Info), new(Rounded.None, Shadow.None, InputVariant.Outlined, ButtonVariant.Outlined)),
        Create("Synthwave", "Neon magenta and cyan at sunset", ThemeTone.Fun, ("#fd40be", "#008d9f", Vivid.Success, Vivid.Danger, Vivid.Warning, Vivid.Info), new(Rounded.Lg, Shadow.Lg, InputVariant.Filled, ButtonVariant.Elevated)),
        Create("Arcade", "Electric blue and coin-slot yellow", ThemeTone.Fun, ("#528dff", "#998001", Vivid.Success, Vivid.Danger, Vivid.Warning, Vivid.Info), new(Rounded.Sm, Shadow.Md, InputVariant.Filled, ButtonVariant.Elevated)),
        Create("Handheld", "Olive greens from an old pocket console", ThemeTone.Fun, ("#869858", "#8c7aae", Soft.Success, Soft.Danger, Soft.Warning, Soft.Info), new(Rounded.Sm, Shadow.None, InputVariant.Default, ButtonVariant.Filled)),
        Create("Vaporwave", "Pink and teal statues and sunsets", ThemeTone.Fun, ("#dd68a4", "#0b8d8e", Soft.Success, Soft.Danger, Soft.Warning, Soft.Info), new(Rounded.Lg, Shadow.Sm, InputVariant.Filled, ButtonVariant.Filled)),
        Create("Halloween", "Pumpkin orange and witchy purple", ThemeTone.Fun, ("#e76d04", "#b058fe", Vivid.Success, Vivid.Danger, Vivid.Warning, Vivid.Info), new(Rounded.Md, Shadow.Md, InputVariant.Filled, ButtonVariant.Filled)),
        Create("Festive", "Holly red and evergreen", ThemeTone.Fun, ("#ff5352", "#07933b", Vivid.Success, Vivid.Danger, Vivid.Warning, Vivid.Info), new(Rounded.Md, Shadow.Sm, InputVariant.Outlined, ButtonVariant.Filled)),
        Create("Lavalamp", "Molten orange blobs in violet", ThemeTone.Fun, ("#fe550f", "#ce47eb", Vivid.Success, Vivid.Danger, Vivid.Warning, Vivid.Info), new(Rounded.Lg, Shadow.Lg, InputVariant.Filled, ButtonVariant.Elevated)),
        Create("Disco", "Gold and magenta under the mirror ball", ThemeTone.Fun, ("#b68901", "#e339ca", Vivid.Success, Vivid.Danger, Vivid.Warning, Vivid.Info), new(Rounded.Lg, Shadow.Lg, InputVariant.Filled, ButtonVariant.Elevated)),
        Create("Pixel", "Eight bit blue and red", ThemeTone.Fun, ("#3090fe", "#fb0c2d", Vivid.Success, Vivid.Danger, Vivid.Warning, Vivid.Info), new(Rounded.None, Shadow.Sm, InputVariant.Default, ButtonVariant.Filled)),
    ];

    /// <summary>
    /// Gets whether the given colors, defaults and backgrounds are exactly this preset, ignoring hex case and shorthand.
    /// </summary>
    /// <param name="colors">Hex color per <see cref="ThemeColorFamily.Name"/>.</param>
    /// <param name="defaults">The enum defaults.</param>
    /// <param name="surfaces">The page backgrounds.</param>
    public bool Matches(IReadOnlyDictionary<string, string> colors, ThemeDefaults defaults, ThemeSurfaces surfaces) =>
        defaults == Defaults
        && Same(surfaces.Light, Surfaces.Light)
        && Same(surfaces.Dark, Surfaces.Dark)
        && surfaces.BodyLight is null
        && surfaces.BodyDark is null
        && ThemeColorFamily.All.All(f => colors.TryGetValue(f.Name, out var hex) && Same(hex, Colors[f.Name]));

    private static bool Same(string a, string b) =>
        OklchColor.TryParseHex(a, out var x) && OklchColor.TryParseHex(b, out var y) && x.ToCss() == y.ToCss();

    private static ThemePreset Create(
        string name,
        string description,
        ThemeTone tone,
        (string Primary, string Accent, string Success, string Danger, string Warning, string Info) colors,
        ThemeDefaults defaults) =>
        new(
            name,
            description,
            tone,
            new Dictionary<string, string>
            {
                ["Primary"] = colors.Primary,
                ["Accent"] = colors.Accent,
                ["Success"] = colors.Success,
                ["Danger"] = colors.Danger,
                ["Warning"] = colors.Warning,
                ["Info"] = colors.Info,
            },
            defaults,
            TintFor(colors.Primary, colors.Accent, tone));

    /// <summary>
    /// Gets whether themes filed under <paramref name="tone"/> get clearly tinted backgrounds rather than a faint tint.
    /// </summary>
    /// <param name="tone">The color family.</param>
    public static bool HasClearlyTintedSurfaces(ThemeTone tone) => tone is ThemeTone.Pastel or ThemeTone.Fun;

    /// <summary>
    /// Backgrounds tinted toward the primary color, with a little of the accent mixed in so two themes sharing a
    /// primary still differ. A gray primary gives only a hint of tint, unless the theme is clearly tinted: pastel
    /// themes, and fun ones, whose dark mode takes on more of their color as well.
    /// </summary>
    /// <param name="primaryHex">The primary color.</param>
    /// <param name="accentHex">The accent color.</param>
    /// <param name="tone">The color family the theme is filed under.</param>
    private static ThemeSurfaces TintFor(string primaryHex, string accentHex, ThemeTone tone)
    {
        if (!OklchColor.TryParseHex(primaryHex, out var primary))
            return ThemeSurfaces.Default;

        // Without a readable accent the tint leans on the primary alone.
        if (!OklchColor.TryParseHex(accentHex, out var accent))
            accent = primary;

        // Hues are angles, so they are averaged as points on the color wheel, each weighted by how colorful it is.
        var x = (0.75 * primary.Chroma * Math.Cos(Radians(primary.Hue))) + (0.25 * accent.Chroma * Math.Cos(Radians(accent.Hue)));
        var y = (0.75 * primary.Chroma * Math.Sin(Radians(primary.Hue))) + (0.25 * accent.Chroma * Math.Sin(Radians(accent.Hue)));
        var hue = ((Math.Atan2(y, x) * 180 / Math.PI) + 360) % 360;

        // The primary's own lightness nudges the backgrounds too, which keeps two themes of similar hue apart.
        var lightnessShift = (primary.Lightness - 0.5) * 0.03;

        return tone switch
        {
            ThemeTone.Pastel => ThemeSurface.Pastel(hue, lightnessShift),
            ThemeTone.Fun => ThemeSurface.Pastel(hue, lightnessShift, darkChroma: 0.045),
            _ => ThemeSurface.Tinted(hue, Math.Clamp(primary.Chroma / 0.15, 0.3, 1), lightnessShift),
        };
    }

    private static double Radians(double degrees) => degrees * Math.PI / 180;

    /// <summary>
    /// The soft status colors the pastel themes share. Like the pastel brand colors, each is the lightest shade of
    /// its hue that still keeps readable text on its filled shade and a visible border on white.
    /// </summary>
    private static class Soft
    {
        public const string Success = "#4e8e5d";
        public const string Danger = "#bb6d6b";
        public const string Warning = "#d6ad68";
        public const string Info = "#5795c8";
    }

    /// <summary>
    /// The saturated status colors the vivid and most fun themes share: each is the most saturated shade of its hue
    /// that still keeps readable text on its filled shade and a visible border on white.
    /// </summary>
    private static class Vivid
    {
        public const string Success = "#079343";
        public const string Danger = "#fb0c2d";
        public const string Warning = "#e8a70a";
        public const string Info = "#3090fe";
    }

    /// <summary>
    /// The Tailwind palette shades the example themes are built from, named by palette and shade.
    /// </summary>
    private static class Palette
    {
        public const string Amber600 = "#d97706";
        public const string Amber700 = "#b45309";
        public const string Amber800 = "#92400e";
        public const string Amber900 = "#78350f";
        public const string Blue600 = "#2563eb";
        public const string Blue700 = "#1d4ed8";
        public const string Blue800 = "#1e40af";
        public const string Blue900 = "#1e3a8a";
        public const string Cyan700 = "#0e7490";
        public const string Cyan800 = "#155e75";
        public const string Emerald600 = "#059669";
        public const string Emerald700 = "#047857";
        public const string Fuchsia600 = "#c026d3";
        public const string Fuchsia700 = "#a21caf";
        public const string Fuchsia800 = "#86198f";
        public const string Gray500 = "#6b7280";
        public const string Gray700 = "#374151";
        public const string Green700 = "#15803d";
        public const string Green800 = "#166534";
        public const string Indigo500 = "#6366f1";
        public const string Indigo600 = "#4f46e5";
        public const string Indigo700 = "#4338ca";
        public const string Lime700 = "#4d7c0f";
        public const string Lime800 = "#3f6212";
        public const string Orange600 = "#ea580c";
        public const string Orange700 = "#c2410c";
        public const string Orange800 = "#9a3412";
        public const string Pink600 = "#db2777";
        public const string Pink700 = "#be185d";
        public const string Pink800 = "#9d174d";
        public const string Purple600 = "#9333ea";
        public const string Purple800 = "#6b21a8";
        public const string Red600 = "#dc2626";
        public const string Red700 = "#b91c1c";
        public const string Red800 = "#991b1b";
        public const string Red900 = "#7f1d1d";
        public const string Rose700 = "#be123c";
        public const string Rose800 = "#9f1239";
        public const string Sky600 = "#0284c7";
        public const string Sky700 = "#0369a1";
        public const string Slate500 = "#64748b";
        public const string Slate700 = "#334155";
        public const string Stone600 = "#57534e";
        public const string Teal600 = "#0d9488";
        public const string Teal700 = "#0f766e";
        public const string Teal800 = "#115e59";
        public const string Violet600 = "#7c3aed";
        public const string Violet700 = "#6d28d9";
        public const string Violet800 = "#5b21b6";
        public const string Yellow600 = "#ca8a04";
        public const string Yellow700 = "#a16207";
        public const string Yellow800 = "#854d0e";
        public const string Zinc800 = "#27272a";
    }
}
