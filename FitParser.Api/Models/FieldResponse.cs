/*
FieldResponse is a small data structure that describes one field for the API and UI.
*/ 

namespace FitParser.Api.Contracts;

public sealed class FieldResponse
{
    public int FieldNumber { get; init; }

    public string FieldName { get; init; } = string.Empty;

    public int Occurrences { get; init; }
}