/*
Exports normalized FIT data to CSV.

Supported formats:

1. Original message-based format:
   One row per field value.

2. Timestamp-based format:
   One row per timestamp and one column per field.
*/

namespace FitParser.Export;

using System.Globalization;
using FitParser.Models;

public sealed class CsvExporter
{
    /*
     * Exports the original MessageCollection format.
     */
    public void Export(
        MessageCollection collection,
        string outputFilePath)
    {
        ArgumentNullException.ThrowIfNull(collection);

        ArgumentException.ThrowIfNullOrWhiteSpace(
            outputFilePath);

        using StreamWriter writer =
            new(outputFilePath);

        writer.WriteLine(
            "Message Index," +
            "Message Name," +
            "Message Number," +
            "Field Name," +
            "Field Number," +
            "FitDataType," +
            "ClrDataType," +
            "Value," +
            "Units");

        foreach (NormalizedMessage message
            in collection.Messages)
        {
            foreach (NormalizedField field
                in message.Fields)
            {
                foreach (object? value
                    in field.Values)
                {
                    string valueText =
                        FitDateTimeFormatter.Format(
                            value,
                            field.FitDataType);

                    string outputUnits =
                        GetOutputUnits(field);

                    List<string> rowValues =
                        new()
                        {
                            message.MessageIndex
                                .ToString(
                                    CultureInfo.InvariantCulture),

                            message.MessageName,

                            message.MessageNumber
                                .ToString(
                                    CultureInfo.InvariantCulture),

                            field.FieldName,

                            field.FieldNumber
                                .ToString(
                                    CultureInfo.InvariantCulture),

                            field.FitDataType
                                ?? string.Empty,

                            field.ClrDataType
                                ?? string.Empty,

                            valueText,

                            outputUnits
                        };

                    WriteRow(
                        writer,
                        rowValues);
                }
            }
        }
    }

    /*
     * Exports the timestamp-based TimeSeriesTable format.
     */
    public void Export(
        TimeSeriesTable table,
        string outputFilePath)
    {
        ArgumentNullException.ThrowIfNull(table);

        ArgumentException.ThrowIfNullOrWhiteSpace(
            outputFilePath);

        Dictionary<string, TimeSeriesColumn>
            columnMetadata =
                table.Columns
                    .GroupBy(
                        column => column.ColumnName,
                        StringComparer.Ordinal)
                    .ToDictionary(
                        group => group.Key,
                        group => group.First(),
                        StringComparer.Ordinal);

        List<string> columnNames =
            table.Columns
                .Select(column => column.ColumnName)
                .Concat(
                    table.Rows
                        .SelectMany(row =>
                            row.Values.Keys))
                .Distinct(StringComparer.Ordinal)
                .ToList();

        using StreamWriter writer =
            new(outputFilePath);

        List<string> headers =
            new()
            {
                "Timestamp"
            };

        headers.AddRange(columnNames);

        WriteRow(
            writer,
            headers);

        foreach (TimeSeriesRow row
            in table.Rows)
        {
            List<string> rowValues =
                new()
                {
                    FormatTimestamp(row)
                };

            foreach (string columnName
                in columnNames)
            {
                if (!row.Values.TryGetValue(
                        columnName,
                        out IReadOnlyList<object?>?
                            fieldValues))
                {
                    rowValues.Add(string.Empty);
                    continue;
                }

                columnMetadata.TryGetValue(
                    columnName,
                    out TimeSeriesColumn? column);

                rowValues.Add(
                    FormatFieldValues(
                        fieldValues,
                        column));
            }

            WriteRow(
                writer,
                rowValues);
        }
    }

    private static string FormatTimestamp(
    TimeSeriesRow row)
    {
    if (row.TimestampValue is null)
    {
        return string.Empty;
    }

    // FIT timestamp fields are UTC DateTime values.
    return FitDateTimeFormatter.Format(
        row.TimestampValue.Value,
        "DateTime");
    }

    private static string FormatFieldValues(
        IReadOnlyList<object?> values,
        TimeSeriesColumn? column)
    {
        List<string> formattedValues =
            values
                .Select(value =>
                    FitDateTimeFormatter.Format(
                        value,
                        column?.FitDataType))
                .ToList();

        /*
         * Multiple values from one field are kept
         * inside one CSV cell.
         */
        return string.Join(
            ";",
            formattedValues);
    }

    private static string GetOutputUnits(
        NormalizedField field)
    {
        if (string.Equals(
                field.FitDataType,
                "DateTime",
                StringComparison.OrdinalIgnoreCase))
        {
            return "UTC";
        }

        if (string.Equals(
                field.FitDataType,
                "LocalDateTime",
                StringComparison.OrdinalIgnoreCase))
        {
            return "Local time";
        }

        return field.Units ?? string.Empty;
    }

    private static void WriteRow(
        StreamWriter writer,
        IEnumerable<string> values)
    {
        writer.WriteLine(
            string.Join(
                ",",
                values.Select(EscapeCsv)));
    }

    private static string EscapeCsv(
        string value)
    {
        if (value.Contains('"') ||
            value.Contains(',') ||
            value.Contains('\n') ||
            value.Contains('\r'))
        {
            return $"\"{value.Replace(
                "\"",
                "\"\"")}\"";
        }

        return value;
    }
}