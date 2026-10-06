using TwBlazor.Docs.Generated;

namespace TwBlazor.Docs.Services.ThemeBuilder;

/// <summary>
/// A top-level statement from the default theme's shared utility section.
/// </summary>
/// <param name="Name">The declared variable's name.</param>
/// <param name="Text">The statement's source text.</param>
internal sealed record ThemeStatement(string Name, string Text);

/// <summary>
/// One component theme from the default theme's <c>Components</c> list.
/// </summary>
/// <param name="TypeName">The theme type it configures, e.g. <c>TwAlertTheme</c>.</param>
/// <param name="Text">The entry's source text.</param>
internal sealed record ThemeBlock(string TypeName, string Text);

/// <summary>
/// The default <c>Theme.cs</c> split into recombinable pieces, plus which theme types each component needs.
/// Produced from the real file at build time by <c>TwBlazor.BuildTools</c>.
/// </summary>
/// <param name="Usings">The namespaces to import.</param>
/// <param name="Statements">The shared utility statements, in source order.</param>
/// <param name="ReturnPrefix">The start of the returned theme, through the opening bracket of the components list.</param>
/// <param name="Blocks">The component themes, in source order.</param>
/// <param name="ReturnSuffix">The end of the components list and returned theme.</param>
/// <param name="ComponentThemes">Each component type name mapped to every theme type it needs, including those of components it renders.</param>
internal sealed record ThemeTemplate(
    IReadOnlyList<string> Usings,
    IReadOnlyList<ThemeStatement> Statements,
    string ReturnPrefix,
    IReadOnlyList<ThemeBlock> Blocks,
    string ReturnSuffix,
    IReadOnlyDictionary<string, IReadOnlyList<string>> ComponentThemes)
{
    /// <summary>The template built from the generated <see cref="ThemeTemplateData"/>.</summary>
    public static ThemeTemplate Default { get; } = new(
        ThemeTemplateData.Usings,
        [.. ThemeTemplateData.Statements.Select(s => new ThemeStatement(s.Name, s.Text))],
        ThemeTemplateData.ReturnPrefix,
        [.. ThemeTemplateData.Blocks.Select(b => new ThemeBlock(b.TypeName, b.Text))],
        ThemeTemplateData.ReturnSuffix,
        ThemeTemplateData.ComponentThemes.ToDictionary(c => c.Component, c => (IReadOnlyList<string>)c.Themes));

    /// <summary>
    /// Returns every theme type needed by the given components, so a selection never produces a theme that
    /// throws for a missing dependency at runtime. Components the template doesn't know are ignored.
    /// </summary>
    /// <param name="componentNames">Component type names, e.g. <c>TwCalendar</c>.</param>
    public IReadOnlySet<string> ResolveThemeTypes(IEnumerable<string> componentNames) =>
        componentNames
            .Where(ComponentThemes.ContainsKey)
            .SelectMany(name => ComponentThemes[name])
            .ToHashSet(StringComparer.Ordinal);
}
