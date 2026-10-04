using Microsoft.AspNetCore.Components;
using System.Reflection;
using TwBlazor.Configuration;
using TwBlazor.Configuration.Color;
using TwBlazor.Configuration.Components;
using TwBlazor.Docs.Services;

namespace TwBlazor.Docs.Layout;

/// <summary>
/// Renders a "Theme Configuration" card for a component's theme class, built entirely from
/// reflection over its public properties - no page has to hand-write (and let drift) its own table.
/// </summary>
public partial class ThemeConfigurationCard : ComponentBase
{
    /// <summary>
    /// The DOM id given to this card's <see cref="PageCard"/>, so a link elsewhere on the site (e.g. a
    /// search result) can deep-link straight to it via a <c>#theme-configuration</c> URL fragment.
    /// </summary>
    public const string AnchorId = "theme-configuration";

    // Shared, cross-cutting building blocks (colour palettes, screen positions) that several components'
    // themes reuse as-is, rather than defining their own Tailwind classes. Properties of these types read
    // as "global theme configuration" - split into their own table instead of looking component-specific.
    private static readonly HashSet<Type> _globalTokenTypes =
    [
        typeof(TwBlazorPalette),
        typeof(TwSurfacePalette),
        typeof(TwTextColor),
        typeof(TwBackgroundColor),
        typeof(TwSurfaceColor),
        typeof(TwNeutralTextPalette),
        typeof(TwAnchorPosition),
        typeof(TwBlazorPositioning),
        typeof(TwBlazorDisplay),
        typeof(TwBlazorFlexbox),
        typeof(TwFlexAlignmentScale),
        typeof(TwBlazorSpacing),
        typeof(TwBlazorSpacingScale),
        typeof(TwBlazorInteraction),
        typeof(TwBlazorBorder),
        typeof(TwBlazorBorderWidth),
        typeof(TwBorderNeutralTones),
        typeof(TwBlazorTextTransform),
    ];

    // Theme classes reused verbatim by more than one documented component, keyed by type and valued by the
    // page (title, route) that "owns" it. When the current page isn't that owner, the table is labeled as
    // inherited instead of presented as if it were unique to this component. Hardcoded rather than derived,
    // since nothing about the types themselves says which page is the canonical one (e.g. TwInputTheme is
    // shared by TwSelect and TwTextfield - Textfield is just the one we consider "home").
    private static readonly Dictionary<Type, (string Title, string Route)> _canonicalOwners = new()
    {
        [typeof(TwInputTheme)] = ("TwTextfield", "/textfield"),
        [typeof(TwButtonTheme)] = ("TwButton", "/button"),
        [typeof(TwTableTheme)] = ("TwTable", "/table"),
    };

    [Parameter, EditorRequired]
    public Type ThemeType { get; set; } = null!;

    /// <summary>
    /// The current page's own title (e.g. "TwSelect"), used to detect when <see cref="ThemeType"/> is
    /// actually owned by a different, already-documented component.
    /// </summary>
    [Parameter]
    public string? ComponentTitle { get; set; }

    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    // NavigationManager, not an injected HttpClient: it's always available on both hosts with zero
    // DI wiring, and resolving an absolute URI from it means XmlDocSummary needs no BaseAddress setup
    // for the one-off wasm fetch either (see its remarks for why that fetch exists at all).
    [Inject]
    private NavigationManager navigation { get; set; } = null!;

    private string apiUrl = string.Empty;
    private List<ThemePropertyRow> ownProperties = [];
    private List<ThemePropertyRow> globalProperties = [];
    private (string Title, string Route)? inheritedFrom;

    protected override async Task OnParametersSetAsync()
    {
        apiUrl = $"{XmlDocSummary.ApiBaseUrl}api/{ThemeType.FullName}.html";
        var wasmAssetUri = navigation.ToAbsoluteUri(XmlDocSummary.WasmAssetPath);

        var rows = new List<ThemePropertyRow>();
        foreach (var property in ThemeType.GetProperties(BindingFlags.Public | BindingFlags.Instance).OrderBy(p => p.MetadataToken))
        {
            rows.Add(await BuildRowAsync(property, wasmAssetUri));
        }

        globalProperties = [.. rows.Where(static r => r.IsGlobalToken)];
        ownProperties = [.. rows.Where(static r => !r.IsGlobalToken)];

        inheritedFrom = _canonicalOwners.TryGetValue(ThemeType, out var owner) && owner.Title != ComponentTitle
            ? owner
            : null;
    }

    private async Task<ThemePropertyRow> BuildRowAsync(PropertyInfo property, Uri wasmAssetUri)
    {
        var anchor = $"{ThemeType.FullName}.{property.Name}".Replace('.', '_');
        var docsUrl = $"{apiUrl}#{anchor}";
        var description = await XmlDocSummary.GetSummaryAsync(property, wasmAssetUri);
        var underlyingType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;

        return new ThemePropertyRow(property.Name, FriendlyTypeName(property.PropertyType), description, docsUrl, _globalTokenTypes.Contains(underlyingType));
    }

    private static string FriendlyTypeName(Type type)
    {
        var underlying = Nullable.GetUnderlyingType(type);
        if (underlying is not null)
        {
            return FriendlyTypeName(underlying) + "?";
        }

        if (type == typeof(string))
        {
            return "string";
        }

        if (type == typeof(bool))
        {
            return "bool";
        }

        if (type == typeof(int))
        {
            return "int";
        }

        if (type == typeof(double))
        {
            return "double";
        }

        return type.Name;
    }

    private sealed record ThemePropertyRow(string Name, string TypeDisplay, IReadOnlyList<DescriptionSegment> Description, string DocsUrl, bool IsGlobalToken);
}
