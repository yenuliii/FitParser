using FitParser.Extract;
using FitParser.Export;
using FitParser.Select;
using FitParser.Models;

WebApplicationBuilder builder =
    WebApplication.CreateBuilder(args);

builder.Services.AddScoped<
    FitDataExtractionService>();

builder.Services.AddScoped<
    FitDataExportService>();

builder.Services.AddScoped<
    FitDataSelectionService>();

builder.Services.AddCors(
    options =>
    {
        options.AddPolicy(
            "FitParserWeb",
            policy =>
            {
                policy
                    .AllowAnyOrigin()
                    .AllowAnyHeader()
                    .AllowAnyMethod();
            });
    });

WebApplication app =
    builder.Build();

app.UseCors("FitParserWeb");

app.MapGet(
    "/api/health",
    () => Results.Ok(
        new
        {
            status = "API is running"
        }));

app.MapPost(
    "/api/fit/export-all",
    async (
        IFormFile? file,
        FitDataExtractionService extractionService,
        FitDataExportService exportService) =>
    {
        if (file is null || file.Length == 0)
        {
            return Results.BadRequest(
                new
                {
                    error = "A FIT file is required."
                });
        }

        string fileExtension =
            Path.GetExtension(file.FileName);

        if (!string.Equals(
                fileExtension,
                ".fit",
                StringComparison.OrdinalIgnoreCase))
        {
            return Results.BadRequest(
                new
                {
                    error = "Only .fit files are supported."
                });
        }

        string fileId =
            Guid.NewGuid().ToString("N");

        string inputFilePath =
            Path.Combine(
                Path.GetTempPath(),
                $"{fileId}.fit");

        string outputFilePath =
            Path.Combine(
                Path.GetTempPath(),
                $"{fileId}.csv");

        try
        {
            await using (
                FileStream inputStream =
                    new(
                        inputFilePath,
                        FileMode.Create,
                        FileAccess.Write,
                        FileShare.None))
            {
                await file.CopyToAsync(inputStream);
            }

            FitExtractionResult extractionResult =
                extractionService.Extract(
                    inputFilePath);

            if (!extractionResult.IsSuccess ||
                extractionResult.Collection is null)
            {
                return Results.BadRequest(
                    new
                    {
                        error =
                            extractionResult.ErrorMessage
                            ?? "The FIT file could not be extracted."
                    });
            }

            ExportResult exportResult =
                exportService.Export(
                    extractionResult.Collection,
                    ExportFormat.Csv,
                    outputFilePath);

            if (!exportResult.IsSuccess)
            {
                return Results.Problem(
                    exportResult.ErrorMessage
                    ?? "The FIT data could not be exported.");
            }

            byte[] csvBytes =
                await File.ReadAllBytesAsync(
                    outputFilePath);

            return Results.File(
                csvBytes,
                "text/csv",
                "fit-export.csv");
        }
        catch (Exception exception)
        {
            return Results.Problem(
                $"The FIT file could not be processed: " +
                exception.Message);
        }
        finally
        {
            if (File.Exists(inputFilePath))
            {
                File.Delete(inputFilePath);
            }

            if (File.Exists(outputFilePath))
            {
                File.Delete(outputFilePath);
            }
        }
    })
    .DisableAntiforgery();

app.MapPost(
    "/api/fit/analyze",
    async (
        IFormFile? file,
        FitDataExtractionService extractionService,
        FitDataSelectionService selectionService) =>
    {
        if (file is null || file.Length == 0)
        {
            return Results.BadRequest(
                new
                {
                    error = "A FIT file is required."
                });
        }

        if (!string.Equals(
                Path.GetExtension(file.FileName),
                ".fit",
                StringComparison.OrdinalIgnoreCase))
        {
            return Results.BadRequest(
                new
                {
                    error = "Only .fit files are supported."
                });
        }

        string inputFilePath =
            Path.Combine(
                Path.GetTempPath(),
                $"{Guid.NewGuid():N}.fit");

        try
        {
            await using (
                FileStream inputStream =
                    new(
                        inputFilePath,
                        FileMode.Create,
                        FileAccess.Write,
                        FileShare.None))
            {
                await file.CopyToAsync(inputStream);
            }

            FitExtractionResult extractionResult =
                extractionService.Extract(
                    inputFilePath);

            if (!extractionResult.IsSuccess ||
                extractionResult.Collection is null)
            {
                return Results.BadRequest(
                    new
                    {
                        error =
                            extractionResult.ErrorMessage
                            ?? "The FIT file could not be extracted."
                    });
            }

            MessageCollection collection =
                extractionResult.Collection;

            IReadOnlyList<AvailableMessage>
                availableMessages =
                    selectionService.GetAvailableMessages(
                        collection);

            IReadOnlyList<AvailableField>
                availableFields =
                    selectionService.GetAvailableFields(
                        collection);

            return Results.Ok(
                new
                {
                    messages = availableMessages,
                    fields = availableFields
                });
        }
        catch (Exception exception)
        {
            return Results.Problem(
                $"The FIT file could not be analyzed: " +
                exception.Message);
        }
        finally
        {
            if (File.Exists(inputFilePath))
            {
                File.Delete(inputFilePath);
            }
        }
    })
    .DisableAntiforgery();

app.Run();