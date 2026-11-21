/*
Coordinates the work between the API services.

This service:

- Saves uploaded FIT files
- Extracts FIT data
- Finds available messages and fields
- Selects requested messages and fields
- Exports selected data to CSV
*/

namespace FitParser.Api.Services;

using FitParser.Api.Contracts;
using FitParser.Api.Storage;
using FitParser.Extract;
using FitParser.Export;
using FitParser.Models;
using FitParser.Select;

public sealed class FitParserApiService
{
    private readonly ITemporaryFileStore
        _temporaryFileStore;

    private readonly FitDataExtractionService
        _extractionService;

    private readonly FitDataSelectionService
        _selectionService;

    private readonly FitDataExportService
        _exportService;

    public FitParserApiService(
        ITemporaryFileStore temporaryFileStore,
        FitDataExtractionService extractionService,
        FitDataSelectionService selectionService,
        FitDataExportService exportService)
    {
        ArgumentNullException.ThrowIfNull(
            temporaryFileStore);

        ArgumentNullException.ThrowIfNull(
            extractionService);

        ArgumentNullException.ThrowIfNull(
            selectionService);

        ArgumentNullException.ThrowIfNull(
            exportService);

        _temporaryFileStore =
            temporaryFileStore;

        _extractionService =
            extractionService;

        _selectionService =
            selectionService;

        _exportService =
            exportService;
    }

    public async Task<AnalyzeFitResponse> AnalyzeAsync(
        Stream contents,
        string originalFileName,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(contents);

        ArgumentException.ThrowIfNullOrWhiteSpace(
            originalFileName);

        string fileId =
            await _temporaryFileStore.SaveAsync(
                contents,
                originalFileName,
                cancellationToken);

        try
        {
            string temporaryFilePath =
                _temporaryFileStore.GetPathOfFileId(
                    fileId);

            FitExtractionResult extractionResult =
                _extractionService.Extract(
                    temporaryFilePath);

            if (!extractionResult.IsSuccess ||
                extractionResult.Collection is null)
            {
                throw new InvalidOperationException(
                    extractionResult.ErrorMessage
                    ?? "The FIT file could not be analyzed.");
            }

            MessageCollection collection =
                extractionResult.Collection;

            IReadOnlyList<AvailableMessage>
                availableMessages =
                _selectionService.GetAvailableMessages(
                    collection);

            IReadOnlyList<AvailableField>
                availableFields =
                _selectionService.GetAvailableFields(
                    collection);

            List<MessageResponse>
                messageResponses =
                availableMessages
                    .Select(message =>
                    {
                        IReadOnlyList<FieldResponse>
                            fieldResponses =
                            availableFields
                                .Where(field =>
                                    field.MessageNumber ==
                                    message.MessageNumber)
                                .Select(field =>
                                    new FieldResponse
                                    {
                                        FieldNumber =
                                            field.FieldNumber,

                                        FieldName =
                                            field.FieldName,

                                        Occurrences =
                                            field.Count
                                    })
                                .ToList()
                                .AsReadOnly();

                        return new MessageResponse
                        {
                            MessageNumber =
                                message.MessageNumber,

                            MessageName =
                                message.MessageName,

                            Occurrences =
                                message.Count,

                            Fields =
                                fieldResponses
                        };
                    })
                    .ToList();

            return new AnalyzeFitResponse
            {
                FileId =
                    fileId,

                FileName =
                    originalFileName,

                MessageCount =
                    collection.Count,

                Messages =
                    messageResponses.AsReadOnly()
            };
        }
        catch
        {
            await _temporaryFileStore.DeleteAsync(
                fileId,
                CancellationToken.None);

            throw;
        }
    }

    public async Task<SelectedFitExportResult>
        ExportSelectedAsync(
            SelectFitDataRequest request,
            CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        ArgumentException.ThrowIfNullOrWhiteSpace(
            request.FileId);

        if (request.SelectedMessageNumbers.Count == 0)
        {
            throw new InvalidOperationException(
                "At least one message must be selected.");
        }

        if (request.SelectedFieldKeys.Count == 0)
        {
            throw new InvalidOperationException(
                "At least one field must be selected.");
        }

        string inputFilePath =
            _temporaryFileStore.GetPathOfFileId(
                request.FileId);

        if (!File.Exists(inputFilePath))
        {
            throw new FileNotFoundException(
                "The analyzed FIT file could not be found.");
        }

        string storageDirectory =
            Path.GetDirectoryName(
                inputFilePath)
            ?? Path.GetTempPath();

        Directory.CreateDirectory(
            storageDirectory);

        string outputFilePath =
            Path.Combine(
                storageDirectory,
                $"{Guid.NewGuid():N}.csv");

        bool exportSucceeded =
            false;

        try
        {
            FitExtractionResult extractionResult =
                _extractionService.Extract(
                    inputFilePath);

            if (!extractionResult.IsSuccess ||
                extractionResult.Collection is null)
            {
                throw new InvalidOperationException(
                    extractionResult.ErrorMessage
                    ?? "The FIT file could not be extracted.");
            }

            MessageCollection collection =
                extractionResult.Collection;

            HashSet<int> selectedMessageNumbers =
                request.SelectedMessageNumbers
                    .ToHashSet();

            HashSet<FieldKey> selectedFieldKeys =
                request.SelectedFieldKeys
                    .ToHashSet();

            IReadOnlyList<AvailableMessage>
                availableMessages =
                _selectionService.GetAvailableMessages(
                    collection);

            HashSet<int> availableMessageNumbers =
                availableMessages
                    .Select(message =>
                        message.MessageNumber)
                    .ToHashSet();

            if (!selectedMessageNumbers.IsSubsetOf(
                    availableMessageNumbers))
            {
                throw new InvalidOperationException(
                    "The request contains an invalid message.");
            }

            /*
            Get fields from the complete collection first.

            This allows the user to select only some fields from
            a message without causing an invalid-field error.
            */
            IReadOnlyList<AvailableField>
                availableFields =
                _selectionService.GetAvailableFields(
                    collection);

            HashSet<FieldKey> availableFieldKeys =
                availableFields
                    .Select(field =>
                        new FieldKey(
                            field.MessageNumber,
                            field.FieldNumber))
                    .ToHashSet();

            if (!selectedFieldKeys.IsSubsetOf(
                    availableFieldKeys))
            {
                throw new InvalidOperationException(
                    "The request contains an invalid field.");
            }

            /*
            Every selected field belongs to a message.

            Add those message numbers to the selected-message set
            so partially selected messages are supported.
            */
            foreach (FieldKey fieldKey
                in selectedFieldKeys)
            {
                selectedMessageNumbers.Add(
                    fieldKey.MessageNumber);
            }

            if (!selectedMessageNumbers.IsSubsetOf(
                    availableMessageNumbers))
            {
                throw new InvalidOperationException(
                    "The request contains an invalid message.");
            }

            MessageCollection selectedMessages =
                _selectionService.SelectMessages(
                    collection,
                    selectedMessageNumbers);

            MessageCollection selectedCollection =
                _selectionService.SelectFields(
                    selectedMessages,
                    selectedFieldKeys);

            if (selectedCollection.Count == 0)
            {
                throw new InvalidOperationException(
                    "No data matched the selected fields.");
            }

            ExportResult exportResult =
                _exportService.Export(
                    selectedCollection,
                    ExportFormat.Csv,
                    request.FormatTableBasedOn,
                    outputFilePath);

            if (!exportResult.IsSuccess)
            {
                throw new InvalidOperationException(
                    exportResult.ErrorMessage
                    ?? "The CSV could not be created.");
            }

            byte[] fileContents =
                await File.ReadAllBytesAsync(
                    outputFilePath,
                    cancellationToken);

            exportSucceeded =
                true;

            return new SelectedFitExportResult(
                fileContents,
                "fitparser-selected-data.csv");
        }
        finally
        {
            if (File.Exists(outputFilePath))
            {
                File.Delete(outputFilePath);
            }

            /*
            Delete the original FIT file only after a
            successful export.

            If export fails, the original file remains available
            so the user can retry.
            */
            if (exportSucceeded)
            {
                await _temporaryFileStore.DeleteAsync(
                    request.FileId,
                    CancellationToken.None);
            }
        }
    }
}

public sealed class SelectedFitExportResult
{
    public byte[] FileContents { get; }

    public string FileName { get; }

    public SelectedFitExportResult(
        byte[] fileContents,
        string fileName)
    {
        ArgumentNullException.ThrowIfNull(
            fileContents);

        ArgumentException.ThrowIfNullOrWhiteSpace(
            fileName);

        FileContents =
            fileContents;

        FileName =
            fileName;
    }
}