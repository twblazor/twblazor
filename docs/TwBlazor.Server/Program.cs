using System.Globalization;
using Microsoft.AspNetCore.Localization;
using TwBlazor;
using TwBlazor.Docs.Services;
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

// The navbar's language switcher saves its choice in the culture cookie. Only the cookie is read, so a visitor
// who has not picked a language keeps the server's default rather than their browser's language.
var docsCultures = DocsLanguages.All.Select(l => l.Code)
    .Append(CultureInfo.CurrentCulture.Name)
    .Distinct()
    .ToArray();
var localizationOptions = new RequestLocalizationOptions()
    .AddSupportedCultures(docsCultures)
    .AddSupportedUICultures(docsCultures);
localizationOptions.RequestCultureProviders = [new CookieRequestCultureProvider()];
app.UseRequestLocalization(localizationOptions);

app.UseAntiforgery();

app.MapStaticAssets();
// The docs pages live in TwBlazor.Docs. Without this they are not endpoints, so every request for one is
// answered with the "not found" page and a 404 until the interactive router replaces it.
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddAdditionalAssemblies(typeof(TwBlazor.Docs._Imports).Assembly);

await app.RunAsync();
