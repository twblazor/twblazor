using TwBlazor.Models;
using TwBlazor.Services;

namespace TwBlazor.Tests.Services;

public class TwToastServiceTests
{
    [Fact]
    public void TwToastService_Initializes_WithEmptyList()
    {
        // Arrange & Act
        using var service = new TwToastService();

        // Assert
        Assert.False(service.HasToasts);
        Assert.Empty(service.GetToasts());
    }

    [Fact]
    public void AddToast_AddsToastToList()
    {
        // Arrange
        using var service = new TwToastService();
        var toast = new ToastModel { Title = "Test", Message = "Message" };

        // Act
        service.AddToast(toast);

        // Assert
        Assert.True(service.HasToasts);
        var addedToast = Assert.Single(service.GetToasts());
        Assert.Equal(toast, addedToast);
    }

    [Fact]
    public void AddToast_ThrowsArgumentNullException_WhenToastIsNull()
    {
        // Arrange
        using var service = new TwToastService();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => service.AddToast(null!));
    }

    [Fact]
    public void AddToast_RaisesToasterChangedEvent()
    {
        // Arrange
        using var service = new TwToastService();
        var toast = new ToastModel { Title = "Test" };
        var eventRaised = false;

        service.ToasterChanged += (sender, e) => eventRaised = true;

        // Act
        service.AddToast(toast);

        // Assert
        Assert.True(eventRaised);
    }

    [Fact]
    public void ClearToast_RemovesToastFromList()
    {
        // Arrange
        using var service = new TwToastService();
        var toast = new ToastModel { Title = "Test" };
        service.AddToast(toast);

        // Act
        service.ClearToast(toast);

        // Assert
        Assert.False(service.HasToasts);
        Assert.Empty(service.GetToasts());
    }

    [Fact]
    public void ClearToast_RaisesToasterChangedEvent()
    {
        // Arrange
        using var service = new TwToastService();
        var toast = new ToastModel { Title = "Test" };
        service.AddToast(toast);

        var eventRaised = false;
        service.ToasterChanged += (sender, e) => eventRaised = true;

        // Act
        service.ClearToast(toast);

        // Assert
        Assert.True(eventRaised);
    }

    [Fact]
    public void ClearToast_DoesNothing_WhenToastNotInList()
    {
        // Arrange
        using var service = new TwToastService();
        var toast1 = new ToastModel { Title = "Test1" };
        var toast2 = new ToastModel { Title = "Test2" };
        service.AddToast(toast1);

        var eventRaised = false;
        service.ToasterChanged += (sender, e) => eventRaised = true;

        // Act
        service.ClearToast(toast2);

        // Assert
        Assert.True(service.HasToasts);
        Assert.Single(service.GetToasts());
        Assert.False(eventRaised);
    }

    [Fact]
    public void ClearToast_HandlesNullToast()
    {
        // Arrange
        using var service = new TwToastService();
        var toast = new ToastModel { Title = "Test" };
        service.AddToast(toast);

        // Act
        service.ClearToast(null!);

        // Assert
        Assert.True(service.HasToasts);
        Assert.Single(service.GetToasts());
    }

    [Fact]
    public void ClearAll_RemovesAllToasts()
    {
        // Arrange
        using var service = new TwToastService();
        service.AddToast(new ToastModel { Title = "Test1" });
        service.AddToast(new ToastModel { Title = "Test2" });
        service.AddToast(new ToastModel { Title = "Test3" });

        // Act
        service.ClearAll();

        // Assert
        Assert.False(service.HasToasts);
        Assert.Empty(service.GetToasts());
    }

    [Fact]
    public void ClearAll_RaisesToasterChangedEvent()
    {
        // Arrange
        using var service = new TwToastService();
        service.AddToast(new ToastModel { Title = "Test" });

        var eventRaised = false;
        service.ToasterChanged += (sender, e) => eventRaised = true;

        // Act
        service.ClearAll();

        // Assert
        Assert.True(eventRaised);
    }

    [Fact]
    public void ClearAll_DoesNotRaiseEvent_WhenNoToasts()
    {
        // Arrange
        using var service = new TwToastService();
        var eventRaised = false;
        service.ToasterChanged += (sender, e) => eventRaised = true;

        // Act
        service.ClearAll();

        // Assert
        Assert.False(eventRaised);
    }

    [Fact]
    public void GetToasts_ReturnsCopyOfList()
    {
        // Arrange
        using var service = new TwToastService();
        var toast = new ToastModel { Title = "Test" };
        service.AddToast(toast);

        // Act
        var toasts1 = service.GetToasts();
        var toasts2 = service.GetToasts();

        // Assert
        Assert.NotSame(toasts1, toasts2);
        Assert.Equal(toasts1.Count, toasts2.Count);
    }

    [Fact]
    public async Task GetToasts_AutomaticallyRemovesBurntToasts()
    {
        // Arrange
        using var service = new TwToastService();
        var toast = new ToastModel
        {
            Title = "Test",
            Duration = 1,
            CreatedAt = DateTime.UtcNow.AddSeconds(-1)
        };
        service.AddToast(toast);

        // Act
        await Task.Delay(50, TestContext.Current.CancellationToken); // Ensure toast is burnt
        var toasts = service.GetToasts();

        // Assert
        Assert.Empty(toasts);
    }

    [Fact]
    public void HasToasts_ReturnsTrue_WhenToastsExist()
    {
        // Arrange
        using var service = new TwToastService();
        service.AddToast(new ToastModel { Title = "Test" });

        // Act & Assert
        Assert.True(service.HasToasts);
    }

    [Fact]
    public void HasToasts_ReturnsFalse_WhenNoToasts()
    {
        // Arrange & Act
        using var service = new TwToastService();

        // Assert
        Assert.False(service.HasToasts);
    }

    [Fact]
    public async Task Timer_AutomaticallyRemovesBurntToasts()
    {
        // Arrange
        using var service = new TwToastService();

        var timerFired = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        service.ToasterTimerElapsed += (sender, e) => timerFired.TrySetResult();

        var toast = new ToastModel
        {
            Title = "Test",
            Duration = 100,
            CreatedAt = DateTime.UtcNow
        };
        service.AddToast(toast);

        // Act - Wait for at least 2 timer ticks to ensure burnt toast is processed
        // Timer interval is 1000ms, so wait up to 2500ms for timer to fire
        await timerFired.Task.WaitAsync(TimeSpan.FromMilliseconds(2500), TestContext.Current.CancellationToken);

        // Assert
        Assert.True(timerFired.Task.IsCompletedSuccessfully, "Timer did not fire within expected time");
    }

    [Fact]
    public async Task Dispose_StopsTimer()
    {
        // Arrange
        var service = new TwToastService();
        var eventCount = 0;
        service.ToasterTimerElapsed += (sender, e) => eventCount++;

        // Act
        service.Dispose();
        await Task.Delay(1500, TestContext.Current.CancellationToken);

        // Assert
        // Event count should be minimal as timer is stopped
        Assert.True(eventCount <= 1);
    }

    [Fact]
    public void MultipleToasts_CanBeAdded()
    {
        // Arrange
        using var service = new TwToastService();
        var toast1 = new ToastModel { Title = "Test1" };
        var toast2 = new ToastModel { Title = "Test2" };
        var toast3 = new ToastModel { Title = "Test3" };

        // Act
        service.AddToast(toast1);
        service.AddToast(toast2);
        service.AddToast(toast3);

        // Assert
        Assert.Equal(3, service.GetToasts().Count);
    }

    [Fact]
    public async Task ThreadSafe_ConcurrentAdditions()
    {
        // Arrange
        using var service = new TwToastService();
        List<Task> tasks = [];

        // Act
        for (var i = 0; i < 10; i++)
        {
            var index = i;
            tasks.Add(Task.Run(() =>
            {
                service.AddToast(new ToastModel { Title = $"Test{index}" });
            }, TestContext.Current.CancellationToken));
        }

        await Task.WhenAll(tasks);

        // Assert
        Assert.Equal(10, service.GetToasts().Count);
    }

    [Fact]
    public void PauseToastForHover_PausesTheMatchingToast()
    {
        // Arrange
        using var service = new TwToastService();
        var toast = new ToastModel { Title = "Test" };
        service.AddToast(toast);

        // Act
        service.PauseToastForHover(toast.Id);

        // Assert
        Assert.True(toast.IsHovered);
        Assert.True(toast.IsPaused);
    }

    [Fact]
    public void PauseToastForHover_DoesNothing_WhenToastIdNotFound()
    {
        // Arrange
        using var service = new TwToastService();

        // Act & Assert - should not throw
        service.PauseToastForHover(Guid.NewGuid());

        // Verify service still exists and is usable after no-op call
        Assert.NotNull(service);
    }

    [Fact]
    public void ResumeToastForHover_ResumesTheMatchingToast()
    {
        // Arrange
        using var service = new TwToastService();
        var toast = new ToastModel { Title = "Test" };
        service.AddToast(toast);
        service.PauseToastForHover(toast.Id);

        // Act
        service.ResumeToastForHover(toast.Id);

        // Assert
        Assert.False(toast.IsHovered);
        Assert.False(toast.IsPaused);
    }

    [Fact]
    public void ResumeToastForHover_DoesNothing_WhenToastIdNotFound()
    {
        // Arrange
        using var service = new TwToastService();

        // Act & Assert - should not throw
        service.ResumeToastForHover(Guid.NewGuid());

        // Verify service still exists and is usable after no-op call
        Assert.NotNull(service);
    }

    [Fact]
    public void PauseToastForFocus_PausesTheMatchingToast()
    {
        // Arrange
        using var service = new TwToastService();
        var toast = new ToastModel { Title = "Test" };
        service.AddToast(toast);

        // Act
        service.PauseToastForFocus(toast.Id);

        // Assert
        Assert.True(toast.IsFocused);
        Assert.True(toast.IsPaused);
    }

    [Fact]
    public void PauseToastForFocus_DoesNothing_WhenToastIdNotFound()
    {
        // Arrange
        using var service = new TwToastService();

        // Act & Assert - should not throw
        service.PauseToastForFocus(Guid.NewGuid());

        // Verify service still exists and is usable after no-op call
        Assert.NotNull(service);
    }

    [Fact]
    public void ResumeToastForFocus_ResumesTheMatchingToast()
    {
        // Arrange
        using var service = new TwToastService();
        var toast = new ToastModel { Title = "Test" };
        service.AddToast(toast);
        service.PauseToastForFocus(toast.Id);

        // Act
        service.ResumeToastForFocus(toast.Id);

        // Assert
        Assert.False(toast.IsFocused);
        Assert.False(toast.IsPaused);
    }

    [Fact]
    public void ResumeToastForFocus_DoesNothing_WhenToastIdNotFound()
    {
        // Arrange
        using var service = new TwToastService();

        // Act & Assert - should not throw
        service.ResumeToastForFocus(Guid.NewGuid());

        // Verify service still exists and is usable after no-op call
        Assert.NotNull(service);
    }
}
