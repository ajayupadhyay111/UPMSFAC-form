using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Blazored.LocalStorage;
using UPMSF.Client;
using UPMSF.Client.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// API base address. Defaults to the host serving the app (single-unit deployment);
// override with "ApiBaseUrl" in wwwroot/appsettings.json when client + API are split.
var apiBase = builder.Configuration["ApiBaseUrl"];
var baseAddress = string.IsNullOrWhiteSpace(apiBase) ? builder.HostEnvironment.BaseAddress : apiBase;

builder.Services.AddBlazoredLocalStorage();
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<AppAuthStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<AppAuthStateProvider>());
builder.Services.AddScoped<AuthHeaderHandler>();
builder.Services.AddScoped<LocalizationService>();
builder.Services.AddScoped<FlowService>();

builder.Services.AddHttpClient<ApiClient>(c => c.BaseAddress = new Uri(baseAddress))
    .AddHttpMessageHandler<AuthHeaderHandler>();

await builder.Build().RunAsync();
