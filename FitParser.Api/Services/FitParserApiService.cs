/*
Coordinates work between the services 
saves the uploaded file, 
finds temporary path of file, 
extracts file, 
gets available messages,
gets availble fields, 
return the completed response  
*/

namespace FitParser.Api.Services;

using FitParser.Api.Contracts;
using FitParser.Api.Storage;
using FitParser.Extract;
using FitParser.Models;
using FitParser.Select;

public sealed class FitParserApiService
{
    private readonly ITemporaryFileStore _temporaryFileStore;
    private readonly FitDataExtractionService _extractionService;
    private readonly FitDataSelectionService _selectionService;

    public FitParserApiService(
        ITemporaryFileStore temporaryFileStore,
        FitDataExtractionService extractionService,
        FitDataSelectionService selectionService)
    {
        ArgumentNullException.ThrowIfNull(
            temporaryFileStore);

        ArgumentNullException.ThrowIfNull(
            extractionService);

        ArgumentNullException.ThrowIfNull(
            selectionService);

        _temporaryFileStore =
            temporaryFileStore;

        _extractionService =
            extractionService;

        _selectionService =
            selectionService;
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
                _temporaryFileStore.GetPathOfFileId(fileId);

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

            List<MessageResponse> messageResponses =
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
                FileId = fileId,

                FileName = originalFileName,

                MessageCount = collection.Count,

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
}