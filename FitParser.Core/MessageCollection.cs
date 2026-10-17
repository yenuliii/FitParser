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

//TODO: when reporting timestamps must indicate what kind of timestamp 
// TODO: must indicate what kind of message 
// WARNING: All data is extracted as retrieved from the file. Some data is stored with scales and offsets and need mathematical conversion to get the real value. 
// so our goal is to be able to select what data we want to extract and between what time periods 
// so our next step would be getting the message extracted in a way we can do this / right here \ 
    public void SaveAsCsv(string outputFilePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(outputFilePath);

        using StreamWriter writer = new(outputFilePath);

        writer.WriteLine(
            "Message Index, Message Name, Message Number, Field Name, Field Number, ClrDataType, Value");

        for (int messageIndex = 0;
             messageIndex < _messages.Count;
             messageIndex++)
        {
            NormalizedMessage message =
                _messages[messageIndex];

            foreach (NormalizedField field in message.Fields)
            {
                foreach (object? value in field.Values)
                {
                    string valueText =
                        Convert.ToString(
                            value,
                            CultureInfo.InvariantCulture) ?? "";

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