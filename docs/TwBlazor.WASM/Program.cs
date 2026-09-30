using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using TwBlazor;
using TwBlazor.Theme;
using TwBlazor.Wasm;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<Routes>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

ConfigureServices(builder.Services);

await builder.Build().RunAsync();

// Extracted so BlazorWasmPreRendering.Build can re-run service registration
// inside its prerendering host, which never executes top-level statements.
static void ConfigureServices(IServiceCollection services)
{
    services.AddTwBlazor(_ => { }, Theme.CreateDefaultTheme);
}
