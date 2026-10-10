using Microsoft.AspNetCore.Components;
using TwBlazor.Docs.Services;
using TwBlazor.Services;

namespace TwBlazor.Docs.Layout;

/// <summary>
/// Content of the global search dialog (opened via <see cref="Navigation.SearchDialog"/>): a live,
/// as-you-type search across this library's components, their theme configuration, and docfx API
/// documentation.
/// </summary>
public partial class SearchDisplay : ComponentBase
{
    [CascadingParameter] private TwDialogInstance? DialogInstance { get; set; }

    private string searchTerm = string.Empty;
    private bool hasSearched;

    // Announced as the results change, since they are replaced below the field the user is typing in.
    private string statusMessage = string.Empty;
    private IReadOnlyList<SearchResult> componentResults = [];
    private IReadOnlyList<SearchResult> themeResults = [];
    private IReadOnlyList<SearchResult> documentationResults = [];

    /// <summary>
    /// The result groups to render, in display order - components in this library, then their theme
    /// configuration, then docfx class documentation - skipping any group with no matches for the
    /// current search term.
    /// </summary>
    private IEnumerable<(string Title, IReadOnlyList<SearchResult> Results)> sections
    {
        get
        {
            if (componentResults.Count > 0)
            {
                yield return ("Components", componentResults);
            }

            if (themeResults.Count > 0)
            {
                yield return ("Theme", themeResults);
            }

            if (documentationResults.Count > 0)
            {
                yield return ("API Documentation", documentationResults);
            }
        }
    }

    private void OnSearchInput(string value)
    {
        searchTerm = value;
        hasSearched = !string.IsNullOrWhiteSpace(searchTerm);
        (componentResults, themeResults, documentationResults) = SearchIndex.Search(searchTerm);

        var count = componentResults.Count + themeResults.Count + documentationResults.Count;
        statusMessage = !hasSearched
            ? string.Empty
            : count switch
            {
                0 => $"No results for {searchTerm}",
                1 => "1 result",
                _ => $"{count} results"
            };
    }

    // Selecting a result is a successful pick, not the user backing out - Close rather than Cancel.
    // Internal component/theme links navigate via the anchor's own href (Blazor's client-side routing
    // intercepts it); external docfx links open in a new tab via target="_blank". Either way the
    // dialog itself should close so it isn't left open behind the new page/tab.
    private void SelectResult() => DialogInstance?.Close();
}
