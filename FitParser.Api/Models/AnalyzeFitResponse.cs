/*
a data structure that holds the results returned after analyzing a fitfile containes 
temporary ID, origincal filename, number of messages and available messages types 
and fields
*/ 

namespace FitParser.Api.Contracts;

public sealed class AnalyzeFitResponse
{
    public string FileId { get; init; } = string.Empty;

    public string FileName { get; init; } = string.Empty;

    public int MessageCount { get; init; }

    public IReadOnlyList<MessageResponse> Messages { get; init; } =
        [];
}