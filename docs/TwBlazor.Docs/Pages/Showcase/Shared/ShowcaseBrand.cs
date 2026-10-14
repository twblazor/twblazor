using TwBlazor.Configuration;
using TwBlazor.Docs.Pages.Showcase.Themes;
using IconEnum = TwBlazor.Enums.Icon;

namespace TwBlazor.Docs.Pages.Showcase.Shared;

/// <summary>
/// An item in a showcase page's product grid. Names, prices and offers are illustrative.
/// </summary>
/// <param name="Name">The product name.</param>
/// <param name="Description">A one-line description.</param>
/// <param name="Price">The formatted price.</param>
/// <param name="Tag">A short label, such as a badge or an offer.</param>
/// <param name="Icon">The icon that stands in for the product's picture when it has none.</param>
/// <param name="Image">The address of the product's picture, on the brand's own site or under <c>wwwroot/images/showcase</c>, if there is one.</param>
public sealed record ShowcaseProduct(string Name, string Description, string Price, string Tag, IconEnum Icon, string? Image = null);

/// <summary>
/// One thing a brand-inspired theme changes from the default theme, shown as a fact on its showcase page.
/// </summary>
/// <param name="Label">What is changed, e.g. "Corners".</param>
/// <param name="Value">What it is changed to.</param>
public sealed record ShowcaseTrait(string Label, string Value);

/// <summary>
/// A brand-inspired example: a complete custom theme and the facts its showcase page is built from.
/// </summary>
/// <remarks>
/// These are independent examples. twblazor is not affiliated with, endorsed by or sponsored by any of the
/// brands. Pictures are loaded from each brand's own website by their address, and where no address is set the
/// page falls back to text and icons. Logos belong to their owners and are used here only to identify them.
/// </remarks>
public sealed class ShowcaseBrand
{
    private readonly Lazy<TwBlazorTheme> _theme;

    private ShowcaseBrand(Func<TwBlazorTheme> createTheme) => _theme = new Lazy<TwBlazorTheme>(createTheme);

    /// <summary>Gets the brand's name.</summary>
    public required string Name { get; init; }

    /// <summary>Gets the last segment of the page's route and the name of its stylesheet.</summary>
    public required string Slug { get; init; }

    /// <summary>Gets a sentence describing the look the theme goes for.</summary>
    public required string Summary { get; init; }

    /// <summary>Gets the name of the code example that holds the theme's source file.</summary>
    public required string CodeExample { get; init; }

    /// <summary>Gets the brand colors as hex, for the swatches on the landing page.</summary>
    public required IReadOnlyList<string> Swatches { get; init; }

    /// <summary>Gets what the theme changes from the default.</summary>
    public required IReadOnlyList<ShowcaseTrait> Traits { get; init; }

    /// <summary>Gets the products the page shows.</summary>
    public required IReadOnlyList<ShowcaseProduct> Products { get; init; }

    /// <summary>Gets the class that sets the page's font stack.</summary>
    public required string FontClass { get; init; }

    /// <summary>Gets the classes for the brand's name where no logo file is set.</summary>
    public required string WordmarkClass { get; init; }

    /// <summary>
    /// Gets whether the page ignores the visitor's dark mode, for a brand whose own site has no dark version.
    /// </summary>
    public bool LightOnly { get; init; }

    /// <summary>
    /// Gets the icon shown beside or in place of the brand's name. Only set where the icon set the library
    /// already ships includes the brand's mark.
    /// </summary>
    public IconEnum? LogoIcon { get; init; }

    /// <summary>Gets whether <see cref="LogoIcon"/> stands alone, with the name kept for screen readers only.</summary>
    public bool LogoIconOnly { get; init; }

    /// <summary>
    /// Gets the address of the brand's logo, or <see langword="null"/> to set the name as text. It is either on
    /// the brand's own website or a file under <c>wwwroot/images/showcase</c> taken from that website.
    /// </summary>
    public string? LogoUrl { get; init; }

    /// <summary>Gets the classes that size the logo.</summary>
    public string LogoClass { get; init; } = "h-7";

    /// <summary>Gets the address of the picture in the page's hero, on the brand's own website, if one is known.</summary>
    public string? HeroImage { get; init; }

    /// <summary>Gets the name of the page's component, which is also the name of its Razor file.</summary>
    public required string PageName { get; init; }

    /// <summary>Gets the theme, built once and reused.</summary>
    public TwBlazorTheme Theme => _theme.Value;

    /// <summary>Gets the route of the showcase page.</summary>
    public string Path => $"/showcase/{Slug}";

    /// <summary>Gets the stylesheet Tailwind compiles from the theme's own source file.</summary>
    public string Stylesheet => $"_content/TwBlazor.Docs/css/showcase/{Slug}.css";

    private const string microsoftMedia = "https://cdn-dynmedia-1.microsoft.com/is/image/microsoftcorp";

    private const string microsoftCard = "resMode=sharp2&op_usm=1.5,0.65,15,0&wid=800&hei=450&qlt=85&fit=constrain";

    private const string microsoftLogo = "https://uhf.microsoft.com/images/microsoft/RE1Mu3b.png";

    private const string legoLogo = "https://assets.lego.com/logos/v4.10.0/brand-lego.svg";

    private const string elevationTrait = "Elevation";

    /// <summary>The folder of the pictures and logos kept with the site.</summary>
    public const string LocalImages = "_content/TwBlazor.Docs/images/showcase";

    /// <summary>Gets the Microsoft-inspired example.</summary>
    public static ShowcaseBrand Microsoft { get; } = new(MicrosoftTheme.CreateTheme)
    {
        Name = "Microsoft",
        Slug = "microsoft",
        Summary = "A crisp, quiet system look: 2px corners, hairline borders, subtle elevation and inputs that underline on focus.",
        CodeExample = "ShowcaseMicrosoftTheme",
        Swatches = ["#0067B8", "#F25022", "#7FBA00", "#FFB900"],
        Traits = [new("Corners", "2px"), new(elevationTrait, "Hairline shadows"), new("Inputs", "Underline on focus"), new("Type", "Segoe UI, semibold labels")],
        FontClass = "font-fluent",
        WordmarkClass = "text-xl font-semibold text-neutral-600 dark:text-neutral-300",
        LogoIcon = IconEnum.Microsoft,
        LogoUrl = microsoftLogo,
        HeroImage = $"{microsoftMedia}/Hero-Featured-Mobile-Surface-Laptop-Ultra-Nightfall-Angle-Photo-No-Badge?resMode=sharp2&op_usm=1.5,0.65,15,0&wid=1600&hei=900&qlt=90&fit=constrain",
        PageName = "MicrosoftShowcase",
        Products =
        [
            new("Surface Laptop, 13-inch", "Thin, light and built for all-day work.", "From £899", "New", IconEnum.Laptop, $"{microsoftMedia}/Card-Feature-Surface-Laptop-SnapdragonX2-13-2Ed-Violet?{microsoftCard}"),
            new("Microsoft 365", "Word, Excel, PowerPoint and more, with Copilot built in.", "£8.49 a month", "Popular", IconEnum.Windows, $"{microsoftMedia}/Content-Card-M365-Icon-Bounce-Premium?{microsoftCard}"),
            new("Xbox Game Pass Ultimate", "Hundreds of games on console, PC and cloud.", "£14.99 a month", "Gaming", IconEnum.Box_Seam, $"{microsoftMedia}/455251-Card-Feature-Xbox-Game-Pass?{microsoftCard}"),
            new("Surface Mouse", "Sculpted for comfort, built to travel.", "£44.99", "New", IconEnum.Laptop, $"{microsoftMedia}/Card-Feature-Surface-Mouse-2Ed-Platinum-Homepage?{microsoftCard}")
        ]
    };

    /// <summary>Gets the Apple-inspired example.</summary>
    public static ShowcaseBrand Apple { get; } = new(AppleTheme.CreateTheme)
    {
        Name = "Apple",
        Slug = "apple",
        Summary = "Generous space and soft geometry: pill buttons, large rounded tiles, no shadows and a translucent navigation bar.",
        CodeExample = "ShowcaseAppleTheme",
        Swatches = ["#0071E3", "#1D1D1F", "#F5F5F7"],
        Traits = [new("Corners", "Pills and 24px tiles"), new(elevationTrait, "None"), new("Navigation", "Translucent, blurred"), new("Type", "System font, tight headings")],
        FontClass = "font-cupertino",
        WordmarkClass = "text-lg",
        LogoIcon = IconEnum.Apple,
        LogoIconOnly = true,
        HeroImage = $"{LocalImages}/apple-hero.jpg",
        PageName = "AppleShowcase",
        Products =
        [
            new("iPhone Air", "The thinnest iPhone ever.", "From £999", "New", IconEnum.Phone, "https://www.apple.com/v/iphone/home/ck/images/overview/select/iphone_air__b5qmgl05ojyq_large.jpg"),
            new("iPhone 17", "Even more delightful. Even more durable.", "From £799", "iPhone", IconEnum.Phone, "https://www.apple.com/v/iphone/home/ck/images/overview/select/iphone_17__fb1277oq3eaa_large.jpg"),
            new("Apple Watch Ultra 3", "Personal beast.", "From £749", "Watch", IconEnum.Smartwatch, "https://www.apple.com/v/iphone/home/ck/images/overview/augment/watch__co62o3s4jn6u_large.jpg"),
            new("MacBook", "iPhone and Mac. Better together.", "From £999", "Mac", IconEnum.Laptop, "https://www.apple.com/uk/iphone/home/images/overview/augment/mac__e1itm725bz8m_large.jpg")
        ]
    };

    /// <summary>Gets the LEGO-inspired example.</summary>
    public static ShowcaseBrand Lego { get; } = new(LegoTheme.CreateTheme)
    {
        Name = "LEGO",
        Slug = "lego",
        Summary = "Bright and friendly: a yellow bar, orange pill buttons with dark text, blue links, bold type and flat white cards.",
        CodeExample = "ShowcaseLegoTheme",
        Swatches = ["#FFD502", "#E3000B", "#FD8024", "#006DB7"],
        Traits = [new("Navigation", "Yellow bar"), new("Buttons", "Orange pills, dark text"), new(elevationTrait, "Flat"), new("Type", "Rounded and bold")],
        FontClass = "font-brick",
        WordmarkClass = "text-2xl font-black tracking-tight",
        LightOnly = true,
        LogoUrl = legoLogo,
        LogoClass = "h-11",
        HeroImage = $"{LocalImages}/lego-hero.webp",
        PageName = "LegoShowcase",
        Products =
        [
            new("City Fire Station", "Three floors, a fire engine and a helicopter.", "£64.99", "New", IconEnum.Box_Seam, $"{LocalImages}/fire-station.jpg"),
            new("Technic Supra MK4 Race Car", "A detailed model with a working engine and steering.", "£169.99", "Hard to find", IconEnum.Box_Seam, $"{LocalImages}/supra-mk4-race-car.png"),
            new("Icons Orchid", "A flower display that never needs watering.", "£44.99", "Best seller", IconEnum.Box_Seam, $"{LocalImages}/orchid.png"),
            new("Classic Creative Bricks", "790 pieces in 33 colours.", "£24.99", "Ages 4+", IconEnum.Box_Seam, $"{LocalImages}/creative-bricks.jpg")
        ]
    };

    /// <summary>Gets the Tesco-inspired example.</summary>
    public static ShowcaseBrand Tesco { get; } = new(TescoTheme.CreateTheme)
    {
        Name = "Tesco",
        Slug = "tesco",
        Summary = "Clear and practical: confident blue, pill buttons with large touch targets, flat bordered tiles and red for offers.",
        CodeExample = "ShowcaseTescoTheme",
        Swatches = ["#00539F", "#E81C2D", "#FFD100"],
        Traits = [new("Buttons", "Pills, 44px tall"), new(elevationTrait, "Flat, bordered"), new("Offers", "Red and yellow"), new("Type", "Bold labels and headings")],
        FontClass = "font-grocer",
        WordmarkClass = "text-3xl font-black uppercase tracking-widest text-grocerred-500",
        LightOnly = true,
        LogoUrl = $"{LocalImages}/tesco.svg",
        LogoClass = "h-16",
        PageName = "TescoShowcase",
        Products =
        [
            new("British Whole Milk 2.272L, 4 Pints", "£0.73 per litre", "£1.65", "£1.45 Clubcard Price", IconEnum.Basket2, $"{LocalImages}/whole-milk.avif"),
            new("Free Range Large Eggs 12 Pack", "£0.27 each", "£3.25", "£2.75 Clubcard Price", IconEnum.Basket2, $"{LocalImages}/eggs.avif"),
            new("Finest Sourdough Loaf 400g", "£0.55 per 100g", "£2.20", "Any 2 for £3.50 Clubcard Price", IconEnum.Basket2, $"{LocalImages}/sourdough.avif"),
            new("British Strawberries 400g", "£6.25 per kg", "£2.50", "£2.00 Clubcard Price", IconEnum.Basket2, $"{LocalImages}/strawberries.avif")
        ]
    };

    /// <summary>Gets the Trading 212-inspired example.</summary>
    public static ShowcaseBrand Trading212 { get; } = new(Trading212Theme.CreateTheme)
    {
        Name = "Trading 212",
        Slug = "trading212",
        Summary = "Clean and confident: bright blue pill buttons, large soft cards, cool neutral surfaces and green and red kept for gains and losses.",
        CodeExample = "ShowcaseTrading212Theme",
        Swatches = ["#00A7E1", "#0F172A", "#16A34A", "#DC2626"],
        Traits = [new("Buttons", "Blue pills, 44px tall"), new("Corners", "16px cards"), new("Color", "Green and red mean gain and loss"), new("Type", "Bold, tabular figures")],
        FontClass = "font-trade",
        WordmarkClass = "text-xl font-black uppercase tracking-tight",
        LogoUrl = $"{LocalImages}/trading212.svg",
        LogoClass = "h-6",
        PageName = "Trading212Showcase",
        Products =
        [
            new("Apple", "AAPL", "$213.55", "+1.24%", IconEnum.Apple),
            new("Microsoft", "MSFT", "$428.90", "+0.62%", IconEnum.Microsoft),
            new("Tesla", "TSLA", "$251.44", "-2.31%", IconEnum.Lightning_Charge_Fill),
            new("Vanguard S&P 500", "VUSA", "£84.12", "+0.38%", IconEnum.Globe)
        ]
    };

    /// <summary>Gets every example, in the order they are listed on the site.</summary>
    public static IReadOnlyList<ShowcaseBrand> All { get; } = [Microsoft, Apple, Lego, Tesco, Trading212];

    /// <summary>
    /// The notice shown wherever the examples appear.
    /// </summary>
    public const string Disclaimer = "twblazor is not affiliated with, endorsed by or sponsored by Microsoft, Apple, the LEGO Group, Tesco or Trading 212. These are independent examples made to show what a custom theme can do, and are not those companies' websites. Product names, prices, offers and market data are illustrative, and nothing here is financial advice. Logos and pictures come from each company's own website and remain its property, and all trademarks belong to their owners.";
}