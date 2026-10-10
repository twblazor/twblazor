// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using Microsoft.AspNetCore.Components;
using System.Text;
using TwBlazor.Configuration.Components;
using TwBlazor.Enums;
using TwBlazor.Models;

namespace TwBlazor.Components;

/// <summary>
/// A navigation component which displays an ordered list of the current page navigation.
/// </summary>
public partial class TwBreadcrumb : TwBlazorComponentBase
{
    [Inject] public NavigationManager NavigationManager { get; set; } = null!;

    private TwBreadcrumbTheme theme => options.Theme.Components.Require<TwBreadcrumbTheme>();

    /// <summary>
    /// The list of breadcrumbs to display.
    /// </summary>
    [Parameter] public List<BreadcrumbItem> Breadcrumbs { get; set; } = [];

    /// <summary>
    /// Adds classes to individual breadcrumbs, default value is "capitalize".
    /// </summary>
    [Parameter] public string BreadcrumbClass { get; set; } = "capitalize";

    /// <summary>
    /// The child content rendered in the ordered list.
    /// </summary>
    [Parameter] public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Determines if the breadcrumbs are automatically constructed from the URI.
    /// </summary>
    [Parameter] public bool Auto { get; set; }

    /// <summary>
    /// Gets or sets the text shown between breadcrumbs. Defaults to <c>/</c>.
    /// </summary>
    /// <remarks>
    /// The separator is decorative and hidden from assistive technology, which already announces the trail as a
    /// list. <see cref="SeparatorIcon"/> takes its place when set.
    /// </remarks>
    [Parameter] public string Separator { get; set; } = defaultSeparator;

    /// <summary>
    /// Gets or sets an icon shown between breadcrumbs in place of the <see cref="Separator"/> text, such as
    /// <see cref="Icon.Chevron_Right"/>.
    /// </summary>
    [Parameter] public Icon? SeparatorIcon { get; set; }

    /// <summary>
    /// The separator text used when none is set.
    /// </summary>
    internal const string defaultSeparator = "/";

    /// <summary>
    /// Inline breadcrumb items.
    /// </summary>
    internal List<TwBreadcrumbItem> inlineBreadcrumbs { get; } = [];

    internal void AddItem(TwBreadcrumbItem item)
    {
        if (!inlineBreadcrumbs.Contains(item))
        {
            inlineBreadcrumbs.Add(item);
            StateHasChanged();
        }
    }

    protected override async Task OnInitializedAsync()
    {
        if (!Auto) return;

        // At the site root, ToBaseRelativePath returns "", and "".Split("/") yields a list containing a
        // single empty-string element rather than an empty list. Left unfiltered, that produces one
        // breadcrumb with an empty Label marked as the current page (aria-current="page") - an empty,
        // nameless entry. Filtering out empty/whitespace segments means visiting the root produces zero
        // auto-generated breadcrumbs instead of one broken one, while a single-segment path like "/docs"
        // still produces exactly one breadcrumb ("docs").
        var uri = NavigationManager.ToBaseRelativePath(NavigationManager.Uri)
            .Split("/")
            .Where(segment => !string.IsNullOrWhiteSpace(segment))
            .ToList();

        if (uri.Count > 0)
        {
            var path = new StringBuilder();
            List<BreadcrumbItem> generatedBreadcrumbs = [];
            foreach (var item in uri)
            {
                path.Append('/');
                path.Append(item);
                generatedBreadcrumbs.Add(new BreadcrumbItem { Label = item, Href = path.ToString() });
            }

            // Mark the last generated breadcrumb (the current page) so assistive technology can
            // announce it via aria-current="page" instead of rendering it as a navigable link.
            for (var i = 0; i < generatedBreadcrumbs.Count; i++)
            {
                generatedBreadcrumbs[i].AriaCurrent = i == generatedBreadcrumbs.Count - 1;
            }

            Breadcrumbs.AddRange(generatedBreadcrumbs);
        }
    }
}
