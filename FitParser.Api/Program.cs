/*
Program.cs is the startup file. It tells the API what services
and routes exist, then starts the server.
*/

using FitParser.Api.Endpoints;
using FitParser.Api.Services;
using FitParser.Api.Storage;
using FitParser.Extract;
using FitParser.Select;

WebApplicationBuilder builder =
    WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy(
        "FitParserWeb",
        policy =>
        {
            policy
                .WithOrigins(
                    "http://localhost:5247",
                    "http://127.0.0.1:5247")
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
});

builder.Services.AddScoped<
    FitDataExtractionService>();

builder.Services.AddScoped<
    FitDataSelectionService>();

builder.Services.AddSingleton<
    ITemporaryFileStore,
    TemporaryFileStore>();

builder.Services.AddScoped<
    FitParserApiService>();

WebApplication app =
    builder.Build();

app.UseCors("FitParserWeb");

app.UseHttpsRedirection();

app.MapFitFileEndpoints();

app.MapGet(
    "/api/health",
    () => Results.Ok(
        new
        {
            status = "API is running"
        }));

app.Run();