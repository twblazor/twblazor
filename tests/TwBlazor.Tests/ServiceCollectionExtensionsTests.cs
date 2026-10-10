using Microsoft.Extensions.DependencyInjection;
using TwBlazor.Builders;
using TwBlazor.Configuration;
using TwBlazor.Configuration.Components;
using TwBlazor.Services;
using TwBlazor.Utilities;
using TwBlazor.Utilities.ClassMerge;
using ThemeBase = TwBlazor.Theme.Theme;

namespace TwBlazor.Tests;

[Collection("ServiceCollectionExtensions")]
public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddTwBlazor_WithTheme_RegistersAllServices()
    {
        // Arrange
        var services = new ServiceCollection();
        var theme = ThemeBase.CreateDefaultTheme();

        // Act
        services.AddTwBlazor(theme);
        var provider = services.BuildServiceProvider();

        // Assert
        Assert.NotNull(provider.GetService<TwBlazorOptions>());
        Assert.NotNull(provider.GetService<TwBlazorTheme>());
        Assert.NotNull(provider.GetService<ITwToastService>());
        Assert.NotNull(provider.GetService<ITwDialogService>());
        Assert.NotNull(provider.GetService<ToastBuilder>());
        Assert.NotNull(provider.GetService<ChipBuilder>());
        Assert.NotNull(provider.GetService<DialogBuilder>());
    }

    [Fact]
    public void AddTwBlazor_WithConfigureAndTheme_AppliesConfiguration()
    {
        // Arrange
        var services = new ServiceCollection();
        var theme = ThemeBase.CreateDefaultTheme();
        var configureWasCalled = false;

        // Act
        services.AddTwBlazor(_ => configureWasCalled = true, theme);
        services.BuildServiceProvider();

        // Assert
        Assert.True(configureWasCalled);
    }

    [Fact]
    public void AddTwBlazor_WithoutClassMergeConfiguration_MergesWithTheDefaults()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddTwBlazor(ThemeBase.CreateDefaultTheme());
        var options = services.BuildServiceProvider().GetRequiredService<TwBlazorOptions>();

        // Assert
        Assert.True(options.ClassMerge.Enabled);
        Assert.Empty(options.ClassMerge.Groups);
        Assert.Equal("px-2", new ClassBuilder("px-4 px-2").Build());
    }

    [Fact]
    public void AddTwBlazor_WithClassMergeConfiguration_AppliesItToClassBuilder()
    {
        // Arrange
        var services = new ServiceCollection();

        try
        {
            // Act
            services.AddTwBlazor(
                options => options.ClassMerge.Groups.Add(new TwClassGroup("service-collection-test", ["service-collection-test"])),
                ThemeBase.CreateDefaultTheme());

            // Assert
            Assert.Equal("service-collection-test-b", new ClassBuilder("service-collection-test-a service-collection-test-b").Build());
        }
        finally
        {
            TwClassMerger.Configure(new TwClassMergeOptions());
        }
    }

    [Fact]
    public void AddTwBlazor_WithThemeFactory_SetsUpdateHandler()
    {
        // Arrange
        var services = new ServiceCollection();
        var theme = ThemeBase.CreateDefaultTheme();

        // Act
        services.AddTwBlazor(_ => { }, () => theme);

        // Assert
        Assert.NotNull(TwBlazorUpdateHandler.options);
        Assert.NotNull(TwBlazorUpdateHandler.themeFactory);
    }

    [Fact]
    public void UpdateApplication_DoesNothing_WhenOptionsIsNull()
    {
        // Arrange
        TwBlazorUpdateHandler.options = null;
        TwBlazorUpdateHandler.themeFactory = null;

        // Capture any relevant initial state
        var initialOptions = TwBlazorUpdateHandler.options;
        var initialFactory = TwBlazorUpdateHandler.themeFactory;

        // Act
        var exception = Record.Exception(() => TwBlazorUpdateHandler.UpdateApplication(null));

        // Assert
        Assert.Null(exception);
        Assert.Null(TwBlazorUpdateHandler.options);
        Assert.Null(TwBlazorUpdateHandler.themeFactory);
        Assert.Equal(initialOptions, TwBlazorUpdateHandler.options);
        Assert.Equal(initialFactory, TwBlazorUpdateHandler.themeFactory);
    }

    [Fact]
    public void UpdateApplication_DoesNothing_WhenThemeFactoryIsNull()
    {
        // Arrange
        var options = new TwBlazorOptions { Theme = ThemeBase.CreateDefaultTheme() };
        TwBlazorUpdateHandler.options = options;
        TwBlazorUpdateHandler.themeFactory = null;

        // Capture initial state
        var initialOptions = TwBlazorUpdateHandler.options;

        // Act
        var exception = Record.Exception(() => TwBlazorUpdateHandler.UpdateApplication(null));

        // Assert
        Assert.Null(exception);
        Assert.NotNull(TwBlazorUpdateHandler.options);
        Assert.Same(initialOptions, TwBlazorUpdateHandler.options);
        Assert.Null(TwBlazorUpdateHandler.themeFactory);
    }

    [Fact]
    public void UpdateApplication_UpdatesTheme_WhenBothAreSet()
    {
        // Arrange
        var theme = ThemeBase.CreateDefaultTheme();

        theme.Components.Require<TwButtonTheme>().Uppercase = "a-new-uppercase-class"; // Set a known value to verify it gets updated

        var newTheme = ThemeBase.CreateDefaultTheme();
        var options = new TwBlazorOptions { Theme = theme };
        TwBlazorUpdateHandler.options = options;
        TwBlazorUpdateHandler.themeFactory = () => newTheme;

        // Act
        TwBlazorUpdateHandler.UpdateApplication(null);

        // Assert
        Assert.Same(newTheme, options.Theme);
    }
}
