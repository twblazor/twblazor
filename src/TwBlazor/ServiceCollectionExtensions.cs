// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using Microsoft.Extensions.DependencyInjection;
using System.Reflection.Metadata;
using TwBlazor.Builders;
using TwBlazor.Configuration;
using TwBlazor.Services;

[assembly: MetadataUpdateHandler(typeof(TwBlazor.TwBlazorUpdateHandler))]

namespace TwBlazor;

internal static class TwBlazorUpdateHandler
{
    internal static TwBlazorOptions? options { get; set; }
    internal static Func<TwBlazorTheme>? themeFactory { get; set; }

    internal static void UpdateApplication(Type[]? _)
    {
        if (options is null || themeFactory is null) return;
        options.Theme = themeFactory();
    }
}

/// <summary>
/// Extension methods for configuring TwBlazor services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds TwBlazor services to the service collection with default configuration.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="theme">The theme configuration.</param>
    /// <returns>The service collection for method chaining.</returns>
    public static IServiceCollection AddTwBlazor(this IServiceCollection services, TwBlazorTheme theme)
    {
        return services.AddTwBlazor(_ => { }, theme);
    }

    /// <summary>
    /// Adds TwBlazor services to the service collection with custom configuration.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">Action to configure TwBlazor options.</param>
    /// <param name="theme">The theme configuration.</param>
    /// <returns>The service collection for method chaining.</returns>
    public static IServiceCollection AddTwBlazor(this IServiceCollection services, Action<TwBlazorOptions> configure, TwBlazorTheme theme)
    {
        return services.AddTwBlazor(configure, () => theme);
    }

    /// <summary>
    /// Adds TwBlazor services to the service collection with a theme factory, enabling hot reload support.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">Action to configure TwBlazor options.</param>
    /// <param name="themeFactory">A factory function that creates the theme. Called again on hot reload.</param>
    /// <returns>The service collection for method chaining.</returns>
    public static IServiceCollection AddTwBlazor(this IServiceCollection services, Action<TwBlazorOptions> configure, Func<TwBlazorTheme> themeFactory)
    {
        var theme = themeFactory();
        var options = new TwBlazorOptions { Theme = theme };
        configure(options);
        TwBlazorUpdateHandler.options = options;
        TwBlazorUpdateHandler.themeFactory = themeFactory;
        services.AddSingleton(options);
        services.AddSingleton(theme);
        services.AddScoped<ITwToastService, TwToastService>();
        services.AddScoped<ITwDialogService, TwDialogService>();
        services.AddScoped<ColorBuilder>();
        services.AddScoped<IconColorBuilder>();
        services.AddScoped<ShadowBuilder>();
        services.AddScoped<RoundedBuilder>();
        services.AddScoped<ButtonBuilder>();
        services.AddScoped<InputVariantBuilder>();
        services.AddScoped<ChipBuilder>();
        services.AddScoped<ToastBuilder>();
        services.AddScoped<DialogBuilder>();
        services.AddScoped<PopoverBuilder>();
        return services;
    }
}
