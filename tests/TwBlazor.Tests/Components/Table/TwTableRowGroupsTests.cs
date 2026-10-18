using Bunit;
using Microsoft.AspNetCore.Components;
using TwBlazor.Components;
using TwBlazor.Configuration.Components;

namespace TwBlazor.Tests.Components.Table;

public class TwTableRowGroupsTests : TwBlazorTestBase
{
    private TwTableTheme tableTheme => Theme.Components.Require<TwTableTheme>();

    private static RenderFragment<string> Groups(params string[] names) => bodyClass => builder =>
    {
        var sequence = 0;
        foreach (var name in names)
        {
            builder.OpenElement(sequence++, "tbody");
            builder.AddAttribute(sequence++, "class", bodyClass);
            builder.OpenElement(sequence++, "tr");
            builder.OpenElement(sequence++, "th");
            builder.AddAttribute(sequence++, "scope", "rowgroup");
            builder.AddContent(sequence++, name);
            builder.CloseElement();
            builder.CloseElement();
            builder.CloseElement();
        }
    };

    [Fact]
    public void RowGroups_RendersEachGroupAsItsOwnBody()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwTable>(parameters => parameters
            .Add(p => p.RowGroups, Groups("Engineering", "Design")));

        // Assert
        var bodies = cut.FindAll("table > tbody");
        Assert.Equal(2, bodies.Count);
        Assert.Equal(["Engineering", "Design"], bodies.Select(b => b.QuerySelector("th[scope='rowgroup']")!.TextContent));
    }

    [Fact]
    public void RowGroups_ReceiveTheBodyClasses_IncludingStripedHoverableAndBordered()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwTable>(parameters => parameters
            .Add(p => p.Striped, true)
            .Add(p => p.Hoverable, true)
            .Add(p => p.Bordered, true)
            .Add(p => p.BodyClass, "custom-body")
            .Add(p => p.RowGroups, Groups("Engineering")));

        // Assert
        var classes = cut.Find("tbody").GetAttribute("class");
        Assert.Contains(tableTheme.BodyStriped, classes);
        Assert.Contains(tableTheme.BodyHoverable, classes);
        Assert.Contains(tableTheme.RowDivider, classes);
        Assert.Contains("custom-body", classes);
    }

    [Fact]
    public void RowGroups_TakePrecedenceOverTableBody()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwTable>(parameters => parameters
            .Add(p => p.TableBody, RenderFragmentBuilder("single body"))
            .Add(p => p.RowGroups, Groups("Engineering")));

        // Assert
        Assert.Single(cut.FindAll("tbody"));
        Assert.DoesNotContain("single body", cut.Markup);
    }

    [Fact]
    public void RowGroups_RenderBetweenTheHeaderAndTheFooter()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwTable>(parameters => parameters
            .Add(p => p.TableHeader, RenderFragmentBuilder(string.Empty))
            .Add(p => p.TableFooter, RenderFragmentBuilder(string.Empty))
            .Add(p => p.RowGroups, Groups("A", "B")));

        // Assert
        var sections = cut.Find("table").Children.Select(c => c.TagName.ToLowerInvariant()).ToList();
        Assert.Equal(["thead", "tbody", "tbody", "tfoot"], sections);
    }
}
