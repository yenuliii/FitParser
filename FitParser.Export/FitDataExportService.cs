/*
*/ 

using FitParser.Models;

namespace FitParser.Export;

public enum ExportFormat
{
    Csv
}

public enum FormatTableBasedOn
{
    Original,
    Timestamp
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
    private readonly TimeSeriesFormatter _timeSeriesFormatter;

    public FitDataExportService()
    {
        _csvExporter = new CsvExporter();
        _timeSeriesFormatter =
            new TimeSeriesFormatter();
    }

    public ExportResult Export(
        MessageCollection collection,
        ExportFormat format,
        FormatTableBasedOn formatTableBasedOn,
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
                    return ExportCsv(
                        collection,
                        formatTableBasedOn,
                        outputFilePath);

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

    private ExportResult ExportCsv(
        MessageCollection collection,
        FormatTableBasedOn formatTableBasedOn,
        string outputFilePath)
    {
        switch (formatTableBasedOn)
        {
            case FormatTableBasedOn.Original:
                _csvExporter.Export(
                    collection,
                    outputFilePath);

                return ExportResult.Success();

            case FormatTableBasedOn.Timestamp:
                TimeSeriesTable timeSeriesTable =
                    _timeSeriesFormatter.Format(
                        collection);

                _csvExporter.Export(
                    timeSeriesTable,
                    outputFilePath);

                return ExportResult.Success();

            default:
                return ExportResult.Failure(
                    "The selected table organization " +
                    "is not supported.");
        }
    }
}