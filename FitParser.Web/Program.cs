using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using FitParser.Web;
using FitParser.Web.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

builder.Services.AddScoped(
    _ => new HttpClient
    {
        BaseAddress =
            new Uri("http://localhost:5000/")
    });

builder.Services.AddScoped<
    FitParserApiClient>();
    
await builder.Build().RunAsync();
