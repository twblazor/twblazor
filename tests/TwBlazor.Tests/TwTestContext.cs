using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using TwBlazor.Builders;
using TwBlazor.Configuration;
using ThemeBase = TwBlazor.Theme.Theme;

namespace TwBlazor.Tests;

[Collection("ServiceCollectionExtensions")]
public class TwBlazorTestBase
{
    public BunitContext TestContext { get; set; }

    public TwBlazorTheme Theme { get; set; }

    public RoundedBuilder RoundedBuilder => TestContext.Services.GetRequiredService<RoundedBuilder>();

    public ShadowBuilder ShadowBuilder => TestContext.Services.GetRequiredService<ShadowBuilder>();

    public ButtonBuilder ButtonBuilder => TestContext.Services.GetRequiredService<ButtonBuilder>();

    public ChipBuilder ChipBuilder => TestContext.Services.GetRequiredService<ChipBuilder>();

    public ColorBuilder ColorBuilder => TestContext.Services.GetRequiredService<ColorBuilder>();

    public IconColorBuilder IconColorBuilder => TestContext.Services.GetRequiredService<IconColorBuilder>();

    public ToastBuilder ToastBuilder => TestContext.Services.GetRequiredService<ToastBuilder>();

    public InputVariantBuilder InputVariantBuilder => TestContext.Services.GetRequiredService<InputVariantBuilder>();

    public TwBlazorTestBase()
    {
        TestContext = new BunitContext();
        Theme = ThemeBase.CreateDefaultTheme();
        TestContext.Services.AddTwBlazor(Theme);

        // Loose mode lets unconfigured JS interop calls (e.g. twDevice.prefersNativePicker fired from
        // picker components' OnAfterRenderAsync) return their default value instead of throwing, so
        // existing tests that don't care about device detection keep working unmodified.
        TestContext.JSInterop.Mode = JSRuntimeMode.Loose;
    }

    public static RenderFragment RenderFragmentBuilder(string content) => builder =>
    {
        builder.AddContent(1, content);
    };
}
