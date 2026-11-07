/*
*/

namespace FitParser.Models;

using System.Collections.ObjectModel;
public sealed class NormalizedField
{
    public string FieldName { get; }

    public int FieldNumber { get; }

    public string? FitDataType { get; }

    public string? ClrDataType { get; }

    public string? Units { get; }

    public IReadOnlyList<object?> Values { get; }

    public NormalizedField(
        string fieldName,
        int fieldNumber,
        string? fitDataType,
        string? clrDataType,
        string? units,
        IEnumerable<object?> values)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(
            fieldName);

        ArgumentNullException.ThrowIfNull(values);

        FieldName = fieldName;
        FieldNumber = fieldNumber;
        FitDataType = fitDataType;
        ClrDataType = clrDataType;
        Units = units;

        Values = new ReadOnlyCollection<object?>(
            values.ToList());
    }
}