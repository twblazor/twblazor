// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using TwBlazor.Components;

namespace TwBlazor.Tests.Components.TreeList;

public class TwTreeListInteropTests : TwBlazorTestBase
{
    private const string attachIdentifier = "twRoving.attach";

    private static RenderFragment OneItem() => builder =>
    {
        builder.OpenComponent<TwTreeListItem>(0);
        builder.AddComponentParameter(1, "Label", "Program.cs");
        builder.CloseComponent();
    };

    private IRenderedComponent<TwTreeList> Render() =>
        TestContext.Render<TwTreeList>(p => p.Add(x => x.ChildContent, OneItem()));

    [Fact]
    public void AttachesRovingFocus_ToTheTree_AsATree()
    {
        // Arrange & Act
        Render();

        // Assert
        var invocation = TestContext.JSInterop.Invocations.First(i => i.Identifier == attachIdentifier);
        Assert.Equal("[role=\"treeitem\"]", invocation.Arguments[1]);
        Assert.Equal("tree", invocation.Arguments[2]);
    }

    [Fact]
    public void WhenTheCircuitHasGone_StillRendersTheTree()
    {
        // Arrange
        TestContext.JSInterop.SetupVoid(attachIdentifier, _ => true).SetException(new JSDisconnectedException("gone"));

        // Act
        var cut = Render();

        // Assert
        Assert.Single(cut.FindAll("[role='treeitem']"));
    }

    [Fact]
    public void WhenScriptIsNotAvailableYet_StillRendersTheTree_AndTriesAgainOnTheNextRender()
    {
        // Arrange - what happens while prerendering
        TestContext.JSInterop.SetupVoid(attachIdentifier, _ => true).SetException(new InvalidOperationException("JavaScript interop calls cannot be issued at this time."));

        // Act
        var cut = Render();
        cut.Render();

        // Assert
        Assert.Single(cut.FindAll("[role='treeitem']"));
        Assert.True(TestContext.JSInterop.Invocations.Count(i => i.Identifier == attachIdentifier) >= 2);
    }
}
