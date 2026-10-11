using System.Globalization;

namespace FitParser.Core;

public sealed class MessageCollection
{
    private readonly List<NormalizedMessage> _messages = new();

    public IReadOnlyList<NormalizedMessage> Messages => _messages;

    public int Count => _messages.Count;

    public void Add(NormalizedMessage message)
    {
        ArgumentNullException.ThrowIfNull(message);

        _messages.Add(message);
    }

    public void AddRange(
        IEnumerable<NormalizedMessage> messages)
    {
        ArgumentNullException.ThrowIfNull(messages);

        foreach (NormalizedMessage message in messages)
        {
            Add(message);
        }
    }

    public void Clear()
    {
        _messages.Clear();
    }

    // Temporary CSV output support while a separate exporter is not included.
    public void SaveAsCsv(string outputFilePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(outputFilePath);

        using StreamWriter writer = new(outputFilePath);

        writer.WriteLine(
            "MessageIndex,MessageType,MessageNumber,FieldName,Value");

        for (int messageIndex = 0;
             messageIndex < _messages.Count;
             messageIndex++)
        {
            NormalizedMessage message = _messages[messageIndex];

            foreach (NormalizedField field in message.Fields)
            {
                foreach (object? value in field.Values)
                {
                    string valueText = Convert.ToString(
                        value,
                        CultureInfo.InvariantCulture) ?? "";

                    writer.WriteLine(string.Join(
                        ",",
                        EscapeCsv((messageIndex + 1).ToString()),
                        EscapeCsv(message.MessageType),
                        EscapeCsv(message.MessageNumber.ToString()),
                        EscapeCsv(field.Name),
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
