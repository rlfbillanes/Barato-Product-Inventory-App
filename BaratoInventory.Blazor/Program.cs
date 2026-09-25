using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using BaratoInventory.Blazor;
using BaratoInventory.Blazor.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Read API Base URL from configuration or fallback to standard API port
var apiBaseUrl = builder.Configuration["ApiBaseUrl"] ?? "http://localhost:5000/";

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(apiBaseUrl) });
builder.Services.AddScoped<IProductApiClient, ProductApiClient>();

await builder.Build().RunAsync();
