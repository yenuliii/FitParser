/*
WARNING:
All data will be shown as the values retrieved from the file.

Some data is stored with scales and offsets
and may need mathematical conversion to get the real value.
*/

using System.Globalization;
using FitParser.Core;

namespace FitParser.Export;

public sealed class CsvExporter
{
    public void Export(
        MessageCollection collection,
        string outputFilePath)
    {
        ArgumentNullException.ThrowIfNull(collection);
        ArgumentException.ThrowIfNullOrWhiteSpace(outputFilePath);

        using StreamWriter writer = new(outputFilePath);

        writer.WriteLine(
            "Message Index,Message Name,Message Number,Field Name,Field Number,FitDataType,ClrDataType,Value");

        for (int messageIndex = 0;
             messageIndex < collection.Messages.Count;
             messageIndex++)
        {
            NormalizedMessage message =
                collection.Messages[messageIndex];

            foreach (NormalizedField field in message.Fields)
            {
                foreach (object? value in field.Values)
                {
                    string valueText =
                        FitDateTimeFormatter.Format(
                            value,
                            field.FitDataType);

                    writer.WriteLine(string.Join(
                        ",",
                        EscapeCsv(
                            (messageIndex + 1).ToString(
                                CultureInfo.InvariantCulture)),

                        EscapeCsv(message.MessageName),

                        EscapeCsv(
                            message.MessageNumber.ToString(
                                CultureInfo.InvariantCulture)),

                        EscapeCsv(field.FieldName),

                        EscapeCsv(
                            field.FieldNumber.ToString(
                                CultureInfo.InvariantCulture)),

                        EscapeCsv(field.FitDataType ?? ""),

                        EscapeCsv(field.ClrDataType ?? ""),

                        EscapeCsv(valueText)));
                }
            }
        }
    }

    private static string EscapeCsv(string value)
    {
        if (value.Contains('"') ||
            value.Contains(',') ||
            value.Contains('\n') ||
            value.Contains('\r'))
        {
            return $"\"{value.Replace("\"", "\"\"")}\"";
        }

        return value;
    }
}