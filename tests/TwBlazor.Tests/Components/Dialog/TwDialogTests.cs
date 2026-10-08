using AngleSharp.Dom;
using Bunit;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using TwBlazor.Components;
using TwBlazor.Configuration.Components;
using TwBlazor.Enums;
using TwBlazor.Models;
using TwBlazor.Services;

namespace TwBlazor.Tests.Components.Dialog;

public class TwDialogTests : TwBlazorTestBase
{
    private TwDialogTheme dialogTheme => Theme.Components.Require<TwOverlayTheme>().Dialog;

    private ITwDialogReference CreateReference(TwDialogOptions? options = null, string? title = "Test Title", string message = "Dialog body")
    {
        var service = TestContext.Services.GetRequiredService<ITwDialogService>();
        var reference = service.CreateReference();
        reference.InjectOptions(options ?? new TwDialogOptions());
        reference.InjectTitle(title);
        reference.InjectRenderFragment(builder =>
        {
            builder.OpenElement(0, "p");
            builder.AddAttribute(1, "class", "dialog-body");
            builder.AddContent(2, message);
            builder.CloseElement();
        });
        return reference;
    }

    #region Rendering

    [Fact]
    public void TwDialog_Renders_BackdropAndSurface()
    {
        // Arrange
        var reference = CreateReference();

        // Act
        var cut = TestContext.Render<TwDialog>(p => p.Add(x => x.Reference, reference));

        // Assert
        var surface = cut.Find("div[role='dialog']");
        Assert.NotNull(surface);
        Assert.Equal("true", surface.GetAttribute("aria-modal"));
    }

    [Fact]
    public void TwDialog_Renders_Title()
    {
        // Arrange
        var reference = CreateReference(title: "My Dialog Title");

        // Act
        var cut = TestContext.Render<TwDialog>(p => p.Add(x => x.Reference, reference));

        // Assert
        Assert.Contains("My Dialog Title", cut.Find("h2").TextContent);
    }

    [Fact]
    public void TwDialog_Renders_Content()
    {
        // Arrange
        var reference = CreateReference(message: "Custom body text");

        // Act
        var cut = TestContext.Render<TwDialog>(p => p.Add(x => x.Reference, reference));

        // Assert
        Assert.Contains("Custom body text", cut.Find(".dialog-body").TextContent);
    }

    [Fact]
    public void TwDialog_Renders_CloseButton_ByDefault()
    {
        // Arrange
        var reference = CreateReference();

        // Act
        var cut = TestContext.Render<TwDialog>(p => p.Add(x => x.Reference, reference));

        // Assert
        Assert.NotEmpty(cut.FindAll("button[aria-label='Close']"));
    }

    [Fact]
    public void TwDialog_HidesCloseButton_WhenOptionsCloseButtonFalse()
    {
        // Arrange
        var reference = CreateReference(options: new TwDialogOptions { CloseButton = false });

        // Act
        var cut = TestContext.Render<TwDialog>(p => p.Add(x => x.Reference, reference));

        // Assert
        Assert.Empty(cut.FindAll("button[aria-label='Close']"));
    }

    [Fact]
    public void TwDialog_HidesHeader_WhenNoHeaderTrue()
    {
        // Arrange
        var reference = CreateReference(options: new TwDialogOptions { NoHeader = true });

        // Act
        var cut = TestContext.Render<TwDialog>(p => p.Add(x => x.Reference, reference));

        // Assert
        Assert.Empty(cut.FindAll("h2"));
        Assert.Empty(cut.FindAll("button[aria-label='Close']"));
    }

    [Fact]
    public void TwDialog_OmitsAriaLabelledBy_WhenNoHeaderAndNoAriaLabelledByProvided()
    {
        // Arrange
        var reference = CreateReference(options: new TwDialogOptions { NoHeader = true });

        // Act
        var cut = TestContext.Render<TwDialog>(p => p.Add(x => x.Reference, reference));

        // Assert
        var surface = cut.Find("div[role='dialog']");
        Assert.Null(surface.GetAttribute("aria-labelledby"));
    }

    [Fact]
    public void TwDialog_SetsAriaLabelledBy_ToTitleId_WhenHeaderShown()
    {
        // Arrange
        var reference = CreateReference();

        // Act
        var cut = TestContext.Render<TwDialog>(p => p.Add(x => x.Reference, reference));

        // Assert
        var surface = cut.Find("div[role='dialog']");
        var title = cut.Find("h2");
        Assert.Equal(title.GetAttribute("id"), surface.GetAttribute("aria-labelledby"));
    }

    #endregion

    #region Backdrop click

    [Fact]
    public void TwDialog_BackdropClick_ClosesDialog_WhenEnabled()
    {
        // Arrange
        var reference = CreateReference();
        var dialogService = TestContext.Services.GetRequiredService<ITwDialogService>();
        TwDialogResult? capturedResult = null;
        dialogService.OnDialogCloseRequested += (_, result) => capturedResult = result;
        var cut = TestContext.Render<TwDialog>(p => p.Add(x => x.Reference, reference));

        // Act - click the outer overlay element directly (identified by tabindex=-1)
        cut.Find("div[tabindex='-1']").Click();

        // Assert
        Assert.NotNull(capturedResult);
        Assert.True(capturedResult.Canceled);
    }

    [Fact]
    public void TwDialog_BackdropClick_DoesNotClose_WhenDisabled()
    {
        // Arrange
        var reference = CreateReference(options: new TwDialogOptions { BackdropClick = false });
        var dialogService = TestContext.Services.GetRequiredService<ITwDialogService>();
        var closeRequested = false;
        dialogService.OnDialogCloseRequested += (_, _) => closeRequested = true;
        var cut = TestContext.Render<TwDialog>(p => p.Add(x => x.Reference, reference));

        // Act
        cut.Find("div[tabindex='-1']").Click();

        // Assert
        Assert.False(closeRequested);
    }

    [Fact]
    public void TwDialog_Surface_HasStopPropagationOnClick()
    {
        // Arrange - bUnit does not simulate real DOM event bubbling, so we cannot click through the
        // surface into the backdrop to prove propagation stops. Instead we assert the surface element
        // carries the stopPropagation directive that prevents a real click from reaching the backdrop.
        var reference = CreateReference();

        // Act
        var cut = TestContext.Render<TwDialog>(p => p.Add(x => x.Reference, reference));

        // Assert
        Assert.True(cut.Find("div[role='dialog']").HasAttribute("blazor:onclick:stopPropagation"));
    }

    #endregion

    #region Close button

    [Fact]
    public void TwDialog_CloseButtonClick_ClosesDialog()
    {
        // Arrange
        var reference = CreateReference();
        var dialogService = TestContext.Services.GetRequiredService<ITwDialogService>();
        TwDialogResult? capturedResult = null;
        dialogService.OnDialogCloseRequested += (_, result) => capturedResult = result;
        var cut = TestContext.Render<TwDialog>(p => p.Add(x => x.Reference, reference));

        // Act
        cut.Find("button[aria-label='Close']").Click();

        // Assert
        Assert.NotNull(capturedResult);
        Assert.True(capturedResult.Canceled);
    }

    #endregion

    #region Escape key

    [Fact]
    public void TwDialog_EscapeKey_ClosesDialog_WhenEnabled()
    {
        // Arrange
        var reference = CreateReference();
        var dialogService = TestContext.Services.GetRequiredService<ITwDialogService>();
        TwDialogResult? capturedResult = null;
        dialogService.OnDialogCloseRequested += (_, result) => capturedResult = result;
        var cut = TestContext.Render<TwDialog>(p => p.Add(x => x.Reference, reference));

        // Act
        cut.Find("div[tabindex='-1']").KeyDown(new KeyboardEventArgs { Key = "Escape" });

        // Assert
        Assert.NotNull(capturedResult);
        Assert.True(capturedResult.Canceled);
    }

    [Fact]
    public void TwDialog_EscapeKey_DoesNotClose_WhenDisabled()
    {
        // Arrange
        var reference = CreateReference(options: new TwDialogOptions { CloseOnEscapeKey = false });
        var dialogService = TestContext.Services.GetRequiredService<ITwDialogService>();
        var closeRequested = false;
        dialogService.OnDialogCloseRequested += (_, _) => closeRequested = true;
        var cut = TestContext.Render<TwDialog>(p => p.Add(x => x.Reference, reference));

        // Act
        cut.Find("div[tabindex='-1']").KeyDown(new KeyboardEventArgs { Key = "Escape" });

        // Assert
        Assert.False(closeRequested);
    }

    [Fact]
    public void TwDialog_OtherKey_DoesNotCloseDialog()
    {
        // Arrange
        var reference = CreateReference();
        var dialogService = TestContext.Services.GetRequiredService<ITwDialogService>();
        var closeRequested = false;
        dialogService.OnDialogCloseRequested += (_, _) => closeRequested = true;
        var cut = TestContext.Render<TwDialog>(p => p.Add(x => x.Reference, reference));

        // Act
        cut.Find("div[tabindex='-1']").KeyDown(new KeyboardEventArgs { Key = "Enter" });

        // Assert
        Assert.False(closeRequested);
    }

    #endregion

    #region Sizing

    [Theory]
    [InlineData(DialogMaxWidth.Small, "sm:max-w-lg")]
    [InlineData(DialogMaxWidth.Medium, "sm:max-w-xl")]
    [InlineData(DialogMaxWidth.Large, "sm:max-w-3xl")]
    public void TwDialog_AppliesMaxWidthClass(DialogMaxWidth maxWidth, string expectedClass)
    {
        // Arrange
        var reference = CreateReference(options: new TwDialogOptions { MaxWidth = maxWidth });

        // Act
        var cut = TestContext.Render<TwDialog>(p => p.Add(x => x.Reference, reference));

        // Assert
        Assert.Contains(expectedClass, cut.Find("div[role='dialog']").GetAttribute("class"));
    }

    [Fact]
    public void TwDialog_AppliesFullScreenClasses_WhenFullScreenTrue()
    {
        // Arrange
        var reference = CreateReference(options: new TwDialogOptions { FullScreen = true });

        // Act
        var cut = TestContext.Render<TwDialog>(p => p.Add(x => x.Reference, reference));

        // Assert
        var classes = cut.Find("div[role='dialog']").GetAttribute("class");
        Assert.Contains(dialogTheme.FullScreen, classes);
        Assert.Contains(Theme.Rounded.None, classes);
    }

    [Fact]
    public void TwDialog_AppliesCustomClass_FromOptions()
    {
        // Arrange
        var reference = CreateReference(options: new TwDialogOptions { Class = "my-custom-surface-class" });

        // Act
        var cut = TestContext.Render<TwDialog>(p => p.Add(x => x.Reference, reference));

        // Assert
        Assert.Contains("my-custom-surface-class", cut.Find("div[role='dialog']").GetAttribute("class"));
    }

    [Fact]
    public void TwDialog_AppliesCustomBackdropClass_FromOptions()
    {
        // Arrange
        var reference = CreateReference(options: new TwDialogOptions { BackdropClass = "my-custom-backdrop-class" });

        // Act
        var cut = TestContext.Render<TwDialog>(p => p.Add(x => x.Reference, reference));

        // Assert
        Assert.Contains("my-custom-backdrop-class", cut.Find("div[tabindex='-1']").GetAttribute("class"));
    }

    [Fact]
    public void TwDialog_AppliesDefaultRounded_FromTheme_WhenNoOverrideSet()
    {
        // Regression test: the surface's rounded class must come from RoundedBuilder/the theme's
        // DefaultRounded (via DialogBuilder.GetSurfaceClasses), not be hardcoded or omitted, when
        // neither TwDialogOptions.Rounded nor TwOverlayTheme.DialogRounded is set.
        var reference = CreateReference();

        var cut = TestContext.Render<TwDialog>(p => p.Add(x => x.Reference, reference));

        Assert.Contains(Theme.Rounded.Md, cut.Find("div[role='dialog']").GetAttribute("class"));
    }

    [Fact]
    public void TwDialog_AppliesRoundedOverride_FromOptions()
    {
        // Arrange
        var reference = CreateReference(options: new TwDialogOptions { Rounded = Rounded.Full });

        // Act
        var cut = TestContext.Render<TwDialog>(p => p.Add(x => x.Reference, reference));

        // Assert
        Assert.Contains(Theme.Rounded.Full, cut.Find("div[role='dialog']").GetAttribute("class"));
    }

    [Fact]
    public void TwDialog_AppliesShadowOverride_FromOptions()
    {
        // Arrange
        var reference = CreateReference(options: new TwDialogOptions { Shadow = Shadow.Lg });

        // Act
        var cut = TestContext.Render<TwDialog>(p => p.Add(x => x.Reference, reference));

        // Assert
        Assert.Contains(Theme.Shadows.Lg, cut.Find("div[role='dialog']").GetAttribute("class"));
    }

    #endregion

    #region effectiveAriaLabel

    [Fact]
    public void TwDialog_UsesExplicitAriaLabel_WhenProvided()
    {
        // Arrange
        var reference = CreateReference();

        // Act
        var cut = TestContext.Render<TwDialog>(p => p
            .Add(x => x.Reference, reference)
            .Add(x => x.AriaLabel, "Explicit label"));

        // Assert
        Assert.Equal("Explicit label", cut.Find("div[role='dialog']").GetAttribute("aria-label"));
    }

    [Fact]
    public void TwDialog_FallsBackToTitle_WhenNoHeaderAndNoAriaLabelOrLabelledBy()
    {
        // Arrange
        var reference = CreateReference(options: new TwDialogOptions { NoHeader = true }, title: "Fallback Title");

        // Act
        var cut = TestContext.Render<TwDialog>(p => p.Add(x => x.Reference, reference));

        // Assert
        Assert.Equal("Fallback Title", cut.Find("div[role='dialog']").GetAttribute("aria-label"));
    }

    [Fact]
    public void TwDialog_FallsBackToGenericLabel_WhenNoHeaderAndNoTitleOrAriaLabelOrLabelledBy()
    {
        // Arrange
        var reference = CreateReference(options: new TwDialogOptions { NoHeader = true }, title: null);

        // Act
        var cut = TestContext.Render<TwDialog>(p => p.Add(x => x.Reference, reference));

        // Assert
        Assert.Equal("Dialog", cut.Find("div[role='dialog']").GetAttribute("aria-label"));
    }

    [Fact]
    public void TwDialog_OmitsAriaLabel_WhenNoHeaderButAriaLabelledByProvided()
    {
        // Arrange - an explicitly-supplied AriaLabelledBy should suppress the title/generic fallback.
        var reference = CreateReference(options: new TwDialogOptions { NoHeader = true });

        // Act
        var cut = TestContext.Render<TwDialog>(p => p
            .Add(x => x.Reference, reference)
            .Add(x => x.AriaLabelledBy, "external-label"));

        // Assert
        var surface = cut.Find("div[role='dialog']");
        Assert.Null(surface.GetAttribute("aria-label"));
        Assert.Equal("external-label", surface.GetAttribute("aria-labelledby"));
    }

    [Fact]
    public void TwDialog_OmitsAriaLabel_WhenHeaderShown_AndNoExplicitAriaLabel()
    {
        // Arrange - with the header shown, aria-labelledby (pointing at the title) is the accessible
        // name; effectiveAriaLabel should stay null rather than duplicating it as aria-label too.
        var reference = CreateReference();

        // Act
        var cut = TestContext.Render<TwDialog>(p => p.Add(x => x.Reference, reference));

        // Assert
        Assert.Null(cut.Find("div[role='dialog']").GetAttribute("aria-label"));
    }

    #endregion

    #region Focus management (JS interop)

    [Fact]
    public void TwDialog_OnAfterRender_TrapsFocus_AndFocusesSurface_OnFirstRenderOnly()
    {
        // Arrange
        var reference = CreateReference();

        // Act
        var cut = TestContext.Render<TwDialog>(p => p.Add(x => x.Reference, reference));
        cut.Render(); // trigger a subsequent, non-first render

        // Assert - each only invoked once, on the first render
        Assert.Single(TestContext.JSInterop.Invocations, i => i.Identifier == "twDialog.trapFocus");
        Assert.Single(TestContext.JSInterop.Invocations, i => i.Identifier == "twDialog.focusSurface");
    }

    [Fact]
    public void TwDialog_OnAfterRender_SwallowsJSDisconnectedException()
    {
        // Arrange
        TestContext.JSInterop.SetupVoid("twDialog.trapFocus", _ => true)
            .SetException(new JSDisconnectedException("Circuit disconnected"));
        var reference = CreateReference();

        // Act & Assert - should not throw/propagate during rendering
        var cut = TestContext.Render<TwDialog>(p => p.Add(x => x.Reference, reference));
        Assert.NotNull(cut.Find("div[role='dialog']"));
    }

    [Fact]
    public async Task DisposeAsync_ReleasesFocusTrap_WhenRegistered()
    {
        // Arrange
        var reference = CreateReference();
        var cut = TestContext.Render<TwDialog>(p => p.Add(x => x.Reference, reference));

        // Act
        await cut.Instance.DisposeAsync();

        // Assert
        Assert.Contains(TestContext.JSInterop.Invocations, i => i.Identifier == "twDialog.releaseFocusTrap");
    }

    [Fact]
    public async Task DisposeAsync_DoesNothing_WhenTrapWasNeverRegistered()
    {
        // Arrange - a bare instance constructed outside DI/rendering never runs OnAfterRenderAsync,
        // so trapRegistered stays false. This intentionally sets the Reference parameter outside a component
        // to test the disposal behavior in this edge case scenario.
#pragma warning disable BL0005 // Component parameter should not be set outside of its component
        var dialog = new TwDialog { Reference = CreateReference() };
#pragma warning restore BL0005

        // Act & Assert - should not throw despite JSRuntime never being injected
        await dialog.DisposeAsync();

        // Verify disposal completed without exception
        Assert.True(true, "DisposeAsync completed without throwing");
    }

    [Fact]
    public async Task DisposeAsync_SwallowsJSDisconnectedException()
    {
        // Arrange
        var reference = CreateReference();
        var cut = TestContext.Render<TwDialog>(p => p.Add(x => x.Reference, reference));
        TestContext.JSInterop.SetupVoid("twDialog.releaseFocusTrap", _ => true)
            .SetException(new JSDisconnectedException("Circuit disconnected"));

        // Act & Assert - should not throw
        await cut.Instance.DisposeAsync();

        // Verify disposal completed despite JSDisconnectedException
        Assert.NotNull(cut.Instance);
    }

    [Fact]
    public async Task DisposeAsync_SwallowsInvalidOperationException()
    {
        // Arrange
        var reference = CreateReference();
        var cut = TestContext.Render<TwDialog>(p => p.Add(x => x.Reference, reference));
        TestContext.JSInterop.SetupVoid("twDialog.releaseFocusTrap", _ => true)
            .SetException(new InvalidOperationException("JS interop unavailable"));

        // Act & Assert - should not throw
        await cut.Instance.DisposeAsync();

        // Verify disposal completed despite InvalidOperationException
        Assert.NotNull(cut.Instance);
    }

    #endregion
}
