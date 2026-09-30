using System.Text;

namespace TwBlazor.Docs.Layout;

/// <summary>
/// One entry in a page's "On this page" list.
/// </summary>
/// <param name="Id">The DOM id of the section the entry links to.</param>
/// <param name="Title">The section's heading text.</param>
public sealed record PageOutlineItem(string Id, string Title);

/// <summary>
/// Collects the sections of one docs page so <see cref="PageOutlineNav"/> can list them. <see cref="PageContainer"/>
/// owns one per page and cascades it, and each <see cref="PageCard"/> registers itself on it.
/// </summary>
public sealed class PageOutline
{
    private readonly List<PageOutlineItem> items = [];

    /// <summary>
    /// The registered sections, in the order they were registered.
    /// </summary>
    public IReadOnlyList<PageOutlineItem> Items => items;

    /// <summary>
    /// Raised after a section is added or removed.
    /// </summary>
    public event Action? Changed;

    /// <summary>
    /// Adds a section to the outline.
    /// </summary>
    /// <param name="title">The section's heading text.</param>
    /// <param name="id">The section's DOM id, or <see langword="null"/> to derive one from <paramref name="title"/>.</param>
    /// <returns>The id the section must use. It differs from <paramref name="id"/> only when that id is already taken.</returns>
    public string Register(string title, string? id = null)
    {
        var baseId = string.IsNullOrWhiteSpace(id) ? Slugify(title) : id;
        var uniqueId = baseId;

        var suffix = 2;

        while (items.Exists(item => item.Id == uniqueId))
        {
            uniqueId = $"{baseId}-{suffix++}";
        }

        items.Add(new PageOutlineItem(uniqueId, title));
        Changed?.Invoke();
        return uniqueId;
    }

    /// <summary>
    /// Removes a section from the outline, e.g. when its card is no longer rendered.
    /// </summary>
    /// <param name="id">The id returned by <see cref="Register"/>.</param>
    public void Unregister(string id)
    {
        if (items.RemoveAll(item => item.Id == id) > 0)
        {
            Changed?.Invoke();
        }
    }

    /// <summary>
    /// Turns a heading into a lower-case, hyphen-separated id, e.g. <c>"1. Create the project"</c> becomes
    /// <c>"1-create-the-project"</c>.
    /// </summary>
    /// <param name="title">The heading text.</param>
    /// <returns>The slug, or <c>"section"</c> when the title has no letters or digits.</returns>
    public static string Slugify(string title)
    {
        var slug = new StringBuilder(title.Length);

        foreach (var character in title)
        {
            if (char.IsLetterOrDigit(character))
            {
                slug.Append(char.ToLowerInvariant(character));
            }
            else if (slug.Length > 0 && slug[^1] != '-')
            {
                slug.Append('-');
            }
        }

        var result = slug.ToString().TrimEnd('-');
        return result.Length > 0 ? result : "section";
    }
}
