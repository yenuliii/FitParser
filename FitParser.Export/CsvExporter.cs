/*
Exports selected normalized FIT data to CSV.

Date and time values are formatted by
FitDateTimeFormatter.
*/

namespace FitParser.Export;

using System.Globalization;
using FitParser.Models;

public sealed class CsvExporter
{
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

                    writer.WriteLine(
                        string.Join(
                            ",",
                            EscapeCsv(
                                message.MessageIndex
                                    .ToString(
                                        CultureInfo
                                            .InvariantCulture)),

                            EscapeCsv(
                                message.MessageName),

                            EscapeCsv(
                                message.MessageNumber
                                    .ToString(
                                        CultureInfo
                                            .InvariantCulture)),

                            EscapeCsv(
                                field.FieldName),

                            EscapeCsv(
                                field.FieldNumber
                                    .ToString(
                                        CultureInfo
                                            .InvariantCulture)),

                            EscapeCsv(
                                field.FitDataType ?? string.Empty),

                            EscapeCsv(
                                field.ClrDataType ?? string.Empty),

                            EscapeCsv(
                                valueText),

                            EscapeCsv(
                                outputUnits)));
                }
            }
        }
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