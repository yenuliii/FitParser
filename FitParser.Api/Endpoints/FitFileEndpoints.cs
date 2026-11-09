/*
This file defines the routes
this like the api's front desk: 
http request comes to this endpoint and it hands it 
over to the FitParserApiService
*/ 

namespace FitParser.Api.Endpoints;

using FitParser.Api.Services;

public static class FitFileEndpoints
{
    public static IEndpointRouteBuilder MapFitFileEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        endpoints
            .MapPost(
                "/api/fit/analyze",
                AnalyzeAsync)
            .DisableAntiforgery();

        return endpoints;
    }

    private static async Task<IResult> AnalyzeAsync(
        IFormFile file,
        FitParserApiService apiService,
        CancellationToken cancellationToken)
    {
        if (file.Length == 0)
        {
            return Results.BadRequest(
                new
                {
                    error = "The uploaded file is empty."
                });
        }

        if (!Path.GetExtension(file.FileName)
                .Equals(
                    ".fit",
                    StringComparison.OrdinalIgnoreCase))
        {
            return Results.BadRequest(
                new
                {
                    error = "Only FIT files are supported."
                });
        }

        try
        {
            await using Stream contents =
                file.OpenReadStream();

            var response =
                await apiService.AnalyzeAsync(
                    contents,
                    file.FileName,
                    cancellationToken);

            return Results.Ok(response);
        }
        catch (InvalidOperationException exception)
        {
            return Results.BadRequest(
                new
                {
                    error = exception.Message
                });
        }
        catch (Exception)
        {
            return Results.Problem(
                "The FIT file could not be analyzed.");
        }
    }
}