using TwBlazor.Configuration;
using TwBlazor.Docs.Pages.Showcase.Themes;
using TwBlazor.Enums;

namespace TwBlazor.Docs.Pages.Showcase.Shared;

/// <summary>
/// An item in a showcase page's product grid. The names are made up for the example: none is a real product.
/// </summary>
/// <param name="Name">The product name.</param>
/// <param name="Description">A one-line description.</param>
/// <param name="Price">The formatted price.</param>
/// <param name="Tag">A short label shown as a chip.</param>
public sealed record ShowcaseProduct(string Name, string Description, string Price, string Tag);

/// <summary>
/// One thing a brand-inspired theme changes from the default theme, shown as a fact on its showcase page.
/// </summary>
/// <param name="Label">What is changed, e.g. "Corners".</param>
/// <param name="Value">What it is changed to.</param>
public sealed record ShowcaseTrait(string Label, string Value);

/// <summary>
/// A brand-inspired example: a complete custom theme and the copy and page-level classes of its showcase page.
/// </summary>
/// <remarks>
/// These are independent examples. twblazor is not affiliated with, endorsed by or sponsored by any of the
/// brands, and the pages use plain text for their names, never a logo or other brand asset.
/// </remarks>
public sealed class ShowcaseBrand
{
    private readonly Lazy<TwBlazorTheme> _theme;

    private ShowcaseBrand(Func<TwBlazorTheme> createTheme) => _theme = new Lazy<TwBlazorTheme>(createTheme);

    /// <summary>Gets the brand's name, written as plain text.</summary>
    public required string Name { get; init; }

    /// <summary>Gets the last segment of the page's route and the name of its stylesheet.</summary>
    public required string Slug { get; init; }

    /// <summary>Gets a sentence describing the look the theme goes for.</summary>
    public required string Summary { get; init; }

    /// <summary>Gets the name of the code example that holds the theme's source file.</summary>
    public required string CodeExample { get; init; }

    /// <summary>Gets the two brand colors as hex, for the swatches on the landing page.</summary>
    public required IReadOnlyList<string> Swatches { get; init; }

    /// <summary>Gets what the theme changes from the default.</summary>
    public required IReadOnlyList<ShowcaseTrait> Traits { get; init; }

    /// <summary>Gets the class that sets the page's font stack.</summary>
    public required string FontClass { get; init; }

    /// <summary>Gets the classes for a link in the navigation bar.</summary>
    public required string NavLinkClass { get; init; }

    /// <summary>Gets the label of the navigation link to the product grid.</summary>
    public required string ProductsLabel { get; init; }

    /// <summary>Gets the classes for the hero band.</summary>
    public required string HeroClass { get; init; }

    /// <summary>Gets the classes that align the hero's content.</summary>
    public required string HeroAlignClass { get; init; }

    /// <summary>Gets the classes for the hero's heading.</summary>
    public required string HeroTitleClass { get; init; }

    /// <summary>Gets the hero's heading.</summary>
    public required string HeroTitle { get; init; }

    /// <summary>Gets the hero's supporting sentence.</summary>
    public required string HeroText { get; init; }

    /// <summary>Gets the label of the hero's main button.</summary>
    public required string PrimaryAction { get; init; }

    /// <summary>Gets the color of the hero's main button, chosen to stand out on the hero band.</summary>
    public required Color PrimaryActionColor { get; init; }

    /// <summary>Gets the color of the hero's outlined button.</summary>
    public required Color SecondaryActionColor { get; init; }

    /// <summary>Gets the heading of the product grid.</summary>
    public required string ProductsTitle { get; init; }

    /// <summary>Gets the products in the grid.</summary>
    public required IReadOnlyList<ShowcaseProduct> Products { get; init; }

    /// <summary>Gets the theme, built once and reused.</summary>
    public TwBlazorTheme Theme => _theme.Value;

    /// <summary>Gets the route of the showcase page.</summary>
    public string Path => $"/showcase/{Slug}";

    /// <summary>Gets the stylesheet Tailwind compiles from the theme's own source file.</summary>
    public string Stylesheet => $"_content/TwBlazor.Docs/css/showcase/{Slug}.css";

    private const string navLinkBase = "rounded-sm px-3 py-2 focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-current";

    /// <summary>Gets the Microsoft-inspired example.</summary>
    public static ShowcaseBrand Microsoft { get; } = new(MicrosoftTheme.CreateTheme)
    {
        Name = "Microsoft",
        Slug = "microsoft",
        Summary = "A crisp, quiet system look: small corners, hairline borders, subtle elevation and inputs that underline on focus.",
        CodeExample = "ShowcaseMicrosoftTheme",
        Swatches = ["#0078D4", "#107C10"],
        Traits = [new("Corners", "4px"), new("Elevation", "Hairline shadows"), new("Inputs", "Underline on focus"), new("Type", "Segoe UI, semibold labels")],
        FontClass = "font-fluent",
        NavLinkClass = $"{navLinkBase} text-sm underline-offset-8 decoration-2 decoration-fluentblue-600 hover:underline",
        ProductsLabel = "Products",
        HeroClass = "bg-fluentblue-50 dark:bg-fluentblue-950/40",
        HeroAlignClass = "items-start text-start",
        HeroTitleClass = "text-4xl font-semibold md:text-5xl",
        HeroTitle = "Tools that get out of the way",
        HeroText = "Everything your team needs to plan, write and ship, in one calm workspace.",
        PrimaryAction = "Explore the suite",
        PrimaryActionColor = Color.Primary,
        SecondaryActionColor = Color.Dark,
        ProductsTitle = "Built for work",
        Products =
        [
            new("Workspace", "Documents, sheets and slides that stay in step.", "£9.99 a month", "Productivity"),
            new("Cloud Console", "Run and watch your services from one place.", "Pay as you go", "Cloud"),
            new("Studio Tablet", "A light touchscreen that becomes a laptop.", "£1,099", "Devices")
        ]
    };

    /// <summary>Gets the Apple-inspired example.</summary>
    public static ShowcaseBrand Apple { get; } = new(AppleTheme.CreateTheme)
    {
        Name = "Apple",
        Slug = "apple",
        Summary = "Generous space and soft geometry: pill buttons, large rounded cards, no shadows and a translucent navigation bar.",
        CodeExample = "ShowcaseAppleTheme",
        Swatches = ["#0071E3", "#1D1D1F"],
        Traits = [new("Corners", "Pills and 24px cards"), new("Elevation", "None"), new("Navigation", "Translucent, blurred"), new("Type", "System font, tight headings")],
        FontClass = "font-cupertino",
        NavLinkClass = $"{navLinkBase} text-xs opacity-80 hover:opacity-100",
        ProductsLabel = "Store",
        HeroClass = string.Empty,
        HeroAlignClass = "items-center text-center",
        HeroTitleClass = "text-5xl font-semibold tracking-tight md:text-7xl",
        HeroTitle = "Lighter. Faster. Yours.",
        HeroText = "A laptop that lasts all day and weighs next to nothing.",
        PrimaryAction = "Buy",
        PrimaryActionColor = Color.Primary,
        SecondaryActionColor = Color.Primary,
        ProductsTitle = "The latest",
        Products =
        [
            new("Notebook Air", "All-day battery in a body thinner than a pencil.", "From £999", "New"),
            new("Phone Pro", "A camera that sees in the dark.", "From £1,099", "Phone"),
            new("Watch Ultra", "Built for the summit and the school run.", "From £799", "Wearable")
        ]
    };

    /// <summary>Gets the LEGO-inspired example.</summary>
    public static ShowcaseBrand Lego { get; } = new(LegoTheme.CreateTheme)
    {
        Name = "LEGO",
        Slug = "lego",
        Summary = "Chunky and loud: thick black outlines, hard offset shadows, heavy uppercase type and a bright yellow bar.",
        CodeExample = "ShowcaseLegoTheme",
        Swatches = ["#E3000B", "#FFD500"],
        Traits = [new("Borders", "Thick and black"), new("Elevation", "Hard offset shadows"), new("Buttons", "Chunky, uppercase"), new("Type", "Rounded and extra bold")],
        FontClass = "font-brick",
        NavLinkClass = $"{navLinkBase} rounded-lg text-sm font-extrabold uppercase hover:bg-black/10",
        ProductsLabel = "Sets",
        HeroClass = "border-b-4 border-black bg-brickred-600 text-white",
        HeroAlignClass = "items-start text-start",
        HeroTitleClass = "text-4xl font-extrabold uppercase md:text-6xl",
        HeroTitle = "Build anything",
        HeroText = "Thousands of pieces. One rule: there are no rules.",
        PrimaryAction = "Shop sets",
        PrimaryActionColor = Color.Accent,
        SecondaryActionColor = Color.Light,
        ProductsTitle = "New this month",
        Products =
        [
            new("Rocket Launch Pad", "A 1,200 piece rocket with a working gantry.", "£89.99", "Ages 9+"),
            new("Pirate Cove", "A hidden cave, a ship and six pirates.", "£64.99", "Ages 7+"),
            new("City Fire Station", "Three floors, two engines and a pole.", "£54.99", "Ages 6+")
        ]
    };

    /// <summary>Gets the Tesco-inspired example.</summary>
    public static ShowcaseBrand Tesco { get; } = new(TescoTheme.CreateTheme)
    {
        Name = "Tesco",
        Slug = "tesco",
        Summary = "Clear and practical: confident blue, pill buttons with large touch targets, flat cards and a strong blue rule under the bar.",
        CodeExample = "ShowcaseTescoTheme",
        Swatches = ["#00539F", "#EE1C2E"],
        Traits = [new("Buttons", "Pills, 44px tall"), new("Elevation", "Flat, bordered"), new("Navigation", "Blue rule under the bar"), new("Type", "Bold labels and headings")],
        FontClass = "font-grocer",
        NavLinkClass = $"{navLinkBase} rounded-full text-sm font-bold hover:bg-grocerblue-50 dark:hover:bg-white/10",
        ProductsLabel = "Groceries",
        HeroClass = "bg-grocerblue-600 text-white",
        HeroAlignClass = "items-start text-start",
        HeroTitleClass = "text-4xl font-bold md:text-5xl",
        HeroTitle = "Your weekly shop, sorted",
        HeroText = "Book a slot, fill your basket and we will bring it to your door.",
        PrimaryAction = "Book a delivery slot",
        PrimaryActionColor = Color.Light,
        SecondaryActionColor = Color.Light,
        ProductsTitle = "This week's offers",
        Products =
        [
            new("Free Range Eggs", "A box of twelve large eggs.", "£2.75", "Offer"),
            new("Sourdough Loaf", "Baked in store this morning.", "£1.90", "Bakery"),
            new("British Strawberries", "A 400g punnet, in season now.", "£2.50", "2 for £4")
        ]
    };

    /// <summary>Gets the Cadbury-inspired example.</summary>
    public static ShowcaseBrand Cadbury { get; } = new(CadburyTheme.CreateTheme)
    {
        Name = "Cadbury",
        Slug = "cadbury",
        Summary = "Rich and warm: deep purple with gold accents, cream surfaces, serif headings, filled inputs and soft tinted shadows.",
        CodeExample = "ShowcaseCadburyTheme",
        Swatches = ["#3B0084", "#C6A866"],
        Traits = [new("Surfaces", "Warm cream"), new("Elevation", "Soft purple-tinted shadows"), new("Inputs", "Filled"), new("Type", "Serif headings, spaced labels")],
        FontClass = string.Empty,
        NavLinkClass = $"{navLinkBase} text-sm tracking-wide hover:text-white",
        ProductsLabel = "Chocolate",
        HeroClass = "bg-dairypurple-600 text-dairygold-100",
        HeroAlignClass = "items-center text-center",
        HeroTitleClass = "font-chocolate text-4xl font-semibold md:text-6xl",
        HeroTitle = "A little moment of joy",
        HeroText = "Smooth milk chocolate, made the same way for generations.",
        PrimaryAction = "Find your favourite",
        PrimaryActionColor = Color.Accent,
        SecondaryActionColor = Color.Light,
        ProductsTitle = "Our favourites",
        Products =
        [
            new("Milk Chocolate Bar", "The classic, with a glass and a half of milk.", "£1.50", "Classic"),
            new("Caramel Squares", "Soft caramel in a milk chocolate shell.", "£1.75", "Caramel"),
            new("Fruit and Nut", "Raisins and roasted almonds in every bite.", "£1.75", "Sharing")
        ]
    };

    /// <summary>Gets every example, in the order they are listed on the site.</summary>
    public static IReadOnlyList<ShowcaseBrand> All { get; } = [Microsoft, Apple, Lego, Tesco, Cadbury];

    /// <summary>
    /// The notice shown wherever the examples appear.
    /// </summary>
    public const string Disclaimer = "twblazor is not affiliated with, endorsed by or sponsored by Microsoft, Apple, the LEGO Group, Tesco or Cadbury. These are independent examples made to show what a custom theme can do. They use no logos or other brand assets, and all trademarks belong to their owners.";
}