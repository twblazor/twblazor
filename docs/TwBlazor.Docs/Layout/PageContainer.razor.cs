using Microsoft.AspNetCore.Components;
using TwBlazor.Docs.Generated;
using TwBlazor.Docs.Services;

namespace TwBlazor.Docs.Layout;

/// <summary>
/// The shared shell for a documentation page: SEO metadata, heading, description, optional beta notice,
/// page content, and an optional theme configuration card, alongside the page's outline navigation.
/// </summary>
public partial class PageContainer : ComponentBase, IDisposable
{
    [Parameter] public string? Title { get; set; }

    /// <summary>
    /// The page's route (e.g. <c>/card</c>), used for its canonical URL and its "last updated" date.
    /// </summary>
    [Parameter, EditorRequired] public string Path { get; set; } = string.Empty;

    /// <summary>
    /// One or two sentences saying what the page documents. Shown under the heading and reused as the
    /// meta, Open Graph and Twitter description, so what a search result quotes matches what the page says.
    /// </summary>
    [Parameter, EditorRequired] public string Description { get; set; } = string.Empty;

    /// <summary>
    /// The full <c>&lt;title&gt;</c> text, in the <c>{name} | twblazor - {summary}</c> shape. Defaults to
    /// <see cref="SiteMetadata.ComponentTitle"/> built from <see cref="Title"/>; set it explicitly for pages
    /// whose heading isn't the name people search for (e.g. "Get Started").
    /// </summary>
    [Parameter] public string? SeoTitle { get; set; }

    [Parameter] public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// The component's theme class (e.g. <c>typeof(TwDialogTheme)</c>). When set, a "Theme Configuration"
    /// card is appended automatically, reflecting the type's public properties into a table instead of
    /// requiring every page to hand-write (and keep in sync) its own. Set explicitly per page rather than
    /// derived from <see cref="Title"/>, since a handful of theme classes don't share the page's name
    /// (e.g. the "TwDialogProvider" page's theme is <c>TwDialogTheme</c>).
    /// </summary>
    [Parameter] public Type? ThemeType { get; set; }

    /// <summary>
    /// Optional content rendered below the reflected property table - e.g. a worked example of
    /// overriding a couple of the theme's properties.
    /// </summary>
    [Parameter] public RenderFragment? ThemeContent { get; set; }

    /// <summary>
    /// The small label above the heading. Defaults to the component's category in <c>components.json</c>
    /// (e.g. "Forms"), so only pages outside that catalog (e.g. Get Started) need to set it.
    /// </summary>
    [Parameter] public string? Category { get; set; }

    /// <summary>
    /// Marks the page's component as new (the <c>isNew</c> flag in <c>components.json</c>). Shows a warning
    /// alert above the content saying the component may have bugs and where to report them.
    /// </summary>
    [Parameter] public bool Beta { get; set; }

    /// <summary>
    /// Hides the sticky "On this page" list and lets the content use its width, for pages that need the room
    /// (e.g. side by side previews).
    /// </summary>
    [Parameter] public bool HideOutline { get; set; }

    private string containerClass =>
        $"mx-auto flex {(HideOutline ? "max-w-7xl" : "max-w-6xl")} items-start gap-10 p-6 text-sm text-gray-900 dark:text-gray-200";

    private readonly PageOutline _outline = new();
    private DateOnly lastModified;
    private bool hasLastModified;
    private string? category;

    /// <inheritdoc />
    protected override void OnInitialized() => _outline.Changed += OnOutlineChanged;

    /// <inheritdoc />
    protected override void OnParametersSet()
    {
        hasLastModified = PageLastModified.TryGet(Path, out lastModified);
        category = Category ?? ComponentCatalog.LoadLeafEntries()
            .Where(leaf => leaf.Entry.Url == Path)
            .Select(leaf => leaf.Category)
            .FirstOrDefault();
    }

    /// <summary>
    /// Re-renders when a section appears or disappears. Cards nested in a child component (e.g. a tab
    /// panel) register after this page has rendered its outline list, so they need the extra render.
    /// </summary>
    private void OnOutlineChanged() => _ = InvokeAsync(StateHasChanged);

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Unsubscribes from the page outline.
    /// </summary>
    /// <param name="disposing"><see langword="true"/> when called from <see cref="Dispose()"/>.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            _outline.Changed -= OnOutlineChanged;
        }
    }
}
