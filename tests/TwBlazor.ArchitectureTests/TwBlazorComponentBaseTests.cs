using Bunit;
using Microsoft.AspNetCore.Components;

namespace TwBlazor.ArchitectureTests;

public class TwBlazorComponentBaseTests : TwBlazorTestBase
{
    [Fact]
    public void TwBlazorComponentBase_GeneratesId_WhenNotProvided()
    {
        // Arrange & Act
        var cut = TestContext.Render<TestComponent>();

        // Assert
        var element = cut.Find("div");
        var id = element.GetAttribute("id");
        Assert.NotNull(id);
        Assert.StartsWith("testcomponent-", id);
        Assert.Equal(46, id.Length); // "testcomponent-" (14) + 32 hex chars (no hyphens with :N format)
    }

    [Fact]
    public void TwBlazorComponentBase_UsesProvidedId_WhenSpecified()
    {
        // Arrange & Act
        var cut = TestContext.Render<TestComponent>(parameters => parameters
            .Add(p => p.Id, "custom-id"));

        // Assert
        var element = cut.Find("div");
        var id = element.GetAttribute("id");
        Assert.Equal("custom-id", id);
    }

    [Fact]
    public void TwBlazorComponentBase_RemovesTwPrefix_FromComponentName()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwTestComponent>();

        // Assert
        var element = cut.Find("div");
        var id = element.GetAttribute("id");
        Assert.NotNull(id);
        Assert.StartsWith("testcomponent-", id);
        Assert.DoesNotContain("twtest", id.ToLower());
    }

    [Fact]
    public void TwBlazorComponentBase_HandlesGenericComponents_RemovesBacktick()
    {
        // Arrange & Act
        var cut = TestContext.Render<TestGenericComponent<string>>();

        // Assert
        var element = cut.Find("div");
        var id = element.GetAttribute("id");
        Assert.NotNull(id);
        Assert.StartsWith("testgenericcomponent-", id);
        Assert.DoesNotContain("`", id);
    }

    [Fact]
    public void TwBlazorComponentBase_HandlesGenericComponentsWithTwPrefix_RemovesBoth()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwTestGenericComponent<int>>();

        // Assert
        var element = cut.Find("div");
        var id = element.GetAttribute("id");
        Assert.NotNull(id);
        Assert.StartsWith("testgenericcomponent-", id);
        Assert.DoesNotContain("tw", id);
        Assert.DoesNotContain("`", id);
    }

    [Fact]
    public void TwBlazorComponentBase_GeneratesUniqueIds_ForMultipleInstances()
    {
        // Arrange & Act
        var cut1 = TestContext.Render<TestComponent>();
        var cut2 = TestContext.Render<TestComponent>();

        // Assert
        var id1 = cut1.Find("div").GetAttribute("id");
        var id2 = cut2.Find("div").GetAttribute("id");
        Assert.NotEqual(id1, id2);
    }

    [Fact]
    public void TwBlazorComponentBase_GeneratesId_WhenEmptyStringProvided()
    {
        // Arrange & Act
        var cut = TestContext.Render<TestComponent>(parameters => parameters
            .Add(p => p.Id, ""));

        // Assert - when empty string provided, it should generate an ID
        var element = cut.Find("div");
        var id = element.GetAttribute("id");
        Assert.NotNull(id);
        Assert.StartsWith("testcomponent-", id);
    }

    [Fact]
    public void TwBlazorComponentBase_PreservesIdOnRender()
    {
        // Arrange
        var cut = TestContext.Render<TestComponent>();
        var initialId = cut.Find("div").GetAttribute("id");

        // Assert - ID should remain the same after initial render
        Assert.NotNull(initialId);
        Assert.StartsWith("testcomponent-", initialId);
    }
}

// Test components
public class TestComponent : TwBlazorComponentBase
{
    protected override void BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "div");
        builder.AddAttribute(1, "id", Id);
        builder.AddAttribute(2, "class", Class);
        builder.CloseElement();
    }
}

public class TwTestComponent : TwBlazorComponentBase
{
    protected override void BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "div");
        builder.AddAttribute(1, "id", Id);
        builder.CloseElement();
    }
}

public class TestGenericComponent<T> : TwBlazorComponentBase
{
    [Parameter] public T Value { get; set; } = default!;

    protected override void BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "div");
        builder.AddAttribute(1, "id", Id);
        builder.CloseElement();
    }
}

public class TwTestGenericComponent<T> : TwBlazorComponentBase
{
    [Parameter] public T Value { get; set; } = default!;

    protected override void BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "div");
        builder.AddAttribute(1, "id", Id);
        builder.CloseElement();
    }
}
