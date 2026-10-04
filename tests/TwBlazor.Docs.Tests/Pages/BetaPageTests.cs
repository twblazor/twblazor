using System.Text.RegularExpressions;
using TwBlazor.Docs.Compiler;
using TwBlazor.Docs.Services;

namespace TwBlazor.Docs.Tests.Pages;

/// <summary>
/// Keeps the "this component is new" warning in step with <c>components.json</c>: every component flagged
/// <c>isNew</c> must tell readers it's in beta, and a page that says so must really be flagged new.
/// </summary>
public partial class BetaPageTests
{
    private static Dictionary<string, string> PageSourceByRoute()
    {
        var pages = new Dictionary<string, string>();

        foreach (var file in Directory.EnumerateFiles(Paths.PagesPath, "*.razor", SearchOption.AllDirectories))
        {
            var source = File.ReadAllText(file);
            var route = RoutePattern().Match(source);
            if (route.Success)
            {
                pages[route.Groups["route"].Value] = source;
            }
        }

        return pages;
    }

    private static bool IsBeta(string source) => BetaAttributePattern().IsMatch(source);

    [Fact]
    public void EveryNewComponentPage_IsMarkedBeta()
    {
        var pages = PageSourceByRoute();
        var newComponents = ComponentCatalog.LoadLeafEntries().Where(leaf => leaf.Entry.IsNew).ToList();

        Assert.NotEmpty(newComponents); // guards against the catalog read silently returning nothing

        var missing = newComponents
            .Where(leaf => !pages.TryGetValue(leaf.Entry.Url, out var source) || !IsBeta(source))
            .Select(leaf => $"{leaf.Entry.Name} ({leaf.Entry.Url})")
            .ToList();

        Assert.True(missing.Count == 0, $"New components whose page doesn't set Beta: {string.Join(", ", missing)}");
    }

    [Fact]
    public void NoPage_IsMarkedBeta_ForAComponentThatIsNotNew()
    {
        var pages = PageSourceByRoute();
        var newUrls = ComponentCatalog.LoadLeafEntries().Where(leaf => leaf.Entry.IsNew).Select(leaf => leaf.Entry.Url).ToHashSet();

        var stale = pages.Where(page => IsBeta(page.Value) && !newUrls.Contains(page.Key)).Select(page => page.Key).ToList();

        Assert.True(stale.Count == 0, $"Pages marked Beta but not flagged isNew in components.json: {string.Join(", ", stale)}");
    }

    [GeneratedRegex("""^@page\s+"(?<route>[^"]+)"\s*$""", RegexOptions.Multiline)]
    private static partial Regex RoutePattern();

    [GeneratedRegex("""<PageContainer\b[^>]*?\sBeta(?:\s|=|/|>)""", RegexOptions.Singleline)]
    private static partial Regex BetaAttributePattern();
}
