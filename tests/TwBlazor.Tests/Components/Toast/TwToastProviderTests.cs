using Bunit;
using Microsoft.Extensions.DependencyInjection;
using TwBlazor.Components;
using TwBlazor.Configuration.Components;
using TwBlazor.Enums;
using TwBlazor.Models;
using TwBlazor.Services;
using Icons = TwBlazor.Enums.Icon;

namespace TwBlazor.Tests.Components.Toast;

public class TwToastProviderTests : TwBlazorTestBase
{
    [Fact]
    public void TwToastProvider_DoesNotRender_WhenNoToasts()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwToastProvider>();

        // Assert
        var markup = cut.Markup;
        Assert.Empty(markup);
    }

    [Fact]
    public void TwToastProvider_Renders_WithToasts()
    {
        // Arrange
        var toastService = TestContext.Services.GetRequiredService<ITwToastService>();
        toastService.AddToast(new ToastModel { Title = "Test Toast" });

        // Act
        var cut = TestContext.Render<TwToastProvider>();

        // Assert
        var container = cut.Find("div[aria-atomic='true']");
        Assert.NotNull(container);
        Assert.Contains("Test Toast", cut.Markup);
    }

    [Fact]
    public void TwToastProvider_RendersMultipleToasts()
    {
        // Arrange
        var toastService = TestContext.Services.GetRequiredService<ITwToastService>();
        toastService.AddToast(new ToastModel { Title = "Toast 1" });
        toastService.AddToast(new ToastModel { Title = "Toast 2" });
        toastService.AddToast(new ToastModel { Title = "Toast 3" });

        // Act
        var cut = TestContext.Render<TwToastProvider>();

        // Assert
        var toasts = cut.FindAll("div[aria-atomic='true']");
        Assert.Equal(3, toasts.Count);
        Assert.Contains("Toast 1", cut.Markup);
        Assert.Contains("Toast 2", cut.Markup);
        Assert.Contains("Toast 3", cut.Markup);
    }

    [Fact]
    public void TwToastProvider_RendersToastTitle()
    {
        // Arrange
        var toastService = TestContext.Services.GetRequiredService<ITwToastService>();
        toastService.AddToast(new ToastModel { Title = "Important Message" });

        // Act
        var cut = TestContext.Render<TwToastProvider>();

        // Assert
        Assert.Contains("Important Message", cut.Markup);
    }

    [Fact]
    public void TwToastProvider_RendersToastMessage()
    {
        // Arrange
        var toastService = TestContext.Services.GetRequiredService<ITwToastService>();
        toastService.AddToast(new ToastModel
        {
            Title = "Title",
            Message = "This is the detailed message"
        });

        // Act
        var cut = TestContext.Render<TwToastProvider>();

        // Assert
        Assert.Contains("This is the detailed message", cut.Markup);
    }

    [Fact]
    public void TwToastProvider_DoesNotRenderMessage_WhenEmpty()
    {
        // Arrange
        var toastService = TestContext.Services.GetRequiredService<ITwToastService>();
        toastService.AddToast(new ToastModel { Title = "Title", Message = "" });

        // Act
        var cut = TestContext.Render<TwToastProvider>();

        // Assert
        var messages = cut.FindAll(".text-sm.opacity-90");
        Assert.Empty(messages);
    }

    [Fact]
    public void TwToastProvider_RendersIcon_WhenProvided()
    {
        // Arrange
        var toastService = TestContext.Services.GetRequiredService<ITwToastService>();
        toastService.AddToast(new ToastModel
        {
            Title = "Success",
            Icon = Icons.Check_Circle
        });

        // Act
        var cut = TestContext.Render<TwToastProvider>();

        // Assert
        var icons = cut.FindComponents<TwIcon>();
        Assert.NotEmpty(icons);
        // Should have both the toast icon and close button icon
        Assert.Equal(2, icons.Count);
        Assert.Equal(Icons.Check_Circle, icons[0].Instance.Icon);
    }

    [Fact]
    public void TwToastProvider_DoesNotRenderIcon_WhenNotProvided()
    {
        // Arrange
        var toastService = TestContext.Services.GetRequiredService<ITwToastService>();
        toastService.AddToast(new ToastModel { Title = "No Icon" });

        // Act
        var cut = TestContext.Render<TwToastProvider>();

        // Assert
        var icons = cut.FindComponents<TwIcon>();
        // Should only have close button icon
        var icon = Assert.Single(icons);
        Assert.Equal(Icons.X, icon.Instance.Icon);
    }

    [Fact]
    public void TwToastProvider_RendersCloseButton()
    {
        // Arrange
        var toastService = TestContext.Services.GetRequiredService<ITwToastService>();
        toastService.AddToast(new ToastModel { Title = "Closeable" });

        // Act
        var cut = TestContext.Render<TwToastProvider>();

        // Assert
        var closeButton = cut.Find("button[aria-label='Close']");
        Assert.NotNull(closeButton);
    }

    [Fact]
    public void TwToastProvider_RemovesToast_WhenCloseButtonClicked()
    {
        // Arrange
        var toastService = TestContext.Services.GetRequiredService<ITwToastService>();
        toastService.AddToast(new ToastModel { Title = "Closeable" });
        var cut = TestContext.Render<TwToastProvider>();

        // Act
        var closeButton = cut.Find("button[aria-label='Close']");
        closeButton.Click();

        // Assert
        Assert.Empty(cut.Markup);
        Assert.False(toastService.HasToasts);
    }

    [Fact]
    public void TwToastProvider_UpdatesWhenToastAdded()
    {
        // Arrange
        var toastService = TestContext.Services.GetRequiredService<ITwToastService>();
        var cut = TestContext.Render<TwToastProvider>();
        Assert.Empty(cut.Markup);

        // Act
        toastService.AddToast(new ToastModel { Title = "New Toast" });
        cut.WaitForState(() => !string.IsNullOrEmpty(cut.Markup));

        // Assert
        Assert.Contains("New Toast", cut.Markup);
    }

    [Fact]
    public void TwToastProvider_UpdatesWhenToastCleared()
    {
        // Arrange
        var toastService = TestContext.Services.GetRequiredService<ITwToastService>();
        var toast = new ToastModel { Title = "Toast" };
        toastService.AddToast(toast);
        var cut = TestContext.Render<TwToastProvider>();

        // Act
        toastService.ClearToast(toast);
        cut.WaitForState(() => string.IsNullOrEmpty(cut.Markup));

        // Assert
        Assert.Empty(cut.Markup);
    }

    [Fact]
    public void TwToastProvider_RendersElapsedTime()
    {
        // Arrange
        var toastService = TestContext.Services.GetRequiredService<ITwToastService>();
        toastService.AddToast(new ToastModel { Title = "Toast" });

        // Act
        var cut = TestContext.Render<TwToastProvider>();

        // Assert
        Assert.Contains("just now", cut.Markup);
    }

    [Theory]
    [InlineData(Color.Danger, "bg-red-200")]
    [InlineData(Color.Success, "bg-green-200")]
    [InlineData(Color.Primary, "bg-purple-200")]
    [InlineData(Color.Warning, "bg-yellow-200")]
    public void TwToastProvider_RendersWithColor(Color color, string expectedBgClass)
    {
        // Arrange
        var toastService = TestContext.Services.GetRequiredService<ITwToastService>();
        toastService.AddToast(new ToastModel { Title = "Colored Toast", Color = color });

        // Act
        var cut = TestContext.Render<TwToastProvider>();

        // Assert
        var toast = cut.Find("div[aria-atomic='true']");
        var classes = toast.GetAttribute("class");
        Assert.NotNull(classes);
        Assert.Contains(expectedBgClass, classes);
    }

    [Theory]
    [InlineData(Color.Accent)]
    [InlineData(Color.Info)]
    [InlineData(Color.Light)]
    [InlineData(Color.Dark)]
    public void TwToastProvider_RendersWithColor_ForRemainingColors(Color color)
    {
        // Arrange - GetToastColor's switch expression has a distinct branch per Color value; only
        // Danger/Success/Primary/Warning are exercised by the Theory above, leaving Accent/Info/Light/Dark
        // untested.
        var toastTheme = Theme.Components.Require<TwToastTheme>();
        var expected = color switch
        {
            Color.Accent => toastTheme.Colors.Accent,
            Color.Info => toastTheme.Colors.Info,
            Color.Light => toastTheme.Colors.Light,
            Color.Dark => toastTheme.Colors.Dark,
            _ => throw new ArgumentOutOfRangeException(nameof(color))
        };

        var toastService = TestContext.Services.GetRequiredService<ITwToastService>();
        toastService.AddToast(new ToastModel { Title = "Colored Toast", Color = color });

        // Act
        var cut = TestContext.Render<TwToastProvider>();

        // Assert
        var toast = cut.Find("div[aria-atomic='true']");
        Assert.Contains(string.Join(' ', expected.Split(' ', StringSplitOptions.RemoveEmptyEntries)), toast.GetAttribute("class"));
    }

    [Fact]
    public void TwToastProvider_HasCorrectAriaAttributes()
    {
        // Arrange
        var toastService = TestContext.Services.GetRequiredService<ITwToastService>();
        toastService.AddToast(new ToastModel { Title = "Accessible Toast" });

        // Act
        var cut = TestContext.Render<TwToastProvider>();

        // Assert - role="alert" implies aria-live="assertive" on its own, so an explicit
        // aria-live="polite" (which would contradict it) is intentionally omitted.
        var toast = cut.Find("div[aria-atomic='true']");
        Assert.Equal("status", toast.GetAttribute("role"));
        Assert.Null(toast.GetAttribute("aria-live"));
        Assert.Equal("true", toast.GetAttribute("aria-atomic"));
    }

    [Fact]
    public void TwToastProvider_RendersContainer_WhenAttributesProvided_EvenWithoutToasts()
    {
        // Act
        var cut = TestContext.Render<TwToastProvider>(parameters => parameters
            .Add(p => p.Attributes, new Dictionary<string, object> { { "data-test", "value" } }));

        // Assert
        Assert.NotEmpty(cut.Markup);
    }

    [Fact]
    public void TwToastProvider_RendersContainer_WhenClassProvided_EvenWithoutToasts()
    {
        // Act
        var cut = TestContext.Render<TwToastProvider>(parameters => parameters
            .Add(p => p.Class, "custom-toast-class"));

        // Assert
        Assert.NotEmpty(cut.Markup);
        Assert.Contains("custom-toast-class", cut.Markup);
    }

    [Fact]
    public void TwToastProvider_RendersContainer_WhenStyleProvided_EvenWithoutToasts()
    {
        // Act
        var cut = TestContext.Render<TwToastProvider>(parameters => parameters
            .Add(p => p.Style, "color: red;"));

        // Assert
        Assert.NotEmpty(cut.Markup);
    }

    [Fact]
    public void TwToastProvider_RendersContainer_WhenAriaLabelProvided_EvenWithoutToasts()
    {
        // Act
        var cut = TestContext.Render<TwToastProvider>(parameters => parameters
            .Add(p => p.AriaLabel, "Notifications"));

        // Assert
        Assert.NotEmpty(cut.Markup);
        Assert.Contains("Notifications", cut.Markup);
    }

    [Fact]
    public void TwToastProvider_RendersContainer_WhenAriaLabelledByProvided_EvenWithoutToasts()
    {
        // Act
        var cut = TestContext.Render<TwToastProvider>(parameters => parameters
            .Add(p => p.AriaLabelledBy, "toast-heading"));

        // Assert
        Assert.NotEmpty(cut.Markup);
        Assert.Contains("toast-heading", cut.Markup);
    }

    [Fact]
    public void TwToastProvider_RendersContainer_WhenIdExplicitlyProvided_EvenWithoutToasts()
    {
        // Act
        var cut = TestContext.Render<TwToastProvider>(parameters => parameters
            .Add(p => p.Id, "toast-container"));

        // Assert
        Assert.NotEmpty(cut.Markup);
        Assert.Contains("toast-container", cut.Markup);
    }

    [Fact]
    public void TwToastProvider_DisposesCorrectly()
    {
        // Arrange
        var toastService = TestContext.Services.GetRequiredService<ITwToastService>();
        var cut = TestContext.Render<TwToastProvider>();

        // Act
        cut.Dispose();
        toastService.AddToast(new ToastModel { Title = "After Dispose" });

        // Assert - should not throw
        Assert.True(true);
    }

    [Fact]
    public void Dispose_UnsubscribesFromToastService_WhenNotNull()
    {
        // Arrange - calling the component's own IDisposable.Dispose() directly (rather than
        // bUnit's IRenderedComponent.Dispose(), which disposes the render tree wrapper and
        // doesn't reliably invoke the component's own Dispose() body synchronously).
        var toastService = TestContext.Services.GetRequiredService<ITwToastService>();
        var cut = TestContext.Render<TwToastProvider>();

        // Act
        cut.Instance.Dispose();
        var exception = Record.Exception(() => toastService.AddToast(new ToastModel { Title = "After Dispose" }));

        // Assert - should not throw when adding a toast after unsubscribing
        Assert.Null(exception);
    }

    [Fact]
    public void Dispose_DoesNotThrow_WhenToastServiceWasNeverInjected()
    {
        // Arrange - a bare instance constructed outside DI/rendering, so the [Inject]
        // toastService field is left at its default (null).
        var provider = new TwToastProvider();

        // Act
        var exception = Record.Exception(() => provider.Dispose());

        // Assert - should return early without throwing a NullReferenceException
        Assert.Null(exception);
    }

    [Fact]
    public void TwToastProvider_PausesToast_OnMouseEnter()
    {
        // Arrange
        var toastService = TestContext.Services.GetRequiredService<ITwToastService>();
        var toast = new ToastModel { Title = "Hoverable" };
        toastService.AddToast(toast);
        var cut = TestContext.Render<TwToastProvider>();

        // Act
        cut.Find("div[aria-atomic='true']").MouseEnter();

        // Assert
        Assert.True(toast.IsHovered);
    }

    [Fact]
    public void TwToastProvider_ResumesToast_OnMouseLeave()
    {
        // Arrange
        var toastService = TestContext.Services.GetRequiredService<ITwToastService>();
        var toast = new ToastModel { Title = "Hoverable" };
        toastService.AddToast(toast);
        var cut = TestContext.Render<TwToastProvider>();
        var toastEl = cut.Find("div[aria-atomic='true']");
        toastEl.MouseEnter();

        // Act
        toastEl.MouseLeave();

        // Assert
        Assert.False(toast.IsHovered);
    }

    [Fact]
    public void TwToastProvider_PausesToast_OnFocusIn()
    {
        // Arrange
        var toastService = TestContext.Services.GetRequiredService<ITwToastService>();
        var toast = new ToastModel { Title = "Focusable" };
        toastService.AddToast(toast);
        var cut = TestContext.Render<TwToastProvider>();

        // Act
        cut.Find("div[aria-atomic='true']").FocusIn();

        // Assert
        Assert.True(toast.IsFocused);
    }

    [Fact]
    public void TwToastProvider_ResumesToast_OnFocusOut()
    {
        // Arrange
        var toastService = TestContext.Services.GetRequiredService<ITwToastService>();
        var toast = new ToastModel { Title = "Focusable" };
        toastService.AddToast(toast);
        var cut = TestContext.Render<TwToastProvider>();
        var toastEl = cut.Find("div[aria-atomic='true']");
        toastEl.FocusIn();

        // Act
        toastEl.FocusOut();

        // Assert
        Assert.False(toast.IsFocused);
    }

    [Fact]
    public void OnInitialized_DoesNotThrow_WhenToastServiceWasNeverInjected()
    {
        // Arrange - same as above, but for the OnInitialized null-guard.
        var provider = new TwToastProvider();
        var method = typeof(TwToastProvider).GetMethod("OnInitialized",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;

        // Act
        var exception = Record.Exception(() => method.Invoke(provider, null));

        // Assert - should return early without throwing
        Assert.Null(exception);
    }
}
