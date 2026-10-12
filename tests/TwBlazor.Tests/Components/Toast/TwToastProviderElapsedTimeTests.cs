// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using Bunit;
using Microsoft.Extensions.DependencyInjection;
using TwBlazor.Components;
using TwBlazor.Models;
using TwBlazor.Services;

namespace TwBlazor.Tests.Components.Toast;

public class TwToastProviderElapsedTimeTests : TwBlazorTestBase
{
    private IRenderedComponent<TwToastProvider> RenderToastCreated(TimeSpan ago)
    {
        // Duration 0 keeps the toast on screen however long ago it was created.
        TestContext.Services.GetRequiredService<ITwToastService>()
            .AddToast(new ToastModel { Title = "Saved", Duration = 0, CreatedAt = DateTime.UtcNow.Subtract(ago) });

        return TestContext.Render<TwToastProvider>();
    }

    [Theory]
    [InlineData(0, "just now")]
    [InlineData(59, "just now")]
    [InlineData(5 * 60, "5m ago")]
    [InlineData(59 * 60, "59m ago")]
    [InlineData(3 * 60 * 60, "3h ago")]
    [InlineData(23 * 60 * 60, "23h ago")]
    [InlineData(2 * 24 * 60 * 60, "2d ago")]
    public void Timestamp_SaysHowLongAgoTheToastAppeared(int secondsAgo, string expected)
    {
        // Arrange & Act - half a second of slack so a slow run cannot tip a value over its boundary
        var cut = RenderToastCreated(TimeSpan.FromSeconds(secondsAgo).Add(TimeSpan.FromMilliseconds(secondsAgo == 59 ? 0 : 500)));

        // Assert
        Assert.Contains(cut.FindAll("div[aria-hidden='true']"), element => element.TextContent.Trim() == expected);
    }

    [Fact]
    public void Timestamp_IsLocalized()
    {
        // Arrange
        using var context = new BunitContext();
        context.Services.AddTwBlazor(options => options.Localization.Culture = System.Globalization.CultureInfo.GetCultureInfo("es"), TwBlazor.Theme.Theme.CreateDefaultTheme());
        context.JSInterop.Mode = JSRuntimeMode.Loose;
        var localizer = context.Services.GetRequiredService<TwBlazor.Localization.ITwLocalizer>();
        context.Services.GetRequiredService<ITwToastService>()
            .AddToast(new ToastModel { Title = "Guardado", Duration = 0, CreatedAt = DateTime.UtcNow.AddMinutes(-5).AddSeconds(-1) });

        // Act
        var cut = context.Render<TwToastProvider>();

        // Assert
        var expected = localizer[TwBlazor.Localization.TwStrings.Toast_MinutesAgo, 5];
        Assert.NotEqual("5m ago", expected);
        Assert.Contains(cut.FindAll("div[aria-hidden='true']"), element => element.TextContent.Trim() == expected);
    }
}
