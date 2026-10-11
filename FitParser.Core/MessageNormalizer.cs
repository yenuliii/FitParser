using Dynastream.Fit;

namespace FitParser.Core;

public sealed class MessageNormalizer
{
    public NormalizedMessage Normalize(Mesg message)
    {
        ArgumentNullException.ThrowIfNull(message);

        List<NormalizedField> fields = new();

        foreach (Field field in message.Fields)
        {
            List<object?> values = new();

            for (int index = 0;
                 index < field.GetNumValues();
                 index++)
            {
                values.Add(field.GetValue(index));
            }

            string fieldName = string.IsNullOrWhiteSpace(field.Name)
                ? "UnknownField"
                : field.Name;

            fields.Add(new NormalizedField(fieldName, values));
        }

        return new NormalizedMessage(
            message.GetType().Name,
            message.Num,
            fields);
    }
}

public sealed class NormalizedMessage
{
    public string MessageType { get; }

    public int MessageNumber { get; }

    public IReadOnlyList<NormalizedField> Fields { get; }

    public NormalizedMessage(
        string messageType,
        int messageNumber,
        IReadOnlyList<NormalizedField> fields)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(messageType);
        ArgumentNullException.ThrowIfNull(fields);

        MessageType = messageType;
        MessageNumber = messageNumber;
        Fields = fields;
    }
}

public sealed class NormalizedField
{
    public string Name { get; }

    public IReadOnlyList<object?> Values { get; }

    public NormalizedField(
        string name,
        IReadOnlyList<object?> values)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(values);

        Name = name;
        Values = values;
    }
}
