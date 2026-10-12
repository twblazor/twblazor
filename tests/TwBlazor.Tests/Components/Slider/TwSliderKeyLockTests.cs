// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using Bunit;
using Microsoft.JSInterop;
using TwBlazor.Components;

namespace TwBlazor.Tests.Components.Slider;

public class TwSliderKeyLockTests : TwBlazorTestBase
{
    private const string lockIdentifier = "twSliderLock.set";

    private IRenderedComponent<TwSlider<int>> RenderReadOnly() =>
        TestContext.Render<TwSlider<int>>(p => p
            .Add(x => x.Value, 50)
            .Add(x => x.Min, 0)
            .Add(x => x.Max, 100)
            .Add(x => x.ReadOnly, true));

    private int LockCalls => TestContext.JSInterop.Invocations.Count(invocation => invocation.Identifier == lockIdentifier);

    [Fact]
    public void ReadOnly_LocksTheKeysOnce_AndNotAgainOnLaterRenders()
    {
        // Arrange
        var cut = RenderReadOnly();

        // Act
        cut.Render();

        // Assert
        Assert.Equal(1, LockCalls);
    }

    [Fact]
    public void ReadOnly_WhenTheCircuitHasGone_DoesNotThrow_AndDoesNotRetry()
    {
        // Arrange
        TestContext.JSInterop.SetupVoid(lockIdentifier, _ => true).SetException(new JSDisconnectedException("gone"));

        // Act
        var cut = RenderReadOnly();
        cut.Render();

        // Assert
        Assert.Equal(1, LockCalls);
        Assert.Equal("true", cut.Find("input[type='range']").GetAttribute("aria-readonly"));
    }

    [Fact]
    public void ReadOnly_WhenScriptIsNotAvailableYet_DoesNotThrow_AndRetriesOnTheNextRender()
    {
        // Arrange - what happens while prerendering
        TestContext.JSInterop.SetupVoid(lockIdentifier, _ => true).SetException(new InvalidOperationException("JavaScript interop calls cannot be issued at this time."));

        // Act
        var cut = RenderReadOnly();
        cut.Render();

        // Assert
        Assert.Equal(2, LockCalls);
        Assert.Equal("true", cut.Find("input[type='range']").GetAttribute("aria-readonly"));
    }
}
