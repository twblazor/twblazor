using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System.Globalization;
using System.Resources;

namespace TwBlazor.Docs.Layout;

/// <summary>
/// Translates a piece of a docs page. The English stays in the page as the content, and the translation comes from
/// <c>Resources/DocsStrings.&lt;culture&gt;.resx</c> under <see cref="Key"/>. When the current language has no
/// translation the English content is rendered as it is.
/// </summary>
/// <remarks>
/// <para>
/// Keeping the English inline means the docs compiler, which reads the <c>.razor</c> source to write
/// <c>llms.txt</c> and the Markdown pages, still sees the English. It ignores the <c>&lt;Loc&gt;</c> tag and turns
/// every <see cref="Text"/> call back into its English argument.
/// </para>
/// <para>
/// A translation can hold inline HTML (<c>&lt;code&gt;</c>, <c>&lt;a&gt;</c>, <c>&lt;strong&gt;</c>) and is rendered
/// as markup. <c>DocsStrings.resx</c> holds the English of every key for translators; it is never shown, because
/// the English on the site is the content written in the page.
/// </para>
/// </remarks>
public sealed class Loc : ComponentBase
{
    private static readonly ResourceManager _resources =
        new("TwBlazor.Docs.Resources.DocsStrings", typeof(Loc).Assembly);

    /// <summary>
    /// Gets or sets the entry name in <c>DocsStrings.resx</c>.
    /// </summary>
    [Parameter, EditorRequired] public string Key { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the English, rendered when the current language has no translation.
    /// </summary>
    [Parameter] public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Gets whether the current UI culture has docs translations, for formatting that should follow the language
    /// only when the text does.
    /// </summary>
    public static bool IsTranslated => Find(translationCulture) is not null;

    /// <summary>
    /// Gets the current UI culture when the docs are translated into it, and otherwise the invariant culture, so
    /// numbers and dates on an English page keep the formats they always had.
    /// </summary>
    public static CultureInfo Culture => IsTranslated ? CultureInfo.CurrentCulture : CultureInfo.InvariantCulture;

    private static CultureInfo translationCulture => CultureInfo.CurrentUICulture;

    /// <summary>
    /// Returns the translation of <paramref name="key"/>, or <paramref name="english"/> when there is none.
    /// </summary>
    public static string Text(string key, string english) => Get(key) ?? english;

    /// <summary>
    /// Returns the translation of <paramref name="key"/>, or <paramref name="english"/>, with its <c>{0}</c>
    /// placeholders filled in.
    /// </summary>
    public static string Format(string key, string english, params object?[] args) =>
        string.Format(CultureInfo.CurrentCulture, Text(key, english), args);

    /// <summary>
    /// Returns the translation of <paramref name="key"/> for the current UI culture, or <see langword="null"/>.
    /// The neutral English file is never used: its text is a reference copy of what the pages say.
    /// </summary>
    public static string? Get(string key)
    {
        for (var culture = translationCulture; !string.IsNullOrEmpty(culture.Name); culture = culture.Parent)
        {
            if (Load(culture)?.GetString(key) is { } value)
                return value;
        }

        return null;
    }

    // The nearest language file for the culture, walking es-MX to es but never to the neutral English file.
    private static ResourceSet? Find(CultureInfo culture)
    {
        for (var current = culture; !string.IsNullOrEmpty(current.Name); current = current.Parent)
        {
            if (Load(current) is { } set)
                return set;
        }

        return null;
    }

    private static ResourceSet? Load(CultureInfo culture)
    {
        try
        {
            return _resources.GetResourceSet(culture, createIfNotExists: true, tryParents: false);
        }
        catch (MissingManifestResourceException)
        {
            return null;
        }
    }

    /// <inheritdoc />
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        if (Get(Key) is { } translation)
            builder.AddMarkupContent(0, translation);
        else
            builder.AddContent(1, ChildContent);
    }
}
