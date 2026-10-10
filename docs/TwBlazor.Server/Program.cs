using TwBlazor;
using TwBlazor.Server.Components;
using TwBlazor.Theme;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Add TwBlazor services
#region CodeExample GetStartedDependencyInjection
builder.Services.AddTwBlazor(_ => { }, Theme.CreateDefaultTheme);
#endregion

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
// The docs pages live in TwBlazor.Docs. Without this they are not endpoints, so every request for one is
// answered with the "not found" page and a 404 until the interactive router replaces it.
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddAdditionalAssemblies(typeof(TwBlazor.Docs._Imports).Assembly);

await app.RunAsync();
