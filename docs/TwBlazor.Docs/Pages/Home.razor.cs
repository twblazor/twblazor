using System.Globalization;
using TwBlazor.Components;
using TwBlazor.Docs.Services;
using TwBlazor.Models;
using TwBlazor.Utilities;

namespace TwBlazor.Docs.Pages;

public partial class Home
{
    private const string quickStartInstall =
        "dotnet add package twblazor";

    private const string quickStartSnippet =
        "<TwButton Label=\"Get Started\" Color=\"Color.Primary\" />\n" +
        "<TwAlert Color=\"Color.Success\" Dense>Everything is working!</TwAlert>";

    private const string themingSnippet =
        "builder.Services.AddTwBlazor(_ => { }, () => new TwBlazorTheme\n" +
        "{\n" +
        "    // ...position, colors, shadows, rounded...\n" +
        "    Components =\n" +
        "    [\n" +
        "        new TwButtonTheme\n" +
        "        {\n" +
        "            Base = \"inline-flex items-center gap-2 rounded-full font-semibold\",\n" +
        "            Padding = \"px-8\"\n" +
        "        },\n" +
        "        new TwInputTheme\n" +
        "        {\n" +
        "            DefaultInputVariant = InputVariant.Outlined\n" +
        "        }\n" +
        "        // ...every other component you use\n" +
        "    ]\n" +
        "});";

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

    private static readonly IReadOnlyList<MergePoint> _mergePoints =
    [
        new("The last class wins", "Conflicts are settled by what you wrote last, not by the order of Tailwind's stylesheet."),
        new("Understands Tailwind v4", "Variants, arbitrary values, the important modifier and prefixes, checked against tailwind-merge on more than 700 class strings."),
        new("Yours to configure", "Teach it your own utilities, set your Tailwind prefix or turn it off, all from AddTwBlazor."),
    ];

    private MergeExample mergeExample = _mergeExamples[0];

    /// <summary>
    /// Gets the classes the example renders with, merged the way every twblazor component merges them.
    /// </summary>
    private string MergedClasses => new ClassBuilder(mergeThemeClasses).AddClass(mergeExample.YourClass).Build();

    /// <summary>
    /// Gets the example's theme classes, each with whether it survived the merge.
    /// </summary>
    private IEnumerable<(string Name, bool Kept)> MergeThemeClasses
    {
        get
        {
            var kept = MergedClasses.Split(' ').ToHashSet(StringComparer.Ordinal);
            return mergeThemeClasses.Split(' ').Select(name => (name, kept.Contains(name)));
        }
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
}
