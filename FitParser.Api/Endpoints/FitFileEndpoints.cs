/*
This file defines the routes.

The endpoint acts like the API's front desk:
HTTP requests arrive here and are passed
to FitParserApiService.
*/

namespace FitParser.Api.Endpoints;

using FitParser.Api.Contracts;
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

        endpoints
            .MapPost(
                "/api/fit/export",
                ExportAsync)
            .DisableAntiforgery();

        return endpoints;
    }

    private static async Task<IResult> AnalyzeAsync(
        IFormFile file,
        FitParserApiService apiService,
        CancellationToken cancellationToken)
    {
        if (file is null)
        {
            return Results.BadRequest(
                new
                {
                    error = "A FIT file is required."
                });
        }

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

    private static async Task<IResult> ExportAsync(
        SelectFitDataRequest request,
        FitParserApiService apiService,
        CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return Results.BadRequest(
                new
                {
                    error = "An export request is required."
                });
        }

        try
        {
            SelectedFitExportResult result =
                await apiService.ExportSelectedAsync(
                    request,
                    cancellationToken);

            return Results.File(
                result.FileContents,
                "text/csv",
                result.FileName);
        }
        catch (ArgumentException exception)
        {
            return Results.BadRequest(
                new
                {
                    error = exception.Message
                });
        }
        catch (InvalidOperationException exception)
        {
            return Results.BadRequest(
                new
                {
                    error = exception.Message
                });
        }
        catch (FileNotFoundException exception)
        {
            return Results.NotFound(
                new
                {
                    error = exception.Message
                });
        }
        catch (Exception)
        {
            return Results.Problem(
                "The selected FIT data could not be exported.");
        }
    }
}