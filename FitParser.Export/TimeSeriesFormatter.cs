/*
*/ 

namespace FitParser.Export;

using System.Globalization;
using FitParser.Models;

public sealed class TimeSeriesFormatter
{
    public TimeSeriesTable Format(
        MessageCollection collection)
    {
        ArgumentNullException.ThrowIfNull(collection);

        Dictionary<string, TimeSeriesColumn> columns =
            new();

        Dictionary<TimestampKey, RowBuilder> rows =
            new();

        foreach (NormalizedMessage message
            in collection.Messages)
        {
            TimestampInfo? timestamp =
                FindTimestamp(message);

            TimestampKey rowKey =
                timestamp is not null
                    ? new TimestampKey(
                        timestamp.Type,
                        timestamp.Value)
                    : new TimestampKey(
                        "NoTimestamp",
                        (ulong)message.MessageIndex);

            if (!rows.TryGetValue(
                    rowKey,
                    out RowBuilder? row))
            {
                row = new RowBuilder(
                    timestamp,
                    message.MessageIndex);

                rows.Add(rowKey, row);
            }

            foreach (NormalizedField field
                in message.Fields)
            {
                if (IsTimestampField(field))
                {
                    continue;
                }

                string columnName =
                    CreateColumnName(
                        message,
                        field);

                if (!columns.ContainsKey(columnName))
                {
                    columns.Add(
                        columnName,
                        new TimeSeriesColumn(
                            columnName,
                            message.MessageName,
                            message.MessageNumber,
                            field.FieldName,
                            field.FieldNumber,
                            field.FitDataType,
                            field.Units));
                }

                row.AddValues(
                    columnName,
                    field.Values);
            }
        }

        List<TimeSeriesColumn> orderedColumns =
            columns.Values
                .OrderBy(column => column.MessageName)
                .ThenBy(column => column.MessageNumber)
                .ThenBy(column => column.FieldNumber)
                .ToList();

        List<TimeSeriesRow> formattedRows =
            rows.Values
                .OrderBy(row =>
                    row.Timestamp is null ? 1 : 0)
                .ThenBy(row =>
                    row.Timestamp?.Value)
                .ThenBy(row =>
                    row.SourceMessageIndex)
                .Select(row =>
                    row.ToTimeSeriesRow())
                .ToList();

        return new TimeSeriesTable(
            orderedColumns.AsReadOnly(),
            formattedRows.AsReadOnly());
    }

    private static string CreateColumnName(
        NormalizedMessage message,
        NormalizedField field)
    {
        return $"{message.MessageName}.{field.FieldName}";
    }

    private static bool IsTimestampField(
        NormalizedField field)
    {
        return string.Equals(
                   field.FitDataType,
                   "DateTime",
                   StringComparison.OrdinalIgnoreCase)
               ||
               string.Equals(
                   field.FitDataType,
                   "LocalDateTime",
                   StringComparison.OrdinalIgnoreCase)
               ||
               field.FieldName.Contains(
                   "Timestamp",
                   StringComparison.OrdinalIgnoreCase);
    }

    private static TimestampInfo? FindTimestamp(
        NormalizedMessage message)
    {
        foreach (NormalizedField field
            in message.Fields)
        {
            if (!IsTimestampField(field))
            {
                continue;
            }

            object? value =
                field.Values.FirstOrDefault();

            if (value is null)
            {
                continue;
            }

            if (ulong.TryParse(
                    Convert.ToString(
                        value,
                        CultureInfo.InvariantCulture),
                    NumberStyles.Integer,
                    CultureInfo.InvariantCulture,
                    out ulong timestampValue))
            {
                return new TimestampInfo(
                    field.FitDataType ?? field.FieldName,
                    timestampValue);
            }
        }

        return null;
    }

    private sealed record TimestampKey(
        string Type,
        ulong Value);

    private sealed record TimestampInfo(
        string Type,
        ulong Value);

    private sealed class RowBuilder
    {
        private readonly Dictionary<
            string,
            IReadOnlyList<object?>> _values =
            new();

        public TimestampInfo? Timestamp { get; }

        public int SourceMessageIndex { get; }

        public RowBuilder(
            TimestampInfo? timestamp,
            int sourceMessageIndex)
        {
            Timestamp = timestamp;
            SourceMessageIndex = sourceMessageIndex;
        }

        public void AddValues(
            string columnName,
            IReadOnlyList<object?> values)
        {
            _values[columnName] = values;
        }

        public TimeSeriesRow ToTimeSeriesRow()
        {
            return new TimeSeriesRow(
                Timestamp?.Value,
                Timestamp?.Type,
                SourceMessageIndex,
                new Dictionary<
                    string,
                    IReadOnlyList<object?>>(
                    _values));
        }
    }
}