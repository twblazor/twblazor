using System.Globalization;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using TwBlazor.Components;
using TwBlazor.Docs.Layout;
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

    // The page's text is built when the component is created, in the language of the request, and kept: a
    // language change reloads the page, and keeping the same arrays saves the charts from redrawing on every render.
    private readonly List<string> _environments =
    [
        Loc.Text("Home_Env_Production", "Production"),
        Loc.Text("Home_Env_Staging", "Staging"),
        Loc.Text("Home_Env_Development", "Development")
    ];

    // The plan starts on the Monday four weeks ago, so the today line always falls inside it.
    private static readonly DateTime _planStart = DateTime.Today.AddDays(-28 - ((int)DateTime.Today.DayOfWeek + 6) % 7);

    private List<ChartTask> planTasks =
    [
        new(Loc.Text("Home_Plan_Discovery", "Discovery"), PlanDay(0), PlanDay(0)) { Id = "discovery", ColorIndex = 0 },
        new(Loc.Text("Home_Plan_Research", "Research"), PlanDay(0), PlanDay(9)) { Id = "research", ParentId = "discovery", Progress = 1, ColorIndex = 0 },
        new(Loc.Text("Home_Plan_Interviews", "User interviews"), PlanDay(5), PlanDay(16)) { Id = "interviews", ParentId = "discovery", Progress = 1, ColorIndex = 0 },
        new(Loc.Text("Home_Review_Tag_Design", "Design"), PlanDay(0), PlanDay(0)) { Id = "design", ColorIndex = 1 },
        new(Loc.Text("Home_Plan_Wireframes", "Wireframes"), PlanDay(14), PlanDay(25)) { Id = "wireframes", ParentId = "design", Progress = 1, ColorIndex = 1 },
        new(Loc.Text("Home_Plan_Visual", "Visual design"), PlanDay(21), PlanDay(39)) { Id = "visual", ParentId = "design", Progress = 0.6, ColorIndex = 1 },
        new(Loc.Text("Home_Plan_Build", "Build"), PlanDay(0), PlanDay(0)) { Id = "build", ColorIndex = 2 },
        new(Loc.Text("Home_Plan_Api", "API"), PlanDay(21), PlanDay(46)) { Id = "api", ParentId = "build", Progress = 0.45, ColorIndex = 2 },
        new(Loc.Text("Home_Plan_Frontend", "Front end"), PlanDay(35), PlanDay(60)) { Id = "frontend", ParentId = "build", Progress = 0.1, ColorIndex = 2 },
        new(Loc.Text("Home_Plan_Testing", "Testing"), PlanDay(49), PlanDay(67)) { Id = "testing", ParentId = "build", ColorIndex = 2 },
        new(Loc.Text("Home_Plan_Launch", "Launch"), PlanDay(0), PlanDay(0)) { Id = "launch", ColorIndex = 3 },
        new(Loc.Text("Home_Plan_Beta", "Beta"), PlanDay(67), PlanDay(81)) { Id = "beta", ParentId = "launch", ColorIndex = 3 },
        new(Loc.Text("Home_Plan_Signoff", "Sign-off"), PlanDay(84), PlanDay(84)) { Id = "signoff", ParentId = "launch", ColorIndex = 3 },
        new(Loc.Text("Home_Plan_Release", "Release"), PlanDay(88), PlanDay(88)) { Id = "release", ParentId = "launch", ColorIndex = 3 }
    ];

    private List<ChartTaskDependency> planDependencies =
    [
        new("research", "interviews", ChartDependencyType.StartToStart),
        new("interviews", "wireframes"),
        new("wireframes", "visual", ChartDependencyType.StartToStart),
        new("visual", "frontend"),
        new("api", "frontend", ChartDependencyType.FinishToFinish),
        new("frontend", "testing"),
        new("testing", "beta"),
        new("beta", "signoff"),
        new("signoff", "release")
    ];

    // Discovery is finished, so it starts folded away.
    private IReadOnlySet<string> planCollapsed = new HashSet<string> { "discovery" };

    private static DateTime PlanDay(int days) => _planStart.AddDays(days);

    private void OnPlanTaskChange(ChartTaskChange change)
    {
        var index = planTasks.FindIndex(task => task.Key == change.Task.Key);
        planTasks[index] = change.Task with { Start = change.Start, End = change.End, Progress = change.Progress };
        planTasks = [.. planTasks];
    }

    private void OnPlanDependencyCreate(ChartDependencyRequest request)
    {
        if (planDependencies.Any(d => (d.FromId == request.Dependency.FromId && d.ToId == request.Dependency.ToId)
            || (d.FromId == request.Dependency.ToId && d.ToId == request.Dependency.FromId)))
        {
            request.Cancel = true;
            return;
        }

        planDependencies = [.. planDependencies, request.Dependency];
    }

    private void OnPlanDependencyClick(ChartTaskDependency dependency) =>
        planDependencies = [.. planDependencies.Where(d => d != dependency)];

    private readonly List<Figure> _releaseFigures =
    [
        new(Loc.Text("Home_Release_Deploys", "Deploys"), "128"),
        new(Loc.Text("Home_Failed", "Failed"), "3"),
        new(Loc.Text("Home_Release_AvgTime", "Avg. time"), "4m")
    ];

    private readonly ChartValue[] _storageFolders =
    [
        new(Loc.Text("Home_Folder_Videos", "Videos"), 2.5), new(Loc.Text("Home_Folder_Photos", "Photos"), 1.8),
        new(Loc.Text("Home_Folder_Projects", "Projects"), 1.1), new(Loc.Text("Home_Folder_Music", "Music"), 0.6),
        new(Loc.Text("Home_Folder_Documents", "Documents"), 0.4), new(Loc.Text("Home_Folder_Downloads", "Downloads"), 0.3),
        new(Loc.Text("Home_Folder_Backups", "Backups"), 0.2), new(Loc.Text("Home_Folder_Apps", "Apps"), 0.1),
        new(Loc.Text("Home_Folder_Fonts", "Fonts"), 0.1), new(Loc.Text("Home_Folder_Temp", "Temp"), 0.1)
    ];

    private readonly string[] _responseHours = [.. Enumerable.Range(0, 24).Select(hour => $"{hour:00}:00")];

    private readonly ChartSeries[] _responseTimes =
    [
        new(Loc.Text("Home_Series_WebApp", "Web app"), [132, 128, 121, 118, 124, 139, 158, 181, 204, 226, 241, 252, 258, 249, 243, 236, 228, 215, 198, 182, 166, 151, 142, 136]),
        new("API", [188, 182, 176, 171, 179, 196, 221, 248, 274, 292, 305, 318, 341, 372, 418, 476, 538, 602, 648, 612, 541, 463, 392, 331]),
        new(Loc.Text("Home_Series_Database", "Database"), [24, 22, 21, 20, 22, 25, 29, 33, 36, 39, 41, 43, 44, 42, 41, 58, 40, 38, 35, 32, 29, 27, 25, 24]),
        new(Loc.Text("Home_Series_Workers", "Workers"), [342, 335, 328, 322, 331, 349, 372, 398, 421, 447, 468, 512, 604, 742, 868, 912, 834, 701, 588, 497, 441, 402, 371, 352])
    ];

    private readonly string[] _deployDays =
        [.. Enumerable.Range(0, 14).Select(day => new DateOnly(2026, 9, 26).AddDays(day).ToString("d MMM", Loc.Culture))];

    private readonly ChartSeries[] _deploys =
    [
        new(Loc.Text("Home_Series_Succeeded", "Succeeded"), [3, 2, 12, 14, 13, 15, 11, 2, 1, 13, 12, 14, 7, 6]),
        new(Loc.Text("Home_Failed", "Failed"), [0, 0, 1, 0, 0, 1, 0, 0, 0, 0, 0, 1, 0, 0])
    ];

    /// <summary>
    /// The shell shared by every card on the page (features, guides and the live examples), so they all have
    /// the same radius, border, padding and elevation.
    /// </summary>
    private const string cardClasses =
        "rounded-2xl border border-purple-200/70 p-6 shadow-xs dark:border-white/10";

    /// <summary>
    /// One word of the hero heading. Each word rises into place after the one before, set by its animation delay.
    /// </summary>
    private const string heroWordClasses = "inline-block animate-word-rise motion-reduce:animate-none";

    /// <summary>
    /// A guide card, which is a link: the shared shell on the same surface as <c>TwCard</c>, lifting on hover.
    /// </summary>
    private const string guideCardClasses =
        "group flex items-center gap-4 " + cardClasses + " " + LandingStyles.Surface + " " + LandingStyles.HoverLift;

    /// <summary>
    /// The soft icon tile on a guide card, which tilts as the card lifts.
    /// </summary>
    private const string guideIconClasses =
        LandingStyles.IconTileSoft + " transition duration-300 group-hover:-rotate-6 group-hover:scale-110 motion-reduce:transition-none motion-reduce:group-hover:rotate-0 motion-reduce:group-hover:scale-100";

    // Counted from components.json, leaving out the chart overview page, and rounded down to a ten so the "+"
    // stays true. The languages are the ones twblazor ships, English included.
    private static readonly int _componentCount =
        ComponentCatalog.LoadLeafEntries().Count(leaf => leaf.Entry.Url != "/chart") / 10 * 10;

    private readonly IReadOnlyList<Stat> _stats =
    [
        new(IconEnum.Grid_3X3_Gap, Loc.Format("Home_Stats_Components", "{0}+ components", _componentCount)),
        new(IconEnum.Bar_Chart_Line, Loc.Format("Home_Stats_Charts", "{0}+ chart types", _chartTypeCount)),
        new(IconEnum.Universal_Access, Loc.Text("Home_Stats_Accessible", "Keyboard and screen reader ready")),
        new(IconEnum.Translate, Loc.Format("Home_Stats_Languages", "{0} languages", _localizationLanguages.Count)),
        new(IconEnum.Unlock, Loc.Text("Home_Stats_Mit", "Free and MIT licensed")),
    ];

    private readonly IReadOnlyList<ThemingPoint> _accessibilityPoints =
    [
        new(IconEnum.Keyboard, Loc.Text("Home_A11y_Keyboard_Title", "Keyboard first"), Loc.Text("Home_A11y_Keyboard_Body", "Tab, arrow keys, Home, End and Escape work in every component, as WAI-ARIA describes.")),
        new(IconEnum.Megaphone, Loc.Text("Home_A11y_Reader_Title", "Screen reader ready"), Loc.Text("Home_A11y_Reader_Body", "Roles, names and live announcements, translated into every shipped language.")),
        new(IconEnum.Bullseye, Loc.Text("Home_A11y_Focus_Title", "Focus you can see"), Loc.Text("Home_A11y_Focus_Body", "A clear focus ring on every control, and focus returned where it belongs when a popup closes.")),
        new(IconEnum.Pause_Circle, Loc.Text("Home_A11y_Motion_Title", "Calm when asked"), Loc.Text("Home_A11y_Motion_Body", "Animations stop when the system asks for reduced motion.")),
        new(IconEnum.Circle_Half, Loc.Text("Home_A11y_Contrast_Title", "High contrast mode"), Loc.Text("Home_A11y_Contrast_Body", "Windows high contrast colours are respected, so states stay visible.")),
        new(IconEnum.Shield_Check, Loc.Text("Home_A11y_Tested_Title", "Tested with axe-core"), Loc.Text("Home_A11y_Tested_Body", "Every docs page is scanned for accessibility issues in the test suite.")),
    ];

    private readonly IReadOnlyList<ThemingPoint> _themingPoints =
    [
        new(IconEnum.Braces, Loc.Text("Home_Theming_Typed_Title", "Every class, typed and overridable"), Loc.Text("Home_Theming_Typed_Body", "One C# theme class per component, every class a named property.")),
        new(IconEnum.Layers_Fill, Loc.Text("Home_Theming_Tokens_Title", "Reusable design tokens"), Loc.Text("Home_Theming_Tokens_Body", "Set a colour once and every component follows.")),
        new(IconEnum.Sliders, Loc.Text("Home_Theming_Defaults_Title", "Global defaults, local overrides"), Loc.Text("Home_Theming_Defaults_Body", "Set a default once, override any instance.")),
        new(IconEnum.Wind, Loc.Text("Home_Theming_Jit_Title", "Real Tailwind, real JIT"), Loc.Text("Home_Theming_Jit_Body", "Plain string literals Tailwind can see. No safelists.")),
    ];

    /// <summary>
    /// What each line of the Theme.cs example changes on the button, in the order the lines type out.
    /// </summary>
    private readonly IReadOnlyList<string> _themeBuildChanges =
    [
        Loc.Text("Home_Theming_Change_Corners", "Corners: full"),
        Loc.Text("Home_Theming_Change_Shadow", "Shadow: large"),
        Loc.Text("Home_Theming_Change_Colour", "Colour: pink"),
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

    private readonly IReadOnlyList<MergePoint> _localizationPoints =
    [
        new(Loc.Text("Home_Loc_Resx_Title", "One .resx file per language"), Loc.Text("Home_Loc_Resx_Body", "Add or reword any string with your own resource files.")),
        new(Loc.Text("Home_Loc_Dates_Title", "Dates and times that fit"), Loc.Text("Home_Loc_Dates_Body", "Week starts and 12 or 24 hour time follow the culture.")),
        new(Loc.Text("Home_Loc_Config_Title", "Configured in AddTwBlazor"), Loc.Text("Home_Loc_Config_Body", "Pin a culture, set a format or swap the localizer.")),
    ];

    private sealed record LocalizationLanguage(string NativeName, string Name, string Culture);

    // How many of the languages are listed before the link to the rest. The full list is on the Localization page.
    private const int shownLanguageCount = 10;

    private const string languageChipClasses =
        "inline-flex items-baseline gap-2 rounded-full border border-purple-200 bg-white px-3 py-1.5 text-sm shadow-xs dark:border-white/10 dark:bg-[oklch(28%_0.016_252.42)]";

    private readonly IReadOnlyList<MergePoint> _rtlPoints =
    [
        new(Loc.Text("Home_Rtl_Follows_Title", "Follows the language"), Loc.Text("Home_Rtl_Follows_Body", "Right to left cultures mirror automatically.")),
        new(Loc.Text("Home_Rtl_Keys_Title", "Keys that match the screen"), Loc.Text("Home_Rtl_Keys_Body", "Arrow keys move the way they point, in every component.")),
        new(Loc.Text("Home_Rtl_Mix_Title", "Mix directions on one page"), Loc.Text("Home_Rtl_Mix_Body", "Wrap any part in TwDirectionProvider.")),
    ];

    private readonly IReadOnlyList<RtlDemoOption> _rtlDemoOptions =
    [
        new("ltr", Loc.Text("Home_Rtl_Ltr", "Left to right"), TextDirection.LeftToRight),
        new("rtl", Loc.Text("Home_Rtl_Rtl", "Right to left"), TextDirection.RightToLeft),
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

    private readonly IReadOnlyList<MergePoint> _mergePoints =
    [
        new(Loc.Text("Home_Merge_LastWins_Title", "The last class wins"), Loc.Text("Home_Merge_LastWins_Body", "A C# port of tailwind-merge. Nothing to install.")),
        new(Loc.Text("Home_Merge_V4_Title", "Understands Tailwind v4"), Loc.Text("Home_Merge_V4_Body", "Variants, arbitrary values and prefixes, tested on 700+ cases.")),
        new(Loc.Text("Home_Merge_Configure_Title", "Yours to configure"), Loc.Text("Home_Merge_Configure_Body", "Custom utilities, a prefix, or off, from AddTwBlazor.")),
    ];

    /// <summary>
    /// The label of a class merging example, translated. The id and the class stay as they are.
    /// </summary>
    private static string MergeLabel(MergeExample example) => example.Id switch
    {
        "padding" => Loc.Text("Home_Merge_Padding", "Padding"),
        "shorthand" => Loc.Text("Home_Merge_Shorthand", "Shorthand"),
        "shape" => Loc.Text("Home_Merge_Shape", "Shape"),
        "color" => Loc.Text("Home_Merge_Color", "Color"),
        _ => example.Label,
    };

    /// <summary>
    /// The words of a translated hero heading before its highlighted ending, or <see langword="null"/> in English,
    /// whose heading is written out in the page with a delay class per word.
    /// </summary>
    private readonly string[]? _heroWords =
        Loc.Get("Home_Hero_TitleLead")?.Split(' ', StringSplitOptions.RemoveEmptyEntries);

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
    private readonly IReadOnlyList<string> _cycleWords =
    [
        Loc.Text("Home_Cycle_Dashboard", "dashboard"),
        Loc.Text("Home_Cycle_Checkout", "checkout"),
        Loc.Text("Home_Cycle_Admin", "admin panel"),
        Loc.Text("Home_Cycle_SignIn", "sign-in form"),
        Loc.Text("Home_Cycle_Settings", "settings page"),
    ];

    private bool wordCyclePaused;

    // --- Live examples showcase ---

    [Inject] private IJSRuntime js { get; set; } = null!;

    /// <summary>
    /// The colours the showcase can take. Each swatch is drawn with fixed colours, because the showcase remaps the
    /// purple and fuchsia it sits inside. <see cref="ShowcaseSwatch.ThemeClass"/> is defined in input.css.
    /// </summary>
    private readonly IReadOnlyList<ShowcaseSwatch> _showcaseSwatches =
    [
        new("purple", Loc.Text("Home_Showcase_Purple", "Purple"), "bg-[oklch(55.8%_0.288_302.321)]", string.Empty, "#9333ea"),
        new("blue", Loc.Text("Home_Showcase_Blue", "Blue"), "bg-[oklch(54.6%_0.245_262.881)]", "showcase-blue", "#2563eb"),
        new("green", Loc.Text("Home_Showcase_Green", "Green"), "bg-[oklch(59.6%_0.145_163.225)]", "showcase-green", "#059669"),
        new("orange", Loc.Text("Home_Showcase_Orange", "Orange"), "bg-[oklch(64.6%_0.222_41.116)]", "showcase-orange", "#ea580c"),
    ];

    /// <summary>
    /// How many chart types the docs list, rounded down to a ten for the "50+" in the Charts tab. Read from
    /// components.json, leaving out the overview page, so it keeps up as charts are added.
    /// </summary>
    private static readonly int _chartTypeCount =
        ComponentCatalog.LoadLeafEntries().Count(leaf => leaf.Category == "Charts" && leaf.Entry.Url != "/chart") / 10 * 10;

    private static readonly IReadOnlyList<string> _showcaseChartTypes =
        ["Gantt", "Area", "Column", "Line", "Bar", "Pie", "Treemap", "Funnel", "Gauge", "Heatmap"];

    private readonly ChartSeries[] _environmentDeploys =
    [
        new(Loc.Text("Home_Series_Succeeded", "Succeeded"), [96, 214, 342]),
        new(Loc.Text("Home_Failed", "Failed"), [2, 9, 21]),
    ];

    private readonly ChartValue[] _checkoutFunnel =
    [
        new(Loc.Text("Home_Checkout_Account", "Account"), 1200),
        new(Loc.Text("Home_Checkout_Preferences", "Preferences"), 860),
        new(Loc.Text("Home_Checkout_Done", "Done"), 640),
    ];

    private readonly string[] _weekdays = [.. Loc.Culture.DateTimeFormat.AbbreviatedDayNames.Skip(1).Take(5)];

    private readonly ChartSeries[] _environmentWeek;

    private ShowcaseSwatch showcaseSwatch;
    private string showcaseChartType = "Gantt";

    // The showcase follows the site's mode until its own toggle is used. Before then this holds the site's mode,
    // read once the page is interactive, so the toggle's label names the mode it will switch to.
    private bool showcaseDark;
    private bool showcaseModeSet;

    public Home()
    {
        showcaseSwatch = _showcaseSwatches[0];
        _environmentWeek =
        [
            new(_environments[0], [3, 5, 4, 6, 2]),
            new(_environments[1], [8, 11, 9, 12, 7]),
            new(_environments[2], [14, 18, 16, 21, 11]),
        ];
    }

    // No overflow-hidden: the date, time and select popups inside the forms must not be clipped by the frame.
    private string showcaseFrameClasses => string.Join(' ',
        "mt-5 text-gray-950 dark:text-white " + LandingStyles.Figure,
        showcaseSwatch.ThemeClass,
        showcaseModeClass).Trim();

    // Until its own toggle is used, the showcase has no mode class and follows the site.
    private string showcaseModeClass
    {
        get
        {
            if (!showcaseModeSet)
                return string.Empty;

            return showcaseDark ? "dark" : "theme-light";
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender)
            return;

        try
        {
            showcaseDark = await js.InvokeAsync<bool>("themeToggle.isDarkMode");
            StateHasChanged();
        }
        catch (JSException)
        {
            // The theme script is missing, so the toggle starts from light.
        }
        catch (JSDisconnectedException)
        {
            // The circuit went away before the page became interactive.
        }
    }

    /// <summary>
    /// Recolours the showcase. The Forms tab gives its workspace colour picker the same colour.
    /// </summary>
    private void SelectSwatch(ShowcaseSwatch swatch) => showcaseSwatch = swatch;

    // The dashboard's breadcrumb. The links stay on the page: it is a picture of an app, not a way out of this one.
    private readonly List<BreadcrumbItem> _dashboardCrumbs =
    [
        new() { Label = Loc.Text("Nav_Home", "Home"), Href = "#showcase", Icon = IconEnum.House },
        new() { Label = Loc.Text("Home_Crumb_Projects", "Projects"), Href = "#showcase", Icon = IconEnum.Folder2 },
        new() { Label = "Atlas", Href = "#showcase", Icon = IconEnum.Box },
        new() { Label = Loc.Text("Home_Release_Title", "Release health"), Icon = IconEnum.Rocket_Takeoff, AriaCurrent = true },
    ];

    private void ToggleShowcaseMode()
    {
        showcaseDark = !showcaseDark;
        showcaseModeSet = true;
    }

    private const string chartTypeClasses =
        "shrink-0 cursor-pointer rounded-lg px-3 py-1.5 text-sm text-gray-600 transition-colors duration-200 motion-reduce:transition-none hover:text-gray-950 focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-purple-600 dark:text-gray-400 dark:hover:text-white dark:focus-visible:outline-purple-300";

    // An inset ring: the chart type row scrolls sideways, and a ring drawn outside the button would be clipped.
    private const string chartTypeActiveClasses =
        "shrink-0 cursor-pointer rounded-lg bg-white px-3 py-1.5 text-sm font-medium text-purple-700 shadow-xs ring-1 ring-inset ring-purple-300 transition-colors duration-200 motion-reduce:transition-none focus-visible:outline-2 focus-visible:-outline-offset-2 focus-visible:outline-purple-600 forced-colors:underline dark:bg-gray-900 dark:text-purple-200 dark:ring-purple-400/50 dark:focus-visible:outline-purple-300";

    private const string showcaseFooterButtonClasses =
        "inline-flex min-h-9 cursor-pointer items-center gap-1.5 rounded-lg px-3 font-medium text-gray-700 transition-colors hover:bg-gray-200/70 hover:text-gray-950 focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-purple-600 dark:text-gray-200 dark:hover:bg-white/10 dark:hover:text-white dark:focus-visible:outline-purple-300";

    /// <param name="Hex">The swatch's 600 shade, given to the Forms tab's colour picker so it matches the example.</param>
    private sealed record ShowcaseSwatch(string Id, string Label, string SwatchClass, string ThemeClass, string Hex);

    private sealed record Figure(string Label, string Value);

    private sealed record MergeExample(string Id, string Label, string YourClass);

    private sealed record MergePoint(string Title, string Description);

    private sealed record Stat(IconEnum Icon, string Text);

    private sealed record ThemingPoint(IconEnum Icon, string Title, string Description);
}
