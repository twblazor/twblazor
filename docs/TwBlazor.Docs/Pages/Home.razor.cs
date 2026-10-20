using System.Globalization;
using TwBlazor.Components;
using TwBlazor.Docs.Services;
using TwBlazor.Enums;
using TwBlazor.Models;
using TwBlazor.Utilities;
using IconEnum = TwBlazor.Enums.Icon;

namespace TwBlazor.Docs.Pages;

public partial class Home
{
    private const string quickStartInstall =
        "dotnet add package twblazor";

    private static readonly IReadOnlyList<string> _jsonLd = [SiteMetadata.HomeJsonLd()];

    private readonly List<string> _environments = ["Production", "Staging", "Development"];

    private readonly List<Figure> _releaseFigures =
    [
        new("Deploys", "128"),
        new("Failed", "3"),
        new("Avg. time", "4m")
    ];

    private readonly ChartValue[] _storageFolders =
    [
        new("Videos", 2.5), new("Photos", 1.8), new("Projects", 1.1), new("Music", 0.6), new("Documents", 0.4),
        new("Downloads", 0.3), new("Backups", 0.2), new("Apps", 0.1), new("Fonts", 0.1), new("Temp", 0.1)
    ];

    private readonly string[] _responseHours = [.. Enumerable.Range(0, 24).Select(hour => $"{hour:00}:00")];

    private readonly ChartSeries[] _responseTimes =
    [
        new("Web app", [132, 128, 121, 118, 124, 139, 158, 181, 204, 226, 241, 252, 258, 249, 243, 236, 228, 215, 198, 182, 166, 151, 142, 136]),
        new("API", [188, 182, 176, 171, 179, 196, 221, 248, 274, 292, 305, 318, 341, 372, 418, 476, 538, 602, 648, 612, 541, 463, 392, 331]),
        new("Database", [24, 22, 21, 20, 22, 25, 29, 33, 36, 39, 41, 43, 44, 42, 41, 58, 40, 38, 35, 32, 29, 27, 25, 24]),
        new("Workers", [342, 335, 328, 322, 331, 349, 372, 398, 421, 447, 468, 512, 604, 742, 868, 912, 834, 701, 588, 497, 441, 402, 371, 352])
    ];

    private readonly string[] _deployDays =
        [.. Enumerable.Range(0, 14).Select(day => new DateOnly(2026, 9, 26).AddDays(day).ToString("d MMM", CultureInfo.InvariantCulture))];

    private readonly ChartSeries[] _deploys =
    [
        new("Succeeded", [3, 2, 12, 14, 13, 15, 11, 2, 1, 13, 12, 14, 7, 6]),
        new("Failed", [0, 0, 1, 0, 0, 1, 0, 0, 0, 0, 0, 1, 0, 0])
    ];

    /// <summary>
    /// The shell shared by every card on the page (features, guides and the live examples), so they all have
    /// the same radius, border, padding and elevation.
    /// </summary>
    private const string cardClasses =
        "rounded-2xl border border-purple-200/70 p-6 shadow-xs dark:border-white/10";

    /// <summary>
    /// A feature card: the shared shell with the brand gradient along its top edge. These are not links, so
    /// they have no hover state.
    /// </summary>
    private const string featureCardClasses =
        cardClasses + " relative overflow-hidden before:absolute before:inset-x-0 before:top-0 before:h-1 before:bg-linear-to-r before:from-purple-600 before:to-fuchsia-500";

    /// <summary>
    /// One word of the hero heading. Each word rises into place after the one before, set by its animation delay.
    /// </summary>
    private const string heroWordClasses = "inline-block animate-word-rise motion-reduce:animate-none";

    /// <summary>
    /// The gradient icon tile on a feature card. Each card adds its own gradient and glow colours.
    /// </summary>
    private const string featureIconClasses =
        "inline-flex size-11 shrink-0 items-center justify-center rounded-xl bg-linear-to-br text-xl text-white shadow-lg";

    /// <summary>
    /// A guide card, which is a link: the shared shell on the same surface as <c>TwCard</c>, lifting on hover.
    /// </summary>
    private const string guideCardClasses =
        "group flex items-start gap-4 " + cardClasses + " bg-white dark:bg-[oklch(28%_0.016_252.42)] transition duration-300 hover:-translate-y-1 hover:border-fuchsia-400 hover:shadow-xl hover:shadow-purple-500/15 motion-reduce:transition-none motion-reduce:hover:translate-y-0 dark:hover:border-fuchsia-400";

    /// <summary>
    /// The icon tile on a guide card, which tilts as the card lifts. Each card adds its own colours.
    /// </summary>
    private const string guideIconClasses =
        "inline-flex size-11 shrink-0 items-center justify-center rounded-xl text-xl transition duration-300 group-hover:-rotate-6 group-hover:scale-110 motion-reduce:transition-none motion-reduce:group-hover:rotate-0 motion-reduce:group-hover:scale-100";

    private static readonly IReadOnlyList<ThemingPoint> _themingPoints =
    [
        new(IconEnum.Braces, "Every class, typed and overridable", "One C# theme class per component, every class a named property."),
        new(IconEnum.Layers_Fill, "Reusable design tokens", "Set a colour once and every component follows."),
        new(IconEnum.Sliders, "Global defaults, local overrides", "Set a default once, override any instance."),
        new(IconEnum.Wind, "Real Tailwind, real JIT", "Plain string literals Tailwind can see. No safelists."),
    ];

    private const string mergeChipClasses = "rounded-md border px-2 py-1 font-mono text-xs";

    private const string mergeThemeChipClasses =
        mergeChipClasses + " border-purple-200 bg-purple-50 text-purple-900 dark:border-purple-500/30 dark:bg-purple-500/10 dark:text-purple-200";

    private const string mergeYourChipClasses =
        mergeChipClasses + " border-fuchsia-300 bg-fuchsia-50 text-fuchsia-900 dark:border-fuchsia-500/40 dark:bg-fuchsia-500/10 dark:text-fuchsia-200";

    private const string mergeRemovedChipClasses =
        mergeChipClasses + " border-dashed border-gray-300 text-gray-600 line-through decoration-fuchsia-500 decoration-2 dark:border-gray-700 dark:text-gray-400";

    private const string mergeTabClasses =
        "cursor-pointer rounded-full px-3 py-1.5 text-sm font-medium text-purple-900 transition-colors hover:bg-purple-100 focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-purple-600 dark:text-purple-200 dark:hover:bg-purple-500/15";

    private const string mergeTabSelectedClasses =
        "cursor-pointer rounded-full bg-purple-700 px-3 py-1.5 text-sm font-medium text-white shadow-sm shadow-purple-500/30 transition-colors focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-purple-600";

    // What a button's theme might hold. The class merging example adds each example's class to these.
    private const string mergeThemeClasses =
        "inline-flex items-center rounded-md bg-purple-700 px-6 py-2 text-sm font-semibold text-white";

    private static readonly IReadOnlyList<MergeExample> _mergeExamples =
    [
        new("padding", "Padding", "px-3"),
        new("shorthand", "Shorthand", "p-3"),
        new("shape", "Shape", "rounded-full"),
        new("color", "Color", "bg-pink-700"),
    ];

    private const string localizationSnippet =
        "builder.Services.AddTwBlazor(options =>\n" +
        "{\n" +
        "    // Reword a string or add a language with your own .resx files.\n" +
        "    options.Localization.AddResource(MyStrings.ResourceManager);\n" +
        "\n" +
        "    // Dates and times follow the culture. Set only what you want to change.\n" +
        "    options.Localization.Formats.FirstDayOfWeek = DayOfWeek.Monday;\n" +
        "    options.Localization.Formats.Use12HourClock = false;\n" +
        "}, Theme.CreateTheme);";

    /// <summary>
    /// The languages twblazor ships a resource file for, in the order of the Localization page's table.
    /// </summary>
    private static readonly IReadOnlyList<LocalizationLanguage> _localizationLanguages =
    [
        new("English", "English", "en"),
        new("中文（简体）", "Chinese, Simplified", "zh-Hans"),
        new("Español", "Spanish", "es"),
        new("हिन्दी", "Hindi", "hi"),
        new("Português", "Portuguese", "pt"),
        new("বাংলা", "Bengali", "bn"),
        new("Русский", "Russian", "ru"),
        new("日本語", "Japanese", "ja"),
        new("پنجابی", "Western Punjabi", "pa-Arab"),
        new("Türkçe", "Turkish", "tr"),
        new("Tiếng Việt", "Vietnamese", "vi"),
        new("中文（繁體）", "Chinese, Traditional", "zh-Hant"),
        new("العربية", "Arabic", "ar"),
        new("मराठी", "Marathi", "mr"),
        new("తెలుగు", "Telugu", "te"),
        new("한국어", "Korean", "ko"),
        new("தமிழ்", "Tamil", "ta"),
        new("اردو", "Urdu", "ur"),
        new("Bahasa Indonesia", "Indonesian", "id"),
        new("Deutsch", "German", "de"),
        new("Français", "French", "fr"),
        new("Basa Jawa", "Javanese", "jv"),
        new("فارسی", "Persian", "fa"),
        new("Italiano", "Italian", "it"),
        new("Polski", "Polish", "pl"),
    ];

    private static readonly IReadOnlyList<MergePoint> _localizationPoints =
    [
        new("One .resx file per language", "Add or reword any string with your own resource files."),
        new("Dates and times that fit", "Week starts and 12 or 24 hour time follow the culture."),
        new("Configured in AddTwBlazor", "Pin a culture, set a format or swap the localizer."),
    ];

    private sealed record LocalizationLanguage(string NativeName, string Name, string Culture);

    // How many of the languages a wide screen lists before the link to the rest.
    private const int desktopLanguageCount = 10;

    private const string languageChipClasses =
        "inline-flex items-baseline gap-2 rounded-full border border-purple-200 bg-white px-3 py-1.5 text-sm shadow-xs dark:border-white/10 dark:bg-white/5";

    private const string languageChipCollapsedClasses = languageChipClasses + " lg:hidden";

    private static readonly IReadOnlyList<MergePoint> _rtlPoints =
    [
        new("Follows the language", "Right to left cultures mirror automatically."),
        new("Keys that match the screen", "Arrow keys move the way they point, in every component."),
        new("Mix directions on one page", "Wrap any part in TwDirectionProvider."),
    ];

    private static readonly IReadOnlyList<RtlDemoOption> _rtlDemoOptions =
    [
        new("ltr", "Left to right", TextDirection.LeftToRight),
        new("rtl", "Right to left", TextDirection.RightToLeft),
    ];

    private static readonly RtlDemoText _rtlDemoEnglish =
        new("en", "Your changes were saved.", "Name", "Enter your name", "Notifications", "Pages");

    private static readonly RtlDemoText _rtlDemoArabic =
        new("ar", "تم حفظ التغييرات.", "الاسم", "اكتب اسمك", "الإشعارات", "الصفحات");

    private TextDirection rtlDemoDirection = TextDirection.RightToLeft;

    private string rtlDemoName = string.Empty;

    private bool rtlDemoNotifications = true;

    private int rtlDemoPage = 2;

    private RtlDemoText rtlDemoText => rtlDemoDirection == TextDirection.RightToLeft ? _rtlDemoArabic : _rtlDemoEnglish;

    private string rtlDemoMarkup => $"<TwDirectionProvider Direction=\"TextDirection.{rtlDemoDirection}\">";

    private sealed record RtlDemoOption(string Id, string Label, TextDirection Direction);

    private sealed record RtlDemoText(string Culture, string Alert, string NameLabel, string NamePlaceholder, string SwitchLabel, string PaginationLabel);

    private static readonly IReadOnlyList<MergePoint> _mergePoints =
    [
        new("The last class wins", "A C# port of tailwind-merge. Nothing to install."),
        new("Understands Tailwind v4", "Variants, arbitrary values and prefixes, tested on 700+ cases."),
        new("Yours to configure", "Custom utilities, a prefix, or off, from AddTwBlazor."),
    ];

    private MergeExample mergeExample = _mergeExamples[0];

    /// <summary>
    /// Gets the classes the example renders with, merged the way every twblazor component merges them.
    /// </summary>
    private string mergedClasses => new ClassBuilder(mergeThemeClasses).AddClass(mergeExample.YourClass).Build();

    /// <summary>
    /// Gets the example's theme classes, each with whether it survived the merge.
    /// </summary>
    private IEnumerable<(string Name, bool Kept)> mergeThemeClassResults
    {
        get
        {
            var kept = mergedClasses.Split(' ').ToHashSet(StringComparer.Ordinal);
            return mergeThemeClasses.Split(' ').Select(name => (name, kept.Contains(name)));
        }
    }

    /// <summary>
    /// The words the features heading cycles through. The animation in input.css expects five.
    /// </summary>
    private static readonly IReadOnlyList<string> _cycleWords = ["dashboard", "checkout", "admin panel", "sign-in form", "settings page"];

    private bool wordCyclePaused;

    // Changing the key rebuilds the live build panel, which restarts its CSS animations.
    private int liveBuildRun;
    private int liveBuildProgress = 40;
    private bool liveBuildAutoDeploy = true;
    private bool liveBuildDeployed;

    /// <summary>
    /// Fills the live build's progress bar, then marks it deployed once the bar's own transition has had time to run.
    /// </summary>
    private async Task Deploy()
    {
        liveBuildProgress = 100;
        await Task.Delay(700);
        liveBuildDeployed = true;
    }

    /// <summary>
    /// Resets the live build panel and plays its animation again.
    /// </summary>
    private void ReplayLiveBuild()
    {
        liveBuildProgress = 40;
        liveBuildAutoDeploy = true;
        liveBuildDeployed = false;
        liveBuildRun++;
    }

    private string email = string.Empty;
    private bool notificationsEnabled = true;

    private TwStepper? checkoutStepper;
    private int checkoutStep;

    private string signInEmail = string.Empty;
    private string signInPassword = string.Empty;
    private bool rememberMe = true;

    private string projectName = "Atlas";
    private string visibility = "team";
    private string environment = "Production";
    private bool emailAlerts = true;
    private bool autoDeploy;
    private bool settingsSaved;

    /// <summary>
    /// Handles the sample settings form. It only demonstrates the controls, so it just confirms the save.
    /// </summary>
    private void SaveSettings() => settingsSaved = true;

    private void ResetSettings()
    {
        projectName = "Atlas";
        visibility = "team";
        environment = "Production";
        emailAlerts = true;
        autoDeploy = false;
        settingsSaved = false;
    }

    /// <summary>
    /// Handles the sample sign-in form. It only demonstrates the controls, so it just clears the password.
    /// </summary>
    private void SignIn() => signInPassword = string.Empty;

    private sealed record Figure(string Label, string Value);

    private sealed record MergeExample(string Id, string Label, string YourClass);

    private sealed record MergePoint(string Title, string Description);

    private sealed record ThemingPoint(IconEnum Icon, string Title, string Description);
}
