using TwBlazor.Utilities;

namespace TwBlazor.Tests.Utilities;

public class ClassBuilderTests
{
    [Fact]
    public void ShouldBuild_Class()
    {
        // Arrange 
        var inputClasses = "example-class-1 example-class-2 example-class-3";
        var expectedClasses = "example-class-1 example-class-2 example-class-3";

        // Act 
        var classes = new ClassBuilder(inputClasses).Build();

        // Assert 
        Assert.Equal(expectedClasses, classes);
    }

    [Fact]
    public void ShouldTrim_Classes()
    {
        // Arrange 
        var inputClasses = "     example-class-1    ";
        var expectedClasses = inputClasses.Trim();

        // Act 
        var classes = new ClassBuilder(inputClasses).Build();

        // Assert 
        Assert.Equal(expectedClasses, classes);
    }

    [Fact]
    public void ShouldAdd_Class()
    {
        // Arrange 
        var inputClasses = " example-class-1 example-class-2 example-class-3";
        var additionalClass = " example-class-4 ";
        var expectedClasses = "example-class-1 example-class-2 example-class-3 example-class-4";

        // Act 
        var classes = new ClassBuilder(inputClasses)
            .AddClass(additionalClass).Build();

        // Assert 
        Assert.Equal(expectedClasses, classes);
    }

    [Fact]
    public void ShouldAdd_ConditionalClass()
    {
        // Arrange 
        var inputClasses = " example-class-1";
        var conditionalClass = "conditional-class";
        var expectedClasses = "example-class-1 conditional-class";

        // Act 
        var classes = new ClassBuilder(inputClasses)
            .AddClass(conditionalClass, true).Build();

        Assert.Equal(expectedClasses, classes);
    }

    [Fact]
    public void ShouldNotAdd_ConditionalClass()
    {
        // Arrange 
        var inputClasses = " example-class-1";
        var conditionalClass = "conditional-class";
        var expectedClasses = "example-class-1";

        // Act 
        var classes = new ClassBuilder(inputClasses)
            .AddClass(conditionalClass, false).Build();

        // Assert 
        Assert.Equal(expectedClasses, classes);
    }

    [Fact]
    public void ShouldBuild_EmptyClasses()
    {
        // Arrange 
        var inputClasses = " ";

        // Act 
        var classes = new ClassBuilder(inputClasses).Build();

        // Assert 
        Assert.Equal(string.Empty, classes);
    }

    [Fact]
    public void ShouldBuild_EmptyString_WhenInitializedWithEmptyString()
    {
        // Arrange & Act
        var classes = new ClassBuilder(string.Empty).Build();

        // Assert
        Assert.Equal(string.Empty, classes);
    }

    [Fact]
    public void ShouldBuild_EmptyString_WhenInitializedWithNull()
    {
        // Arrange & Act
        var classes = new ClassBuilder(null!).Build();

        // Assert
        Assert.Equal(string.Empty, classes);
    }

    [Fact]
    public void ShouldHandleNull_InAddClass()
    {
        // Arrange & Act
        var classes = new ClassBuilder("base-class")
            .AddClass(null!)
            .Build();

        // Assert
        Assert.Equal("base-class", classes);
    }

    [Fact]
    public void ShouldHandleEmptyString_InAddClass()
    {
        // Arrange & Act
        var classes = new ClassBuilder("base-class")
            .AddClass(string.Empty)
            .Build();

        // Assert
        Assert.Equal("base-class", classes);
    }

    [Fact]
    public void ShouldHandleWhitespace_InAddClass()
    {
        // Arrange & Act
        var classes = new ClassBuilder("base-class")
            .AddClass("   ")
            .Build();

        // Assert
        Assert.Equal("base-class", classes);
    }

    [Fact]
    public void ShouldAddMultipleClasses_Fluently()
    {
        // Arrange & Act
        var classes = new ClassBuilder("base")
            .AddClass("class-1")
            .AddClass("class-2")
            .AddClass("class-3")
            .Build();

        // Assert
        Assert.Equal("base class-1 class-2 class-3", classes);
    }

    [Fact]
    public void ShouldAddMultipleConditionalClasses()
    {
        // Arrange & Act
        var classes = new ClassBuilder("base")
            .AddClass("included-1", true)
            .AddClass("excluded", false)
            .AddClass("included-2", true)
            .Build();

        // Assert
        Assert.Equal("base included-1 included-2", classes);
    }

    [Fact]
    public void ShouldHandleMixedConditionalAndRegularClasses()
    {
        // Arrange & Act
        var classes = new ClassBuilder("base")
            .AddClass("regular-1")
            .AddClass("conditional", true)
            .AddClass("regular-2")
            .AddClass("not-included", false)
            .Build();

        // Assert
        Assert.Equal("base regular-1 conditional regular-2", classes);
    }

    [Fact]
    public void ShouldUseAddValue_Directly()
    {
        // Arrange & Act
        var classes = new ClassBuilder("base")
            .AddValue("-suffix")
            .Build();

        // Assert
        Assert.Equal("base-suffix", classes);
    }

    [Fact]
    public void ShouldHandleComplexChaining()
    {
        // Arrange & Act
        var isActive = true;
        var isDisabled = false;
        var classes = new ClassBuilder("btn")
            .AddClass("btn-primary")
            .AddClass("btn-active", isActive)
            .AddClass("btn-disabled", isDisabled)
            .AddClass("btn-lg")
            .Build();

        // Assert
        Assert.Equal("btn btn-primary btn-active btn-lg", classes);
    }

    [Fact]
    public void ShouldCollapseInternalWhitespace()
    {
        // Arrange & Act - merging splits on whitespace, so extra spaces never reach the element
        var classes = new ClassBuilder("  base  ")
            .AddClass("  class-1  ")
            .AddClass("  class-2  ")
            .Build();

        // Assert
        Assert.Equal("base class-1 class-2", classes);
    }

    [Fact]
    public void ShouldBuildEmptyString_WhenOnlyFalseConditions()
    {
        // Arrange & Act
        var classes = new ClassBuilder(string.Empty)
            .AddClass("class-1", false)
            .AddClass("class-2", false)
            .AddClass("class-3", false)
            .Build();

        // Assert
        Assert.Equal(string.Empty, classes);
    }

    [Fact]
    public void ShouldHandleLongClassList()
    {
        // Arrange & Act
        var classes = new ClassBuilder("base")
            .AddClass("flex")
            .AddClass("items-center")
            .AddClass("justify-between")
            .AddClass("px-4")
            .AddClass("py-2")
            .AddClass("rounded-lg")
            .AddClass("shadow-md")
            .AddClass("bg-blue-600")
            .AddClass("text-white")
            .AddClass("hover:bg-blue-700")
            .Build();

        // Assert
        var expected = "base flex items-center justify-between px-4 py-2 rounded-lg shadow-md bg-blue-600 text-white hover:bg-blue-700";
        Assert.Equal(expected, classes);
    }

    [Fact]
    public void ShouldReturnNewInstance_WhenChainingMethods()
    {
        // Arrange
        var builder = new ClassBuilder("base");

        // Act
        var builder2 = builder.AddClass("class-1");
        var builder3 = builder2.AddClass("class-2");

        // Assert - All should work independently since it's a struct
        Assert.Equal("base class-1 class-2", builder3.Build());
    }

    [Theory]
    [InlineData("", "")]
    [InlineData("single", "single")]
    [InlineData("class-1 class-2", "class-1 class-2")]
    [InlineData("  trimmed  ", "trimmed")]
    [InlineData("   multiple   spaces   ", "multiple spaces")]
    public void ShouldBuild_VariousInputs(string input, string expected)
    {
        // Arrange & Act
        var classes = new ClassBuilder(input).Build();

        // Assert
        Assert.Equal(expected, classes);
    }

    [Theory]
    [InlineData(true, "base active")]
    [InlineData(false, "base")]
    public void ShouldAddConditionalClass_BasedOnCondition(bool condition, string expected)
    {
        // Arrange & Act
        var classes = new ClassBuilder("base")
            .AddClass("active", condition)
            .Build();

        // Assert
        Assert.Equal(expected, classes);
    }

    [Fact]
    public void ShouldHandleDefaultConstructor()
    {
        // Arrange & Act
        var builder = new ClassBuilder();
        var classes = builder.AddClass("test").Build();

        // Assert
        Assert.Equal("test", classes);
    }

    [Fact]
    public void ShouldCreateIndependentBuilders()
    {
        // Arrange
        var builder1 = new ClassBuilder("base-1").AddClass("class-1");
        var builder2 = new ClassBuilder("base-2").AddClass("class-2");

        // Act
        var result1 = builder1.Build();
        var result2 = builder2.Build();

        // Assert
        Assert.Equal("base-1 class-1", result1);
        Assert.Equal("base-2 class-2", result2);
    }

    [Fact]
    public void ShouldHandleDuplicateClasses()
    {
        // Arrange & Act - ClassBuilder doesn't prevent duplicates
        var classes = new ClassBuilder("base")
            .AddClass("duplicate")
            .AddClass("duplicate")
            .Build();

        // Assert - Should contain both (ClassBuilder doesn't deduplicate)
        Assert.Equal("base duplicate duplicate", classes);
    }

    [Fact]
    public void ShouldOverride_ConflictingTailwindClass_WithTheOneAddedLast()
    {
        // Arrange & Act
        var classes = new ClassBuilder("flex px-4 text-sm")
            .AddClass("px-2")
            .AddClass("text-lg")
            .Build();

        // Assert
        Assert.Equal("flex px-2 text-lg", classes);
    }

    [Fact]
    public void ShouldOverride_ConflictingTailwindClass_OnlyWhenConditionIsTrue()
    {
        // Arrange & Act
        var overridden = new ClassBuilder("px-4").AddClass("px-2", true).Build();
        var kept = new ClassBuilder("px-4").AddClass("px-2", false).Build();

        // Assert
        Assert.Equal("px-2", overridden);
        Assert.Equal("px-4", kept);
    }

    [Fact]
    public void ShouldKeep_ClassesThatAreNotTailwindUtilities()
    {
        // Arrange & Act
        var classes = new ClassBuilder("custom-card px-4")
            .AddClass("px-2")
            .AddClass("custom-card-lg")
            .Build();

        // Assert
        Assert.Equal("custom-card px-2 custom-card-lg", classes);
    }

    [Fact]
    public void ShouldNotMerge_ClassesWithDifferentVariants()
    {
        // Arrange & Act
        var classes = new ClassBuilder("px-4 hover:px-4").AddClass("hover:px-2").Build();

        // Assert
        Assert.Equal("px-4 hover:px-2", classes);
    }

    [Fact]
    public void ShouldPreserveClassOrder()
    {
        // Arrange & Act
        var classes = new ClassBuilder("first")
            .AddClass("second")
            .AddClass("third")
            .AddClass("fourth")
            .Build();

        // Assert
        Assert.Equal("first second third fourth", classes);
    }
}
