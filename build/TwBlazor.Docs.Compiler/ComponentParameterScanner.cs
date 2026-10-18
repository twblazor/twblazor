using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace TwBlazor.Docs.Compiler;

/// <summary>
/// One <c>[Parameter]</c> property of a component.
/// </summary>
/// <param name="Name">The parameter name, e.g. <c>Label</c>.</param>
/// <param name="Type">The declared type as written in source, e.g. <c>Color?</c>.</param>
/// <param name="Description">The plain text of its XML doc summary, or empty when it has none.</param>
/// <param name="DeclaredIn">The type that declares it, which differs from the component for inherited parameters.</param>
public sealed record ComponentParameter(string Name, string Type, string Description, string DeclaredIn);

/// <summary>
/// A public component type found in the library source, with every parameter it accepts.
/// </summary>
/// <param name="Name">The type name without generic arguments, e.g. <c>TwSelect</c>.</param>
/// <param name="Folder">The absolute folder of the file that declares it.</param>
/// <param name="Summary">The plain text of its XML doc summary, or empty when it has none.</param>
/// <param name="Parameters">Its own parameters first, then inherited ones from the nearest base type outwards.</param>
public sealed record ComponentInfo(string Name, string Folder, string Summary, IReadOnlyList<ComponentParameter> Parameters);

/// <summary>
/// Reads every public, non-abstract <c>Tw*</c> class in the library source and lists the <c>[Parameter]</c>
/// properties it declares or inherits, with their XML doc summaries, so generated docs name only parameters
/// that really exist.
/// </summary>
/// <remarks>
/// The source is parsed, not compiled, so base types are followed by name through the scanned files only.
/// <c>&lt;inheritdoc cref="Type.Member"/&gt;</c> resolves against any scanned type or interface, and a bare
/// <c>&lt;inheritdoc/&gt;</c> against the same member on a base type.
/// </remarks>
public static partial class ComponentParameterScanner
{
    private sealed record TypeSource(string Name, string Folder, string? BaseName, string Summary, bool IsComponent, List<ComponentParameter> Parameters);

    [GeneratedRegex(@"\s+", RegexOptions.None, matchTimeoutMilliseconds: 5000)]
    private static partial Regex WhitespaceRegex();

    /// <summary>
    /// Scans <paramref name="sourceRoot"/> recursively and returns the components keyed by type name.
    /// </summary>
    /// <param name="sourceRoot">The library's source folder.</param>
    public static Dictionary<string, ComponentInfo> Scan(string sourceRoot)
    {
        var types = new Dictionary<string, TypeSource>(StringComparer.Ordinal);
        var memberDocs = new Dictionary<string, string>(StringComparer.Ordinal);

        foreach (var file in Directory.GetFiles(sourceRoot, "*.cs", SearchOption.AllDirectories).Where(f => !IsBuildOutput(f)).Order(StringComparer.Ordinal))
        {
            var root = CSharpSyntaxTree.ParseText(File.ReadAllText(file)).GetRoot();
            var folder = Path.GetDirectoryName(Path.GetFullPath(file))!;

            foreach (var declaration in root.DescendantNodes().OfType<TypeDeclarationSyntax>())
                Collect(declaration, folder, types, memberDocs);
        }

        return types.Values
            .Where(t => t.IsComponent)
            .ToDictionary(t => t.Name, t => new ComponentInfo(t.Name, t.Folder, t.Summary, Resolve(t, types, memberDocs)), StringComparer.Ordinal);
    }

    private static void Collect(TypeDeclarationSyntax declaration, string folder, Dictionary<string, TypeSource> types, Dictionary<string, string> memberDocs)
    {
        var name = declaration.Identifier.Text;
        var type = MergeDeclaration(declaration, folder, types);

        foreach (var property in declaration.Members.OfType<PropertyDeclarationSyntax>())
        {
            var propertyName = property.Identifier.Text;
            var summary = SummaryOf(property);
            if (summary.Length > 0)
                memberDocs[$"{name}.{propertyName}"] = summary;

            if (IsParameter(property) && property.Modifiers.Any(SyntaxKind.PublicKeyword))
                type.Parameters.Add(new ComponentParameter(propertyName, property.Type.ToString(), InheritDocCref(property) is { } cref ? "@inherit:" + cref : summary, name));
        }
    }

    /// <summary>
    /// Records a type the first time one of its declarations is seen. A partial class may carry its base list and
    /// summary on any one of its parts, so a later part fills in whatever an earlier one left out.
    /// </summary>
    private static TypeSource MergeDeclaration(TypeDeclarationSyntax declaration, string folder, Dictionary<string, TypeSource> types)
    {
        var name = declaration.Identifier.Text;
        var baseName = BaseClassName(declaration);

        if (!types.TryGetValue(name, out var type))
        {
            type = new TypeSource(name, folder, baseName, SummaryOf(declaration), IsPublicComponentClass(declaration), []);
        }
        else
        {
            var summary = type.Summary.Length > 0 ? type.Summary : SummaryOf(declaration);
            type = type with { BaseName = type.BaseName ?? baseName, Summary = summary };
        }

        types[name] = type;
        return type;
    }

    private static string? BaseClassName(TypeDeclarationSyntax declaration) =>
        declaration is ClassDeclarationSyntax
            ? declaration.BaseList?.Types.Select(t => StripGenerics(t.Type.ToString())).FirstOrDefault(n => !IsInterfaceName(n))
            : null;

    private static bool IsPublicComponentClass(TypeDeclarationSyntax declaration) =>
        declaration is ClassDeclarationSyntax
        && declaration.Modifiers.Any(SyntaxKind.PublicKeyword)
        && !declaration.Modifiers.Any(SyntaxKind.AbstractKeyword)
        && declaration.Identifier.Text.StartsWith("Tw", StringComparison.Ordinal);

    private static List<ComponentParameter> Resolve(TypeSource type, Dictionary<string, TypeSource> types, Dictionary<string, string> memberDocs)
    {
        var parameters = new List<ComponentParameter>();
        var seen = new HashSet<string>(StringComparer.Ordinal);
        var visited = new HashSet<string>(StringComparer.Ordinal);

        for (var current = type; current is not null && visited.Add(current.Name); current = current.BaseName is { } baseName && types.TryGetValue(baseName, out var next) ? next : null)
        {
            foreach (var parameter in current.Parameters.Where(p => seen.Add(p.Name)))
                parameters.Add(parameter with { Description = Describe(parameter, current, types, memberDocs) });
        }

        return parameters;
    }

    private static string Describe(ComponentParameter parameter, TypeSource owner, Dictionary<string, TypeSource> types, Dictionary<string, string> memberDocs)
    {
        if (parameter.Description.StartsWith("@inherit:", StringComparison.Ordinal))
        {
            var cref = parameter.Description["@inherit:".Length..];
            var key = cref.Contains('.') ? cref : $"{owner.Name}.{cref}";
            return memberDocs.TryGetValue(key, out var inherited) ? inherited : string.Empty;
        }

        if (parameter.Description.Length > 0)
            return parameter.Description;

        // A bare <inheritdoc/> on an override: take the summary of the same member further up the chain.
        for (var current = owner.BaseName is { } b && types.TryGetValue(b, out var t) ? t : null; current is not null; current = current.BaseName is { } n && types.TryGetValue(n, out var next) ? next : null)
        {
            if (memberDocs.TryGetValue($"{current.Name}.{parameter.Name}", out var summary))
                return summary;
        }

        return string.Empty;
    }

    private static bool IsParameter(PropertyDeclarationSyntax property) =>
        property.AttributeLists.SelectMany(l => l.Attributes).Any(a => a.Name.ToString() is "Parameter" or "ParameterAttribute");

    private static string? InheritDocCref(SyntaxNode node)
    {
        var xml = DocXml(node);
        return xml?.Descendants("inheritdoc").Select(e => (string?)e.Attribute("cref")).FirstOrDefault(c => c is not null);
    }

    private static string SummaryOf(SyntaxNode node)
    {
        var summary = DocXml(node)?.Element("summary");
        return summary is null ? string.Empty : ToPlainText(summary);
    }

    private static XElement? DocXml(SyntaxNode node)
    {
        var lines = node.GetLeadingTrivia()
            .Where(t => t.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia))
            .SelectMany(t => t.ToFullString().Split('\n'))
            .Select(l => l.Trim())
            .Where(l => l.StartsWith("///", StringComparison.Ordinal))
            .Select(l => l[3..]);

        var text = string.Join('\n', lines);
        if (text.Length == 0)
            return null;

        try
        {
            return XElement.Parse($"<doc>{text}</doc>");
        }
        catch (System.Xml.XmlException)
        {
            return null;
        }
    }

    /// <summary>
    /// Flattens an XML doc element to one line of Markdown: <c>see</c> references become their last name segment,
    /// <c>c</c> and <c>langword</c> become inline code.
    /// </summary>
    private static string ToPlainText(XElement element)
    {
        var parts = element.Nodes().Select(node => node switch
        {
            XText text => text.Value,
            XElement { Name.LocalName: "c" } code => $"`{code.Value}`",
            XElement { Name.LocalName: "see" or "seealso" } see when see.Attribute("langword") is { } word => $"`{word.Value}`",
            XElement { Name.LocalName: "see" or "seealso" } see when see.Attribute("cref") is { } cref => $"`{ShortCref(cref.Value)}`",
            XElement { Name.LocalName: "paramref" or "typeparamref" } reference => $"`{(string?)reference.Attribute("name")}`",
            XElement child => ToPlainText(child),
            _ => string.Empty,
        });

        return WhitespaceRegex().Replace(string.Concat(parts), " ").Trim();
    }

    private static string ShortCref(string cref)
    {
        var withoutPrefix = cref.Length > 2 && cref[1] == ':' ? cref[2..] : cref;
        var withoutArgs = withoutPrefix.Split('(')[0];
        return StripGenerics(withoutArgs.Split('.')[^1]).Replace('{', '<').Replace('}', '>');
    }

    private static string StripGenerics(string name)
    {
        var lt = name.IndexOfAny(['<', '{']);
        return lt < 0 ? name : name[..lt];
    }

    private static bool IsInterfaceName(string name) => name.Length > 1 && name[0] == 'I' && char.IsUpper(name[1]);

    private static bool IsBuildOutput(string path)
    {
        var separators = new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar };
        return path.Split(separators).Any(segment => segment is "bin" or "obj");
    }
}
