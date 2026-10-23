using System.Globalization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.JSInterop;
using TwBlazor;
using TwBlazor.Docs.Services;
using TwBlazor.Theme;
using TwBlazor.Wasm;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<Routes>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

ConfigureServices(builder.Services);

var host = builder.Build();
await ApplySavedCultureAsync(host.Services.GetRequiredService<IJSRuntime>());
await host.RunAsync();

// Extracted so BlazorWasmPreRendering.Build can re-run service registration
// inside its prerendering host, which never executes top-level statements.
static void ConfigureServices(IServiceCollection services)
{
    services.AddTwBlazor(_ => { }, Theme.CreateDefaultTheme);
}

// The navbar's language switcher saves its choice in localStorage and reloads. The culture has to be set before
// RunAsync, which is when Blazor loads the matching resource assemblies.
static async Task ApplySavedCultureAsync(IJSRuntime js)
{
    string? code;
    try
    {
        code = await js.InvokeAsync<string?>("localStorage.getItem", DocsLanguages.StorageKey);
    }
    catch (JSException)
    {
        return; // Storage is blocked: stay on the default language.
    }

    if (DocsLanguages.TryGetCulture(code) is not { } culture)
        return;

    CultureInfo.DefaultThreadCurrentCulture = culture;
    CultureInfo.DefaultThreadCurrentUICulture = culture;

    await js.InvokeVoidAsync("docsLocale.applyToDocument", culture.Name, culture.TextInfo.IsRightToLeft ? "rtl" : "ltr");
}
