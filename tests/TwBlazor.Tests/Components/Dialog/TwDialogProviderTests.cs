using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using TwBlazor.Components;
using TwBlazor.Models;
using TwBlazor.Services;
using TwBlazor.Tests.Services;

namespace TwBlazor.Tests.Components.Dialog;

public class TwDialogProviderTests : TwBlazorTestBase
{
    private ITwDialogService DialogService => TestContext.Services.GetRequiredService<ITwDialogService>();

    [Fact]
    public void TwDialogProvider_RendersEmptyContainer_WhenNoDialogsShown()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwDialogProvider>();

        // Assert
        Assert.NotNull(cut.Find("div"));
        Assert.Empty(cut.FindAll("[role='dialog']"));
    }

    [Fact]
    public async Task TwDialogProvider_RendersDialog_WhenServiceShowsOne()
    {
        // Arrange
        var cut = TestContext.Render<TwDialogProvider>();

        // Act
        await DialogService.ShowAsync<DialogTestContent>("Greeting", new TwDialogParameters { ["Message"] = "Hello" });
        await cut.WaitForStateAsync(() => cut.FindAll("[role='dialog']").Count > 0);

        // Assert
        Assert.Single(cut.FindAll("[role='dialog']"));
        Assert.Contains("Greeting", cut.Markup);
        Assert.Contains("Hello", cut.Markup);
    }

    [Fact]
    public async Task TwDialogProvider_RendersMultipleDialogs_WhenServiceShowsMultiple()
    {
        // Arrange
        var cut = TestContext.Render<TwDialogProvider>();

        // Act
        await DialogService.ShowAsync<DialogTestContent>("First", new TwDialogParameters { ["Message"] = "One" });
        await DialogService.ShowAsync<DialogTestContent>("Second", new TwDialogParameters { ["Message"] = "Two" });
        await cut.WaitForStateAsync(() => cut.FindAll("[role='dialog']").Count == 2);

        // Assert
        Assert.Equal(2, cut.FindAll("[role='dialog']").Count);
        Assert.Contains("First", cut.Markup);
        Assert.Contains("Second", cut.Markup);
    }

    [Fact]
    public async Task TwDialogProvider_RemovesDialog_WhenClosedViaService()
    {
        // Arrange
        var cut = TestContext.Render<TwDialogProvider>();
        var reference = await DialogService.ShowAsync<DialogTestContent>("Closable");
        await cut.WaitForStateAsync(() => cut.FindAll("[role='dialog']").Count > 0);

        // Act
        reference.Close();
        await cut.WaitForStateAsync(() => cut.FindAll("[role='dialog']").Count == 0);

        // Assert
        Assert.Empty(cut.FindAll("[role='dialog']"));
    }

    [Fact]
    public async Task TwDialogProvider_KeepsOtherDialogs_WhenOneIsClosed()
    {
        // Arrange
        var cut = TestContext.Render<TwDialogProvider>();
        var reference1 = await DialogService.ShowAsync<DialogTestContent>("Keep me visible");
        await DialogService.ShowAsync<DialogTestContent>("Close me");
        await cut.WaitForStateAsync(() => cut.FindAll("[role='dialog']").Count == 2);

        // Act
        reference1.Close();
        await cut.WaitForStateAsync(() => cut.FindAll("[role='dialog']").Count == 1);

        // Assert
        Assert.Single(cut.FindAll("[role='dialog']"));
        Assert.Contains("Close me", cut.Markup);
        Assert.DoesNotContain("Keep me visible", cut.Markup);
    }

    [Fact]
    public async Task TwDialogProvider_CompletesResultTask_WhenReferenceClosed()
    {
        // Arrange
        var cut = TestContext.Render<TwDialogProvider>();
        var reference = await DialogService.ShowAsync<DialogTestContent>();
        await cut.WaitForStateAsync(() => cut.FindAll("[role='dialog']").Count > 0);

        // Act
        reference.Close(TwDialogResult.Ok("done"));
        await cut.WaitForStateAsync(() => reference.Result.IsCompleted);

        // Assert
        var result = await reference.Result;
        Assert.NotNull(result);
        Assert.Equal("done", result.Data);
    }

    #region Container parameters

    [Fact]
    public void TwDialogProvider_UsesProvidedId()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwDialogProvider>(parameters => parameters
            .Add(p => p.Id, "dialog-root"));

        // Assert
        Assert.Equal("dialog-root", cut.Find("div").GetAttribute("id"));
    }

    [Fact]
    public void TwDialogProvider_AppliesCustomClass()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwDialogProvider>(parameters => parameters
            .Add(p => p.Class, "custom-dialog-provider-class"));

        // Assert
        Assert.Contains("custom-dialog-provider-class", cut.Find("div").GetAttribute("class"));
    }

    [Fact]
    public void TwDialogProvider_AppliesStyle()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwDialogProvider>(parameters => parameters
            .Add(p => p.Style, "color: red;"));

        // Assert
        Assert.Contains("color: red", cut.Find("div").GetAttribute("style"));
    }

    [Fact]
    public void TwDialogProvider_AppliesCustomAttributes()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwDialogProvider>(parameters => parameters
            .Add(p => p.Attributes, new Dictionary<string, object> { { "data-testid", "dialog-host" } }));

        // Assert
        Assert.Equal("dialog-host", cut.Find("div").GetAttribute("data-testid"));
    }

    #endregion

    #region Lifecycle

    [Fact]
    public void TwDialogProvider_DisposesCorrectly()
    {
        // Arrange
        var cut = TestContext.Render<TwDialogProvider>();

        // Act
        cut.Dispose();
        var exception = Record.Exception(() => DialogService.Close(DialogService.CreateReference()));

        // Assert - should not throw
        Assert.Null(exception);
    }

    [Fact]
    public void Dispose_UnsubscribesFromDialogService_WhenNotNull()
    {
        // Arrange
        var cut = TestContext.Render<TwDialogProvider>();

        // Act
        cut.Instance.Dispose();
        var exception = Record.Exception(() => DialogService.Close(DialogService.CreateReference()));

        // Assert - should not throw when closing after unsubscribing
        Assert.Null(exception);
    }

    [Fact]
    public void Dispose_DoesNotThrow_WhenDialogServiceWasNeverInjected()
    {
        // Arrange - a bare instance constructed outside DI/rendering, so the [Inject]
        // dialogService field is left at its default (null).
        var provider = new TwDialogProvider();

        // Act
        var exception = Record.Exception(() => provider.Dispose());

        // Assert
        Assert.Null(exception);
    }

    [Fact]
    public void OnInitialized_DoesNotThrow_WhenDialogServiceWasNeverInjected()
    {
        // Arrange
        var provider = new TwDialogProvider();
        var method = typeof(TwDialogProvider).GetMethod("OnInitialized",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;

        // Act
        var exception = Record.Exception(() => method.Invoke(provider, null));

        // Assert
        Assert.Null(exception);
    }

    #endregion

    #region Missing provider

    [Fact]
    public async Task ShowAsync_ThrowsInvalidOperationException_WhenProviderNotRendered()
    {
        // Arrange - no TwDialogProvider rendered, so nothing subscribes to DialogInstanceAddedAsync

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => DialogService.ShowAsync<DialogTestContent>());
    }

    [Fact]
    public async Task ShowAsync_DoesNotThrow_WhenProviderIsRendered()
    {
        // Arrange
        TestContext.Render<TwDialogProvider>();

        // Act
        var exception = await Record.ExceptionAsync(() => DialogService.ShowAsync<DialogTestContent>());

        // Assert
        Assert.Null(exception);
    }

    #endregion

    #region Focus management (JS interop)

    [Fact]
    public async Task TwDialogProvider_CapturesFocusToken_AndSetsBackgroundInert_WhenFirstDialogShown()
    {
        // Arrange
        TestContext.JSInterop.Setup<string?>("twDialog.captureFocus").SetResult("tw-focus-token");
        var cut = TestContext.Render<TwDialogProvider>();

        // Act
        await DialogService.ShowAsync<DialogTestContent>("Greeting");
        await cut.WaitForStateAsync(() => cut.FindAll("[role='dialog']").Count > 0);

        // Assert
        Assert.Contains(TestContext.JSInterop.Invocations, i => i.Identifier == "twDialog.captureFocus");
        Assert.Contains(TestContext.JSInterop.Invocations, i => i.Identifier == "twDialog.setBackgroundInert");
    }

    [Fact]
    public async Task TwDialogProvider_DoesNotSetBackgroundInertAgain_ForNestedDialog()
    {
        // Arrange - only the outermost dialog needs to inert the background.
        var cut = TestContext.Render<TwDialogProvider>();

        // Act
        await DialogService.ShowAsync<DialogTestContent>("First");
        await cut.WaitForStateAsync(() => cut.FindAll("[role='dialog']").Count == 1);
        await DialogService.ShowAsync<DialogTestContent>("Second");
        await cut.WaitForStateAsync(() => cut.FindAll("[role='dialog']").Count == 2);

        // Assert
        Assert.Single(TestContext.JSInterop.Invocations, i => i.Identifier == "twDialog.setBackgroundInert");
    }

    [Fact]
    public async Task TwDialogProvider_ClearsBackgroundInert_AndRestoresFocus_WhenLastDialogCloses()
    {
        // Arrange
        TestContext.JSInterop.Setup<string?>("twDialog.captureFocus").SetResult("tw-focus-token");
        var cut = TestContext.Render<TwDialogProvider>();
        var reference = await DialogService.ShowAsync<DialogTestContent>("Closable");
        await cut.WaitForStateAsync(() => cut.FindAll("[role='dialog']").Count > 0);

        // Act
        reference.Close();
        await cut.WaitForStateAsync(() => cut.FindAll("[role='dialog']").Count == 0);

        // Assert
        Assert.Contains(TestContext.JSInterop.Invocations, i => i.Identifier == "twDialog.clearBackgroundInert");
        Assert.Contains(TestContext.JSInterop.Invocations,
            i => i.Identifier == "twDialog.restoreFocus" && (string?)i.Arguments[0] == "tw-focus-token");
    }

    [Fact]
    public async Task TwDialogProvider_DoesNotClearBackgroundInert_WhileOtherDialogsStillOpen()
    {
        // Arrange
        var cut = TestContext.Render<TwDialogProvider>();
        var reference1 = await DialogService.ShowAsync<DialogTestContent>("Keep me visible");
        await DialogService.ShowAsync<DialogTestContent>("Close me");
        await cut.WaitForStateAsync(() => cut.FindAll("[role='dialog']").Count == 2);

        // Act
        reference1.Close();
        await cut.WaitForStateAsync(() => cut.FindAll("[role='dialog']").Count == 1);

        // Assert
        Assert.DoesNotContain(TestContext.JSInterop.Invocations, i => i.Identifier == "twDialog.clearBackgroundInert");
    }

    [Fact]
    public async Task TwDialogProvider_DoesNotRestoreFocus_WhenNoTokenWasCaptured()
    {
        // Arrange - captureFocus returns null (e.g. nothing was focused when the dialog opened).
        TestContext.JSInterop.Setup<string?>("twDialog.captureFocus").SetResult(null);
        var cut = TestContext.Render<TwDialogProvider>();
        var reference = await DialogService.ShowAsync<DialogTestContent>("Closable");
        await cut.WaitForStateAsync(() => cut.FindAll("[role='dialog']").Count > 0);

        // Act
        reference.Close();
        await cut.WaitForStateAsync(() => cut.FindAll("[role='dialog']").Count == 0);

        // Assert
        Assert.DoesNotContain(TestContext.JSInterop.Invocations, i => i.Identifier == "twDialog.restoreFocus");
    }

    [Fact]
    public async Task OnDialogInstanceAdded_SwallowsJSDisconnectedException()
    {
        // Arrange
        TestContext.JSInterop.Setup<string?>("twDialog.captureFocus")
            .SetException(new JSDisconnectedException("Circuit disconnected"));
        var cut = TestContext.Render<TwDialogProvider>();

        // Act & Assert - should not throw/propagate
        var exception = await Record.ExceptionAsync(() => DialogService.ShowAsync<DialogTestContent>());
        Assert.Null(exception);
    }

    [Fact]
    public async Task OnDialogInstanceAdded_SwallowsInvalidOperationException()
    {
        // Arrange
        TestContext.JSInterop.Setup<string?>("twDialog.captureFocus")
            .SetException(new InvalidOperationException("JS interop unavailable"));
        var cut = TestContext.Render<TwDialogProvider>();

        // Act & Assert - should not throw/propagate
        var exception = await Record.ExceptionAsync(() => DialogService.ShowAsync<DialogTestContent>());
        Assert.Null(exception);
    }

    [Fact]
    public async Task FinalizeDialogCloseAsync_SwallowsJSDisconnectedException()
    {
        // Arrange
        var cut = TestContext.Render<TwDialogProvider>();
        var reference = await DialogService.ShowAsync<DialogTestContent>("Closable");
        await cut.WaitForStateAsync(() => cut.FindAll("[role='dialog']").Count > 0);
        TestContext.JSInterop.SetupVoid("twDialog.clearBackgroundInert")
            .SetException(new JSDisconnectedException("Circuit disconnected"));

        // Act & Assert - should not throw
        reference.Close();
        await cut.WaitForStateAsync(() => cut.FindAll("[role='dialog']").Count == 0);

        // Verify dialogs are fully cleared despite exception
        Assert.Empty(cut.FindAll("[role='dialog']"));
    }

    [Fact]
    public async Task Dispose_ClearsBackgroundInert_BestEffort_WhenDialogsStillOpen()
    {
        // Arrange
        var cut = TestContext.Render<TwDialogProvider>();
        await DialogService.ShowAsync<DialogTestContent>("Still open");
        await cut.WaitForStateAsync(() => cut.FindAll("[role='dialog']").Count > 0);

        // Act
        cut.Instance.Dispose();

        // Assert
        Assert.Contains(TestContext.JSInterop.Invocations, i => i.Identifier == "twDialog.clearBackgroundInert");
    }

    [Fact]
    public void Dispose_DoesNotClearBackgroundInert_WhenNoDialogsOpen()
    {
        // Arrange
        var cut = TestContext.Render<TwDialogProvider>();

        // Act
        cut.Instance.Dispose();

        // Assert
        Assert.DoesNotContain(TestContext.JSInterop.Invocations, i => i.Identifier == "twDialog.clearBackgroundInert");
    }

    [Fact]
    public async Task Dispose_SwallowsJSDisconnectedException_DuringBestEffortClear()
    {
        // Arrange
        var cut = TestContext.Render<TwDialogProvider>();
        await DialogService.ShowAsync<DialogTestContent>("Still open");
        await cut.WaitForStateAsync(() => cut.FindAll("[role='dialog']").Count > 0);
        TestContext.JSInterop.SetupVoid("twDialog.clearBackgroundInert")
            .SetException(new JSDisconnectedException("Circuit disconnected"));

        // Act & Assert - should not throw
        var exception = Record.Exception(() => cut.Instance.Dispose());
        Assert.Null(exception);
    }

    #endregion
}
