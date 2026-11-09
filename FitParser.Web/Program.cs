using FitParser.Web; 
using FitParser.Web.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

const string apiBaseAddress =
    "http://localhost:5162/";

WebAssemblyHostBuilder builder =
    WebAssemblyHostBuilder.CreateDefault(
        args);

builder.RootComponents.Add<App>(
    "#app");

builder.RootComponents.Add<HeadOutlet>(
    "head::after");

builder.Services.AddScoped(_ =>
    new HttpClient
    {
        BaseAddress =
            new Uri(apiBaseAddress)
    });

builder.Services.AddScoped<
    FitParserApiClient>();

await builder.Build().RunAsync();