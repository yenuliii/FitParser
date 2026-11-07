/*
*/

namespace FitParser.Models;

public sealed class TimeSeriesRow
{
    public ulong? TimestampValue { get; }

    public string? TimestampType { get; }

    public int SourceMessageIndex { get; }

    public IReadOnlyDictionary<
        string,
        IReadOnlyList<object?>> Values { get; }

    public TimeSeriesRow(
        ulong? timestampValue,
        string? timestampType,
        int sourceMessageIndex,
        IReadOnlyDictionary<
            string,
            IReadOnlyList<object?>> values)
    {
        TimestampValue = timestampValue;
        TimestampType = timestampType;
        SourceMessageIndex = sourceMessageIndex;
        Values = values;
    }
}