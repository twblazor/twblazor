using Bunit;
using Microsoft.Extensions.DependencyInjection;
using TwBlazor.Configuration;
using ThemeBase = TwBlazor.Theme.Theme;

namespace TwBlazor.Docs.Tests;

/// <summary>
/// Base class for bUnit tests of the docs site's components and pages, giving each test a fresh
/// <see cref="BunitContext"/> with the default TwBlazor theme and services already registered.
/// </summary>
public class DocsTestBase
{
    public BunitContext TestContext { get; set; }

    public TwBlazorTheme Theme { get; set; }

    public DocsTestBase()
    {
        TestContext = new BunitContext();
        Theme = ThemeBase.CreateDefaultTheme();
        TestContext.Services.AddTwBlazor(Theme);

        // Loose mode lets unconfigured JS interop calls (e.g. the theme toggle or code highlighting a docs
        // page fires from OnAfterRenderAsync) return their default value instead of throwing, so tests
        // that don't care about them keep working unmodified.
        TestContext.JSInterop.Mode = JSRuntimeMode.Loose;
    }
}
