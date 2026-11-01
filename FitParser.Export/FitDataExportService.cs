/*
*/ 

namespace FitParser.Export;

using FitParser.Models;

public enum ExportFormat
{
    Csv
}

public sealed class ExportResult
{
    public bool IsSuccess { get; }

    public string? ErrorMessage { get; }

    private ExportResult(
        bool isSuccess,
        string? errorMessage)
    {
        IsSuccess = isSuccess;
        ErrorMessage = errorMessage;
    }

    public static ExportResult Success()
    {
        return new ExportResult(
            true,
            null);
    }

    public static ExportResult Failure(
        string errorMessage)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(
            errorMessage);

        return new ExportResult(
            false,
            errorMessage);
    }
}

public sealed class FitDataExportService
{
    private readonly CsvExporter _csvExporter;

    public FitDataExportService()
    {
        _csvExporter = new CsvExporter();
    }

    public ExportResult Export(
        MessageCollection collection,
        ExportFormat format,
        string outputFilePath)
    {
        ArgumentNullException.ThrowIfNull(collection);
        ArgumentException.ThrowIfNullOrWhiteSpace(
            outputFilePath);

        try
        {
            switch (format)
            {
                case ExportFormat.Csv:
                    _csvExporter.Export(
                        collection,
                        outputFilePath);

                    return ExportResult.Success();

                default:
                    return ExportResult.Failure(
                        "The selected output format " +
                        "is not supported.");
            }
        }
        catch (Exception exception)
        {
            return ExportResult.Failure(
                $"The data could not be exported: " +
                $"{exception.Message}");
        }
    }
}