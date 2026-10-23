using System.Globalization;

namespace TwBlazor.Docs.Services;

/// <summary>
/// A language the docs site can switch to: its culture code, its name in that language and its name in English.
/// </summary>
public sealed record DocsLanguage(string Code, string NativeName, string EnglishName);

/// <summary>
/// The languages twblazor ships resource files for, offered by the language switcher in the navbar. English is the
/// site's default, so choosing it clears the saved choice instead of saving "en".
/// </summary>
public static class DocsLanguages
{
    /// <summary>
    /// The name of the cookie and the localStorage key that hold the chosen culture. The cookie is the one ASP.NET
    /// Core's <c>CookieRequestCultureProvider</c> reads.
    /// </summary>
    public const string StorageKey = "docs-culture";

    public const string DefaultCode = "en";

    public static IReadOnlyList<DocsLanguage> All { get; } =
    [
        new("en", "English", "English"),
        new("ar", "العربية", "Arabic"),
        new("bn", "বাংলা", "Bengali"),
        new("de", "Deutsch", "German"),
        new("es", "Español", "Spanish"),
        new("fa", "فارسی", "Persian"),
        new("fr", "Français", "French"),
        new("hi", "हिन्दी", "Hindi"),
        new("id", "Bahasa Indonesia", "Indonesian"),
        new("it", "Italiano", "Italian"),
        new("ja", "日本語", "Japanese"),
        new("jv", "Basa Jawa", "Javanese"),
        new("ko", "한국어", "Korean"),
        new("mr", "मराठी", "Marathi"),
        new("pa-Arab", "پنجابی", "Western Punjabi"),
        new("pl", "Polski", "Polish"),
        new("pt", "Português", "Portuguese"),
        new("ru", "Русский", "Russian"),
        new("ta", "தமிழ்", "Tamil"),
        new("te", "తెలుగు", "Telugu"),
        new("tr", "Türkçe", "Turkish"),
        new("ur", "اردو", "Urdu"),
        new("vi", "Tiếng Việt", "Vietnamese"),
        new("zh-Hans", "简体中文", "Chinese (Simplified)"),
        new("zh-Hant", "繁體中文", "Chinese (Traditional)"),
    ];

    /// <summary>
    /// Returns the language whose code is <paramref name="culture"/> or one of its parents, so <c>es-MX</c> finds
    /// Spanish. Anything else, including the invariant culture, is English.
    /// </summary>
    public static DocsLanguage Find(CultureInfo culture)
    {
        for (var current = culture; !string.IsNullOrEmpty(current.Name); current = current.Parent)
        {
            var match = All.FirstOrDefault(l => string.Equals(l.Code, current.Name, StringComparison.OrdinalIgnoreCase));
            if (match is not null)
                return match;
        }

        return All[0];
    }

    /// <summary>
    /// Returns the culture for a saved code, or <see langword="null"/> when the code is missing, English or not one
    /// of <see cref="All"/>.
    /// </summary>
    public static CultureInfo? TryGetCulture(string? code)
    {
        if (string.IsNullOrWhiteSpace(code) || code == DefaultCode || !All.Any(l => l.Code == code))
            return null;

        try
        {
            return CultureInfo.GetCultureInfo(code);
        }
        catch (CultureNotFoundException)
        {
            return null;
        }
    }
}
