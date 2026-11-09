/*
Data structure of a message and it's fields 
*/

namespace FitParser.Api.Contracts;

public sealed class MessageResponse
{
    public int MessageNumber { get; init; }

    public string MessageName { get; init; } = string.Empty;

    public int Occurrences { get; init; }

    public IReadOnlyList<FieldResponse> Fields { get; init; } =
        [];
}