/*
Responsible for coordinating the work of the FitParser.Extract folder
*/

namespace FitParser.Extract;

using FitParser.Models;

public sealed class FitDataExtractionService
{
    private readonly bool _checkIntegrity;

    public FitDataExtractionService(
        bool checkIntegrity = false)
    {
        _checkIntegrity = checkIntegrity;
    }

    public FitExtractionResult Extract(
        string inputFilePath)
    {
        if (string.IsNullOrWhiteSpace(inputFilePath))
        {
            return FitExtractionResult.Failure(
                "An input FIT file path is required.");
        }

        try
        {
            FitFileChecker checker = new();

            var validationResult =
                checker.Validate(
                    inputFilePath,
                    _checkIntegrity);

            if (!validationResult.IsValid)
            {
                return FitExtractionResult.Failure(
                    validationResult.ErrorMessage
                    ?? "The FIT file is invalid.");
            }

            GeneralMessageHandler handler = new();

            MessageBroadcaster broadcaster =
                new(handler);

            FitFileDecoder decoder =
                new(broadcaster);

            decoder.DecodeFile(inputFilePath);

            MessageNormalizer normalizer = new();

            MessageCollection collection =
                normalizer.NormalizeAll(
                    handler.Messages);

            if (collection.Count == 0)
            {
                return FitExtractionResult.Failure(
                    "No messages were found in the FIT file.");
            }

            return FitExtractionResult.Success(
                collection);
        }
        catch (Exception exception)
        {
            return FitExtractionResult.Failure(
                $"The FIT file could not be processed: " +
                exception.Message);
        }
    }
}

public sealed class FitExtractionResult
{
    public bool IsSuccess { get; }

    public MessageCollection? Collection { get; }

    public string? ErrorMessage { get; }

    private FitExtractionResult(
        bool isSuccess,
        MessageCollection? collection,
        string? errorMessage)
    {
        IsSuccess = isSuccess;
        Collection = collection;
        ErrorMessage = errorMessage;
    }

    public static FitExtractionResult Success(
        MessageCollection collection)
    {
        ArgumentNullException.ThrowIfNull(collection);

        return new FitExtractionResult(
            true,
            collection,
            null);
    }

    public static FitExtractionResult Failure(
        string errorMessage)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(
            errorMessage);

        return new FitExtractionResult(
            false,
            null,
            errorMessage);
    }
}