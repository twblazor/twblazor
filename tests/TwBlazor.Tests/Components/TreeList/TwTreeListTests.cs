using AngleSharp.Dom;
using Bunit;
using Microsoft.AspNetCore.Components;
using TwBlazor.Components;
using IconEnum = TwBlazor.Enums.Icon;

namespace TwBlazor.Tests.Components.TreeList;

public class TwTreeListTests : TwBlazorTestBase
{
    private static bool HasIconClass(IElement element, string iconClass) =>
        (element.GetAttribute("class") ?? string.Empty)
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Contains(iconClass);

    private static RenderFragment SrcFolder(bool collapsed) => builder =>
    {
        builder.OpenComponent<TwTreeListItem>(0);
        builder.AddComponentParameter(1, "Label", "src");
        builder.AddComponentParameter(2, "Collapsed", collapsed);
        builder.AddComponentParameter(3, "ChildContent", (RenderFragment)(childBuilder =>
        {
            childBuilder.OpenComponent<TwTreeListItem>(0);
            childBuilder.AddComponentParameter(1, "Label", "Program.cs");
            childBuilder.CloseComponent();
        }));
        builder.CloseComponent();
    };

    [Fact]
    public void ShowsFolderIcon_ForCollapsedNodeWithChildren_ByDefault()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwTreeList>(parameters => parameters
            .Add(p => p.ChildContent, SrcFolder(collapsed: true)));

        // Assert
        var icons = cut.FindAll("i");
        Assert.Contains(icons, i => HasIconClass(i, "bi-folder"));
        Assert.DoesNotContain(icons, i => HasIconClass(i, "bi-folder2-open"));
    }

    [Fact]
    public void ShowsOpenFolderIcon_ForExpandedNodeWithChildren_ByDefault()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwTreeList>(parameters => parameters
            .Add(p => p.ChildContent, SrcFolder(collapsed: false)));

        // Assert - "bi-folder" is also a prefix of "bi-folder2-open", so this only passes if the
        // check is on the exact class token rather than a substring match.
        var icons = cut.FindAll("i");
        Assert.Contains(icons, i => HasIconClass(i, "bi-folder2-open"));
        Assert.DoesNotContain(icons, i => HasIconClass(i, "bi-folder"));
    }

    [Fact]
    public void ShowsFileIcon_ForLeafNode_ByDefault()
    {
        // Arrange & Act
        RenderFragment tree = builder =>
        {
            builder.OpenComponent<TwTreeListItem>(0);
            builder.AddComponentParameter(1, "Label", "readme.md");
            builder.CloseComponent();
        };
        var cut = TestContext.Render<TwTreeList>(parameters => parameters
            .Add(p => p.ChildContent, tree));

        // Assert
        var icons = cut.FindAll("i");
        Assert.Contains(icons, i => HasIconClass(i, "bi-file-earmark"));
    }

    [Fact]
    public void CustomIcon_OverridesDefault()
    {
        // Arrange & Act
        RenderFragment tree = builder =>
        {
            builder.OpenComponent<TwTreeListItem>(0);
            builder.AddComponentParameter(1, "Label", "readme.md");
            builder.AddComponentParameter(2, "Icon", IconEnum.Star);
            builder.CloseComponent();
        };
        var cut = TestContext.Render<TwTreeList>(parameters => parameters
            .Add(p => p.ChildContent, tree));

        // Assert
        var icons = cut.FindAll("i");
        Assert.Contains(icons, i => HasIconClass(i, "bi-star"));
        Assert.DoesNotContain(icons, i => HasIconClass(i, "bi-file-earmark"));
    }

    [Fact]
    public void HideIcons_HidesItemIcons_ButNotTheToggleChevron()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwTreeList>(parameters => parameters
            .Add(p => p.ChildContent, SrcFolder(collapsed: false))
            .Add(p => p.HideIcons, true));

        // Assert - the expand/collapse chevron is a separate concern from HideIcons and stays visible.
        var icons = cut.FindAll("i");
        Assert.DoesNotContain(icons, i => HasIconClass(i, "bi-folder2-open"));
        Assert.DoesNotContain(icons, i => HasIconClass(i, "bi-file-earmark"));
        Assert.Contains(icons, i => HasIconClass(i, "bi-chevron-right"));
    }

    [Fact]
    public void HideIcons_PropagatesToNestedLevels()
    {
        // Arrange & Act - HideIcons must be visible to every TwTreeListItem via the cascaded root
        // reference, not just the top level.
        RenderFragment tree = builder =>
        {
            builder.OpenComponent<TwTreeListItem>(0);
            builder.AddComponentParameter(1, "Label", "src");
            builder.AddComponentParameter(2, "Collapsed", false);
            builder.AddComponentParameter(3, "ChildContent", (RenderFragment)(childBuilder =>
            {
                childBuilder.OpenComponent<TwTreeListItem>(0);
                childBuilder.AddComponentParameter(1, "Label", "Program.cs");
                childBuilder.CloseComponent();

                childBuilder.OpenComponent<TwTreeListItem>(2);
                childBuilder.AddComponentParameter(3, "Label", "sub");
                childBuilder.AddComponentParameter(4, "Collapsed", false);
                childBuilder.AddComponentParameter(5, "ChildContent", (RenderFragment)(grandchildBuilder =>
                {
                    grandchildBuilder.OpenComponent<TwTreeListItem>(0);
                    grandchildBuilder.AddComponentParameter(1, "Label", "Nested.cs");
                    grandchildBuilder.CloseComponent();
                }));
                childBuilder.CloseComponent();
            }));
            builder.CloseComponent();
        };

        var cut = TestContext.Render<TwTreeList>(parameters => parameters
            .Add(p => p.ChildContent, tree)
            .Add(p => p.HideIcons, true));

        // Assert
        var icons = cut.FindAll("i");
        Assert.DoesNotContain(icons, i => HasIconClass(i, "bi-file-earmark"));
        Assert.DoesNotContain(icons, i => HasIconClass(i, "bi-folder2-open"));
    }

    [Fact]
    public void ToggleChevron_PointsRight_AndRotatesWhenOpen()
    {
        var closed = TestContext.Render<TwTreeList>(parameters => parameters
            .Add(p => p.ChildContent, SrcFolder(collapsed: true)));
        var open = TestContext.Render<TwTreeList>(parameters => parameters
            .Add(p => p.ChildContent, SrcFolder(collapsed: false)));

        var closedChevron = closed.FindAll("i").First(i => HasIconClass(i, "bi-chevron-right"));
        var openChevron = open.FindAll("i").First(i => HasIconClass(i, "bi-chevron-right"));

        Assert.DoesNotContain("rotate-90", closedChevron.GetAttribute("class"));
        Assert.Contains("rotate-90", openChevron.GetAttribute("class"));
    }

    [Fact]
    public void ToggleChevron_HasACircularHoverTarget_SoItReadsAsInteractive()
    {
        var cut = TestContext.Render<TwTreeList>(parameters => parameters
            .Add(p => p.ChildContent, SrcFolder(collapsed: true)));

        var chevron = cut.FindAll("i").First(i => HasIconClass(i, "bi-chevron-right"));

        Assert.Contains("rounded-full", chevron.GetAttribute("class"));
        Assert.Contains("hover:bg-", chevron.GetAttribute("class"));
    }
}
