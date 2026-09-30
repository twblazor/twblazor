using TwBlazor.Docs.Layout;

namespace TwBlazor.Docs.Tests.Layout;

public class PageOutlineTests
{
    [Theory]
    [InlineData("Colors", "colors")]
    [InlineData("1. Create the project", "1-create-the-project")]
    [InlineData("  Dense   Alerts!  ", "dense-alerts")]
    [InlineData("TwAlert & Friends", "twalert-friends")]
    [InlineData("???", "section")]
    public void Slugify_ProducesALowerCaseHyphenatedId(string title, string expected)
    {
        // Act & Assert
        Assert.Equal(expected, PageOutline.Slugify(title));
    }

    [Fact]
    public void Register_ListsSectionsInTheOrderTheyWereAdded()
    {
        // Arrange
        var outline = new PageOutline();

        // Act
        outline.Register("Basic");
        outline.Register("Colors");

        // Assert
        Assert.Equal(["Basic", "Colors"], outline.Items.Select(item => item.Title));
    }

    [Fact]
    public void Register_UsesTheGivenId_WhenOneIsProvided()
    {
        // Arrange
        var outline = new PageOutline();

        // Act
        var id = outline.Register("Theme Configuration", "theme-configuration");

        // Assert
        Assert.Equal("theme-configuration", id);
        Assert.Equal("theme-configuration", Assert.Single(outline.Items).Id);
    }

    [Fact]
    public void Register_SuffixesTheId_WhenItIsAlreadyTaken()
    {
        // Arrange
        var outline = new PageOutline();
        outline.Register("Create the project");

        // Act
        var second = outline.Register("Create the project");
        var third = outline.Register("Create the project");

        // Assert
        Assert.Equal("create-the-project-2", second);
        Assert.Equal("create-the-project-3", third);
    }

    [Fact]
    public void Register_RaisesChanged()
    {
        // Arrange
        var outline = new PageOutline();
        var raised = 0;
        outline.Changed += () => raised++;

        // Act
        outline.Register("Basic");

        // Assert
        Assert.Equal(1, raised);
    }

    [Fact]
    public void Unregister_RemovesTheSectionAndRaisesChanged()
    {
        // Arrange
        var outline = new PageOutline();
        var id = outline.Register("Basic");
        var raised = 0;
        outline.Changed += () => raised++;

        // Act
        outline.Unregister(id);

        // Assert
        Assert.Empty(outline.Items);
        Assert.Equal(1, raised);
    }

    [Fact]
    public void Unregister_DoesNothing_WhenTheIdIsUnknown()
    {
        // Arrange
        var outline = new PageOutline();
        outline.Register("Basic");
        var raised = 0;
        outline.Changed += () => raised++;

        // Act
        outline.Unregister("missing");

        // Assert
        Assert.Single(outline.Items);
        Assert.Equal(0, raised);
    }
}
